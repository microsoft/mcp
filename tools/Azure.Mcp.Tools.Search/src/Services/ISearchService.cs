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
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<IndexInfo>> GetIndexDetails(
        string serviceName,
        string? indexName,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<KnowledgeSourceInfo>> ListKnowledgeSources(
        string serviceName,
        string? knowledgeSourceName = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<KnowledgeBaseInfo>> ListKnowledgeBases(
        string serviceName,
        string? knowledgeBaseName = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<JsonElement>> QueryIndex(
        string serviceName,
        string indexName,
        string searchText,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<string> RetrieveFromKnowledgeBase(
        string serviceName,
        string baseName,
        string? query,
        IEnumerable<(string role, string message)>? messages,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
