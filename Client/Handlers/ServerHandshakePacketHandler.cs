using GOILauncher.Multiplayer.Core.Data.Handlers;
using GOILauncher.Multiplayer.Core.Data.Packets.S2C;
using GOILauncher.Multiplayer.Core.Event;
namespace GOILauncher.Multiplayer.Client.Handlers
{
    public class ServerHandshakePacketHandler : IPacketHandler<ServerHandShakePacket>
    {
        private readonly IEventBus _eventBus;

        public ServerHandshakePacketHandler(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Handle(ServerHandShakePacket packet)
        {
            throw new System.NotImplementedException();
        }
    }
}
