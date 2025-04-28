namespace hrconnectbackend.Features.Services;

public class ArchiveBackgroundService: BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Archive records older than 1 year
            // Wait for 24 hours before running again (or until cancellation request)
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}