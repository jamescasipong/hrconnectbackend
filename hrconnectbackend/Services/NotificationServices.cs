using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories;

public class NotificationServices : GenericRepository<Notifications>, INotificationServices
{
    private readonly ILogger<Notifications> _logger;
    public NotificationServices(DataContext context, ILogger<Notifications> logger) : base(context)
    {
        _logger = logger;

    }

    public async Task<List<Notifications>> GetNotificationsByEmployeeId(int id, int? pageIndex, int? pageSize)
    {
        var notifications = await _context.Notifications.Where(e => e.EmployeeId == id).ToListAsync();

        var notificationPaginations = NotifcationPagination(notifications, pageIndex, pageSize); 

        return notificationPaginations;
    }

    public List<Notifications> NotifcationPagination(List<Notifications> notifications, int? pageIndex, int? pageSize)
    {
        if (pageIndex.HasValue && pageIndex.Value <= 0)
        {
            throw new ArgumentOutOfRangeException($"Page index must be higher than 0");
        }

        if (pageSize.HasValue && pageSize.Value <= 0)
        {
            throw new ArgumentOutOfRangeException($"Page size must be higher than 0");
        }

        if (!pageIndex.HasValue || !pageSize.HasValue)
        {
            return notifications;
        }

        return notifications.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
    }
}