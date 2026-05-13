using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.OR;

public class ExpelledStudentsModel : Pages.ORSpecialistPageModel
{
    private readonly ExpelledStudentService _expelled;
    public ExpelledStudentsModel(ExpelledStudentService expelled) => _expelled = expelled;

    public List<ExpelledStudent> Students { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        Students = await _expelled.GetAllAsync();
        return Page();
    }
}
