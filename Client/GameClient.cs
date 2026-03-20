using System;
using GOILauncher.Multiplayer.Core.Handlers;
using GOILauncher.Multiplayer.Network;

namespace GOILauncher.Multiplayer.Client
{
    public class GameClient
    {
        private readonly INetworkClient _networkClient;
        private readonly IPacketDispatcher _dispatcher;

        public GameClient(INetworkClient networkClient, IPacketDispatcher dispatcher)
        {
            _networkClient = networkClient;
            _dispatcher = dispatcher;
            _networkClient.DataReceived += OnDataReceived;
        }

        public void Connect(string host, int port)
        {
            _networkClient.Connect(host, port);
        }

        public void OnDataReceived(ArraySegment<byte> data)
        {
            _dispatcher.Dispatch(data);
        }
    }
}
