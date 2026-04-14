---
title: HttpInstrumentation
category: api-reference
applies-to: 1.x
---

# HTTP Instrumentation — Client & Server Enrichment

HTTP client and server instrumentation is auto-configured by both Azure Monitor Distro and Application Insights 3.x. This doc covers **customization**: enrichment, filtering, and advanced options.

## Packages

| Scope | Package | Auto-included? |
|---|---|---|
| HTTP Client (`HttpClient`) | `OpenTelemetry.Instrumentation.Http` | Yes (by Distro & AI 3.x) |
| HTTP Server (ASP.NET Core) | `OpenTelemetry.Instrumentation.AspNetCore` | Yes (by Distro & AI 3.x) |

You only need to install these packages explicitly if you want to **customize** the instrumentation options.

## HTTP Client enrichment

```csharp
using OpenTelemetry.Trace;

builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
    tracing.AddHttpClientInstrumentation(options =>
    {
        options.EnrichWithHttpRequestMessage = (activity, request) =>
        {
            activity.SetTag("http.request.header.x-correlation-id",
                request.Headers.TryGetValues("X-Correlation-Id", out var vals) ? vals.First() : null);
        };
        options.EnrichWithHttpResponseMessage = (activity, response) =>
        {
            activity.SetTag("http.response.content_length", response.Content.Headers.ContentLength);
        };
        options.FilterHttpRequestMessage = (request) =>
        {
            // Suppress health check calls
            return request.RequestUri?.AbsolutePath != "/health";
        };
        options.RecordException = true;
    }));
```

## HTTP Client options

| Option | Default | Description |
|---|---|---|
| `EnrichWithHttpRequestMessage` | `null` | `Action<Activity, HttpRequestMessage>` to add tags from the request. |
| `EnrichWithHttpResponseMessage` | `null` | `Action<Activity, HttpResponseMessage>` to add tags from the response. |
| `EnrichWithException` | `null` | `Action<Activity, Exception>` to enrich on failure. |
| `FilterHttpRequestMessage` | `null` | `Func<HttpRequestMessage, bool>` — return `false` to suppress span. |
| `RecordException` | `false` | Record exception events on the span. |

## HTTP Server (ASP.NET Core) enrichment

```csharp
using OpenTelemetry.Trace;

builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
    tracing.AddAspNetCoreInstrumentation(options =>
    {
        options.EnrichWithHttpRequest = (activity, request) =>
        {
            activity.SetTag("http.request.header.user-agent", request.Headers["User-Agent"].ToString());
        };
        options.EnrichWithHttpResponse = (activity, response) =>
        {
            activity.SetTag("http.response.custom_header", response.Headers["X-Custom"].ToString());
        };
        options.Filter = (httpContext) =>
        {
            // Suppress /health and /metrics endpoints
            return httpContext.Request.Path != "/health"
                && httpContext.Request.Path != "/metrics";
        };
        options.RecordException = true;
    }));
```

## HTTP Server options

| Option | Default | Description |
|---|---|---|
| `EnrichWithHttpRequest` | `null` | `Action<Activity, HttpRequest>` to add tags from the request. |
| `EnrichWithHttpResponse` | `null` | `Action<Activity, HttpResponse>` to add tags from the response. |
| `EnrichWithException` | `null` | `Action<Activity, Exception>` to enrich on failure. |
| `Filter` | `null` | `Func<HttpContext, bool>` — return `false` to suppress span. |
| `RecordException` | `false` | Record exception events on the span. |

## Semantic conventions (auto-populated)

| Attribute | Source |
|---|---|
| `http.request.method` | `GET`, `POST`, etc. |
| `url.full` | Full URL (client) |
| `url.path` | Path (server) |
| `http.response.status_code` | `200`, `404`, etc. |
| `server.address` | Target host (client) |
| `network.protocol.version` | `1.1`, `2` |
| `http.route` | Route template (server) |

## Notes

- Both client and server instrumentation are **already active** in AI 3.x setups. Adding the packages explicitly is only needed to access the `options` overload for enrichment/filtering.
- Enrichment callbacks run synchronously on the hot path — keep them lightweight.
- `Filter` on the server side is the recommended way to suppress health check / readiness probe spans (instead of a custom processor).
- **Non-DI usage:** Use `config.ConfigureOpenTelemetryBuilder(otel => otel.WithTracing(t => t.AddHttpClientInstrumentation(...)))` on `TelemetryConfiguration`. See TelemetryClient.md.
