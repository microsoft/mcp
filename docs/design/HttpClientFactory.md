# Using IHttpClientFactory

This document describes how to use `IHttpClientFactory` for HTTP requests in Azure MCP.

## Overview

Azure MCP uses the standard .NET `IHttpClientFactory` for centralized HTTP client management. Direct HTTP consumers inject the factory. Azure-facing services obtain its clients through `IAzureService.GetClient()` and assign them to Azure SDK transports.

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

### Direct HTTP Services

Services should inject `IHttpClientFactory` and create clients as needed:

```csharp
public class MyService(IHttpClientFactory httpClientFactory)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task MakeRequestAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://api.example.com/endpoint", cancellationToken);
    }
}
```

### Azure SDK Services

Azure SDK services inject `IAzureService`, inherit `BaseAzureService` (or `BaseAzureResourceService`), and use the factory-backed client exposed by the base service:

```csharp
public sealed class MyAzureService(IAzureService azureService)
    : BaseAzureService(azureService)
{
    private MyClientOptions CreateClientOptions()
    {
        var options = AddDefaultPolicies(new MyClientOptions());
        options.Transport = new HttpClientTransport(AzureService.GetClient());
        return options;
    }
}
```

Do not instantiate `HttpClient` or an Azure SDK default transport directly. The factory-backed transport is required for configured proxies and recorded-test redirection.

### Setting Custom Timeout

For operations requiring longer timeouts, set it on the client instance:

```csharp
public async Task LongRunningOperationAsync(Uri url, CancellationToken cancellationToken)
{
    var client = _httpClientFactory.CreateClient();
    client.Timeout = TimeSpan.FromMinutes(5);
    var response = await client.GetAsync(url, cancellationToken);
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

For an Azure SDK service, substitute `IAzureService.GetClient()` instead.

### Live/Recorded Tests

The MCP recorded-test host configures proxy-aware clients automatically. When a test constructs services manually, `TestHttpClientFactoryProvider.Create` returns a configured `ServiceProvider`:

```csharp
using var serviceProvider = TestHttpClientFactoryProvider.Create(fixture);
var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
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
