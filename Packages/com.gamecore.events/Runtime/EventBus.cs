using System;
using System.Collections.Generic;

namespace GameCore.Events
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> _handlers = new();

        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var eventType = typeof(T);
            if (_handlers.TryGetValue(eventType, out var existingDelegate))
                _handlers[eventType] = Delegate.Combine(existingDelegate, handler);
            else
                _handlers[eventType] = handler;
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var eventType = typeof(T);
            if (_handlers.TryGetValue(eventType, out var existingDelegate))
            {
                var updatedDelegate = Delegate.Remove(existingDelegate, handler);
                if (updatedDelegate == null) _handlers.Remove(eventType);
                else _handlers[eventType] = updatedDelegate;
            }
        }

        public static void Publish<T>(T gameEvent) where T : IGameEvent
        {
            if (_handlers.TryGetValue(typeof(T), out var handlerDelegate))
            {
                var typedHandler = (Action<T>)handlerDelegate;
                foreach (var invocationTarget in typedHandler.GetInvocationList())
                {
                    try { ((Action<T>)invocationTarget)(gameEvent); }
                    catch (Exception exception) { UnityEngine.Debug.LogException(exception); }
                }
            }
        }

        public static void Clear() => _handlers.Clear();
    }
}
