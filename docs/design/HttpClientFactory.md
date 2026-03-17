# Using IHttpClientFactory

This document describes how to use `IHttpClientFactory` for HTTP requests in Azure MCP.

## Overview

Azure MCP uses the standard .NET `IHttpClientFactory` for centralized HTTP client management. This provides handler pooling, automatic DNS refresh, and consistent configuration across all HTTP requests.

## Key Features

- **Handler Pooling**: `HttpMessageHandler` instances are pooled and reused (2-minute default lifetime)
- **DNS Refresh**: Handlers are recycled periodically to pick up DNS changes
- **Proxy Support**: Automatic proxy configuration from environment variables
- **Consistent Configuration**: All HttpClient instances share the same timeout, UserAgent, and proxy settings
- **Test Recording Support**: Built-in support for test proxy redirection in debug builds

## Environment Variables

The following environment variables are automatically applied:

- `ALL_PROXY`: Global proxy for all protocols
- `HTTP_PROXY`: Proxy for HTTP requests only
- `HTTPS_PROXY`: Proxy for HTTPS requests only
- `NO_PROXY`: Comma-separated list of hosts that should bypass the proxy

## Usage

### Using in Services

Services should inject `IHttpClientFactory` and create clients as needed:

```csharp
public class MyService(IHttpClientFactory httpClientFactory)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task MakeRequestAsync()
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://api.example.com/endpoint");
    }
}
```

### Setting Custom Timeout

For operations requiring longer timeouts, set it on the client instance:

```csharp
public async Task LongRunningOperationAsync()
{
    var client = _httpClientFactory.CreateClient();
    client.Timeout = TimeSpan.FromMinutes(5); 
    var response = await client.GetAsync(url);
}
```

For more details on `IHttpClientFactory` benefits and patterns, see [Microsoft's official documentation](https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory).

## Testing

### Unit Tests

Mock `IHttpClientFactory` for unit tests:

```csharp
var mockFactory = Substitute.For<IHttpClientFactory>();
mockFactory.CreateClient().Returns(new HttpClient(mockHandler));
```

### Live/Recorded Tests

Use `TestHttpClientFactoryProvider` for tests requiring recording support:

```csharp
_httpClientFactory = TestHttpClientFactoryProvider.Create(fixture);
```

## Example: Proxy Configuration

```bash
# Set proxy environment variables
export ALL_PROXY=http://proxy.company.com:8080
export NO_PROXY=localhost,127.0.0.1,*.internal

# Start Azure MCP - proxy configuration is automatically applied
./azmcp server start
```

All HTTP requests made by Azure MCP services will automatically use the configured proxy settings.
