using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central event bus for decoupled communication between systems.
/// Uses a type-safe generic approach for event publishing and subscription.
/// 
/// Usage:
/// - Subscribe: EventBus.Subscribe&lt;MyEvent&gt;(OnMyEvent);
/// - Publish: EventBus.Publish(new MyEvent());
/// - Unsubscribe: EventBus.Unsubscribe&lt;MyEvent&gt;(OnMyEvent);
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<Type, List<object>> subscribers = new Dictionary<Type, List<object>>();
    private static bool enableDebugLogging = false;

    /// <summary>
    /// Enable or disable debug logging for event publishing.
    /// </summary>
    public static bool EnableDebugLogging
    {
        get => enableDebugLogging;
        set => enableDebugLogging = value;
    }

    /// <summary>
    /// Subscribe to events of type T.
    /// </summary>
    /// <typeparam name="T">Event type that implements IEvent</typeparam>
    /// <param name="handler">Action to call when event is published</param>
    public static void Subscribe<T>(Action<T> handler) where T : IEvent
    {
        if (handler == null)
        {
            Debug.LogWarning("EventBus.Subscribe: Handler is null, subscription ignored.");
            return;
        }

        Type eventType = typeof(T);
        
        if (!subscribers.ContainsKey(eventType))
        {
            subscribers[eventType] = new List<object>();
        }

        // Check if already subscribed to avoid duplicates
        if (!subscribers[eventType].Contains(handler))
        {
            subscribers[eventType].Add(handler);
            
            if (enableDebugLogging)
            {
                Debug.Log($"EventBus: Subscribed to {eventType.Name}");
            }
        }
        else
        {
            Debug.LogWarning($"EventBus: Handler already subscribed to {eventType.Name}");
        }
    }

    /// <summary>
    /// Unsubscribe from events of type T.
    /// </summary>
    /// <typeparam name="T">Event type that implements IEvent</typeparam>
    /// <param name="handler">Action to remove from subscribers</param>
    public static void Unsubscribe<T>(Action<T> handler) where T : IEvent
    {
        if (handler == null)
        {
            return;
        }

        Type eventType = typeof(T);
        
        if (subscribers.ContainsKey(eventType))
        {
            subscribers[eventType].Remove(handler);
            
            if (enableDebugLogging)
            {
                Debug.Log($"EventBus: Unsubscribed from {eventType.Name}");
            }
        }
    }

    /// <summary>
    /// Publish an event to all subscribers.
    /// </summary>
    /// <typeparam name="T">Event type that implements IEvent</typeparam>
    /// <param name="eventData">Event instance to publish</param>
    public static void Publish<T>(T eventData) where T : IEvent
    {
        if (eventData == null)
        {
            Debug.LogWarning("EventBus.Publish: Event data is null, publishing ignored.");
            return;
        }

        Type eventType = typeof(T);
        
        if (enableDebugLogging)
        {
            Debug.Log($"EventBus: Publishing {eventType.Name}");
        }

        if (!subscribers.ContainsKey(eventType))
        {
            // No subscribers, which is fine
            return;
        }

        // Create a copy of the list to avoid issues if subscribers modify the list during iteration
        List<object> handlers = new List<object>(subscribers[eventType]);
        
        foreach (object handler in handlers)
        {
            try
            {
                if (handler is Action<T> typedHandler)
                {
                    typedHandler.Invoke(eventData);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"EventBus: Error invoking handler for {eventType.Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Clear all subscriptions. Useful for cleanup or testing.
    /// </summary>
    public static void Clear()
    {
        subscribers.Clear();
        
        if (enableDebugLogging)
        {
            Debug.Log("EventBus: All subscriptions cleared");
        }
    }

    /// <summary>
    /// Get the number of subscribers for a specific event type.
    /// Useful for debugging.
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <returns>Number of subscribers</returns>
    public static int GetSubscriberCount<T>() where T : IEvent
    {
        Type eventType = typeof(T);
        
        if (subscribers.ContainsKey(eventType))
        {
            return subscribers[eventType].Count;
        }
        
        return 0;
    }
}

