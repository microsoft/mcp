// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Search.Models;
using static Azure.Mcp.Tools.Search.Commands.Index.IndexDescribeCommand;

namespace Azure.Mcp.Tools.Search.Services;

public interface ISearchService
{
    Task<List<string>> ListServices(
        string subscription,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<IndexInfo>> ListIndexes(
        string serviceName,
        RetryPolicyOptions? retryPolicy = null);

    Task<SearchIndexProxy?> DescribeIndex(
        string serviceName,
        string indexName,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<JsonElement>> QueryIndex(
        string serviceName,
        string indexName,
        string searchText,
        RetryPolicyOptions? retryPolicy = null);
}
