using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Log;

namespace GOILauncher.Multiplayer.Core.Event
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers
            = new Dictionary<Type, List<Delegate>>();

        private readonly object _lock = new object();
        private readonly ILogger<EventBus> _logger;

        public EventBus(ILogger<EventBus> logger)
        {
            _logger = logger;
        }

        public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            var eventType = typeof(TEvent);
            lock (_lock)
            {
                if (!_subscribers.ContainsKey(eventType))
                {
                    _subscribers[eventType] = new List<Delegate>();
                }
                _subscribers[eventType].Add(handler);
            }

            return new Subscription(() => Unsubscribe(eventType, handler));
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
                        try
                        {
                            eventHandler(@event);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Error handling event {eventType.Name}", ex);
                        }
                    }
                }
            }
        }

        private void Unsubscribe(Type eventType, Delegate handler)
        {
            lock (_lock)
            {
                List<Delegate> handlers;
                if (!_subscribers.TryGetValue(eventType, out handlers))
                    return;

                handlers.Remove(handler);

                if (handlers.Count == 0)
                    _subscribers.Remove(eventType);
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly object _lock = new object();
            private Action _unsubscribe;

            public Subscription(Action unsubscribe)
            {
                _unsubscribe = unsubscribe;
            }

            public void Dispose()
            {
                Action unsubscribe;
                lock (_lock)
                {
                    unsubscribe = _unsubscribe;
                    _unsubscribe = null;
                }

                if (unsubscribe != null)
                    unsubscribe();
            }
        }
    }
}
