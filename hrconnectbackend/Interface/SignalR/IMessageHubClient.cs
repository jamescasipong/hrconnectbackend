using System.Threading.Tasks;

namespace hrconnectbackend.Interface.SignalR
{
    /// <summary>
    /// Defines a contract for a SignalR message hub.
    /// </summary>
    public interface IMessageHubClient
    {
        /// <summary>
        /// Sends a message to a specified user.
        /// </summary>
        /// <param name="user">The user to whom the message will be sent.</param>
        /// <param name="message">The message content to be sent.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task ReceiveMessage(int sender, string message);
        Task ReceiveMessageByGroup(int sender, string message);
        Task ReceiveMessageByAll(int sender, string message);
        Task AddedClient(string message);
    }
}