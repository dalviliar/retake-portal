using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
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
builder.Services.AddSingleton<PostgresXmlRepository>();
builder.Services.AddDataProtection().SetApplicationName("RetakePortal");
builder.Services.AddOptions<KeyManagementOptions>()
    .Configure<PostgresXmlRepository>((o, repo) => o.XmlRepository = repo);
builder.Services.AddScoped<SsoService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddScoped<ExpelledStudentService>();
builder.Services.AddScoped<FileUploadService>();
builder.Services.AddScoped<ImportService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
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
