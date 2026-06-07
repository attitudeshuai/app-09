using System.Data;

namespace KnowledgeBase.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ICategoryRepository Categories { get; }
    IDocumentRepository Documents { get; }
    IDocumentVersionRepository DocumentVersions { get; }
    IDocumentFavoriteRepository DocumentFavorites { get; }
    IDocumentCommentRepository DocumentComments { get; }
    IDocumentViewHistoryRepository DocumentViewHistories { get; }
    Task<int> SaveChangesAsync();
    Task<IDbTransaction> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
