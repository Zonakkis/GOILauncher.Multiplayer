using Autofac;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Network;
using GOILauncher.Multiplayer.Server.Services;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Synchronization
{
    public class PlayerStateRelay : IStartable
    {
        private readonly INetworkServer _network;
        private readonly IServerPacketDispatcher _dispatcher;
        private readonly IPlayerService _players;
        private readonly IRoomService _rooms;
        public PlayerStateRelay(INetworkServer network, IServerPacketDispatcher dispatcher, IPlayerService players, IRoomService rooms)
        { _network = network; _dispatcher = dispatcher; _players = players; _rooms = rooms; }
        void IStartable.Start() { _dispatcher.RegisterStruct<C2SPlayerStatePacket>(OnState); }
        private void OnState(C2SPlayerStatePacket packet, PacketSender sender)
        {
            PlayerInfo player;
            if (!_rooms.IsCurrentMembership(sender.Id, packet.MembershipId)
                || !_players.Players.TryGetValue(sender.Id, out player) || !player.IsInGame) return;
            foreach (var peer in _rooms.Peers(sender.Id))
            {
                if (!_players.Players.TryGetValue(peer.PlayerId, out player) || !player.IsInGame) continue;
                _network.Send(peer.PlayerId, new S2CPlayerStatePacket
                {
                    Scope = peer.Scope, PlayerId = sender.Id,
                    Sequence = packet.Sequence, State = packet.State
                }, DeliveryMethod.Unreliable);
            }
        }
    }
}
