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
            "DocumentDb index tests are live-only and do not support record/playback mode");

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
            "documentdb_index_index_stats",
            new()
            {
                { "connection-string", ConnectionString },
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
        await ConnectAsync();

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
        await ConnectAsync();

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
        await ConnectAsync();

        var result = await CallToolAsync(
            "documentdb_database_db_stats",
            new()
            {
                { "connection-string", ConnectionString },
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
        await ConnectAsync();
        const string databaseName = "dropme";

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
}