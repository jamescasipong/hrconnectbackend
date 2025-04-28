using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services.ExternalServices;
using hrconnectbackend.Models;
using hrconnectbackend.Services.ExternalServices;

namespace hrconnectbackend.Features.Ongoing.Services;

public interface INotificationService
{
    Task SendExpirationWarning(UserAccount user);
}

public class NotificationService(DataContext dbContext, IEmailServices emailServices) : INotificationService
{
    private readonly DataContext _dbContext = dbContext;

    public async Task SendExpirationWarning(UserAccount user)
    {
        int? tenantId = user.OrganizationId!;
        var notification = new Notifications
        {
            TenantId = tenantId.Value,
            Title = "Your Premium Subscription is Expiring Soon",
            Message =
                "Your premium subscription will expire in 5 days. To avoid losing access to premium features, please renew your subscription.",
            CreatedAt = DateTime.Now,
            UpdatedAt = null
        };
        
        await _dbContext.Notifications.AddAsync(notification);
        await _dbContext.SaveChangesAsync();
        
        var userNotification = new UserNotification
        {
            EmployeeId = user.UserId,
            NotificationId = notification.Id,
            IsRead = false,
            TenantId = tenantId.Value,
            Notification = notification
        };
        
        await _dbContext.UserNotifications.AddAsync(userNotification);
        await _dbContext.SaveChangesAsync();

        // Send email notification
        await emailServices.SendEmailAsync(
            user.Email,
            "Your Premium Subscription is Expiring Soon",
            $"Hello {user.UserName},<br><br>" +
            $"Your premium subscription will expire in 5 days. " +
            $"To avoid losing access to premium features, please renew your subscription.<br><br>" +
            $"<a href='https://yourdomain.com/account/subscription'>Renew Now</a><br><br>" +
            $"Thank you for your continued support!<br>" +
            $"The Team"
        );
    }
}