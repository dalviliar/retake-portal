using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using RetakePortal.Services;

namespace RetakePortal.Pages.Director;

public class ReportModel : RetakePortal.Pages.DirectorPageModel
{
    private readonly ApplicationService _apps;
    public ReportModel(ApplicationService apps) => _apps = apps;

    public List<ReportRow> Data { get; set; } = [];

    public async Task OnGetAsync()
    {
        Data = await _apps.GetReportDataAsync();
    }

    public async Task<IActionResult> OnGetDownloadAsync()
    {
        var data = await _apps.GetReportDataAsync();

        using var workbook = new XLWorkbook();

        var institutes = data.Select(d => d.Institute).Distinct().OrderBy(i => i).ToList();

        foreach (var institute in institutes)
        {
            var sheetName = institute.Length > 31 ? institute[..31] : institute;
            var ws = workbook.Worksheets.Add(sheetName);

            ws.Cell(1, 1).Value = "№";
            ws.Cell(1, 2).Value = "ФИО";
            ws.Cell(1, 3).Value = "ИИН";
            ws.Cell(1, 4).Value = "Курс";
            ws.Cell(1, 5).Value = "Кол-во дисциплин пересдачи";

            var header = ws.Range("A1:E1");
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightBlue;
            header.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            var rows = data.Where(d => d.Institute == institute)
                           .OrderBy(d => d.StudentFullName)
                           .ToList();
            int rowNum = 2;
            int num = 1;
            foreach (var row in rows)
            {
                ws.Cell(rowNum, 1).Value = num++;
                ws.Cell(rowNum, 2).Value = row.StudentFullName;
                ws.Cell(rowNum, 3).Value = row.IIN;
                ws.Cell(rowNum, 4).Value = row.Course;
                ws.Cell(rowNum, 5).Value = row.DisciplineCount;
                rowNum++;
            }

            ws.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var fileName = $"retake_report_{DateTime.Now:yyyyMMdd}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Director/Login");
    }
}
