// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Tests.Client.Helpers;
using Microsoft.Azure.Cosmos;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.LiveTests;

public class CosmosDbFixture : IAsyncLifetime
{
    private CosmosClient? _client;

    public async ValueTask InitializeAsync()
    {
        var settingsFixture = new LiveTestSettingsFixture();
        await settingsFixture.InitializeAsync();

        _client = new CosmosClient(
            accountEndpoint: $"https://{settingsFixture.Settings.ResourceBaseName}.documents.azure.com:443/",
            tokenCredential: new CustomChainedCredential()
        );
        Container container = _client.GetContainer("ToDoList", "Items");
        ToDoItem entry = new ToDoItem();
        Stream stream = ToStream(entry);
        await container.UpsertItemStreamAsync(streamPayload: stream, partitionKey: new PartitionKey(entry.id)
        );
    }

    public ValueTask DisposeAsync()
    {
        _client?.Dispose();
        return ValueTask.CompletedTask;
    }

    private static Stream ToStream(ToDoItem input)
    {
        MemoryStream stream = new();
        JsonSerializer.Serialize(stream, input, JsonContext.Default.ToDoItem);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ToDoItem))]
[JsonSerializable(typeof(List<ToDoItem>))]
public partial class JsonContext : JsonSerializerContext { }

public class ToDoItem
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public string title { get; set; } = "Test Task";
    public bool completed { get; set; } = false;
}
