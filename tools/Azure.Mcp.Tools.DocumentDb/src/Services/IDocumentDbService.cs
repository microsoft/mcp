// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MongoDB.Bson;
using MongoDB.Driver;

namespace Azure.Mcp.Tools.DocumentDb.Services;

public interface IDocumentDbService : IDisposable
{
    // Connection Management
    Task<object> ConnectAsync(string connectionString, bool testConnection = true);
    Task<object> DisconnectAsync();
    object GetConnectionStatus();

    // Database Operations
    Task<List<string>> ListDatabasesAsync();
    Task<BsonDocument> GetDatabaseStatsAsync(string databaseName);
    Task<object> GetDatabaseInfoAsync(string databaseName);
    Task<object> DropDatabaseAsync(string databaseName);

    // Collection Operations
    Task<BsonDocument> GetCollectionStatsAsync(string databaseName, string collectionName);
    Task<object> RenameCollectionAsync(string databaseName, string oldName, string newName);
    Task<object> DropCollectionAsync(string databaseName, string collectionName);
    Task<List<BsonDocument>> SampleDocumentsAsync(string databaseName, string collectionName, int sampleSize = 10);

    // Document Operations
    Task<object> FindDocumentsAsync(string databaseName, string collectionName, BsonDocument? query = null, object? options = null);
    Task<object> CountDocumentsAsync(string databaseName, string collectionName, BsonDocument? query = null);
    Task<object> InsertDocumentAsync(string databaseName, string collectionName, BsonDocument document);
    Task<object> InsertManyAsync(string databaseName, string collectionName, List<BsonDocument> documents);
    Task<object> UpdateDocumentAsync(string databaseName, string collectionName, BsonDocument filter, BsonDocument update, bool upsert = false);
    Task<object> UpdateManyAsync(string databaseName, string collectionName, BsonDocument filter, BsonDocument update, bool upsert = false);
    Task<object> DeleteDocumentAsync(string databaseName, string collectionName, BsonDocument filter);
    Task<object> DeleteManyAsync(string databaseName, string collectionName, BsonDocument filter);
    Task<object> AggregateAsync(string databaseName, string collectionName, List<BsonDocument> pipeline, bool allowDiskUse = false);
    Task<object> FindAndModifyAsync(string databaseName, string collectionName, BsonDocument query, BsonDocument update, bool upsert = false);
    Task<object> ExplainFindQueryAsync(string databaseName, string collectionName, BsonDocument? query = null, object? options = null);
    Task<object> ExplainCountQueryAsync(string databaseName, string collectionName, BsonDocument? query = null);
    Task<object> ExplainAggregateQueryAsync(string databaseName, string collectionName, List<BsonDocument> pipeline);

    // Index Operations
    Task<object> CreateIndexAsync(string databaseName, string collectionName, BsonDocument keys, BsonDocument? options = null);
    Task<object> ListIndexesAsync(string databaseName, string collectionName);
    Task<object> DropIndexAsync(string databaseName, string collectionName, string indexName);
    Task<List<BsonDocument>> GetIndexStatsAsync(string databaseName, string collectionName);
    Task<BsonDocument> GetCurrentOpsAsync(BsonDocument? filter = null);
}
