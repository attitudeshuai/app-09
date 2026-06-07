using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface IPointRecordRepository : IRepository<PointRecord>
{
    Task<List<PointRecord>> GetByUserIdAsync(long userId, int pageNumber, int pageSize);
    Task<int> GetCountByUserIdAsync(long userId);
    Task<int> GetTotalPointsByUserIdAsync(long userId);
    Task<bool> ExistsBySourceAndReferenceAsync(long userId, PointSource source, long referenceId, string referenceType);
}
