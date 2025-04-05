using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients
{
    public class UserNotificationServices(DataContext context)
        : GenericRepository<UserNotification>(context), IUserNotificationServices
    {
        public async Task<List<UserNotification>> GetNotificationByUserId(int userId)
        {
            var employeeNotifications = await _context.UserNotifications.Include(e => e.Notification).Where(e => e.EmployeeId == userId).ToListAsync();

            return employeeNotifications;

        }

        public async Task<UserNotification?> GetUserNotificationById(int notificationId)
        {
            var employeeNotification = await _context.UserNotifications.FirstOrDefaultAsync(e => e.NotificationId == notificationId);

            return employeeNotification;
        }
    }
}
