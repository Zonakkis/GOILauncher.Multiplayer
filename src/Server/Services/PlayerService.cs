using System.Collections.Generic;
using Autofac;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Server.Events;

namespace GOILauncher.Multiplayer.Server.Services
{
    /// <summary>Connection-wide identity and scene facts; RoomService owns membership and roster routing.</summary>
    public class PlayerService : IPlayerService, IStartable
    {
        private readonly IServerPacketDispatcher _dispatcher;
        private readonly IServerEventBus _events;
        public Dictionary<int, PlayerInfo> Players { get; } = new Dictionary<int, PlayerInfo>();
        public PlayerService(IServerPacketDispatcher dispatcher, IServerEventBus events)
        { _dispatcher = dispatcher; _events = events; }
        void IStartable.Start()
        {
            _dispatcher.RegisterStruct<C2SClientHandShakePacket>(OnClientHandshake);
            _dispatcher.RegisterStruct<C2SIsInGameUpdatePacket>(OnIsInGameUpdate);
            _events.Subscribe<ClientDisconnectedEvent>(e => Players.Remove(e.ClientId));
            _events.Subscribe<ServerStoppedEvent>(e => Players.Clear());
        }
        private void OnClientHandshake(C2SClientHandShakePacket packet, PacketSender sender)
        {
            // A second handshake must not rename/reinitialize an already authenticated connection.
            if (Players.ContainsKey(sender.Id)) return;
            var player = new PlayerInfo(sender.Id, packet.PlayerName, packet.Platform, packet.IsInGame);
            Players.Add(sender.Id, player);
            _events.Publish(new ClientHandshakeEvent(player.Id, player.Name, player.Platform));
        }
        private void OnIsInGameUpdate(C2SIsInGameUpdatePacket packet, PacketSender sender)
        {
            PlayerInfo player;
            if (!Players.TryGetValue(sender.Id, out player) || player.IsInGame == packet.IsInGame) return;
            player = player.WithIsInGame(packet.IsInGame);
            Players[sender.Id] = player;
            _events.Publish(new PlayerStatusChangedEvent(player));
        }
    }
}
