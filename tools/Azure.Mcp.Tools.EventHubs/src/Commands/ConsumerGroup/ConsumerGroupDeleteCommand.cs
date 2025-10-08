// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.EventHubs.Commands;
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
        
        The tool requires specifying the resource group, namespace name, event hub name, and consumer 
        group name to identify the consumer group to delete.
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
        command.Options.Add(EventHubsOptionDefinitions.NamespaceName.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.EventHubNameOption.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.ConsumerGroupNameOption.AsRequired());
    }

    protected override ConsumerGroupDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.NamespaceName = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.NamespaceName.Name) ?? string.Empty;
        options.EventHubName = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.EventHubNameOption.Name) ?? string.Empty;
        options.ConsumerGroupName = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.ConsumerGroupNameOption.Name) ?? string.Empty;
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
                options.ConsumerGroupName,
                options.EventHubName,
                options.NamespaceName,
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(deleted, options.ConsumerGroupName, options.EventHubName, options.NamespaceName, options.ResourceGroup!), EventHubsJsonContext.Default.ConsumerGroupDeleteCommandResult);
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
