using System.Linq;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Network;
using GOILauncher.Multiplayer.Server.Services;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Synchronization
{
    /// <summary>
    /// Relays visual player state only between peers that have completed the handshake
    /// and are currently in the Mian gameplay scene.
    /// </summary>
    public class PlayerStateRelay
    {
        private readonly INetworkServer _networkServer;
        private readonly IPlayerService _playerService;

        public PlayerStateRelay(INetworkServer networkServer,
            IPacketDispatcher dispatcher,
            IPlayerService playerService)
        {
            _networkServer = networkServer;
            _playerService = playerService;
            dispatcher.RegisterStruct<C2SPlayerStatePacket>(OnPlayerStateReceived);
        }

        private void OnPlayerStateReceived(C2SPlayerStatePacket packet, NetPeer peer)
        {
            PlayerInfo sender;
            if (peer == null || !_playerService.Players.TryGetValue(peer.Id, out sender))
            {
                return;
            }

            if (sender == null || !sender.IsInGame)
            {
                return;
            }

            var relayPacket = new S2CPlayerStatePacket
            {
                PlayerId = peer.Id,
                Sequence = packet.Sequence,
                State = packet.State
            };

            var recipients = _playerService.Players.Values
                .Where(player => player != null && player.Id != peer.Id && player.IsInGame)
                .Select(player => player.Id);
            _networkServer.Multicast(recipients, relayPacket, DeliveryMethod.Unreliable);
        }
    }
}
