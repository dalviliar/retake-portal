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
