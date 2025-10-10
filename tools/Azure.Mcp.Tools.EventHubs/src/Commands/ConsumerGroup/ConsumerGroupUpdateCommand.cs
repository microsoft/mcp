// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.EventHubs.Options;
using Azure.Mcp.Tools.EventHubs.Options.ConsumerGroup;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.EventHubs.Commands.ConsumerGroup;

public sealed class ConsumerGroupUpdateCommand(ILogger<ConsumerGroupUpdateCommand> logger)
    : BaseEventHubsCommand<ConsumerGroupUpdateOptions>
{
    private const string CommandTitle = "Create or Update Event Hubs Consumer Group";

    private readonly ILogger<ConsumerGroupUpdateCommand> _logger = logger;

    public override string Name => "update";

    public override string Description =>
        """
        Create or Update a Consumer Group. This tool will either create a Consumer Group resource 
        or update a pre-existing Consumer Group resource within the specified Event Hub, depending 
        on whether or not the specified Consumer Group already exists. This tool may modify existing 
        configurations, and is considered to be destructive. 
        
        The tool requires specifying the resource group, namespace name, event hub name, and consumer 
        group name. Optionally, you can provide user metadata for the consumer group.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        OpenWorld = false,
        Destructive = true,     // Creates/modifies resources
        Idempotent = true,      // Same parameters produce same results
        ReadOnly = false,       // Modifies resources
        Secret = false,         // Returns non-sensitive information
        LocalRequired = false   // Pure cloud API calls
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.NamespaceOption.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.EventHubOption.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.ConsumerGroupOption.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.UserMetadataOption);
    }

    protected override ConsumerGroupUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Namespace = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.NamespaceOption.Name);
        options.EventHub = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.EventHubOption.Name);
        options.ConsumerGroup = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.ConsumerGroupOption.Name);
        options.UserMetadata = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.UserMetadataOption.Name);
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
            var eventHubsService = context.GetService<IEventHubsService>();

            var consumerGroup = await eventHubsService.CreateOrUpdateConsumerGroupAsync(
                options.ConsumerGroup!,
                options.EventHub!,
                options.Namespace!,
                options.ResourceGroup!,
                options.Subscription!,
                options.UserMetadata,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(consumerGroup), EventHubsJsonContext.Default.ConsumerGroupUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating consumer group");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ConsumerGroupUpdateCommandResult(Models.ConsumerGroup ConsumerGroup);
}
