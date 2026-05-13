using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.Director;

public class DirectorExpelledStudentsModel : Pages.DirectorPageModel
{
    private readonly ExpelledStudentService _expelled;
    public DirectorExpelledStudentsModel(ExpelledStudentService expelled) => _expelled = expelled;

    public List<ExpelledStudent> Students { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        Students = await _expelled.GetAllAsync();
        return Page();
    }
}
