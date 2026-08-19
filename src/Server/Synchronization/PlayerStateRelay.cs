using System.Linq;
using Autofac;
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
    public class PlayerStateRelay : IStartable
    {
        private readonly INetworkServer _networkServer;
        private readonly IServerPacketDispatcher _dispatcher;
        private readonly IPlayerService _playerService;

        public PlayerStateRelay(INetworkServer networkServer,
            IServerPacketDispatcher dispatcher,
            IPlayerService playerService)
        {
            _networkServer = networkServer;
            _dispatcher = dispatcher;
            _playerService = playerService;
        }

        void IStartable.Start()
        {
            _dispatcher.RegisterStruct<C2SPlayerStatePacket>(OnPlayerStateReceived);
        }

        private void OnPlayerStateReceived(C2SPlayerStatePacket packet, PacketSender sender)
        {
            PlayerInfo senderInfo;
            if (!_playerService.Players.TryGetValue(sender.Id, out senderInfo))
            {
                return;
            }

            if (senderInfo == null || !senderInfo.IsInGame)
            {
                return;
            }

            var relayPacket = new S2CPlayerStatePacket
            {
                PlayerId = sender.Id,
                Sequence = packet.Sequence,
                State = packet.State
            };

            var recipients = _playerService.Players.Values
                .Where(player => player != null && player.Id != sender.Id && player.IsInGame)
                .Select(player => player.Id);
            _networkServer.Multicast(recipients, relayPacket, DeliveryMethod.Unreliable);
        }
    }
}
