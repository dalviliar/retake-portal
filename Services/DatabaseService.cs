using Npgsql;

namespace RetakePortal.Services;

public class DatabaseService
{
    private readonly string _supabaseConn;

    public DatabaseService(IConfiguration config)
    {
        _supabaseConn = config.GetConnectionString("Supabase")
            ?? throw new InvalidOperationException("Supabase connection string not configured");
    }

    public NpgsqlConnection Supabase() => new(_supabaseConn);
}
