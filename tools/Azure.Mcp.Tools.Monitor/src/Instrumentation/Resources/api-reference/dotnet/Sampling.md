---
title: Sampling
category: api-reference
applies-to: 1.x
---

# SetSampler()

## API surface

### `SetSampler` overloads

| Method | Purpose |
| --- | --- |
| `.SetSampler(Sampler)` | Set a sampler instance directly |
| `.SetSampler<T>()` | Set a sampler resolved from DI (registered as singleton) |
| `.SetSampler(Func<IServiceProvider, Sampler>)` | Set a sampler via factory delegate |

### `SamplingDecision` enum

| Value | `IsAllDataRequested` | `Recorded` flag | Description |
| --- | --- | --- | --- |
| `Drop` | `false` | `false` | Activity created but not recorded or exported |
| `RecordOnly` | `true` | `false` | Activity recorded (spans visible to processors) but not exported |
| `RecordAndSample` | `true` | `true` | Activity fully recorded and exported |

## When to use

- Control the volume of traces exported to your backend by sampling a percentage of requests.
- Honor parent sampling decisions for distributed traces while applying custom logic to root spans.
- Drop traces entirely for specific scenarios (e.g., health checks) using a custom sampler.
- Replace the SDK default sampler (`ParentBasedSampler(AlwaysOnSampler)`).

## Minimal example

```csharp
using OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("MyApp")
        .SetSampler(new TraceIdRatioBasedSampler(0.1))
        .AddOtlpExporter());

var app = builder.Build();
app.Run();
```

## Full example

```csharp
using OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("MyApp")
        .SetSampler(new ParentBasedSampler(
            rootSampler: new TraceIdRatioBasedSampler(0.25),
            remoteParentSampled: new AlwaysOnSampler(),
            remoteParentNotSampled: new AlwaysOffSampler(),
            localParentSampled: new AlwaysOnSampler(),
            localParentNotSampled: new AlwaysOffSampler()))
        .AddOtlpExporter());

var app = builder.Build();
app.Run();
```

## Behavior notes

- The SDK default sampler is `ParentBasedSampler(new AlwaysOnSampler())` — all root spans are sampled, child spans follow the parent decision.
- `SetSampler()` replaces any previously set sampler. Only the last call takes effect.
- `TraceIdRatioBasedSampler` is deterministic: the same trace ID always produces the same sampling decision, ensuring consistency across services.
- `SamplingResult` can carry additional attributes and a `TraceStateString` that are attached to the span if sampled.
- Samplers are invoked **before** the span is fully created. Returning `Drop` means the `Activity` object is created but receives no data and is not exported.
- To implement a custom sampler, subclass `Sampler` and override `ShouldSample(in SamplingParameters)`.

## See also

- Use `AddProcessor` with a filtering processor if you need to drop spans **after** they are recorded (post-sampling). See ActivityProcessors.md for filtering and enrichment patterns.
