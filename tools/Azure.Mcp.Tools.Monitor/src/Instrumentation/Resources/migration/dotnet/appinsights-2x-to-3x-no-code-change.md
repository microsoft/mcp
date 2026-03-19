---
title: Application Insights 2.x to 3.x — No-Code-Change Migration
category: migration
applies-to: 3.x
---

# App Insights 2.x → 3.x — No Code Change

## When this applies

Your migration requires **only a package upgrade** (no code changes) if both of these are true:

1. You call `AddApplicationInsightsTelemetry()` with no arguments, an `IConfiguration`, or with options that only set **unchanged properties**.
2. You do not use any **removed extension methods** (`UseApplicationInsights()`, `AddApplicationInsightsTelemetryProcessor<T>()`, `ConfigureTelemetryModule<T>()`).

### Unchanged properties (safe to keep as-is)

| Property | Default |
|---|---|
| `ConnectionString` | `null` |
| `ApplicationVersion` | Entry assembly version |
| `EnableQuickPulseMetricStream` | `true` |
| `EnablePerformanceCounterCollectionModule` | `true` |
| `EnableDependencyTrackingTelemetryModule` | `true` |
| `EnableRequestTrackingTelemetryModule` | `true` |
| `AddAutoCollectedMetricExtractor` | `true` |
| `EnableAuthenticationTrackingJavaScript` | `false` |

If your code only uses these properties (or none at all), **and** does not use `ITelemetryInitializer` or `ITelemetryProcessor`, no code changes are needed. (`TelemetryClient` still works in 3.x — existing usage does not block a no-code-change upgrade.)

## Migration steps

### 1. Update the package

```xml
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="3.*" />
```

### 2. Build and run

That's it. No code changes required.

## Examples that work without changes

**Parameterless call:**
```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry();
var app = builder.Build();
```

**IConfiguration overload:**
```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
var app = builder.Build();
```

**Options with only unchanged properties:**
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
    options.EnableQuickPulseMetricStream = true;
    options.EnableDependencyTrackingTelemetryModule = true;
});
var app = builder.Build();
```

All three examples above work identically in 2.x and 3.x.

## What changes under the hood

Even though your code stays the same, 3.x brings these improvements automatically:

- Telemetry is now collected via OpenTelemetry — better standards alignment and ecosystem compatibility.
- `TracesPerSecond` (effective default `5`) provides rate-limited sampling out of the box. No configuration needed.
- Logging is integrated automatically — `ILogger` output is exported to Application Insights without additional setup.
- Azure resource detection (App Service, VM) happens automatically.

## See also

- Application Insights 2.x to 3.x Code Migration(see in appinsights-2x-to-3x-code-migration.md)
- AddApplicationInsightsTelemetry API reference(see in AddApplicationInsightsTelemetry.md)
