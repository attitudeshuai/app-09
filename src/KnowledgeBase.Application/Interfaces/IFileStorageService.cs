namespace KnowledgeBase.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAvatarAsync(Stream fileStream, string fileName, long userId);
    Task DeleteFileAsync(string relativePath);
}
