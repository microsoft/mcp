---
title: WithMetrics
category: api-reference
applies-to: 1.x
---

# .WithMetrics()

## API surface

| Method | Purpose |
| --- | --- |
| `.WithMetrics()` | Enable metrics with no additional configuration |
| `.WithMetrics(Action<MeterProviderBuilder>)` | Enable metrics and configure the `MeterProviderBuilder` |
| `builder.AddMeter(params string[] names)` | Subscribe to one or more `Meter` names |
| `builder.AddReader(MetricReader)` | Add a metric reader instance |
| `builder.AddReader<T>()` | Add a reader resolved from DI (registered as singleton) |
| `builder.AddReader(Func<IServiceProvider, MetricReader>)` | Add a reader via factory |
| `builder.AddView(string instrumentName, string name)` | Rename a metric stream (no wildcards) |
| `builder.AddView(string instrumentName, MetricStreamConfiguration)` | Customize aggregation for an instrument (wildcards allowed) |
| `builder.AddView(Func<Instrument, MetricStreamConfiguration?>)` | Programmatic view selection |
| `builder.SetMaxMetricStreams(int)` | Set max metric streams (default: 1000) |
| `builder.SetExemplarFilter(ExemplarFilterType)` | Set exemplar filter (`AlwaysOff`, `AlwaysOn`, `TraceBased`) |

See also: ConfigureResource.md for resource configuration

## When to use

- Enable metrics collection in an ASP.NET Core or Worker Service app via `AddOpenTelemetry()`.
- Enable metrics in a standalone/console app via `OpenTelemetrySdk.Create()`.
- Subscribe to `Meter` names from your application code or instrumentation libraries.
- Configure views to customize aggregation, rename streams, or set cardinality limits.
- Call multiple times safely — only one `MeterProvider` is created per `IServiceCollection`.

## Minimal example

```csharp
using OpenTelemetry;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddMeter("MyApp")
        .AddOtlpExporter());

var app = builder.Build();
app.Run();
```

## Full example

```csharp
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("order-api"))
    .WithMetrics(metrics => metrics
        .AddMeter("OrderService", "PaymentService")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddView("request-duration", new ExplicitBucketHistogramConfiguration
        {
            Boundaries = new double[] { 0.01, 0.05, 0.1, 0.5, 1, 5, 10 }
        })
        .SetMaxMetricStreams(500)
        .SetExemplarFilter(ExemplarFilterType.TraceBased)
        .AddOtlpExporter());

var app = builder.Build();
app.Run();
```

### Standalone (non-host) usage

```csharp
using OpenTelemetry;
using OpenTelemetry.Metrics;

using var sdk = OpenTelemetrySdk.Create(builder => builder
    .WithMetrics(metrics => metrics
        .AddMeter("MyConsoleApp")
        .AddOtlpExporter()));
```

## Key parameters

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `maxMetricStreams` | `int` | 1000 | Maximum number of metric streams allowed (min: 1) |
| `exemplarFilter` | `ExemplarFilterType` | `AlwaysOff` | Controls how measurements are offered to exemplar reservoirs |

## Behavior notes

- `AddMeter()` accepts exact `Meter` names — unregistered meters are ignored.
- `AddView()` with a wildcard (`*`) in `instrumentName` cannot rename a stream (would cause conflicts).
- Views are applied in registration order. The first matching view wins per instrument.
- `SetMaxMetricPointsPerMetricStream()` is obsolete since 1.10.0 — use `MetricStreamConfiguration.CardinalityLimit` via `AddView` instead.
- `WithMetrics()` automatically registers an `IMetricsListener` named `"OpenTelemetry"` to bridge `System.Diagnostics.Metrics`.
- When used with `AddOpenTelemetry()`, the host manages `MeterProvider` lifecycle — do **not** call `Dispose()` manually.
