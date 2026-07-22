// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.VectorData;
using ToolSelection.VectorDb;
using Xunit;

namespace ToolDescriptionEvaluator.UnitTests;

public class VectorDBTests
{
    private static Entry Entry(string id, params float[] vector) => new(id, id, vector);

    [Fact]
    public void Constructor_WithEntries_SeedsDefaultCollection()
    {
        var entries = new[] { Entry("a", 1f, 0f), Entry("b", 0f, 1f) };
        using var store = new VectorDB(new CosineSimilarity(), entries);

        Assert.Equal(2, store.DefaultCollection.Count);
    }

    [Fact]
    public void GetCollection_StringEntry_ReturnsCollection()
    {
        using var store = new VectorDB(new CosineSimilarity());
        var collection = store.GetCollection<string, Entry>("custom");

        Assert.IsType<InMemoryVectorStoreCollection>(collection);
        Assert.Equal("custom", collection.Name);
    }

    [Fact]
    public void GetCollection_SameName_ReturnsSameInstance()
    {
        using var store = new VectorDB(new CosineSimilarity());
        var first = store.GetCollection<string, Entry>("custom");
        var second = store.GetCollection<string, Entry>("custom");

        Assert.Same(first, second);
    }

    [Fact]
    public void GetCollection_UnsupportedTypes_Throws()
    {
        using var store = new VectorDB(new CosineSimilarity());
        Assert.Throws<NotSupportedException>(() =>
        {
            _ = store.GetCollection<int, Entry>("bad");
        });
    }

    [Fact]
    public void GetDynamicCollection_Throws()
    {
        using var store = new VectorDB(new CosineSimilarity());
        Assert.Throws<NotSupportedException>(() =>
        {
            _ = store.GetDynamicCollection("bad", new VectorStoreCollectionDefinition());
        });
    }

    [Fact]
    public async Task CollectionExistsAsync_ReflectsCreatedCollections()
    {
        using var store = new VectorDB(new CosineSimilarity());

        Assert.False(await store.CollectionExistsAsync("custom", TestContext.Current.CancellationToken));
        _ = store.GetCollection<string, Entry>("custom");
        Assert.True(await store.CollectionExistsAsync("custom", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListCollectionNamesAsync_ReturnsCreatedCollections()
    {
        using var store = new VectorDB(new CosineSimilarity());
        _ = store.DefaultCollection;
        _ = store.GetCollection<string, Entry>("custom");

        var names = new List<string>();
        await foreach (var name in store.ListCollectionNamesAsync(TestContext.Current.CancellationToken))
        {
            names.Add(name);
        }

        Assert.Contains(VectorDB.DefaultCollectionName, names);
        Assert.Contains("custom", names);
    }

    [Fact]
    public async Task EnsureCollectionDeletedAsync_RemovesCollection()
    {
        using var store = new VectorDB(new CosineSimilarity());
        _ = store.GetCollection<string, Entry>("custom");

        await store.EnsureCollectionDeletedAsync("custom", TestContext.Current.CancellationToken);

        Assert.False(await store.CollectionExistsAsync("custom", TestContext.Current.CancellationToken));
    }

    [Fact]
    public void GetService_ReturnsStoreMetadata()
    {
        using var store = new VectorDB(new CosineSimilarity());
        var metadata = store.GetService(typeof(VectorStoreMetadata));

        var typed = Assert.IsType<VectorStoreMetadata>(metadata);
        Assert.Equal("InMemory", typed.VectorStoreSystemName);
    }
}
