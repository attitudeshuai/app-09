using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.Users;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;

namespace KnowledgeBase.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordValidator _passwordValidator;
    private const int PasswordHistoryLimit = 3;

    public UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IPasswordValidator passwordValidator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _passwordValidator = passwordValidator;
    }

    public async Task<UserDto> GetByIdAsync(long id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("用户不存在");
        }
        return MapToDto(user);
    }

    public async Task<PagedResult<UserDto>> GetPagedAsync(UserPagedRequest request)
    {
        var users = await _unitOfWork.Users.GetPagedAsync(request.PageNumber, request.PageSize, request.Keyword);
        var totalCount = await _unitOfWork.Users.CountAsync(request.Keyword);

        return new PagedResult<UserDto>
        {
            Items = users.Select(MapToDto),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, long currentUserId)
    {
        if (await _unitOfWork.Users.UsernameExistsAsync(request.Username))
        {
            throw new InvalidOperationException("用户名已存在");
        }
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException("邮箱已存在");
        }

        var validationResult = _passwordValidator.Validate(request.Password);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException(string.Join("；", validationResult.Errors));
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Nickname = request.Nickname,
            Avatar = request.Avatar,
            Role = (UserRole)request.Role,
            IsActive = true,
            CreatedBy = currentUserId
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task UpdateAsync(long id, UpdateUserRequest request, long currentUserId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("用户不存在");
        }

        if (user.Email != request.Email && await _unitOfWork.Users.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException("邮箱已被使用");
        }

        user.Email = request.Email;
        user.Nickname = request.Nickname;
        user.Avatar = request.Avatar;
        if (request.Role.HasValue)
        {
            user.Role = (UserRole)request.Role.Value;
        }
        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }
        user.UpdatedBy = currentUserId;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        if (!await _unitOfWork.Users.ExistsAsync(id))
        {
            throw new KeyNotFoundException("用户不存在");
        }
        await _unitOfWork.Users.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(long id, ChangePasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("用户不存在");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("账号已被禁用，无法修改密码");
        }

        if (user.IsLockedOut)
        {
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                throw new InvalidOperationException($"账号已被锁定，锁定将于 {user.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss} 解除");
            }

            user.IsLockedOut = false;
            user.LockoutEnd = null;
        }

        if (!_passwordHasher.VerifyPassword(request.OldPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("原密码错误");
        }

        var validationResult = _passwordValidator.Validate(request.NewPassword);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException(string.Join("；", validationResult.Errors));
        }

        if (_passwordHasher.VerifyPassword(request.NewPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("新密码不能与当前密码相同");
        }

        var recentHistories = await _unitOfWork.UserPasswordHistories.GetRecentByUserIdAsync(id, PasswordHistoryLimit);
        foreach (var history in recentHistories)
        {
            if (_passwordHasher.VerifyPassword(request.NewPassword, history.PasswordHash))
            {
                throw new InvalidOperationException($"新密码不能与最近 {PasswordHistoryLimit} 次使用过的历史密码重复");
            }
        }

        var oldPasswordHash = user.PasswordHash;
        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedBy = id;

        await _unitOfWork.UserPasswordHistories.AddAsync(id, oldPasswordHash);

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _unitOfWork.Users.UsernameExistsAsync(username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _unitOfWork.Users.EmailExistsAsync(email);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Nickname = user.Nickname,
            Avatar = user.Avatar,
            Role = (int)user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
