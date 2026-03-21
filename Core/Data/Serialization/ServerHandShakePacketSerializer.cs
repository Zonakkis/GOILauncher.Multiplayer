
using System;
using System.IO;
using GOILauncher.Multiplayer.Core.Data.Serialization;

namespace GOILauncher.Multiplayer.Core.Data.Packets.S2C
{
    public class ServerHandShakePacketSerializer : BasePacketSerializer<ServerHandShakePacket>
    {
        private readonly ArraySegmentReader _reader;

        public ServerHandShakePacketSerializer(ArraySegmentReader reader)
        {
            _reader = reader;
        }

        protected override void Write(BinaryWriter binaryWriter, ServerHandShakePacket packet)
        {
            binaryWriter.Write(packet.PlayerId);
        }

        public override ServerHandShakePacket Deserialize(ArraySegment<byte> data)
        {
            _reader.Begin(data);
            int playerId = _reader.ReadInt();
            _reader.End();
            return new ServerHandShakePacket
            {
                PlayerId = playerId
            };
        }
    }
}