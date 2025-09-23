// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.EventGrid.Options;
using Azure.Mcp.Tools.EventGrid.Options.Events;
using Azure.Mcp.Tools.EventGrid.Services;

namespace Azure.Mcp.Tools.EventGrid.Commands.Events;

public sealed class EventsPublishCommand(ILogger<EventsPublishCommand> logger) : BaseEventGridCommand<EventsPublishOptions>
{
    private const string CommandTitle = "Publish Events to Event Grid Topic";
    private readonly ILogger<EventsPublishCommand> _logger = logger;

    public override string Name => "publish";

    public override string Description =>
        """
        Publish custom events to Event Grid topics for event-driven architectures. This tool sends structured event data to 
        Event Grid topics with schema validation and delivery guarantees for downstream subscribers. Returns publish operation 
        status. Requires topic, event-data, and optional event-schema.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(EventGridOptionDefinitions.TopicName.AsRequired());
        command.Options.Add(EventGridOptionDefinitions.EventData.AsRequired());
        command.Options.Add(EventGridOptionDefinitions.EventSchema.AsOptional());
    }

    protected override EventsPublishOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.TopicName = parseResult.GetValueOrDefault<string>(EventGridOptionDefinitions.TopicName.Name);
        options.EventData = parseResult.GetValueOrDefault<string>(EventGridOptionDefinitions.EventData.Name);
        options.EventSchema = parseResult.GetValueOrDefault<string>(EventGridOptionDefinitions.EventSchema.Name);
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
            var eventGridService = context.GetService<IEventGridService>();
            var result = await eventGridService.PublishEventsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.TopicName!,
                options.EventData!,
                options.EventSchema,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new EventsPublishCommandResult(result),
                EventGridJsonContext.Default.EventsPublishCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing events to Event Grid topic. Subscription: {Subscription}, Topic: {TopicName}, Options: {@Options}",
                options.Subscription, options.TopicName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
            "Event Grid topic not found. Please verify the topic name and resource group exist.",
        Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
            "Access denied to Event Grid topic. Please verify you have Event Grid Data Sender permissions.",
        Azure.RequestFailedException reqEx when reqEx.Status == 400 =>
            "Invalid event data or schema format. Please verify the event data is valid JSON and matches the expected schema.",
        ArgumentException argEx when argEx.Message.Contains("schema") =>
            "Invalid event schema specified. Supported schemas are: CloudEvents, EventGridEvent, or Custom.",
        System.Text.Json.JsonException jsonEx =>
            $"Invalid JSON format in event data: {jsonEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => reqEx.Status,
        ArgumentException => 400,
        System.Text.Json.JsonException => 400,
        _ => base.GetStatusCode(ex)
    };

    internal record EventsPublishCommandResult(EventPublishResult Result);
}
