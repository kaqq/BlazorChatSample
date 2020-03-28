using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorChatSample.Shared
{
    public class GameRoom
    {
        public string Name { get; set; }
        public string GuestCount { get; set; }
    }

    public class Player
    {
        public string Name { get; set; }
    }
    public class PlayerAction
    {
    }
}
