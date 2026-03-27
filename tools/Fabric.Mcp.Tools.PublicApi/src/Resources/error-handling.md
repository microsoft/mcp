---
title: Error Handling and Resilience for Fabric REST APIs
description: Learn how to handle errors, implement resilience patterns, and build robust applications that gracefully handle failures when working with Microsoft Fabric REST APIs.
ms.date: 01/25/2026
#customer intent: As a Microsoft Fabric developer I want to learn how to handle errors and build resilient applications with Fabric APIs.
---

# Error Handling and Resilience

Building robust applications that interact with Microsoft Fabric REST APIs requires comprehensive error handling and resilience patterns. This guide covers common error scenarios, classification strategies, and implementation patterns for reliable Fabric integrations.

## Understanding Fabric API Errors

### Standard Error Response Format

Fabric APIs return errors in a consistent JSON format:

```json
{
  "error": {
    "code": "ItemNotFound",
    "message": "The requested item could not be found.",
    "details": [
      {
        "code": "ResourceNotFound",
        "message": "Workspace with ID 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx' does not exist."
      }
    ]
  }
}
```

### Common HTTP Status Codes

| Status Code | Meaning | Typical Cause | Recommended Action |
|-------------|---------|---------------|-------------------|
| 400 | Bad Request | Invalid request body or parameters | Fix request and retry |
| 401 | Unauthorized | Invalid or expired token | Refresh token and retry |
| 403 | Forbidden | Insufficient permissions | Check user permissions |
| 404 | Not Found | Resource doesn't exist | Verify resource ID/name |
| 409 | Conflict | Resource state conflict | Resolve conflict, then retry |
| 429 | Too Many Requests | Rate limit exceeded | Wait and retry with backoff |
| 500 | Internal Server Error | Service error | Retry with backoff |
| 502 | Bad Gateway | Service temporarily unavailable | Retry with backoff |
| 503 | Service Unavailable | Service overloaded | Retry with backoff |
| 504 | Gateway Timeout | Request timeout | Retry with backoff |

## Error Classification

Classifying errors helps determine the appropriate response strategy:

### Transient Errors (Retryable)

These errors are temporary and may succeed on retry:

```csharp
public static class ErrorClassifier
{
    private static readonly HashSet<int> TransientStatusCodes = new()
    {
        408, // Request Timeout
        429, // Too Many Requests
        500, // Internal Server Error
        502, // Bad Gateway
        503, // Service Unavailable
        504  // Gateway Timeout
    };
    
    private static readonly HashSet<string> TransientErrorCodes = new()
    {
        "ServiceUnavailable",
        "ServerBusy",
        "TemporaryError",
        "Timeout",
        "InternalError"
    };
    
    public static bool IsTransient(HttpResponseMessage response, FabricError? error)
    {
        if (TransientStatusCodes.Contains((int)response.StatusCode))
            return true;
            
        if (error?.Code != null && TransientErrorCodes.Contains(error.Code))
            return true;
            
        return false;
    }
}
```

### Permanent Errors (Non-Retryable)

These errors require corrective action before retrying:

```csharp
public static bool IsPermanent(HttpResponseMessage response, FabricError? error)
{
    return response.StatusCode switch
    {
        HttpStatusCode.BadRequest => true,      // 400 - Fix request
        HttpStatusCode.Unauthorized => true,    // 401 - Re-authenticate
        HttpStatusCode.Forbidden => true,       // 403 - Check permissions
        HttpStatusCode.NotFound => true,        // 404 - Resource missing
        HttpStatusCode.Conflict => true,        // 409 - Resolve conflict
        HttpStatusCode.Gone => true,            // 410 - Resource deleted
        HttpStatusCode.UnprocessableEntity => true, // 422 - Validation error
        _ => false
    };
}
```

## Implementing Retry Logic

### Basic Retry with Exponential Backoff

