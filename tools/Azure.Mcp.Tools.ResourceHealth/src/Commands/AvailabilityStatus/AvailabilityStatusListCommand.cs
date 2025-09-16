// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ResourceHealth.Options.AvailabilityStatus;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ResourceHealth.Commands.AvailabilityStatus;

/// <summary>
/// Lists availability statuses for all resources in a subscription or resource group.
/// </summary>
public sealed class AvailabilityStatusListCommand(ILogger<AvailabilityStatusListCommand> logger)
    : BaseResourceHealthCommand<AvailabilityStatusListOptions>()
{
    private const string CommandTitle = "List Resource Availability Statuses";
    private readonly ILogger<AvailabilityStatusListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        $"""
        List availability statuses for all resources in a subscription or resource group.
        Provides health status information for multiple Azure resources at once, including availability state,
        summaries, and timestamps. This is useful for getting an overview of resource health across your infrastructure.
        Results can be filtered by resource group to narrow the scope.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override AvailabilityStatusListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
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
            var resourceHealthService = context.GetService<IResourceHealthService>() ??
                throw new InvalidOperationException("Resource Health service is not available.");

            var statuses = await resourceHealthService.ListAvailabilityStatusesAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = statuses?.Count > 0
                ? ResponseResult.Create(
                    new AvailabilityStatusListCommandResult(statuses),
                    ResourceHealthJsonContext.Default.AvailabilityStatusListCommandResult)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list availability statuses for subscription {Subscription}{ResourceGroupInfo}",
                options.Subscription,
                options.ResourceGroup != null ? $" and resource group {options.ResourceGroup}" : "");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AvailabilityStatusListCommandResult(List<Models.AvailabilityStatus> Statuses);
}
