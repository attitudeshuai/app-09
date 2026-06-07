using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace KnowledgeBase.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IUserRepository? _users;
    private ICategoryRepository? _categories;
    private IDocumentRepository? _documents;
    private IDocumentVersionRepository? _documentVersions;
    private IDocumentFavoriteRepository? _documentFavorites;
    private IDocumentCommentRepository? _documentComments;
    private IDocumentViewHistoryRepository? _documentViewHistories;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public IDocumentRepository Documents => _documents ??= new DocumentRepository(_context);
    public IDocumentVersionRepository DocumentVersions => _documentVersions ??= new DocumentVersionRepository(_context);
    public IDocumentFavoriteRepository DocumentFavorites => _documentFavorites ??= new DocumentFavoriteRepository(_context);
    public IDocumentCommentRepository DocumentComments => _documentComments ??= new DocumentCommentRepository(_context);
    public IDocumentViewHistoryRepository DocumentViewHistories => _documentViewHistories ??= new DocumentViewHistoryRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<IDbTransaction> BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
        return _transaction.GetDbTransaction();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
