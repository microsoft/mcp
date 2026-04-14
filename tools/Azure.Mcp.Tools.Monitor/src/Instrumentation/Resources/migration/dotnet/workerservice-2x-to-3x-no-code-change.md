---
title: WorkerService 2.x to 3.x — No-Code-Change Migration
category: migration
applies-to: 3.x
---

# Worker Service 2.x → 3.x — No Code Change

## When this applies

Your migration requires **only a package upgrade** (no code changes) if both of these are true:

1. You call `AddApplicationInsightsTelemetryWorkerService()` with no arguments, an `IConfiguration`, or with options that only set **unchanged properties**.
2. You do not call `AddApplicationInsightsTelemetryWorkerService(string instrumentationKey)`.

### Unchanged properties (safe to keep as-is)

| Property | Default |
|---|---|
| `ConnectionString` | `null` |
| `ApplicationVersion` | Entry assembly version |
| `EnableQuickPulseMetricStream` | `true` |
| `EnablePerformanceCounterCollectionModule` | `true` |
| `EnableDependencyTrackingTelemetryModule` | `true` |
| `AddAutoCollectedMetricExtractor` | `true` |

If your code only uses these properties (or none at all), no code changes are needed.

## Migration steps

### 1. Update the package

```xml
<PackageReference Include="Microsoft.ApplicationInsights.WorkerService" Version="3.*" />
```

### 2. Build and run

That's it. No code changes required.

## Examples that work without changes

**Parameterless call:**
```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices(services =>
{
    services.AddApplicationInsightsTelemetryWorkerService();
});
var host = builder.Build();
await host.RunAsync();
```

**IConfiguration overload:**
```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices(services =>
{
    services.AddApplicationInsightsTelemetryWorkerService(builder.Configuration);
});
var host = builder.Build();
await host.RunAsync();
```

**Options with only unchanged properties:**
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.WorkerService;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices(services =>
{
    services.AddApplicationInsightsTelemetryWorkerService(options =>
    {
        options.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
        options.EnableQuickPulseMetricStream = true;
        options.EnableDependencyTrackingTelemetryModule = true;
    });
});
var host = builder.Build();
await host.RunAsync();
```

All three examples above work identically in 2.x and 3.x.

## What changes under the hood

Even though your code stays the same, 3.x brings these improvements automatically:

- Telemetry is now collected via OpenTelemetry — better standards alignment and ecosystem compatibility.
- `TracesPerSecond` (effective default `5`) provides rate-limited sampling out of the box. No configuration needed.
- Logging is integrated automatically — `ILogger` output is exported to Application Insights without additional setup.
- Azure resource detection (App Service, VM) happens automatically.

## See also

- WorkerService 2.x to 3.x Code Migration(see in workerservice-2x-to-3x-code-migration.md)
- AddApplicationInsightsTelemetryWorkerService API reference(see in AddApplicationInsightsTelemetryWorkerService.md)
