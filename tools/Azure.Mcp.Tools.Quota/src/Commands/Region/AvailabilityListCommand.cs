// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Quota.Models;
using Azure.Mcp.Tools.Quota.Options.Region;
using Azure.Mcp.Tools.Quota.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Quota.Commands.Region;

[CommandMetadata(
    Id = "0b8902f5-3fd4-49d9-b73e-4cea88afdd62",
    Name = "list",
    Title = "Get available regions for Azure resource types",
    Description = "Given a list of Azure resource types, this tool will return a list of regions where the resource types are available. Always get the user's subscription ID before calling this tool.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AvailabilityListCommand(ILogger<AvailabilityListCommand> logger, IQuotaService quotaService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AvailabilityListOptions, AvailabilityListCommand.RegionCheckCommandResult>(subscriptionResolver)
{
    private readonly ILogger<AvailabilityListCommand> _logger = logger;
    private readonly IQuotaService _quotaService = quotaService;

    public override void ValidateOptions(AvailabilityListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (string.IsNullOrWhiteSpace(options.ResourceTypes) || !options.ResourceTypes.Split(',').Any(rt => !string.IsNullOrWhiteSpace(rt)))
        {
            validationResult.Errors.Add("Resource types cannot be empty.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AvailabilityListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?.AddTag(QuotaTelemetryTags.ResourceTypes, options.ResourceTypes);

            var resourceTypes = options.ResourceTypes.Split(',')
                .Select(rt => rt.Trim())
                .Where(rt => !string.IsNullOrWhiteSpace(rt))
                .ToArray();

            List<string> toolResult = await _quotaService.GetAvailableRegionsForResourceTypesAsync(
                resourceTypes,
                options.Subscription!,
                options.CognitiveServiceModelName,
                options.CognitiveServiceModelVersion,
                options.CognitiveServiceDeploymentSkuName,
                cancellationToken);

            _logger.LogInformation("Region check result: {ToolResult}", toolResult);

            context.Response.Results = ResponseResult.Create(new(toolResult ?? []), QuotaJsonContext.Default.RegionCheckCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred checking available Azure regions.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record RegionCheckCommandResult(List<string> AvailableRegions);
}
