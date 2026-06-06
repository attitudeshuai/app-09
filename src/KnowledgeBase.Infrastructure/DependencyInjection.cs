using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using KnowledgeBase.Infrastructure.Caching;
using KnowledgeBase.Infrastructure.Data;
using KnowledgeBase.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace KnowledgeBase.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                b => b.MigrationsAssembly("KnowledgeBase.Infrastructure")));

        var redisConnectionString = configuration.GetConnectionString("RedisConnection");
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString ?? "localhost:6379"));

        services.AddScoped<IRedisCacheService, RedisCacheService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
