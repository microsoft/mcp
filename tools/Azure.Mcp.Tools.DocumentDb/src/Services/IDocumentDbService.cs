// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.DocumentDb.Models;
using MongoDB.Bson;

namespace Azure.Mcp.Tools.DocumentDb.Services;

public interface IDocumentDbService
{
    Task<DocumentDbResponse> CreateIndexAsync(string connectionString, string databaseName, string collectionName, BsonDocument keys, BsonDocument? options = null, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> ListIndexesAsync(string connectionString, string databaseName, string collectionName, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> DropIndexAsync(string connectionString, string databaseName, string collectionName, string indexName, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> GetIndexStatsAsync(string connectionString, string databaseName, string collectionName, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> GetCurrentOpsAsync(string connectionString, BsonDocument? filter = null, CancellationToken cancellationToken = default);
}
