// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.LiveTests;

public class DocumentDbCommandTests(ITestOutputHelper output, LiveServerFixture serverFixture)
    : CommandTestsBase(output, serverFixture)
{
    private const string TestDatabaseName = "test";
    private const string CollectionName = "items";
    private static bool _testDataInitialized;
    private static readonly SemaphoreSlim InitLock = new(1, 1);

    private string ConnectionString => Settings.DeploymentOutputs["DOCUMENTDB_CONNECTION_STRING"];

    public override async ValueTask InitializeAsync()
    {
        await LoadSettingsAsync();

        Assert.SkipWhen(TestMode != Microsoft.Mcp.Tests.Helpers.TestMode.Live,
            "DocumentDb live tests are live-only and do not support record/playback mode");

        SetArguments("server", "start", "--mode", "all", "--dangerously-disable-elicitation");
        await base.InitializeAsync();

        if (_testDataInitialized)
        {
            return;
        }

        await InitLock.WaitAsync();

        try
        {
            if (_testDataInitialized)
            {
                return;
            }

            await SeedTestDatabaseAsync();
            _testDataInitialized = true;
        }
        finally
        {
            InitLock.Release();
        }
    }

    [Fact]
    public async Task Should_list_indexes_with_connection_string()
    {
        var result = await CallToolAsync(
            "documentdb_index_list_indexes",
            new()
            {
                { "connection-string", ConnectionString },
                { "db-name", TestDatabaseName },
                { "collection-name", CollectionName }
            });

        var indexesArray = result.AssertProperty("indexes");
        Assert.Equal(JsonValueKind.Array, indexesArray.ValueKind);
        Assert.NotEmpty(indexesArray.EnumerateArray());
    }

    [Fact]
    public async Task Should_create_and_drop_index_with_connection_string()
    {
        var indexName = $"value_1_mcp_{Guid.NewGuid():N}";

        var createResult = await CallToolAsync(
            "documentdb_index_create_index",
            new()
            {
                { "connection-string", ConnectionString },
                { "db-name", TestDatabaseName },
                { "collection-name", CollectionName },
                { "keys", "{\"value\":1}" },
                { "options", $"{{\"name\":\"{indexName}\"}}" }
            });

        Assert.Equal(indexName, createResult.AssertProperty("index_name").GetString());

        var listResult = await CallToolAsync(
            "documentdb_index_list_indexes",
            new()
            {
                { "connection-string", ConnectionString },
                { "db-name", TestDatabaseName },
                { "collection-name", CollectionName }
            });

        Assert.Contains(listResult.AssertProperty("indexes").EnumerateArray(), element =>
            element.GetString()?.Contains(indexName, StringComparison.Ordinal) == true);

        var dropResult = await CallToolAsync(
            "documentdb_index_drop_index",
            new()
            {
                { "connection-string", ConnectionString },
                { "db-name", TestDatabaseName },
                { "collection-name", CollectionName },
                { "index-name", indexName }
            });

        Assert.Equal(indexName, dropResult.AssertProperty("index_name").GetString());
    }

    [Fact]
    public async Task Should_get_index_stats_with_connection_string()
    {
        var indexName = $"category_1_mcp_{Guid.NewGuid():N}";

        await CallToolAsync(
            "documentdb_index_create_index",
            new()
            {
                { "connection-string", ConnectionString },
                { "db-name", TestDatabaseName },
                { "collection-name", CollectionName },
                { "keys", "{\"category\":1}" },
                { "options", $"{{\"name\":\"{indexName}\"}}" }
            });

        var statsResult = await CallToolAsync(
            "documentdb_others_get_stats",
            new()
            {
                { "connection-string", ConnectionString },
                { "resource-type", "index" },
                { "db-name", TestDatabaseName },
                { "collection-name", CollectionName }
            });

        var stats = statsResult.AssertProperty("stats");
        Assert.Equal(JsonValueKind.Array, stats.ValueKind);
        Assert.True(stats.EnumerateArray().Any());

        await CallToolAsync(
            "documentdb_index_drop_index",
            new()
            {
                { "connection-string", ConnectionString },
                { "db-name", TestDatabaseName },
                { "collection-name", CollectionName },
                { "index-name", indexName }
            });
    }

    [Fact]
    public async Task Should_list_all_databases()
    {
        var result = await CallToolAsync(
            "documentdb_database_list_databases",
            new()
            {
                { "connection-string", ConnectionString }
            });

        Assert.NotNull(result);
        Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);
        Assert.NotEmpty(result.Value.EnumerateArray());

        foreach (var database in result.Value.EnumerateArray())
        {
            var name = database.AssertProperty("name");
            Assert.False(string.IsNullOrWhiteSpace(name.GetString()));
        }
    }

    [Fact]
    public async Task Should_get_single_database_details_when_db_name_is_provided()
    {
        var result = await CallToolAsync(
            "documentdb_database_list_databases",
            new()
            {
                { "connection-string", ConnectionString },
                { "db-name", "test" }
            });

        Assert.NotNull(result);
        Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);

        var database = Assert.Single(result.Value.EnumerateArray());
        var name = database.AssertProperty("name");
        Assert.Equal("test", name.GetString());

        var collectionCount = database.AssertProperty("collectionCount");
        Assert.True(collectionCount.GetInt32() >= 1);

        var collections = database.AssertProperty("collections");
        Assert.Equal(JsonValueKind.Array, collections.ValueKind);
        Assert.NotEmpty(collections.EnumerateArray());
    }

    [Fact]
    public async Task Should_get_database_statistics()
    {
        var result = await CallToolAsync(
            "documentdb_others_get_stats",
            new()
            {
                { "connection-string", ConnectionString },
                { "resource-type", "database" },
                { "db-name", "test" }
            });

        Assert.NotNull(result);

        var database = result.Value.AssertProperty("db");
        Assert.Equal("test", database.GetString());

        var collections = result.Value.AssertProperty("collections");
        Assert.True(collections.GetInt32() >= 1);
    }

    [Fact]
    public async Task Should_drop_database()
    {
        const string databaseName = "dropme";

        await CreateCollectionWithDocumentsAsync(
            databaseName,
            CollectionName,
            [new BsonDocument { { "name", "drop-item" }, { "value", 1 } }]);

        var result = await CallToolAsync(
            "documentdb_database_drop_database",
            new()
            {
                { "connection-string", ConnectionString },
                { "db-name", databaseName }
            });

        Assert.NotNull(result);

        var name = result.Value.AssertProperty("name");
        Assert.Equal(databaseName, name.GetString());

        var deleted = result.Value.AssertProperty("deleted");
        Assert.True(deleted.GetBoolean());
    }

    [Fact]
    public async Task Should_find_documents_with_query_and_options()
    {
        var result = await CallToolAsync(
            "documentdb_document_find_documents",
            new()
            {
                { "connection-string", ConnectionString },
                { "db-name", TestDatabaseName },
                { "collection-name", CollectionName },
                { "query", "{\"category\":\"A\"}" },
                { "options", "{\"limit\":1,\"sort\":{\"value\":-1}}" }
            });

        var documents = result.AssertProperty("documents");
        Assert.Equal(JsonValueKind.Array, documents.ValueKind);

        var returnedCount = result.AssertProperty("returned_count");
        Assert.Equal(1, returnedCount.GetInt32());

        var totalCount = result.AssertProperty("total_count");
        Assert.True(totalCount.GetInt32() >= 2);

        var firstDocument = Assert.Single(documents.EnumerateArray()).GetString();
        Assert.Contains("\"category\":\"A\"", firstDocument, StringComparison.Ordinal);
        Assert.Contains("\"value\":300", firstDocument, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Should_count_documents_with_query()
    {
        var result = await CallToolAsync(
            "documentdb_document_count_documents",
            new()
            {
                { "connection-string", ConnectionString },
                { "db-name", TestDatabaseName },
                { "collection-name", CollectionName },
                { "query", "{\"category\":\"A\"}" }
            });

        var count = result.AssertProperty("count");
        Assert.Equal(2, count.GetInt32());
    }

    [Fact]
    public async Task Should_insert_single_document()
    {
        var databaseName = CreateUniqueName("doc-insert-db-");
        var collectionName = CreateUniqueName("doc-insert-col-");

        try
        {
            await CreateCollectionWithDocumentsAsync(databaseName, collectionName, []);

            var result = await CallToolAsync(
                "documentdb_document_insert_documents",
                new()
                {
                    { "connection-string", ConnectionString },
                    { "db-name", databaseName },
                    { "collection-name", collectionName },
                    { "documents", "{\"name\":\"live-insert\",\"value\":42}" }
                });

            Assert.False(string.IsNullOrWhiteSpace(result.AssertProperty("inserted_id").GetString()));
            Assert.Equal(1, result.AssertProperty("inserted_count").GetInt32());

            var inserted = await FindSingleDocumentAsync(databaseName, collectionName, Builders<BsonDocument>.Filter.Eq("name", "live-insert"));
            Assert.NotNull(inserted);
            Assert.Equal(42, inserted!["value"].ToInt32());
        }
        finally
        {
            await DeleteDatabaseIfExistsAsync(databaseName);
        }
    }

    [Fact]
    public async Task Should_insert_many_documents_when_mode_many()
    {
        var databaseName = CreateUniqueName("doc-insertmany-db-");
        var collectionName = CreateUniqueName("doc-insertmany-col-");

        try
        {
            await CreateCollectionWithDocumentsAsync(databaseName, collectionName, []);

            var result = await CallToolAsync(
                "documentdb_document_insert_documents",
                new()
                {
                    { "connection-string", ConnectionString },
                    { "db-name", databaseName },
                    { "collection-name", collectionName },
                    { "documents", "[{\"name\":\"bulk-a\",\"value\":1},{\"name\":\"bulk-b\",\"value\":2}]" },
                    { "mode", "many" }
                });

            Assert.Equal(2, result.AssertProperty("inserted_count").GetInt32());

            var insertedCount = await CountCollectionDocumentsAsync(databaseName, collectionName);
            Assert.Equal(2L, insertedCount);
        }
        finally
        {
            await DeleteDatabaseIfExistsAsync(databaseName);
        }
    }

    [Fact]
    public async Task Should_update_many_documents()
    {
        var databaseName = CreateUniqueName("doc-update-db-");
        var collectionName = CreateUniqueName("doc-update-col-");

        try
        {
            await CreateCollectionWithDocumentsAsync(
                databaseName,
                collectionName,
                [
                    new BsonDocument { { "name", "item-a" }, { "status", "pending" } },
                    new BsonDocument { { "name", "item-b" }, { "status", "pending" } },
                    new BsonDocument { { "name", "item-c" }, { "status", "done" } }
                ]);

            var result = await CallToolAsync(
                "documentdb_document_update_documents",
                new()
                {
                    { "connection-string", ConnectionString },
                    { "db-name", databaseName },
                    { "collection-name", collectionName },
                    { "filter", "{\"status\":\"pending\"}" },
                    { "update", "{\"$set\":{\"status\":\"processed\"}}" },
                    { "mode", "many" }
                });

            Assert.Equal(2, result.AssertProperty("matched_count").GetInt32());
            Assert.Equal(2, result.AssertProperty("modified_count").GetInt32());

            var updatedCount = await CountCollectionDocumentsAsync(
                databaseName,
                collectionName,
                Builders<BsonDocument>.Filter.Eq("status", "processed"));

            Assert.Equal(2L, updatedCount);
        }
        finally
        {
            await DeleteDatabaseIfExistsAsync(databaseName);
        }
    }

    [Fact]
    public async Task Should_delete_many_documents()
    {
        var databaseName = CreateUniqueName("doc-delete-db-");
        var collectionName = CreateUniqueName("doc-delete-col-");

        try
        {
            await CreateCollectionWithDocumentsAsync(
                databaseName,
                collectionName,
                [
                    new BsonDocument { { "name", "item-a" }, { "status", "remove" } },
                    new BsonDocument { { "name", "item-b" }, { "status", "remove" } },
                    new BsonDocument { { "name", "item-c" }, { "status", "keep" } }
                ]);

            var result = await CallToolAsync(
                "documentdb_document_delete_documents",
                new()
                {
                    { "connection-string", ConnectionString },
                    { "db-name", databaseName },
                    { "collection-name", collectionName },
                    { "filter", "{\"status\":\"remove\"}" },
                    { "mode", "many" }
                });

            Assert.Equal(2, result.AssertProperty("deleted_count").GetInt32());

            var remainingCount = await CountCollectionDocumentsAsync(databaseName, collectionName);
            Assert.Equal(1L, remainingCount);
        }
        finally
        {
            await DeleteDatabaseIfExistsAsync(databaseName);
        }
    }

    [Fact]
    public async Task Should_run_aggregate_pipeline()
    {
        var result = await CallToolAsync(
            "documentdb_document_aggregate",
            new()
            {
                { "connection-string", ConnectionString },
                { "db-name", TestDatabaseName },
                { "collection-name", CollectionName },
                { "pipeline", "[{\"$match\":{\"category\":\"A\"}},{\"$group\":{\"_id\":\"$category\",\"count\":{\"$sum\":1}}}]" }
            });

        var results = result.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, results.ValueKind);
        Assert.Equal(1, result.AssertProperty("total_count").GetInt32());

        var aggregateResult = Assert.Single(results.EnumerateArray()).GetString();
        Assert.Contains("\"_id\":\"A\"", aggregateResult, StringComparison.Ordinal);
        Assert.Contains("\"count\":2", aggregateResult, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Should_find_and_modify_document()
    {
        var databaseName = CreateUniqueName("doc-fam-db-");
        var collectionName = CreateUniqueName("doc-fam-col-");

        try
        {
            await CreateCollectionWithDocumentsAsync(
                databaseName,
                collectionName,
                [new BsonDocument { { "name", "workflow-item" }, { "status", "pending" } }]);

            var result = await CallToolAsync(
                "documentdb_document_find_and_modify",
                new()
                {
                    { "connection-string", ConnectionString },
                    { "db-name", databaseName },
                    { "collection-name", collectionName },
                    { "query", "{\"name\":\"workflow-item\"}" },
                    { "update", "{\"$set\":{\"status\":\"processing\"}}" }
                });

            Assert.True(result.AssertProperty("matched").GetBoolean());
            Assert.Contains("\"status\":\"pending\"", result.AssertProperty("original_document").GetString(), StringComparison.Ordinal);

            var updated = await FindSingleDocumentAsync(databaseName, collectionName, Builders<BsonDocument>.Filter.Eq("name", "workflow-item"));
            Assert.NotNull(updated);
            Assert.Equal("processing", updated!["status"].AsString);
        }
        finally
        {
            await DeleteDatabaseIfExistsAsync(databaseName);
        }
    }

    [Fact]
    public async Task Should_explain_find_query()
    {
        var result = await CallToolAsync(
            "documentdb_document_explain_query",
            new()
            {
                { "connection-string", ConnectionString },
                { "db-name", TestDatabaseName },
                { "collection-name", CollectionName },
                { "operation", "find" },
                { "query", "{\"category\":\"A\"}" },
                { "options", "{\"limit\":1}" }
            });

        var explain = result.AssertProperty("explain").GetString();
        Assert.False(string.IsNullOrWhiteSpace(explain));
        Assert.Contains("executionStats", explain, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_explain_aggregate_query()
    {
        var result = await CallToolAsync(
            "documentdb_document_explain_query",
            new()
            {
                { "connection-string", ConnectionString },
                { "db-name", TestDatabaseName },
                { "collection-name", CollectionName },
                { "operation", "aggregate" },
                { "pipeline", "[{\"$match\":{\"category\":\"A\"}}]" }
            });

        var explain = result.AssertProperty("explain").GetString();
        Assert.False(string.IsNullOrWhiteSpace(explain));
        Assert.Contains("executionStats", explain, StringComparison.OrdinalIgnoreCase);
    }

    private async Task SeedTestDatabaseAsync()
    {
        const int maxAttempts = 3;
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Output.WriteLine($"Seeding DocumentDB index test data (attempt {attempt}/{maxAttempts})...");

                var client = new MongoClient(ConnectionString);
                var database = client.GetDatabase(TestDatabaseName);

                var existingCollections = await (await database.ListCollectionNamesAsync()).ToListAsync();
                if (!existingCollections.Contains(CollectionName, StringComparer.Ordinal))
                {
                    await database.CreateCollectionAsync(CollectionName);
                }

                var collection = database.GetCollection<BsonDocument>(CollectionName);
                await collection.DeleteManyAsync(Builders<BsonDocument>.Filter.Empty);
                await collection.InsertManyAsync([
                    new BsonDocument { { "name", "item1" }, { "value", 100 }, { "category", "A" } },
                    new BsonDocument { { "name", "item2" }, { "value", 200 }, { "category", "B" } },
                    new BsonDocument { { "name", "item3" }, { "value", 300 }, { "category", "A" } }
                ]);

                Output.WriteLine("DocumentDB index test data seeded successfully.");
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (attempt == maxAttempts)
                {
                    break;
                }

                Output.WriteLine($"DocumentDB seeding attempt {attempt} failed: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        throw new InvalidOperationException("Failed to seed DocumentDB index test database.", lastException);
    }

    [Fact]
    public async Task Should_get_collection_statistics()
    {
        var result = await CallToolAsync(
            "documentdb_others_get_stats",
            new()
            {
                { "connection-string", ConnectionString },
                { "resource-type", "collection" },
                { "db-name", TestDatabaseName },
                { "collection-name", CollectionName }
            });

        Assert.NotNull(result);

        var ns = result.Value.AssertProperty("ns");
        Assert.Equal($"{TestDatabaseName}.{CollectionName}", ns.GetString());

        var count = result.Value.AssertProperty("count");
        Assert.True(count.GetInt32() >= 3);
    }

    [Fact]
    public async Task Should_sample_documents_from_collection()
    {
        const int sampleSize = 2;

        var result = await CallToolAsync(
            "documentdb_collection_sample_documents",
            new()
            {
                { "connection-string", ConnectionString },
                { "db-name", TestDatabaseName },
                { "collection-name", CollectionName },
                { "sample-size", sampleSize.ToString() }
            });

        Assert.NotNull(result);
        Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);

        var samples = result.Value.EnumerateArray().ToList();
        Assert.NotEmpty(samples);
        Assert.True(samples.Count <= sampleSize);

        foreach (var sample in samples)
        {
            Assert.Equal(JsonValueKind.Object, sample.ValueKind);
        }
    }

    [Fact]
    public async Task Should_rename_collection()
    {
        var databaseName = CreateUniqueName("rename-db-");
        var collectionName = CreateUniqueName("old-");
        var newCollectionName = CreateUniqueName("new-");

        try
        {
            await CreateCollectionWithDocumentsAsync(
                databaseName,
                collectionName,
                [new BsonDocument { { "name", "rename-item" }, { "value", 1 } }]);

            var result = await CallToolAsync(
                "documentdb_collection_rename_collection",
                new()
                {
                    { "connection-string", ConnectionString },
                    { "db-name", databaseName },
                    { "collection-name", collectionName },
                    { "new-collection-name", newCollectionName }
                });

            Assert.NotNull(result);

            var resultDatabaseName = result.Value.AssertProperty("databaseName");
            Assert.Equal(databaseName, resultDatabaseName.GetString());

            var oldName = result.Value.AssertProperty("oldName");
            Assert.Equal(collectionName, oldName.GetString());

            var newName = result.Value.AssertProperty("newName");
            Assert.Equal(newCollectionName, newName.GetString());

            var renamed = result.Value.AssertProperty("renamed");
            Assert.True(renamed.GetBoolean());

            Assert.False(await CollectionExistsAsync(databaseName, collectionName));
            Assert.True(await CollectionExistsAsync(databaseName, newCollectionName));
        }
        finally
        {
            await DeleteDatabaseIfExistsAsync(databaseName);
        }
    }

    [Fact]
    public async Task Should_drop_collection()
    {
        var databaseName = CreateUniqueName("drop-db-");
        var collectionName = CreateUniqueName("drop-col-");

        try
        {
            await CreateCollectionWithDocumentsAsync(
                databaseName,
                collectionName,
                [new BsonDocument { { "name", "drop-item" }, { "value", 1 } }]);

            var result = await CallToolAsync(
                "documentdb_collection_drop_collection",
                new()
                {
                    { "connection-string", ConnectionString },
                    { "db-name", databaseName },
                    { "collection-name", collectionName }
                });

            Assert.NotNull(result);

            var resultDatabaseName = result.Value.AssertProperty("databaseName");
            Assert.Equal(databaseName, resultDatabaseName.GetString());

            var resultCollectionName = result.Value.AssertProperty("collectionName");
            Assert.Equal(collectionName, resultCollectionName.GetString());

            var deleted = result.Value.AssertProperty("deleted");
            Assert.True(deleted.GetBoolean());

            Assert.False(await CollectionExistsAsync(databaseName, collectionName));
        }
        finally
        {
            await DeleteDatabaseIfExistsAsync(databaseName);
        }
    }

    private static string CreateUniqueName(string prefix)
    {
        return $"{prefix}{Guid.NewGuid():N}";
    }

    private async Task CreateCollectionWithDocumentsAsync(string databaseName, string collectionName, IEnumerable<BsonDocument> documents)
    {
        var client = new MongoClient(ConnectionString);
        var database = client.GetDatabase(databaseName);

        var existingCollections = await (await database.ListCollectionNamesAsync()).ToListAsync();
        if (!existingCollections.Contains(collectionName, StringComparer.Ordinal))
        {
            await database.CreateCollectionAsync(collectionName);
        }

        var collection = database.GetCollection<BsonDocument>(collectionName);
        await collection.DeleteManyAsync(Builders<BsonDocument>.Filter.Empty);

        var documentsList = documents.ToList();
        if (documentsList.Count > 0)
        {
            await collection.InsertManyAsync(documentsList);
        }
    }

    private async Task<bool> CollectionExistsAsync(string databaseName, string collectionName)
    {
        var client = new MongoClient(ConnectionString);
        var database = client.GetDatabase(databaseName);
        var collections = await (await database.ListCollectionNamesAsync()).ToListAsync();

        return collections.Contains(collectionName, StringComparer.Ordinal);
    }

    private async Task<long> CountCollectionDocumentsAsync(string databaseName, string collectionName, FilterDefinition<BsonDocument>? filter = null)
    {
        var client = new MongoClient(ConnectionString);
        var database = client.GetDatabase(databaseName);
        var collection = database.GetCollection<BsonDocument>(collectionName);

        return await collection.CountDocumentsAsync(filter ?? Builders<BsonDocument>.Filter.Empty);
    }

    private async Task<BsonDocument?> FindSingleDocumentAsync(string databaseName, string collectionName, FilterDefinition<BsonDocument> filter)
    {
        var client = new MongoClient(ConnectionString);
        var database = client.GetDatabase(databaseName);
        var collection = database.GetCollection<BsonDocument>(collectionName);

        return await collection.Find(filter).FirstOrDefaultAsync();
    }

    private async Task DeleteDatabaseIfExistsAsync(string databaseName)
    {
        var client = new MongoClient(ConnectionString);
        var databases = await (await client.ListDatabaseNamesAsync()).ToListAsync();

        if (databases.Contains(databaseName, StringComparer.Ordinal))
        {
            await client.DropDatabaseAsync(databaseName);
        }
    }
}