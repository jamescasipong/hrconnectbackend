using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.SignalR;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace hrconnectbackend.SignalR
{
    public class NotificationHub : Hub<INotificationHubClient>, INotificationHub
    {
        private readonly IEmployeeServices _employeeServices;
        private readonly INotificationServices _notificationServices;
        private static Dictionary<int, string> _userConnections = new Dictionary<int, string>();
        private static Dictionary<string, List<int>> _groupConnections = new Dictionary<string, List<int>>();
        private readonly IUserNotificationServices _userNotificationServices;
        private readonly DataContext _context;
        public NotificationHub(INotificationServices notificationServices, IEmployeeServices employeeServices, IUserNotificationServices userNotificationServices, DataContext context)
        {
            _notificationServices = notificationServices;
            _employeeServices = employeeServices;
            _userNotificationServices = userNotificationServices;
            _context = context;
        }

        public async Task RegisterUser(int userId)
        {
            try
            {
                var connectionId = Context.ConnectionId;

                // Store the userId and connectionId mapping
                if (!_userConnections.ContainsKey(userId))
                {
                    //_userConnections.Add(userId, connectionId);
                    _userConnections[userId] = connectionId;
                }

                Console.WriteLine($"User {userId} connected with Connection ID: {connectionId}");

                // Optionally, you can notify the user or other users that the user is online
                await Clients.Caller.RegisteredUser($"You are connected as {userId}");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task AddGroup(int groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());

            if (!_groupConnections.ContainsKey(Context.ConnectionId))
            {
                _groupConnections.Add(Context.ConnectionId, new List<int>());
            }
            else
            {
                _groupConnections[Context.ConnectionId].Add(groupId);
            }

            await Clients.Caller.AddedGroup($"You are added to group {groupId}");
        }

        // public override Task OnConnectedAsync()
        // {
        //     var userId = Context.ConnectionId;

        //     if (!string.IsNullOrEmpty(userId))
        //     {
        //         _userConnections[userId] = Context.ConnectionId;
        //     }

        //     return base.OnConnectedAsync();
        // }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

            if (userId != null)
            {
                _userConnections.Remove(userId, out _);
            }

            List<int> groupNames = _groupConnections[Context.ConnectionId];

            foreach (var groupName in groupNames)
            {
                Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName.ToString());
                Console.WriteLine($"User {userId} removed from group {groupName}");
            }


            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotificationToUser(int userId, CreateNotificationHubDTO notificationDTO)
        {
            var employee = await _employeeServices.GetByIdAsync(userId);
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (employee == null)
                {
                    throw new KeyNotFoundException($"Employee with id: {userId} not found.");
                }

                if (_userConnections.TryGetValue(userId, out var connectionId))
                {
                    var notification = new Notifications
                    {
                        Title = notificationDTO.Title,
                        Message = notificationDTO.Message,
                    };

                    await _notificationServices.AddAsync(notification);

                    var userNotification = new UserNotification
                    {
                        EmployeeId = userId,
                        NotificationId = notification.Id
                    };

                    await _userNotificationServices.AddAsync(userNotification);

                    await transaction.CommitAsync();

                    await Clients.Client(connectionId).ReceiveNotification(notificationDTO);
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        //public async Task SendNotificationByGroup(int groupName, CreateNotificationHubDTO notificationDTO)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        var notification = new Notifications
        //        {
        //            Title = notificationDTO.Title,
        //            Message = notificationDTO.Message,
        //        };

        //        await _notificationServices.AddAsync(notification);



        //        await Clients.Group(groupName.ToString()).ReceiveNotification(notificationDTO);
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        // Method to send notifications to all connected clients
        public async Task SendNotification(CreateNotificationHubDTO notificationDTO)
        {
            var notification = new Notifications
            {
                Title = notificationDTO.Title,
                Message = notificationDTO.Message
            };

            await _notificationServices.AddAsync(notification);

            await Clients.All.ReceiveNotification(notificationDTO);
        }

        // Method to receive notifications from clients (if needed)
    }
}
