using System;
using System.Collections.Generic;
using System.Linq;
using GOILauncher.Multiplayer.Shared.Constants;
using GOILauncher.Multiplayer.Shared.Events;
using GOILauncher.Multiplayer.Shared.Extensions;
using GOILauncher.Multiplayer.Shared.Packets;
using GOILauncher.Multiplayer.Shared.Utilities;
using LiteNetLib;
using LiteNetLib.Layers;
using LiteNetLib.Utils;
using NLog;

namespace GOILauncher.Multiplayer.Shared.Net
{
    public class NetServer : NetManager, INetServer
    {
        public EventBasedNetListener Listener { get; }
        public long BytesSent => Statistics.BytesSent;
        public long BytesReceived => Statistics.BytesReceived;
        public long PacketLossPercentage => Statistics.PacketLossPercent;
        public event EventHandler<ClientConnectedEventArgs> ClientConnected;
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
        private readonly Dictionary<int, NetPeer> _clients = new Dictionary<int, NetPeer>();
        private readonly IdGenerator _idGenerator = new IdGenerator(1);
        public NetServer()
            : this(new EventBasedNetListener())
        {
            EnableStatistics = true;
            DisconnectTimeout = ConnectionConstants.Timeout;
            UpdateTime = ConnectionConstants.ServerUpdateTime;
        }

        private NetServer(EventBasedNetListener listener, PacketLayerBase extraPacketLayer = null)
            : base(listener, extraPacketLayer)
        {
            Listener = listener;
            listener.ConnectionRequestEvent += OnConnectionRequested;
            listener.PeerConnectedEvent += OnClientConnected;
            listener.PeerDisconnectedEvent += OnClientDisconnected;
        }


        void INetServer.Start(int port)
        {
            if (IsRunning)
                return;
            Start(port);
        }

        public void Poll()
        {
            if (IsRunning)
                PollEvents();
        }

        public void SendTo(int clientId, IPacket packet, DeliveryMethod deliveryMethod)
        {
            NetDataWriter writer = new NetPacketWriter(packet);
            if (_clients.TryGetValue(clientId, out var peer))
                peer.Send(writer, deliveryMethod);
        }

        public void SendToAll(IPacket packet, DeliveryMethod deliveryMethod)
        {
            NetDataWriter writer = new NetPacketWriter(packet);
            foreach (var client in ConnectedPeerList)
                client.Send(writer, deliveryMethod);
        }

        public void SendToAllExcept(int exceptClientId, IPacket packet, DeliveryMethod deliveryMethod)
        {
            NetDataWriter writer = new NetPacketWriter(packet);
            foreach (var client in _clients.Where(client => client.Key != exceptClientId))
                client.Value.Send(writer, deliveryMethod);
        }
        private void OnConnectionRequested(ConnectionRequest request)
        {
            if (ConnectedPeersCount < ConnectionConstants.MaxPlayers)
                request.AcceptIfKey(ConnectionConstants.Key);
            else
                request.Reject();
        }

        private void OnClientConnected(NetPeer peer)
        {
            var id = _idGenerator.Generate();
            _clients.Add(id, peer);
            ClientConnected?.Invoke(this, new ClientConnectedEventArgs
            {
                ClientId = id,
                Peer = peer
            });
        }
        private void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectinfo)
        {
            var clientId = _clients.First(c => c.Value == peer).Key;
            if (_clients.Remove(clientId))
            {
                ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs
                {
                    ClientId = clientId,
                    Reason = disconnectinfo.Reason.GetDescription()
                });
            }

        }
    }
}
