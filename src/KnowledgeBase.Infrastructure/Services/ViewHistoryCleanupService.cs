using KnowledgeBase.Application.Options;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.Infrastructure.Services;

public class ViewHistoryCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<ViewHistoryOptions> _options;
    private readonly ILogger<ViewHistoryCleanupService> _logger;

    public ViewHistoryCleanupService(
        IServiceProvider serviceProvider,
        IOptions<ViewHistoryOptions> options,
        ILogger<ViewHistoryCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ViewHistory Cleanup Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                await CleanupAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while cleaning up view history.");
            }

            var interval = TimeSpan.FromHours(_options.Value.CleanupIntervalHours);
            await Task.Delay(interval, stoppingToken);
        }

        _logger.LogInformation("ViewHistory Cleanup Service is stopping.");
    }

    private async Task CleanupAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting view history cleanup...");

        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var maxRecordsPerUser = _options.Value.MaxRecordsPerUser;
        var expirationPeriod = TimeSpan.FromDays(_options.Value.ExpirationDays);

        var removedCount = await unitOfWork.DocumentViewHistories.CleanupOldRecordsAsync(maxRecordsPerUser, expirationPeriod);
        await unitOfWork.SaveChangesAsync();

        _logger.LogInformation("View history cleanup completed successfully. Removed {Count} records.", removedCount);
    }
}
