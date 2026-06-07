using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Application.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace KnowledgeBase.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _options;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _webRootPath;

    public LocalFileStorageService(IOptions<FileStorageOptions> options, ILogger<LocalFileStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    public async Task<string> UploadAvatarAsync(Stream fileStream, string fileName, long userId)
    {
        if (fileStream == null || fileStream.Length == 0)
        {
            throw new InvalidOperationException("请选择要上传的文件");
        }

        if (fileStream.Length > _options.MaxFileSize)
        {
            throw new InvalidOperationException($"文件大小不能超过 {_options.MaxFileSize / 1024 / 1024}MB");
        }

        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(fileExtension))
        {
            throw new InvalidOperationException($"不支持的文件格式，仅支持 {string.Join("、", _options.AllowedExtensions)} 格式");
        }

        var avatarDirectory = Path.Combine(_webRootPath, _options.UploadPath, "avatars");
        if (!Directory.Exists(avatarDirectory))
        {
            Directory.CreateDirectory(avatarDirectory);
        }

        var newFileName = $"avatar_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
        var filePath = Path.Combine(avatarDirectory, newFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        var relativePath = $"/{_options.UploadPath.TrimStart('/')}/avatars/{newFileName}";
        _logger.LogInformation("用户 {UserId} 上传头像成功，路径: {FilePath}", userId, relativePath);

        return relativePath;
    }

    public Task DeleteFileAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.CompletedTask;
        }

        var filePath = Path.Combine(_webRootPath, relativePath.TrimStart('/'));
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                _logger.LogInformation("删除文件成功: {FilePath}", relativePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "删除文件失败: {FilePath}", relativePath);
            }
        }

        return Task.CompletedTask;
    }
}
