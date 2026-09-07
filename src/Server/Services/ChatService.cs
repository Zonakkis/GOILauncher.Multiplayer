using Autofac;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Services
{
    public class ChatService : IStartable
    {
        private readonly IRoomService _rooms;
        private readonly INetworkServer _network;
        private readonly IServerPacketDispatcher _dispatcher;
        public ChatService(IRoomService rooms, INetworkServer network, IServerPacketDispatcher dispatcher)
        { _rooms = rooms; _network = network; _dispatcher = dispatcher; }
        void IStartable.Start() { _dispatcher.RegisterStruct<C2SChatMessagePacket>(OnChatMessage); }
        private void OnChatMessage(C2SChatMessagePacket packet, PacketSender sender)
        {
            if (!_rooms.IsCurrentMembership(sender.Id, packet.MembershipId)) return;
            foreach (var recipient in _rooms.GetMembers(sender.Id))
            {
                if (recipient.PlayerId == sender.Id) continue;
                _network.Send(recipient.PlayerId, new S2CChatMessagePacket
                {
                    Scope = new RoomPacketScope(recipient.Id, packet.MembershipId),
                    PlayerId = sender.Id, Content = packet.Content, Timestamp = packet.Timestamp
                }, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}
