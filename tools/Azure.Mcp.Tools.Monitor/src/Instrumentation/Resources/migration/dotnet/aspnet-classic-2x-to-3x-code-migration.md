---
title: Classic ASP.NET 2.x to 3.x Migration
category: migration
applies-to: 3.x
related:
  - api-reference/ApplicationInsightsWeb.md
  - api-reference/TelemetryConfigurationBuilder.md
  - api-reference/TelemetryClient.md
  - api-reference/ActivityProcessors.md
  - api-reference/LogProcessors.md
---

# Classic ASP.NET 2.x → 3.x Code Migration

## What changed

3.x is built on OpenTelemetry internally. The NuGet package name stays the same — `Microsoft.ApplicationInsights.Web` — but the internal architecture changed. `applicationinsights.config` format is simplified, satellite packages are removed, and extensibility uses OpenTelemetry processors via `ConfigureOpenTelemetryBuilder`.

Key changes:
- `<InstrumentationKey>` → `<ConnectionString>` (connection string is **required** — 3.x throws without it)
- `<TelemetryInitializers>`, `<TelemetryModules>`, `<TelemetryProcessors>` sections → removed entirely
- `<TelemetryChannel>` section → removed (export managed by OpenTelemetry)
- 12 satellite packages → removed (functionality built into the main package)
- `TelemetryConfiguration.Active` → `TelemetryConfiguration.CreateDefault()` (returns static singleton in 3.x)
- Minimum .NET Framework: 4.5.2 → **4.6.2**
- Custom initializers/processors → OpenTelemetry processors via `ConfigureOpenTelemetryBuilder`

## applicationinsights.config — before / after

**2.x:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<ApplicationInsights xmlns="http://schemas.microsoft.com/ApplicationInsights/2013/Settings">
  <InstrumentationKey>00000000-0000-0000-0000-000000000000</InstrumentationKey>
  <TelemetryInitializers>
    <Add Type="Microsoft.ApplicationInsights.Web.WebTestTelemetryInitializer, Microsoft.AI.Web" />
    <Add Type="Microsoft.ApplicationInsights.Web.SyntheticUserAgentTelemetryInitializer, Microsoft.AI.Web">
      <Filters>search|spider|crawl|Bot|Monitor</Filters>
    </Add>
    <!-- ... more initializers ... -->
  </TelemetryInitializers>
  <TelemetryModules>
    <Add Type="Microsoft.ApplicationInsights.Web.RequestTrackingTelemetryModule, Microsoft.AI.Web" />
    <Add Type="Microsoft.ApplicationInsights.Web.ExceptionTrackingTelemetryModule, Microsoft.AI.Web" />
    <Add Type="Microsoft.ApplicationInsights.DependencyCollector.DependencyTrackingTelemetryModule, Microsoft.AI.DependencyCollector" />
    <!-- ... more modules ... -->
  </TelemetryModules>
  <TelemetryChannel Type="Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel.ServerTelemetryChannel, Microsoft.AI.ServerTelemetryChannel">
    <DeveloperMode>false</DeveloperMode>
  </TelemetryChannel>
</ApplicationInsights>
```

**3.x:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<ApplicationInsights xmlns="http://schemas.microsoft.com/ApplicationInsights/2013/Settings">
  <ConnectionString>InstrumentationKey=...;IngestionEndpoint=https://dc.applicationinsights.azure.com/</ConnectionString>
  <TracesPerSecond>5.0</TracesPerSecond>
  <EnableTraceBasedLogsSampler>true</EnableTraceBasedLogsSampler>
  <EnableQuickPulseMetricStream>true</EnableQuickPulseMetricStream>
  <EnablePerformanceCounterCollectionModule>true</EnablePerformanceCounterCollectionModule>
  <EnableDependencyTrackingTelemetryModule>true</EnableDependencyTrackingTelemetryModule>
  <EnableRequestTrackingTelemetryModule>true</EnableRequestTrackingTelemetryModule>
  <AddAutoCollectedMetricExtractor>true</AddAutoCollectedMetricExtractor>
</ApplicationInsights>
```

## Web.config changes

**Remove** `TelemetryCorrelationHttpModule` (OpenTelemetry handles correlation natively).

**Keep** `ApplicationInsightsHttpModule` — still needed in 3.x.

**Add** `TelemetryHttpModule` (from `OpenTelemetry.Instrumentation.AspNet`) — added automatically by NuGet install.

## Satellite packages to remove

These packages are no longer needed — their functionality is built into `Microsoft.ApplicationInsights.Web` 3.x. **Remove in this order** (dependents before dependencies):

1. `Microsoft.ApplicationInsights.WindowsServer`
2. `Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel`
3. `Microsoft.ApplicationInsights.DependencyCollector`
4. `Microsoft.ApplicationInsights.PerfCounterCollector`
5. `Microsoft.ApplicationInsights.Agent.Intercept`
6. `Microsoft.AspNet.TelemetryCorrelation`