```csharp
public class RetryHandler
{
    private readonly int _maxRetries;
    private readonly TimeSpan _baseDelay;
    private readonly TimeSpan _maxDelay;
    
    public RetryHandler(int maxRetries = 3, int baseDelayMs = 1000, int maxDelayMs = 30000)
    {
        _maxRetries = maxRetries;
        _baseDelay = TimeSpan.FromMilliseconds(baseDelayMs);
        _maxDelay = TimeSpan.FromMilliseconds(maxDelayMs);
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken ct = default)
    {
        int attempt = 0;
        
        while (true)
        {
            try
            {
                return await operation();
            }
            catch (FabricApiException ex) when (ex.IsTransient && attempt < _maxRetries)
            {
                attempt++;
                var delay = CalculateDelay(attempt, ex.RetryAfter);
                
                Console.WriteLine($"Transient error (attempt {attempt}/{_maxRetries}). " +
                    $"Retrying in {delay.TotalSeconds:F1}s. Error: {ex.Message}");
                
                await Task.Delay(delay, ct);
            }
        }
    }
    
    private TimeSpan CalculateDelay(int attempt, TimeSpan? retryAfter)
    {
        // Use Retry-After header if provided
        if (retryAfter.HasValue)
            return retryAfter.Value;
        
        // Exponential backoff with jitter
        var exponentialDelay = TimeSpan.FromMilliseconds(
            _baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
        
        // Add random jitter (0-25% of delay)
        var jitter = TimeSpan.FromMilliseconds(
            exponentialDelay.TotalMilliseconds * Random.Shared.NextDouble() * 0.25);
        
        var totalDelay = exponentialDelay + jitter;
        
        // Cap at maximum delay
        return totalDelay > _maxDelay ? _maxDelay : totalDelay;
    }
}
```

### Python Implementation

```python
import asyncio
import random
from typing import TypeVar, Callable, Awaitable

T = TypeVar('T')

class RetryHandler:
    def __init__(self, max_retries: int = 3, base_delay_ms: int = 1000, max_delay_ms: int = 30000):
        self.max_retries = max_retries
        self.base_delay_ms = base_delay_ms
        self.max_delay_ms = max_delay_ms
    
    async def execute(self, operation: Callable[[], Awaitable[T]]) -> T:
        attempt = 0
        
        while True:
            try:
                return await operation()
            except FabricApiError as ex:
                if not ex.is_transient or attempt >= self.max_retries:
                    raise
                
                attempt += 1
                delay = self._calculate_delay(attempt, ex.retry_after)
                
                print(f"Transient error (attempt {attempt}/{self.max_retries}). "
                      f"Retrying in {delay:.1f}s. Error: {ex.message}")
                
                await asyncio.sleep(delay)
    
    def _calculate_delay(self, attempt: int, retry_after: float | None) -> float:
        if retry_after is not None:
            return retry_after
        
        # Exponential backoff with jitter
        exponential_delay = self.base_delay_ms * (2 ** (attempt - 1))
        jitter = exponential_delay * random.random() * 0.25
        total_delay = (exponential_delay + jitter) / 1000  # Convert to seconds
        
        return min(total_delay, self.max_delay_ms / 1000)
```

## Circuit Breaker Pattern

Prevent cascading failures by stopping requests when a service is unhealthy:

```csharp
public class CircuitBreaker
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _openDuration;
    private int _failureCount;
    private DateTime _lastFailureTime;
    private CircuitState _state = CircuitState.Closed;
    private readonly object _lock = new();
    
    public enum CircuitState { Closed, Open, HalfOpen }
    
    public CircuitBreaker(int failureThreshold = 5, int openDurationSeconds = 60)
    {
        _failureThreshold = failureThreshold;
        _openDuration = TimeSpan.FromSeconds(openDurationSeconds);
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        lock (_lock)
        {
            if (_state == CircuitState.Open)
            {
                if (DateTime.UtcNow - _lastFailureTime >= _openDuration)
                {
                    _state = CircuitState.HalfOpen;
                    Console.WriteLine("Circuit breaker entering half-open state");
                }
                else
                {
                    throw new CircuitBreakerOpenException(
                        $"Circuit breaker is open. Retry after {(_lastFailureTime + _openDuration - DateTime.UtcNow).TotalSeconds:F0}s");
                }
            }
        }
        
        try
        {
            var result = await operation();
            
            lock (_lock)
            {
                _failureCount = 0;
                _state = CircuitState.Closed;
            }
            
            return result;
        }
        catch (Exception ex) when (IsTransientException(ex))
        {
            RecordFailure();
            throw;
        }
    }
    
    private void RecordFailure()
    {
        lock (_lock)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            
            if (_failureCount >= _failureThreshold)
            {
                _state = CircuitState.Open;
                Console.WriteLine($"Circuit breaker opened after {_failureCount} failures");
            }
        }
    }
    
    private static bool IsTransientException(Exception ex)
    {
        return ex is FabricApiException { IsTransient: true } or HttpRequestException;
    }
}
```

## Graceful Degradation

Provide fallback behavior when the primary operation fails:

