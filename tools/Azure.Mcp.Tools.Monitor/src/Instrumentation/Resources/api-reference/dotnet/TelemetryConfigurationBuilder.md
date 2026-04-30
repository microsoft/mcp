---
title: TelemetryConfigurationBuilder
category: api-reference
applies-to: 3.x
---

# TelemetryConfiguration.ConfigureOpenTelemetryBuilder

The non-DI extensibility API for Application Insights 3.x. Use this when you don't have an `IServiceCollection` — classic ASP.NET (`Global.asax`), console apps, or test scenarios.

For DI-based apps (ASP.NET Core, Worker Service), use ConfigureOpenTelemetryTracerProvider(see in ConfigureOpenTelemetryProvider.md) instead.

## API

```csharp
TelemetryConfiguration.ConfigureOpenTelemetryBuilder(
    Action<IOpenTelemetryBuilder> configure)
```

## Setup pattern

```csharp
using Microsoft.ApplicationInsights.Extensibility;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

var config = TelemetryConfiguration.CreateDefault();
config.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";

config.ConfigureOpenTelemetryBuilder(otel =>
{
    // Add custom trace processors
    otel.WithTracing(tracing =>
    {
        tracing.AddProcessor<MyEnrichmentProcessor>();
        tracing.AddProcessor<MyFilterProcessor>();
    });

    // Add custom log processors
    otel.WithLogging(logging =>
    {
        logging.AddProcessor<MyLogProcessor>();
    });

    // Set resource attributes (Cloud.RoleName, etc.)
    otel.ConfigureResource(r => r.AddService(
        serviceName: "MyWebApp",
        serviceInstanceId: Environment.MachineName,
        serviceVersion: "1.0.0"));
});
```

## Add instrumentations

```csharp
using OpenTelemetry;
using OpenTelemetry.Trace;

config.ConfigureOpenTelemetryBuilder(otel =>
{
    otel.WithTracing(tracing =>
    {
        tracing.AddRedisInstrumentation();
        tracing.AddSqlClientInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
        });
    });
});
```

## Add exporters (dual export)

```csharp
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;

config.ConfigureOpenTelemetryBuilder(otel =>
{
    // Console exporter for debugging
    otel.WithTracing(tracing => tracing.AddConsoleExporter());
    otel.WithLogging(logging => logging.AddConsoleExporter());

    // OTLP exporter for secondary destination
    otel.WithTracing(tracing => tracing.AddOtlpExporter());
    otel.WithMetrics(metrics => metrics.AddOtlpExporter());
});
```

## TelemetryClient usage

In classic ASP.NET, create a **single static** `TelemetryClient` instance. `TelemetryConfiguration.CreateDefault()` returns a singleton shared with `ApplicationInsightsHttpModule` — do not create per-request instances.

```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

public class MvcApplication : HttpApplication
{
    public static TelemetryClient TelemetryClient { get; private set; }

    protected void Application_Start()
    {
        var config = TelemetryConfiguration.CreateDefault();
        config.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
        config.ConfigureOpenTelemetryBuilder(otel =>
        {
            otel.WithTracing(tracing => tracing.AddProcessor<MyProcessor>());
            otel.ConfigureResource(r => r.AddService("MyWebApp"));
        });

        TelemetryClient = new TelemetryClient(config);

        AreaRegistration.RegisterAllAreas();
        RouteConfig.RegisterRoutes(RouteTable.Routes);
    }

    protected void Application_End()
    {
        TelemetryClient?.Flush();
        System.Threading.Tasks.Task.Delay(1000).Wait();
    }
}
```

Then in controllers:

```csharp
public class HomeController : Controller
{
    public ActionResult Index()
    {
        MvcApplication.TelemetryClient.TrackEvent("HomeVisited");
        return View();
    }
}
```

## Relationship to ConfigureOpenTelemetryTracerProvider

| Scenario | API to use |
|---|---|
| ASP.NET Core / Worker Service (DI) | `services.ConfigureOpenTelemetryTracerProvider(...)` |
| Classic ASP.NET (Global.asax) | `config.ConfigureOpenTelemetryBuilder(...)` |
| Console apps without DI | `config.ConfigureOpenTelemetryBuilder(...)` |
| Tests | `config.ConfigureOpenTelemetryBuilder(...)` |

Both APIs provide access to the same OTel builders (`TracerProviderBuilder`, `MeterProviderBuilder`, `LoggerProviderBuilder`). The difference is the entry point — `IServiceCollection` extension vs `TelemetryConfiguration` method.

## Key differences from 2.x

| 2.x Pattern | 3.x via `ConfigureOpenTelemetryBuilder` |
|---|---|
| `config.TelemetryInitializers.Add(new MyInit())` | `otel.WithTracing(t => t.AddProcessor<MyProcessor>())` in `OnStart` |
| `config.TelemetryProcessorChainBuilder.Use(...)` | `otel.WithTracing(t => t.AddProcessor<MyProcessor>())` in `OnEnd` |
| `config.TelemetryChannel = new InMemoryChannel()` | Not needed — export managed by OTel |
| `config.TelemetrySinks.Add(...)` | `otel.WithTracing(t => t.AddOtlpExporter())` |

## Notes

- **`using OpenTelemetry;` is required** — the `WithTracing()`, `WithLogging()`, `WithMetrics()`, and `ConfigureResource()` extension methods on `IOpenTelemetryBuilder` are defined in the root `OpenTelemetry` namespace. Without this using directive, these methods will not resolve. This is separate from `using OpenTelemetry.Trace;` / `using OpenTelemetry.Logs;` which are also needed.
- `CreateDefault()` returns a **static singleton** in 3.x (not a new instance). Call `ConfigureOpenTelemetryBuilder` only once, in `Application_Start`.
- Connection string is **required** — 3.x throws if not set. For tests, use a dummy: `InstrumentationKey=00000000-0000-0000-0000-000000000000`.
- Call `TelemetryClient.Flush()` in `Application_End` followed by a short delay to avoid data loss on shutdown.
- `GlobalProperties` on `TelemetryClient.Context` works for custom properties. Other `Context` properties (User.Id, Operation.Name) have known propagation limitations in 3.x.
