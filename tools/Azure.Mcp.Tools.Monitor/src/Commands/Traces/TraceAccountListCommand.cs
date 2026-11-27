// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Options.Traces;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Traces;

[CommandMetadata(
    Id = "8c1bb1ec-2cf4-4d2e-9e17-d0f66b5eb1fd",
    Name = "list",
    Title = "List Distributed Trace Accounts",
    Description = """
        List distributed-trace accounts available in a subscription. A trace
        account is an Azure Monitor Workspace with a Microsoft.Monitor/accounts/traceContainers
        sub-resource — i.e. one where Trace Store has been enabled. Returns each account's
        name, resource id, resource group, region, and trace-retention window. Use the
        returned name and resource group as --workspace and --resource-group for
        `monitor traces query run` and `monitor traces field list`.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TraceAccountListCommand(
    ILogger<TraceAccountListCommand> logger,
    IMonitorTracesService tracesService,
    ISubscriptionResolver subscriptionResolver)
    : BaseTraceCommand<TraceAccountListOptions, TraceAccountListCommand.TraceAccountListCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        TraceAccountListOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var accounts = await tracesService.ListAccountsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new TraceAccountListCommandResult(accounts.Results, accounts.AreResultsTruncated),
                MonitorJsonContext.Default.TraceAccountListCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing trace accounts. Subscription: {Subscription}", options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record TraceAccountListCommandResult(List<TraceAccountInfo> Accounts, bool AreResultsTruncated);
}
