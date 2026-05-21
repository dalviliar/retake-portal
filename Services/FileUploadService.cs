using System.Net.Http.Headers;

namespace RetakePortal.Services;

public class FileUploadService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly string _supabaseUrl;
    private readonly string _serviceKey;
    private readonly string _bucket;
    private static readonly string[] Allowed = [".pdf", ".jpg", ".jpeg", ".png"];

    public FileUploadService(IHttpClientFactory httpFactory, IConfiguration config)
    {
        _httpFactory = httpFactory;
        _supabaseUrl = config["SupabaseStorage:Url"]!;
        _serviceKey  = config["SupabaseStorage:ServiceKey"]!;
        _bucket      = config["SupabaseStorage:Bucket"]!;
    }

    public async Task<string?> UploadAsync(IFormFile? file, string subfolder)
    {
        if (file == null || file.Length == 0) return null;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext)) return null;

        var fileName    = $"{Guid.NewGuid()}{ext}";
        var objectPath  = $"{subfolder}/{fileName}";
        var uploadUrl   = $"{_supabaseUrl}/storage/v1/object/{_bucket}/{objectPath}";

        using var http    = _httpFactory.CreateClient();
        using var content = new StreamContent(file.OpenReadStream());
        content.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(ext));

        var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        request.Headers.Add("Authorization", $"Bearer {_serviceKey}");
        request.Content = content;

        var response = await http.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        return $"{_supabaseUrl}/storage/v1/object/public/{_bucket}/{objectPath}";
    }

    private static string GetContentType(string ext) => ext switch
    {
        ".pdf"           => "application/pdf",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png"           => "image/png",
        _                => "application/octet-stream"
    };
}
