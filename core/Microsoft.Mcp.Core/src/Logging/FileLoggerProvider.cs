// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Logging;

/// <summary>
/// A file logger provider that writes logs to a file using a background thread for improved performance.
/// Log entries are queued and written asynchronously to avoid blocking the calling thread.
/// Log files are automatically created with timestamp-based filenames and rotated hourly.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _folderPath;
    private readonly BlockingCollection<string> _logQueue;
    private readonly Thread _writerThread;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly object _writerLock = new();
    private StreamWriter? _writer;
    private DateTime _currentFileHour;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileLoggerProvider"/> class.
    /// Creates a log file with an auto-generated timestamp-based filename in the specified folder.
    /// Log files are rotated hourly to prevent excessively large files during long-running sessions.
    /// </summary>
    /// <param name="folderPath">The folder path where the log file should be created.</param>
    public FileLoggerProvider(string folderPath)
    {
        ArgumentNullException.ThrowIfNull(folderPath);

        _folderPath = folderPath;

        // Ensure the directory exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        _logQueue = new BlockingCollection<string>(boundedCapacity: 10000);
        _cancellationTokenSource = new CancellationTokenSource();

        // Create the initial log file
        _currentFileHour = GetCurrentHour();
        _writer = CreateLogFileWriter(_currentFileHour);

        // Start the background writer thread
        _writerThread = new Thread(ProcessLogQueue)
        {
            IsBackground = true,
            Name = "FileLoggerWriter"
        };
        _writerThread.Start();
    }

    /// <summary>
    /// Gets the current time truncated to the hour for file rotation comparison.
    /// </summary>
    private static DateTime GetCurrentHour()
    {
        var now = DateTime.Now;
        return new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);
    }

    /// <summary>
    /// Creates a new StreamWriter for a log file based on the specified hour.
    /// </summary>
    /// <param name="hour">The hour to use for the filename.</param>
    /// <returns>A StreamWriter configured for the log file.</returns>
    private StreamWriter CreateLogFileWriter(DateTime hour)
    {
        // Generate timestamp-based filename: azmcp_yyyyMMdd_HH.log (hourly rotation)
        var timestamp = hour.ToString("yyyyMMdd_HH", CultureInfo.InvariantCulture);
        var fileName = $"azmcp_{timestamp}.log";
        var filePath = Path.Combine(_folderPath, fileName);

        return new StreamWriter(filePath, append: true)
        {
            AutoFlush = true
        };
    }

    /// <summary>
    /// Rotates the log file if the current hour has changed since the last write.
    /// This method is called from the background writer thread before each write operation.
    /// </summary>
    private void RotateFileIfNeeded()
    {
        var currentHour = GetCurrentHour();
        if (currentHour == _currentFileHour)
        {
            return;
        }

        // Hour has changed, rotate to a new file
        lock (_writerLock)
        {
            // Double-check after acquiring the lock
            if (currentHour == _currentFileHour)
            {
                return;
            }

            // Close the old writer
            _writer?.Dispose();

            // Create a new writer for the new hour
            _currentFileHour = currentHour;
            _writer = CreateLogFileWriter(_currentFileHour);
        }
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, this);
    }

    /// <summary>
    /// Queues a log entry to be written to the file by the background thread.
    /// </summary>
    /// <param name="message">The log message to write.</param>
    internal void WriteLog(string message)
    {
        if (_disposed)
        {
            return;
        }

        // Try to add to the queue, but don't block if the queue is full
        // This prevents the logging from blocking the application if logs are being generated faster than they can be written
        _logQueue.TryAdd(message);
    }

    /// <summary>
    /// Background thread method that processes the log queue and writes entries to the file.
    /// Handles hourly log file rotation automatically.
    /// </summary>
    private void ProcessLogQueue()
    {
        try
        {
            foreach (var message in _logQueue.GetConsumingEnumerable(_cancellationTokenSource.Token))
            {
                try
                {
                    RotateFileIfNeeded();
                    _writer?.WriteLine(message);
                }
                catch (ObjectDisposedException)
                {
                    // Writer was disposed, exit the loop
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested during shutdown
        }

        // Drain any remaining messages in the queue before exiting
        while (_logQueue.TryTake(out var remainingMessage))
        {
            try
            {
                _writer?.WriteLine(remainingMessage);
            }
            catch (ObjectDisposedException)
            {
                break;
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // Signal the writer thread to stop and complete adding to the queue
        _logQueue.CompleteAdding();
        _cancellationTokenSource.Cancel();

        // Wait for the writer thread to finish (with timeout to prevent hanging)
        _writerThread.Join(TimeSpan.FromSeconds(5));

        // Clean up resources
        _cancellationTokenSource.Dispose();
        _logQueue.Dispose();
        _writer?.Dispose();
        _writer = null;
    }
}
