// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Services.Telemetry;
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

    private readonly Option<string> _resourceTypesOption = QuotaOptionDefinitions.RegionCheck.ResourceTypes;
    private readonly Option<string> _cognitiveServiceModelNameOption = QuotaOptionDefinitions.RegionCheck.CognitiveServiceModelName;
    private readonly Option<string> _cognitiveServiceModelVersionOption = QuotaOptionDefinitions.RegionCheck.CognitiveServiceModelVersion;
    private readonly Option<string> _cognitiveServiceDeploymentSkuNameOption = QuotaOptionDefinitions.RegionCheck.CognitiveServiceDeploymentSkuName;

    public override string Name => "list";

    public override string Description =>
        """
        Given a list of Azure resource types, this tool will return a list of regions where the resource types are available. Always get the user's subscription ID before calling this tool.
        """;

    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_resourceTypesOption);
        command.AddOption(_cognitiveServiceModelNameOption);
        command.AddOption(_cognitiveServiceModelVersionOption);
        command.AddOption(_cognitiveServiceDeploymentSkuNameOption);
    }

    protected override AvailabilityListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceTypes = parseResult.GetValueForOption(_resourceTypesOption) ?? string.Empty;
        options.CognitiveServiceModelName = parseResult.GetValueForOption(_cognitiveServiceModelNameOption);
        options.CognitiveServiceModelVersion = parseResult.GetValueForOption(_cognitiveServiceModelVersionOption);
        options.CognitiveServiceDeploymentSkuName = parseResult.GetValueForOption(_cognitiveServiceDeploymentSkuNameOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

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

            context.Response.Results = toolResult?.Count > 0 ?
                ResponseResult.Create(
                    new RegionCheckCommandResult(toolResult),
                    QuotaJsonContext.Default.RegionCheckCommandResult) :
                null;
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
