using System.Threading.Tasks;

namespace hrconnectbackend.Interface.SignalR
{
    public interface IMessageHub
    {
        Task RegisterUser(int user);
        Task SendMessageToAll(int user, string message);
        Task SendMessageToUser(int userId, int user, string message);
        Task SendMessageToGroup(string groupName, int user, string message);
        Task AddToGroup(List<string> groupName);
        Task RemoveFromGroup(string connectionId, string groupName);
    }
}