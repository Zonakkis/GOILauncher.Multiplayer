using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Network
{
    public interface INetworkServer : IDisposable
    {
        bool IsRunning { get; }
        void Start(int port);
        void Stop();
        void Poll();
        void Send(int clientId, INetSerializable packet, DeliveryMethod method);
        void Multicast(IEnumerable<int> clientIds, INetSerializable packet, DeliveryMethod method);
        void Broadcast(INetSerializable packet, DeliveryMethod method);
        
        /// <summary>
        /// 指定 LiteNetLib 通道发送。通道号见
        /// <see cref="Core.Data.Constants.NetworkChannels"/>。
        /// </summary>
        void Send(int clientId, INetSerializable packet, byte channel, DeliveryMethod method);

        void Multicast(IEnumerable<int> clientIds, INetSerializable packet, byte channel, DeliveryMethod method);

        /// <summary>
        /// Snapshot of per-peer traffic counters. Call on the Poll thread: the underlying
        /// peer list is LiteNetLib's shared internal cache and must not be held across threads.
        /// Empty unless statistics collection is enabled on the NetManager.
        /// Returns List rather than IReadOnlyList because Core also builds against net35.
        /// </summary>
        List<PeerTraffic> SamplePeerTraffic();
    }
}
