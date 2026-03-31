using Npgsql;
using Personenvervoer.Models;

namespace Personenvervoer.Services;

public class RideService
{
    private readonly DatabaseService _db;
    private readonly GoogleApiService _googleApi;

    public RideService(DatabaseService db, GoogleApiService googleApi)
    {
        _db = db;
        _googleApi = googleApi;
    }

    public async Task<List<RideModel>> GetAllAsync(int page = 0)
    {
        var rides = new List<RideModel>();
        int offset = page * 100;

        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(GetJoinQuery("ORDER BY r.created_at DESC LIMIT 100 OFFSET @offset"), conn);
        cmd.Parameters.AddWithValue("offset", offset);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rides.Add(MapRow(reader));
        }

        return rides;
    }

    public async Task<RideModel?> GetByIdAsync(Guid id)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(GetJoinQuery("WHERE r.id = @id"), conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapRow(reader);

        return null;
    }

    public async Task<RideModel> CreateAsync(RideModel ride)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO rides (ridepattern_id, vehicle_id, end_location_id, ride_type, max_boarding_time, location_time, ride_time) VALUES (@ridepattern_id, @vehicle_id, @end_location_id, @ride_type, @max_boarding_time, @location_time, @ride_time) RETURNING id",
            conn);
        cmd.Parameters.AddWithValue("ridepattern_id", (object?)ride.RidepatternId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("vehicle_id", (object?)ride.VehicleId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("end_location_id", (object?)ride.EndLocationId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("ride_type", ride.RideType);
        cmd.Parameters.AddWithValue("max_boarding_time", (object?)ride.MaxBoardingTime ?? DBNull.Value);
        cmd.Parameters.AddWithValue("location_time", (object?)ride.LocationTime ?? DBNull.Value);
        cmd.Parameters.AddWithValue("ride_time", (object?)ride.RideTime ?? DBNull.Value);

        var scalar = await cmd.ExecuteScalarAsync()
            ?? throw new InvalidOperationException("INSERT INTO rides did not return a generated ID.");
        var id = (Guid)scalar;
        return (await GetByIdAsync(id))!;
    }

    public async Task<RideModel?> UpdateAsync(Guid id, RideModel ride)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "UPDATE rides SET ridepattern_id = @ridepattern_id, vehicle_id = @vehicle_id, end_location_id = @end_location_id, ride_type = @ride_type, max_boarding_time = @max_boarding_time, location_time = @location_time, ride_time = @ride_time, updated_at = NOW() WHERE id = @id",
            conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("ridepattern_id", (object?)ride.RidepatternId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("vehicle_id", (object?)ride.VehicleId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("end_location_id", (object?)ride.EndLocationId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("ride_type", ride.RideType);
        cmd.Parameters.AddWithValue("max_boarding_time", (object?)ride.MaxBoardingTime ?? DBNull.Value);
        cmd.Parameters.AddWithValue("location_time", (object?)ride.LocationTime ?? DBNull.Value);
        cmd.Parameters.AddWithValue("ride_time", (object?)ride.RideTime ?? DBNull.Value);

        int affected = await cmd.ExecuteNonQueryAsync();
        if (affected == 0) return null;

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM rides WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<GoogleApiResult> GetRideDistanceAsync(Guid id)
    {
        var ride = await GetByIdAsync(id);
        if (ride == null)
            return new GoogleApiResult { Status = "NOT_FOUND", DistanceKm = 0, DurationMinutes = 0 };

        return await _googleApi.GetRideDistance(ride);
    }

    private static string GetJoinQuery(string whereOrOrder) => $@"
        SELECT
            r.id, r.ridepattern_id, r.vehicle_id, r.end_location_id, r.ride_type,
            r.max_boarding_time, r.location_time, r.ride_time, r.created_at, r.updated_at,
            rp.id, rp.member_id, rp.is_wheelchair, rp.rank, rp.created_at, rp.updated_at,
            m.id, m.name, m.email, m.phone, m.address, m.latitude, m.longitude, m.created_at, m.updated_at,
            v.id, v.vehicle_type, v.license_plate, v.seats, v.created_at, v.updated_at,
            l.id, l.name, l.address, l.latitude, l.longitude, l.created_at, l.updated_at
        FROM rides r
        LEFT JOIN ridepatterns rp ON rp.id = r.ridepattern_id
        LEFT JOIN members m ON m.id = rp.member_id
        LEFT JOIN vehicles v ON v.id = r.vehicle_id
        LEFT JOIN locations l ON l.id = r.end_location_id
        {whereOrOrder}";

    private static RideModel MapRow(NpgsqlDataReader reader)
    {
        var ride = new RideModel
        {
            Id = reader.GetGuid(0),
            RidepatternId = reader.IsDBNull(1) ? null : reader.GetGuid(1),
            VehicleId = reader.IsDBNull(2) ? null : reader.GetGuid(2),
            EndLocationId = reader.IsDBNull(3) ? null : reader.GetGuid(3),
            RideType = reader.GetString(4),
            MaxBoardingTime = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
            LocationTime = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
            RideTime = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
            CreatedAt = reader.GetDateTime(8),
            UpdatedAt = reader.GetDateTime(9)
        };

        // Ridepattern
        if (!reader.IsDBNull(10))
        {
            ride.Ridepattern = new RidepatternModel
            {
                Id = reader.GetGuid(10),
                MemberId = reader.IsDBNull(11) ? null : reader.GetGuid(11),
                IsWheelchair = reader.GetBoolean(12),
                Rank = reader.GetInt32(13),
                CreatedAt = reader.GetDateTime(14),
                UpdatedAt = reader.GetDateTime(15)
            };

            // Member (inside ridepattern)
            if (!reader.IsDBNull(16))
            {
                ride.Ridepattern.Member = new MemberModel
                {
                    Id = reader.GetGuid(16),
                    Name = reader.GetString(17),
                    Email = reader.IsDBNull(18) ? null : reader.GetString(18),
                    Phone = reader.IsDBNull(19) ? null : reader.GetString(19),
                    Address = reader.IsDBNull(20) ? null : reader.GetString(20),
                    Latitude = reader.IsDBNull(21) ? null : reader.GetDecimal(21),
                    Longitude = reader.IsDBNull(22) ? null : reader.GetDecimal(22),
                    CreatedAt = reader.GetDateTime(23),
                    UpdatedAt = reader.GetDateTime(24)
                };
            }
        }

        // Vehicle
        if (!reader.IsDBNull(25))
        {
            ride.Vehicle = new VehicleModel
            {
                Id = reader.GetGuid(25),
                VehicleType = reader.GetString(26),
                LicensePlate = reader.GetString(27),
                Seats = reader.GetInt32(28),
                CreatedAt = reader.GetDateTime(29),
                UpdatedAt = reader.GetDateTime(30)
            };
        }

        // End Location
        if (!reader.IsDBNull(31))
        {
            ride.EndLocation = new LocationModel
            {
                Id = reader.GetGuid(31),
                Name = reader.GetString(32),
                Address = reader.IsDBNull(33) ? null : reader.GetString(33),
                Latitude = reader.IsDBNull(34) ? null : reader.GetDecimal(34),
                Longitude = reader.IsDBNull(35) ? null : reader.GetDecimal(35),
                CreatedAt = reader.GetDateTime(36),
                UpdatedAt = reader.GetDateTime(37)
            };
        }

        return ride;
    }
}
