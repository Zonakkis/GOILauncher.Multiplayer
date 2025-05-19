using System;
using System.Net;
using System.Net.Sockets;
using GOILauncher.Multiplayer.Shared.Constants;
using GOILauncher.Multiplayer.Shared.Events;
using GOILauncher.Multiplayer.Shared.Extensions;
using GOILauncher.Multiplayer.Shared.Packets;
using LiteNetLib;
using LiteNetLib.Layers;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Shared.Net
{
    public class NetClient : NetManager, INetClient
    {
        public event EventHandler<ServerDisconnectedEventArgs> ServerDisconnected;
        public EventBasedNetListener Listener { get; }

        private NetPeer _server;
        public long BytesSent => _server?.Statistics.BytesSent ?? 0;
        public long BytesReceived => _server?.Statistics.BytesReceived ?? 0;
        public long PacketLossPercentage => _server?.Statistics.PacketLossPercent ?? 0;
        public int Ping => _server?.Ping ?? 0;

        public NetClient()
            : this(new EventBasedNetListener())
        {
            EnableStatistics = true;
            DisconnectTimeout = ConnectionConstants.Timeout;
            UpdateTime = ConnectionConstants.ClientUpdateTime;
        }

        private NetClient(EventBasedNetListener listener, PacketLayerBase extraPacketLayer = null)
            : base(listener, extraPacketLayer)
        {
            Listener = listener;
            listener.NetworkErrorEvent += OnNetworkError;
            listener.PeerConnectedEvent += OnServerConnected;
            listener.PeerDisconnectedEvent += OnServerDisconnected;
        }


        public void Start(string host, int port)
        {
            if (IsRunning)
                return;
            Start();
            Connect(host, port, ConnectionConstants.Key);
        }

        public void Poll()
        {
            if (IsRunning)
                PollEvents();
        }

        public void Send(IPacket packet, DeliveryMethod deliveryMethod)
        {
            NetDataWriter writer = new NetPacketWriter(packet);
            _server?.Send(writer, deliveryMethod);
        }
        private void OnNetworkError(IPEndPoint endpoint, SocketError socketerror)
        {
            //_logger?.Error($"网络错误：{socketerror}，地址：{endpoint.Address}:{endpoint.Port}");
        }

        private void OnServerConnected(NetPeer peer)
        {
            _server = peer;
            //var iPEndPoint = peer.EndPoint;
            //_logger?.Info($"成功连接到服务器：{iPEndPoint.Address}:{iPEndPoint.Port}");
        }

        private void OnServerDisconnected(NetPeer peer, DisconnectInfo disconnectinfo)
        {
            _server = null;
            ServerDisconnected?.Invoke(this, new ServerDisconnectedEventArgs
            {
                Reason = disconnectinfo.Reason.GetDescription()
            });
        }
    }
}
