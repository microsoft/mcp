// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Services.Logging;

/// <summary>
/// Simple file logger provider for writing logs to a file.
/// Uses a shared StreamWriter with AutoFlush for efficient high-frequency logging.
/// </summary>
internal sealed class SimpleFileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly LogLevel _minLevel;
    private readonly StreamWriter _streamWriter;
    private readonly object _lock = new();
    private bool _disposed = false;

    public SimpleFileLoggerProvider(string filePath, LogLevel minLevel)
    {
        _filePath = filePath;
        _minLevel = minLevel;

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
        return new SimpleFileLogger(categoryName, _minLevel, _streamWriter, _lock);
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
/// Simple file logger implementation.
/// Uses a shared StreamWriter for efficient logging without frequent file open/close operations.
/// </summary>
internal sealed class SimpleFileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly LogLevel _minLevel;
    private readonly StreamWriter _streamWriter;
    private readonly object _lock;

    public SimpleFileLogger(string categoryName, LogLevel minLevel, StreamWriter streamWriter, object lockObject)
    {
        _categoryName = categoryName;
        _minLevel = minLevel;
        _streamWriter = streamWriter;
        _lock = lockObject;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _minLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

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
