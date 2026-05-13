namespace RetakePortal;

public static class AppHelper
{
    // Almaty UTC+5, no DST
    public static string ToAlmaty(this DateTime dt) => dt.AddHours(5).ToString("dd.MM.yyyy HH:mm");
    public static string ToAlmaty(this DateTime? dt) => dt.HasValue ? dt.Value.ToAlmaty() : "—";
    public static string ToAlmatyShort(this DateTime dt) => dt.AddHours(5).ToString("dd.MM.yy");
}
