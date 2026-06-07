namespace KnowledgeBase.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ICategoryRepository Categories { get; }
    IDocumentRepository Documents { get; }
    IDocumentVersionRepository DocumentVersions { get; }
    IDocumentFavoriteRepository DocumentFavorites { get; }
    IDocumentLikeRepository DocumentLikes { get; }
    IDocumentCommentRepository DocumentComments { get; }
    IDocumentViewHistoryRepository DocumentViewHistories { get; }
    IUserPasswordHistoryRepository UserPasswordHistories { get; }
    IOperationLogRepository OperationLogs { get; }
    Task<int> SaveChangesAsync();
    Task<ITransaction> BeginTransactionAsync();
}
