using Dapper;
using Npgsql;
using RetakePortal.Models;

namespace RetakePortal.Services;

public class ApplicationService
{
    private readonly DatabaseService _db;

    public ApplicationService(DatabaseService db) => _db = db;

    public async Task<bool> HasPendingApplicationAsync(string iin)
    {
        using var conn = _db.Supabase();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM applications WHERE iin = @iin AND status = 'pending'",
            new { iin });
        return count > 0;
    }

    public async Task<int> CreateApplicationAsync(Application app)
    {
        using var conn = _db.Supabase();
        const string sql = @"
            INSERT INTO applications
                (iin, student_full_name, specialty, institute, department,
                 course, education_level, total_amount)
            VALUES
                (@IIN, @StudentFullName, @Specialty, @Institute, @Department,
                 @Course, @EducationLevel, @TotalAmount)
            RETURNING id";
        return await conn.ExecuteScalarAsync<int>(sql, app);
    }

    public async Task AddApplicationItemAsync(ApplicationItem item)
    {
        using var conn = _db.Supabase();
        const string sql = @"
            INSERT INTO application_items
                (application_id, discipline_name, grade, credits, cost_per_credit, total_cost,
                 confirmation_document_url, payment_receipt_url)
            VALUES
                (@ApplicationId, @DisciplineName, @Grade, @Credits, @CostPerCredit, @TotalCost,
                 @ConfirmationDocumentUrl, @PaymentReceiptUrl)";
        await conn.ExecuteAsync(sql, item);
    }

    public async Task<List<Application>> GetAllApplicationsAsync()
    {
        using var conn = _db.Supabase();
        const string sql = @"
            SELECT id, iin, student_full_name AS StudentFullName, specialty, institute, department,
                   course, education_level AS EducationLevel, status,
                   rejection_reason AS RejectionReason, total_amount AS TotalAmount,
                   submitted_at AS SubmittedAt, reviewed_at AS ReviewedAt, reviewed_by AS ReviewedBy
            FROM applications ORDER BY submitted_at DESC";
        var apps = (await conn.QueryAsync<Application>(sql)).ToList();
        foreach (var app in apps)
            app.Items = await GetItemsAsync(conn, app.Id);
        return apps;
    }

    public async Task<Application?> GetApplicationByIdAsync(int id)
    {
        using var conn = _db.Supabase();
        const string sql = @"
            SELECT id, iin, student_full_name AS StudentFullName, specialty, institute, department,
                   course, education_level AS EducationLevel, status,
                   rejection_reason AS RejectionReason, total_amount AS TotalAmount,
                   submitted_at AS SubmittedAt, reviewed_at AS ReviewedAt, reviewed_by AS ReviewedBy
            FROM applications WHERE id = @id";
        var app = await conn.QueryFirstOrDefaultAsync<Application>(sql, new { id });
        if (app == null) return null;
        app.Items = await GetItemsAsync(conn, id);
        return app;
    }

    public async Task<List<Application>> GetApplicationsByIINAsync(string iin)
    {
        using var conn = _db.Supabase();
        const string sql = @"
            SELECT id, iin, student_full_name AS StudentFullName, specialty, institute, department,
                   course, education_level AS EducationLevel, status,
                   rejection_reason AS RejectionReason, total_amount AS TotalAmount,
                   submitted_at AS SubmittedAt, reviewed_at AS ReviewedAt, reviewed_by AS ReviewedBy
            FROM applications WHERE iin = @iin ORDER BY submitted_at DESC";
        var apps = (await conn.QueryAsync<Application>(sql, new { iin })).ToList();
        foreach (var app in apps)
            app.Items = await GetItemsAsync(conn, app.Id);
        return apps;
    }

    public async Task<HashSet<string>> GetAppliedDisciplinesAsync(string iin)
    {
        using var conn = _db.Supabase();
        const string sql = @"
            SELECT DISTINCT ai.discipline_name
            FROM application_items ai
            JOIN applications a ON a.id = ai.application_id
            WHERE a.iin = @iin AND a.status IN ('pending', 'approved')";
        var result = await conn.QueryAsync<string>(sql, new { iin });
        return result.ToHashSet();
    }

    public async Task ReviewApplicationAsync(int id, string status, string? rejectionReason, int reviewedBy)
    {
        using var conn = _db.Supabase();
        const string sql = @"
            UPDATE applications
            SET status = @status, rejection_reason = @rejectionReason,
                reviewed_at = NOW(), reviewed_by = @reviewedBy
            WHERE id = @id";
        await conn.ExecuteAsync(sql, new { id, status, rejectionReason, reviewedBy });
    }

    public async Task<List<Application>> GetApplicationsForDirectorAsync()
    {
        using var conn = _db.Supabase();
        const string sql = @"
            SELECT id, iin, student_full_name AS StudentFullName, specialty, institute, department,
                   course, education_level AS EducationLevel, status,
                   rejection_reason AS RejectionReason, total_amount AS TotalAmount,
                   submitted_at AS SubmittedAt, reviewed_at AS ReviewedAt, reviewed_by AS ReviewedBy
            FROM applications WHERE status = 'pending_director' ORDER BY submitted_at";
        var apps = (await conn.QueryAsync<Application>(sql)).ToList();
        foreach (var app in apps)
            app.Items = await GetItemsAsync(conn, app.Id);
        return apps;
    }

    public async Task<List<ReportRow>> GetReportDataAsync()
    {
        using var conn = _db.Supabase();
        const string sql = @"
            SELECT a.institute AS Institute,
                   a.student_full_name AS StudentFullName,
                   a.iin AS IIN,
                   a.course AS Course,
                   COUNT(ai.id) AS DisciplineCount
            FROM applications a
            JOIN application_items ai ON ai.application_id = a.id
            WHERE a.status != 'rejected'
            GROUP BY a.institute, a.student_full_name, a.iin, a.course
            ORDER BY a.institute, a.student_full_name";
        return (await conn.QueryAsync<ReportRow>(sql)).ToList();
    }

    public async Task<int> GetActionRequiredCountAsync()
    {
        using var conn = _db.Supabase();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM applications WHERE status IN ('pending', 'director_approved')");
    }

    public async Task<(List<Application> Items, int Total)> GetApplicationsPagedAsync(
        string status, string discipline, int page, int pageSize)
    {
        using var conn = _db.Supabase();
        var conditions = new List<string>();
        var p = new Dapper.DynamicParameters();

        if (!string.IsNullOrEmpty(status))
        {
            conditions.Add("a.status = @status");
            p.Add("status", status);
        }
        if (!string.IsNullOrEmpty(discipline))
        {
            conditions.Add("EXISTS (SELECT 1 FROM application_items ai WHERE ai.application_id = a.id AND ai.discipline_name = @discipline)");
            p.Add("discipline", discipline);
        }

        var where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

        var total = await conn.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM applications a {where}", p);

        p.Add("limit", pageSize);
        p.Add("offset", (page - 1) * pageSize);

        var sql = $@"
            SELECT id, iin, student_full_name AS StudentFullName, specialty, institute, department,
                   course, education_level AS EducationLevel, status,
                   rejection_reason AS RejectionReason, total_amount AS TotalAmount,
                   submitted_at AS SubmittedAt, reviewed_at AS ReviewedAt, reviewed_by AS ReviewedBy
            FROM applications a {where}
            ORDER BY submitted_at DESC
            LIMIT @limit OFFSET @offset";

        var apps = (await conn.QueryAsync<Application>(sql, p)).ToList();
        foreach (var app in apps)
            app.Items = await GetItemsAsync(conn, app.Id);
        return (apps, total);
    }

    public async Task<List<string>> GetAllDisciplineNamesAsync()
    {
        using var conn = _db.Supabase();
        var rows = await conn.QueryAsync<string>(
            "SELECT DISTINCT discipline_name FROM application_items ORDER BY discipline_name");
        return rows.ToList();
    }

    private static async Task<List<ApplicationItem>> GetItemsAsync(NpgsqlConnection conn, int appId)
    {
        const string sql = @"
            SELECT id, application_id AS ApplicationId, discipline_name AS DisciplineName,
                   grade, credits, cost_per_credit AS CostPerCredit, total_cost AS TotalCost,
                   confirmation_document_url AS ConfirmationDocumentUrl,
                   payment_receipt_url AS PaymentReceiptUrl
            FROM application_items WHERE application_id = @appId ORDER BY id";
        return (await conn.QueryAsync<ApplicationItem>(sql, new { appId })).ToList();
    }
}

public class ReportRow
{
    public string Institute { get; set; } = string.Empty;
    public string StudentFullName { get; set; } = string.Empty;
    public string IIN { get; set; } = string.Empty;
    public int Course { get; set; }
    public int DisciplineCount { get; set; }
}
