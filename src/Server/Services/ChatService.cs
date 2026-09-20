using Autofac;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Utils;
using GOILauncher.Multiplayer.Network;
using GOILauncher.Multiplayer.Server.Events;
using LiteNetLib;
using System;

namespace GOILauncher.Multiplayer.Server.Services
{
    public class ChatService : IStartable
    {
        private readonly IRoomService _rooms;
        private readonly IPlayerService _players;
        private readonly INetworkServer _network;
        private readonly IServerPacketDispatcher _dispatcher;
        private readonly IServerEventBus _events;
        public ChatService(IRoomService rooms, IPlayerService players, INetworkServer network,
            IServerPacketDispatcher dispatcher, IServerEventBus events)
        { _rooms = rooms; _players = players; _network = network; _dispatcher = dispatcher; _events = events; }
        void IStartable.Start() { _dispatcher.RegisterStruct<C2SChatMessagePacket>(OnChatMessage); }
        private void OnChatMessage(C2SChatMessagePacket packet, PacketSender sender)
        {
            RoomMembership membership;
            if (!_rooms.TryGetMembership(sender.Id, out membership)) return;
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
            // Server-side copy for the observation facade: the room the message belongs to
            // is known right here, and the server's clock is the only trustworthy one.
            PlayerInfo player;
            var name = _players.Players.TryGetValue(sender.Id, out player) ? player.Name : "?";
            _events.Publish(new ChatRelayedEvent(membership.RoomId, sender.Id, name,
                packet.Content, DateTimeUtils.ToUnixTimeSeconds(DateTime.Now)));
        }
    }
}
