using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Client.Models;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using LiteNetLib;
using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Client.Services
{
    public class PlayerService
    {
        private readonly IEventBus _eventBus;
        private Dictionary<int, ClientPlayer> Players { get; } 
            = new Dictionary<int, ClientPlayer>();
        public PlayerService(IPacketDispatcher dispatcher,
            IEventBus eventBus,
            IClientService clientService)
        {
            _eventBus = eventBus;
            dispatcher.RegisterStruct<S2CPlayerJoinedPacket>(OnPlayerJoined);
            // Set local player when handshake is successful
            eventBus.Subscribe<ServerHandshakeEvent>(
                e => Players[e.PlayerId] = clientService.LocalPlayer);
        }

        public void OnPlayerJoined(S2CPlayerJoinedPacket packet, NetPeer _)
        {
            var playerId = packet.PlayerId;
            var playerName = packet.PlayerName;
            var platform = packet.Platform;
            Players[playerId] = new ClientPlayer 
            { Id = playerId, Name = playerName, Platform = platform };
            _eventBus.Publish(
                new PlayerJoinedEvent(playerId, playerName, platform));
        }
    }
}
