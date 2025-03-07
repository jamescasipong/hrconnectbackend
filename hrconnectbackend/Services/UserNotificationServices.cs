using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services
{
    public class UserNotificationServices: GenericRepository<UserNotification>, IUserNotificationServices
    {
        public UserNotificationServices(DataContext context) : base(context)
        {

        }

        public async Task<List<UserNotification>> GetNotificationByUserId(int userId)
        {
            var employeeNotifications = await _context.UserNotifications.Include(e => e.Notification).Where(e => e.EmployeeId == userId).ToListAsync();

            return employeeNotifications;

        }
    }
}
