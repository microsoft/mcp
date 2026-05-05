---
title: Performance Optimization for Fabric REST APIs
description: Learn best practices for optimizing performance when building applications with Microsoft Fabric REST APIs, including batching, parallelization, caching, and payload optimization.
ms.date: 01/25/2026
#customer intent: As a Microsoft Fabric developer I want to learn how to optimize performance when working with Fabric APIs.
---

# Performance Optimization

Building high-performance applications with Microsoft Fabric REST APIs requires understanding optimization techniques for API calls, data transfer, and resource utilization. This guide covers strategies to minimize latency, reduce API calls, and improve overall application responsiveness.

## Performance Principles

### Key Optimization Areas

1. **Reduce API Calls** - Batch operations, cache responses
2. **Minimize Latency** - Parallelize requests, use appropriate regions
3. **Optimize Payloads** - Request only needed fields, compress data
4. **Manage Concurrency** - Balance throughput with rate limits

## Batch Operations

### Combining Multiple Operations

Instead of individual API calls, batch operations when possible:

```csharp
// ❌ Inefficient: Individual calls
foreach (var itemId in itemIds)
{
    await GetItemAsync(client, workspaceId, itemId);
}

// ✅ Efficient: Parallel batch with concurrency control
public async Task<List<Item>> GetItemsBatchAsync(
    HttpClient client,
    string workspaceId,
    List<string> itemIds,
    int maxConcurrency = 10)
{
    using var semaphore = new SemaphoreSlim(maxConcurrency);
    
    var tasks = itemIds.Select(async itemId =>
    {
        await semaphore.WaitAsync();
        try
        {
            return await GetItemAsync(client, workspaceId, itemId);
        }
        finally
        {
            semaphore.Release();
        }
    });
    
    var results = await Task.WhenAll(tasks);
    return results.ToList();
}
```

### Python Parallel Requests

```python
import asyncio
import aiohttp
from typing import List

async def get_items_batch(
    session: aiohttp.ClientSession,
    workspace_id: str,
    item_ids: List[str],
    max_concurrency: int = 10
) -> List[dict]:
    """Fetch multiple items in parallel with concurrency control."""
    
    semaphore = asyncio.Semaphore(max_concurrency)
    
    async def fetch_with_semaphore(item_id: str) -> dict:
        async with semaphore:
            url = f"https://api.fabric.microsoft.com/v1/workspaces/{workspace_id}/items/{item_id}"
            async with session.get(url) as response:
                return await response.json()
    
    tasks = [fetch_with_semaphore(item_id) for item_id in item_ids]
    return await asyncio.gather(*tasks)
```

### Bulk Create Operations

```csharp
public async Task<List<Item>> CreateItemsBulkAsync(
    HttpClient client,
    string workspaceId,
    List<ItemCreateRequest> items)
{
    var createdItems = new List<Item>();
    var semaphore = new SemaphoreSlim(5); // Limit concurrent creates
    
    var tasks = items.Select(async item =>
    {
        await semaphore.WaitAsync();
        try
        {
            var response = await client.PostAsJsonAsync(
                $"https://api.fabric.microsoft.com/v1/workspaces/{workspaceId}/items",
                item);
            
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                // Handle long-running operation
                return await PollForResultAsync<Item>(response);
            }
            
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Item>();
        }
        finally
        {
            semaphore.Release();
        }
    });
    
    var results = await Task.WhenAll(tasks);
    return results.Where(r => r != null).ToList()!;
}
```

## Parallel Request Patterns

### Concurrent API Calls with Rate Limiting

```csharp
public class RateLimitedParallelExecutor
{
    private readonly SemaphoreSlim _concurrencySemaphore;
    private readonly SemaphoreSlim _rateLimitSemaphore;
    private readonly int _requestsPerSecond;
    
    public RateLimitedParallelExecutor(int maxConcurrency = 10, int requestsPerSecond = 50)
    {
        _concurrencySemaphore = new SemaphoreSlim(maxConcurrency);
        _rateLimitSemaphore = new SemaphoreSlim(requestsPerSecond);
        _requestsPerSecond = requestsPerSecond;
        
        // Release rate limit tokens periodically
        StartRateLimitRefresher();
    }
    
    private void StartRateLimitRefresher()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1000); // Every second
                
                var currentCount = _rateLimitSemaphore.CurrentCount;
                var toRelease = _requestsPerSecond - currentCount;
                
                if (toRelease > 0)
                {
                    _rateLimitSemaphore.Release(toRelease);
                }
            }
        });
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        await _rateLimitSemaphore.WaitAsync();
        await _concurrencySemaphore.WaitAsync();
        
        try
        {
            return await operation();
        }
        finally
        {
            _concurrencySemaphore.Release();
        }
    }
}
```

### Parallel Workspace Scanning

