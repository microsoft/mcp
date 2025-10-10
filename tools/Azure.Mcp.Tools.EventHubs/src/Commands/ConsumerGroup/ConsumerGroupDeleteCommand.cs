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

public sealed class ConsumerGroupDeleteCommand(ILogger<ConsumerGroupDeleteCommand> logger)
    : BaseEventHubsCommand<ConsumerGroupDeleteOptions>
{
    private const string CommandTitle = "Delete Event Hubs Consumer Group";

    private readonly ILogger<ConsumerGroupDeleteCommand> _logger = logger;

    public override string Name => "delete";

    public override string Description =>
        """
        Delete a Consumer Group. This tool will delete a pre-existing Consumer Group from the specified 
        Event Hub. This tool will remove existing configurations, and is considered to be destructive.

        The tool requires specifying the resource group, Namespace name, Event Hub name, and Consumer
        Group name to identify the Consumer Group to delete.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        OpenWorld = false,
        Destructive = true,     // Deletes resources
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
    }

    protected override ConsumerGroupDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Namespace = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.NamespaceOption.Name);
        options.EventHub = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.EventHubOption.Name);
        options.ConsumerGroup = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.ConsumerGroupOption.Name);
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

            var deleted = await eventHubsService.DeleteConsumerGroupAsync(
                options.ConsumerGroup!,
                options.EventHub!,
                options.Namespace!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(deleted, options.ConsumerGroup!, options.EventHub!, options.Namespace!, options.ResourceGroup!), EventHubsJsonContext.Default.ConsumerGroupDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting consumer group");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ConsumerGroupDeleteCommandResult(bool Deleted, string ConsumerGroupName, string EventHubName, string NamespaceName, string ResourceGroup);
}
