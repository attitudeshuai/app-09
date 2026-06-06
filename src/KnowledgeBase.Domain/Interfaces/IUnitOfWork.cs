namespace KnowledgeBase.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ICategoryRepository Categories { get; }
    IDocumentRepository Documents { get; }
    IDocumentVersionRepository DocumentVersions { get; }
    Task<int> SaveChangesAsync();
}
