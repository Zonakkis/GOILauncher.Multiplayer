using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Shared.Packets
{
    public class PlayerDisconnectedPacket : IPacket
    {
        public PacketType PacketType => PacketType.PlayerDisconnected;
        public int PlayerId { get; set; }


        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
        }
    }
}
