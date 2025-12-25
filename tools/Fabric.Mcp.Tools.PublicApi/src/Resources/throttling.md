---
title: Handle API Throttling in Fabric REST APIs
description: Learn how to handle throttling and rate limiting when building applications that interact with Microsoft Fabric REST APIs, including retry strategies and best practices.
ms.date: 12/24/2024
#customer intent: As a Microsoft Fabric developer I want to learn how to handle API throttling and implement proper retry logic in my applications.
---

# API Throttling and Rate Limiting

Microsoft Fabric REST APIs implement throttling and rate limiting to ensure service reliability and fair resource allocation across all users. Understanding and properly handling throttling is essential for building robust applications that interact with Fabric APIs.

## What is API Throttling?

API throttling is a mechanism that limits the number of requests a client can make to an API within a specific time window. When the limit is exceeded, the API returns an error response and instructs the client to wait before retrying.

Throttling helps:
- Maintain service stability and performance
- Prevent resource exhaustion
- Ensure fair access for all users
- Protect against denial-of-service scenarios

## Throttling Response

When your application exceeds the rate limit, Fabric APIs respond with:

- **HTTP Status Code**: `429 Too Many Requests`
- **Retry-After Header**: Contains the number of seconds to wait before retrying

### Example Throttled Response

```http
HTTP/1.1 429 Too Many Requests
Retry-After: 30
Content-Type: application/json

{
  "error": {
    "code": "TooManyRequests",
    "message": "The request has been throttled. Please retry after 30 seconds."
  }
}
```

## Best Practices for Handling Throttling

### 1. Implement Exponential Backoff with Retry-After

Always respect the `Retry-After` header value and implement exponential backoff for subsequent retries.

#### C# Example

```csharp
using System.Net.Http.Headers;

public async Task<HttpResponseMessage> MakeRequestWithRetryAsync(
    HttpClient client, 
    string requestUrl, 
    int maxRetries = 3)
{
    int retryCount = 0;
    
    while (retryCount < maxRetries)
    {
        var response = await client.GetAsync(requestUrl);
        
        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            // Get the Retry-After header value
            int retryAfterSeconds = 60; // Default fallback
            
            if (response.Headers.TryGetValues("Retry-After", out var values))
            {
                if (int.TryParse(values.First(), out int parsedValue))
                {
                    retryAfterSeconds = parsedValue;
                }
            }
            
            Console.WriteLine($"Throttled. Waiting {retryAfterSeconds} seconds before retry {retryCount + 1}/{maxRetries}");
            
            // Wait for the specified duration
            await Task.Delay(TimeSpan.FromSeconds(retryAfterSeconds));
            
            retryCount++;
            continue;
        }
        
        // Return successful response or other error codes
        return response;
    }
    
    throw new HttpRequestException($"Request failed after {maxRetries} retries due to throttling");
}
```

#### Python Example

```python
import requests
import time

def make_request_with_retry(url, headers, max_retries=3):
    """Make HTTP request with throttling retry logic."""
    retry_count = 0
    
    while retry_count < max_retries:
        response = requests.get(url, headers=headers)
        
        if response.status_code == 429:
            # Get Retry-After header (defaults to 60 seconds)
            retry_after = int(response.headers.get('Retry-After', 60))
            
            print(f"Throttled. Waiting {retry_after} seconds before retry {retry_count + 1}/{max_retries}")
            
            # Wait for the specified duration
            time.sleep(retry_after)
            
            retry_count += 1
            continue
        
        # Return response for success or other errors
        return response
    
    raise Exception(f"Request failed after {max_retries} retries due to throttling")
```

#### TypeScript/JavaScript Example

```typescript
async function makeRequestWithRetry(
    url: string, 
    headers: Record<string, string>, 
    maxRetries: number = 3
): Promise<Response> {
    let retryCount = 0;
    
    while (retryCount < maxRetries) {
        const response = await fetch(url, { headers });
        
        if (response.status === 429) {
            // Get Retry-After header (defaults to 60 seconds)
            const retryAfter = parseInt(response.headers.get('Retry-After') || '60', 10);
            
            console.log(`Throttled. Waiting ${retryAfter} seconds before retry ${retryCount + 1}/${maxRetries}`);
            
            // Wait for the specified duration
            await new Promise(resolve => setTimeout(resolve, retryAfter * 1000));
            
            retryCount++;
            continue;
        }
        
        // Return response for success or other errors
        return response;
    }
    
    throw new Error(`Request failed after ${maxRetries} retries due to throttling`);
}
```

### 2. Implement Circuit Breaker Pattern

For high-volume applications, implement a circuit breaker to prevent cascading failures:

