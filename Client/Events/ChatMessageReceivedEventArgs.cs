using System;

namespace GOILauncher.Multiplayer.Client.Events
{
    public class ChatMessageReceivedEventArgs : EventArgs
    {
        public ClientPlayer Player { get; set; }
        public string Message { get; set; }
    }
}
