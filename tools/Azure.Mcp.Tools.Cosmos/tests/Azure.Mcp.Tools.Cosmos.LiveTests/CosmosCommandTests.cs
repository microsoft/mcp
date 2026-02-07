// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.LiveTests;

public class CosmosCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture) : RecordedCommandTestsBase(output, fixture, liveServerFixture)
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
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..resourceId"){
            Regex = "resource[Gg]roups/([^?\\/]+)",
            Value = "Sanitized",
            GroupForReplace = "1"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..id"){
            Regex = "resource[Gg]roups/([^?\\/]+)",
            Value = "Sanitized",
            GroupForReplace = "1"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..resourceId"){
            Regex = "subscriptions/([^?\\/]+)",
            Value = "00000000-0000-0000-0000-000000000000",
            GroupForReplace = "1"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..id"){
            Regex = "subscriptions/([^?\\/]+)",
            Value = "00000000-0000-0000-0000-000000000000",
            GroupForReplace = "1"
        })
    ];

    [Fact]
    public async Task Should_list_storage_accounts_by_subscription_id()
    {
        var result = await CallToolAsync(
            "cosmos_database_list",
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
    public async Task Should_list_cosmos_database_containers()
    {
        var result = await CallToolAsync(
            "cosmos_database_container_list",
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
    public async Task Should_list_cosmos_database_containers_by_database_name()
    {
        var result = await CallToolAsync(
            "cosmos_database_container_list",
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
        var resourceBaseName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
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
            "cosmos_account_list",
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
        var resourceBaseName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
        var dbResult = await CallToolAsync(
            "cosmos_database_list",
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
            "cosmos_database_container_list",
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
        var resourceBaseName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
        var dbResult = await CallToolAsync(
            "cosmos_database_list",
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
                "cosmos_database_container_list",
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
