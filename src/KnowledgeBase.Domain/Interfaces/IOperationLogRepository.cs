using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface IOperationLogRepository
{
    Task<OperationLog> AddAsync(OperationLog entity);
    Task<(IEnumerable<OperationLog> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        long? userId,
        string? actionType,
        string? targetType,
        DateTime? startTime,
        DateTime? endTime);
}