```csharp
public class CircuitBreaker
{
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private readonly int _failureThreshold = 5;
    private readonly TimeSpan _resetTimeout = TimeSpan.FromMinutes(1);
    
    public enum CircuitState { Closed, Open, HalfOpen }
    
    public CircuitState State { get; private set; } = CircuitState.Closed;
    
    public bool CanExecute()
    {
        if (State == CircuitState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _resetTimeout)
            {
                State = CircuitState.HalfOpen;
                return true;
            }
            return false;
        }
        return true;
    }
    
    public void RecordSuccess()
    {
        _failureCount = 0;
        State = CircuitState.Closed;
    }
    
    public void RecordFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;
        
        if (_failureCount >= _failureThreshold)
        {
            State = CircuitState.Open;
        }
    }
}
```

### 3. Use Batch Operations When Available

Reduce the number of API calls by using batch operations where supported:

```csharp
// Instead of multiple individual requests
// for (int i = 0; i < items.Count; i++)
// {
//     await CreateItemAsync(items[i]);
// }

// Use batch operation if available
await CreateItemsBatchAsync(items);
```

### 4. Implement Request Queuing

Queue requests and process them at a controlled rate:

```csharp
using System.Threading;
using System.Threading.Channels;

public class RequestQueue(int maxConcurrentRequests = 5)
{
    private readonly int _maxConcurrentRequests = maxConcurrentRequests;
    private readonly SemaphoreSlim _semaphore = new(maxConcurrentRequests);
    private readonly Channel<Func<Task>> _channel = Channel.CreateUnbounded<Func<Task>>();

    public RequestQueue : this(maxConcurrentRequests)
    {
        // Start a single background consumer that processes queued requests.
        _ = Task.Run(ProcessQueueAsync);
    }

    public async Task EnqueueAsync(Func<Task> request)
    {
        // Enqueue the request; processing is handled by the background loop.
        await _channel.Writer.WriteAsync(request);
    }

    private async Task ProcessQueueAsync()
    {
        await foreach (var request in _channel.Reader.ReadAllAsync())
        {
            await _semaphore.WaitAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    await request();
                }
                finally
                {
                    _semaphore.Release();
                }
            });
        }
    }
}
```

### 5. Cache Responses When Appropriate

Implement caching to reduce redundant API calls:

```csharp
using System.Runtime.Caching;

public class ApiCache
{
    private static readonly MemoryCache _cache = MemoryCache.Default;
    
    public async Task<T> GetOrFetchAsync<T>(
        string cacheKey, 
        Func<Task<T>> fetchFunc, 
        TimeSpan cacheDuration)
    {
        if (_cache.Get(cacheKey) is T cachedValue)
        {
            return cachedValue;
        }
        
        var value = await fetchFunc();
        
        var policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow.Add(cacheDuration)
        };
        
        _cache.Set(cacheKey, value, policy);
        return value;
    }
}
```

### 6. Monitor and Log Throttling Events

Track throttling occurrences to identify patterns and optimize your application:

```csharp
public class ThrottlingMetrics
{
    private int _throttleCount = 0;
    private readonly object _lock = new object();
    
    public void RecordThrottle(string endpoint, int retryAfter)
    {
        lock (_lock)
        {
            _throttleCount++;
        }
        
        // Log to your monitoring system
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Throttled on {endpoint}. Retry after {retryAfter}s. Total throttles: {_throttleCount}");
    }
    
    public int GetThrottleCount()
    {
        lock (_lock)
        {
            return _throttleCount;
        }
    }
}
```

## Advanced Retry Strategy

Combine multiple strategies for robust error handling:

```csharp
public async Task<T> ExecuteWithRetryAsync<T>(
    Func<Task<T>> operation,
    int maxRetries = 3,
    bool useExponentialBackoff = true)
{
    int retryCount = 0;
    
    while (true)
    {
        try
        {
            return await operation();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("429"))
        {
            if (retryCount >= maxRetries)
            {
                throw new Exception($"Operation failed after {maxRetries} retries", ex);
            }
            
            // Calculate delay
            int delaySeconds = useExponentialBackoff 
                ? (int)Math.Pow(2, retryCount) * 5  // 5, 10, 20, 40 seconds
                : 30;  // Fixed 30 seconds
            
            Console.WriteLine($"Retry {retryCount + 1}/{maxRetries} after {delaySeconds}s");
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            
            retryCount++;
        }
    }
}
```

## Key Takeaways

1. **Always respect the Retry-After header** - It provides the optimal wait time
2. **Implement exponential backoff** - Gradually increase wait times for subsequent retries
3. **Use circuit breakers** - Prevent cascading failures in high-volume scenarios
4. **Monitor throttling patterns** - Identify and optimize problematic API usage
5. **Cache when possible** - Reduce unnecessary API calls
6. **Batch operations** - Minimize the total number of requests
7. **Queue requests** - Control concurrency and rate of API calls
8. **Plan for throttling** - Design your application architecture to handle rate limits gracefully

## Additional Resources

- [Microsoft Fabric REST API Documentation](https://learn.microsoft.com/rest/api/fabric/)
- [Azure Architecture: Retry Pattern](https://learn.microsoft.com/azure/architecture/patterns/retry)
- [Azure Architecture: Circuit Breaker Pattern](https://learn.microsoft.com/azure/architecture/patterns/circuit-breaker)