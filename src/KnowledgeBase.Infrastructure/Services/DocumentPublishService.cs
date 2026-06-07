using KnowledgeBase.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KnowledgeBase.Infrastructure.Services;

public class DocumentPublishService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DocumentPublishService> _logger;

    public DocumentPublishService(
        IServiceProvider serviceProvider,
        ILogger<DocumentPublishService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Document Publish Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                await PublishScheduledDocumentsAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while publishing scheduled documents.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogInformation("Document Publish Service is stopping.");
    }

    private async Task PublishScheduledDocumentsAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Checking for scheduled documents to publish...");

        using var scope = _serviceProvider.CreateScope();
        var documentService = scope.ServiceProvider.GetRequiredService<IDocumentService>();

        var publishedCount = await documentService.PublishScheduledDocumentsAsync();

        if (publishedCount > 0)
        {
            _logger.LogInformation("Successfully published {Count} scheduled documents.", publishedCount);
        }
    }
}
