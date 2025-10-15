// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Services.Logging;

/// <summary>
/// Extension methods for adding file logging to the logging builder.
/// </summary>
public static class FileLoggerExtensions
{
    /// <summary>
    /// Adds file logging to the logging builder.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="filePath">The path to the log file. Supports {timestamp} and {pid} placeholders.</param>
    /// <returns>The logging builder for chaining.</returns>
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string filePath)
    {
        var resolvedPath = ResolveLogFilePath(filePath);
        builder.Services.AddSingleton<ILoggerProvider>(provider => new FileLoggerProvider(resolvedPath));
        return builder;
    }

    /// <summary>
    /// Resolves log file path with placeholder substitution.
    /// </summary>
    private static string ResolveLogFilePath(string logFilePath)
    {
        if (string.IsNullOrEmpty(logFilePath))
            return logFilePath;

        var resolved = logFilePath
            .Replace("{timestamp}", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"))
            .Replace("{pid}", Environment.ProcessId.ToString());

        // Ensure directory exists
        var directory = Path.GetDirectoryName(resolved);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return resolved;
    }
}

/// <summary>
/// File logger provider for writing logs to a file.
/// Uses a shared StreamWriter with AutoFlush for efficient high-frequency logging.
/// </summary>
internal sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly StreamWriter _streamWriter;
    private readonly object _lock = new();
    private bool _disposed = false;

    public FileLoggerProvider(string filePath)
    {
        _filePath = filePath;

        // Ensure directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Create StreamWriter with AutoFlush for immediate writes without frequent file open/close
        _streamWriter = new StreamWriter(filePath, append: true)
        {
            AutoFlush = true
        };
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _streamWriter, _lock);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            lock (_lock)
            {
                _streamWriter?.Dispose();
                _disposed = true;
            }
        }
    }
}

/// <summary>
/// Uses a shared StreamWriter for efficient logging without frequent file open/close operations.
/// The logging framework handles filtering, so this logger writes all messages passed to it.
/// </summary>
internal sealed class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly StreamWriter _streamWriter;
    private readonly object _lock;

    public FileLogger(string categoryName, StreamWriter streamWriter, object lockObject)
    {
        _categoryName = categoryName;
        _streamWriter = streamWriter;
        _lock = lockObject;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        // The logging framework handles filtering, so we always return true
        // and let the framework decide what to log
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logLevelString = GetLogLevelString(logLevel);
        var message = formatter(state, exception);

        var logEntry = $"[{timestamp}] [{logLevelString}] {_categoryName}: {message}";

        if (exception != null)
        {
            logEntry += Environment.NewLine + exception.ToString();
        }

        lock (_lock)
        {
            try
            {
                _streamWriter.WriteLine(logEntry);
                // AutoFlush is enabled, so no need to manually flush
            }
            catch (IOException)
            {
                // Silently ignore file write errors to prevent logging from breaking the application
            }
        }
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "TRCE",
            LogLevel.Debug => "DBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "FAIL",
            LogLevel.Critical => "CRIT",
            _ => "UNKN"
        };
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();
        public void Dispose() { }
    }
}
