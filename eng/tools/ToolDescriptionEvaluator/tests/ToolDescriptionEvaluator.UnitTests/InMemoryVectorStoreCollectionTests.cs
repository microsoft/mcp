// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.VectorData;
using ToolSelection.VectorDb;
using Xunit;

namespace ToolDescriptionEvaluator.UnitTests;

public class InMemoryVectorStoreCollectionTests
{
    private static Entry Entry(string id, params float[] vector) => new(id, id, vector);

    private static InMemoryVectorStoreCollection CreateCollection(IEnumerable<Entry>? entries = null)
        => new("tools", new CosineSimilarity(), entries);

    private static async Task<List<VectorSearchResult<Entry>>> SearchAsync(
        InMemoryVectorStoreCollection collection, float[] query, int top, VectorSearchOptions<Entry>? options = null)
    {
        var results = new List<VectorSearchResult<Entry>>();
        await foreach (var result in collection.SearchAsync(query, top, options))
        {
            results.Add(result);
        }

        return results;
    }

    [Fact]
    public void Name_ReturnsProvidedName()
    {
        using var collection = CreateCollection();
        Assert.Equal("tools", collection.Name);
    }

    [Fact]
    public async Task UpsertAsync_ThenGetAsync_ReturnsEntry()
    {
        using var collection = CreateCollection();
        var entry = Entry("a", 1f, 0f);

        await collection.UpsertAsync(entry);

        var result = await collection.GetAsync("a");
        Assert.Equal(entry, result);
        Assert.Equal(1, collection.Count);
    }

    [Fact]
    public async Task GetAsync_MissingKey_ReturnsNull()
    {
        using var collection = CreateCollection();
        Assert.Null(await collection.GetAsync("missing"));
    }

    [Fact]
    public async Task UpsertAsync_ExistingKey_UpdatesInPlace()
    {
        using var collection = CreateCollection();
        await collection.UpsertAsync(Entry("a", 1f, 0f));
        await collection.UpsertAsync(new Entry("a", "updated", new[] { 0f, 1f }));

        Assert.Equal(1, collection.Count);
        var result = await collection.GetAsync("a");
        Assert.NotNull(result);
        Assert.Equal("updated", result!.Metadata);
    }

