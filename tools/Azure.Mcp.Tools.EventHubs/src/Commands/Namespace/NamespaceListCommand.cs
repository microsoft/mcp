// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.EventHubs.Commands;
using Azure.Mcp.Tools.EventHubs.Options.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.EventHubs.Commands.Namespace;

public sealed class NamespaceListCommand(ILogger<NamespaceListCommand> logger) 
    : BaseEventHubsCommand<NamespaceListOptions>(logger)
{
    private const string CommandTitle = "List EventHubs Namespaces";

    public override string Name => "list";

    public override string Description =>
        """
        List all EventHubs namespaces in a resource group. This command retrieves all EventHubs namespaces 
        available in the specified resource group and subscription. Results are returned as a JSON array 
        of objects containing the name, id, and resource group of each namespace.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RequireResourceGroup(); // Resource group is required for this command
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var eventHubsService = context.GetService<IEventHubsService>();
            var namespaces = await eventHubsService.ListNamespacesAsync(
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = namespaces?.Count > 0
                ? ResponseResult.Create(new NamespaceListCommandResult(namespaces), EventHubsJsonContext.Default.NamespaceListCommandResult)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing EventHubs namespaces");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record NamespaceListCommandResult(List<Models.EventHubsNamespaceInfo> Namespaces);
}