using BlazorChatSample.Shared;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorChatSample.Server.Hubs
{
    /// <summary>
    /// The SignalR hub 
    /// </summary>
    public class ChatHub : Hub
    {
        /// <summary>
        /// connectionId-to-username lookup
        /// </summary>
        /// <remarks>
        /// Needs to be static as the chat is created dynamically a lot
        /// </remarks>
        private static readonly Dictionary<string, string> userLookup = new Dictionary<string, string>();

        /// <summary>
        /// Send a message to all clients
        /// </summary>
        /// <param name="username"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessage(string username, string message)
        {
            await Clients.All.SendAsync(Messages.RECEIVE, username, message);
        }

        /// <summary>
        /// Send a private message to single user (a 'whisper')
        /// </summary>
        /// <param name="username">sender's nane</param>
        /// <param name="toUser">username to send to</param>
        /// <param name="message">message</param>
        /// <returns></returns>
        public async Task SendPrivateMessage(string username, string toUser, string message)
        {
            // lookup connectionId for toUser
            var connectionId = GetConnectionIdForUser(toUser);
            if (!string.IsNullOrEmpty(connectionId))
            {
                // send to single connection as a 'whisper'
                var msg = $"[whispers] {message}";
                await Clients.Client(connectionId).SendAsync(Messages.RECEIVE, username, msg);
            }
            else
                Console.WriteLine("Unable to find " + toUser);
        }

        /// <summary>
        /// Obtain connectionId from list
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private string GetConnectionIdForUser(string username)
        {
            Console.WriteLine("Lookup user " + username);
            var match = userLookup.FirstOrDefault(u => string.Equals(u.Value, username, StringComparison.InvariantCultureIgnoreCase));
            return match.Key;
        }

        /// <summary>
        /// Register username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task Register(string username)
        {
            var currentId = Context.ConnectionId;
            if (!userLookup.ContainsKey(currentId))
            {
                // maintain a lookup of connectionId-to-username
                userLookup.Add(currentId, username);
                // re-use existing message for now
                await Clients.AllExcept(currentId).SendAsync(
                    Messages.RECEIVE,
                    username, $"{username} joined the chat");
            }
        }

        /// <summary>
        /// Log connection
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            Console.WriteLine("Connected");
            return base.OnConnectedAsync();
        }

        /// <summary>
        /// Log disconnection
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception e)
        {
            Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
            // try to get connection
            string id = Context.ConnectionId;
            if (!userLookup.TryGetValue(id, out string username))
                username = "[unknown]";

            userLookup.Remove(id);
            await Clients.AllExcept(Context.ConnectionId).SendAsync(
                Messages.RECEIVE,
                username, $"{username} has left the chat");
            await base.OnDisconnectedAsync(e);
        }


    }
}
