

using hrconnectbackend.Models.DTOs;

namespace hrconnectbackend.Interface.SignalR {
    public interface INotificationHub {
        Task RegisterUser(int userId);
        Task AddGroup(int groupId);
        Task SendNotificationToUser(int userId, CreateNotificationHubDTO notificationDTO);
        Task SendNotificationByGroup(int groupName, CreateNotificationHubDTO notificationDTO);
        Task SendNotification(CreateNotificationHubDTO notificationDTO);
    }
}