namespace KnowledgeBase.Application.Options;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";
    public string UploadPath { get; set; } = "uploads";
    public long MaxFileSize { get; set; } = 5 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    public string BaseUrl { get; set; } = "/uploads";
}
