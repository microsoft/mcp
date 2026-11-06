// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models.HealthModels;
using Azure.Mcp.Tools.Monitor.Options.HealthModels;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.HealthModels;

[CommandMetadata(
    Id = "4f0c8b51-2a6d-4e39-9c17-b85e3d6a0f24",
    Name = "inspect",
    Title = "Inspect the Entities of an Azure Monitor Health Model",
    Description = """
        Look inside an Azure Monitor health model: its entities with their health state, signal groups and signal
        aggregation groups, what drives an entity's state (walk Causes), what it depends on or what depends on it,
        health state history, signal history and data annotations, now or at a past timestamp. Payloads are the
        CloudHealth API objects, passed through unchanged; --fields keeps only the field paths you name. Use it to
        explain why a health model or entity is unhealthy, degraded or unknown. Use 'get' for the health model
        resource itself.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class HealthModelInspectCommand(IMonitorHealthModelService healthModelService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<HealthModelInspectOptions, HealthModelInspectResult>(subscriptionResolver)
{
    private const int MaxTop = 500;

    // A parsed payload is at most 64 levels deep, so a longer field path can match nothing.
    private const int MaxFieldDepth = 64;

    private readonly IMonitorHealthModelService _healthModelService = healthModelService;

    public override void ValidateOptions(HealthModelInspectOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.Include?.Contains(HealthModelInclude.SignalHistory) == true && (string.IsNullOrEmpty(options.Entity) || options.Walk is not null))
        {
            validationResult.Errors.Add("--include SignalHistory needs --entity and no --walk: signal history is read per signal of one entity.");
        }
        if (options.StartAt is { } start && (options.EndAt ?? options.Timestamp ?? DateTimeOffset.UtcNow) is var end && start >= end)
        {
            validationResult.Errors.Add("--start-at must be before the end: --end-at, else --timestamp, else now.");
        }
        if (options.Top is < 1 or > MaxTop)
        {
            validationResult.Errors.Add($"--top must be 1 through {MaxTop}.");
        }
        if (options.NextMarker is { } marker && !int.TryParse(marker, NumberStyles.None, CultureInfo.InvariantCulture, out _))
        {
            validationResult.Errors.Add("--next-marker must be the nextMarker of a previous result.");
        }
        if (options.Fields?.Any(field => field.Split(',').Any(path => path.Split('.').Length > MaxFieldDepth)) == true)
        {
            validationResult.Errors.Add($"Each --fields path can have at most {MaxFieldDepth} segments.");
        }
    }

    public override JsonTypeInfo<HealthModelInspectResult> ResultTypeInfo => MonitorJsonContext.Default.HealthModelInspectResult;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, HealthModelInspectOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // An empty --entity is no entity, as validation treats it.
            var entity = string.IsNullOrEmpty(options.Entity) ? null : options.Entity;
            var fields = options.Fields?.SelectMany(field => field.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToList();
            var skip = options.NextMarker is { } marker ? int.Parse(marker, CultureInfo.InvariantCulture) : 0;
            var result = await _healthModelService.InspectHealthModel(
                options.Subscription!,
                options.ResourceGroup,
                options.HealthModel,
                new(entity, options.WhereHealth, options.Walk, options.Include ?? [], options.StartAt, options.EndAt, options.Timestamp, fields, options.Top, skip),
                options.Tenant,
                cancellationToken);

            SetResult(context, result);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
