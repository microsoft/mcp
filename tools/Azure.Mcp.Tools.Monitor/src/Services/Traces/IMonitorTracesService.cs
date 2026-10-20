// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Monitor.Models.Traces;

namespace Azure.Mcp.Tools.Monitor.Services.Traces;

public interface IMonitorTracesService
{
    Task<ResourceQueryResults<TraceAccountInfo>> ListAccountsAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<TraceFieldNames> ListFieldNamesAsync(
        string subscription,
        string resourceGroup,
        string workspace,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits a new query and collects results within the polling and row budgets.
    /// </summary>
    Task<TraceQueryResult> RunQueryAsync(
        string subscription,
        string resourceGroup,
        string workspace,
        string query,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int maxResults,
        bool includeSpans,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Polls an existing operation without resubmitting its query or time range.
    /// </summary>
    Task<TraceQueryResult> ResumeQueryAsync(
        string subscription,
        string resourceGroup,
        string workspace,
        string operationId,
        int pageSize = 100,
        bool includeSpans = true,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<TraceQueryCancelResult> CancelQueryAsync(
        string subscription,
        string resourceGroup,
        string workspace,
        string operationId,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
