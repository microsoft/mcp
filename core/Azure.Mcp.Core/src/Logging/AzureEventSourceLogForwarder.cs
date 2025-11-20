// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.Tracing;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Logging;

/// <summary>
/// Forwards Azure SDK EventSource events to the Microsoft.Extensions.Logging infrastructure.
/// This enables capturing Azure SDK diagnostic events (requests, responses, retries, authentication)
/// and forwarding them to configured logging providers.
/// </summary>
/// <remarks>
/// Based on Azure SDK diagnostics documentation:
/// https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/core/Azure.Core/samples/Diagnostics.md#Logging
/// </remarks>
public sealed class AzureSdkEventSourceLogForwarder : EventListener
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly Dictionary<string, ILogger> _loggers = new();
    private EventLevel _level = EventLevel.Informational;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureSdkEventSourceLogForwarder"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create loggers for each Azure event source.</param>
    /// <param name="level">The minimum event level to capture. Defaults to Informational.</param>
    public AzureSdkEventSourceLogForwarder(ILoggerFactory loggerFactory, EventLevel level = EventLevel.Informational)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _level = level;
    }

    /// <summary>
    /// Called when a new EventSource is created. Enables listening to Azure-related event sources.
    /// </summary>
    /// <param name="eventSource">The event source that was created.</param>
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        base.OnEventSourceCreated(eventSource);

        // Listen to all Azure SDK event sources
        // Azure SDK event sources start with "Azure-"
        if (eventSource.Name?.StartsWith("Azure-", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                EnableEvents(eventSource, _level, EventKeywords.All);

                // Create a logger for this event source if not already created
                if (!_loggers.ContainsKey(eventSource.Name))
                {
                    _loggers[eventSource.Name] = _loggerFactory.CreateLogger($"Azure.SDK.{eventSource.Name}");
                }
            }
            catch (Exception)
            {
                // Ignore errors enabling event sources
            }
        }
    }

    /// <summary>
    /// Called when an event is written. Forwards the event to the appropriate logger.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData.EventSource?.Name == null)
        {
            return;
        }

        // Get or create a logger for this event source
        if (!_loggers.TryGetValue(eventData.EventSource.Name, out var logger))
        {
            logger = _loggerFactory.CreateLogger($"Azure.SDK.{eventData.EventSource.Name}");
            _loggers[eventData.EventSource.Name] = logger;
        }

        // Map EventSource EventLevel to Microsoft.Extensions.Logging LogLevel
        var logLevel = MapEventLevel(eventData.Level);

        // Format the message
        var message = FormatMessage(eventData);

        // Log the event
        logger.Log(logLevel, eventData.EventId, message, null, (state, _) => state);
    }

    /// <summary>
    /// Maps EventSource EventLevel to Microsoft.Extensions.Logging LogLevel.
    /// </summary>
    private static LogLevel MapEventLevel(EventLevel eventLevel) => eventLevel switch
    {
        EventLevel.Critical => LogLevel.Critical,
        EventLevel.Error => LogLevel.Error,
        EventLevel.Warning => LogLevel.Warning,
        EventLevel.Informational => LogLevel.Information,
        EventLevel.Verbose => LogLevel.Debug,
        EventLevel.LogAlways => LogLevel.Information,
        _ => LogLevel.Trace
    };

    /// <summary>
    /// Formats the event message with its payload.
    /// </summary>
    private static string FormatMessage(EventWrittenEventArgs eventData)
    {
        if (eventData.Payload == null || eventData.Payload.Count == 0)
        {
            return eventData.Message ?? $"[{eventData.EventName}]";
        }

        // Try to format with payload
        try
        {
            if (!string.IsNullOrEmpty(eventData.Message))
            {
                return string.Format(eventData.Message, eventData.Payload.ToArray());
            }
        }
        catch
        {
            // If formatting fails, fall back to simple concatenation
        }

        // Build message with payload names and values
        var payloadNames = eventData.PayloadNames;
        if (payloadNames != null && payloadNames.Count == eventData.Payload.Count)
        {
            var parts = new List<string> { $"[{eventData.EventName}]" };
            for (int i = 0; i < payloadNames.Count; i++)
            {
                parts.Add($"{payloadNames[i]}={eventData.Payload[i]}");
            }
            return string.Join(" ", parts);
        }

        // Fallback: just event name and payload values
        return $"[{eventData.EventName}] {string.Join(", ", eventData.Payload)}";
    }

    /// <summary>
    /// Disposes the event listener and stops listening to event sources.
    /// </summary>
    public override void Dispose()
    {
        base.Dispose();
        _loggers.Clear();
    }
}
