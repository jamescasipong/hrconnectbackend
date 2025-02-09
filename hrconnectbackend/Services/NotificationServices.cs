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

    public async Task<List<Notifications>> GetNotificationsByEmployeeId(int id)
    {
        var notifications = await _context.Notifications.Where(e => e.EmployeeId == id).ToListAsync();

        return notifications;
    }

    public async Task<List<Notifications>> NotifcationPagination(int pageIndex, int pageSize, int? employeeId)
    {
        var notifcations = new List<Notifications>();

        if (employeeId == null)
        {
            notifcations = await _context.Notifications.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }
        else
        {
            notifcations = await _context.Notifications.Where(n => n.EmployeeId == employeeId).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        return notifcations;
    }
}