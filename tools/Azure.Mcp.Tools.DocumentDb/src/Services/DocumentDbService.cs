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
                    if (string.Equals(element.Name, "currentOp", StringComparison.Ordinal))
                    {
                        return Failure(HttpStatusCode.BadRequest, "The 'currentOp' filter field is reserved and cannot be overridden.");
                    }

                    command[element.Name] = element.Value;
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

    #region Collection Operations

    public async Task<DocumentDbResponse> GetCollectionStatsAsync(string connectionString, string databaseName, string collectionName, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var client = CreateClient(connectionString);

            if (!await DatabaseExistsAsync(client, databaseName, cancellationToken))
            {
                return Failure(HttpStatusCode.NotFound, $"Database '{databaseName}' was not found.");
            }

            if (!await CollectionExistsAsync(client, databaseName, collectionName, cancellationToken))
            {
                return Failure(HttpStatusCode.NotFound, $"Collection '{collectionName}' was not found in database '{databaseName}'.");
            }

            var database = client.GetDatabase(databaseName);
            var command = new BsonDocument { { "collStats", collectionName } };
            var stats = await database.RunCommandAsync<BsonDocument>(command, cancellationToken: cancellationToken);

            return Success($"Collection statistics for '{collectionName}' retrieved successfully.", stats);
        }
        catch (MongoAuthenticationException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access getting stats for collection {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting collection stats for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to get collection stats: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> RenameCollectionAsync(string connectionString, string databaseName, string oldName, string newName, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(oldName, nameof(oldName));
        ValidateParameter(newName, nameof(newName));

        try
        {
            var client = CreateClient(connectionString);

            if (!await DatabaseExistsAsync(client, databaseName, cancellationToken))
            {
                return Failure(HttpStatusCode.NotFound, $"Database '{databaseName}' was not found.");
            }

            if (!await CollectionExistsAsync(client, databaseName, oldName, cancellationToken))
            {
                return Failure(HttpStatusCode.NotFound, $"Collection '{oldName}' was not found in database '{databaseName}'.");
            }

            if (await CollectionExistsAsync(client, databaseName, newName, cancellationToken))
            {
                return Failure(HttpStatusCode.Conflict, $"Collection '{newName}' already exists in database '{databaseName}'.");
            }

            var database = client.GetDatabase(databaseName);
            await database.RenameCollectionAsync(oldName, newName, cancellationToken: cancellationToken);

            _logger.LogInformation("Renamed collection {OldName} to {NewName} in database {DatabaseName}", oldName, newName, databaseName);

            return Success(
                $"Collection '{oldName}' was renamed to '{newName}' successfully.",
                new Dictionary<string, object?>
                {
                    ["databaseName"] = databaseName,
                    ["oldName"] = oldName,
                    ["newName"] = newName,
                    ["renamed"] = true
                });
        }
        catch (MongoAuthenticationException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access renaming collection {OldName} to {NewName} in database {DatabaseName}", oldName, newName, databaseName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renaming collection {OldName} to {NewName} in database {DatabaseName}", oldName, newName, databaseName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to rename collection: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> DropCollectionAsync(string connectionString, string databaseName, string collectionName, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var client = CreateClient(connectionString);

            if (!await DatabaseExistsAsync(client, databaseName, cancellationToken))
            {
                return Failure(HttpStatusCode.NotFound, $"Database '{databaseName}' was not found.");
            }

            if (!await CollectionExistsAsync(client, databaseName, collectionName, cancellationToken))
            {
                return Failure(HttpStatusCode.NotFound, $"Collection '{collectionName}' was not found in database '{databaseName}'.");
            }

            var database = client.GetDatabase(databaseName);
            await database.DropCollectionAsync(collectionName, cancellationToken);

            _logger.LogWarning("Dropped collection {CollectionName} from database {DatabaseName}", collectionName, databaseName);

            return Success(
                $"Collection '{collectionName}' dropped successfully.",
                new Dictionary<string, object?>
                {
                    ["databaseName"] = databaseName,
                    ["collectionName"] = collectionName,
                    ["deleted"] = true
                });
        }
        catch (MongoAuthenticationException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access dropping collection {CollectionName} from database {DatabaseName}", collectionName, databaseName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dropping collection {CollectionName} from database {DatabaseName}", collectionName, databaseName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to drop collection: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> SampleDocumentsAsync(string connectionString, string databaseName, string collectionName, int sampleSize = 10, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var client = CreateClient(connectionString);

            if (!await DatabaseExistsAsync(client, databaseName, cancellationToken))
            {
                return Failure(HttpStatusCode.NotFound, $"Database '{databaseName}' was not found.");
            }

            if (!await CollectionExistsAsync(client, databaseName, collectionName, cancellationToken))
            {
                return Failure(HttpStatusCode.NotFound, $"Collection '{collectionName}' was not found in database '{databaseName}'.");
            }

            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var pipeline = new[]
            {
                new BsonDocument("$sample", new BsonDocument("size", sampleSize))
            };

            var documents = await collection.Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken).ToListAsync(cancellationToken);

            return Success($"Retrieved {documents.Count} sample document(s) from collection '{collectionName}'.", documents);
        }
        catch (MongoAuthenticationException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access sampling documents from collection {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sampling documents from collection {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to sample documents: {ex.Message}");
        }
    }

    #endregion

    #region Document Operations

    public async Task<DocumentDbResponse> FindDocumentsAsync(string connectionString, string databaseName, string collectionName, BsonDocument? filter = null, BsonDocument? options = null, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var collection = GetCollection(connectionString, databaseName, collectionName);
            var effectiveFilter = filter ?? new BsonDocument();
            var limit = options?.GetValue("limit", 100).ToInt32() ?? 100;
            var skip = options?.GetValue("skip", 0).ToInt32() ?? 0;
            var sort = options != null && options.Contains("sort") && options["sort"].IsBsonDocument ? options["sort"].AsBsonDocument : null;
            var projection = options != null && options.Contains("projection") && options["projection"].IsBsonDocument ? options["projection"].AsBsonDocument : null;

            var cursor = collection.Find(effectiveFilter).Limit(limit).Skip(skip);

            if (sort != null)
            {
                cursor = cursor.Sort(sort);
            }

            if (projection != null)
            {
                cursor = cursor.Project<BsonDocument>(projection);
            }

            var documents = await cursor.ToListAsync(cancellationToken);
            var totalCount = await collection.CountDocumentsAsync(effectiveFilter, cancellationToken: cancellationToken);

            return Success(
                "Documents retrieved successfully",
                new Dictionary<string, object?>
                {
                    ["documents"] = BsonDocumentListToJson(documents),
                    ["total_count"] = totalCount,
                    ["returned_count"] = documents.Count,
                    ["has_more"] = totalCount > skip + documents.Count,
                    ["filter"] = BsonDocumentToJson(effectiveFilter),
                    ["applied_options"] = new Dictionary<string, object?>
                    {
                        ["limit"] = limit,
                        ["skip"] = skip,
                        ["sort"] = BsonDocumentToJson(sort),
                        ["projection"] = BsonDocumentToJson(projection)
                    }
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
            _logger.LogWarning(ex, "Unauthorized access finding documents in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding documents in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to find documents: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> CountDocumentsAsync(string connectionString, string databaseName, string collectionName, BsonDocument? filter = null, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var collection = GetCollection(connectionString, databaseName, collectionName);
            var effectiveFilter = filter ?? new BsonDocument();
            var count = await collection.CountDocumentsAsync(effectiveFilter, cancellationToken: cancellationToken);

            return Success(
                "Documents counted successfully",
                new Dictionary<string, object?>
                {
                    ["count"] = count,
                    ["filter"] = BsonDocumentToJson(effectiveFilter)
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
            _logger.LogWarning(ex, "Unauthorized access counting documents in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting documents in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to count documents: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> InsertDocumentAsync(string connectionString, string databaseName, string collectionName, BsonDocument document, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(document);

        try
        {
            var collection = GetCollection(connectionString, databaseName, collectionName);
            await collection.InsertOneAsync(document, cancellationToken: cancellationToken);
            var insertedId = document["_id"].ToString();

            _logger.LogInformation("Inserted document with ID {Id} into {DatabaseName}.{CollectionName}", insertedId, databaseName, collectionName);

            return Success(
                "Document inserted successfully",
                new Dictionary<string, object?>
                {
                    ["inserted_id"] = insertedId,
                    ["acknowledged"] = true,
                    ["inserted_count"] = 1
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
            _logger.LogWarning(ex, "Unauthorized access inserting document into {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting document into {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to insert document: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> InsertManyAsync(string connectionString, string databaseName, string collectionName, List<BsonDocument> documents, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(documents);

        if (documents.Count == 0)
        {
            return Success(
                "No documents to insert",
                new Dictionary<string, object?>
                {
                    ["inserted_ids"] = Array.Empty<string>(),
                    ["acknowledged"] = true,
                    ["inserted_count"] = 0
                });
        }

        try
        {
            var collection = GetCollection(connectionString, databaseName, collectionName);
            await collection.InsertManyAsync(documents, cancellationToken: cancellationToken);
            var insertedIds = documents.Select(document => document["_id"].ToString()).ToList();

            _logger.LogInformation("Inserted {Count} documents into {DatabaseName}.{CollectionName}", documents.Count, databaseName, collectionName);

            return Success(
                $"{documents.Count} documents inserted successfully",
                new Dictionary<string, object?>
                {
                    ["inserted_ids"] = insertedIds,
                    ["acknowledged"] = true,
                    ["inserted_count"] = documents.Count
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
            _logger.LogWarning(ex, "Unauthorized access inserting documents into {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting documents into {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to insert documents: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> UpdateDocumentAsync(string connectionString, string databaseName, string collectionName, BsonDocument filter, BsonDocument update, bool upsert = false, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(update);

        try
        {
            var collection = GetCollection(connectionString, databaseName, collectionName);
            var options = new UpdateOptions { IsUpsert = upsert };
            var result = await collection.UpdateOneAsync(filter, update, options, cancellationToken);

            _logger.LogInformation("Updated document in {DatabaseName}.{CollectionName}. Matched: {Matched}, Modified: {Modified}", databaseName, collectionName, result.MatchedCount, result.ModifiedCount);

            return Success(
                "Document updated successfully",
                new Dictionary<string, object?>
                {
                    ["matched_count"] = result.MatchedCount,
                    ["modified_count"] = result.ModifiedCount,
                    ["upserted_id"] = result.UpsertedId?.ToString(),
                    ["acknowledged"] = result.IsAcknowledged
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
            _logger.LogWarning(ex, "Unauthorized access updating document in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to update document: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> UpdateManyAsync(string connectionString, string databaseName, string collectionName, BsonDocument filter, BsonDocument update, bool upsert = false, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(update);

        try
        {
            var collection = GetCollection(connectionString, databaseName, collectionName);
            var options = new UpdateOptions { IsUpsert = upsert };
            var result = await collection.UpdateManyAsync(filter, update, options, cancellationToken);

            _logger.LogInformation("Updated documents in {DatabaseName}.{CollectionName}. Matched: {Matched}, Modified: {Modified}", databaseName, collectionName, result.MatchedCount, result.ModifiedCount);

            return Success(
                "Documents updated successfully",
                new Dictionary<string, object?>
                {
                    ["matched_count"] = result.MatchedCount,
                    ["modified_count"] = result.ModifiedCount,
                    ["upserted_id"] = result.UpsertedId?.ToString(),
                    ["acknowledged"] = result.IsAcknowledged
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
            _logger.LogWarning(ex, "Unauthorized access updating documents in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating documents in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to update documents: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> DeleteDocumentAsync(string connectionString, string databaseName, string collectionName, BsonDocument filter, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(filter);

        try
        {
            var collection = GetCollection(connectionString, databaseName, collectionName);
            var result = await collection.DeleteOneAsync(filter, cancellationToken);

            _logger.LogInformation("Deleted {Count} document from {DatabaseName}.{CollectionName}", result.DeletedCount, databaseName, collectionName);

            return Success(
                "Document deleted successfully",
                new Dictionary<string, object?>
                {
                    ["deleted_count"] = result.DeletedCount,
                    ["acknowledged"] = result.IsAcknowledged
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
            _logger.LogWarning(ex, "Unauthorized access deleting document from {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document from {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to delete document: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> DeleteManyAsync(string connectionString, string databaseName, string collectionName, BsonDocument filter, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(filter);

        try
        {
            var collection = GetCollection(connectionString, databaseName, collectionName);
            var result = await collection.DeleteManyAsync(filter, cancellationToken);

            _logger.LogInformation("Deleted {Count} documents from {DatabaseName}.{CollectionName}", result.DeletedCount, databaseName, collectionName);

            return Success(
                "Documents deleted successfully",
                new Dictionary<string, object?>
                {
                    ["deleted_count"] = result.DeletedCount,
                    ["acknowledged"] = result.IsAcknowledged
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
            _logger.LogWarning(ex, "Unauthorized access deleting documents from {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting documents from {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to delete documents: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> AggregateAsync(string connectionString, string databaseName, string collectionName, List<BsonDocument> pipeline, bool allowDiskUse = false, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(pipeline);

        try
        {
            var collection = GetCollection(connectionString, databaseName, collectionName);
            var options = new AggregateOptions { AllowDiskUse = allowDiskUse };
            var results = await collection.Aggregate<BsonDocument>(pipeline, options, cancellationToken: cancellationToken).ToListAsync(cancellationToken);

            return Success(
                "Aggregation completed successfully",
                new Dictionary<string, object?>
                {
                    ["results"] = BsonDocumentListToJson(results),
                    ["total_count"] = results.Count
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
            _logger.LogWarning(ex, "Unauthorized access aggregating documents in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing aggregation pipeline in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to execute aggregation: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> FindAndModifyAsync(string connectionString, string databaseName, string collectionName, BsonDocument filter, BsonDocument update, bool upsert = false, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(update);

        try
        {
            var collection = GetCollection(connectionString, databaseName, collectionName);
            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = upsert,
                ReturnDocument = ReturnDocument.Before
            };

            var result = await collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);

            return Success(
                "Find and modify completed successfully",
                new Dictionary<string, object?>
                {
                    ["matched"] = result != null,
                    ["upsertedId"] = result?["_id"]?.ToString(),
                    ["original_document"] = BsonDocumentToJson(result),
                    ["filter"] = BsonDocumentToJson(filter),
                    ["update"] = BsonDocumentToJson(update),
                    ["upsert"] = upsert
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
            _logger.LogWarning(ex, "Unauthorized access finding and modifying documents in {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in find and modify for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to find and modify document: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> ExplainFindQueryAsync(string connectionString, string databaseName, string collectionName, BsonDocument? filter = null, BsonDocument? options = null, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var database = CreateClient(connectionString).GetDatabase(databaseName);
            var effectiveFilter = filter ?? new BsonDocument();
            var sort = options != null && options.Contains("sort") && options["sort"].IsBsonDocument ? options["sort"].AsBsonDocument : null;
            var projection = options != null && options.Contains("projection") && options["projection"].IsBsonDocument ? options["projection"].AsBsonDocument : null;
            var limit = options?.GetValue("limit", BsonNull.Value);
            var skip = options?.GetValue("skip", BsonNull.Value);

            var findCommand = new BsonDocument
            {
                { "find", collectionName },
                { "filter", effectiveFilter }
            };

            if (sort != null)
            {
                findCommand.Add("sort", sort);
            }

            if (projection != null)
            {
                findCommand.Add("projection", projection);
            }

            if (limit != null && !limit.IsBsonNull)
            {
                findCommand.Add("limit", limit.ToInt32());
            }

            if (skip != null && !skip.IsBsonNull)
            {
                findCommand.Add("skip", skip.ToInt32());
            }

            var command = new BsonDocument
            {
                { "explain", findCommand },
                { "verbosity", "executionStats" }
            };

            var explain = await database.RunCommandAsync<BsonDocument>(command, cancellationToken: cancellationToken);

            return Success(
                "Find query explained successfully",
                new Dictionary<string, object?>
                {
                    ["options_applied"] = new Dictionary<string, object?>
                    {
                        ["sort"] = BsonDocumentToJson(sort),
                        ["projection"] = BsonDocumentToJson(projection),
                        ["limit"] = limit?.IsBsonNull == false ? limit.ToInt32() : null,
                        ["skip"] = skip?.IsBsonNull == false ? skip.ToInt32() : null
                    },
                    ["explain"] = BsonDocumentToJson(explain)
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
            _logger.LogWarning(ex, "Unauthorized access explaining find query for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining find query for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to explain find query: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> ExplainCountQueryAsync(string connectionString, string databaseName, string collectionName, BsonDocument? filter = null, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));

        try
        {
            var database = CreateClient(connectionString).GetDatabase(databaseName);
            var command = new BsonDocument
            {
                {
                    "explain",
                    new BsonDocument
                    {
                        { "count", collectionName },
                        { "query", filter ?? new BsonDocument() }
                    }
                },
                { "verbosity", "executionStats" }
            };

            var explain = await database.RunCommandAsync<BsonDocument>(command, cancellationToken: cancellationToken);

            return Success(
                "Count query explained successfully",
                new Dictionary<string, object?>
                {
                    ["explain"] = BsonDocumentToJson(explain)
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
            _logger.LogWarning(ex, "Unauthorized access explaining count query for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining count query for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to explain count query: {ex.Message}");
        }
    }

    public async Task<DocumentDbResponse> ExplainAggregateQueryAsync(string connectionString, string databaseName, string collectionName, List<BsonDocument> pipeline, CancellationToken cancellationToken = default)
    {
        ValidateParameter(connectionString, nameof(connectionString));
        ValidateParameter(databaseName, nameof(databaseName));
        ValidateParameter(collectionName, nameof(collectionName));
        ArgumentNullException.ThrowIfNull(pipeline);

        try
        {
            var database = CreateClient(connectionString).GetDatabase(databaseName);
            var command = new BsonDocument
            {
                {
                    "explain",
                    new BsonDocument
                    {
                        { "aggregate", collectionName },
                        { "pipeline", new BsonArray(pipeline) },
                        { "cursor", new BsonDocument() }
                    }
                },
                { "verbosity", "executionStats" }
            };

            var explain = await database.RunCommandAsync<BsonDocument>(command, cancellationToken: cancellationToken);

            return Success(
                "Aggregate query explained successfully",
                new Dictionary<string, object?>
                {
                    ["explain"] = BsonDocumentToJson(explain)
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
            _logger.LogWarning(ex, "Unauthorized access explaining aggregate query for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.Unauthorized, $"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining aggregate query for {DatabaseName}.{CollectionName}", databaseName, collectionName);
            return Failure(HttpStatusCode.InternalServerError, $"Failed to explain aggregate query: {ex.Message}");
        }
    }

    #endregion

    #region Helper Functions

    private static async Task<bool> DatabaseExistsAsync(MongoClient client, string dbName, CancellationToken cancellationToken)
    {
        var databaseNames = await client.ListDatabaseNames(cancellationToken: cancellationToken).ToListAsync(cancellationToken);
        return databaseNames.Contains(dbName, StringComparer.Ordinal);
    }

    private static async Task<bool> CollectionExistsAsync(MongoClient client, string dbName, string collectionName, CancellationToken cancellationToken)
    {
        var database = client.GetDatabase(dbName);
        var collectionNames = await database.ListCollectionNames(cancellationToken: cancellationToken).ToListAsync(cancellationToken);
        return collectionNames.Contains(collectionName, StringComparer.Ordinal);
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
