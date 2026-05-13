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
        using var conn = _db.Supabase();
        var rows = conn.Query<string>("SELECT xml FROM data_protection_keys").ToList();
        return rows.Select(XElement.Parse).ToList();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        using var conn = _db.Supabase();
        conn.Execute(
            "INSERT INTO data_protection_keys (friendly_name, xml) VALUES (@friendlyName, @xml)",
            new { friendlyName, xml = element.ToString() });
    }
}
