// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Models.Log;

namespace Azure.Mcp.Tools.Monitor.Services;

/// <summary>
/// Searches a single Basic or Auxiliary Log Analytics table through the synchronous Logs search API.
/// </summary>
public interface IMonitorLogSearchService
{
    Task<WorkspaceLogSearchResult> SearchWorkspaceLogs(
        string subscription,
        string resourceGroup,
        string workspace,
        string table,
        string query,
        string timespan,
        int limit,
        string? tenant,
        CancellationToken cancellationToken);
}
