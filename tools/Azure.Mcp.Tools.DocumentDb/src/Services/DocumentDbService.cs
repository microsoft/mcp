// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Azure.Mcp.Tools.DocumentDb.Services;

public class DocumentDbService : IDocumentDbService
{
    private readonly ILogger<DocumentDbService> _logger;
    private MongoClient? _client;
    private string? _connectionString;
    private bool _disposed;

    public DocumentDbService(ILogger<DocumentDbService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Helper method to convert BsonDocument to JSON string for serialization
    /// </summary>
    private static string? BsonDocumentToJson(BsonDocument? doc)
    {
        return doc?.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { OutputMode = MongoDB.Bson.IO.JsonOutputMode.RelaxedExtendedJson });
    }

    /// <summary>
    /// Helper method to convert List of BsonDocument to List of JSON strings for serialization
    /// </summary>
    private static List<string> BsonDocumentListToJson(List<BsonDocument> docs)
    {
        return docs.Select(doc => doc.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { OutputMode = MongoDB.Bson.IO.JsonOutputMode.RelaxedExtendedJson })).ToList();
    }

    #region Connection Management

    public async Task<object> ConnectAsync(string connectionString, bool testConnection = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return new Dictionary<string, object?>
                {
                    ["success"] = false,
                    ["message"] = "Connection string cannot be empty",
                    ["data"] = null
                };
            }

            // Disconnect any existing connection
            if (_client != null)
            {
                await DisconnectAsync();
            }

            _connectionString = connectionString;
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
            _client = new MongoClient(settings);

            if (testConnection)
            {
                // Test the connection by listing databases
                var databases = await _client.ListDatabaseNames().ToListAsync();
                _logger.LogInformation("Successfully connected to DocumentDB. Found {Count} databases", databases.Count);

                return new Dictionary<string, object?>
                {
                    ["success"] = true,
                    ["message"] = "Connected successfully",
                    ["data"] = new Dictionary<string, object?>
                    {
                        ["databaseCount"] = databases.Count,
                        ["databases"] = databases
                    }
                };
            }

            return new Dictionary<string, object?>
            {
                ["success"] = true,
                ["message"] = "Connected successfully (not tested)",
                ["data"] = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to DocumentDB");
            _client = null;
            _connectionString = null;
            return new Dictionary<string, object?>
            {
                ["success"] = false,
                ["message"] = $"Connection failed: {ex.Message}",
                ["data"] = null
            };
        }
    }

    public Task<object> DisconnectAsync()
    {
        try
        {
            if (_client == null)
            {
                return Task.FromResult<object>(new Dictionary<string, object?>
                {
                    ["success"] = true,
                    ["message"] = "No active connection"
                });
            }

            _client = null;
            _connectionString = null;
            _logger.LogInformation("Disconnected from DocumentDB");
            return Task.FromResult<object>(new Dictionary<string, object?>
            {
                ["success"] = true,
                ["message"] = "Disconnected successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnect");
            return Task.FromResult<object>(new Dictionary<string, object?>
            {
                ["success"] = false,
                ["message"] = $"Disconnect failed: {ex.Message}"
            });
        }
    }

    public object GetConnectionStatus()
    {
        if (_client == null)
        {
            return new Dictionary<string, object?>
            {
                ["isConnected"] = false,
                ["connectionString"] = null,
                ["details"] = null
            };
        }

        var sanitizedConnectionString = SanitizeConnectionString(_connectionString);
        return new Dictionary<string, object?>
        {
            ["isConnected"] = true,
            ["connectionString"] = sanitizedConnectionString,
            ["details"] = new Dictionary<string, object?>
            {
                ["status"] = "Connected"
            }
        };
    }

    private string? SanitizeConnectionString(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return null;

        // Hide password from connection string
        try
        {
            var uri = new Uri(connectionString);
            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                var sanitized = connectionString.Replace(uri.UserInfo, "***:***");
                return sanitized;
            }
        }
        catch
        {
            // If parsing fails, just return a placeholder
            return "mongodb://***";
        }

        return connectionString;
    }

    #endregion

    #region Database Operations

    public async Task<List<string>> ListDatabasesAsync()
    {
        EnsureConnected();
        try
        {
            var databases = await _client!.ListDatabaseNames().ToListAsync();
            return databases;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing databases");
            throw new Exception($"Failed to list databases: {ex.Message}", ex);
        }
    }

    public async Task<BsonDocument> GetDatabaseStatsAsync(string databaseName)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var command = new BsonDocument { { "dbStats", 1 } };
            var stats = await database.RunCommandAsync<BsonDocument>(command);
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database stats for {DatabaseName}", databaseName);
            throw new Exception($"Failed to get database stats: {ex.Message}", ex);
        }
    }

    public async Task<object> GetDatabaseInfoAsync(string databaseName)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collections = await database.ListCollectionNames().ToListAsync();

            var collectionInfos = new List<Dictionary<string, object?>>();
            foreach (var collectionName in collections)
            {
                try
                {
                    var collection = database.GetCollection<BsonDocument>(collectionName);
                    var count = await collection.CountDocumentsAsync(new BsonDocument());
                    collectionInfos.Add(new Dictionary<string, object?>
                    {
                        ["name"] = collectionName,
                        ["count"] = count
                    });
                }
                catch (Exception ex)
                {
                    collectionInfos.Add(new Dictionary<string, object?>
                    {
                        ["name"] = collectionName,
                        ["error"] = ex.Message
                    });
                }
            }

            return new Dictionary<string, object?>
            {
                ["database_name"] = databaseName,
                ["collections"] = collectionInfos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database info for {DatabaseName}", databaseName);
            throw new Exception($"Failed to get database info: {ex.Message}", ex);
        }
    }

    public async Task<object> DropDatabaseAsync(string databaseName)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));

        try
        {
            await _client!.DropDatabaseAsync(databaseName);
            _logger.LogWarning("Dropped database {DatabaseName}", databaseName);
            return new Dictionary<string, object?>
            {
                ["success"] = true,
                ["message"] = $"Database '{databaseName}' dropped successfully",
                ["data"] = new Dictionary<string, object?>
                {
                    ["database_name"] = databaseName
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dropping database {DatabaseName}", databaseName);
            return new Dictionary<string, object?>
            {
                ["success"] = false,
                ["message"] = $"Failed to drop database: {ex.Message}",
                ["data"] = null
            };
        }
    }

    #endregion

    #region Collection Operations

    public async Task<BsonDocument> GetCollectionStatsAsync(string databaseName, string collectionName)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var command = new BsonDocument { { "collStats", collectionName } };
            var stats = await database.RunCommandAsync<BsonDocument>(command);
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting collection stats for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to get collection stats: {ex.Message}", ex);
        }
    }

    public async Task<object> RenameCollectionAsync(string databaseName, string oldName, string newName)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(oldName, nameof(oldName));
        ValidateParameter(newName, nameof(newName));

        try
        {
            var database = _client!.GetDatabase(databaseName);
            await database.RenameCollectionAsync(oldName, newName);
            _logger.LogInformation("Renamed collection {OldName} to {NewName} in database {DatabaseName}", oldName, newName, databaseName);
            return new Dictionary<string, object?>
            {
                ["success"] = true,
                ["message"] = $"Collection renamed from '{oldName}' to '{newName}' successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renaming collection in {DatabaseName}", databaseName);
            return new Dictionary<string, object?>
            {
                ["success"] = false,
                ["message"] = $"Failed to rename collection: {ex.Message}"
            };
        }
    }

    public async Task<object> DropCollectionAsync(string databaseName, string collectionName)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var database = _client!.GetDatabase(databaseName);
            await database.DropCollectionAsync(collectionName);
            _logger.LogWarning("Dropped collection {CollectionName} from database {DatabaseName}", collectionName, databaseName);
            return new Dictionary<string, object?>
            {
                ["success"] = true,
                ["message"] = $"Collection '{collectionName}' dropped successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dropping collection {CollectionName} from {DatabaseName}", collectionName, databaseName);
            return new Dictionary<string, object?>
            {
                ["success"] = false,
                ["message"] = $"Failed to drop collection: {ex.Message}"
            };
        }
    }

    public async Task<List<BsonDocument>> SampleDocumentsAsync(string databaseName, string collectionName, int sampleSize = 10)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var pipeline = new[]
            {
                new BsonDocument("$sample", new BsonDocument("size", sampleSize))
            };

            var documents = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sampling documents from {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to sample documents: {ex.Message}", ex);
        }
    }

    #endregion

    #region Document Operations

    public async Task<object> FindDocumentsAsync(string databaseName, string collectionName, BsonDocument? query = null, object? options = null)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var filter = query ?? new BsonDocument();
            var findOptions = new FindOptions<BsonDocument>();

            // Parse options if provided
            int limit = 100;
            int skip = 0;
            BsonDocument? sort = null;
            BsonDocument? projection = null;

            if (options is BsonDocument optionsDoc)
            {
                if (optionsDoc.Contains("limit"))
                    limit = optionsDoc["limit"].ToInt32();
                if (optionsDoc.Contains("skip"))
                    skip = optionsDoc["skip"].ToInt32();
                if (optionsDoc.Contains("sort") && optionsDoc["sort"].IsBsonDocument)
                    sort = optionsDoc["sort"].AsBsonDocument;
                if (optionsDoc.Contains("projection") && optionsDoc["projection"].IsBsonDocument)
                    projection = optionsDoc["projection"].AsBsonDocument;
            }

            findOptions.Limit = limit;
            findOptions.Skip = skip;
            if (sort != null)
                findOptions.Sort = new SortDefinitionBuilder<BsonDocument>().Combine(sort);
            if (projection != null)
                findOptions.Projection = new ProjectionDefinitionBuilder<BsonDocument>().Combine(projection);

            var cursor = collection.Find(filter)
                .Limit(limit)
                .Skip(skip);

            if (sort != null)
                cursor = cursor.Sort(sort);
            if (projection != null)
                cursor = cursor.Project<BsonDocument>(projection);

            var documents = await cursor.ToListAsync();
            var totalCount = await collection.CountDocumentsAsync(filter);

            return new Dictionary<string, object?>
            {
                ["documents"] = BsonDocumentListToJson(documents),
                ["total_count"] = totalCount,
                ["returned_count"] = documents.Count,
                ["has_more"] = totalCount > (skip + documents.Count),
                ["query"] = BsonDocumentToJson(filter),
                ["applied_options"] = new Dictionary<string, object?>
                {
                    ["limit"] = limit,
                    ["skip"] = skip,
                    ["sort"] = BsonDocumentToJson(sort),
                    ["projection"] = BsonDocumentToJson(projection)
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding documents in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to find documents: {ex.Message}", ex);
        }
    }

    public async Task<object> CountDocumentsAsync(string databaseName, string collectionName, BsonDocument? query = null)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var filter = query ?? new BsonDocument();
            var count = await collection.CountDocumentsAsync(filter);

            return new Dictionary<string, object?>
            {
                ["count"] = count,
                ["query"] = BsonDocumentToJson(filter)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting documents in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to count documents: {ex.Message}", ex);
        }
    }

    public async Task<object> InsertDocumentAsync(string databaseName, string collectionName, BsonDocument document)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(document);

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            await collection.InsertOneAsync(document);
            var insertedId = document["_id"].ToString();

            _logger.LogInformation("Inserted document with ID {Id} into {DatabaseName}.{CollectionName}", insertedId, databaseName, collectionName);

            return new Dictionary<string, object?>
            {
                ["inserted_id"] = insertedId,
                ["acknowledged"] = true,
                ["inserted_count"] = 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting document into {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to insert document: {ex.Message}", ex);
        }
    }

    public async Task<object> InsertManyAsync(string databaseName, string collectionName, List<BsonDocument> documents)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(documents);

        if (documents.Count == 0)
        {
            return new Dictionary<string, object?>
            {
                ["inserted_ids"] = Array.Empty<string>(),
                ["acknowledged"] = true,
                ["inserted_count"] = 0
            };
        }

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            await collection.InsertManyAsync(documents);
            var insertedIds = documents.Select(d => d["_id"].ToString()).ToList();

            _logger.LogInformation("Inserted {Count} documents into {DatabaseName}.{CollectionName}", documents.Count, databaseName, collectionName);

            return new Dictionary<string, object?>
            {
                ["inserted_ids"] = insertedIds,
                ["acknowledged"] = true,
                ["inserted_count"] = documents.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting documents into {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to insert documents: {ex.Message}", ex);
        }
    }

    public async Task<object> UpdateDocumentAsync(string databaseName, string collectionName, BsonDocument filter, BsonDocument update, bool upsert = false)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(update);

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var options = new UpdateOptions { IsUpsert = upsert };
            var result = await collection.UpdateOneAsync(filter, update, options);

            _logger.LogInformation("Updated document in {DatabaseName}.{CollectionName}. Matched: {Matched}, Modified: {Modified}",
                databaseName, collectionName, result.MatchedCount, result.ModifiedCount);

            return new Dictionary<string, object?>
            {
                ["matched_count"] = result.MatchedCount,
                ["modified_count"] = result.ModifiedCount,
                ["upserted_id"] = result.UpsertedId?.ToString(),
                ["acknowledged"] = result.IsAcknowledged
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to update document: {ex.Message}", ex);
        }
    }

    public async Task<object> UpdateManyAsync(string databaseName, string collectionName, BsonDocument filter, BsonDocument update, bool upsert = false)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(update);

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var options = new UpdateOptions { IsUpsert = upsert };
            var result = await collection.UpdateManyAsync(filter, update, options);

            _logger.LogInformation("Updated documents in {DatabaseName}.{CollectionName}. Matched: {Matched}, Modified: {Modified}",
                databaseName, collectionName, result.MatchedCount, result.ModifiedCount);

            return new Dictionary<string, object?>
            {
                ["matched_count"] = result.MatchedCount,
                ["modified_count"] = result.ModifiedCount,
                ["upserted_id"] = result.UpsertedId?.ToString(),
                ["acknowledged"] = result.IsAcknowledged
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating documents in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to update documents: {ex.Message}", ex);
        }
    }

    public async Task<object> DeleteDocumentAsync(string databaseName, string collectionName, BsonDocument filter)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(filter);

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var result = await collection.DeleteOneAsync(filter);

            _logger.LogInformation("Deleted {Count} document from {DatabaseName}.{CollectionName}",
                result.DeletedCount, databaseName, collectionName);

            return new Dictionary<string, object?>
            {
                ["deleted_count"] = result.DeletedCount,
                ["acknowledged"] = result.IsAcknowledged
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document from {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to delete document: {ex.Message}", ex);
        }
    }

    public async Task<object> DeleteManyAsync(string databaseName, string collectionName, BsonDocument filter)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(filter);

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var result = await collection.DeleteManyAsync(filter);

            _logger.LogInformation("Deleted {Count} documents from {DatabaseName}.{CollectionName}",
                result.DeletedCount, databaseName, collectionName);

            return new Dictionary<string, object?>
            {
                ["deleted_count"] = result.DeletedCount,
                ["acknowledged"] = result.IsAcknowledged
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting documents from {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to delete documents: {ex.Message}", ex);
        }
    }

    public async Task<object> AggregateAsync(string databaseName, string collectionName, List<BsonDocument> pipeline, bool allowDiskUse = false)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(pipeline);

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var options = new AggregateOptions { AllowDiskUse = allowDiskUse };
            var results = await collection.Aggregate<BsonDocument>(pipeline, options).ToListAsync();

            return new Dictionary<string, object?>
            {
                ["results"] = BsonDocumentListToJson(results),
                ["total_count"] = results.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing aggregation pipeline in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to execute aggregation: {ex.Message}", ex);
        }
    }

    public async Task<object> FindAndModifyAsync(string databaseName, string collectionName, BsonDocument query, BsonDocument update, bool upsert = false)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(update);

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = upsert,
                ReturnDocument = ReturnDocument.Before
            };

            var result = await collection.FindOneAndUpdateAsync(query, update, options);

            return new Dictionary<string, object?>
            {
                ["matched"] = result != null,
                ["upsertedId"] = result?["_id"]?.ToString(),
                ["original_document"] = BsonDocumentToJson(result),
                ["query"] = BsonDocumentToJson(query),
                ["update"] = BsonDocumentToJson(update),
                ["upsert"] = upsert
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in find and modify for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to find and modify document: {ex.Message}", ex);
        }
    }

    public async Task<object> ExplainFindQueryAsync(string databaseName, string collectionName, BsonDocument? query = null, object? options = null)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var filter = query ?? new BsonDocument();
            var findOptions = new FindOptions<BsonDocument>();

            // Parse options
            BsonDocument? sort = null;
            BsonDocument? projection = null;
            int? limit = null;
            int? skip = null;

            if (options is BsonDocument optionsDoc)
            {
                if (optionsDoc.Contains("sort") && optionsDoc["sort"].IsBsonDocument)
                    sort = optionsDoc["sort"].AsBsonDocument;
                if (optionsDoc.Contains("projection") && optionsDoc["projection"].IsBsonDocument)
                    projection = optionsDoc["projection"].AsBsonDocument;
                if (optionsDoc.Contains("limit"))
                    limit = optionsDoc["limit"].ToInt32();
                if (optionsDoc.Contains("skip"))
                    skip = optionsDoc["skip"].ToInt32();
            }

            // Build the explain command
            var findCommand = new BsonDocument
            {
                { "find", collectionName },
                { "filter", filter }
            };

            if (sort != null)
                findCommand.Add("sort", sort);
            if (projection != null)
                findCommand.Add("projection", projection);
            if (limit.HasValue)
                findCommand.Add("limit", limit.Value);
            if (skip.HasValue)
                findCommand.Add("skip", skip.Value);

            var command = new BsonDocument
            {
                { "explain", findCommand },
                { "verbosity", "executionStats" }
            };

            var explain = await database.RunCommandAsync<BsonDocument>(command);

            return new Dictionary<string, object?>
            {
                ["options_applied"] = new Dictionary<string, object?>
                {
                    ["sort"] = BsonDocumentToJson(sort),
                    ["projection"] = BsonDocumentToJson(projection),
                    ["limit"] = limit,
                    ["skip"] = skip
                },
                ["explain"] = BsonDocumentToJson(explain)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining find query for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to explain find query: {ex.Message}", ex);
        }
    }

    public async Task<object> ExplainCountQueryAsync(string databaseName, string collectionName, BsonDocument? query = null)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var database = _client!.GetDatabase(databaseName);

            // Build explain command for count
            var command = new BsonDocument
            {
                { "explain", new BsonDocument
                    {
                        { "count", collectionName },
                        { "query", query ?? new BsonDocument() }
                    }
                },
                { "verbosity", "executionStats" }
            };

            var explain = await database.RunCommandAsync<BsonDocument>(command);
            return new Dictionary<string, object?>
            {
                ["explain"] = BsonDocumentToJson(explain)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining count query for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to explain count query: {ex.Message}", ex);
        }
    }

    public async Task<object> ExplainAggregateQueryAsync(string databaseName, string collectionName, List<BsonDocument> pipeline)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(pipeline);

        try
        {
            var database = _client!.GetDatabase(databaseName);

            var command = new BsonDocument
            {
                { "explain", new BsonDocument
                    {
                        { "aggregate", collectionName },
                        { "pipeline", new BsonArray(pipeline) },
                        { "cursor", new BsonDocument() }
                    }
                },
                { "verbosity", "executionStats" }
            };

            var explain = await database.RunCommandAsync<BsonDocument>(command);
            return new Dictionary<string, object?>
            {
                ["explain"] = BsonDocumentToJson(explain)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining aggregate query for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to explain aggregate query: {ex.Message}", ex);
        }
    }

    #endregion

    #region Index Operations

    public async Task<object> CreateIndexAsync(string databaseName, string collectionName, BsonDocument keys, BsonDocument? options = null)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(keys);

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var indexKeysDefinition = new BsonDocumentIndexKeysDefinition<BsonDocument>(keys);
            var createIndexOptions = new CreateIndexOptions();

            if (options != null)
            {
                if (options.Contains("unique"))
                    createIndexOptions.Unique = options["unique"].AsBoolean;
                if (options.Contains("name"))
                    createIndexOptions.Name = options["name"].AsString;
                if (options.Contains("sparse"))
                    createIndexOptions.Sparse = options["sparse"].AsBoolean;
                if (options.Contains("expireAfterSeconds"))
                    createIndexOptions.ExpireAfter = TimeSpan.FromSeconds(options["expireAfterSeconds"].ToInt32());
            }

            var model = new CreateIndexModel<BsonDocument>(indexKeysDefinition, createIndexOptions);
            var indexName = await collection.Indexes.CreateOneAsync(model);

            _logger.LogInformation("Created index {IndexName} on {DatabaseName}.{CollectionName}", indexName, databaseName, collectionName);

            return new Dictionary<string, object?>
            {
                ["index_name"] = indexName,
                ["keys"] = BsonDocumentToJson(keys),
                ["options"] = BsonDocumentToJson(options ?? new BsonDocument())
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating index on {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to create index: {ex.Message}", ex);
        }
    }

    public async Task<object> ListIndexesAsync(string databaseName, string collectionName)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var indexes = await collection.Indexes.List().ToListAsync();

            return new Dictionary<string, object?>
            {
                ["indexes"] = BsonDocumentListToJson(indexes),
                ["count"] = indexes.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing indexes for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to list indexes: {ex.Message}", ex);
        }
    }

    public async Task<object> DropIndexAsync(string databaseName, string collectionName, string indexName)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ValidateParameter(indexName, nameof(indexName));

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            await collection.Indexes.DropOneAsync(indexName);

            _logger.LogWarning("Dropped index {IndexName} from {DatabaseName}.{CollectionName}", indexName, databaseName, collectionName);

            return new Dictionary<string, object?>
            {
                ["success"] = true,
                ["message"] = $"Index '{indexName}' dropped successfully",
                ["data"] = new Dictionary<string, object?>
                {
                    ["index_name"] = indexName
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dropping index {IndexName} from {DatabaseName}.{CollectionName}", indexName, databaseName, collectionName);
            return new Dictionary<string, object?>
            {
                ["success"] = false,
                ["message"] = $"Failed to drop index: {ex.Message}",
                ["data"] = null
            };
        }
    }

    public async Task<List<BsonDocument>> GetIndexStatsAsync(string databaseName, string collectionName)
    {
        EnsureConnected();
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var database = _client!.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var pipeline = new[]
            {
                new BsonDocument("$indexStats", new BsonDocument())
            };

            var stats = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting index stats for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            throw new Exception($"Failed to get index stats: {ex.Message}", ex);
        }
    }

    public async Task<BsonDocument> GetCurrentOpsAsync(BsonDocument? filter = null)
    {
        EnsureConnected();

        try
        {
            var adminDb = _client!.GetDatabase("admin");
            var command = new BsonDocument("currentOp", 1);

            if (filter != null && filter.ElementCount > 0)
            {
                foreach (var element in filter)
                {
                    command.Add(element);
                }
            }

            var result = await adminDb.RunCommandAsync<BsonDocument>(command);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current operations");
            throw new Exception($"Failed to get current operations: {ex.Message}", ex);
        }
    }

    #endregion

    #region Helper Methods

    private void EnsureConnected()
    {
        if (_client == null)
        {
            throw new InvalidOperationException("Not connected to DocumentDB. Please call ConnectAsync first.");
        }
    }

    private void ValidateParameter(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed)
            return;

        _client = null;
        _connectionString = null;
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    #endregion
}
