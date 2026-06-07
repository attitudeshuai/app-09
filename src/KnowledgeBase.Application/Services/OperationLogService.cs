using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.OperationLogs;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace KnowledgeBase.Application.Services;

public class OperationLogService : IOperationLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OperationLogService> _logger;

    public OperationLogService(IUnitOfWork unitOfWork, ILogger<OperationLogService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task LogAsync(long userId, CreateOperationLogRequest request, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var log = new OperationLog
            {
                UserId = userId,
                ActionType = request.ActionType,
                TargetType = request.TargetType,
                TargetId = request.TargetId,
                TargetName = request.TargetName,
                Details = request.Details,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            await _unitOfWork.OperationLogs.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录操作日志失败，UserId: {UserId}, ActionType: {ActionType}", userId, request.ActionType);
        }
    }

    public async Task<PagedResult<OperationLogDto>> GetPagedAsync(OperationLogPagedRequest request)
    {
        try
        {
            var (items, totalCount) = await _unitOfWork.OperationLogs.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.UserId,
                request.ActionType,
                request.TargetType,
                request.StartTime,
                request.EndTime);

            return new PagedResult<OperationLogDto>
            {
                Items = items.Select(MapToDto),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取操作日志列表失败");
            throw;
        }
    }

    private static OperationLogDto MapToDto(OperationLog log)
    {
        return new OperationLogDto
        {
            Id = log.Id,
            UserId = log.UserId,
            Username = log.User?.Username,
            Nickname = log.User?.Nickname,
            ActionType = log.ActionType,
            TargetType = log.TargetType,
            TargetId = log.TargetId,
            TargetName = log.TargetName,
            Details = log.Details,
            CreatedAt = log.CreatedAt,
            IpAddress = log.IpAddress
        };
    }
}
