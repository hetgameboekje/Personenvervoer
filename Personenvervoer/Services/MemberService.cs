using Npgsql;
using Personenvervoer.Models;

namespace Personenvervoer.Services;

public class MemberService
{
    private readonly DatabaseService _db;

    public MemberService(DatabaseService db)
    {
        _db = db;
    }

    public async Task<List<MemberModel>> GetAllAsync(int page = 0)
    {
        var members = new List<MemberModel>();
        int offset = page * 100;

        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, name, email, phone, address, latitude, longitude, created_at, updated_at FROM members ORDER BY name LIMIT 100 OFFSET @offset",
            conn);
        cmd.Parameters.AddWithValue("offset", offset);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            members.Add(MapRow(reader));
        }

        return members;
    }

    public async Task<MemberModel?> GetByIdAsync(Guid id)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, name, email, phone, address, latitude, longitude, created_at, updated_at FROM members WHERE id = @id",
            conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapRow(reader);

        return null;
    }

    public async Task<MemberModel> CreateAsync(MemberModel member)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO members (name, email, phone, address, latitude, longitude) VALUES (@name, @email, @phone, @address, @latitude, @longitude) RETURNING id, name, email, phone, address, latitude, longitude, created_at, updated_at",
            conn);
        cmd.Parameters.AddWithValue("name", member.Name);
        cmd.Parameters.AddWithValue("email", (object?)member.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("phone", (object?)member.Phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("address", (object?)member.Address ?? DBNull.Value);
        cmd.Parameters.AddWithValue("latitude", (object?)member.Latitude ?? DBNull.Value);
        cmd.Parameters.AddWithValue("longitude", (object?)member.Longitude ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        return MapRow(reader);
    }

    public async Task<MemberModel?> UpdateAsync(Guid id, MemberModel member)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "UPDATE members SET name = @name, email = @email, phone = @phone, address = @address, latitude = @latitude, longitude = @longitude, updated_at = NOW() WHERE id = @id RETURNING id, name, email, phone, address, latitude, longitude, created_at, updated_at",
            conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("name", member.Name);
        cmd.Parameters.AddWithValue("email", (object?)member.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("phone", (object?)member.Phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("address", (object?)member.Address ?? DBNull.Value);
        cmd.Parameters.AddWithValue("latitude", (object?)member.Latitude ?? DBNull.Value);
        cmd.Parameters.AddWithValue("longitude", (object?)member.Longitude ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapRow(reader);

        return null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM members WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private static MemberModel MapRow(NpgsqlDataReader reader)
    {
        return new MemberModel
        {
            Id = reader.GetGuid(0),
            Name = reader.GetString(1),
            Email = reader.IsDBNull(2) ? null : reader.GetString(2),
            Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
            Address = reader.IsDBNull(4) ? null : reader.GetString(4),
            Latitude = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
            Longitude = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
            CreatedAt = reader.GetDateTime(7),
            UpdatedAt = reader.GetDateTime(8)
        };
    }
}
