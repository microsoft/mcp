// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.VectorData;

namespace ToolSelection.VectorDb;

/// <summary>
/// A record stored in the vector database. The properties are annotated with
/// <c>Microsoft.Extensions.VectorData</c> attributes so the model can be reused with any
/// <see cref="VectorStore"/> provider (for example Azure AI Search, Cosmos DB, Redis, ...).
/// </summary>
public record Entry(
    [property: VectorStoreKey] string Id,
    [property: VectorStoreData] object? Metadata,
    // Default output dimensions of the Azure OpenAI text-embedding-3-large model used by EmbeddingService.
    // The in-memory implementation does not enforce this value; it is metadata for real providers.
    [property: VectorStoreVector(3072, DistanceFunction = DistanceFunction.CosineSimilarity)] ReadOnlyMemory<float> Vector);

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

/// <summary>
/// An in-memory <see cref="VectorStoreCollection{TKey, TRecord}"/> that keeps records sorted by key
/// and performs brute-force nearest-neighbor search using a pluggable <see cref="IDistanceMetric"/>.
/// </summary>
public sealed class InMemoryVectorStoreCollection : VectorStoreCollection<string, Entry>
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly List<Entry> _entries;
    private readonly IDistanceMetric _distanceMetric;
    private readonly IComparer<VectorSearchResult<Entry>> _resultComparer;
    private readonly string _name;
    private readonly VectorStoreCollectionMetadata _metadata;
    private bool _disposed;

    public InMemoryVectorStoreCollection(string name, IDistanceMetric distanceMetric, IEnumerable<Entry>? entries = null)
    {
        _name = name;
        _distanceMetric = distanceMetric;
        _resultComparer = Comparer<VectorSearchResult<Entry>>.Create((a, b) =>
        {
            int result = Nullable.Compare(a.Score, b.Score);
            return distanceMetric.BiggerIsCloser ? -result : result;
        });
        _entries = entries?.OrderBy(e => e.Id, StringComparer.Ordinal).ToList() ?? new();
        _metadata = new VectorStoreCollectionMetadata
        {
            VectorStoreSystemName = "InMemory",
            CollectionName = name
        };
    }

    public override string Name => _name;

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _entries.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    private int BinarySearch(string id)
    {
        return _entries.BinarySearch(new Entry(id, null, ReadOnlyMemory<float>.Empty),
            Comparer<Entry>.Create((a, b) => string.Compare(a.Id, b.Id, StringComparison.Ordinal)));
    }

    public override Task<bool> CollectionExistsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public override Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public override Task EnsureCollectionDeletedAsync(CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock();
        try
        {
            _entries.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    public override Task<Entry?> GetAsync(string key, RecordRetrievalOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        _lock.EnterReadLock();
        try
        {
            int index = BinarySearch(key);
            return Task.FromResult(index >= 0 ? _entries[index] : null);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public override Task UpsertAsync(Entry record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        _lock.EnterWriteLock();
        try
        {
            UpsertCore(record);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    public override Task UpsertAsync(IEnumerable<Entry> records, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(records);

        _lock.EnterWriteLock();
        try
        {
            foreach (var record in records)
            {
                UpsertCore(record);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    // Assumes the write lock is already held.
    private void UpsertCore(Entry record)
    {
        int index = BinarySearch(record.Id);
        if (index >= 0)
        {
            _entries[index] = record;
        }
        else
        {
            _entries.Insert(~index, record);
        }
    }

    public override Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        _lock.EnterWriteLock();
        try
        {
            int index = BinarySearch(key);
            if (index >= 0)
            {
                _entries.RemoveAt(index);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    public override IAsyncEnumerable<Entry> GetAsync(Expression<Func<Entry, bool>> filter, int top, FilteredRecordRetrievalOptions<Entry>? options = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Filtered record retrieval is not supported by the in-memory vector store.");

    public override async IAsyncEnumerable<VectorSearchResult<Entry>> SearchAsync<TInput>(TInput searchValue, int top, VectorSearchOptions<Entry>? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(searchValue);

        if (top <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(top), "The number of search results (top) must be greater than zero.");
        }

        var vector = ExtractVector(searchValue);
        double? scoreThreshold = options?.ScoreThreshold;

        List<VectorSearchResult<Entry>> results;

        _lock.EnterReadLock();
        try
        {
            results = SearchSlice(_entries, vector, top, scoreThreshold);
        }
        finally
        {
            _lock.ExitReadLock();
        }

        foreach (var result in results)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return result;
        }
    }

    private static ReadOnlyMemory<float> ExtractVector(object searchValue) => searchValue switch
    {
        ReadOnlyMemory<float> memory => memory,
        Memory<float> memory => memory,
        float[] array => array,
        _ => throw new NotSupportedException(
            $"Search value of type '{searchValue.GetType()}' is not supported. Provide a ReadOnlyMemory<float> or float[] embedding.")
    };

    private List<VectorSearchResult<Entry>> SearchSlice(List<Entry> entries, ReadOnlyMemory<float> vector, int top, double? scoreThreshold)
    {
        const int threshold = 100;

        if (entries.Count > threshold)
        {
            int half = entries.Count / 2;
            var left = entries.Take(half).ToList();
            var right = entries.Skip(half).ToList();

            var leftTask = Task.Run(() => SearchSlice(left, vector, top, scoreThreshold));
            var rightResult = SearchSlice(right, vector, top, scoreThreshold);
            var leftResult = leftTask.Result;

            return Merge(leftResult, rightResult, top);
        }

        var results = new List<VectorSearchResult<Entry>>();

        foreach (var entry in entries)
        {
            float score = _distanceMetric.Distance(vector.Span, entry.Vector.Span);
            if (scoreThreshold.HasValue && score < scoreThreshold.Value)
            {
                continue;
            }

            var candidate = new VectorSearchResult<Entry>(entry, score);

            int insertIndex = results.BinarySearch(candidate, _resultComparer);
            if (insertIndex < 0)
            {
                insertIndex = ~insertIndex;
            }

            if (insertIndex == top)
            {
                // Score is worse than all current results, skip.
                continue;
            }

            if (results.Count == top)
            {
                // Remove the worst result.
                results.RemoveAt(results.Count - 1);
            }

            results.Insert(insertIndex, candidate);
        }

        return results;
    }

    private List<VectorSearchResult<Entry>> Merge(List<VectorSearchResult<Entry>> left, List<VectorSearchResult<Entry>> right, int top)
    {
        var merged = new List<VectorSearchResult<Entry>>(Math.Min(top, left.Count + right.Count));
        int leftIndex = 0, rightIndex = 0;

        while (leftIndex < left.Count && rightIndex < right.Count && merged.Count < top)
        {
            if (_resultComparer.Compare(left[leftIndex], right[rightIndex]) <= 0)
            {
                merged.Add(left[leftIndex++]);
            }
            else
            {
                merged.Add(right[rightIndex++]);
            }
        }

        while (leftIndex < left.Count && merged.Count < top)
        {
            merged.Add(left[leftIndex++]);
        }

        while (rightIndex < right.Count && merged.Count < top)
        {
            merged.Add(right[rightIndex++]);
        }

        return merged;
    }

    public override object? GetService(Type serviceType, object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        return
            serviceKey is not null ? null :
            serviceType == typeof(VectorStoreCollectionMetadata) ? _metadata :
            serviceType.IsInstanceOfType(this) ? this :
            null;
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _lock.Dispose();
            }

            _disposed = true;
        }

        base.Dispose(disposing);
    }
}