Also remove if present (skip any not in packages.config):
- `Microsoft.ApplicationInsights.EventCounterCollector`
- `Microsoft.Extensions.Logging.ApplicationInsights`
- `Microsoft.ApplicationInsights.Log4NetAppender`
- `Microsoft.ApplicationInsights.TraceListener`
- `Microsoft.ApplicationInsights.DiagnosticSourceListener`
- `Microsoft.ApplicationInsights.EtwCollector`
- `Microsoft.ApplicationInsights.EventSourceListener`

## TelemetryConfiguration changes

| 2.x Pattern | 3.x Replacement |
|---|---|
| `TelemetryConfiguration.Active` | `TelemetryConfiguration.CreateDefault()` (returns static singleton) |
| `new TelemetryConfiguration(ikey)` | `TelemetryConfiguration.CreateDefault()` + set `ConnectionString` |
| `config.InstrumentationKey = "..."` | `config.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=..."` |
| `config.TelemetryInitializers.Add(...)` | Use `ConfigureOpenTelemetryBuilder` — see below |
| `config.TelemetryProcessorChainBuilder` | Use `ConfigureOpenTelemetryBuilder` — see below |
| `config.TelemetryChannel` | Removed — export managed by OpenTelemetry |
| `config.TelemetrySinks` | Removed — use OpenTelemetry exporters |

## Custom processor migration (non-DI)

In classic ASP.NET, use `ConfigureOpenTelemetryBuilder` on `TelemetryConfiguration` in `Global.asax.cs`:

```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
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
            otel.WithTracing(tracing =>
            {
                tracing.AddProcessor<MyEnrichmentProcessor>();
                tracing.AddProcessor<MyFilterProcessor>();
            });
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

> **Important:** Create a **single static** `TelemetryClient` instance in `Application_Start`. Do not create per-request or per-controller instances — this causes memory leaks and duplicate telemetry. `TelemetryConfiguration.CreateDefault()` returns a singleton that is shared with the `ApplicationInsightsHttpModule`.

See [TelemetryConfigurationBuilder.md](../api-reference/dotnet/TelemetryConfigurationBuilder.md) for full API details.

## Sampling changes

- Per-type sampling (e.g. exclude exceptions from sampling) is **no longer supported**
- Default: rate-limited at 5 traces/sec via `<TracesPerSecond>`
- Fixed-rate: `<SamplingRatio>0.25</SamplingRatio>` (25%)
- Custom OTel samplers not supported with 3.x shim

## TelemetryClient changes

See [TelemetryClient.md](../api-reference/dotnet/TelemetryClient.md) for full breaking changes. Key items:
- `TrackPageView` — removed entirely
- `TrackEvent`/`TrackException`/`TrackAvailability` — metrics dict parameter removed
- Parameterless `new TelemetryClient()` — removed, use `new TelemetryClient(TelemetryConfiguration.CreateDefault())`
- `client.InstrumentationKey` — removed
- Create a **single static instance** — do not create per-request (see processor migration section above)

## Migration steps

1. Check .NET Framework target — must be **4.6.2** or later
2. Upgrade `Microsoft.ApplicationInsights.Web` to 3.x via Package Manager Console: `Update-Package Microsoft.ApplicationInsights.Web`
3. Remove satellite packages via Package Manager Console in this order (dependents before dependencies):
   - `Microsoft.ApplicationInsights.WindowsServer`
   - `Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel`
   - `Microsoft.ApplicationInsights.DependencyCollector`
   - `Microsoft.ApplicationInsights.PerfCounterCollector`
   - `Microsoft.ApplicationInsights.Agent.Intercept`
   - `Microsoft.AspNet.TelemetryCorrelation`
4. Rewrite `applicationinsights.config` to 3.x format (remove all `<TelemetryInitializers>`, `<TelemetryModules>`, `<TelemetryProcessors>`, `<TelemetryChannel>` sections; add `<ConnectionString>` and feature flags)
5. Update `Web.config` — remove `TelemetryCorrelationHttpModule`, verify `TelemetryHttpModule` present
6. Replace `TelemetryConfiguration.Active` with `TelemetryConfiguration.CreateDefault()` in code
7. Create a single static `TelemetryClient` in `Application_Start`, flush in `Application_End`
8. Migrate custom `ITelemetryInitializer`/`ITelemetryProcessor` to OpenTelemetry processors via `ConfigureOpenTelemetryBuilder`
9. Fix TelemetryClient breaking calls (TrackEvent metrics dict, TrackPageView, etc.)
10. Set `<ConnectionString>` in applicationinsights.config
11. Build and verify

## See also

- [ApplicationInsightsWeb API reference](../api-reference/dotnet/ApplicationInsightsWeb.md)
- [ConfigureOpenTelemetryBuilder](../api-reference/dotnet/TelemetryConfigurationBuilder.md)
- [TelemetryClient breaking changes](../api-reference/dotnet/TelemetryClient.md)
