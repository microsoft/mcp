// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Identity;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.EventHubs.Models;
using Azure.Mcp.Tools.EventHubs.Options;
using Azure.Mcp.Tools.EventHubs.Options.EventHub;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.EventHubs.Commands.EventHub;

public sealed class EventHubUpdateCommand(ILogger<EventHubUpdateCommand> logger, IEventHubsService service)
    : BaseEventHubsCommand<EventHubUpdateOptions>
{
    private const string CommandTitle = "Create or Update Event Hub";
    private readonly IEventHubsService _service = service;
    private readonly ILogger<EventHubUpdateCommand> _logger = logger;

    public override string Name => "update";

    public override string Description =>
        """
        Create or update an event hub within an Azure Event Hubs namespace. This command can either:
        1. Create a new event hub if it doesn't exist
        2. Update an existing event hub's configuration
        
        You can configure:
        - Partition count (number of partitions for parallel processing)
        - Message retention time (how long messages are retained in hours)
        
        Note: Some properties like partition count cannot be changed after creation.
        This is a potentially long-running operation that waits for completion.
        
        Required options:
        - --eventhub (event hub name)
        - --namespace (namespace name)
        - --resource-group
        
        Optional configuration:
        - --partition-count (number of partitions, typically 1-32)
        - --message-retention-in-hours (retention time in hours)
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        OpenWorld = true,
        Destructive = true,
        Idempotent = true,
        ReadOnly = false,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.NamespaceOption.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.EventHubNameOption.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.PartitionCountOption);
        command.Options.Add(EventHubsOptionDefinitions.MessageRetentionInHoursOption);
        command.Options.Add(EventHubsOptionDefinitions.StatusOption);
    }

    protected override EventHubUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Namespace = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.Namespace) ?? string.Empty;
        options.EventHub = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.EventHubName) ?? string.Empty;
        options.PartitionCount = parseResult.GetValueOrDefault<int?>(EventHubsOptionDefinitions.PartitionCount);
        options.MessageRetentionInHours = parseResult.GetValueOrDefault<long?>(EventHubsOptionDefinitions.MessageRetentionInHours);
        options.Status = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.EventHubStatus);
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
            var eventHub = await _service.CreateOrUpdateEventHubAsync(
                options.EventHub!,
                options.Namespace!,
                options.ResourceGroup!,
                options.Subscription!,
                options.PartitionCount,
                options.MessageRetentionInHours,
                options.Status,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new EventHubUpdateCommandResult(eventHub),
                EventHubsJsonContext.Default.EventHubUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating or updating event hub. EventHub: {EventHub}, Namespace: {Namespace}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Options: {@Options}",
                options.EventHub, options.Namespace, options.ResourceGroup, options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        AuthenticationFailedException => HttpStatusCode.Unauthorized,
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        AuthenticationFailedException =>
            "Authentication failed. Please ensure your Azure credentials are properly configured and have not expired.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            "Access denied. Please ensure you have sufficient permissions to create or update Event Hubs in the specified namespace and resource group.",
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "The specified namespace, resource group, or subscription was not found. Please verify all names and identifiers.",
        RequestFailedException reqEx when reqEx.Status == 400 =>
            $"Invalid event hub configuration. Please check your parameters (partition count, retention time, etc.). Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == 409 =>
            "Conflict occurred. The event hub may be in a transitional state or the operation conflicts with another in progress.",
        ArgumentException argEx when argEx.ParamName == "eventHubName" =>
            "Invalid event hub name. Please provide a valid event hub name.",
        ArgumentException argEx when argEx.ParamName == "namespaceName" =>
            "Invalid namespace name. Please provide a valid Event Hubs namespace name.",
        ArgumentException argEx when argEx.ParamName == "resourceGroup" =>
            "Invalid resource group name. Please provide a valid resource group name.",
        ArgumentException argEx when argEx.ParamName == "subscription" =>
            "Invalid subscription. Please provide a valid subscription ID or name.",
        _ => base.GetErrorMessage(ex)
    };

    internal record EventHubUpdateCommandResult(EventHubInfo EventHub);
}
