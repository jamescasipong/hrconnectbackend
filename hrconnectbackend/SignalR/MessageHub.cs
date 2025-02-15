using hrconnectbackend.Data;
using hrconnectbackend.Interface.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace hrconnectbackend.SignalR
{
    public class MessageHub: Hub<IMessageHubClient>, IMessageHub
    {
        private readonly Dictionary<int, string> _userConnections = new Dictionary<int, string>();
        private readonly Dictionary<string, List<string>> _userGroupConnections = new Dictionary<string, List<string>>();
        private readonly DataContext _context;

        public MessageHub(DataContext context){
            _context = context;
        } 

        public override async Task OnDisconnectedAsync(Exception? ex){
            
            List<string> groupNames = _userGroupConnections[GetConnectionId()];

            foreach (var groupName in groupNames){
                await RemoveFromGroup(GetConnectionId(), groupName);
            }

            var userId = _userConnections.FirstOrDefault(x => x.Value == GetConnectionId()).Key;

            _userConnections.Remove(userId);

            await base.OnDisconnectedAsync(ex);
        }
        
        public async Task RegisterUser(int userId)
        {
            try {
                if (!_userConnections.ContainsKey(userId)){
                    _userConnections[userId] = GetConnectionId();
                }

                await Clients.Caller.AddedClient($"{userId} with {GetConnectionId()} has successfully been added.");
            }
            catch (Exception ex){
                throw new Exception(ex.Message);
            }
        }

        public async Task SendMessageToAll(int sender, string message)
        {
            try {
                await Clients.All.ReceiveMessageByAll(sender, message);
            }
            catch (Exception ex){
                throw new Exception(ex.Message);
            }
        }

        public async Task SendMessageToUser(int userId, int sender, string message)
        {
            try {
                if (_userConnections.TryGetValue(userId, out var connectionId)){
                    await Clients.Client(connectionId).ReceiveMessage(sender, message);
                }
                else {
                    throw new Exception($"{userId} not online.");
                }
            }
            catch (Exception ex){
                throw new Exception(ex.Message);
            }
        }

        public async Task SendMessageToGroup(string groupName, int user, string message)
        {
            try {
                await Clients.Group(groupName).ReceiveMessageByGroup(user, message);
            }
            catch (Exception ex){
                throw new Exception(ex.Message);
            }
        }

        public async Task AddToGroup(List<string> groupName)
        {
            if (!groupName.Any()){
                throw new Exception($"No group name found.");
            }

            try {
                if (!_userGroupConnections.ContainsKey(GetConnectionId())){
                    _userGroupConnections.Add(GetConnectionId(), new List<string>());
                }
                else {
                    foreach (var grpName in groupName){
                        _userGroupConnections[GetConnectionId()].Add(grpName);
                    }
                }

                foreach (var grpName in groupName){
                    await Groups.AddToGroupAsync(GetConnectionId(), grpName);
                }
            }
            catch (Exception ex){
                throw new Exception(ex.Message);
            }
        }

        public async Task RemoveFromGroup(string connectionId, string groupName)
        {
            await Groups.RemoveFromGroupAsync(connectionId, groupName);
        }

        private string GetConnectionId(){
            return Context.ConnectionId;
        }
    }
}
