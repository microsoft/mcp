// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Search.Models;

namespace Azure.Mcp.Tools.Search.Services;

public interface ISearchService
{
    Task<List<string>> ListServices(
        string subscription,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<IndexInfo>> GetIndexDetails(
        string serviceName,
        string? indexName,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<JsonElement>> QueryIndex(
        string serviceName,
        string indexName,
        string searchText,
        RetryPolicyOptions? retryPolicy = null);
}