```csharp
public async Task<Dictionary<string, List<Item>>> ScanAllWorkspacesAsync(
    HttpClient client,
    string? itemTypeFilter = null)
{
    // Get all workspaces first
    var workspaces = await GetAllWorkspacesAsync(client);
    
    var executor = new RateLimitedParallelExecutor(maxConcurrency: 10);
    var results = new ConcurrentDictionary<string, List<Item>>();
    
    var tasks = workspaces.Select(async workspace =>
    {
        var items = await executor.ExecuteAsync(async () =>
        {
            return await GetWorkspaceItemsAsync(client, workspace.Id, itemTypeFilter);
        });
        
        results[workspace.Id] = items;
    });
    
    await Task.WhenAll(tasks);
    
    return new Dictionary<string, List<Item>>(results);
}
```

## Caching Strategies

### In-Memory Caching

```csharp
public class FabricApiCache
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _client;
    
    public FabricApiCache(IMemoryCache cache, HttpClient client)
    {
        _cache = cache;
        _client = client;
    }
    
    public async Task<List<Workspace>> GetWorkspacesAsync(bool forceRefresh = false)
    {
        var cacheKey = "workspaces:all";
        
        if (!forceRefresh && _cache.TryGetValue(cacheKey, out List<Workspace>? cached))
        {
            return cached!;
        }
        
        var workspaces = await FetchWorkspacesFromApiAsync();
        
        _cache.Set(cacheKey, workspaces, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        });
        
        return workspaces;
    }
    
    public async Task<Item?> GetItemAsync(string workspaceId, string itemId)
    {
        var cacheKey = $"item:{workspaceId}:{itemId}";
        
        if (_cache.TryGetValue(cacheKey, out Item? cached))
        {
            return cached;
        }
        
        var item = await FetchItemFromApiAsync(workspaceId, itemId);
        
        if (item != null)
        {
            _cache.Set(cacheKey, item, TimeSpan.FromMinutes(10));
        }
        
        return item;
    }
    
    public void InvalidateWorkspaceCache(string workspaceId)
    {
        // Remove all cached items for this workspace
        // Implementation depends on cache provider capabilities
    }
}
```

### Distributed Caching with Redis

```csharp
public class DistributedFabricCache
{
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan expiration)
    {
        var cached = await _cache.GetStringAsync(key);
        
        if (cached != null)
        {
            return JsonSerializer.Deserialize<T>(cached, _jsonOptions);
        }
        
        var value = await factory();
        
        if (value != null)
        {
            var serialized = JsonSerializer.Serialize(value, _jsonOptions);
            await _cache.SetStringAsync(key, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
        }
        
        return value;
    }
}
```

### Cache Invalidation Patterns

```csharp
public class CacheInvalidationManager
{
    private readonly IMemoryCache _cache;
    
    // Invalidate on write operations
    public async Task CreateItemAsync(HttpClient client, string workspaceId, ItemCreateRequest request)
    {
        var item = await CreateItemInternalAsync(client, workspaceId, request);
        
        // Invalidate related caches
        InvalidateCache($"workspace:{workspaceId}:items");
        InvalidateCache($"workspace:{workspaceId}:items:{item.Type}");
    }
    
    // Time-based invalidation for metadata
    private readonly Dictionary<string, TimeSpan> _cacheTtls = new()
    {
        ["workspaces"] = TimeSpan.FromMinutes(5),
        ["capacities"] = TimeSpan.FromMinutes(15),
        ["items"] = TimeSpan.FromMinutes(2),
        ["item-definitions"] = TimeSpan.FromMinutes(10)
    };
}
```

## Payload Optimization

### Request Only Needed Fields

Some APIs support field selection:

```csharp
public async Task<List<WorkspaceSummary>> GetWorkspaceSummariesAsync(HttpClient client)
{
    // If API supports $select, use it
    var response = await client.GetAsync(
        "https://api.fabric.microsoft.com/v1/workspaces?$select=id,displayName,type");
    
    // Parse only the fields you need
    var fullResponse = await response.Content.ReadFromJsonAsync<WorkspaceListResponse>();
    
    return fullResponse?.Value?.Select(w => new WorkspaceSummary
    {
        Id = w.Id,
        DisplayName = w.DisplayName
    }).ToList() ?? new List<WorkspaceSummary>();
}
```

### Compress Request Bodies

```csharp
public class CompressedHttpClient
{
    private readonly HttpClient _client;
    
    public async Task<HttpResponseMessage> PostCompressedAsync<T>(string url, T content)
    {
        var json = JsonSerializer.Serialize(content);
        var compressed = CompressGzip(Encoding.UTF8.GetBytes(json));
        
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new ByteArrayContent(compressed)
        };
        
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request.Content.Headers.ContentEncoding.Add("gzip");
        
        return await _client.SendAsync(request);
    }
    
    private static byte[] CompressGzip(byte[] data)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionMode.Compress))
        {
            gzip.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }
}
```

### Efficient Pagination

