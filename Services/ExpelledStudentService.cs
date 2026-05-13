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
            SELECT es.id          AS Id,
                   es.iin         AS IIN,
                   COALESCE(s.full_name, '') AS StudentFullName,
                   es.discipline_name  AS DisciplineName,
                   es.expulsion_date   AS ExpulsionDate,
                   es.act_document_url AS ActDocumentUrl,
                   es.added_by         AS AddedBy,
                   es.added_at         AS AddedAt
            FROM expelled_students es
            LEFT JOIN students s ON s.iin = es.iin
            ORDER BY es.added_at DESC";
        return (await conn.QueryAsync<ExpelledStudent>(sql)).ToList();
    }

    public async Task BulkAddAsync(string iin, string[] disciplines, DateTime date, string? actUrl, int addedBy)
    {
        using var conn = _db.Supabase();
        const string insertSql = @"
            INSERT INTO expelled_students (iin, discipline_name, expulsion_date, act_document_url, added_by)
            VALUES (@iin, @discipline, @date, @actUrl, @addedBy)
            ON CONFLICT (iin, discipline_name) DO UPDATE
                SET expulsion_date   = EXCLUDED.expulsion_date,
                    act_document_url = EXCLUDED.act_document_url,
                    added_by         = EXCLUDED.added_by,
                    added_at         = NOW()";
        foreach (var discipline in disciplines)
            await conn.ExecuteAsync(insertSql, new { iin, discipline, date, actUrl, addedBy });

        const string conflictSql = @"
            UPDATE applications
            SET expulsion_conflict = TRUE
            WHERE iin = @iin
              AND status IN ('pending', 'pending_director', 'director_approved')
              AND EXISTS (
                  SELECT 1 FROM application_items ai
                  WHERE ai.application_id = applications.id
                    AND ai.discipline_name = ANY(@disciplines)
              )";
        await conn.ExecuteAsync(conflictSql, new { iin, disciplines });
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
