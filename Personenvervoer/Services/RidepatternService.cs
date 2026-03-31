using Npgsql;
using Personenvervoer.Models;

namespace Personenvervoer.Services;

public class RidepatternService
{
    private readonly DatabaseService _db;

    public RidepatternService(DatabaseService db)
    {
        _db = db;
    }

    public async Task<List<RidepatternModel>> GetAllAsync(int page = 0)
    {
        var patterns = new List<RidepatternModel>();
        int offset = page * 100;

        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            @"SELECT rp.id, rp.member_id, rp.is_wheelchair, rp.rank, rp.created_at, rp.updated_at,
                     m.id, m.name, m.email, m.phone, m.address, m.latitude, m.longitude, m.created_at, m.updated_at
              FROM ridepatterns rp
              LEFT JOIN members m ON m.id = rp.member_id
              ORDER BY rp.rank, rp.created_at
              LIMIT 100 OFFSET @offset",
            conn);
        cmd.Parameters.AddWithValue("offset", offset);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            patterns.Add(MapRow(reader));
        }

        return patterns;
    }

    public async Task<RidepatternModel?> GetByIdAsync(Guid id)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            @"SELECT rp.id, rp.member_id, rp.is_wheelchair, rp.rank, rp.created_at, rp.updated_at,
                     m.id, m.name, m.email, m.phone, m.address, m.latitude, m.longitude, m.created_at, m.updated_at
              FROM ridepatterns rp
              LEFT JOIN members m ON m.id = rp.member_id
              WHERE rp.id = @id",
            conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapRow(reader);

        return null;
    }

    public async Task<RidepatternModel> CreateAsync(RidepatternModel pattern)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO ridepatterns (member_id, is_wheelchair, rank) VALUES (@member_id, @is_wheelchair, @rank) RETURNING id, member_id, is_wheelchair, rank, created_at, updated_at",
            conn);
        cmd.Parameters.AddWithValue("member_id", (object?)pattern.MemberId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("is_wheelchair", pattern.IsWheelchair);
        cmd.Parameters.AddWithValue("rank", pattern.Rank);

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        return new RidepatternModel
        {
            Id = reader.GetGuid(0),
            MemberId = reader.IsDBNull(1) ? null : reader.GetGuid(1),
            IsWheelchair = reader.GetBoolean(2),
            Rank = reader.GetInt32(3),
            CreatedAt = reader.GetDateTime(4),
            UpdatedAt = reader.GetDateTime(5)
        };
    }

    public async Task<RidepatternModel?> UpdateAsync(Guid id, RidepatternModel pattern)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "UPDATE ridepatterns SET member_id = @member_id, is_wheelchair = @is_wheelchair, rank = @rank, updated_at = NOW() WHERE id = @id RETURNING id, member_id, is_wheelchair, rank, created_at, updated_at",
            conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("member_id", (object?)pattern.MemberId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("is_wheelchair", pattern.IsWheelchair);
        cmd.Parameters.AddWithValue("rank", pattern.Rank);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new RidepatternModel
            {
                Id = reader.GetGuid(0),
                MemberId = reader.IsDBNull(1) ? null : reader.GetGuid(1),
                IsWheelchair = reader.GetBoolean(2),
                Rank = reader.GetInt32(3),
                CreatedAt = reader.GetDateTime(4),
                UpdatedAt = reader.GetDateTime(5)
            };
        }

        return null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM ridepatterns WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private static RidepatternModel MapRow(NpgsqlDataReader reader)
    {
        var pattern = new RidepatternModel
        {
            Id = reader.GetGuid(0),
            MemberId = reader.IsDBNull(1) ? null : reader.GetGuid(1),
            IsWheelchair = reader.GetBoolean(2),
            Rank = reader.GetInt32(3),
            CreatedAt = reader.GetDateTime(4),
            UpdatedAt = reader.GetDateTime(5)
        };

        if (!reader.IsDBNull(6))
        {
            pattern.Member = new MemberModel
            {
                Id = reader.GetGuid(6),
                Name = reader.GetString(7),
                Email = reader.IsDBNull(8) ? null : reader.GetString(8),
                Phone = reader.IsDBNull(9) ? null : reader.GetString(9),
                Address = reader.IsDBNull(10) ? null : reader.GetString(10),
                Latitude = reader.IsDBNull(11) ? null : reader.GetDecimal(11),
                Longitude = reader.IsDBNull(12) ? null : reader.GetDecimal(12),
                CreatedAt = reader.GetDateTime(13),
                UpdatedAt = reader.GetDateTime(14)
            };
        }

        return pattern;
    }
}
