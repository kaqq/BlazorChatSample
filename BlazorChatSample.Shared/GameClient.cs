using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace BlazorChatSample.Shared
{

    public class GameClient : IAsyncDisposable
    {
        public const string HUBURL = "/ChatHub";

        private readonly string _hubUrl;
        private HubConnection _hubConnection;

        public GameClient(string username,string roomName, string siteUrl)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(roomName))
                throw new ArgumentNullException(nameof(roomName));
            if (string.IsNullOrWhiteSpace(siteUrl))
                throw new ArgumentNullException(nameof(siteUrl));
            _username = username;
            _roomName = roomName;
            _hubUrl = siteUrl.TrimEnd('/') + HUBURL;
        }

        private readonly string _username;

        private readonly string _roomName;

        private bool _started = false;


        public async Task StartAsync()
        {
            if (!_started)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl)
                    .Build();
                RegisterServerEvents();
                await _hubConnection.StartAsync();
                await _hubConnection.SendAsync(ServerMessages.JOINROOM, _username, _roomName);
                _started = true;
            }
        }

        private void RegisterServerEvents()
        {
            _hubConnection.On<string, string>(ServerMessages.RECEIVE, (user, message) =>
            {
                HandleReceiveMessage(user, message);
            });
            _hubConnection.On<string>(ClientMessages.CLIENTJOINED, (user) =>
            {
                HandleNewClient(user);
            });
        }

        private void HandleNewClient(string user)
        {
          
        }

        /// <summary>
        /// Handle an inbound message from a hub
        /// </summary>
        /// <param name="method">event name</param>
        /// <param name="message">message content</param>
        private void HandleReceiveMessage(string username, string message)
        {
            // raise an event to subscribers
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(username, message));
        }

        /// <summary>
        /// Event raised when this client receives a message
        /// </summary>
        /// <remarks>
        /// Instance classes should subscribe to this event
        /// </remarks>
        public event MessageReceivedEventHandler MessageReceived;

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
            await _hubConnection.SendAsync(ServerMessages.SEND, _username, message);
        }

        public async Task PlayerAction(PlayerAction action)
        {
            await _hubConnection.SendAsync(ServerMessages.PLAYERACTION,  action);
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
    }

    /// <summary>
    /// Delegate for the message handler
    /// </summary>
    /// <param name="sender">the SignalRclient instance</param>
    /// <param name="e">Event args</param>
    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

    /// <summary>
    /// Message received argument class
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(string username, string message)
        {
            Username = username;
            Message = message;
        }

        /// <summary>
        /// Name of the message/event
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Message data items
        /// </summary>
        public string Message { get; set; }

    }

}

