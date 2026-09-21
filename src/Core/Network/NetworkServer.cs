using System;
using GOILauncher.Multiplayer.Core.Event;
using System.Collections.Generic;
using System.Linq;
using GOILauncher.Multiplayer.Core.Data.Constants;
using GOILauncher.Multiplayer.Core.Log;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Network
{
    public class NetworkServer : INetworkServer
    {
        public bool IsRunning => _netManager.IsRunning;
        private readonly NetManager _netManager;
        private readonly IServerEventBus _eventBus;
        private readonly ILogger<NetworkServer> _logger;

        public NetworkServer(
            NetManager netManager,
            IServerEventBus eventBus,
            ILogger<NetworkServer> logger)
        {
            _netManager = netManager;
            _eventBus = eventBus;
            _logger = logger;
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start(int port)
        {
            _netManager.Statistics.Reset();
            _netManager.Start(port);
            _logger.Info($"Server started on {port}.");
        }

        public void Stop()
        {
            // NetManager.Stop 对没在跑的实例是空操作，日志不该跟着空跑一趟：
            // 上次退出时联机是关闭状态的话，启动就会走一次 Stop，那时什么都没停下来。
            if (!_netManager.IsRunning)
                return;

            _netManager.Stop();
            _eventBus.Publish(new ServerStoppedEvent());
            _logger.Info("Server stopped.");
        }

        public void Poll()
        {
            _netManager.PollEvents();
        }

        public List<PeerTraffic> SamplePeerTraffic()
        {
            // ConnectedPeerList hands back NetManager's shared cache, so copy out the
            // counters immediately and never retain the list. Poll-thread only by contract.
            var traffic = new List<PeerTraffic>();
            foreach (var peer in _netManager.ConnectedPeerList)
            {
                var s = peer.Statistics;
                if (s == null) continue;
                traffic.Add(new PeerTraffic(peer.Id, s.PacketsSent, s.PacketsReceived,
                    s.BytesSent, s.BytesReceived, s.PacketLoss, s.PacketLossPercent));
            }
            return traffic;
        }

        public ServerTraffic SampleServerTraffic()
        {
            var statistics = _netManager.Statistics;
            return new ServerTraffic(
                statistics.PacketsSent,
                statistics.PacketsReceived,
                statistics.BytesSent,
                statistics.BytesReceived,
                statistics.PacketLoss,
                statistics.PacketLossPercent);
        }

        public void Send(int clientId, INetSerializable packet, DeliveryMethod method)
        {
            Send(clientId, packet, NetworkChannels.Default, method);
        }

        public void Send(int clientId, INetSerializable packet, byte channel, DeliveryMethod method)
        {
            var peer = _netManager.GetPeerById(clientId);
            if (peer == null) return;

            var bytes = NetSerializablePacketWriter.Write(packet);
            peer.Send(bytes, channel, method);
        }

        public void Multicast(IEnumerable<int> clientIds, INetSerializable packet, DeliveryMethod method)
        {
            Multicast(clientIds, packet, NetworkChannels.Default, method);
        }

        public void Multicast(IEnumerable<int> clientIds, INetSerializable packet, byte channel, DeliveryMethod method)
        {
            foreach (var clientId in clientIds)
            {
                try
                {
                    Send(clientId, packet, channel, method);
                }
                catch (Exception e)
                {
                    _logger.Error($"Failed to send packet to client {clientId}: {e.Message}");
                }
            }
        }

        public void Broadcast(INetSerializable packet, DeliveryMethod method)
        {
            var peerIds = _netManager.ConnectedPeerList.Select(peer => peer.Id);
            Multicast(peerIds, packet, method);
        }
    }
}
