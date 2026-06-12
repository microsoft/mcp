// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.EventGrid.Options;
using Azure.Mcp.Tools.EventGrid.Options.Topic;
using Azure.Mcp.Tools.EventGrid.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.EventGrid.Commands.Topic;

[CommandMetadata(
    Id = "f8a3b2c1-4d5e-6f7a-8b9c-0d1e2f3a4b5c",
    Name = "create",
    Title = "Create Event Grid Topic",
    Description = "Create an Azure Event Grid topic in a specified resource group and location. Returns the created topic's endpoint, provisioning state, and configuration details.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class TopicCreateCommand(ILogger<TopicCreateCommand> logger, IEventGridService eventGridService) : BaseEventGridCommand<TopicCreateOptions>
{
    private readonly ILogger<TopicCreateCommand> _logger = logger;
    private readonly IEventGridService _eventGridService = eventGridService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(EventGridOptionDefinitions.TopicName.AsRequired());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(EventGridOptionDefinitions.Location.AsRequired());
    }

    protected override TopicCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Topic = parseResult.GetValueOrDefault(EventGridOptionDefinitions.TopicName);
        options.ResourceGroup ??= parseResult.GetValueOrDefault(OptionDefinitions.Common.ResourceGroup);
        options.Location = parseResult.GetValueOrDefault(EventGridOptionDefinitions.Location);
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
            var topic = await _eventGridService.CreateTopicAsync(
                options.Topic!,
                options.ResourceGroup!,
                options.Location!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new TopicCreateCommandResult(topic), EventGridJsonContext.Default.TopicCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating Event Grid topic. Topic: {Topic}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
                options.Topic, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record TopicCreateCommandResult(EventGridTopicInfo? Topic);
}
