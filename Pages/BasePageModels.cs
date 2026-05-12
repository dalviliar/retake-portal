using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RetakePortal.Pages;

public static class SessionKeys
{
    public const string StudentIIN  = "StudentIIN";
    public const string StudentName = "StudentName";
    public const string SpecId      = "SpecId";
    public const string SpecRole    = "SpecRole";
    public const string SpecName    = "SpecName";
}

public abstract class StudentPageModel : PageModel
{
    protected string StudentIIN => HttpContext.Session.GetString(SessionKeys.StudentIIN)!;

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeys.StudentIIN)))
            context.Result = RedirectToPage("/Student/Login");
        base.OnPageHandlerExecuting(context);
    }
}

public abstract class ORSpecialistPageModel : PageModel
{
    protected int SpecialistId   => int.Parse(HttpContext.Session.GetString(SessionKeys.SpecId)!);
    protected string SpecName    => HttpContext.Session.GetString(SessionKeys.SpecName)!;

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (HttpContext.Session.GetString(SessionKeys.SpecRole) != "or_specialist")
            context.Result = RedirectToPage("/OR/Login");
        base.OnPageHandlerExecuting(context);
    }
}

public abstract class ActsSpecialistPageModel : PageModel
{
    protected int SpecialistId => int.Parse(HttpContext.Session.GetString(SessionKeys.SpecId)!);
    protected string SpecName  => HttpContext.Session.GetString(SessionKeys.SpecName)!;

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (HttpContext.Session.GetString(SessionKeys.SpecRole) != "acts_specialist")
            context.Result = RedirectToPage("/Acts/Login");
        base.OnPageHandlerExecuting(context);
    }
}

public abstract class AdminPageModel : PageModel
{
    protected int SpecialistId => int.Parse(HttpContext.Session.GetString(SessionKeys.SpecId)!);
    protected string SpecName  => HttpContext.Session.GetString(SessionKeys.SpecName)!;

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (HttpContext.Session.GetString(SessionKeys.SpecRole) != "admin")
            context.Result = RedirectToPage("/Admin/Login");
        base.OnPageHandlerExecuting(context);
    }
}

public abstract class DirectorPageModel : PageModel
{
    protected int SpecialistId => int.Parse(HttpContext.Session.GetString(SessionKeys.SpecId)!);
    protected string SpecName  => HttpContext.Session.GetString(SessionKeys.SpecName)!;

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (HttpContext.Session.GetString(SessionKeys.SpecRole) != "director")
            context.Result = RedirectToPage("/Director/Login");
        base.OnPageHandlerExecuting(context);
    }
}
