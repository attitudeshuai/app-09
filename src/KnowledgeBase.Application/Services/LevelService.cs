using KnowledgeBase.Application.DTOs.Levels;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;

namespace KnowledgeBase.Application.Services;

public class LevelService : ILevelService
{
    private readonly IUnitOfWork _unitOfWork;

    public LevelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<LevelDto>> GetAllAsync()
    {
        var levels = await _unitOfWork.Levels.GetAllAsync();
        return levels.OrderBy(l => l.LevelNumber).Select(MapToDto).ToList();
    }

    public async Task<LevelDto?> GetByIdAsync(long id)
    {
        var level = await _unitOfWork.Levels.GetByIdAsync(id);
        return level != null ? MapToDto(level) : null;
    }

    public async Task<LevelDto?> GetByLevelNumberAsync(int levelNumber)
    {
        var level = await _unitOfWork.Levels.GetByLevelNumberAsync(levelNumber);
        return level != null ? MapToDto(level) : null;
    }

    public async Task<LevelDto> CreateAsync(CreateLevelRequest request, long currentUserId)
    {
        var existing = await _unitOfWork.Levels.GetByLevelNumberAsync(request.LevelNumber);
        if (existing != null)
        {
            throw new InvalidOperationException("该等级编号已存在");
        }

        var level = new Level
        {
            Name = request.Name,
            LevelNumber = request.LevelNumber,
            RequiredPoints = request.RequiredPoints,
            Icon = request.Icon,
            Color = request.Color,
            Description = request.Description,
            CreatedBy = currentUserId
        };

        await _unitOfWork.Levels.AddAsync(level);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(level);
    }

    public async Task UpdateAsync(long id, UpdateLevelRequest request, long currentUserId)
    {
        var level = await _unitOfWork.Levels.GetByIdAsync(id);
        if (level == null)
        {
            throw new KeyNotFoundException("等级不存在");
        }

        if (request.LevelNumber.HasValue && request.LevelNumber.Value != level.LevelNumber)
        {
            var existing = await _unitOfWork.Levels.GetByLevelNumberAsync(request.LevelNumber.Value);
            if (existing != null && existing.Id != id)
            {
                throw new InvalidOperationException("该等级编号已存在");
            }
        }

        if (request.Name != null) level.Name = request.Name;
        if (request.LevelNumber.HasValue) level.LevelNumber = request.LevelNumber.Value;
        if (request.RequiredPoints.HasValue) level.RequiredPoints = request.RequiredPoints.Value;
        if (request.Icon != null) level.Icon = request.Icon;
        if (request.Color != null) level.Color = request.Color;
        if (request.Description != null) level.Description = request.Description;

        level.UpdatedBy = currentUserId;
        await _unitOfWork.Levels.UpdateAsync(level);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        if (!await _unitOfWork.Levels.ExistsAsync(id))
        {
            throw new KeyNotFoundException("等级不存在");
        }

        await _unitOfWork.Levels.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    private static LevelDto MapToDto(Level level)
    {
        return new LevelDto
        {
            Id = level.Id,
            Name = level.Name,
            LevelNumber = level.LevelNumber,
            RequiredPoints = level.RequiredPoints,
            Icon = level.Icon,
            Color = level.Color,
            Description = level.Description
        };
    }
}
