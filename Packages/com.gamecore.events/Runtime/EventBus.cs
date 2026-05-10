using System;
using System.Collections.Generic;

namespace GameCore.Events
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> _handlers = new();

        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
                _handlers[type] = Delegate.Combine(existing, handler);
            else
                _handlers[type] = handler;
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                var updated = Delegate.Remove(existing, handler);
                if (updated == null) _handlers.Remove(type);
                else _handlers[type] = updated;
            }
        }

        public static void Publish<T>(T evt) where T : IGameEvent
        {
            if (_handlers.TryGetValue(typeof(T), out var del))
            {
                var handler = (Action<T>)del;
                foreach (var d in handler.GetInvocationList())
                {
                    try { ((Action<T>)d)(evt); }
                    catch (Exception e) { UnityEngine.Debug.LogException(e); }
                }
            }
        }

        public static void Clear() => _handlers.Clear();
    }
}
