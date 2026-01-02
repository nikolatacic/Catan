using UnityEngine;

/// <summary>
/// Unity implementation of ILogger that wraps Unity's Debug.Log methods.
/// </summary>
public class UnityLogger : ILogger
{
    private readonly string context;

    /// <summary>
    /// Create a new UnityLogger instance.
    /// </summary>
    /// <param name="context">Optional context string to prefix log messages</param>
    public UnityLogger(string context = null)
    {
        this.context = context;
    }

    /// <summary>
    /// Log an informational message.
    /// </summary>
    public void Log(string message)
    {
        string formattedMessage = FormatMessage(message);
        Debug.Log(formattedMessage);
    }

    /// <summary>
    /// Log a warning message.
    /// </summary>
    public void LogWarning(string message)
    {
        string formattedMessage = FormatMessage(message);
        Debug.LogWarning(formattedMessage);
    }

    /// <summary>
    /// Log an error message.
    /// </summary>
    public void LogError(string message)
    {
        string formattedMessage = FormatMessage(message);
        Debug.LogError(formattedMessage);
    }

    /// <summary>
    /// Log an exception.
    /// </summary>
    public void LogException(System.Exception exception)
    {
        string formattedMessage = FormatMessage(exception.Message);
        Debug.LogError($"{formattedMessage}\n{exception.StackTrace}");
    }

    private string FormatMessage(string message)
    {
        if (string.IsNullOrEmpty(context))
        {
            return message;
        }

        return $"[{context}] {message}";
    }
}

