// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.LiveTests;

public class DocumentDbCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    protected override RecordingOptions? RecordingOptions => new()
    {
        HandleRedirects = false
    };

    public override CustomDefaultMatcher? TestMatcher => new()
    {
        IgnoredHeaders = "x-ms-activity-id,x-ms-request-id,x-ms-session-token"
    };

    /// <summary>
    /// Disable default sanitizers that may interfere with DocumentDB responses
    /// </summary>
    public override List<string> DisabledDefaultSanitizers => [.. base.DisabledDefaultSanitizers, "AZSDK3493"];

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        ..base.BodyKeySanitizers,
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..connectionString"){
            Value = "Sanitized"
        })
    ];

    [Fact]
    public async Task Should_connect_and_list_databases()
    {
        // First connect to DocumentDB
        await CallToolAsync(
            "documentdb_connection_connect",
            new()
            {
                { "connection-string", Settings.DeploymentOutputs["DOCUMENTDB_CONNECTION_STRING"] }
            });

        var result = await CallToolAsync(
            "documentdb_database_list_databases",
            new());

        var databasesArray = result.AssertProperty("databases");
        Assert.Equal(JsonValueKind.Array, databasesArray.ValueKind);
        Assert.NotEmpty(databasesArray.EnumerateArray());
    }

    [Fact]
    public async Task Should_sample_documents_from_collection()
    {
        await CallToolAsync(
            "documentdb_connection_connect",
            new()
            {
                { "connection-string", Settings.DeploymentOutputs["DOCUMENTDB_CONNECTION_STRING"] }
            });

        var result = await CallToolAsync(
            "documentdb_collection_sample_documents",
            new()
            {
                { "db-name", "test" },
                { "collection-name", "items" },
                { "sample-size", "5" }
            });

        Assert.NotNull(result);
        Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);
    }

    [Fact]
    public async Task Should_find_documents_in_collection()
    {
        await CallToolAsync(
            "documentdb_connection_connect",
            new()
            {
                { "connection-string", Settings.DeploymentOutputs["DOCUMENTDB_CONNECTION_STRING"] }
            });

        var result = await CallToolAsync(
            "documentdb_document_find_documents",
            new()
            {
                { "db-name", "test" },
                { "collection-name", "items" },
                { "query", "{}" }
            });

        var documentsArray = result.AssertProperty("documents");
        Assert.Equal(JsonValueKind.Array, documentsArray.ValueKind);
    }

    [Fact]
    public async Task Should_list_indexes()
    {
        await CallToolAsync(
            "documentdb_connection_connect",
            new()
            {
                { "connection-string", Settings.DeploymentOutputs["DOCUMENTDB_CONNECTION_STRING"] }
            });

        var result = await CallToolAsync(
            "documentdb_index_list_indexes",
            new()
            {
                { "db-name", "test" },
                { "collection-name", "items" }
            });

        var indexesArray = result.AssertProperty("indexes");
        Assert.Equal(JsonValueKind.Array, indexesArray.ValueKind);
        Assert.NotEmpty(indexesArray.EnumerateArray());
    }

    [Fact]
    public async Task Should_insert_update_and_delete_document()
    {
        await CallToolAsync(
            "documentdb_connection_connect",
            new()
            {
                { "connection-string", Settings.DeploymentOutputs["DOCUMENTDB_CONNECTION_STRING"] }
            });

        // Insert a test document
        var insertResult = await CallToolAsync(
            "documentdb_document_insert_document",
            new()
            {
                { "db-name", "test" },
                { "collection-name", "items" },
                { "document", "{\"testField\": \"originalValue\"}" }
            });

        var insertedId = insertResult.AssertProperty("inserted_id");

        // Update the document
        var updateResult = await CallToolAsync(
            "documentdb_document_update_document",
            new()
            {
                { "db-name", "test" },
                { "collection-name", "items" },
                { "filter", $"{{\"_id\": {{\"$oid\": {insertedId.GetRawText()}}}}}" },
                { "update", "{\"$set\": {\"testField\": \"updatedValue\"}}" }
            });

        var modifiedCount = updateResult.AssertProperty("modified_count");
        Assert.Equal(1, modifiedCount.GetInt32());

        // Clean up - delete the document
        var deleteResult = await CallToolAsync(
            "documentdb_document_delete_document",
            new()
            {
                { "db-name", "test" },
                { "collection-name", "items" },
                { "filter", $"{{\"_id\": {{\"$oid\": {insertedId.GetRawText()}}}}}" }
            });

        var deletedCount = deleteResult.AssertProperty("deleted_count");
        Assert.Equal(1, deletedCount.GetInt32());
    }
}
