// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ResourceHealth.Options.AvailabilityStatus;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ResourceHealth.Commands.AvailabilityStatus;

/// <summary>
/// Gets or lists availability status information for Azure resources.
/// </summary>
public sealed class AvailabilityStatusGetCommand(ILogger<AvailabilityStatusGetCommand> logger)
    : BaseResourceHealthCommand<AvailabilityStatusGetOptions>()
{
    private const string CommandTitle = "Get/List Resource Availability Status";
    private readonly ILogger<AvailabilityStatusGetCommand> _logger = logger;

    public override string Id => "3b388cc7-4b16-4919-9e90-f592247d9891";

    public override string Name => "get";

    public override string Description =>
        "Get availability status and health status for Azure resources. Shows the health status of a specific virtual machine, storage account, or other resource. Lists availability status for all resources in a subscription or resource group to identify health issues and availability problems.";

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
        command.Options.Add(ResourceHealthOptionDefinitions.ResourceId.AsOptional());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override AvailabilityStatusGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceId = parseResult.GetValueOrDefault<string>(ResourceHealthOptionDefinitions.ResourceId.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var resourceHealthService = context.GetService<IResourceHealthService>() ??
                throw new InvalidOperationException("Resource Health service is not available.");

            List<Models.AvailabilityStatus> statuses;

            // If resourceId is provided, get single resource status
            if (!string.IsNullOrEmpty(options.ResourceId))
            {
                var status = await resourceHealthService.GetAvailabilityStatusAsync(
                    options.ResourceId,
                    options.RetryPolicy,
                    cancellationToken);

                statuses = [status];
            }
            // Otherwise, list all resources
            else
            {
                statuses = await resourceHealthService.ListAvailabilityStatusesAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken) ?? [];
            }

            context.Response.Results = ResponseResult.Create(
                new AvailabilityStatusGetCommandResult(statuses),
                ResourceHealthJsonContext.Default.AvailabilityStatusGetCommandResult);
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrEmpty(options.ResourceId))
            {
                _logger.LogError(ex, "Failed to get availability status for resource {ResourceId}", options.ResourceId);
            }
            else
            {
                _logger.LogError(ex, "Failed to list availability statuses for subscription {Subscription}{ResourceGroupInfo}",
                    options.Subscription,
                    options.ResourceGroup != null ? $" and resource group {options.ResourceGroup}" : "");
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AvailabilityStatusGetCommandResult(List<Models.AvailabilityStatus> Statuses);
}
