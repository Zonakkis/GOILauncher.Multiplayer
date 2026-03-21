using System;
using System.Collections.Generic;

namespace GOILauncher.Multiplayer.Core.Event
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers
            = new Dictionary<Type, List<Delegate>>();

        private readonly object _lock = new object();

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof(TEvent);
            lock (_lock)
            {
                if (!_subscribers.ContainsKey(eventType))
                {
                    _subscribers[eventType] = new List<Delegate>();
                }
                _subscribers[eventType].Add(handler);
            }
        }

        public void Publish<TEvent>(TEvent @event)
        {
            var eventType = typeof(TEvent);
            List<Delegate> handlers = null;
            lock (_lock)
            {
                if(_subscribers.ContainsKey(eventType))
                {
                    // Create a copy of the handlers to avoid issues if handlers are added/removed during invocation
                    handlers = new List<Delegate>(_subscribers[eventType]);
                }
            }
            if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    if (handler is Action<TEvent> eventHandler)
                    {
                        eventHandler(@event);
                    }
                }
            }
        }
    }
}
