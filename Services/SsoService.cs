using Dapper;
using RetakePortal.Models;

namespace RetakePortal.Services;

public class SsoService
{
    private readonly DatabaseService _db;

    public SsoService(DatabaseService db) => _db = db;

    public async Task<Student?> GetStudentByIINAsync(string iin)
    {
        using var conn = _db.Supabase();
        const string sql = @"
            SELECT
                iin              AS IIN,
                full_name        AS FullName,
                specialty        AS Specialty,
                institute        AS Institute,
                department       AS Department,
                course           AS Course,
                education_level  AS EducationLevel
            FROM students
            WHERE iin = @iin
            LIMIT 1";
        return await conn.QueryFirstOrDefaultAsync<Student>(sql, new { iin });
    }

    public async Task<List<Grade>> GetFailedGradesAsync(string iin, string semester)
    {
        using var conn = _db.Supabase();
        const string sql = @"
            SELECT
                discipline_name  AS DisciplineName,
                grade            AS GradeValue,
                credits          AS Credits,
                semester         AS Semester
            FROM grades
            WHERE student_iin = @iin
              AND semester    = @semester
              AND grade       IN ('FX', 'F', 'I')
            ORDER BY discipline_name";
        var rows = await conn.QueryAsync<Grade>(sql, new { iin, semester });
        return rows.ToList();
    }
}
