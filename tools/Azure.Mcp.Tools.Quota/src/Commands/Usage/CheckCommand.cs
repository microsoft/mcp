// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.Quota.Options;
using Azure.Mcp.Tools.Quota.Options.Usage;
using Azure.Mcp.Tools.Quota.Services;
using Azure.Mcp.Tools.Quota.Services.Util;
using Azure.Mcp.Tools.Quota.Services.Util.Usage;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Commands.Usage;

public class CheckCommand(ILogger<CheckCommand> logger) : SubscriptionCommand<CheckOptions>()
{
    private const string CommandTitle = "Check Azure resources usage and quota in a region";
    private readonly ILogger<CheckCommand> _logger = logger;

    private readonly Option<string> _regionOption = QuotaOptionDefinitions.QuotaCheck.Region;
    private readonly Option<string> _resourceTypesOption = QuotaOptionDefinitions.QuotaCheck.ResourceTypes;

    public override string Name => "check";

    public override string Description =>
        """
        This tool will check the usage and quota information for Azure resources in a region.
        """;

    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_regionOption);
        command.AddOption(_resourceTypesOption);
    }

    protected override CheckOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Region = parseResult.GetValueForOption(_regionOption) ?? string.Empty;
        options.ResourceTypes = parseResult.GetValueForOption(_resourceTypesOption) ?? string.Empty;
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

            context.Activity?
                .AddTag("Region", options.Region)
                .AddTag("ResourceTypes", options.ResourceTypes);

            var ResourceTypes = options.ResourceTypes.Split(',')
                .Select(rt => rt.Trim())
                .Where(rt => !string.IsNullOrWhiteSpace(rt))
                .ToList();
            var quotaService = context.GetService<IQuotaService>();
            Dictionary<string, List<UsageInfo>> toolResult = await quotaService.GetAzureQuotaAsync(
                ResourceTypes,
                options.Subscription!,
                options.Region);

            _logger.LogInformation("Quota check result: {ToolResult}", toolResult);

            context.Response.Results = toolResult?.Count > 0 ?
                ResponseResult.Create(
                    new UsageCheckCommandResult(toolResult),
                    QuotaJsonContext.Default.UsageCheckCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Azure resource usage");
            HandleException(context, ex);
        }
        return context.Response;

    }

    public record UsageCheckCommandResult(Dictionary<string, List<UsageInfo>> UsageInfo);

}
