using KnowledgeBase.Application.DTOs.DocumentVersions;
using KnowledgeBase.Application.DTOs.Documents;

namespace KnowledgeBase.Application.Interfaces;

public interface IDocumentVersionService
{
    Task<IEnumerable<DocumentVersionDto>> GetByDocumentIdAsync(long documentId);
    Task<DocumentVersionDto> GetByIdAsync(long id);
    Task<DocumentVersionDto?> GetByDocumentIdAndVersionAsync(long documentId, int versionNumber);
    Task<DocumentDto> RestoreVersionAsync(long documentId, int versionNumber, string? changeLog, long currentUserId);
}
