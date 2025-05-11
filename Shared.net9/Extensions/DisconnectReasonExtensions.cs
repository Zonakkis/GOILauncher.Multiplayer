using LiteNetLib;
using System;

namespace GOILauncher.Multiplayer.Shared.Extensions
{
    public static class DisconnectReasonExtensions
    {
        public static string GetDescription(this DisconnectReason disconnectReason)
        {
            switch (disconnectReason)
            {
                case DisconnectReason.ConnectionFailed:
                    return "连接失败。";
                case DisconnectReason.Timeout:
                    return "连接超时。";
                case DisconnectReason.HostUnreachable:
                    return "主机不可到达。";
                case DisconnectReason.NetworkUnreachable:
                    return "网络不可到达。";
                case DisconnectReason.RemoteConnectionClose:
                    return "远程连接被关闭。";
                case DisconnectReason.DisconnectPeerCalled:
                    return "对等方关闭连接。";
                case DisconnectReason.ConnectionRejected:
                    return "连接被拒绝。";
                case DisconnectReason.InvalidProtocol:
                    return "无效的协议。";
                case DisconnectReason.UnknownHost:
                    return "未知主机。";
                case DisconnectReason.Reconnect:
                    return "重新连接。";
                case DisconnectReason.PeerToPeerConnection:
                    return "点对点连接。";
                default:
                    throw new ArgumentOutOfRangeException(nameof(disconnectReason), disconnectReason, null);
            }
        }
    }
}
