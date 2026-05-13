using Dapper;
using RetakePortal.Models;

namespace RetakePortal.Services;

public class AuthService
{
    private readonly DatabaseService _db;

    public AuthService(DatabaseService db) => _db = db;

    public async Task<Specialist?> AuthenticateAsync(string username, string password)
    {
        using var conn = _db.Supabase();
        const string sql = @"
            SELECT id, username, password_hash AS PasswordHash, role,
                   full_name AS FullName, created_at AS CreatedAt
            FROM specialists WHERE username = @username LIMIT 1";
        var s = await conn.QueryFirstOrDefaultAsync<Specialist>(sql, new { username });
        if (s == null || !BCrypt.Net.BCrypt.Verify(password, s.PasswordHash)) return null;
        return s;
    }

    public async Task<bool> HasAnySpecialistAsync()
    {
        using var conn = _db.Supabase();
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM specialists") > 0;
    }

    public async Task CreateSpecialistAsync(string username, string password, string role, string fullName)
    {
        using var conn = _db.Supabase();
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        const string sql = @"
            INSERT INTO specialists (username, password_hash, role, full_name)
            VALUES (@username, @hash, @role, @fullName)";
        await conn.ExecuteAsync(sql, new { username, hash, role, fullName });
    }

    public async Task<List<Specialist>> GetAllSpecialistsAsync()
    {
        using var conn = _db.Supabase();
        const string sql = @"
            SELECT id, username, role, full_name AS FullName, created_at AS CreatedAt
            FROM specialists WHERE role != 'admin' ORDER BY role, full_name";
        return (await conn.QueryAsync<Specialist>(sql)).ToList();
    }

    public async Task DeleteSpecialistAsync(int id)
    {
        using var conn = _db.Supabase();
        await conn.ExecuteAsync("DELETE FROM specialists WHERE id = @id AND role != 'admin'", new { id });
    }

    public async Task<bool> ChangePasswordAsync(int id, string currentPassword, string newPassword)
    {
        using var conn = _db.Supabase();
        var hash = await conn.ExecuteScalarAsync<string>(
            "SELECT password_hash FROM specialists WHERE id = @id", new { id });
        if (hash == null || !BCrypt.Net.BCrypt.Verify(currentPassword, hash)) return false;
        var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await conn.ExecuteAsync(
            "UPDATE specialists SET password_hash = @newHash WHERE id = @id", new { id, newHash });
        return true;
    }

    public async Task ResetPasswordAsync(int id, string newPassword)
    {
        using var conn = _db.Supabase();
        var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await conn.ExecuteAsync(
            "UPDATE specialists SET password_hash = @newHash WHERE id = @id", new { id, newHash });
    }

    public async Task<List<SpecialistStat>> GetSpecialistStatsAsync()
    {
        using var conn = _db.Supabase();
        const string sql = @"
            SELECT s.id, s.full_name AS FullName, s.role,
                   COUNT(a.id)                                       AS Total,
                   COUNT(a.id) FILTER (WHERE a.status = 'approved') AS Approved,
                   COUNT(a.id) FILTER (WHERE a.status = 'rejected') AS Rejected,
                   COUNT(a.id) FILTER (WHERE a.status = 'pending')  AS Pending
            FROM specialists s
            LEFT JOIN applications a ON a.reviewed_by = s.id
            WHERE s.role = 'or_specialist'
            GROUP BY s.id, s.full_name, s.role
            ORDER BY s.full_name";
        return (await conn.QueryAsync<SpecialistStat>(sql)).ToList();
    }
}

public class SpecialistStat
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Pending { get; set; }
}
