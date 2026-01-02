using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central service locator for accessing registered services.
/// Provides type-safe access to singleton services and managers.
/// 
/// Usage:
/// - Register: ServiceLocator.Register&lt;IMyService&gt;(myServiceInstance);
/// - Get: var service = ServiceLocator.Get&lt;IMyService&gt;();
/// - Unregister: ServiceLocator.Unregister&lt;IMyService&gt;();
/// </summary>
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
    private static bool enableDebugLogging = false;

    /// <summary>
    /// Enable or disable debug logging for service registration.
    /// </summary>
    public static bool EnableDebugLogging
    {
        get => enableDebugLogging;
        set => enableDebugLogging = value;
    }

    /// <summary>
    /// Register a service instance.
    /// </summary>
    /// <typeparam name="T">Service type (interface or class)</typeparam>
    /// <param name="service">Service instance to register</param>
    /// <exception cref="ArgumentNullException">Thrown if service is null</exception>
    public static void Register<T>(T service)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service), "Cannot register null service");
        }

        Type serviceType = typeof(T);
        
        if (services.ContainsKey(serviceType))
        {
            Debug.LogWarning($"ServiceLocator: Service of type {serviceType.Name} is already registered. Overwriting previous registration.");
        }

        services[serviceType] = service;
        
        if (enableDebugLogging)
        {
            Debug.Log($"ServiceLocator: Registered service {serviceType.Name}");
        }
    }

    /// <summary>
    /// Get a registered service instance.
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <returns>Service instance, or default(T) if not registered</returns>
    public static T Get<T>()
    {
        Type serviceType = typeof(T);
        
        if (services.TryGetValue(serviceType, out object service))
        {
            return (T)service;
        }

        if (enableDebugLogging)
        {
            Debug.LogWarning($"ServiceLocator: Service of type {serviceType.Name} is not registered.");
        }

        return default(T);
    }

    /// <summary>
    /// Try to get a registered service instance.
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <param name="service">Output parameter for the service instance</param>
    /// <returns>True if service was found, false otherwise</returns>
    public static bool TryGet<T>(out T service)
    {
        Type serviceType = typeof(T);
        
        if (services.TryGetValue(serviceType, out object serviceObj))
        {
            service = (T)serviceObj;
            return true;
        }

        service = default(T);
        return false;
    }

    /// <summary>
    /// Check if a service is registered.
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <returns>True if service is registered, false otherwise</returns>
    public static bool IsRegistered<T>()
    {
        return services.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Unregister a service.
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    public static void Unregister<T>()
    {
        Type serviceType = typeof(T);
        
        if (services.Remove(serviceType))
        {
            if (enableDebugLogging)
            {
                Debug.Log($"ServiceLocator: Unregistered service {serviceType.Name}");
            }
        }
        else
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning($"ServiceLocator: Service of type {serviceType.Name} was not registered.");
            }
        }
    }

    /// <summary>
    /// Clear all registered services. Useful for cleanup or testing.
    /// </summary>
    public static void Clear()
    {
        services.Clear();
        
        if (enableDebugLogging)
        {
            Debug.Log("ServiceLocator: All services cleared");
        }
    }

    /// <summary>
    /// Get the number of registered services.
    /// Useful for debugging.
    /// </summary>
    /// <returns>Number of registered services</returns>
    public static int GetServiceCount()
    {
        return services.Count;
    }
}

