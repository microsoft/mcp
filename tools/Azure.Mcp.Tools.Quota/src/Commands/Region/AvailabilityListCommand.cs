// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Quota.Models;
using Azure.Mcp.Tools.Quota.Options;
using Azure.Mcp.Tools.Quota.Options.Region;
using Azure.Mcp.Tools.Quota.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Commands.Region;

public sealed class AvailabilityListCommand(ILogger<AvailabilityListCommand> logger) : SubscriptionCommand<AvailabilityListOptions>()
{
    private const string CommandTitle = "Get available regions for Azure resource types";
    private readonly ILogger<AvailabilityListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        """
        Given a list of Azure resource types, this tool will return a list of regions where the resource types are available. Always get the user's subscription ID before calling this tool.
        """;

    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(QuotaOptionDefinitions.RegionCheck.ResourceTypes);
        command.Options.Add(QuotaOptionDefinitions.RegionCheck.CognitiveServiceModelName);
        command.Options.Add(QuotaOptionDefinitions.RegionCheck.CognitiveServiceModelVersion);
        command.Options.Add(QuotaOptionDefinitions.RegionCheck.CognitiveServiceDeploymentSkuName);
    }

    protected override AvailabilityListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceTypes = parseResult.GetValueOrDefault<string>(QuotaOptionDefinitions.RegionCheck.ResourceTypes.Name) ?? string.Empty;
        options.CognitiveServiceModelName = parseResult.GetValueOrDefault<string>(QuotaOptionDefinitions.RegionCheck.CognitiveServiceModelName.Name);
        options.CognitiveServiceModelVersion = parseResult.GetValueOrDefault<string>(QuotaOptionDefinitions.RegionCheck.CognitiveServiceModelVersion.Name);
        options.CognitiveServiceDeploymentSkuName = parseResult.GetValueOrDefault<string>(QuotaOptionDefinitions.RegionCheck.CognitiveServiceDeploymentSkuName.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            context.Activity?.AddTag(QuotaTelemetryTags.ResourceTypes, options.ResourceTypes);

            var resourceTypes = options.ResourceTypes.Split(',')
                .Select(rt => rt.Trim())
                .Where(rt => !string.IsNullOrWhiteSpace(rt))
                .ToArray();

            if (resourceTypes.Length == 0)
            {
                throw new ArgumentException("Resource types cannot be empty.", nameof(options.ResourceTypes));
            }

            var quotaService = context.GetService<IQuotaService>();
            List<string> toolResult = await quotaService.GetAvailableRegionsForResourceTypesAsync(
                resourceTypes,
                options.Subscription!,
                options.CognitiveServiceModelName,
                options.CognitiveServiceModelVersion,
                options.CognitiveServiceDeploymentSkuName);

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

    public record RegionCheckCommandResult(List<string> AvailableRegions);
}
