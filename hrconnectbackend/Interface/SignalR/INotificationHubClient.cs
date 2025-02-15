

using hrconnectbackend.Models.DTOs;

namespace hrconnectbackend.Interface.SignalR {
    public interface INotificationHubClient {
        Task ReceiveNotificationByGroup(CreateNotificationHubDTO notif);
        Task ReceiveNotification(CreateNotificationHubDTO notif);
        Task RegisteredUser(string message);
        Task AddedGroup(string message);
    }
}