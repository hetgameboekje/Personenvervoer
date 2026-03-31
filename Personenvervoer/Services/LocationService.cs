using Npgsql;
using Personenvervoer.Models;

namespace Personenvervoer.Services;

public class LocationService
{
    private readonly DatabaseService _db;

    public LocationService(DatabaseService db)
    {
        _db = db;
    }

    public async Task<List<LocationModel>> GetAllAsync(int page = 0)
    {
        var locations = new List<LocationModel>();
        int offset = page * 100;

        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, name, address, latitude, longitude, created_at, updated_at FROM locations ORDER BY name LIMIT 100 OFFSET @offset",
            conn);
        cmd.Parameters.AddWithValue("offset", offset);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            locations.Add(MapRow(reader));
        }

        return locations;
    }

    public async Task<LocationModel?> GetByIdAsync(Guid id)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, name, address, latitude, longitude, created_at, updated_at FROM locations WHERE id = @id",
            conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapRow(reader);

        return null;
    }

    public async Task<LocationModel> CreateAsync(LocationModel location)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO locations (name, address, latitude, longitude) VALUES (@name, @address, @latitude, @longitude) RETURNING id, name, address, latitude, longitude, created_at, updated_at",
            conn);
        cmd.Parameters.AddWithValue("name", location.Name);
        cmd.Parameters.AddWithValue("address", (object?)location.Address ?? DBNull.Value);
        cmd.Parameters.AddWithValue("latitude", (object?)location.Latitude ?? DBNull.Value);
        cmd.Parameters.AddWithValue("longitude", (object?)location.Longitude ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        return MapRow(reader);
    }

    public async Task<LocationModel?> UpdateAsync(Guid id, LocationModel location)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "UPDATE locations SET name = @name, address = @address, latitude = @latitude, longitude = @longitude, updated_at = NOW() WHERE id = @id RETURNING id, name, address, latitude, longitude, created_at, updated_at",
            conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("name", location.Name);
        cmd.Parameters.AddWithValue("address", (object?)location.Address ?? DBNull.Value);
        cmd.Parameters.AddWithValue("latitude", (object?)location.Latitude ?? DBNull.Value);
        cmd.Parameters.AddWithValue("longitude", (object?)location.Longitude ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapRow(reader);

        return null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM locations WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private static LocationModel MapRow(NpgsqlDataReader reader)
    {
        return new LocationModel
        {
            Id = reader.GetGuid(0),
            Name = reader.GetString(1),
            Address = reader.IsDBNull(2) ? null : reader.GetString(2),
            Latitude = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
            Longitude = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
            CreatedAt = reader.GetDateTime(5),
            UpdatedAt = reader.GetDateTime(6)
        };
    }
}
