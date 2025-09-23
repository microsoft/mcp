// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Quota.Models;
using Azure.Mcp.Tools.Quota.Options;
using Azure.Mcp.Tools.Quota.Options.Usage;
using Azure.Mcp.Tools.Quota.Services;
using Azure.Mcp.Tools.Quota.Services.Util;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Commands.Usage;

public class CheckCommand(ILogger<CheckCommand> logger) : SubscriptionCommand<CheckOptions>()
{
    private const string CommandTitle = "Check Azure resources usage and quota in a region";
    private readonly ILogger<CheckCommand> _logger = logger;

    public override string Name => "check";

    public override string Description =>
        """
        This tool will check the usage and quota information for Azure resources in a region.
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
        command.Options.Add(QuotaOptionDefinitions.QuotaCheck.Region);
        command.Options.Add(QuotaOptionDefinitions.QuotaCheck.ResourceTypes);
    }

    protected override CheckOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Region = parseResult.GetValueOrDefault<string>(QuotaOptionDefinitions.QuotaCheck.Region.Name) ?? string.Empty;
        options.ResourceTypes = parseResult.GetValueOrDefault<string>(QuotaOptionDefinitions.QuotaCheck.ResourceTypes.Name) ?? string.Empty;
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
            context.Activity?
                .AddTag(QuotaTelemetryTags.Region, options.Region)
                .AddTag(QuotaTelemetryTags.ResourceTypes, options.ResourceTypes);

            var resourceTypes = options.ResourceTypes.Split(',')
                .Select(rt => rt.Trim())
                .Where(rt => !string.IsNullOrWhiteSpace(rt))
                .ToList();
            var quotaService = context.GetService<IQuotaService>();
            Dictionary<string, List<UsageInfo>> toolResult = await quotaService.GetAzureQuotaAsync(
                resourceTypes,
                options.Subscription!,
                options.Region);

            _logger.LogInformation("Quota check result: {ToolResult}", toolResult);

            context.Response.Results = ResponseResult.Create(new(toolResult ?? []), QuotaJsonContext.Default.UsageCheckCommandResult);
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
