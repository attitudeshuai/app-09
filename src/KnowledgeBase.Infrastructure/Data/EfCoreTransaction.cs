using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace KnowledgeBase.Infrastructure.Data;

public class EfCoreTransaction : ITransaction
{
    private readonly IDbContextTransaction _transaction;
    private bool _disposed;

    public EfCoreTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task CommitAsync()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(EfCoreTransaction));
        }
        await _transaction.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(EfCoreTransaction));
        }
        await _transaction.RollbackAsync();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _transaction.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _transaction.DisposeAsync();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
