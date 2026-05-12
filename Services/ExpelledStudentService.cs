using Dapper;
using RetakePortal.Models;

namespace RetakePortal.Services;

public class ExpelledStudentService
{
    private readonly DatabaseService _db;

    public ExpelledStudentService(DatabaseService db) => _db = db;

    public async Task<List<string>> GetExpelledDisciplinesAsync(string iin)
    {
        using var conn = _db.Supabase();
        const string sql = "SELECT discipline_name FROM expelled_students WHERE iin = @iin";
        return (await conn.QueryAsync<string>(sql, new { iin })).ToList();
    }

    public async Task<List<ExpelledStudent>> GetAllAsync()
    {
        using var conn = _db.Supabase();
        const string sql = @"
            SELECT id, iin, discipline_name AS DisciplineName,
                   expulsion_date AS ExpulsionDate, act_document_url AS ActDocumentUrl,
                   added_by AS AddedBy, added_at AS AddedAt
            FROM expelled_students ORDER BY added_at DESC";
        return (await conn.QueryAsync<ExpelledStudent>(sql)).ToList();
    }

    public async Task AddAsync(ExpelledStudent e)
    {
        using var conn = _db.Supabase();
        const string sql = @"
            INSERT INTO expelled_students (iin, discipline_name, expulsion_date, act_document_url, added_by)
            VALUES (@IIN, @DisciplineName, @ExpulsionDate, @ActDocumentUrl, @AddedBy)
            ON CONFLICT (iin, discipline_name) DO UPDATE
                SET expulsion_date   = EXCLUDED.expulsion_date,
                    act_document_url = EXCLUDED.act_document_url,
                    added_by         = EXCLUDED.added_by,
                    added_at         = NOW()";
        await conn.ExecuteAsync(sql, e);
    }
}
