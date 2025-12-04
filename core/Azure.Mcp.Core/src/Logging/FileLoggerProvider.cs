// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Logging;

/// <summary>
/// A file logger provider that writes logs to a file using a background thread for improved performance.
/// Log entries are queued and written asynchronously to avoid blocking the calling thread.
/// Log files are automatically created with timestamp-based filenames.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly BlockingCollection<string> _logQueue;
    private readonly Thread _writerThread;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private StreamWriter? _writer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileLoggerProvider"/> class.
    /// Creates a log file with an auto-generated timestamp-based filename in the specified folder.
    /// </summary>
    /// <param name="folderPath">The folder path where the log file should be created.</param>
    public FileLoggerProvider(string folderPath)
    {
        ArgumentNullException.ThrowIfNull(folderPath);

        // Ensure the directory exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Generate timestamp-based filename: azmcp_yyyyMMdd_HHmmss.log
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        var fileName = $"azmcp_{timestamp}.log";
        _filePath = Path.Combine(folderPath, fileName);

        _logQueue = new BlockingCollection<string>(boundedCapacity: 10000);
        _cancellationTokenSource = new CancellationTokenSource();

        // Create the log file
        _writer = new StreamWriter(_filePath, append: true)
        {
            AutoFlush = true
        };

        // Start the background writer thread
        _writerThread = new Thread(ProcessLogQueue)
        {
            IsBackground = true,
            Name = "FileLoggerWriter"
        };
        _writerThread.Start();
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
    /// </summary>
    private void ProcessLogQueue()
    {
        try
        {
            foreach (var message in _logQueue.GetConsumingEnumerable(_cancellationTokenSource.Token))
            {
                try
                {
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
