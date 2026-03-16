// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.DocumentDb.Models;
using MongoDB.Bson;

namespace Azure.Mcp.Tools.DocumentDb.Services;

public interface IDocumentDbService
{
    // Index Management
    Task<DocumentDbResponse> CreateIndexAsync(string connectionString, string databaseName, string collectionName, BsonDocument keys, BsonDocument? options = null, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> ListIndexesAsync(string connectionString, string databaseName, string collectionName, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> DropIndexAsync(string connectionString, string databaseName, string collectionName, string indexName, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> GetIndexStatsAsync(string connectionString, string databaseName, string collectionName, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> GetCurrentOpsAsync(string connectionString, BsonDocument? filter = null, CancellationToken cancellationToken = default);

    // Database Management
    Task<DocumentDbResponse> GetDatabasesAsync(string connectionString, string? dbName = null, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> GetDatabaseStatsAsync(string connectionString, string dbName, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> DropDatabaseAsync(string connectionString, string dbName, CancellationToken cancellationToken = default);

    // Collection Operations
    Task<DocumentDbResponse> GetCollectionStatsAsync(string connectionString, string databaseName, string collectionName, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> RenameCollectionAsync(string connectionString, string databaseName, string oldName, string newName, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> DropCollectionAsync(string connectionString, string databaseName, string collectionName, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> SampleDocumentsAsync(string connectionString, string databaseName, string collectionName, int sampleSize = 10, CancellationToken cancellationToken = default);

    // Document Operations
    Task<DocumentDbResponse> FindDocumentsAsync(string connectionString, string databaseName, string collectionName, BsonDocument? filter = null, BsonDocument? options = null, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> CountDocumentsAsync(string connectionString, string databaseName, string collectionName, BsonDocument? filter = null, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> InsertDocumentAsync(string connectionString, string databaseName, string collectionName, BsonDocument document, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> InsertManyAsync(string connectionString, string databaseName, string collectionName, List<BsonDocument> documents, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> UpdateDocumentAsync(string connectionString, string databaseName, string collectionName, BsonDocument filter, BsonDocument update, bool upsert = false, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> UpdateManyAsync(string connectionString, string databaseName, string collectionName, BsonDocument filter, BsonDocument update, bool upsert = false, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> DeleteDocumentAsync(string connectionString, string databaseName, string collectionName, BsonDocument filter, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> DeleteManyAsync(string connectionString, string databaseName, string collectionName, BsonDocument filter, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> AggregateAsync(string connectionString, string databaseName, string collectionName, List<BsonDocument> pipeline, bool allowDiskUse = false, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> FindAndModifyAsync(string connectionString, string databaseName, string collectionName, BsonDocument filter, BsonDocument update, bool upsert = false, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> ExplainFindQueryAsync(string connectionString, string databaseName, string collectionName, BsonDocument? filter = null, BsonDocument? options = null, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> ExplainCountQueryAsync(string connectionString, string databaseName, string collectionName, BsonDocument? filter = null, CancellationToken cancellationToken = default);
    Task<DocumentDbResponse> ExplainAggregateQueryAsync(string connectionString, string databaseName, string collectionName, List<BsonDocument> pipeline, CancellationToken cancellationToken = default);
}
