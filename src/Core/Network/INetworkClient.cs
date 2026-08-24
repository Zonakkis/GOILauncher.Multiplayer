using LiteNetLib;
using LiteNetLib.Utils;
using System;

namespace GOILauncher.Multiplayer.Network
{
    public interface INetworkClient : IDisposable
    {
        bool IsConnected { get; }
        void Connect(string host, int port);
        void Disconnect();
        void Poll();
        void Send(INetSerializable packet, DeliveryMethod method);

        /// <summary>
        /// 指定 LiteNetLib 通道发送。通道号见
        /// <see cref="Core.Data.Constants.NetworkChannels"/>。
        /// </summary>
        void Send(INetSerializable packet, byte channel, DeliveryMethod method);
    }
}
