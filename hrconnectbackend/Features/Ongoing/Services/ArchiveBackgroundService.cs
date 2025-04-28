using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Features.Ongoing.Services;

public class ArchiveBackgroundService(DataContext dbContext, 
    INotificationService notificationService, 
    ISubscriptionServices subscriptionServices): BackgroundService
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