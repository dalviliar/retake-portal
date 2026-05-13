using System.Xml.Linq;
using Dapper;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace RetakePortal.Services;

public class PostgresXmlRepository : IXmlRepository
{
    private readonly DatabaseService _db;

    public PostgresXmlRepository(DatabaseService db) => _db = db;

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        try
        {
            using var conn = _db.Supabase();
            var rows = conn.Query<string>("SELECT xml FROM data_protection_keys").ToList();
            var result = new List<XElement>();
            foreach (var row in rows)
            {
                try { result.Add(XElement.Parse(row)); }
                catch { /* skip malformed rows */ }
            }
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DataProtection] GetAllElements failed: {ex.Message}");
            return [];
        }
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        try
        {
            using var conn = _db.Supabase();
            conn.Execute(
                "INSERT INTO data_protection_keys (friendly_name, xml) VALUES (@friendlyName, @xml)",
                new { friendlyName, xml = element.ToString() });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DataProtection] StoreElement failed: {ex.Message}");
        }
    }
}
