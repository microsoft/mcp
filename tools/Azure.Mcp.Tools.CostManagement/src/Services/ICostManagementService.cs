// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.CostManagement.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.CostManagement.Services;

public interface ICostManagementService
{
    /// <summary>
    /// Queries actual Azure costs for a subscription, optionally narrowed to a resource group,
    /// using the Azure Cost Management Query/Usage API.
    /// </summary>
    Task<CostQueryResult> QueryAsync(
        string subscription,
        string? resourceGroup,
        QueryTimeframe timeframe,
        DateTime? from,
        DateTime? to,
        QueryGranularity granularity,
        string? groupBy,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default);
}
