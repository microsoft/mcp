---
title: AddApplicationInsightsTelemetry
category: api-reference
applies-to: 3.x
source: NETCORE/src/Microsoft.ApplicationInsights.AspNetCore/Extensions/ApplicationInsightsExtensions.cs
related:
  - api-reference/UseAzureMonitor.md
  - api-reference/UseAzureMonitorExporter.md
  - migration/appinsights-2x-to-3x-code-migration.md
---

# AddApplicationInsightsTelemetry

**Package:** `Microsoft.ApplicationInsights.AspNetCore` (3.x)

## Signatures

```csharp
using Microsoft.Extensions.DependencyInjection;

// 1. Parameterless — reads config from appsettings.json / env vars
public static IServiceCollection AddApplicationInsightsTelemetry(
    this IServiceCollection services);

// 2. IConfiguration — binds "ApplicationInsights" section
public static IServiceCollection AddApplicationInsightsTelemetry(
    this IServiceCollection services,
    IConfiguration configuration);

// 3. Action delegate — configure options inline
public static IServiceCollection AddApplicationInsightsTelemetry(
    this IServiceCollection services,
    Action<ApplicationInsightsServiceOptions> options);

// 4. Options instance — pass a pre-built options object
public static IServiceCollection AddApplicationInsightsTelemetry(
    this IServiceCollection services,
    ApplicationInsightsServiceOptions options);
```

All overloads return `IServiceCollection`. Overloads 2–4 call overload 1 internally, then apply configuration.

## ApplicationInsightsServiceOptions

Namespace: `Microsoft.ApplicationInsights.AspNetCore.Extensions`

| Property | Type | Default | Description |
|---|---|---|---|
| `ConnectionString` | `string` | `null` | Connection string for Application Insights. Can also be set via env var or config (see below). |
| `Credential` | `TokenCredential` | `null` | AAD credential for token-based authentication. When null, the instrumentation key from the connection string is used. |
| `ApplicationVersion` | `string` | Entry assembly version | Application version reported with telemetry. |
| `EnableQuickPulseMetricStream` | `bool` | `true` | Enables Live Metrics. |
| `EnablePerformanceCounterCollectionModule` | `bool` | `true` | Enables performance counter collection. |
| `EnableDependencyTrackingTelemetryModule` | `bool` | `true` | Enables HTTP and SQL dependency tracking. |
| `EnableRequestTrackingTelemetryModule` | `bool` | `true` | Enables ASP.NET Core request tracking. |
| `AddAutoCollectedMetricExtractor` | `bool` | `true` | Enables standard metric extraction. |
| `TracesPerSecond` | `double?` | `null` (effective: `5`) | Rate-limited sampling — targets this many traces per second. Must be ≥ 0. |
| `SamplingRatio` | `float?` | `null` | Fixed-rate sampling (0.0–1.0, where 1.0 = no sampling). When set, overrides `TracesPerSecond`. |
| `EnableTraceBasedLogsSampler` | `bool?` | `null` (effective: `true`) | When true, logs are sampled with their parent trace. Set `false` to collect all logs. |
| `EnableAuthenticationTrackingJavaScript` | `bool` | `false` | Injects authenticated user tracking in the JavaScript snippet. ASP.NET Core only. |

## JavaScriptSnippet

Automatically registered as a singleton (`IJavaScriptSnippet` and `JavaScriptSnippet`) when `AddApplicationInsightsTelemetry()` is called.

### Razor usage

```cshtml
@inject Microsoft.ApplicationInsights.AspNetCore.JavaScriptSnippet JavaScriptSnippet

<!-- FullScript includes <script> tags -->
@Html.Raw(JavaScriptSnippet.FullScript)

<!-- ScriptBody returns JS only (no <script> tags) — use inside your own script block -->
<script>
    @Html.Raw(JavaScriptSnippet.ScriptBody)
</script>
```

- `FullScript` — complete `<script>` block ready to drop into a layout page. Return `string.Empty` when telemetry is disabled or connection string is not set.
- `ScriptBody` — JavaScript only, for use inside an existing `<script>` tag.

## Minimal example

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry();
var app = builder.Build();
```

Set the connection string via environment variable:
```
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=...;IngestionEndpoint=...
```

Or in `appsettings.json`:
```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=...;IngestionEndpoint=..."
  }
}
```

## Full example

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
    options.EnableQuickPulseMetricStream = true;
    options.SamplingRatio = 0.5f;    // Collect 50% of telemetry
    options.EnableAuthenticationTrackingJavaScript = true;
});
var app = builder.Build();
```

## Behavior notes

- Connection string resolution order: `ApplicationInsightsServiceOptions.ConnectionString` → env var `APPLICATIONINSIGHTS_CONNECTION_STRING` → config key `ApplicationInsights:ConnectionString`.
- `TracesPerSecond` is the default sampling mode (effective default `5`). Set `SamplingRatio` for fixed-rate sampling instead.
- Additional OTel sources/meters can be added: `builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource("MySource"))`.
## See also

- [UseAzureMonitor (distro)](./UseAzureMonitor.md)
- [UseAzureMonitorExporter](./UseAzureMonitorExporter.md)
- [App Insights 2.x → 3.x Migration](../../migration/dotnet/appinsights-2x-to-3x-code-migration.md)
