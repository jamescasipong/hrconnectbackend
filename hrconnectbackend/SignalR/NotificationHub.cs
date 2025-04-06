using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.SignalR;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using hrconnectbackend.Interface.Services.Clients;

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
        private readonly ILogger<NotificationHub> _logger;
        public NotificationHub(INotificationServices notificationServices, IEmployeeServices employeeServices, IUserNotificationServices userNotificationServices, DataContext context, ILogger<NotificationHub> logger)
        {
            _notificationServices = notificationServices;
            _employeeServices = employeeServices;
            _userNotificationServices = userNotificationServices;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// This method is called when a user connects to the hub.
        /// It registers the user by storing their userId and connectionId in a dictionary.
        /// It also logs the connection event and notifies the user that they are connected.
        /// The userId is expected to be passed as a parameter.
        /// The method handles exceptions and throws them if any occur.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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

        /// <summary>
        /// This method is called to add a user to a specific group.
        /// It adds the user to the group using SignalR's Groups.AddToGroupAsync method.
        /// It also updates the _groupConnections dictionary to keep track of the user's group memberships.
        /// The method logs the addition of the user to the group.
        /// Finally, it sends a message to the caller indicating that they have been added to the group.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
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

        
        /// <summary>
        /// This method is called when a client disconnects from the hub.
        /// It removes the userId and connectionId mapping from the dictionary.
        /// It also removes the user from all groups they were part of.
        /// The method logs the disconnection event.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

            if (userId != 0)
            {
                // Remove the userId and connectionId mapping
                _userConnections.Remove(userId);
                _logger.LogInformation($"User {userId} disconnected with Connection ID: {Context.ConnectionId}");
            }


            List<int> groupNames = _groupConnections[Context.ConnectionId];

            foreach (var groupName in groupNames)
            {
                Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName.ToString());
                Console.WriteLine($"User {userId} removed from group {groupName}");
            }


            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Sends a notification to a specific user by their userId.
        /// This method checks if the user is connected and sends the notification to their connectionId.
        /// It also creates a new notification and associates it with the user in the database.
        /// If the user is not found, it throws a KeyNotFoundException.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="notificationDTO"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task SendNotificationToUser(int userId, CreateNotificationHubDto notificationDTO)
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

        /// <summary>
        /// Sends a notification to all connected clients.
        /// This method creates a new notification and sends it to all clients.
        /// It does not check for specific users or groups.
        /// It also creates a new notification in the database.
        /// </summary>
        /// <param name="notificationDTO"></param>
        /// <returns></returns>
        public async Task SendNotification(CreateNotificationHubDto notificationDTO)
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
