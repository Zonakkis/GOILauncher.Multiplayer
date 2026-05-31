using System;
using System.Collections.Generic;
using System.Linq;
using GOILauncher.Multiplayer.Core.Log;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Network
{
    public class NetworkServer : INetworkServer
    {
        public bool IsRunning => _netManager.IsRunning;
        private readonly NetManager _netManager;
        private readonly ILogger<NetworkServer> _logger;

        public NetworkServer(
            NetManager netManager,
            ILogger<NetworkServer> logger)
        {
            _netManager = netManager;
            _logger = logger;
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start(int port)
        {
            _netManager.Start(port);
            _logger.Info($"Server started on {port}.");
        }

        public void Stop()
        {
            _netManager.Stop();
            _logger.Info("Server stopped.");
        }

        public void Poll()
        {
            _netManager.PollEvents();
        }

        public void Send(int clientId, INetSerializable packet, DeliveryMethod method)
        {
            var peer = _netManager.GetPeerById(clientId);
            if (peer == null) return;

            var bytes = NetSerializablePacketWriter.Write(packet);
            peer.Send(bytes, method);
        }

        public void Multicast(IEnumerable<int> clientIds, INetSerializable packet, DeliveryMethod method)
        {
            foreach (var clientId in clientIds)
            {
                try
                {
                    Send(clientId, packet, method);
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
