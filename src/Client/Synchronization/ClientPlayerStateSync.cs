using System.Collections.Generic;
using GOILauncher.Multiplayer.Client.Events;
using GOILauncher.Multiplayer.Core.Data;
using GOILauncher.Multiplayer.Core.Data.Models;
using GOILauncher.Multiplayer.Core.Data.Packets;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Utils;
using GOILauncher.Multiplayer.Network;
using LiteNetLib;

namespace GOILauncher.Multiplayer.Client.Synchronization
{
    /// <summary>
    /// Owns player-state packet sequencing and receive ordering for one client connection.
    /// The module is persistent across gameplay scene transitions; the outgoing counter is
    /// reset only when the server connection is lost.
    /// </summary>
    public class ClientPlayerStateSync
    {
        private readonly INetworkClient _networkClient;
        private readonly IEventBus _eventBus;
        private readonly Dictionary<int, uint> _lastReceivedSequences = new Dictionary<int, uint>();
        private uint _nextSequence;

        public bool IsConnected => _networkClient.IsConnected;

        public ClientPlayerStateSync(INetworkClient networkClient,
            IPacketDispatcher dispatcher,
            IEventBus eventBus)
        {
            _networkClient = networkClient;
            _eventBus = eventBus;
            dispatcher.RegisterStruct<S2CPlayerStatePacket>(OnPlayerStateReceived);
            eventBus.Subscribe<ServerDisconnectedEvent>(OnServerDisconnected);
            eventBus.Subscribe<PlayerLeftEvent>(OnPlayerLeft);
        }

        public void Send(PlayerState state)
        {
            if (!_networkClient.IsConnected)
            {
                return;
            }

            var packet = new C2SPlayerStatePacket
            {
                Sequence = _nextSequence,
                State = state
            };
            _nextSequence = unchecked(_nextSequence + 1);
            _networkClient.Send(packet, DeliveryMethod.Unreliable);
        }

        private void OnPlayerStateReceived(S2CPlayerStatePacket packet, NetPeer _)
        {
            uint lastSequence;
            if (_lastReceivedSequences.TryGetValue(packet.PlayerId, out lastSequence) &&
                !SequenceNumber.IsNewer(packet.Sequence, lastSequence))
            {
                return;
            }

            _lastReceivedSequences[packet.PlayerId] = packet.Sequence;
            _eventBus.Publish(new PlayerStateReceivedEvent(packet.PlayerId, packet.State));
        }

        private void OnPlayerLeft(PlayerLeftEvent e)
        {
            _lastReceivedSequences.Remove(e.PlayerId);
        }

        private void OnServerDisconnected(ServerDisconnectedEvent e)
        {
            _nextSequence = 0;
            _lastReceivedSequences.Clear();
        }
    }
}
