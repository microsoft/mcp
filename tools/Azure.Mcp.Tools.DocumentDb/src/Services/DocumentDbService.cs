// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.DocumentDb.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;

namespace Azure.Mcp.Tools.DocumentDb.Services;

public sealed class DocumentDbService(ILogger<DocumentDbService> logger) : IDocumentDbService
{
    private readonly ILogger<DocumentDbService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private static readonly JsonWriterSettings s_jsonWriterSettings = new() { OutputMode = JsonOutputMode.RelaxedExtendedJson };

    #region Index Management

    public async Task<DocumentDbResponse> CreateIndexAsync(string connectionString, string databaseName, string collectionName, BsonDocument keys, BsonDocument? options = null, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(keys);

        try
        {
            var collection = GetCollection(connectionString, databaseName, collectionName);
            var createIndexOptions = CreateIndexOptions(options);
            var model = new CreateIndexModel<BsonDocument>(new BsonDocumentIndexKeysDefinition<BsonDocument>(keys), createIndexOptions);
            var indexName = await collection.Indexes.CreateOneAsync(model, cancellationToken: cancellationToken);

            _logger.LogInformation("Created index {IndexName} on {DatabaseName}.{CollectionName}", indexName, databaseName, collectionName);

            return Success(
                "Index created successfully",
                new Dictionary<string, object?>
                {
                    ["index_name"] = indexName,
                    ["keys"] = BsonDocumentToJson(keys),
                    ["options"] = BsonDocumentToJson(options)
                });
        }
        catch (MongoCommandException ex) when (ex.Code == 26)
        {
            _logger.LogWarning("Database '{DatabaseName}' not found", databaseName);
            return Failure(HttpStatusCode.BadRequest, $"Database '{databaseName}' not found");
        }
        catch (MongoCommandException ex) when (ex.CodeName == "NamespaceNotFound")
        {
            _logger.LogWarning("Collection '{CollectionName}' not found in database '{DatabaseName}'", collectionName, databaseName);
            return Failure(HttpStatusCode.BadRequest, $"Collection '{collectionName}' not found");
        }
        catch (MongoAuthenticationException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access creating index on {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating index on {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to create index: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> ListIndexesAsync(string connectionString, string databaseName, string collectionName, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var collection = GetCollection(connectionString, databaseName, collectionName);
            var indexes = await collection.Indexes.List(cancellationToken: cancellationToken).ToListAsync(cancellationToken);

            return Success(
                "Indexes retrieved successfully",
                new Dictionary<string, object?>
                {
                    ["indexes"] = BsonDocumentListToJson(indexes),
                    ["count"] = indexes.Count
                });
        }
        catch (MongoCommandException ex) when (ex.Code == 26)
        {
            _logger.LogWarning("Database '{DatabaseName}' not found", databaseName);
            return Failure(HttpStatusCode.BadRequest, $"Database '{databaseName}' not found");
        }
        catch (MongoCommandException ex) when (ex.CodeName == "NamespaceNotFound")
        {
            _logger.LogWarning("Collection '{CollectionName}' not found in database '{DatabaseName}'", collectionName, databaseName);
            return Failure(HttpStatusCode.BadRequest, $"Collection '{collectionName}' not found");
        }
        catch (MongoAuthenticationException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access listing indexes for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing indexes for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to list indexes: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> DropIndexAsync(string connectionString, string databaseName, string collectionName, string indexName, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ValidateParameter(indexName, nameof(indexName));

        try
        {
            var collection = GetCollection(connectionString, databaseName, collectionName);
            await collection.Indexes.DropOneAsync(indexName, cancellationToken: cancellationToken);

            _logger.LogInformation("Dropped index {IndexName} from {DatabaseName}.{CollectionName}", indexName, databaseName, collectionName);

            return Success(
                $"Index '{indexName}' dropped successfully",
                new Dictionary<string, object?>
                {
                    ["index_name"] = indexName
                });
        }
        catch (MongoCommandException ex) when (ex.Code == 26)
        {
            _logger.LogWarning("Database '{DatabaseName}' not found", databaseName);
            return Failure(HttpStatusCode.BadRequest, $"Database '{databaseName}' not found");
        }
        catch (MongoCommandException ex) when (ex.CodeName == "NamespaceNotFound")
        {
            _logger.LogWarning("Collection '{CollectionName}' not found in database '{DatabaseName}'", collectionName, databaseName);
            return Failure(HttpStatusCode.BadRequest, $"Collection '{collectionName}' not found");
        }
        catch (MongoCommandException ex) when (ex.CodeName == "IndexNotFound")
        {
            _logger.LogWarning("Index '{IndexName}' not found in {DatabaseName}.{CollectionName}", indexName, databaseName, collectionName);
            return Failure(HttpStatusCode.BadRequest, $"Index '{indexName}' not found");
        }
        catch (MongoAuthenticationException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access dropping index {IndexName} from {DatabaseName}.{CollectionName}", indexName, databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dropping index {IndexName} from {DatabaseName}.{CollectionName}", indexName, databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to drop index: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> GetIndexStatsAsync(string connectionString, string databaseName, string collectionName, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var collection = GetCollection(connectionString, databaseName, collectionName);
            var pipeline = new[]
            {
                new BsonDocument("$indexStats", new BsonDocument())
            };

            var stats = await collection.Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken).ToListAsync(cancellationToken);

            return Success(
                "Index statistics retrieved successfully",
                new Dictionary<string, object?>
                {
                    ["stats"] = BsonDocumentListToJson(stats),
                    ["count"] = stats.Count
                });
        }
        catch (MongoCommandException ex) when (ex.Code == 26)
        {
            _logger.LogWarning("Database '{DatabaseName}' not found", databaseName);
            return Failure(HttpStatusCode.BadRequest, $"Database '{databaseName}' not found");
        }
        catch (MongoCommandException ex) when (ex.CodeName == "NamespaceNotFound")
        {
            _logger.LogWarning("Collection '{CollectionName}' not found in database '{DatabaseName}'", collectionName, databaseName);
            return Failure(HttpStatusCode.BadRequest, $"Collection '{collectionName}' not found");
        }
        catch (MongoAuthenticationException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access getting index stats for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting index stats for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to get index stats: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> GetCurrentOpsAsync(string connectionString, BsonDocument? filter = null, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));

        try
        {
            var adminDb = CreateClient(connectionString).GetDatabase("admin");
            var command = new BsonDocument("currentOp", 1);

            if (filter != null && filter.ElementCount > 0)
            {
                foreach (var element in filter)
                {
                    command.Add(element);
                }
            }

            var result = await adminDb.RunCommandAsync<BsonDocument>(command, cancellationToken: cancellationToken);

            return Success(
                "Current operations retrieved successfully",
                new Dictionary<string, object?>
                {
                    ["operations"] = BsonDocumentToJson(result)
                });
        }
        catch (MongoCommandException ex) when (ex.Code == 26)
        {
            _logger.LogWarning("Admin database not found");
            return Failure(HttpStatusCode.BadRequest, "Admin database not found");
        }
        catch (MongoCommandException ex) when (ex.CodeName == "NamespaceNotFound")
        {
            _logger.LogWarning("Namespace not found for current operations");
            return Failure(HttpStatusCode.BadRequest, "Namespace not found");
        }
        catch (MongoAuthenticationException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access getting current operations");
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current operations");
            return Failure(HttpStatusCode.InternalServerError, $"Failed to get current operations: {ex.Message}");
        }
    }

    #endregion

    #region Database Management

    public async Task<DocumentDbResponse> GetDatabasesAsync(string connectionString, string? dbName = null, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));

        try
        {
            var client = CreateClient(connectionString);
            var databaseNames = await client.ListDatabaseNames(cancellationToken: cancellationToken).ToListAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(dbName) && !databaseNames.Contains(dbName, StringComparer.Ordinal))
            {
                return Failure(HttpStatusCode.NotFound, $"Database '{dbName}' was not found.");
            }

            List<Dictionary<string, object?>> databases;
            if (string.IsNullOrWhiteSpace(dbName))
            {
                databases = databaseNames
                    .Select(databaseName => new Dictionary<string, object?>
                    {
                        ["name"] = databaseName
                    })
                    .ToList();
            }
            else
            {
                databases = [await GetDatabaseInfoAsync(client, dbName, cancellationToken)];
            }

            return Success(
                string.IsNullOrWhiteSpace(dbName)
                    ? "Databases retrieved successfully."
                    : $"Database '{dbName}' retrieved successfully.",
                databases);
        }
        catch (MongoAuthenticationException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access listing databases");
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing databases. Database: {DatabaseName}", dbName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to list databases: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> GetDatabaseStatsAsync(string connectionString, string dbName, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(dbName, nameof(dbName));

        try
        {
            var client = CreateClient(connectionString);

            if (!await DatabaseExistsAsync(client, dbName, cancellationToken))
            {
                return Failure(HttpStatusCode.NotFound, $"Database '{dbName}' was not found.");
            }

            var database = client.GetDatabase(dbName);
            var stats = await database.RunCommandAsync<BsonDocument>(new BsonDocument("dbStats", 1), cancellationToken: cancellationToken);

            return Success($"Database statistics for '{dbName}' retrieved successfully.", stats);
        }
        catch (MongoAuthenticationException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access getting stats for database {DatabaseName}", dbName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database stats for {DatabaseName}", dbName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to get database stats: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> DropDatabaseAsync(string connectionString, string dbName, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(dbName, nameof(dbName));

        try
        {
            var client = CreateClient(connectionString);

            if (!await DatabaseExistsAsync(client, dbName, cancellationToken))
            {
                return Failure(HttpStatusCode.NotFound, $"Database '{dbName}' was not found.");
            }

            await client.DropDatabaseAsync(dbName, cancellationToken);

            _logger.LogInformation("Dropped DocumentDB database {DatabaseName}", dbName);

            return Success(
                $"Database '{dbName}' dropped successfully.",
                new Dictionary<string, object?>
                {
                    ["name"] = dbName,
                    ["deleted"] = true
                });
        }
        catch (MongoAuthenticationException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access dropping database {DatabaseName}", dbName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dropping database {DatabaseName}", dbName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to drop database: {ex.Message}");
        }
    }

    #endregion

    #region Helper Functions

    private static async Task<bool> DatabaseExistsAsync(MongoClient client, string dbName, CancellationToken cancellationToken)
    {
        var databaseNames = await client.ListDatabaseNames(cancellationToken: cancellationToken).ToListAsync(cancellationToken);
        return databaseNames.Contains(dbName, StringComparer.Ordinal);
    }

    private static async Task<Dictionary<string, object?>> GetDatabaseInfoAsync(MongoClient client, string dbName, CancellationToken cancellationToken)
    {
        var database = client.GetDatabase(dbName);
        var collectionNames = await database.ListCollectionNames(cancellationToken: cancellationToken).ToListAsync(cancellationToken);

        var collections = new List<Dictionary<string, object?>>(collectionNames.Count);
        foreach (var collectionName in collectionNames)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            var documentCount = await collection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty, cancellationToken: cancellationToken);

            collections.Add(new Dictionary<string, object?>
            {
                ["name"] = collectionName,
                ["documentCount"] = documentCount
            });
        }

        return new Dictionary<string, object?>
        {
            ["name"] = dbName,
            ["collectionCount"] = collectionNames.Count,
            ["collections"] = collections
        };
    }

    private static IMongoCollection<BsonDocument> GetCollection(string connectionString, string databaseName, string collectionName)
    {
        return CreateClient(connectionString)
            .GetDatabase(databaseName)
            .GetCollection<BsonDocument>(collectionName);
    }

    private static MongoClient CreateClient(string connectionString)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
        return new MongoClient(settings);
    }

    private static CreateIndexOptions CreateIndexOptions(BsonDocument? options)
    {
        var createIndexOptions = new CreateIndexOptions();

        if (options == null)
        {
            return createIndexOptions;
        }

        if (options.Contains("unique"))
        {
            createIndexOptions.Unique = options["unique"].AsBoolean;
        }

        if (options.Contains("name"))
        {
            createIndexOptions.Name = options["name"].AsString;
        }

        if (options.Contains("sparse"))
        {
            createIndexOptions.Sparse = options["sparse"].AsBoolean;
        }

        if (options.Contains("expireAfterSeconds"))
        {
            createIndexOptions.ExpireAfter = TimeSpan.FromSeconds(options["expireAfterSeconds"].ToInt32());
        }

        return createIndexOptions;
    }

    private static string? BsonDocumentToJson(BsonDocument? doc)
    {
        return doc?.ToJson(s_jsonWriterSettings);
    }

    private static List<string> BsonDocumentListToJson(List<BsonDocument> docs)
    {
        return docs.Select(doc => doc.ToJson(s_jsonWriterSettings)).ToList();
    }

    private static DocumentDbResponse Success(string message, object? data = null)
    {
        return new DocumentDbResponse
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = message,
            Data = data
        };
    }

    private static DocumentDbResponse Failure(HttpStatusCode statusCode, string message)
    {
        return new DocumentDbResponse
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Data = null
        };
    }

    private static void ValidateParameter(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        }
    }

    #endregion
}
