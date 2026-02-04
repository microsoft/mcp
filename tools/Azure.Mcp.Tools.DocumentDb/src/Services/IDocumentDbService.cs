// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MongoDB.Bson;
using MongoDB.Driver;

namespace Azure.Mcp.Tools.DocumentDb.Services;

public interface IDocumentDbService : IDisposable
{
    // Connection Management
    Task<object> ConnectAsync(string connectionString, bool testConnection = true, CancellationToken cancellationToken = default);
    Task<object> DisconnectAsync(CancellationToken cancellationToken = default);
    object GetConnectionStatus();

    // Database Operations
    Task<List<string>> ListDatabasesAsync(CancellationToken cancellationToken = default);
    Task<BsonDocument> GetDatabaseStatsAsync(string databaseName, CancellationToken cancellationToken = default);
    Task<object> GetDatabaseInfoAsync(string databaseName, CancellationToken cancellationToken = default);
    Task<object> DropDatabaseAsync(string databaseName, CancellationToken cancellationToken = default);

    // Collection Operations
    Task<BsonDocument> GetCollectionStatsAsync(string databaseName, string collectionName, CancellationToken cancellationToken = default);
    Task<object> RenameCollectionAsync(string databaseName, string oldName, string newName, CancellationToken cancellationToken = default);
    Task<object> DropCollectionAsync(string databaseName, string collectionName, CancellationToken cancellationToken = default);
    Task<List<BsonDocument>> SampleDocumentsAsync(string databaseName, string collectionName, int sampleSize = 10, CancellationToken cancellationToken = default);

    // Document Operations
    Task<object> FindDocumentsAsync(string databaseName, string collectionName, BsonDocument? query = null, object? options = null, CancellationToken cancellationToken = default);
    Task<object> CountDocumentsAsync(string databaseName, string collectionName, BsonDocument? query = null, CancellationToken cancellationToken = default);
    Task<object> InsertDocumentAsync(string databaseName, string collectionName, BsonDocument document, CancellationToken cancellationToken = default);
    Task<object> InsertManyAsync(string databaseName, string collectionName, List<BsonDocument> documents, CancellationToken cancellationToken = default);
    Task<object> UpdateDocumentAsync(string databaseName, string collectionName, BsonDocument filter, BsonDocument update, bool upsert = false, CancellationToken cancellationToken = default);
    Task<object> UpdateManyAsync(string databaseName, string collectionName, BsonDocument filter, BsonDocument update, bool upsert = false, CancellationToken cancellationToken = default);
    Task<object> DeleteDocumentAsync(string databaseName, string collectionName, BsonDocument filter, CancellationToken cancellationToken = default);
    Task<object> DeleteManyAsync(string databaseName, string collectionName, BsonDocument filter, CancellationToken cancellationToken = default);
    Task<object> AggregateAsync(string databaseName, string collectionName, List<BsonDocument> pipeline, bool allowDiskUse = false, CancellationToken cancellationToken = default);
    Task<object> FindAndModifyAsync(string databaseName, string collectionName, BsonDocument query, BsonDocument update, bool upsert = false, CancellationToken cancellationToken = default);
    Task<object> ExplainFindQueryAsync(string databaseName, string collectionName, BsonDocument? query = null, object? options = null, CancellationToken cancellationToken = default);
    Task<object> ExplainCountQueryAsync(string databaseName, string collectionName, BsonDocument? query = null, CancellationToken cancellationToken = default);
    Task<object> ExplainAggregateQueryAsync(string databaseName, string collectionName, List<BsonDocument> pipeline, CancellationToken cancellationToken = default);

    // Index Operations
    Task<object> CreateIndexAsync(string databaseName, string collectionName, BsonDocument keys, BsonDocument? options = null, CancellationToken cancellationToken = default);
    Task<object> ListIndexesAsync(string databaseName, string collectionName, CancellationToken cancellationToken = default);
    Task<object> DropIndexAsync(string databaseName, string collectionName, string indexName, CancellationToken cancellationToken = default);
    Task<List<BsonDocument>> GetIndexStatsAsync(string databaseName, string collectionName, CancellationToken cancellationToken = default);
    Task<BsonDocument> GetCurrentOpsAsync(BsonDocument? filter = null, CancellationToken cancellationToken = default);
}
