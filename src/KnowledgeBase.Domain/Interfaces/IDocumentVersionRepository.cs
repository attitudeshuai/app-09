using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface IDocumentVersionRepository : IRepository<DocumentVersion>
{
    Task<IEnumerable<DocumentVersion>> GetByDocumentIdAsync(long documentId);
    Task<DocumentVersion?> GetByDocumentIdAndVersionAsync(long documentId, int versionNumber);
    Task<int> GetLatestVersionNumberAsync(long documentId);
}