    [Fact]
    public async Task UpsertAsync_Batch_AddsAllEntries()
    {
        using var collection = CreateCollection();
        var entries = new[] { Entry("b", 0f, 1f), Entry("a", 1f, 0f), Entry("c", 1f, 1f) };

        await collection.UpsertAsync(entries);

        Assert.Equal(3, collection.Count);
        Assert.NotNull(await collection.GetAsync("a"));
        Assert.NotNull(await collection.GetAsync("b"));
        Assert.NotNull(await collection.GetAsync("c"));
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntry()
    {
        using var collection = CreateCollection(new[] { Entry("a", 1f, 0f), Entry("b", 0f, 1f) });

        await collection.DeleteAsync("a");

        Assert.Equal(1, collection.Count);
        Assert.Null(await collection.GetAsync("a"));
        Assert.NotNull(await collection.GetAsync("b"));
    }

    [Fact]
    public async Task DeleteAsync_MissingKey_IsNoOp()
    {
        using var collection = CreateCollection(new[] { Entry("a", 1f, 0f) });

        await collection.DeleteAsync("missing");

        Assert.Equal(1, collection.Count);
    }

    [Fact]
    public async Task EnsureCollectionDeletedAsync_ClearsEntries()
    {
        using var collection = CreateCollection(new[] { Entry("a", 1f, 0f), Entry("b", 0f, 1f) });

        await collection.EnsureCollectionDeletedAsync();

        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public async Task CollectionExistsAsync_ReturnsTrue()
    {
        using var collection = CreateCollection();
        Assert.True(await collection.CollectionExistsAsync());
    }

    [Fact]
    public async Task SearchAsync_ReturnsResultsOrderedByDescendingSimilarity()
    {
        using var collection = CreateCollection(new[]
        {
            Entry("match", 1f, 0f),
            Entry("orthogonal", 0f, 1f),
            Entry("opposite", -1f, 0f),
        });

        var results = await SearchAsync(collection, [1f, 0f], top: 3);

        Assert.Equal(3, results.Count);
        Assert.Equal("match", results[0].Record.Id);
        Assert.Equal("orthogonal", results[1].Record.Id);
        Assert.Equal("opposite", results[2].Record.Id);
        Assert.True(results[0].Score >= results[1].Score);
        Assert.True(results[1].Score >= results[2].Score);
    }

    [Fact]
    public async Task SearchAsync_RespectsTopLimit()
    {
        using var collection = CreateCollection(new[]
        {
            Entry("match", 1f, 0f),
            Entry("orthogonal", 0f, 1f),
            Entry("opposite", -1f, 0f),
        });

        var results = await SearchAsync(collection, [1f, 0f], top: 1);

        Assert.Single(results);
        Assert.Equal("match", results[0].Record.Id);
    }

    [Fact]
    public async Task SearchAsync_AppliesScoreThreshold()
    {
        using var collection = CreateCollection(new[]
        {
            Entry("match", 1f, 0f),
            Entry("orthogonal", 0f, 1f),
            Entry("opposite", -1f, 0f),
        });

        var results = await SearchAsync(collection, [1f, 0f], top: 3,
            new VectorSearchOptions<Entry> { ScoreThreshold = 0.5 });

        Assert.Single(results);
        Assert.Equal("match", results[0].Record.Id);
    }

    [Fact]
    public async Task SearchAsync_AcceptsReadOnlyMemoryAndArrayInputs()
    {
        using var collection = CreateCollection(new[] { Entry("match", 1f, 0f) });

        var arrayResults = new List<VectorSearchResult<Entry>>();
        await foreach (var r in collection.SearchAsync(new float[] { 1f, 0f }, 1))
        {
            arrayResults.Add(r);
        }

        var memoryResults = new List<VectorSearchResult<Entry>>();
        await foreach (var r in collection.SearchAsync(new ReadOnlyMemory<float>(new[] { 1f, 0f }), 1))
        {
            memoryResults.Add(r);
        }

        Assert.Equal("match", Assert.Single(arrayResults).Record.Id);
        Assert.Equal("match", Assert.Single(memoryResults).Record.Id);
    }

    [Fact]
    public async Task SearchAsync_UnsupportedSearchValue_Throws()
    {
        using var collection = CreateCollection(new[] { Entry("match", 1f, 0f) });

        await Assert.ThrowsAsync<NotSupportedException>(async () =>
        {
            await foreach (var _ in collection.SearchAsync("not a vector", 1))
            {
            }
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchAsync_NonPositiveTop_Throws(int top)
    {
        using var collection = CreateCollection(new[] { Entry("match", 1f, 0f) });

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await foreach (var _ in collection.SearchAsync(new float[] { 1f, 0f }, top))
            {
            }
        });
    }

    [Fact]
    public async Task SearchAsync_WithManyEntries_UsesParallelPathAndKeepsRanking()
    {
        // More than the internal divide-and-conquer threshold (100) to exercise the parallel path.
        var entries = Enumerable.Range(0, 250)
            .Select(i => new Entry($"e{i:D4}", i, new[] { 1f, i / 1000f }))
            .ToList();
        using var collection = CreateCollection(entries);

        var query = new float[] { 1f, 0f };
        var results = await SearchAsync(collection, query, top: 5);

        Assert.Equal(5, results.Count);

        // The nearest neighbours to (1, 0) are the entries with the smallest second component.
        Assert.Equal(new[] { "e0000", "e0001", "e0002", "e0003", "e0004" }, results.Select(r => r.Record.Id));

        for (int i = 1; i < results.Count; i++)
        {
            Assert.True(results[i - 1].Score >= results[i].Score);
        }
    }

    [Fact]
    public async Task SearchAsync_ParallelPathMatchesIndependentRanking()
    {
        var random = new Random(42);
        var entries = Enumerable.Range(0, 200)
            .Select(i => new Entry($"e{i:D4}", i, new[] { (float)random.NextDouble(), (float)random.NextDouble() }))
            .ToList();

        using var parallel = CreateCollection(entries);

        var query = new float[] { 0.5f, 0.5f };

        var parallelTop = (await SearchAsync(parallel, query, top: 10)).Select(r => r.Record.Id).ToList();

        // Recompute the expected top-10 across all entries independently to validate the parallel merge.
        var metric = new CosineSimilarity();
        var expected = entries
            .Select(e => (e.Id, Score: metric.Distance(query, e.Vector.Span)))
            .OrderByDescending(x => x.Score)
            .Take(10)
            .Select(x => x.Id)
            .ToList();

        Assert.Equal(expected, parallelTop);
    }

    [Fact]
    public void GetAsync_WithFilter_Throws()
    {
        using var collection = CreateCollection();

        Assert.Throws<NotSupportedException>(() =>
        {
            _ = collection.GetAsync(e => e.Id == "a", 1);
        });
    }

    [Fact]
    public async Task UpsertAsync_NullRecord_Throws()
    {
        using var collection = CreateCollection();
        await Assert.ThrowsAsync<ArgumentNullException>(() => collection.UpsertAsync((Entry)null!));
    }

    [Fact]
    public async Task GetAsync_NullKey_Throws()
    {
        using var collection = CreateCollection();
        await Assert.ThrowsAsync<ArgumentNullException>(() => collection.GetAsync((string)null!));
    }

    [Fact]
    public void GetService_ReturnsCollectionMetadata()
    {
        using var collection = CreateCollection();
        var metadata = collection.GetService(typeof(VectorStoreCollectionMetadata));

        var typed = Assert.IsType<VectorStoreCollectionMetadata>(metadata);
        Assert.Equal("tools", typed.CollectionName);
    }
}
