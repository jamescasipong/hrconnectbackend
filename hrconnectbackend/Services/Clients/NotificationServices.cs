using hrconnectbackend.Constants;
using hrconnectbackend.Data;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients;

public class NotificationServices(DataContext context)
    : GenericRepository<Notifications>(context), INotificationServices
{
    public async Task<List<UserNotification>> GetNotificationsByEmployeeId(int id, int? pageIndex, int? pageSize)
    {
        List<UserNotification> notifications = await _context.UserNotifications
            .Include(x => x.Notification)
            .Where(x => x.EmployeeId == id)
            .ToListAsync();

        var notificationPaginations = UserNotificationsPagination(notifications, pageIndex, pageSize);

        return notificationPaginations;
    }

    public async Task<UserNotification> RetrieveNotification(int id)
    {
        var notification = await _context.UserNotifications.Include(x => x.Notification).FirstOrDefaultAsync(x => x.NotificationId == id);

        if (notification == null)
        {
            throw new KeyNotFoundException($"No notification found with an id {id}");
        }

        return notification;
    }

    public List<UserNotification> UserNotificationsPagination(List<UserNotification> notifications, int? pageIndex, int? pageSize)
    {
        if (pageIndex.HasValue && pageIndex.Value <= 0)
        {
            throw new BadRequestException(ErrorCodes.InvalidInput, $"Page index must be higher than 0");
        }

        if (pageSize.HasValue && pageSize.Value <= 0)
        {
            throw new BadRequestException(ErrorCodes.InvalidInput, $"Page index must be higher than 0");
        }

        if (!pageIndex.HasValue || !pageSize.HasValue)
        {
            return notifications;
        }

        return notifications.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
    }

    public List<Notifications> NotificationPagination(List<Notifications> notifications, int? pageIndex, int? pageSize)
    {
        if (pageIndex.HasValue && pageIndex.Value <= 0)
        {
            throw new BadRequestException(ErrorCodes.InvalidInput, $"Page index must be higher than 0");
        }
        if (pageSize.HasValue && pageSize.Value <= 0)
        {
            throw new BadRequestException(ErrorCodes.InvalidInput, $"Page size must be higher than 0");
        }
        if (!pageIndex.HasValue || !pageSize.HasValue)
        {
            return notifications;
        }
        return notifications.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
    }
}