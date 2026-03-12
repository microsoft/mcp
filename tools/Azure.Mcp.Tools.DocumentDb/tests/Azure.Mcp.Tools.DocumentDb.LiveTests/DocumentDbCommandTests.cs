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
    private string ConnectionString => Settings.DeploymentOutputs["DOCUMENTDB_CONNECTION_STRING"];
    private const string TestDatabaseName = "test";
    private const string DropDatabaseName = "dropme";
    private const string CollectionName = "items";

    public override async ValueTask InitializeAsync()
    {
        SetArguments("server", "start", "--mode", "all", "--dangerously-disable-elicitation");
        await base.InitializeAsync();
        await SeedTestDatabasesAsync();
    }

    private async Task SeedTestDatabasesAsync()
    {
        const int maxAttempts = 3;
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Output.WriteLine($"Seeding DocumentDB test data (attempt {attempt}/{maxAttempts})...");

                var client = new MongoClient(ConnectionString);

                await SeedDatabaseAsync(
                    client,
                    TestDatabaseName,
                    [
                        new BsonDocument { { "name", "item1" }, { "value", 100 }, { "category", "A" } },
                        new BsonDocument { { "name", "item2" }, { "value", 200 }, { "category", "B" } },
                        new BsonDocument { { "name", "item3" }, { "value", 300 }, { "category", "A" } }
                    ]);

                await SeedDatabaseAsync(
                    client,
                    DropDatabaseName,
                    [
                        new BsonDocument { { "name", "drop-item1" }, { "value", 1 } },
                        new BsonDocument { { "name", "drop-item2" }, { "value", 2 } }
                    ]);

                Output.WriteLine("DocumentDB test data seeded successfully.");
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

        throw new InvalidOperationException("Failed to seed DocumentDB test databases.", lastException);
    }

    private static async Task SeedDatabaseAsync(MongoClient client, string databaseName, IEnumerable<BsonDocument> documents)
    {
        var database = client.GetDatabase(databaseName);
        var filter = Builders<BsonDocument>.Filter.Empty;

        var existingCollectionsCursor = await database.ListCollectionNamesAsync();
        var existingCollections = await existingCollectionsCursor.ToListAsync();

        if (!existingCollections.Contains(CollectionName))
        {
            await database.CreateCollectionAsync(CollectionName);
        }

        var collection = database.GetCollection<BsonDocument>(CollectionName);
        await collection.DeleteManyAsync(filter);
        await collection.InsertManyAsync(documents);
    }

    private async Task ConnectAsync(bool testConnection = true)
    {
        await CallToolAsync(
            "documentdb_connection_toggle",
            new()
            {
                { "action", "connect" },
                { "connection-string", ConnectionString },
                { "test-connection", testConnection.ToString().ToLowerInvariant() }
            });
    }

    [Fact]
    public async Task Should_connect_with_connection_action()
    {
        var result = await CallToolAsync(
            "documentdb_connection_toggle",
            new()
            {
                { "action", "connect" },
                { "connection-string", ConnectionString },
                { "test-connection", "true" }
            });

        Assert.NotNull(result);

        var databaseCount = result.Value.AssertProperty("databaseCount");
        Assert.True(databaseCount.GetInt32() >= 0);

        var databases = result.Value.AssertProperty("databases");
        Assert.Equal(JsonValueKind.Array, databases.ValueKind);
    }

    [Fact]
    public async Task Should_disconnect_with_connection_action()
    {
        await ConnectAsync();

        var result = await CallToolAsync(
            "documentdb_connection_toggle",
            new()
            {
                { "action", "disconnect" }
            });

        Assert.NotNull(result);

        var isConnected = result.Value.AssertProperty("isConnected");
        Assert.False(isConnected.GetBoolean());
    }

    [Fact]
    public async Task Should_get_connection_status_when_not_connected()
    {
        await CallToolAsync(
            "documentdb_connection_toggle",
            new()
            {
                { "action", "disconnect" }
            });

        var result = await CallToolAsync(
            "documentdb_connection_get_connection_status",
            new());

        Assert.NotNull(result);

        var isConnected = result.Value.AssertProperty("isConnected");
        Assert.False(isConnected.GetBoolean());
    }

    [Fact]
    public async Task Should_get_connection_status_when_connected()
    {
        await ConnectAsync();

        var result = await CallToolAsync(
            "documentdb_connection_get_connection_status",
            new());

        Assert.NotNull(result);

        var isConnected = result.Value.AssertProperty("isConnected");
        Assert.True(isConnected.GetBoolean());

        var details = result.Value.AssertProperty("details");
        Assert.Equal(JsonValueKind.Object, details.ValueKind);
    }

    [Fact]
    public async Task Should_list_all_databases()
    {
        await ConnectAsync();

        var result = await CallToolAsync(
            "documentdb_database_list_databases",
            new());

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
                { "db-name", databaseName }
            });

        Assert.NotNull(result);

        var name = result.Value.AssertProperty("name");
        Assert.Equal(databaseName, name.GetString());

        var deleted = result.Value.AssertProperty("deleted");
        Assert.True(deleted.GetBoolean());
    }

    // [Fact]
    // public async Task Should_sample_documents_from_collection()
    // {
    //     await CallToolAsync(
    //         "documentdb_connection_toggle",
    //         new()
    //         {
    //             { "action", "connect" },
    //             { "connection-string", Settings.DeploymentOutputs["DOCUMENTDB_CONNECTION_STRING"] }
    //         });

    //     var result = await CallToolAsync(
    //         "documentdb_collection_sample_documents",
    //         new()
    //         {
    //             { "db-name", "test" },
    //             { "collection-name", "items" },
    //             { "sample-size", "5" }
    //         });

    //     Assert.NotNull(result);
    //     Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);
    // }

    // [Fact]
    // public async Task Should_find_documents_in_collection()
    // {
    //     await CallToolAsync(
    //         "documentdb_connection_toggle",
    //         new()
    //         {
    //             { "action", "connect" },
    //             { "connection-string", Settings.DeploymentOutputs["DOCUMENTDB_CONNECTION_STRING"] }
    //         });

    //     var result = await CallToolAsync(
    //         "documentdb_document_find_documents",
    //         new()
    //         {
    //             { "db-name", "test" },
    //             { "collection-name", "items" },
    //             { "query", "{}" }
    //         });

    //     var documentsArray = result.AssertProperty("documents");
    //     Assert.Equal(JsonValueKind.Array, documentsArray.ValueKind);
    // }

    // [Fact]
    // public async Task Should_list_indexes()
    // {
    //     await CallToolAsync(
    //         "documentdb_connection_toggle",
    //         new()
    //         {
    //             { "action", "connect" },
    //             { "connection-string", Settings.DeploymentOutputs["DOCUMENTDB_CONNECTION_STRING"] }
    //         });

    //     var result = await CallToolAsync(
    //         "documentdb_index_list_indexes",
    //         new()
    //         {
    //             { "db-name", "test" },
    //             { "collection-name", "items" }
    //         });

    //     var indexesArray = result.AssertProperty("indexes");
    //     Assert.Equal(JsonValueKind.Array, indexesArray.ValueKind);
    //     Assert.NotEmpty(indexesArray.EnumerateArray());
    // }

    // [Fact]
    // public async Task Should_insert_update_and_delete_document()
    // {
    //     await CallToolAsync(
    //         "documentdb_connection_toggle",
    //         new()
    //         {
    //             { "action", "connect" },
    //             { "connection-string", Settings.DeploymentOutputs["DOCUMENTDB_CONNECTION_STRING"] }
    //         });

    //     // Insert a test document
    //     var insertResult = await CallToolAsync(
    //         "documentdb_document_insert_document",
    //         new()
    //         {
    //             { "db-name", "test" },
    //             { "collection-name", "items" },
    //             { "document", "{\"testField\": \"originalValue\"}" }
    //         });

    //     var insertedId = insertResult.AssertProperty("inserted_id");

    //     // Update the document
    //     var updateResult = await CallToolAsync(
    //         "documentdb_document_update_document",
    //         new()
    //         {
    //             { "db-name", "test" },
    //             { "collection-name", "items" },
    //             { "filter", $"{{\"_id\": {{\"$oid\": {insertedId.GetRawText()}}}}}" },
    //             { "update", "{\"$set\": {\"testField\": \"updatedValue\"}}" }
    //         });

    //     var modifiedCount = updateResult.AssertProperty("modified_count");
    //     Assert.Equal(1, modifiedCount.GetInt32());

    //     // Clean up - delete the document
    //     var deleteResult = await CallToolAsync(
    //         "documentdb_document_delete_document",
    //         new()
    //         {
    //             { "db-name", "test" },
    //             { "collection-name", "items" },
    //             { "filter", $"{{\"_id\": {{\"$oid\": {insertedId.GetRawText()}}}}}" }
    //         });

    //     var deletedCount = deleteResult.AssertProperty("deleted_count");
    //     Assert.Equal(1, deletedCount.GetInt32());
    // }
}