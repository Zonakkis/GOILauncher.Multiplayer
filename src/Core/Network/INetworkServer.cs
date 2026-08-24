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
    }
}
