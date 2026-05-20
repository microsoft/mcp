// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.EventHubs.Options;
using Azure.Mcp.Tools.EventHubs.Options.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.EventHubs.Commands.Namespace;

[CommandMetadata(
    Id = "187ffc25-1e32-4e39-a7d4-94859852ac50",
    Name = "delete",
    Title = "Delete Event Hubs Namespace",
    Description = """
        Delete Event Hubs namespace. This tool will delete a pre-existing Namespace from the 
        specified resource group. This tool will remove existing configurations, and is 
        considered to be destructive.

        WARNING: This operation is irreversible. All Event Hubs, Consumer Groups, and
        configurations within the namespace will be permanently deleted.

        The namespace must exist in the specified resource group. If the namespace is not found,
        an error will be returned.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class NamespaceDeleteCommand(ILogger<NamespaceDeleteCommand> logger, IEventHubsService service)
    : BaseEventHubsCommand<NamespaceDeleteOptions>
{

    private readonly IEventHubsService _service = service;
    private readonly ILogger<NamespaceDeleteCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.NamespaceOption.AsRequired());
    }

    protected override NamespaceDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Namespace = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.NamespaceOption.Name);
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
            var success = await _service.DeleteNamespaceAsync(
                options.Namespace!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(success, $"Namespace '{options.Namespace}' deleted successfully"),
                EventHubsJsonContext.Default.NamespaceDeleteCommandResult);
            context.Response.Status = HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Event Hubs namespace '{NamespaceName}' from resource group '{ResourceGroup}'",
                options.Namespace, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record NamespaceDeleteCommandResult(bool Success, string Message);
}
