using System;

namespace GOILauncher.Multiplayer.Core.Event
{
    public interface IEventBus
    {
        void Publish<TEvent>(TEvent @event);
        IDisposable Subscribe<TEvent>(Action<TEvent> handler);
    }
}
