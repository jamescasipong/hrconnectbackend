

using hrconnectbackend.Models.DTOs;

namespace hrconnectbackend.Interface.SignalR {
    public interface INotificationHubClient {
        Task ReceiveNotificationByGroup(CreateNotificationHubDto notif);
        Task ReceiveNotification(CreateNotificationHubDto notif);
        Task RegisteredUser(string message);
        Task AddedGroup(string message);
    }
}