using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlazorChatSample.Shared
{
    public interface IChatClient
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Send a message to the hub
        /// </summary>
        /// <param name="message">message to send</param>
        Task SendAsync(string message);
    }
    public interface IRoomDataClient
    {
        event EventHandler<RoomUpdateEventArgs> GroupChanged;
        event EventHandler<RoomUpdateEventArgs> GroupRemoved;
    }

    public class GameClient : IGameClient, IAsyncDisposable, IChatClient, IRoomDataClient
    {
        public static string HUBURL = "/ChatHub";

        private string _hubUrl;

        private HubConnection _hubConnection;

        public void Init(string siteUrl)
        {
            if (string.IsNullOrWhiteSpace(siteUrl))
                throw new ArgumentNullException(nameof(siteUrl));
            _hubUrl = siteUrl.TrimEnd('/') + HUBURL;
        }

        private string _username;

        private string _roomName;

        private bool _started = false;


        public async Task StartAsync()
        {
            if (!_started)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl)
                    .Build();
                BrowserAgentHelper.RegisterServerEventsForType<GameClient,IGameClient>(this,_hubConnection);
                await _hubConnection.StartAsync();
                _started = true;
            }
        }

        public async Task StartGame(string username, string roomName)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(roomName))
                throw new ArgumentNullException(nameof(roomName));
            _username = username;
            _roomName = roomName;
            await _hubConnection.SendAsync(ServerMessages.JOINROOM, _username, _roomName);
        }


        public async Task ReceiveMessage(string user, string message)
        {
            Console.WriteLine($"Incomming message: {user} {message}" );
            bool isMine = false;
            if (!string.IsNullOrWhiteSpace(user))
            {
                isMine = string.Equals(user, _username, StringComparison.CurrentCultureIgnoreCase);
            }
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(user, message, isMine));
        }

        public async Task NewGroupAdded(Group @group)
        {
            this.GroupChanged?.Invoke(this,new RoomUpdateEventArgs(group.Name,group.GuestCount));
        }

        public async Task OnClientJoined(string username)
        {
           this.ClientJoined?.Invoke(this,new ClientUpdateEventArgs(username));
        }

        public async Task OnGroupDataInfo(List<Group> groups)
        {
            foreach (var group in groups)
            {
                this.GroupChanged?.Invoke(this, new RoomUpdateEventArgs(group.Name, group.GuestCount));
            }
        }

        public async Task OnClientLeaved(string username)
        {
            this.ClientLeaved?.Invoke(this, new ClientUpdateEventArgs(username));
        }

        public async Task OnGroupRemoved(String groupName)
        {
            this.GroupRemoved?.Invoke(this, new RoomUpdateEventArgs(groupName, "0"));
        }

        public event EventHandler<RoomUpdateEventArgs> GroupChanged;

        public event EventHandler<RoomUpdateEventArgs> GroupRemoved;

        public event EventHandler<ClientUpdateEventArgs> ClientLeaved;

        public event EventHandler<ClientUpdateEventArgs> ClientJoined;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Send a message to the hub
        /// </summary>
        /// <param name="message">message to send</param>
        public async Task SendAsync(string message)
        {
            // check we are connected
            if (!_started)
                throw new InvalidOperationException("Client not started");
            // send the message
            await _hubConnection.SendAsync(nameof(IServerAgent.SendMessage), message);
        }

        public async Task PlayerAction(PlayerAction action)
        {
            await _hubConnection.SendAsync(ServerMessages.PLAYERACTION, action);
        }

        /// <summary>
        /// Stop the client (if started)
        /// </summary>
        public async Task StopAsync()
        {
            if (_started)
            {
                // disconnect the client
                await _hubConnection.StopAsync();
                // There is a bug in the mono/SignalR client that does not
                // close connections even after stop/dispose
                // see https://github.com/mono/mono/issues/18628
                // this means the demo won't show "xxx left the chat" since 
                // the connections are left open
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
                _started = false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("ChatClient: Disposing");
            await StopAsync();
        }


        public async Task ExitGroup(string groupName)
        {
            await _hubConnection.SendAsync(nameof(IServerAgent.ExitGroup), groupName);
        }
    }


    public class MessageReceivedEventArgs : ClientUpdateEventArgs
    {
        public MessageReceivedEventArgs(string username, string message,bool isMine) :base(username)
        {
            Message = message;
            IsMine = isMine;
        }

        public bool IsMine { get; set; }

        public string Message { get; set; }
    }

    public class ClientUpdateEventArgs : EventArgs
    {
        public ClientUpdateEventArgs(string username)
        {
            Username = username;
        }
        public string Username { get; set; }
    }
    public class RoomUpdateEventArgs : EventArgs
    {
        public RoomUpdateEventArgs(string roomName,string guests)
        {
            RoomName = roomName;
            Guests = guests;
        }
        public string RoomName { get; set; }
        public string Guests { get; set; }
    }
}

