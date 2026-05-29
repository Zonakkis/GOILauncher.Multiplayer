using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Data.Models;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Server.Services
{
    public class PlayerService : IPlayerService
    {
        public Dictionary<int, ServerPlayer> Players { get; }
            = new Dictionary<int, ServerPlayer>();

        public ServerPlayer AddOrUpdate(ServerPlayer player)
        {
            Players[player.Id] = player;
            return player;
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

        public void Clear()
        {
            Players.Clear();
        }
    }
}
