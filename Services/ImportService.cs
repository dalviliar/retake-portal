using Dapper;
using RetakePortal.Models;
using System.Globalization;

namespace RetakePortal.Services;

public class ImportResult
{
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = [];
}

public class ImportService
{
    private readonly DatabaseService _db;

    public ImportService(DatabaseService db) => _db = db;

    // CSV format: iin,full_name,specialty,institute,department,course,education_level
    public async Task<ImportResult> ImportStudentsAsync(IFormFile file)
    {
        var result = new ImportResult();
        var lines = await ReadLinesAsync(file);

        foreach (var (line, num) in lines.Select((l, i) => (l, i + 1)))
        {
            var parts = line.Split(',');
            if (parts.Length < 7)
            {
                result.Errors.Add($"Строка {num}: недостаточно столбцов");
                result.Skipped++;
                continue;
            }

            var iin = parts[0].Trim();
            if (iin.Length != 12 || !iin.All(char.IsDigit))
            {
                result.Errors.Add($"Строка {num}: некорректный ИИН «{iin}»");
                result.Skipped++;
                continue;
            }

            var level = parts[6].Trim().ToLower();
            if (!new[] { "bachelor", "master_sci", "master_prof", "doctoral" }.Contains(level))
            {
                result.Errors.Add($"Строка {num}: неизвестный уровень «{level}»");
                result.Skipped++;
                continue;
            }

            using var conn = _db.Supabase();
            const string sql = @"
                INSERT INTO students (iin, full_name, specialty, institute, department, course, education_level, updated_at)
                VALUES (@iin, @fullName, @specialty, @institute, @department, @course, @level, NOW())
                ON CONFLICT (iin) DO UPDATE
                    SET full_name       = EXCLUDED.full_name,
                        specialty       = EXCLUDED.specialty,
                        institute       = EXCLUDED.institute,
                        department      = EXCLUDED.department,
                        course          = EXCLUDED.course,
                        education_level = EXCLUDED.education_level,
                        updated_at      = NOW()";
            await conn.ExecuteAsync(sql, new
            {
                iin,
                fullName   = parts[1].Trim(),
                specialty  = parts[2].Trim(),
                institute  = parts[3].Trim(),
                department = parts[4].Trim(),
                course     = int.TryParse(parts[5].Trim(), out var c) ? c : 1,
                level
            });
            result.Imported++;
        }

        return result;
    }

    // CSV format: student_iin,discipline_name,grade,credits,semester
    public async Task<ImportResult> ImportGradesAsync(IFormFile file)
    {
        var result = new ImportResult();
        var lines = await ReadLinesAsync(file);

        foreach (var (line, num) in lines.Select((l, i) => (l, i + 1)))
        {
            var parts = line.Split(',');
            if (parts.Length < 5)
            {
                result.Errors.Add($"Строка {num}: недостаточно столбцов");
                result.Skipped++;
                continue;
            }

            var iin   = parts[0].Trim();
            var grade = parts[2].Trim().ToUpper();

            if (grade != "FX" && grade != "F")
            {
                result.Skipped++;
                continue; // пропускаем не-FX/F оценки без ошибки
            }

            if (!int.TryParse(parts[3].Trim(), out var credits))
            {
                result.Errors.Add($"Строка {num}: некорректные кредиты");
                result.Skipped++;
                continue;
            }

            using var conn = _db.Supabase();
            const string sql = @"
                INSERT INTO grades (student_iin, discipline_name, grade, credits, semester)
                VALUES (@iin, @discipline, @grade, @credits, @semester)
                ON CONFLICT (student_iin, discipline_name, semester) DO UPDATE
                    SET grade   = EXCLUDED.grade,
                        credits = EXCLUDED.credits";
            await conn.ExecuteAsync(sql, new
            {
                iin,
                discipline = parts[1].Trim(),
                grade,
                credits,
                semester   = parts[4].Trim()
            });
            result.Imported++;
        }

        return result;
    }

    private static async Task<List<string>> ReadLinesAsync(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();
        return content
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.TrimEnd('\r'))
            .Where(l => !l.StartsWith('#') && !string.IsNullOrWhiteSpace(l))
            .Skip(1) // пропускаем заголовок
            .ToList();
    }
}
