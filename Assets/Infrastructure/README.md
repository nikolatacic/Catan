# Infrastructure Package

## Overview
The Infrastructure package provides the foundational systems for event-driven architecture. It contains zero dependencies and can be used as a standalone package in any Unity project.

**Package Name**: `com.yourcompany.infrastructure`  
**Version**: 1.0.0  
**Dependencies**: None

---

## What's Included

### EventBus
Central event system for decoupled communication between systems. Allows any system to publish events and any other system to subscribe to them without direct references.

### ServiceLocator
Central registry for accessing services and managers. Provides type-safe access to singleton services throughout your application.

### Logger
Logging interface and Unity implementation. Allows for centralized logging with the ability to swap implementations.

---

## How to Use

### EventBus

**Subscribe to Events:**
```csharp
private void OnEnable()
{
    EventBus.Subscribe<MyEvent>(OnMyEvent);
}

private void OnDisable()
{
    EventBus.Unsubscribe<MyEvent>(OnMyEvent);
}

private void OnMyEvent(MyEvent evt)
{
    // Handle event
}
```

**Publish Events:**
```csharp
// Create and publish an event
EventBus.Publish(new MyEvent(data));
```

**Create Custom Events:**
```csharp
public class MyEvent : IEvent
{
    public float Timestamp { get; }
    public string Message { get; }
    
    public MyEvent(string message)
    {
        Timestamp = Time.time;
        Message = message;
    }
}
```

### ServiceLocator

**Register Services:**
```csharp
// In Awake() or initialization
ServiceLocator.Register<IMyService>(myServiceInstance);
```

**Get Services:**
```csharp
// Get a service
IMyService service = ServiceLocator.Get<IMyService>();

// Try to get a service (safe)
if (ServiceLocator.TryGet<IMyService>(out IMyService service))
{
    // Use service
}

// Check if registered
if (ServiceLocator.IsRegistered<IMyService>())
{
    // Service is available
}
```

**Unregister Services:**
```csharp
ServiceLocator.Unregister<IMyService>();
```

### Logger

**Use Logger:**
```csharp
// Create logger
ILogger logger = new UnityLogger("MySystem");

// Log messages
logger.Log("Info message");
logger.LogWarning("Warning message");
logger.LogError("Error message");
logger.LogException(exception);
```

**Register Logger with ServiceLocator:**
```csharp
ILogger logger = new UnityLogger("Game");
ServiceLocator.Register<ILogger>(logger);

// Use anywhere
ILogger logger = ServiceLocator.Get<ILogger>();
logger.Log("Message");
```

---

## How to Modify/Upgrade

### Extending EventBus

The EventBus is a static class designed to be used as-is. If you need additional functionality:

1. **Create Wrapper Class**: Wrap EventBus in your own class
```csharp
public class MyEventBus
{
    public static void SubscribeWithLogging<T>(Action<T> handler) where T : IEvent
    {
        EventBus.Subscribe<T>(handler);
        Debug.Log($"Subscribed to {typeof(T).Name}");
    }
}
```

2. **Create Custom Event Base**: Extend IEvent with additional properties
```csharp
public abstract class BaseEvent : IEvent
{
    public float Timestamp { get; }
    public int SourceID { get; }
    
    protected BaseEvent(int sourceID)
    {
        Timestamp = Time.time;
        SourceID = sourceID;
    }
}
```

### Extending ServiceLocator

ServiceLocator is designed to be used as-is. For additional features:

1. **Create Wrapper**: Add validation or logging
```csharp
public static class MyServiceLocator
{
    public static void RegisterWithValidation<T>(T service) where T : class
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }
        ServiceLocator.Register<T>(service);
    }
}
```

### Creating Custom Logger

Implement ILogger for custom logging:
```csharp
public class FileLogger : ILogger
{
    public void Log(string message) { /* Write to file */ }
    public void LogWarning(string message) { /* Write to file */ }
    public void LogError(string message) { /* Write to file */ }
    public void LogException(Exception exception) { /* Write to file */ }
}
```

---

## Best Practices

1. **Always Unsubscribe**: Unsubscribe from events in OnDisable() to prevent memory leaks
2. **Use TryGet**: Use TryGet() when service availability is uncertain
3. **Register Early**: Register services in Awake() or early initialization
4. **Type Safety**: Use interfaces for services, not concrete classes
5. **Event Naming**: End event class names with "Event" (e.g., `DiceRolledEvent`)

---

## Testing

Use the `InfrastructureTest` script to validate all systems:
1. Add `InfrastructureTest` component to a GameObject
2. Run the scene
3. Check console for test results

---

## Package Extraction

When extracting to a package:
1. Copy `Infrastructure/` folder to package structure
2. Create `package.json` with no dependencies
3. Update namespaces if needed
4. Test package in isolation

---

## Version History

- **1.0.0** (2024-12-19): Initial release
  - EventBus system
  - ServiceLocator system
  - Logger system

---

## Support

For issues or questions, refer to the main project documentation or create an issue in the repository.

