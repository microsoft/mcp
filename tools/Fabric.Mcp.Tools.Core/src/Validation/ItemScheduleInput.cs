// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class ItemScheduleInput
{
    internal const int MaxRequestBytes = FabricItemJsonInput.MaxObjectBytes;

    internal static void ValidateScheduleContext(
        string workspaceId, string itemId, string jobType, string scheduleId, ValidationResult validation)
    {
        ItemJobRunInput.ValidateContext(workspaceId, itemId, jobType, validation);
        if (!Guid.TryParse(scheduleId, out var schedule) || schedule == Guid.Empty)
        {
            validation.Errors.Add("--schedule-id must be a nonempty UUID.");
        }
    }

    internal static string CreateBody(bool enabled, string configuration, string? executionData, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var settings = FabricItemJsonInput.ParseObject(configuration, "--configuration")
                ?? throw new FabricItemInputException("--configuration is required; no schedule or timezone defaults are inferred.");
            FabricItemScheduleJson.ValidateUnicode(settings);
            FabricItemScheduleJson.ValidateConfiguration(settings);
            _ = FabricItemScheduleJson.TryReadTimestamp(settings.GetProperty("startDateTime").GetString()!, out var start);
            _ = FabricItemScheduleJson.TryReadTimestamp(settings.GetProperty("endDateTime").GetString()!, out var end);
            if (start.Offset != TimeSpan.Zero || end.Offset != TimeSpan.Zero || end <= start)
            {
                throw new FabricItemInputException("--configuration requires UTC startDateTime and endDateTime, with end later than start. Offset-less API examples are interpreted as UTC. No times are rewritten.");
            }
            var execution = FabricItemJsonInput.ParseObject(executionData, "--execution-data");
            if (execution is { } data)
            {
                FabricItemScheduleJson.ValidateUnicode(data);
            }
            // Create and Update share the same required payload in the Fabric contract.
            var body = JsonSerializer.Serialize(new ItemScheduleCreateRequest(enabled, settings, execution), CoreJsonContext.Default.ItemScheduleCreateRequest);
            if (Encoding.UTF8.GetByteCount(body) > MaxRequestBytes)
            {
                throw new FabricItemInputException("The serialized schedule request must not exceed the 1 MiB (1,048,576 byte) tool budget.");
            }
            cancellationToken.ThrowIfCancellationRequested();
            return body;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or ArgumentException { InnerException: EncoderFallbackException })
        {
            throw new FabricItemInputException("Invalid schedule JSON. Supply valid Unicode and configuration with type, UTC start/end, localTimeZoneId, and the documented fields and limits for that type. Cron interval is in minutes, not a cron expression.");
        }
    }
}
