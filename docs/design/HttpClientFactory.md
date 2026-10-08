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
- **SSRF Protection**: Default and named clients use `AntiSSRFPolicy(PolicyConfigOptions.ExternalOnlyLatest)` for HTTPS and DNS-to-IP enforcement

## Transport Protection

The default client, service-named clients, and the shared `ArmClientName`
use external-only AntiSSRF protection. Shared ARM SDK clients also retain their
`EndpointValidator` domain policy on every send, even without a command context,
and disable automatic redirects in server mode. Missing contexts cannot enable a
namespace bypass. Ordinary, explicit unprotected, and proxy-routed transports use
`SocketsHttpHandler` directly; protected transports retain the AntiSSRF handler.

`--dangerously-disable-ssrf-protections-by-namespace` applies to both domain checks and
the shared HTTP transport. The original executing namespace is checked on every send,
not when a client is created. Protected and unprotected connection pools are isolated,
so cached SDK clients cannot retain another command's bypass or private connections.
Missing or unresolved contexts remain protected, including when `ALL` is configured.

Each host owns one immutable, DI-registered `SsrfProtectionPolicy`. Its namespace
configuration is copied during host composition and shared by `EndpointValidator`,
the HTTP transports, and namespace override startup telemetry. Direct CLI registrations
default to no overrides. Independent hosts and test providers can configure different
policies concurrently without process-global initialization or resets.

`EndpointValidator` is injected as `IEndpointValidator` and shares the host's singleton
`ICommandContextAccessor` with command loaders and HTTP transports. Both Azure endpoint
and public-target validation resolve the original executing namespace on every call;
tool services supply only the endpoint and its service allow-list key to `IAzureService`,
which uses its own configured cloud. Direct `IEndpointValidator` calls retain an explicit
cloud argument for lower-level validation. No caller-supplied namespace can select a bypass,
and cached validators and SDK policies retain the accessor rather than an invocation's namespace.

`NoSsrfClientName` is an explicit client without AntiSSRF
for trusted infrastructure. It retains timeout, user-agent, proxy, and recording defaults,
but does not disable domain checks in callers. Tool implementations must not select it
from untrusted input or use it instead of the namespace override mechanism.

### Startup Usage Telemetry

When telemetry is enabled, the `ServerStarted` activity includes a privacy-safe
summary of configured namespace overrides:

| Tag | Values |
| --- | --- |
| `SsrfNamespaceOverrideScope` | `none`, `selected`, or `all` |
| `SsrfNamespaceOverrideCount` | Count of distinct, non-blank configured entries, ignoring case; includes the `ALL` marker |

The count describes configuration entries, not the number of tools actually affected.
Names are not trimmed into different override values.

The summary does not attest to per-request enforcement; missing contexts still cannot
enable a namespace override.

Raw override namespace strings are not included in these tags. Existing telemetry
opt-outs remain effective. The activity uses the trace pipeline, including
Microsoft-owned usage telemetry in release builds when enabled, rather than relying
on `ILogger` warnings.

## Environment Variables

The following environment variables are automatically applied:

- `ALL_PROXY`: Global proxy for all protocols
- `HTTP_PROXY`: Proxy for HTTP requests only
- `HTTPS_PROXY`: Proxy for HTTPS requests only
- `NO_PROXY`: Comma-separated list of hosts that should bypass the proxy

> **Security warning:** Configured HTTP/HTTPS/ALL proxies take precedence and disable
> transport-level AntiSSRF protections. This also applies to requests excluded by
> `NO_PROXY`. Use only trusted proxies with appropriate network access controls.
> Explicit domain validation remains active unless separately bypassed.

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

> **Security warning:** Debug recording proxies, whether supplied by a fixture or
> `TEST_PROXY_URL`, also take precedence over transport-level AntiSSRF checks.
> Upstream domain validation remains active before requests are rewritten.
> Use recording proxies only in trusted test environments.

The `TEST_PROXY_URL` fallback is captured into `HttpClientOptions.RecordingProxy`
when HTTP services are registered, rather than read during handler construction.
Set the environment variable before host composition. In-process tests should
configure this option per provider instead of modifying the process environment.
Fixture callbacks remain deferred until handler construction and a non-null result
takes precedence over the captured fallback.

`ConfigureDefaultHttpClient` registers shared client and handler defaults once per
service collection. Repeated calls are ignored rather than stacking duplicate
configuration delegates; the recording resolver from the first call is retained.

## Example: Proxy Configuration

```bash
# Set proxy environment variables
export ALL_PROXY=http://proxy.company.com:8080
export NO_PROXY=localhost,127.0.0.1,*.internal

# Start Azure MCP - proxy configuration is automatically applied
./azmcp server start
```

All HTTP requests made by Azure MCP services will automatically use the configured proxy settings.
