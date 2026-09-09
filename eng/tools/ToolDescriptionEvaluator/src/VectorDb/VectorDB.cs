// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.VectorData;

namespace ToolSelection.VectorDb;

/// <summary>
/// A record stored in the vector database. The properties are annotated with
/// <c>Microsoft.Extensions.VectorData</c> attributes so the model can be reused with any
/// <see cref="VectorStore"/> provider (for example Azure AI Search, Cosmos DB, Redis, ...).
/// </summary>
public record Entry
{
    public Entry(string id, object? metadata, ReadOnlyMemory<float> vector)
    {
        Id = id;
        Metadata = metadata;
        Vector = vector;
    }

    [VectorStoreKey]
    public string Id { get; init; }

    [VectorStoreData]
    public object? Metadata { get; init; }

    // Default output dimensions of the Azure OpenAI text-embedding-3-large model used by EmbeddingService.
    // The in-memory implementation does not enforce this value; it is metadata for real providers.
    [VectorStoreVector(3072, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; init; }
}

public interface IDistanceMetric
{
    float Distance(ReadOnlySpan<float> a, ReadOnlySpan<float> b);
    bool BiggerIsCloser { get; }
}

public class CosineSimilarity : IDistanceMetric
{
    public bool BiggerIsCloser => true;  // Cosine similarity: 1 = most similar, -1 = least similar

    public float Distance(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vector lengths must match");

        return TensorPrimitives.CosineSimilarity(a, b);
    }
}

public class DotProduct : IDistanceMetric
{
    public bool BiggerIsCloser => true;

    public float Distance(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vector lengths must match");

        return TensorPrimitives.Dot(a, b);
    }
}

/// <summary>
/// An in-memory <see cref="VectorStore"/> implementation of <c>Microsoft.Extensions.VectorData</c>.
/// It exposes named <see cref="VectorStoreCollection{TKey, TRecord}"/> instances so the current
/// custom implementation can be swapped for any of the available providers with little to no code changes.
/// </summary>
public sealed class VectorDB : VectorStore
{
    public const string DefaultCollectionName = "tools";

    private readonly IDistanceMetric _distanceMetric;
    private readonly Dictionary<string, InMemoryVectorStoreCollection> _collections = new(StringComparer.Ordinal);
    private readonly object _sync = new();
    private readonly VectorStoreMetadata _metadata = new() { VectorStoreSystemName = "InMemory" };
    private bool _disposed;

    public VectorDB(IDistanceMetric distanceMetric, IEnumerable<Entry>? entries = null)
    {
        _distanceMetric = distanceMetric;

        if (entries != null)
        {
            _collections[DefaultCollectionName] = new InMemoryVectorStoreCollection(DefaultCollectionName, distanceMetric, entries);
        }
    }

    /// <summary>Gets the default, strongly-typed collection used by the evaluator.</summary>
    public InMemoryVectorStoreCollection DefaultCollection => GetOrCreateCollection(DefaultCollectionName);

    private InMemoryVectorStoreCollection GetOrCreateCollection(string name)
    {
        lock (_sync)
        {
            if (!_collections.TryGetValue(name, out var collection))
            {
                collection = new InMemoryVectorStoreCollection(name, _distanceMetric);
                _collections[name] = collection;
            }

            return collection;
        }
    }

    [RequiresUnreferencedCode("Uses generic type checks that may be trimmed.")]
    [RequiresDynamicCode("Uses generic type checks that may require dynamic code.")]
    public override VectorStoreCollection<TKey, TRecord> GetCollection<TKey, TRecord>(string name, VectorStoreCollectionDefinition? definition = null)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (typeof(TKey) != typeof(string) || typeof(TRecord) != typeof(Entry))
        {
            throw new NotSupportedException($"This store only supports collections of <{nameof(String)}, {nameof(Entry)}>.");
        }

        return (VectorStoreCollection<TKey, TRecord>)(object)GetOrCreateCollection(name);
    }

    public override VectorStoreCollection<object, Dictionary<string, object?>> GetDynamicCollection(string name, VectorStoreCollectionDefinition definition)
        => throw new NotSupportedException("Dynamic collections are not supported by the in-memory vector store.");

    public override async IAsyncEnumerable<string> ListCollectionNamesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        List<string> names;

        lock (_sync)
        {
            names = _collections.Keys.ToList();
        }

        foreach (var name in names)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return name;
        }
    }

    public override Task<bool> CollectionExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            return Task.FromResult(_collections.ContainsKey(name));
        }
    }

    public override Task EnsureCollectionDeletedAsync(string name, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (_collections.Remove(name, out var collection))
            {
                collection.Dispose();
            }
        }

        return Task.CompletedTask;
    }

    public override object? GetService(Type serviceType, object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        return
            serviceKey is not null ? null :
            serviceType == typeof(VectorStoreMetadata) ? _metadata :
            serviceType.IsInstanceOfType(this) ? this :
            null;
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                lock (_sync)
                {
                    foreach (var collection in _collections.Values)
                    {
                        collection.Dispose();
                    }

                    _collections.Clear();
                }
            }

            _disposed = true;
        }

        base.Dispose(disposing);
    }
}
