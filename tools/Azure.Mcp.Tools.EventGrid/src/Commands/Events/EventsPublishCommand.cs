// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.EventGrid.Options;
using Azure.Mcp.Tools.EventGrid.Options.Events;
using Azure.Mcp.Tools.EventGrid.Services;

namespace Azure.Mcp.Tools.EventGrid.Commands.Events;

public sealed class EventGridPublishCommand(ILogger<EventGridPublishCommand> logger) : BaseEventGridCommand<EventsPublishOptions>
{
    private const string CommandTitle = "Publish Events to Event Grid Topic";
    private readonly ILogger<EventGridPublishCommand> _logger = logger;

    public override string Name => "publish";

    public override string Description =>
        """
        Publish custom events to Event Grid topics for event-driven architectures. This tool sends structured event data to 
        Event Grid topics with schema validation and delivery guarantees for downstream subscribers. Returns publish operation 
        status. Requires topic, data, and optional schema.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    private static readonly string[] s_item = ["cloudevents", "eventgrid", "custom"];

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup);
        command.Options.Add(EventGridOptionDefinitions.TopicName);
        command.Options.Add(EventGridOptionDefinitions.EventData);
        command.Options.Add(EventGridOptionDefinitions.EventSchema);
        command.Validators.Add(commandResult =>
        {
            var eventSchema = commandResult.GetValueOrDefault(EventGridOptionDefinitions.EventSchema);
            if (!string.IsNullOrEmpty(eventSchema))
            {
                var normalizedSchema = eventSchema.Trim().ToLowerInvariant().Replace(" ", "");
                if (!s_item.Contains(normalizedSchema))
                {
                    commandResult.AddError("Invalid event schema specified. Supported schemas are: CloudEvents, EventGrid, or Custom.");
                }
            }
        });
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
            var result = await eventGridService.PublishEventAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.TopicName!,
                options.EventData!,
                options.EventSchema,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new(result),
                EventGridJsonContext.Default.EventGridPublishCommandResult);
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
        Azure.RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Event Grid topic not found. Please verify the topic name and resource group exist.",
        Azure.RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Access denied to Event Grid topic. Please verify you have Event Grid Data Sender permissions.",
        Azure.RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            "Invalid event data or schema format. Please verify the event data is valid JSON and matches the expected schema.",
        System.Text.Json.JsonException jsonEx =>
            $"Invalid JSON format in event data: {jsonEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        System.Text.Json.JsonException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    internal record EventGridPublishCommandResult(EventPublishResult Result);
}
