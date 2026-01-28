# Making Session Affinity Work with TestServer

## Current Problem

The session affinity middleware fails with TestServer because:

1. **Network-based forwarding**: The middleware uses YARP's `IHttpForwarder` to forward requests to other server instances based on stored addresses (e.g., `http://localhost:5001`). This expects real network endpoints.

2. **In-memory handlers**: TestServer uses in-memory `HttpMessageHandler` instances, not network sockets. When the middleware tries to forward to `http://localhost:5001`, it makes a real HTTP call that fails because no server is listening on that port.

3. **Address resolution**: The middleware resolves addresses from `IServerAddressesFeature`, which for TestServer returns fake addresses like `http://localhost/` that don't correspond to actual network bindings.

## What It Would Take

### Option 1: Custom Test-Aware Forwarder (Recommended)

Create a test-specific implementation of request forwarding that uses in-memory handlers instead of network calls.

**Changes needed:**

1. **Abstract the forwarding logic**:

```csharp
public interface ISessionForwarder
{
    Task<ForwarderError> ForwardAsync(
        HttpContext context, 
        string targetAddress, 
        CancellationToken cancellationToken);
}

// Production implementation uses YARP
internal class YarpSessionForwarder : ISessionForwarder { ... }

// Test implementation uses handler lookup
internal class InMemorySessionForwarder : ISessionForwarder { ... }
```

1. **Create a handler registry for tests**:

```csharp
public interface ITestServerRegistry
{
    void Register(string address, HttpMessageHandler handler);
    HttpMessageHandler? GetHandler(string address);
}
```

1. **Modify SessionAffinityEndpointFilter**:

```csharp
public SessionAffinityEndpointFilter(
    ISessionStore sessionStore,
    ISessionForwarder forwarder,  // Instead of IHttpForwarder
    ...
)
```

1. **Test setup**:

```csharp
var registry = new TestServerRegistry();
services.AddSingleton<ITestServerRegistry>(registry);
services.AddSingleton<ISessionForwarder, InMemorySessionForwarder>();

// After creating test servers:
registry.Register("http://localhost:5001", server1.CreateHandler());
registry.Register("http://localhost:5002", server2.CreateHandler());
```

**Pros**:

- Clean separation between test and production code
- Preserves TestServer's in-memory benefits
- No network overhead in tests

**Cons**:

- Requires refactoring the middleware
- Adds test-specific interfaces
- More complex implementation

---

### Option 2: Mock IHttpForwarder for Tests

Create a test double that intercepts forwarding calls and routes to in-memory handlers.

**Changes needed:**

1. **Create mock forwarder**:

```csharp
public class TestServerHttpForwarder : IHttpForwarder
{
    private readonly Dictionary<string, HttpMessageHandler> _handlers = new();
    
    public void RegisterHandler(string address, HttpMessageHandler handler)
    {
        _handlers[NormalizeAddress(address)] = handler;
    }

    public async Task<ForwarderError> SendAsync(
        HttpContext context,
        string destinationPrefix,
        HttpMessageInvoker httpClient,
        ForwarderRequestConfig requestConfig,
        HttpTransformer? transformer = null)
    {
        if (_handlers.TryGetValue(NormalizeAddress(destinationPrefix), out var handler))
        {
            // Create an HttpClient with the test handler
            using var testClient = new HttpClient(handler, disposeHandler: false);
            
            // Build the destination URI
            var request = new HttpRequestMessage
            {
                Method = new HttpMethod(context.Request.Method),
                RequestUri = new Uri(testClient.BaseAddress, context.Request.Path + context.Request.QueryString)
            };
            
            // Copy headers, body, etc.
            await CopyRequestAsync(context.Request, request);
            
            // Send and copy response
            var response = await testClient.SendAsync(request, context.RequestAborted);
            await CopyResponseAsync(response, context.Response);
            
            return ForwarderError.None;
        }
        
        // Fallback to real forwarding (will fail for TestServer)
        return ForwarderError.RequestCreationFailed;
    }
}
```

2. **Test setup**:

```csharp
var mockForwarder = new TestServerHttpForwarder();
services.AddSingleton<IHttpForwarder>(mockForwarder);

// After creating servers:
mockForwarder.RegisterHandler("http://localhost:5001", server1.CreateHandler());
```

**Pros**:

- No changes to production middleware code
- Simpler than Option 1
- Test-only implementation

**Cons**:

- Need to manually implement request/response copying logic
- YARP's `IHttpForwarder` interface might be complex to fully implement
- Risk of test mock diverging from production behavior

---

### Option 3: Use Real Kestrel Servers (Current Working Solution)

Instead of fighting TestServer's limitations, use real Kestrel servers in integration tests.

**What you already have**:

```csharp
webHost.UseKestrel(options =>
{
    options.ListenLocalhost(port);
});
```

**Pros**:

- Works with existing middleware unchanged
- Tests real production behavior
- No test-specific code in production
- Already implemented and working! ✅

