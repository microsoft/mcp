// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Identity;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
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
        Create or update an Event Hub within an Azure Event Hubs namespace. This command can either:
        1. Create a new Event Hub if it doesn't exist
        2. Update an existing Event Hub's configuration

        You can configure:
        - Partition count (number of partitions for parallel processing)
        - Message retention time (how long messages are retained in hours)
        
        Note: Some properties like partition count cannot be changed after creation.
        This is a potentially long-running operation that waits for completion.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        OpenWorld = false,
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
        command.Options.Add(EventHubsOptionDefinitions.EventHubOption.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.PartitionCountOption);
        command.Options.Add(EventHubsOptionDefinitions.MessageRetentionInHoursOption);
        command.Options.Add(EventHubsOptionDefinitions.StatusOption);
    }

    protected override EventHubUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Namespace = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.NamespaceOption.Name) ?? string.Empty;
        options.EventHub = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.EventHubOption.Name) ?? string.Empty;
        options.PartitionCount = parseResult.GetValueOrDefault<int?>(EventHubsOptionDefinitions.PartitionCountOption.Name);
        options.MessageRetentionInHours = parseResult.GetValueOrDefault<long?>(EventHubsOptionDefinitions.MessageRetentionInHoursOption.Name);
        options.Status = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.StatusOption.Name);
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
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new(eventHub),
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

    internal record EventHubUpdateCommandResult(Models.EventHub EventHub);
}
