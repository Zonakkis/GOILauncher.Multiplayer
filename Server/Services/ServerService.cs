using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Network;
using GOILauncher.Multiplayer.Server.Events;
using LiteNetLib;
using LiteNetLib.Utils;

namespace GOILauncher.Multiplayer.Server.Services
{
    public class ServerService : IServerService
    {
        private readonly INetworkServer _networkServer;
        private readonly IEventBus _eventBus;
        private readonly Dictionary<int, ServerPlayer> _players
            = new Dictionary<int, ServerPlayer>();

        public ServerService(INetworkServer networkServer,
            IPacketDispatcher dispatcher,
            IEventBus eventBus)
        {
            _networkServer = networkServer;
            _eventBus = eventBus;
            dispatcher.RegisterStruct<C2SClientHandShakePacket>(OnClientHandshake);
            dispatcher.RegisterStruct<C2SChatMessagePacket>(OnChatMessage);
        }

        public void Dispose()
        {
            _networkServer.Dispose();
        }

        public void Start(int port)
        {
            _networkServer.Start(port);
        }

        public void Stop()
        {
            _networkServer.Stop();
        }

        public void Poll()
        {
            _networkServer.Poll();
        }

        public void Broadcast(INetSerializable packet, Func<ServerPlayer, bool> predicate = null)
        {
            foreach (var player in _players.Values)
            {
                if (predicate != null && !predicate(player)) continue;
                _networkServer.Send(player.Id, packet, DeliveryMethod.ReliableUnordered);
            }
        }

        private void OnClientHandshake(C2SClientHandShakePacket packet, NetPeer peer)
        {
            var playerId = peer.Id;
            var playerName = packet.PlayerName;
            var platform = packet.Platform;
            _players[playerId] = new ServerPlayer 
            { Peer = peer, Name = playerName, Platform = platform };
            var playerJoinedPacket = new S2CPlayerJoinedPacket 
            { PlayerId = playerId, PlayerName = playerName, Platform = platform };
            // Notify existing players about the new player
            Broadcast(playerJoinedPacket, p => p.Id != playerId);
            _eventBus.Publish(
                new ClientHandshakeEvent(playerName, platform));
        }

        private void OnChatMessage(C2SChatMessagePacket packet, NetPeer peer)
        {
            var playerId = peer.Id;
            var chatPacket = new S2CChatMessagePacket
            {
                PlayerId = playerId,
                Message = packet.Message
            };
            Broadcast(chatPacket);
            _eventBus.Publish(new ChatMessageEvent(playerId, packet.Message));
        }

    }
}
