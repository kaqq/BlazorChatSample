using BlazorChatSample.Shared;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorChatSample.Server.Hubs
{

    /// <summary>
    /// The SignalR hub 
    /// </summary>
    public class ChatHub : Hub<IGameClient>, IServerAgent
    {

        private static readonly Dictionary<string, string> userLookup = new Dictionary<string, string>();

        private static readonly Dictionary<string, string> userGroups = new Dictionary<string, string>();

        public async Task SendMessage(string message)
        {
            if (!userLookup.TryGetValue(Context.ConnectionId, out string username))
                username = "[unknown]";
            userGroups.TryGetValue(username, out string group);
            await Clients.Group(group).ReceiveMessage(username, message);
        }

        public async Task ExitGroup(string name)
        {
            if (!userLookup.TryGetValue(Context.ConnectionId, out string username))
                username = "[unknown]";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, name);
            userGroups.Remove(username);
            userGroups.TryGetValue(username, out string group);
            if (!string.IsNullOrEmpty(group))
            {
                await Clients.OthersInGroup(group).OnClientLeaved(username);
            }
            else
            {
                await Clients.All.OnGroupRemoved(name);
            }
        }

        public async Task PlayerAction(PlayerAction action)
        {

        }

        public async Task JoinRoom(string username, string groupName)
        {
            var currentId = Context.ConnectionId;
            if (!userLookup.ContainsKey(currentId))
            {
                userLookup.Add(currentId, username);
            }
            userGroups.Add(username, groupName);

            var group = userGroups.Values.Where(x => x == groupName).GroupBy(x => x)
                .Select(x => new Group() {GuestCount = x.Count().ToString(), Name = x.First()}).FirstOrDefault();
            await Clients.All.NewGroupAdded(group);
            Groups.AddToGroupAsync(currentId, groupName);
            await Clients.Group(groupName).OnClientJoined(username);
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Connected");
            await base.OnConnectedAsync();
            var groups = userGroups.Values.GroupBy(x => x)
                .Select(x => new Group() { GuestCount = x.Count().ToString(), Name = x.First() }).ToList();
            await Clients.Caller.OnGroupDataInfo(groups);
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            string id = Context.ConnectionId;
            if (!userLookup.TryGetValue(id, out string username))
                username = "[unknown]";
            userLookup.Remove(id);
            userGroups.TryGetValue(username, out string group);
            if (!string.IsNullOrEmpty(group))
            {
                await Clients.OthersInGroup(group).OnClientLeaved(username);
            }
            await base.OnDisconnectedAsync(e);
        }


    }

   
}