**Cons**:

- Slightly slower test execution (real network stack)
- Requires port management
- Can't run tests in parallel easily (port conflicts)

---

### Option 4: Conditional Forwarding in Middleware

Add a "test mode" to the middleware that disables forwarding.

**Changes needed:**

```csharp
public class SessionAffinityOptions
{
    // Existing options...
    public bool DisableForwarding { get; set; }  // NEW
}

// In SessionAffinityEndpointFilter:
if (ownerInfo.OwnerId != _localOwnerId)
{
    if (_options.DisableForwarding)
    {
        // In test mode, just handle locally even if not owner
        _logger.LogWarning("Forwarding disabled, handling locally despite session on {Owner}", ownerInfo.OwnerId);
        return await next(context);
    }
    
    // Normal forwarding logic...
}
```

**Test setup**:

```csharp
services.AddMcpHttpSessionAffinity(options =>
{
    options.DisableForwarding = true;  // Test mode
    options.LocalServerAddress = $"http://localhost:{port}";
});
```

**Pros**:

- Minimal code changes
- Tests can run with TestServer
- Session tracking still works

**Cons**:

- Doesn't actually test forwarding behavior
- Requires production code to know about testing
- Less valuable tests (can't verify load balancing)

---

## Recommendation

**Use Option 3 (Real Kestrel Servers)** - which you've already implemented!

**Why:**

1. ✅ Already working - `RealServerIntegrationTests.cs` passes
2. ✅ Tests actual production behavior
3. ✅ No test-specific production code
4. ✅ No complex mocking or abstraction layers
5. ✅ Simpler to maintain

**When to use each approach:**

- **Integration tests** (testing session affinity across servers): Use Kestrel (Option 3)
- **Unit tests** (testing middleware logic in isolation): Mock `IHttpForwarder` (Option 2)
- **Feature development** (if you want TestServer to work): Implement Option 1

## Implementation Guide for Option 1 (If Needed)

If you absolutely need TestServer support, here's the implementation roadmap:

### Step 1: Create abstractions

```csharp
// In Abstractions/ISessionForwarder.cs
public interface ISessionForwarder
{
    Task<ForwarderError> ForwardAsync(
        HttpContext context,
        SessionOwnerInfo ownerInfo,
        CancellationToken cancellationToken);
}
```

### Step 2: Implement production version

```csharp
// YarpSessionForwarder.cs
internal class YarpSessionForwarder : ISessionForwarder
{
    private readonly IHttpForwarder _forwarder;
    private readonly HttpMessageInvoker _httpClient;
    private readonly ForwarderRequestConfig _requestConfig;
    
    public Task<ForwarderError> ForwardAsync(...)
    {
        return _forwarder.SendAsync(
            context, 
            ownerInfo.Address, 
            _httpClient, 
            _requestConfig);
    }
}
```

### Step 3: Implement test version

```csharp
// In test project: InMemorySessionForwarder.cs
public class InMemorySessionForwarder : ISessionForwarder
{
    private readonly Dictionary<string, HttpMessageHandler> _handlers = new();
    
    public void RegisterHandler(string address, HttpMessageHandler handler)
    {
        _handlers[address] = handler;
    }
    
    public async Task<ForwarderError> ForwardAsync(
        HttpContext context,
        SessionOwnerInfo ownerInfo,
        CancellationToken cancellationToken)
    {
        if (!_handlers.TryGetValue(ownerInfo.Address, out var handler))
        {
            return ForwarderError.RequestCreationFailed;
        }
        
        using var client = new HttpClient(handler, disposeHandler: false)
        {
            BaseAddress = new Uri(ownerInfo.Address)
        };
        
        // Forward request through in-memory handler
        var request = await CreateForwardRequestAsync(context.Request);
        var response = await client.SendAsync(request, cancellationToken);
        await CopyResponseAsync(response, context.Response);
        
        return ForwarderError.None;
    }
}
```

### Step 4: Update service registration

```csharp
// ServiceCollectionExtensions.cs
public static IServiceCollection AddMcpHttpSessionAffinity(...)
{
    services.AddSingleton<ISessionForwarder, YarpSessionForwarder>();
    // ... other registrations
}
```

### Step 5: Update tests

```csharp
var forwarder = new InMemorySessionForwarder();
services.AddSingleton<ISessionForwarder>(forwarder);

// After creating servers:
forwarder.RegisterHandler("http://localhost:5001", server1.CreateHandler());
forwarder.RegisterHandler("http://localhost:5002", server2.CreateHandler());
```

**Estimated effort**: 4-6 hours of development + testing

---

## Conclusion

You've already solved this problem with **Option 3 (Kestrel servers)**. The tests in `RealServerIntegrationTests.cs` are passing and provide good coverage of the session affinity behavior.

If you want TestServer support for other reasons (faster test execution, easier debugging), go with **Option 1** for a clean architectural solution, or **Option 2** for a quicker test-only fix.
