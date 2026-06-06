using KnowledgeBase.Application.DTOs.DocumentVersions;
using KnowledgeBase.Application.DTOs.Documents;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;

namespace KnowledgeBase.Application.Services;

public class DocumentVersionService : IDocumentVersionService
{
    private readonly IUnitOfWork _unitOfWork;

    public DocumentVersionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<DocumentVersionDto>> GetByDocumentIdAsync(long documentId)
    {
        if (!await _unitOfWork.Documents.ExistsAsync(documentId))
        {
            throw new KeyNotFoundException("文档不存在");
        }

        var versions = await _unitOfWork.DocumentVersions.GetByDocumentIdAsync(documentId);
        return versions.Select(MapToDto);
    }

    public async Task<DocumentVersionDto> GetByIdAsync(long id)
    {
        var version = await _unitOfWork.DocumentVersions.GetByIdAsync(id);
        if (version == null)
        {
            throw new KeyNotFoundException("版本不存在");
        }
        return MapToDto(version);
    }

    public async Task<DocumentVersionDto?> GetByDocumentIdAndVersionAsync(long documentId, int versionNumber)
    {
        var version = await _unitOfWork.DocumentVersions.GetByDocumentIdAndVersionAsync(documentId, versionNumber);
        return version != null ? MapToDto(version) : null;
    }

    public async Task<DocumentDto> RestoreVersionAsync(long documentId, int versionNumber, string? changeLog, long currentUserId)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new KeyNotFoundException("文档不存在");
        }

        var version = await _unitOfWork.DocumentVersions.GetByDocumentIdAndVersionAsync(documentId, versionNumber);
        if (version == null)
        {
            throw new KeyNotFoundException("版本不存在");
        }

        var oldVersion = new DocumentVersion
        {
            DocumentId = document.Id,
            VersionNumber = document.Version,
            Title = document.Title,
            Content = document.Content,
            Summary = document.Summary,
            Tags = document.Tags,
            ChangeLog = changeLog ?? $"恢复到版本 {versionNumber}",
            CreatedBy = currentUserId
        };

        await _unitOfWork.DocumentVersions.AddAsync(oldVersion);

        document.Title = version.Title;
        document.Content = version.Content;
        document.Summary = version.Summary;
        document.Tags = version.Tags;
        document.Version++;
        document.UpdatedBy = currentUserId;

        await _unitOfWork.Documents.UpdateAsync(document);
        await _unitOfWork.SaveChangesAsync();

        return new DocumentDto
        {
            Id = document.Id,
            Title = document.Title,
            Content = document.Content,
            Summary = document.Summary,
            Tags = document.Tags,
            CategoryId = document.CategoryId,
            CategoryName = document.Category?.Name,
            Status = (int)document.Status,
            ViewCount = document.ViewCount,
            Version = document.Version,
            CreatedBy = document.CreatedBy,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }

    private static DocumentVersionDto MapToDto(DocumentVersion version)
    {
        return new DocumentVersionDto
        {
            Id = version.Id,
            DocumentId = version.DocumentId,
            VersionNumber = version.VersionNumber,
            Title = version.Title,
            Content = version.Content,
            Summary = version.Summary,
            Tags = version.Tags,
            ChangeLog = version.ChangeLog,
            CreatedBy = version.CreatedBy,
            CreatedAt = version.CreatedAt
        };
    }
}
