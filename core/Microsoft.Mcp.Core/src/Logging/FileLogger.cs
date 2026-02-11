// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Logging;

/// <summary>
/// A simple file logger that writes logs to a file for support and troubleshooting purposes.
/// </summary>
internal sealed class FileLogger(string categoryName, FileLoggerProvider provider) : ILogger
{
    private readonly string _categoryName = categoryName;
    private readonly FileLoggerProvider _provider = provider;

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logLevelString = GetLogLevelString(logLevel);

        var logEntry = $"[{timestamp}] [{logLevelString}] [{_categoryName}] {message}";

        if (exception != null)
        {
            logEntry += Environment.NewLine + exception.ToString();
        }

        _provider.WriteLog(logEntry);
    }

    private static string GetLogLevelString(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => "TRCE",
        LogLevel.Debug => "DBUG",
        LogLevel.Information => "INFO",
        LogLevel.Warning => "WARN",
        LogLevel.Error => "ERRR",
        LogLevel.Critical => "CRIT",
        _ => "NONE"
    };
}
