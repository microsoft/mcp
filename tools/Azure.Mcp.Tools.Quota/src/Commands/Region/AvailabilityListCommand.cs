// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
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
        command.Options.Add(_resourceTypesOption);
        command.Options.Add(_cognitiveServiceModelNameOption);
        command.Options.Add(_cognitiveServiceModelVersionOption);
        command.Options.Add(_cognitiveServiceDeploymentSkuNameOption);
    }

    protected override AvailabilityListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceTypes = parseResult.GetValue(_resourceTypesOption) ?? string.Empty;
        options.CognitiveServiceModelName = parseResult.GetValue(_cognitiveServiceModelNameOption);
        options.CognitiveServiceModelVersion = parseResult.GetValue(_cognitiveServiceModelVersionOption);
        options.CognitiveServiceDeploymentSkuName = parseResult.GetValue(_cognitiveServiceDeploymentSkuNameOption);
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
