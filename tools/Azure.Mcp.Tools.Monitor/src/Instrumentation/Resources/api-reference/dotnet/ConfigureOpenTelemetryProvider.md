---
title: ConfigureOpenTelemetryProvider
category: api-reference
applies-to: 3.x
---

# ConfigureOpenTelemetry*Provider Extensions

These `IServiceCollection` extension methods register actions used to
configure OpenTelemetry providers. They work anywhere you have access to an
`IServiceCollection` - in application startup, inside a builder's
`ConfigureServices` callback, or in a shared extension method.

## When to use

- You want to **register sources, meters, or logging configuration separately**
  from where the provider is created (e.g., different files, methods, or
  packages).
- You need to **add configuration from inside a `ConfigureServices` callback**
  on a provider builder.
- Your app has **modular startup** where each feature area configures its own
  instrumentation independently.
- You are a **library author** registering instrumentation without owning the
  provider creation call.
- You need access to `IServiceProvider` at configuration time to resolve
  services such as `IConfiguration`.

> [!IMPORTANT]
> These methods register configuration but **do not create a
> provider**. A provider must still be created via
> Host & DI-Integrated (`AddOpenTelemetry`)(see in AddOpenTelemetry.md),
> Unified Multi-Signal (`OpenTelemetrySdk.Create`)(see in OpenTelemetrySdkCreate.md), or
> Per-Signal / Legacy (`Sdk.CreateTracerProviderBuilder`)(see in SdkCreateTracerProviderBuilder.md)
> for the configuration to take effect.

## Namespaces

| Method | Namespace |
| --- | --- |
| `ConfigureOpenTelemetryTracerProvider` | `OpenTelemetry.Trace` |
| `ConfigureOpenTelemetryMeterProvider` | `OpenTelemetry.Metrics` |
| `ConfigureOpenTelemetryLoggerProvider` | `OpenTelemetry.Logs` |

## Available methods

| Method | Signal |
| --- | --- |
| `services.ConfigureOpenTelemetryTracerProvider(...)` | Tracing |
| `services.ConfigureOpenTelemetryMeterProvider(...)` | Metrics |
| `services.ConfigureOpenTelemetryLoggerProvider(...)` | Logging |

### Overload 1 - `Action<*ProviderBuilder>` (executed immediately)

```csharp
public static IServiceCollection ConfigureOpenTelemetryTracerProvider(
    this IServiceCollection services,
    Action<TracerProviderBuilder> configure);

public static IServiceCollection ConfigureOpenTelemetryMeterProvider(
    this IServiceCollection services,
    Action<MeterProviderBuilder> configure);

public static IServiceCollection ConfigureOpenTelemetryLoggerProvider(
    this IServiceCollection services,
    Action<LoggerProviderBuilder> configure);
```

The `configure` delegate is **invoked immediately** (not deferred). Internally
it creates a `*ProviderServiceCollectionBuilder` (e.g.,
`TracerProviderServiceCollectionBuilder`) wrapping the `IServiceCollection` and
passes it to the delegate. Because the delegate receives the builder before the
`IServiceProvider` exists, it is **safe to register services** (e.g., call
`AddSource`, `AddMeter`, `AddInstrumentation<T>`, etc.).

### Overload 2 - `Action<IServiceProvider, *ProviderBuilder>` (deferred)

```csharp
public static IServiceCollection ConfigureOpenTelemetryTracerProvider(
    this IServiceCollection services,
    Action<IServiceProvider, TracerProviderBuilder> configure);

public static IServiceCollection ConfigureOpenTelemetryMeterProvider(
    this IServiceCollection services,
    Action<IServiceProvider, MeterProviderBuilder> configure);

public static IServiceCollection ConfigureOpenTelemetryLoggerProvider(
    this IServiceCollection services,
    Action<IServiceProvider, LoggerProviderBuilder> configure);
```

This overload registers the delegate as an `IConfigureTracerProviderBuilder` /
`IConfigureMeterProviderBuilder` / `IConfigureLoggerProviderBuilder` singleton
in the `IServiceCollection`. The delegate is **not invoked until the provider
is being constructed** (when the `IServiceProvider` is available). Because the
service container is already built at that point, **you cannot register new
services** inside this callback - many builder helper extensions that register
services will throw `NotSupportedException`.

```csharp
services.ConfigureOpenTelemetryTracerProvider((sp, tracing) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    tracing.SetSampler(new TraceIdRatioBasedSampler(
        config.GetValue<double>("Telemetry:SamplingRate")));
});
```

## Example - separating source registration from exporter setup

Register instrumentation sources early in startup, then configure exporters
separately:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register sources (could be in a different file or method)
builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
    tracing.AddSource("MyApp"));

builder.Services.ConfigureOpenTelemetryMeterProvider(metrics =>
    metrics.AddMeter("MyApp"));

// Configure exporters and create the provider
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("my-app"))
    .WithTracing(tracing => tracing.AddOtlpExporter())
    .WithMetrics(metrics => metrics.AddOtlpExporter());

var app = builder.Build();
app.Run();
```

## Example - library author registering sources

A library can register its instrumentation without depending on the app's
startup code:

```csharp
public static class MyLibraryExtensions
{
    public static IServiceCollection AddMyLibrary(this IServiceCollection services)
    {
        services.AddSingleton<MyService>();

        services.ConfigureOpenTelemetryTracerProvider(tracing =>
            tracing.AddSource("MyLibrary"));
        services.ConfigureOpenTelemetryMeterProvider(metrics =>
            metrics.AddMeter("MyLibrary"));

        return services;
    }
}
```

The host application composes everything:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMyLibrary();  // library registers its sources

builder.Services.AddOpenTelemetry()          // app owns the provider
    .ConfigureResource(r => r.AddService("my-app"))
    .WithTracing(tracing => tracing.AddOtlpExporter())
    .WithMetrics(metrics => metrics.AddOtlpExporter());

var app = builder.Build();
app.Run();
```



## Relationship with `WithTracing` / `WithMetrics` / `WithLogging`

These methods are **complementary** with the `With*` methods, not
interchangeable. `WithTracing()` / `WithMetrics()` / `WithLogging()` **register
the provider**; `ConfigureOpenTelemetry*Provider` only **queues configuration
actions**. Without a `With*` call (or equivalent provider creation), queued
actions are never consumed.

## Hosting package not required

These methods are defined in `OpenTelemetry.Api.ProviderBuilderExtensions` and
work with **any `IServiceCollection`** — hosted or non-hosted. The only
requirement is that a provider is eventually resolved from the same
`IServiceCollection` for the queued actions to take effect.

## Related methods

| Method | Defined on | Purpose |
| --- | --- | --- |
| Host & DI-Integrated (`AddOpenTelemetry`)(see in AddOpenTelemetry.md) | `IServiceCollection` | Creates the provider and starts the hosted service |
| `.WithTracing()` / `.WithMetrics()` / `.WithLogging()` | `IOpenTelemetryBuilder` | Registers a provider for the signal and optionally configures it |
| `.ConfigureResource()` | `IOpenTelemetryBuilder` | Shared resource configuration (internally uses `ConfigureOpenTelemetry*Provider`) |

