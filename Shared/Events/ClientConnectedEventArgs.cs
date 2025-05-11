using System;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Shared.Events
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public NetPeer Peer { get; set; }
        public int ClientId { get; set; }
    }
}