// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.EventGrid.Models;
using Azure.Mcp.Tools.EventGrid.Options.Events;
using Azure.Mcp.Tools.EventGrid.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.EventGrid.Commands.Events;

[CommandMetadata(
    Id = "d5f216a4-c45e-4c29-a414-d3feaa5929e2",
    Name = "publish",
    Title = "Publish Events to Event Grid Topic",
    Description = """
        Publish custom events to Event Grid topics for event-driven architectures. This tool sends structured event data to 
        Event Grid topics with schema validation and delivery guarantees for downstream subscribers. Returns publish operation 
        status. Requires topic, data, and optional schema.
        """,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class EventGridPublishCommand(ILogger<EventGridPublishCommand> logger, IEventGridService eventGridService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<EventsPublishOptions, EventGridPublishCommand.EventGridPublishCommandResult>(subscriptionResolver)
{
    private readonly ILogger<EventGridPublishCommand> _logger = logger;
    private readonly IEventGridService _eventGridService = eventGridService;

    private static readonly string[] s_item = ["cloudevents", "eventgrid", "custom"];

    public override void ValidateOptions(EventsPublishOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.IsNullOrEmpty(options.Schema) &&
            !s_item.Contains(options.Schema.Trim().ToLowerInvariant().Replace(" ", "")))
        {
            validationResult.Errors.Add("Invalid event schema specified. Supported schemas are: CloudEvents, EventGrid, or Custom.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, EventsPublishOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _eventGridService.PublishEventAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Topic,
                options.Data,
                options.Schema,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                EventGridJsonContext.Default.EventGridPublishCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing events to Event Grid topic. Subscription: {Subscription}, Topic: {TopicName}.",
                options.Subscription, options.Topic);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Event Grid topic not found. Please verify the topic name and resource group exist.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Access denied to Event Grid topic. Please verify you have Event Grid Data Sender permissions.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
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

    public sealed record EventGridPublishCommandResult(EventPublishResult Result);
}
