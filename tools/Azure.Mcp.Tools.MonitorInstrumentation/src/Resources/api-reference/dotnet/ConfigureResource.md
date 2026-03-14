---
title: ConfigureResource
category: api-reference
applies-to: 1.x
---

# .ConfigureResource()

## API surface

### On `IOpenTelemetryBuilder` (shared across all signals)

| Method | Purpose |
| --- | --- |
| `.ConfigureResource(Action<ResourceBuilder>)` | Modify the resource for traces, metrics, **and** logs |

### On individual provider builders

| Method | Purpose |
| --- | --- |
| `TracerProviderBuilder.ConfigureResource(Action<ResourceBuilder>)` | Modify resource for tracing only |
| `MeterProviderBuilder.ConfigureResource(Action<ResourceBuilder>)` | Modify resource for metrics only |
| `LoggerProviderBuilder.ConfigureResource(Action<ResourceBuilder>)` | Modify resource for logging only |
| `*.SetResourceBuilder(ResourceBuilder)` | Replace the resource builder entirely (all three builders) |

### `ResourceBuilder` static methods

| Method | Purpose |
| --- | --- |
| `ResourceBuilder.CreateDefault()` | Create builder with SDK defaults + env var detector |
| `ResourceBuilder.CreateEmpty()` | Create builder with no attributes |

### `ResourceBuilder` instance methods

| Method | Purpose |
| --- | --- |
| `.AddService(string serviceName, ...)` | Add `service.name`, `service.namespace`, `service.version`, `service.instance.id` |
| `.AddAttributes(IEnumerable<KeyValuePair<string, object>>)` | Add arbitrary resource attributes |
| `.AddTelemetrySdk()` | Add `telemetry.sdk.*` attributes (included in `CreateDefault()`) |
| `.AddEnvironmentVariableDetector()` | Parse `OTEL_RESOURCE_ATTRIBUTES` and `OTEL_SERVICE_NAME` env vars |
| `.AddDetector(IResourceDetector)` | Add a custom resource detector |
| `.AddDetector(Func<IServiceProvider, IResourceDetector>)` | Add a custom detector resolved from DI |
| `.Clear()` | Remove all previously added detectors/resources |

## When to use

- Set `service.name`, `service.version`, and other resource attributes that identify your application.
- Apply a shared resource across all three signals (traces, metrics, logs) via the top-level `IOpenTelemetryBuilder`.
- Override or extend the default resource for a single signal's provider builder.
- Add custom `IResourceDetector` implementations for cloud metadata or environment-specific attributes.
- Call multiple times safely — each configuration action is applied sequentially.

## Minimal example

```csharp
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("my-api"))
    .WithTracing(t => t.AddOtlpExporter());

var app = builder.Build();
app.Run();
```

## Full example

```csharp
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "order-api",
            serviceNamespace: "ecommerce",
            serviceVersion: "2.1.0",
            autoGenerateServiceInstanceId: true)
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = "production",
            ["host.name"] = Environment.MachineName,
        })
        .AddEnvironmentVariableDetector())
    .WithTracing(t => t.AddOtlpExporter())
    .WithMetrics(m => m.AddOtlpExporter());

var app = builder.Build();
app.Run();
```

## Key parameters

`AddService()` parameters:

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `serviceName` | `string` | *(required)* | Value for `service.name` |
| `serviceNamespace` | `string?` | `null` | Value for `service.namespace` |
| `serviceVersion` | `string?` | `null` | Value for `service.version` |
| `autoGenerateServiceInstanceId` | `bool` | `true` | Auto-generate a GUID for `service.instance.id` |
| `serviceInstanceId` | `string?` | `null` | Explicit `service.instance.id` (overrides auto-generate) |

## Behavior notes

- `ConfigureResource()` on `IOpenTelemetryBuilder` delegates to `ConfigureResource()` on all three signal builders (tracer, meter, logger).
- `ConfigureResource()` is **additive** — each call appends to the existing resource.
- `SetResourceBuilder()` **replaces** the entire resource builder. Use `ConfigureResource()` unless you specifically need a full reset.
- `CreateDefault()` includes `telemetry.sdk.*` attributes, `OTEL_RESOURCE_ATTRIBUTES`, `OTEL_SERVICE_NAME`, and a fallback `service.name` of `unknown_service:<process_name>`.
- `AddDetector(Func<IServiceProvider, IResourceDetector>)` throws `NotSupportedException` if `ResourceBuilder.Build()` is called directly without an `IServiceProvider`.
- Resource attributes are merged in registration order. Later values for the same key overwrite earlier ones.
