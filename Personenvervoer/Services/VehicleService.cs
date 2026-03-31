using Npgsql;
using Personenvervoer.Models;

namespace Personenvervoer.Services;

public class VehicleService
{
    private readonly DatabaseService _db;

    public VehicleService(DatabaseService db)
    {
        _db = db;
    }

    public async Task<List<VehicleModel>> GetAllAsync(int page = 0)
    {
        var vehicles = new List<VehicleModel>();
        int offset = page * 100;

        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, vehicle_type, license_plate, seats, created_at, updated_at FROM vehicles ORDER BY license_plate LIMIT 100 OFFSET @offset",
            conn);
        cmd.Parameters.AddWithValue("offset", offset);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            vehicles.Add(MapRow(reader));
        }

        return vehicles;
    }

    public async Task<VehicleModel?> GetByIdAsync(Guid id)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, vehicle_type, license_plate, seats, created_at, updated_at FROM vehicles WHERE id = @id",
            conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapRow(reader);

        return null;
    }

    public async Task<VehicleModel> CreateAsync(VehicleModel vehicle)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO vehicles (vehicle_type, license_plate, seats) VALUES (@vehicle_type, @license_plate, @seats) RETURNING id, vehicle_type, license_plate, seats, created_at, updated_at",
            conn);
        cmd.Parameters.AddWithValue("vehicle_type", vehicle.VehicleType);
        cmd.Parameters.AddWithValue("license_plate", vehicle.LicensePlate);
        cmd.Parameters.AddWithValue("seats", vehicle.Seats);

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        return MapRow(reader);
    }

    public async Task<VehicleModel?> UpdateAsync(Guid id, VehicleModel vehicle)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "UPDATE vehicles SET vehicle_type = @vehicle_type, license_plate = @license_plate, seats = @seats, updated_at = NOW() WHERE id = @id RETURNING id, vehicle_type, license_plate, seats, created_at, updated_at",
            conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("vehicle_type", vehicle.VehicleType);
        cmd.Parameters.AddWithValue("license_plate", vehicle.LicensePlate);
        cmd.Parameters.AddWithValue("seats", vehicle.Seats);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapRow(reader);

        return null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM vehicles WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private static VehicleModel MapRow(NpgsqlDataReader reader)
    {
        return new VehicleModel
        {
            Id = reader.GetGuid(0),
            VehicleType = reader.GetString(1),
            LicensePlate = reader.GetString(2),
            Seats = reader.GetInt32(3),
            CreatedAt = reader.GetDateTime(4),
            UpdatedAt = reader.GetDateTime(5)
        };
    }
}
