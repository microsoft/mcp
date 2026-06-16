// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.Tests;

public class CosmosCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    protected override RecordingOptions? RecordingOptions => new()
    {
        HandleRedirects = false
    };
    public override CustomDefaultMatcher? TestMatcher => new()
    {
        IgnoredHeaders = "x-ms-activity-id,x-ms-cosmos-correlated-activityid"
    };

    /// <summary>
    /// 3493 = $..name
    /// </summary>
    public override List<string> DisabledDefaultSanitizers => [.. base.DisabledDefaultSanitizers, "AZSDK3493"];

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        ..base.BodyKeySanitizers,
        new(new("$..resourceId"){
            Regex = "resource[Gg]roups/([^?\\/]+)",
            Value = "Sanitized",
            GroupForReplace = "1"
        }),
        new(new("$..id"){
            Regex = "resource[Gg]roups/([^?\\/]+)",
            Value = "Sanitized",
            GroupForReplace = "1"
        }),
        new(new("$..resourceId"){
            Regex = "subscriptions/([^?\\/]+)",
            Value = "00000000-0000-0000-0000-000000000000",
            GroupForReplace = "1"
        }),
        new(new("$..id"){
            Regex = "subscriptions/([^?\\/]+)",
            Value = "00000000-0000-0000-0000-000000000000",
            GroupForReplace = "1"
        })
    ];

    [Fact]
    public async Task Should_list_databases_by_account()
    {
        var result = await CallToolAsync(
            "cosmos_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", Settings.ResourceBaseName }
            });

        var databasesArray = result.AssertProperty("databases");
        Assert.Equal(JsonValueKind.Array, databasesArray.ValueKind);
        Assert.NotEmpty(databasesArray.EnumerateArray());
    }

    [Fact]
    public async Task Should_list_cosmos_containers_by_database()
    {
        var result = await CallToolAsync(
            "cosmos_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", Settings.ResourceBaseName },
                { "database", "ToDoList" }
            });

        var containersArray = result.AssertProperty("containers");
        Assert.Equal(JsonValueKind.Array, containersArray.ValueKind);
        Assert.NotEmpty(containersArray.EnumerateArray());
    }

    [Fact]
    public async Task Should_query_cosmos_database_container_items()
    {
        var resourceBaseName = TestMode == TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
        var result = await CallToolAsync(
            "cosmos_database_container_item_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", resourceBaseName },
                { "database", "ToDoList" },
                { "container", "Items" }
            });

        var itemsArray = result.AssertProperty("items");
        Assert.Equal(JsonValueKind.Array, itemsArray.ValueKind);
        Assert.NotEmpty(itemsArray.EnumerateArray());
    }

    [Fact]
    public async Task Should_list_cosmos_accounts()
    {
        var result = await CallToolAsync(
            "cosmos_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var accountsArray = result.AssertProperty("accounts");
        Assert.Equal(JsonValueKind.Array, accountsArray.ValueKind);
        Assert.NotEmpty(accountsArray.EnumerateArray());
    }

    [Fact]
    public async Task Should_show_single_item_from_cosmos_account()
    {
        var resourceBaseName = TestMode == TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
        var dbResult = await CallToolAsync(
            "cosmos_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", resourceBaseName }
            }
        );
        var databases = dbResult.AssertProperty("databases");
        Assert.Equal(JsonValueKind.Array, databases.ValueKind);
        var dbEnum = databases.EnumerateArray();
        Assert.True(dbEnum.Any());

        // The agent will choose one, for this test we're going to take the first one
        var firstDatabase = dbEnum.First();
        string dbName = RegisterOrRetrieveVariable("database", GetStringOrNameElementString(firstDatabase, "database"));
        Assert.False(string.IsNullOrEmpty(dbName));

        var containerResult = await CallToolAsync(
            "cosmos_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", resourceBaseName },
                { "database", dbName }
            });
        var containers = containerResult.AssertProperty("containers");
        Assert.Equal(JsonValueKind.Array, containers.ValueKind);
        var contEnum = containers.EnumerateArray();
        Assert.True(contEnum.Any());

        // The agent will choose one, for this test we're going to take the first one
        var firstContainer = contEnum.First();
        string containerName = RegisterOrRetrieveVariable("container", GetStringOrNameElementString(firstContainer, "container"));
        Assert.False(string.IsNullOrEmpty(containerName));

        var itemResult = await CallToolAsync(
            "cosmos_database_container_item_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", resourceBaseName },
                { "database", dbName },
                { "container", containerName! }
            });
        var items = itemResult.AssertProperty("items");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        Assert.True(items.EnumerateArray().Any());
    }

    [Fact]
    public async Task Should_list_and_query_multiple_databases_and_containers()
    {
        var resourceBaseName = TestMode == TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
        var dbResult = await CallToolAsync(
            "cosmos_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", resourceBaseName }
            }
        );
        var databases = dbResult.AssertProperty("databases");
        Assert.Equal(JsonValueKind.Array, databases.ValueKind);
        var databasesEnum = databases.EnumerateArray();
        Assert.True(databasesEnum.Any());
        var dbNumber = 0;
        foreach (var db in databasesEnum)
        {
            string dbName = RegisterOrRetrieveVariable("database" + dbNumber, GetStringOrNameElementString(db, "database"));
            Assert.False(string.IsNullOrEmpty(dbName));

            var containerResult = await CallToolAsync(
                "cosmos_list",
                new() {
                    { "subscription", Settings.SubscriptionId },
                    { "account", resourceBaseName },
                    { "database", dbName }
                });
            var containers = containerResult.AssertProperty("containers");
            Assert.Equal(JsonValueKind.Array, containers.ValueKind);
            var contEnum = containers.EnumerateArray();
            var containerNumber = 0;
            foreach (var container in contEnum)
            {
                string containerName = RegisterOrRetrieveVariable("db" + dbNumber + "/container" + containerNumber, GetStringOrNameElementString(container, "container"));
                Assert.False(string.IsNullOrEmpty(containerName));

                var itemResult = await CallToolAsync(
                    "cosmos_database_container_item_query",
                    new() {
                        { "subscription", Settings.SubscriptionId },
                        { "account", resourceBaseName },
                        { "database", dbName },
                        { "container", containerName }
                    });
                var items = itemResult.AssertProperty("items");
                Assert.Equal(JsonValueKind.Array, items.ValueKind);
                containerNumber++;
            }
            dbNumber++;
        }
    }

    [Fact]
    public async Task Should_infer_container_schema()
    {
        var resourceBaseName = TestMode == TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
        var result = await CallToolAsync(
            "cosmos_database_container_schema_infer",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", resourceBaseName },
                { "database", "ToDoList" },
                { "container", "Items" }
            });

        // Document schema:
        // {
        //   "completed": "boolean",
        //   "priority": "number",
        //   "id": "string",
        //   "title": "string",
        //   "_rid": "string",
        //   "_self": "string",
        //   "_etag": "string",
        //   "_attachments": "string",
        //   "_ts": "string"
        // }

        var sampleSize = result.AssertProperty("sampleSize");
        Assert.Equal(JsonValueKind.Number, sampleSize.ValueKind);
        Assert.True(sampleSize.GetInt32() > 0);

        var properties = result.AssertProperty("properties");
        Assert.Equal(JsonValueKind.Array, properties.ValueKind);
        // Items seeded by seed-cosmos.ps1 always contain id/title/completed/priority.
        var names = properties.EnumerateArray()
            .Select(p => p.GetProperty("name").GetString())
            .ToHashSet();
        Assert.Contains("id", names);
        Assert.Contains("title", names);
        Assert.Contains("completed", names);
        Assert.Contains("priority", names);
    }

    [Fact]
    public async Task Should_list_recent_items()
    {
        var resourceBaseName = TestMode == TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
        var result = await CallToolAsync(
            "cosmos_database_container_item_list-recent",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", resourceBaseName },
                { "database", "ToDoList" },
                { "container", "Items" }
            });

        var items = result.AssertProperty("items");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        Assert.NotEmpty(items.EnumerateArray());
    }

    [Fact]
    public async Task Should_get_item_by_id()
    {
        var resourceBaseName = TestMode == TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;

        // First, fetch any existing item to obtain a valid id to look up.
        var listResult = await CallToolAsync(
            "cosmos_database_container_item_list-recent",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", resourceBaseName },
                { "database", "ToDoList" },
                { "container", "Items" }
            });

        var items = listResult.AssertProperty("items");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        var firstItem = items.EnumerateArray().FirstOrDefault();
        Assert.Equal(JsonValueKind.Object, firstItem.ValueKind);

        var id = RegisterOrRetrieveVariable("itemId", firstItem.GetProperty("id").GetString()!);
        Assert.False(string.IsNullOrEmpty(id));

        var getResult = await CallToolAsync(
            "cosmos_database_container_item_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", resourceBaseName },
                { "database", "ToDoList" },
                { "container", "Items" },
                { "id", id }
            });

        var item = getResult.AssertProperty("item");
        Assert.Equal(JsonValueKind.Object, item.ValueKind);
        Assert.Equal(id, item.GetProperty("id").GetString());
    }

    [Fact]
    public async Task Should_get_item_by_id_with_partition_key()
    {
        var resourceBaseName = TestMode == TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;

        // Items container uses /id as partition key in test resources.
        var listResult = await CallToolAsync(
            "cosmos_database_container_item_list-recent",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", resourceBaseName },
                { "database", "ToDoList" },
                { "container", "Items" }
            });

        var items = listResult.AssertProperty("items");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        var firstItem = items.EnumerateArray().FirstOrDefault();
        Assert.Equal(JsonValueKind.Object, firstItem.ValueKind);

        var id = RegisterOrRetrieveVariable("itemIdWithPk", firstItem.GetProperty("id").GetString()!);
        Assert.False(string.IsNullOrEmpty(id));

        var getResult = await CallToolAsync(
            "cosmos_database_container_item_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", resourceBaseName },
                { "database", "ToDoList" },
                { "container", "Items" },
                { "id", id },
                { "partition-key", id }
            });

        var item = getResult.AssertProperty("item");
        Assert.Equal(JsonValueKind.Object, item.ValueKind);
        Assert.Equal(id, item.GetProperty("id").GetString());
    }

    [Fact]
    public async Task Should_text_search_documents()
    {
        var resourceBaseName = TestMode == TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
        var result = await CallToolAsync(
            "cosmos_database_container_item_text-search",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", resourceBaseName },
                { "database", "ToDoList" },
                { "container", "TextItems" },
                { "search-property", "description" },
                { "search-phrase", "cosmos" }
            });

        // Document schema:
        // {
        //   "id": "string",
        //   "description": "string",
        //   "_rid": "string",
        //   "_self": "string",
        //   "_attachments": "string",
        //   "_ts": "string"
        // }

        var items = result.AssertProperty("items");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        // TextItems seeded by seed-cosmos.ps1 contains text-1 and text-3 with "cosmos".
        // "hello" is a stop word in Cosmos's extended English list and is not indexed.
        var hitIds = items.EnumerateArray()
            .Select(i => i.GetProperty("id").GetString())
            .ToHashSet();
        Assert.Contains("text-1", hitIds);
        Assert.Contains("text-3", hitIds);
        // Results should not include other properties
        foreach (var item in items.EnumerateArray())
        {
            Assert.True(item.TryGetProperty("id", out _), "id should be present");
            Assert.True(item.TryGetProperty("description", out _), "description should be present");
        }
    }

    [Fact]
    public async Task Should_text_search_documents_with_select_properties()
    {
        var resourceBaseName = TestMode == TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
        var result = await CallToolAsync(
            "cosmos_database_container_item_text-search",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", resourceBaseName },
                { "database", "ToDoList" },
                { "container", "TextItems" },
                { "search-property", "description" },
                { "search-phrase", "cosmos" },
                { "properties-to-select", "id" }
            });

        // Document schema:
        // {
        //   "id": "string",
        //   "description": "string",
        //   "_rid": "string",
        //   "_self": "string",
        //   "_attachments": "string",
        //   "_ts": "string"
        // }

        var items = result.AssertProperty("items");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        // TextItems seeded by seed-cosmos.ps1 contains text-1 and text-3 with "cosmos".
        // "hello" is a stop word in Cosmos's extended English list and is not indexed.
        var hitIds = items.EnumerateArray()
            .Select(i => i.GetProperty("id").GetString())
            .ToHashSet();
        Assert.Contains("text-1", hitIds);
        Assert.Contains("text-3", hitIds);
        // Results should not include other properties
        foreach (var item in items.EnumerateArray())
        {
            Assert.True(item.TryGetProperty("id", out _), "id should be present");
            Assert.False(item.TryGetProperty("description", out _), "description should not be selected");
        }
    }

    [Fact]
    public async Task Should_vector_search_documents()
    {
        var resourceBaseName = TestMode == TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
        var openAiEndpoint = RegisterOrRetrieveDeploymentOutputVariable("openAiEndpoint", "OPENAIENDPOINT");
        var embeddingDeployment = RegisterOrRetrieveDeploymentOutputVariable("embeddingDeployment", "EMBEDDINGDEPLOYMENTNAME");

        // Omitting --properties-to-select triggers the wrapper-projection path: the service
        // requests `c AS doc, VectorDistance(...) AS _score` and strips the vector before returning.
        var result = await CallToolAsync(
            "cosmos_database_container_item_vector-search",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", resourceBaseName },
                { "database", "ToDoList" },
                { "container", "VectorItems" },
                { "vector-property", "vector" },
                { "search-text", "hello world" },
                { "openai-endpoint", openAiEndpoint },
                { "embedding-deployment", embeddingDeployment }
            });

        // Document schema:
        // {
        //   "id": "string",
        //   "text": "string",
        //   "vector": "array",
        //   "_rid": "string",
        //   "_self": "string",
        //   "_attachments": "string",
        //   "_ts": "string",
        // }

        var items = result.AssertProperty("items");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        Assert.NotEmpty(items.EnumerateArray());
        var first = items.EnumerateArray().First();
        Assert.Equal("vec-greeting", first.GetProperty("id").GetString());
        // The score is calculated by Cosmos during the vector search
        Assert.True(first.TryGetProperty("_score", out _), "_score should be present");
        Assert.True(first.TryGetProperty("text", out _), "text property should be selected");
        Assert.False(first.TryGetProperty("vector", out _), "vector property should be stripped");
    }

    [Fact]
    public async Task Should_vector_search_documents_with_select_properties()
    {
        var resourceBaseName = TestMode == TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
        var openAiEndpoint = RegisterOrRetrieveDeploymentOutputVariable("openAiEndpoint", "OPENAIENDPOINT");
        var embeddingDeployment = RegisterOrRetrieveDeploymentOutputVariable("embeddingDeployment", "EMBEDDINGDEPLOYMENTNAME");

        // VectorItems uses 1536-dim vectors to match `text-embedding-3-small`.
        var result = await CallToolAsync(
            "cosmos_database_container_item_vector-search",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", resourceBaseName },
                { "database", "ToDoList" },
                { "container", "VectorItems" },
                { "vector-property", "vector" },
                { "properties-to-select", "id" },
                { "search-text", "hello world" },
                { "openai-endpoint", openAiEndpoint },
                { "embedding-deployment", embeddingDeployment }
            });

        // Document schema:
        // {
        //   "id": "string",
        //   "text": "string",
        //   "vector": "array",
        //   "_rid": "string",
        //   "_self": "string",
        //   "_attachments": "string",
        //   "_ts": "string",
        // }

        var items = result.AssertProperty("items");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        Assert.NotEmpty(items.EnumerateArray());
        // vec-greeting is the seeded doc with text "Hello world, a friendly greeting...";
        // it should rank first by cosine similarity against the "hello world" query embedding.
        var first = items.EnumerateArray().First();
        Assert.Equal("vec-greeting", first.GetProperty("id").GetString());
        // The score is calculated by Cosmos during the vector search
        Assert.True(first.TryGetProperty("_score", out _), "_score should be present");
        Assert.False(first.TryGetProperty("text", out _), "text property should not be selected");
        Assert.False(first.TryGetProperty("vector", out _), "vector property should be stripped");
    }

    private static string GetStringOrNameElementString(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return element.GetString()!;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            return element.GetProperty("name").GetString()!;
        }

        throw new InvalidOperationException($"Unexpected {propertyName} element ValueKind: {element.ValueKind}");
    }
}
