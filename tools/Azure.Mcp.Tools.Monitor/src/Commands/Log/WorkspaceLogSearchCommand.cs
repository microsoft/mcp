// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models.Log;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Log;

[CommandMetadata(
    Id = "f65f15c4-edf2-47f0-af56-0db47e4e4f74",
    Name = "search",
    Title = "Search Basic or Auxiliary Workspace Logs",
    Description = """
        Searches one Basic or Auxiliary table in an Azure Log Analytics workspace through the synchronous Logs search API.
        Requires resourceGroup, workspace, table, a KQL pipeline fragment beginning with '|', and an explicit timespan of at most 30 days.
        The server binds the table and appends a final take from limit (default 20, maximum 100). Returns typed columns and rows and explicitly marks partial results.
        Scan cost is based on table ingestion volume across the timespan, not limit. Use monitor_workspace_log_query for Analytics-plan tables.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class WorkspaceLogSearchCommand(
    ILogger<WorkspaceLogSearchCommand> logger,
    IMonitorLogSearchService logSearchService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<WorkspaceLogSearchOptions, WorkspaceLogSearchResult>(subscriptionResolver)
{
    private readonly ILogger<WorkspaceLogSearchCommand> _logger = logger;
    private readonly IMonitorLogSearchService _logSearchService = logSearchService;

    public override JsonTypeInfo<WorkspaceLogSearchResult>? ResultTypeInfo =>
        MonitorJsonContext.Default.WorkspaceLogSearchResult;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        WorkspaceLogSearchOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _logSearchService.SearchWorkspaceLogs(
                options.Subscription!,
                options.ResourceGroup,
                options.Workspace,
                options.Table,
                options.Query,
                options.Timespan,
                options.Limit,
                options.Tenant,
                cancellationToken);

            SetResult(context, result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (CommandValidationException ex)
        {
            HandleException(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Workspace log search failed. ResourceGroup: {ResourceGroup}, Workspace: {Workspace}, Table: {Table}",
                options.ResourceGroup,
                options.Workspace,
                options.Table);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
