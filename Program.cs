using Microsoft.AspNetCore.DataProtection;
using RetakePortal.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddDataProtection().SetApplicationName("RetakePortal");
builder.Services.AddScoped<SsoService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddScoped<ExpelledStudentService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<FileUploadService>();
builder.Services.AddScoped<ImportService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(exApp => exApp.Run(async ctx =>
    {
        ctx.Response.StatusCode = 500;
        ctx.Response.ContentType = "text/plain; charset=utf-8";
        var f = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var err = f?.Error;
        await ctx.Response.WriteAsync(err == null ? "Unknown error" :
            $"{err.GetType().FullName}: {err.Message}\n\nInner: {err.InnerException?.GetType().FullName}: {err.InnerException?.Message}\n\n{err.StackTrace}");
    }));
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
