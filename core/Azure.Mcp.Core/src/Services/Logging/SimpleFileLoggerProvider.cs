// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Services.Logging;

/// <summary>
/// Simple file logger provider for writing logs to a file.
/// </summary>
internal sealed class SimpleFileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly LogLevel _minLevel;
    private readonly object _lock = new();

    public SimpleFileLoggerProvider(string filePath, LogLevel minLevel)
    {
        _filePath = filePath;
        _minLevel = minLevel;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new SimpleFileLogger(categoryName, _filePath, _minLevel, _lock);
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}

/// <summary>
/// Simple file logger implementation.
/// </summary>
internal sealed class SimpleFileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _filePath;
    private readonly LogLevel _minLevel;
    private readonly object _lock;

    public SimpleFileLogger(string categoryName, string filePath, LogLevel minLevel, object lockObject)
    {
        _categoryName = categoryName;
        _filePath = filePath;
        _minLevel = minLevel;
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

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logLevelString = GetLogLevelString(logLevel);
        var message = formatter(state, exception);
        
        var logEntry = $"[{timestamp}] [{logLevelString}] {_categoryName}: {message}";
        
        if (exception != null)
        {
            logEntry += Environment.NewLine + exception.ToString();
        }
        
        logEntry += Environment.NewLine;

        lock (_lock)
        {
            try
            {
                File.AppendAllText(_filePath, logEntry);
            }
            catch
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