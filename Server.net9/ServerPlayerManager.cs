using GOILauncher.Multiplayer.Server.Events;
using GOILauncher.Multiplayer.Server.Interfaces;

namespace GOILauncher.Multiplayer.Server
{
    internal class ServerPlayerManager
    {
        private IServerEventListener _serverEventListener;
        private int _idCounter = 1;
        public int GetNewPlayerId() => _idCounter++;
        public ServerPlayerManager(IServerEventListener serverEventListener)
        {
            _serverEventListener = serverEventListener;
            _serverEventListener.PlayerConnected += OnPlayerConnected;
        }

        private void OnPlayerConnected(object sender, PlayerConnectedEventArgs e)
        {
            //e.Player.Peer.
            //// Handle player connected event
            //Console.WriteLine($"Player connected: {e.PlayerId}");
        }
    }
}
