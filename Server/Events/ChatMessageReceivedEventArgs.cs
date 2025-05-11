using System;

namespace GOILauncher.Multiplayer.Server.Events
{
    public class ChatMessageReceivedEventArgs : EventArgs
    {
        public ServerPlayer Player { get; set; }
        public string Message { get; set; }
    }
}
