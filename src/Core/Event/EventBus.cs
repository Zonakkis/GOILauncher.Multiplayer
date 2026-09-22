using System;
using System.Collections.Generic;
using GOILauncher.Multiplayer.Core.Log;

namespace GOILauncher.Multiplayer.Core.Event
{
    public class EventBus : IEventBus
    {
        // Copy-on-write: each event type maps to an immutable handler array. Subscribe/Unsubscribe
        // swap in a new array under the lock; Publish reads the current one and iterates it lock-free.
        private readonly Dictionary<Type, Delegate[]> _subscribers
            = new Dictionary<Type, Delegate[]>();

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
                Delegate[] handlers;
                if (_subscribers.TryGetValue(eventType, out handlers))
                {
                    var updated = new Delegate[handlers.Length + 1];
                    Array.Copy(handlers, updated, handlers.Length);
                    updated[handlers.Length] = handler;
                    _subscribers[eventType] = updated;
                }
                else
                {
                    _subscribers[eventType] = new Delegate[] { handler };
                }
            }

            return new Subscription(() => Unsubscribe(eventType, handler));
        }

        public void Publish<TEvent>(TEvent @event)
        {
            var eventType = typeof(TEvent);
            Delegate[] handlers;
            lock (_lock)
            {
                if (!_subscribers.TryGetValue(eventType, out handlers))
                    return;
            }

            // handlers is an immutable snapshot: Subscribe/Unsubscribe replace the array rather than
            // mutate it, so iterating here without the lock stays safe even if a handler (un)subscribes
            // mid-publish. Every delegate stored under typeof(TEvent) is an Action<TEvent>, so the cast
            // always succeeds; a failure would surface through the logger rather than be swallowed.
            foreach (var handler in handlers)
            {
                try
                {
                    ((Action<TEvent>)handler)(@event);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error handling event {eventType.Name}");
                }
            }
        }

        private void Unsubscribe(Type eventType, Delegate handler)
        {
            lock (_lock)
            {
                Delegate[] handlers;
                if (!_subscribers.TryGetValue(eventType, out handlers))
                    return;

                // Remove the first matching subscription, mirroring List.Remove. Swap in a new array
                // so any Publish already iterating the old one is unaffected.
                var index = Array.IndexOf(handlers, handler);
                if (index < 0)
                    return;

                if (handlers.Length == 1)
                {
                    _subscribers.Remove(eventType);
                    return;
                }

                var updated = new Delegate[handlers.Length - 1];
                Array.Copy(handlers, 0, updated, 0, index);
                Array.Copy(handlers, index + 1, updated, index, handlers.Length - index - 1);
                _subscribers[eventType] = updated;
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