```csharp
public async IAsyncEnumerable<Item> StreamAllItemsAsync(
    HttpClient client,
    string workspaceId,
    [EnumeratorCancellation] CancellationToken ct = default)
{
    string? continuationToken = null;
    
    do
    {
        var url = $"https://api.fabric.microsoft.com/v1/workspaces/{workspaceId}/items";
        if (continuationToken != null)
        {
            url += $"?continuationToken={Uri.EscapeDataString(continuationToken)}";
        }
        
        var response = await client.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<ItemListResponse>(cancellationToken: ct);
        
        foreach (var item in result?.Value ?? Enumerable.Empty<Item>())
        {
            yield return item;
        }
        
        continuationToken = result?.ContinuationToken;
        
    } while (!string.IsNullOrEmpty(continuationToken));
}

// Usage - process items as they arrive
await foreach (var item in StreamAllItemsAsync(client, workspaceId))
{
    ProcessItem(item);
}
```

## Connection Management

### HTTP Client Best Practices

```csharp
public class OptimizedHttpClientFactory
{
    public static HttpClient CreateOptimizedClient(string accessToken)
    {
        var handler = new SocketsHttpHandler
        {
            // Connection pooling
            PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
            MaxConnectionsPerServer = 20,
            
            // Enable compression
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            
            // Keep-alive
            EnableMultipleHttp2Connections = true
        };
        
        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30),
            DefaultRequestVersion = HttpVersion.Version20
        };
        
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        
        return client;
    }
}
```

### Connection Reuse with IHttpClientFactory

```csharp
// In Startup/Program.cs
services.AddHttpClient("FabricApi", client =>
{
    client.BaseAddress = new Uri("https://api.fabric.microsoft.com/v1/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(10),
    MaxConnectionsPerServer = 20,
    AutomaticDecompression = DecompressionMethods.GZip
});

// Usage
public class FabricService
{
    private readonly HttpClient _client;
    
    public FabricService(IHttpClientFactory clientFactory)
    {
        _client = clientFactory.CreateClient("FabricApi");
    }
}
```

## Performance Monitoring

### Request Timing

```csharp
public class TimedHttpClient
{
    private readonly HttpClient _client;
    private readonly ILogger _logger;
    
    public async Task<T?> GetWithTimingAsync<T>(string url)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await _client.GetAsync(url);
            stopwatch.Stop();
            
            _logger.LogInformation(
                "GET {Url} completed in {Duration}ms with status {StatusCode}",
                url, stopwatch.ElapsedMilliseconds, (int)response.StatusCode);
            
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "GET {Url} failed after {Duration}ms",
                url, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

### Performance Metrics Collection

```csharp
public class PerformanceMetrics
{
    private readonly ConcurrentDictionary<string, List<long>> _requestDurations = new();
    
    public void RecordRequest(string endpoint, long durationMs)
    {
        _requestDurations.AddOrUpdate(
            endpoint,
            new List<long> { durationMs },
            (_, list) => { list.Add(durationMs); return list; });
    }
    
    public PerformanceReport GenerateReport()
    {
        return new PerformanceReport
        {
            EndpointStats = _requestDurations.ToDictionary(
                kvp => kvp.Key,
                kvp => new EndpointStats
                {
                    TotalRequests = kvp.Value.Count,
                    AverageDurationMs = kvp.Value.Average(),
                    P50DurationMs = Percentile(kvp.Value, 50),
                    P95DurationMs = Percentile(kvp.Value, 95),
                    P99DurationMs = Percentile(kvp.Value, 99),
                    MaxDurationMs = kvp.Value.Max()
                })
        };
    }
    
    private static double Percentile(List<long> values, int percentile)
    {
        var sorted = values.OrderBy(v => v).ToList();
        var index = (int)Math.Ceiling(percentile / 100.0 * sorted.Count) - 1;
        return sorted[Math.Max(0, index)];
    }
}
```

## Performance Checklist

### Before Deployment

- [ ] HTTP client pooling configured
- [ ] Appropriate concurrency limits set
- [ ] Caching implemented for frequently accessed data
- [ ] Pagination handled efficiently (streaming)
- [ ] Request compression enabled
- [ ] Timeouts configured appropriately

### Ongoing Optimization

- [ ] Monitor API response times
- [ ] Track cache hit rates
- [ ] Review throttling occurrences
- [ ] Analyze slow endpoints
- [ ] Optimize hot paths

## Key Takeaways

1. **Batch and parallelize** - Reduce total request count with concurrent operations
2. **Respect rate limits** - Use concurrency controls to avoid throttling
3. **Cache aggressively** - Store frequently accessed data with appropriate TTLs
4. **Stream large results** - Use async enumeration for pagination
5. **Reuse connections** - Configure HTTP client pooling properly
6. **Compress payloads** - Enable gzip for large request/response bodies
7. **Monitor performance** - Track metrics to identify optimization opportunities
8. **Request only what you need** - Minimize payload sizes where possible

## Additional Resources

- [Azure Architecture: Performance Tuning](https://learn.microsoft.com/azure/architecture/framework/scalability/performance)
- [HttpClient Best Practices](https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [IHttpClientFactory Usage](https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory)
- [Caching Guidance](https://learn.microsoft.com/azure/architecture/best-practices/caching)
