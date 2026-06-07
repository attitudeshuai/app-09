using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.OperationLogs;

namespace KnowledgeBase.Application.Interfaces;

public interface IOperationLogService
{
    Task LogAsync(long userId, CreateOperationLogRequest request, string? ipAddress = null, string? userAgent = null);
    Task<PagedResult<OperationLogDto>> GetPagedAsync(OperationLogPagedRequest request);
}
