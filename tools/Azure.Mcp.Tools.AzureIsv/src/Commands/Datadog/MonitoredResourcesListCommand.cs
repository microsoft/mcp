// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.AzureIsv.Options;
using Azure.Mcp.Tools.AzureIsv.Options.Datadog;
using Azure.Mcp.Tools.AzureIsv.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureIsv.Commands.Datadog;

public sealed class MonitoredResourcesListCommand(ILogger<MonitoredResourcesListCommand> logger) : SubscriptionCommand<MonitoredResourcesListOptions>
{
    private const string _commandTitle = "List Monitored Resources in a Datadog Monitor";
    private readonly ILogger<MonitoredResourcesListCommand> _logger = logger;
    private readonly Option<string> _datadogResourceOption = DatadogOptionDefinitions.DatadogResourceName;

    public override string Name => "list";

    public override string Description =>
        """
        List monitored resources in Datadog for a datadog resource taken as input from the user.
        This command retrieves all monitored azure resources available.
        Requires `datadog-resource`, `resource-group` and `subscription`.
        Result is a list of monitored resources as a JSON array.
        """;

    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_datadogResourceOption);
        RequireResourceGroup();
    }

    protected override MonitoredResourcesListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DatadogResource = parseResult.GetValueForOption(_datadogResourceOption);
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

            var service = context.GetService<IDatadogService>();
            List<string> results = await service.ListMonitoredResources(
                options.ResourceGroup!,
                options.Subscription!,
                options.DatadogResource!);
            context.Response.Results = results?.Count > 0
                ? ResponseResult.Create(new MonitoredResourcesListResult(results), DatadogJsonContext.Default.MonitoredResourcesListResult)
                : ResponseResult.Create(new MonitoredResourcesListResult([
                    "No monitored resources found for the specified Datadog resource."]), DatadogJsonContext.Default.MonitoredResourcesListResult);
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
