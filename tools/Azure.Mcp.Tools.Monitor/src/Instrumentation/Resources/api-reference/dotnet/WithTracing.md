---
title: WithTracing
category: api-reference
applies-to: 1.x
---

# .WithTracing()

## API surface

| Method | Purpose |
| --- | --- |
| `.WithTracing()` | Enable tracing with no additional configuration |
| `.WithTracing(Action<TracerProviderBuilder>)` | Enable tracing and configure the `TracerProviderBuilder` |
| `builder.AddSource(params string[] names)` | Subscribe to one or more `ActivitySource` names |
| `builder.AddInstrumentation<T>()` | Add an instrumentation library via DI |
| `builder.AddProcessor(BaseProcessor<Activity>)` | Add an activity processor instance |
| `builder.AddProcessor<T>()` | Add a processor resolved from DI (registered as singleton) |
| `builder.AddProcessor(Func<IServiceProvider, BaseProcessor<Activity>>)` | Add a processor via factory |
| `builder.SetErrorStatusOnException(bool)` | Auto-set `Status.Error` on unhandled exceptions (default `true`) |

See also: Sampling.md for `SetSampler()` overloads · ConfigureResource.md for resource configuration

## When to use

- Enable distributed tracing in an ASP.NET Core or Worker Service app via `AddOpenTelemetry()`.
- Enable tracing in a standalone/console app via `OpenTelemetrySdk.Create()`.
- Subscribe to `ActivitySource` names so the SDK captures spans from your code or instrumentation libraries.
- Attach processors (e.g., `BatchActivityExportProcessor`) or set a custom sampler.
- Call multiple times safely — only one `TracerProvider` is created per `IServiceCollection`.

## Minimal example

```csharp
using OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("MyApp")
        .AddOtlpExporter());

var app = builder.Build();
app.Run();
```

## Full example

```csharp
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("order-api"))
    .WithTracing(tracing => tracing
        .AddSource("OrderService", "PaymentService")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(0.5)))
        .AddProcessor<MyFilteringProcessor>()
        .AddOtlpExporter());

var app = builder.Build();
app.Run();
```

### Standalone (non-host) usage

```csharp
using OpenTelemetry;
using OpenTelemetry.Trace;

using var sdk = OpenTelemetrySdk.Create(builder => builder
    .WithTracing(tracing => tracing
        .AddSource("MyConsoleApp")
        .AddOtlpExporter()));

// sdk.TracerProvider is available if needed
```

## Behavior notes

- `AddSource()` accepts wildcard-free `ActivitySource` names. Activities from unregistered sources are dropped.
- Processors run in registration order. Export processors should be added last.
- When used with `AddOpenTelemetry()`, the host manages `TracerProvider` lifecycle — do **not** call `Dispose()` manually.
- When used with `OpenTelemetrySdk.Create()`, you own the `OpenTelemetrySdk` instance and must dispose it.
