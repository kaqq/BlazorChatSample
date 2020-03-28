using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorChatSample.Shared
{
    public class ChatRoom
    {
        public string Name { get; set; }
        public int GuestCount { get; set; }
    }

    public class Player
    {
        public string Name { get; set; }
    }
    public class PlayerAction
    {
    }
}
