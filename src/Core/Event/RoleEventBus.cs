using GOILauncher.Multiplayer.Core.Log;

namespace GOILauncher.Multiplayer.Core.Event
{
    public interface IClientEventBus : IEventBus { }
    public interface IServerEventBus : IEventBus { }

    public sealed class ClientEventBus : EventBus, IClientEventBus
    {
        public ClientEventBus(ILogger<EventBus> logger) : base(logger) { }
    }

    public sealed class ServerEventBus : EventBus, IServerEventBus
    {
        public ServerEventBus(ILogger<EventBus> logger) : base(logger) { }
    }

    public struct ServerStoppedEvent { }
}
