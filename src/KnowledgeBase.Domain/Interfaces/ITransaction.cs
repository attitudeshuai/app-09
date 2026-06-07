namespace KnowledgeBase.Domain.Interfaces;

public interface ITransaction : IDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}
