// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

public class ValidationResult
{
    private const string DefaultTelemetrySafeMessage = "Invalid options.";
    private readonly List<(string Message, string TelemetrySafeMessage)> _trackedErrors = [];

    public bool IsValid => Errors.Count == 0;

    public List<string> Errors { get; } = [];

    /// <summary>
    /// Gets only explicitly safe messages, or a generic message when an error was added without one.
    /// </summary>
    public string TelemetrySafeMessage => _trackedErrors.Count == Errors.Count &&
        Errors.SequenceEqual(_trackedErrors.Select(error => error.Message))
        ? string.Join('\n', _trackedErrors.Select(error => error.TelemetrySafeMessage))
        : DefaultTelemetrySafeMessage;

    /// <summary>
    /// Adds the client message and a separate static message suitable for telemetry.
    /// </summary>
    public void AddError(string message, string telemetrySafeMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(telemetrySafeMessage);

        Errors.Add(message);
        _trackedErrors.Add((message, telemetrySafeMessage));
    }
}
