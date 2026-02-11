// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Azure.Mcp.Core.Services.Telemetry;

/// <summary>
/// Prevents emitting telemetry events by OpenTelemetryExporter.  Accomplishes this by clearing the log contents
/// sent when calling any log methods on <see cref="Microsoft.Extensions.Logging.ILogger"/>.
/// </summary>
internal class TelemetryLogRecordEraser : BaseProcessor<LogRecord>
{
    private static readonly IReadOnlyList<KeyValuePair<string, object?>> EmptyAttributes = new List<KeyValuePair<string, object?>>().AsReadOnly();

    public override void OnEnd(LogRecord data)
    {
        data.Attributes = EmptyAttributes;
        data.Body = string.Empty;
        data.FormattedMessage = string.Empty;
    }
}
