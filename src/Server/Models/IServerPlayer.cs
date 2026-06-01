using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server
{
    public interface IServerPlayer
    {
        NetPeer Peer { get; }
        PlayerInfo Info { get; }
    }
}