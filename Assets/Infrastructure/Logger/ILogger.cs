/// <summary>
/// Interface for logging functionality.
/// Allows for different logging implementations (Unity, file, remote, etc.).
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Log an informational message.
    /// </summary>
    /// <param name="message">Message to log</param>
    void Log(string message);

    /// <summary>
    /// Log a warning message.
    /// </summary>
    /// <param name="message">Warning message to log</param>
    void LogWarning(string message);

    /// <summary>
    /// Log an error message.
    /// </summary>
    /// <param name="message">Error message to log</param>
    void LogError(string message);

    /// <summary>
    /// Log an exception.
    /// </summary>
    /// <param name="exception">Exception to log</param>
    void LogException(System.Exception exception);
}

