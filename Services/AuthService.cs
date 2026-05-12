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
}
