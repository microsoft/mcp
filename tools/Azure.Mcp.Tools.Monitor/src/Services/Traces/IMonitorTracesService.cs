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
        string account,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits <paramref name="query"/>, or resumes polling an existing
    /// <paramref name="operationId"/>. Exactly one of the two must be supplied.
    /// </summary>
    Task<TraceQueryResult> RunQueryAsync(
        string subscription,
        string account,
        string? query,
        string? operationId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int maxResults,
        bool includeSpans,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<TraceQueryCancelResult> CancelQueryAsync(
        string subscription,
        string account,
        string operationId,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
