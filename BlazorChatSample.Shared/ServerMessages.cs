namespace BlazorChatSample.Shared
{
    /// <summary>
    /// Stores shared names used in both client and hub
    /// </summary>
    public static class ServerMessages
    {
        public const string RECEIVE = "ReceiveMessage";

        public const string JOINROOM = "JoinRoom";

        public const string SEND = "SendMessage";

        public const string PLAYERACTION = "PlayerAction";

        public const string GETGROUPS = "GetGroups";

    }
}
