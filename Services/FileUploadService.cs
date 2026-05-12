namespace RetakePortal.Services;

public class FileUploadService
{
    private readonly IWebHostEnvironment _env;
    private static readonly string[] Allowed = [".pdf", ".jpg", ".jpeg", ".png"];

    public FileUploadService(IWebHostEnvironment env) => _env = env;

    public async Task<string?> UploadAsync(IFormFile? file, string subfolder)
    {
        if (file == null || file.Length == 0) return null;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext)) return null;

        var dir = Path.Combine(_env.WebRootPath, "uploads", subfolder);
        Directory.CreateDirectory(dir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(dir, fileName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/{subfolder}/{fileName}";
    }
}
