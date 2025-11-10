// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Areas.Server.Services;

/// <summary>
/// A simple file logger provider for writing logs to a file.
/// </summary>
internal sealed class SimpleFileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly object _lock = new();

    public SimpleFileLoggerProvider(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new SimpleFileLogger(categoryName, _filePath, _lock);
    }

    public void Dispose()
    {
        // Nothing to dispose
    }

    private sealed class SimpleFileLogger(string categoryName, string filePath, object lockObject) : ILogger
    {
        private readonly string _categoryName = categoryName;
        private readonly string _filePath = filePath;
        private readonly object _lock = lockObject;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel}] [{_categoryName}] {message}";

            if (exception != null)
            {
                logEntry += Environment.NewLine + exception;
            }

            lock (_lock)
            {
                try
                {
                    var directory = Path.GetDirectoryName(_filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    File.AppendAllText(_filePath, logEntry + Environment.NewLine);
                }
                catch
                {
                    // Silently fail if we can't write to the log file
                    // to avoid breaking the application
                }
            }
        }
    }
}
