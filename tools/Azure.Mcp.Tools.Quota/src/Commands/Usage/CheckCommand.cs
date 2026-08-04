// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Quota.Models;
using Azure.Mcp.Tools.Quota.Options.Usage;
using Azure.Mcp.Tools.Quota.Services;
using Azure.Mcp.Tools.Quota.Services.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Quota.Commands.Usage;

[CommandMetadata(
    Id = "81f64603-5a56-4f74-90f8-395da69a99d3",
    Name = "check",
    Title = "Check Azure resources usage and quota in a region",
    Description = "This tool will check the usage and quota information for Azure resources in a region.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class CheckCommand(ILogger<CheckCommand> logger, IQuotaService quotaService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CheckOptions, CheckCommand.UsageCheckCommandResult>(subscriptionResolver)
{
    private readonly ILogger<CheckCommand> _logger = logger;
    private readonly IQuotaService _quotaService = quotaService;

    public override void ValidateOptions(CheckOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (string.IsNullOrWhiteSpace(options.ResourceTypes) || !options.ResourceTypes.Split(',').Any(rt => !string.IsNullOrWhiteSpace(rt)))
        {
            validationResult.Errors.Add("Resource types cannot be empty.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CheckOptions options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?
                .AddTag(QuotaTelemetryTags.Region, options.Region)
                .AddTag(QuotaTelemetryTags.ResourceTypes, options.ResourceTypes);

            var resourceTypes = options.ResourceTypes.Split(',')
                .Select(rt => rt.Trim())
                .Where(rt => !string.IsNullOrWhiteSpace(rt))
                .ToList();
            Dictionary<string, List<UsageInfo>> toolResult = await _quotaService.GetAzureQuotaAsync(
                resourceTypes,
                options.Subscription!,
                options.Region,
                cancellationToken);

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

    public sealed record UsageCheckCommandResult(Dictionary<string, List<UsageInfo>> UsageInfo);
}
