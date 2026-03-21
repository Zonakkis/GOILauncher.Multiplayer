using System;

namespace GOILauncher.Multiplayer.Core.Event
{
    public interface IEventBus
    {
        void Publish<TEvent>(TEvent @event);
        void Subscribe<TEvent>(Action<TEvent> handler);
    }
}