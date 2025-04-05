using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services
{
    public interface IUserNotificationServices: IGenericRepository<UserNotification>
    {
        public Task<List<UserNotification>> GetNotificationByUserId(int userId);
        public Task<UserNotification> GetUserNotificationById(int notificationId);
    }
}
