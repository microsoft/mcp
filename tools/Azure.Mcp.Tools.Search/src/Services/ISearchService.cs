// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Search.Models;

namespace Azure.Mcp.Tools.Search.Services;

public interface ISearchService
{
    Task<List<string>> ListServices(
        string subscription,
        string? resourceGroup = null,
        string? tenantId = null,
        CancellationToken cancellationToken = default);

    Task<List<IndexInfo>> GetIndexDetails(
        string serviceName,
        string? indexName,
        CancellationToken cancellationToken = default);

    Task<List<KnowledgeSourceInfo>> ListKnowledgeSources(
        string serviceName,
        string? knowledgeSourceName = null,
        CancellationToken cancellationToken = default);

    Task<List<KnowledgeBaseInfo>> ListKnowledgeBases(
        string serviceName,
        string? knowledgeBaseName = null,
        CancellationToken cancellationToken = default);

    Task<List<JsonElement>> QueryIndex(
        string serviceName,
        string indexName,
        string searchText,
        CancellationToken cancellationToken = default);

    Task<string> RetrieveFromKnowledgeBase(
        string serviceName,
        string baseName,
        string? query,
        IEnumerable<(string role, string message)>? messages,
        CancellationToken cancellationToken = default);
}
