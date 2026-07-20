// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureIsv.Options.Datadog;
using Azure.Mcp.Tools.AzureIsv.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureIsv.Commands.Datadog;

[CommandMetadata(
    Id = "bbd026b6-df96-4c52-8b72-13734984a600",
    Name = "list",
    Title = "List Monitored Resources in a Datadog Monitor",
    Description = """
        List monitored resources in Datadog for a datadog resource taken as input from the user.
        This command retrieves all monitored azure resources available.
        Requires `datadog-resource`, `resource-group` and `subscription`.
        Result is a list of monitored resources as a JSON array.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class MonitoredResourcesListCommand(ILogger<MonitoredResourcesListCommand> logger, IDatadogService datadogService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<MonitoredResourcesListOptions, MonitoredResourcesListCommand.MonitoredResourcesListResult>(subscriptionResolver)
{
    private readonly ILogger<MonitoredResourcesListCommand> _logger = logger;
    private readonly IDatadogService _datadogService = datadogService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, MonitoredResourcesListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var results = await _datadogService.ListMonitoredResources(
                options.ResourceGroup,
                options.Subscription!,
                options.DatadogResource,
                cancellationToken);
            context.Response.Results = results?.Count > 0
                ? ResponseResult.Create(new(results), DatadogJsonContext.Default.MonitoredResourcesListResult)
                : ResponseResult.Create(new(["No monitored resources found for the specified Datadog resource."]), DatadogJsonContext.Default.MonitoredResourcesListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing the command.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record MonitoredResourcesListResult(List<string> Resources);
}
