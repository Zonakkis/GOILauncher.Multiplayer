using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Server.Events;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IServerService _serverService;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PlayerService> _logger;
        public Dictionary<int, ServerPlayer> Players { get; }
            = new Dictionary<int, ServerPlayer>();

        public PlayerService(IServerService serverService,
            IPacketDispatcher dispatcher,
            IEventBus eventBus,
            ILogger<PlayerService> logger)
        {
            _serverService = serverService;
            _eventBus = eventBus;
            _logger = logger;
            dispatcher.RegisterStruct<C2SClientHandShakePacket>(OnClientHandshake);
        }

        public bool TryGet(int playerId, out ServerPlayer player)
        {
            return Players.TryGetValue(playerId, out player);
        }

        public bool TryRemove(int playerId, out ServerPlayer player)
        {
            if (!Players.TryGetValue(playerId, out player))
                return false;

            Players.Remove(playerId);
            return true;
        }

        private void OnClientHandshake(C2SClientHandShakePacket packet, NetPeer peer)
        {
            var playerId = peer.Id;
            var playerName = packet.PlayerName;
            var platform = packet.Platform;
            var player = new ServerPlayer
            {
                Peer = peer,
                Name = playerName,
                Platform = platform
            };
            Players[player.Id] = player;
            var playerJoinedPacket = new S2CPlayerJoinedPacket
            { PlayerId = playerId, PlayerName = playerName, Platform = platform };
            // Notify existing players about the new player
            _serverService.Broadcast(playerJoinedPacket, p => p.Id != playerId);
            _eventBus.Publish(new ClientHandshakeEvent(playerName, platform));
        }
    }
}
