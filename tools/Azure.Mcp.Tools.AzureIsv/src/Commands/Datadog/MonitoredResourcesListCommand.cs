// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.AzureIsv.Options;
using Azure.Mcp.Tools.AzureIsv.Options.Datadog;
using Azure.Mcp.Tools.AzureIsv.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

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
public sealed class MonitoredResourcesListCommand(ILogger<MonitoredResourcesListCommand> logger, IDatadogService datadogService) : SubscriptionCommand<MonitoredResourcesListOptions>
{
    private readonly ILogger<MonitoredResourcesListCommand> _logger = logger;
    private readonly IDatadogService _datadogService = datadogService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DatadogOptionDefinitions.DatadogResourceName);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
    }

    protected override MonitoredResourcesListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.DatadogResource = parseResult.GetValueOrDefault<string>(DatadogOptionDefinitions.DatadogResourceName.Name);
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
            var results = await _datadogService.ListMonitoredResources(
                options.ResourceGroup!,
                options.Subscription!,
                options.DatadogResource!,
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

    internal record MonitoredResourcesListResult(List<string> resources);
}