```csharp
public class FabricClientWithFallback
{
    private readonly HttpClient _client;
    private readonly ICache _cache;
    
    public async Task<List<Workspace>> GetWorkspacesAsync()
    {
        try
        {
            var workspaces = await FetchWorkspacesFromApiAsync();
            
            // Update cache on success
            await _cache.SetAsync("workspaces", workspaces, TimeSpan.FromMinutes(15));
            
            return workspaces;
        }
        catch (FabricApiException ex) when (ex.IsTransient)
        {
            Console.WriteLine($"API unavailable, using cached data. Error: {ex.Message}");
            
            // Fallback to cached data
            var cached = await _cache.GetAsync<List<Workspace>>("workspaces");
            if (cached != null)
            {
                return cached;
            }
            
            throw new Exception("API unavailable and no cached data available", ex);
        }
    }
    
    public async Task<WorkspaceInfo> GetWorkspaceInfoAsync(string workspaceId)
    {
        try
        {
            return await FetchWorkspaceDetailsAsync(workspaceId);
        }
        catch (FabricApiException ex) when (ex.StatusCode == 404)
        {
            // Return a placeholder for missing workspace
            return new WorkspaceInfo
            {
                Id = workspaceId,
                DisplayName = "Unknown Workspace",
                IsPlaceholder = true
            };
        }
    }
}
```

## Comprehensive Error Handling Example

```csharp
public class FabricApiClient
{
    private readonly HttpClient _httpClient;
    private readonly RetryHandler _retryHandler;
    private readonly CircuitBreaker _circuitBreaker;
    
    public async Task<T> SendRequestAsync<T>(HttpRequestMessage request)
    {
        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            return await _retryHandler.ExecuteAsync(async () =>
            {
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(content)!;
                }
                
                // Parse error response
                var errorContent = await response.Content.ReadAsStringAsync();
                var error = ParseError(errorContent);
                
                // Throw appropriate exception
                throw CreateException(response, error);
            });
        });
    }
    
    private FabricApiException CreateException(HttpResponseMessage response, FabricError? error)
    {
        var statusCode = (int)response.StatusCode;
        var message = error?.Message ?? $"Request failed with status {statusCode}";
        
        // Extract Retry-After header
        TimeSpan? retryAfter = null;
        if (response.Headers.TryGetValues("Retry-After", out var values))
        {
            if (int.TryParse(values.First(), out var seconds))
            {
                retryAfter = TimeSpan.FromSeconds(seconds);
            }
        }
        
        return new FabricApiException(message, statusCode, error?.Code)
        {
            IsTransient = ErrorClassifier.IsTransient(response, error),
            RetryAfter = retryAfter
        };
    }
}

public class FabricApiException : Exception
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }
    public bool IsTransient { get; init; }
    public TimeSpan? RetryAfter { get; init; }
    
    public FabricApiException(string message, int statusCode, string? errorCode) 
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
```

## Logging Best Practices

Log errors with context for debugging:

```csharp
public class FabricApiLogger
{
    private readonly ILogger _logger;
    
    public void LogApiError(
        HttpRequestMessage request,
        HttpResponseMessage response,
        FabricError? error,
        TimeSpan duration)
    {
        _logger.LogError(
            "Fabric API request failed. " +
            "Method: {Method}, " +
            "Url: {Url}, " +
            "StatusCode: {StatusCode}, " +
            "ErrorCode: {ErrorCode}, " +
            "ErrorMessage: {ErrorMessage}, " +
            "Duration: {Duration}ms, " +
            "RequestId: {RequestId}",
            request.Method,
            request.RequestUri,
            (int)response.StatusCode,
            error?.Code,
            error?.Message,
            duration.TotalMilliseconds,
            response.Headers.GetValues("x-ms-request-id").FirstOrDefault());
    }
}
```

## Key Takeaways

1. **Classify errors correctly** - Distinguish between transient (retryable) and permanent errors
2. **Implement exponential backoff** - Avoid overwhelming the service during recovery
3. **Add jitter to retries** - Prevent thundering herd problems
4. **Use circuit breakers** - Protect your application from cascading failures
5. **Respect Retry-After headers** - Always honor the service's guidance
6. **Implement graceful degradation** - Provide fallback behavior when possible
7. **Log errors with context** - Include request details, timing, and request IDs
8. **Set reasonable timeouts** - Don't wait indefinitely for responses
9. **Test failure scenarios** - Verify your error handling works correctly

## Additional Resources

- [Azure Architecture: Retry Pattern](https://learn.microsoft.com/azure/architecture/patterns/retry)
- [Azure Architecture: Circuit Breaker Pattern](https://learn.microsoft.com/azure/architecture/patterns/circuit-breaker)
- [Polly: .NET Resilience Library](https://github.com/App-vNext/Polly)
- [Microsoft Fabric REST API Documentation](https://learn.microsoft.com/rest/api/fabric/)
