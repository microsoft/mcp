// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.EventHubs.Options.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

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
public sealed class NamespaceDeleteCommand(ILogger<NamespaceDeleteCommand> logger, IEventHubsService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<NamespaceDeleteOptions, NamespaceDeleteCommand.NamespaceDeleteCommandResult>(subscriptionResolver)
{
    private readonly IEventHubsService _service = service;
    private readonly ILogger<NamespaceDeleteCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, NamespaceDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _service.DeleteNamespaceAsync(
                options.Namespace,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var message = success
                ? $"Namespace '{options.Namespace}' deleted successfully."
                : $"Namespace '{options.Namespace}' was not found. Nothing was deleted.";
            context.Response.Results = ResponseResult.Create(
                new(success, message),
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

    public sealed record NamespaceDeleteCommandResult(bool Success, string Message);
}
