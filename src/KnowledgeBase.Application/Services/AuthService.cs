using KnowledgeBase.Application.DTOs.Auth;
using KnowledgeBase.Application.DTOs.Users;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Application.Options;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KnowledgeBase.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ICacheService _cacheService;
    private readonly LockoutOptions _lockoutOptions;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ICacheService cacheService,
        IOptions<LockoutOptions> lockoutOptions)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _cacheService = cacheService;
        _lockoutOptions = lockoutOptions.Value;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("用户名或密码错误");
        }

        var failedAttemptsCacheKey = GetFailedAttemptsCacheKey(user.Username);

        if (user.IsLockedOut)
        {
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                var remainingTime = user.LockoutEnd.Value - DateTime.UtcNow;
                throw new UnauthorizedAccessException($"账号已被锁定，请在 {Math.Ceiling(remainingTime.TotalMinutes)} 分钟后再试");
            }

            user.IsLockedOut = false;
            user.LockoutEnd = null;
            await _cacheService.RemoveAsync(failedAttemptsCacheKey);
            await _unitOfWork.SaveChangesAsync();
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            await HandleFailedLoginAsync(user);
            throw new UnauthorizedAccessException("用户名或密码错误");
        }

        await _cacheService.RemoveAsync(failedAttemptsCacheKey);

        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:ExpireHours"] ?? "24"));

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = MapToUserDto(user)
        };
    }

    private async Task HandleFailedLoginAsync(User user)
    {
        var cacheKey = GetFailedAttemptsCacheKey(user.Username);
        var now = DateTime.UtcNow;
        var windowDuration = TimeSpan.FromMinutes(_lockoutOptions.FailedAttemptWindowMinutes);

        var failedAttemptInfo = await _cacheService.GetAsync<FailedAttemptInfo>(cacheKey);
        var failedAttempts = 0;

        if (failedAttemptInfo != null && now - failedAttemptInfo.WindowStart < windowDuration)
        {
            failedAttempts = failedAttemptInfo.AttemptCount;
        }
        else
        {
            failedAttemptInfo = new FailedAttemptInfo
            {
                AttemptCount = 0,
                WindowStart = now
            };
        }

        failedAttempts++;
        failedAttemptInfo.AttemptCount = failedAttempts;

        var cacheExpiry = windowDuration.Add(TimeSpan.FromMinutes(1));
        await _cacheService.SetAsync(cacheKey, failedAttemptInfo, cacheExpiry);

        if (failedAttempts >= _lockoutOptions.MaxFailedAttempts)
        {
            user.IsLockedOut = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(_lockoutOptions.LockoutMinutes);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private static string GetFailedAttemptsCacheKey(string username)
    {
        return $"login:failed_attempts:{username}";
    }

    private class FailedAttemptInfo
    {
        public int AttemptCount { get; set; }
        public DateTime WindowStart { get; set; }
    }

    public async Task<UserDto> GetCurrentUserAsync(long userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("用户不存在");
        }
        return MapToUserDto(user);
    }

    private string GenerateJwtToken(User user)
    {
        var key = _configuration["Jwt:Secret"] ?? "KnowledgeBaseSecretKey1234567890";
        var issuer = _configuration["Jwt:Issuer"] ?? "KnowledgeBase";
        var audience = _configuration["Jwt:Audience"] ?? "KnowledgeBase";
        var expireHours = double.Parse(_configuration["Jwt:ExpireHours"] ?? "24");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expireHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto MapToUserDto(User user)
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
