// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.EventGrid.Options.Topic;
using Azure.Mcp.Tools.EventGrid.Services;

namespace Azure.Mcp.Tools.EventGrid.Commands.Topic;

public sealed class TopicListCommand(ILogger<TopicListCommand> logger) : BaseEventGridCommand<TopicListOptions>
{
    private const string CommandTitle = "List Event Grid Topics";
    private readonly ILogger<TopicListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        """
        List all Event Grid topics in a subscription or a specific subscription with detailed configuration and status information. 
        Can filter by resource group to show topics in a specific resource group. This tool retrieves topic details including 
        endpoints, access keys, provisioning state, and subscription information for event publishing and management. Shows Event Grid 
        topic details and configuration in JSON format. Use this to show, list, or get Event Grid topics. Requires subscription and
        returns topic information as a structured array.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup);
    }

    protected override TopicListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
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
            var topics = await eventGridService.GetTopicsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(topics ?? []), EventGridJsonContext.Default.TopicListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing Event Grid topics. Subscription: {Subscription}, Options: {@Options}",
                options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record TopicListCommandResult(List<EventGridTopicInfo> Topics);
}
