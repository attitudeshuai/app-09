using KnowledgeBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

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
    private IUserPasswordHistoryRepository? _userPasswordHistories;

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
    public IUserPasswordHistoryRepository UserPasswordHistories => _userPasswordHistories ??= new UserPasswordHistoryRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<ITransaction> BeginTransactionAsync()
    {
        var transaction = await _context.Database.BeginTransactionAsync();
        return new EfCoreTransaction(transaction);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
