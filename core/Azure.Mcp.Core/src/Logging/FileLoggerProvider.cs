// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Logging;

/// <summary>
/// A simple file logger provider that writes logs to a file for support and troubleshooting purposes.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly object _lock = new();
    private StreamWriter? _writer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileLoggerProvider"/> class.
    /// </summary>
    /// <param name="filePath">The file path where logs should be written.</param>
    public FileLoggerProvider(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Create or append to the log file
        _writer = new StreamWriter(filePath, append: true)
        {
            AutoFlush = true
        };
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, this);
    }

    /// <summary>
    /// Writes a log entry to the file.
    /// </summary>
    /// <param name="message">The log message to write.</param>
    internal void WriteLog(string message)
    {
        if (_disposed || _writer == null)
        {
            return;
        }

        lock (_lock)
        {
            if (_disposed || _writer == null)
            {
                return;
            }

            _writer.WriteLine(message);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _writer?.Dispose();
            _writer = null;
        }
    }
}
