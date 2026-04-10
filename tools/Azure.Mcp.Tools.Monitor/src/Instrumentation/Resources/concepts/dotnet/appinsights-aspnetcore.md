---
title: Application Insights for ASP.NET Core
category: concept
applies-to: 3.x
---

# Application Insights for ASP.NET Core

**Category:** Concept
**Applies to:** 3.x

## Overview

The `Microsoft.ApplicationInsights.AspNetCore` package provides built-in telemetry collection for ASP.NET Core applications, sending data to Azure Application Insights.

## What It Provides

One line of code enables full observability:

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Automatic Collection
- **Requests** — All incoming HTTP requests with timing, status codes, and URLs
- **Dependencies** — Outgoing HTTP calls, SQL queries, and Azure SDK calls
- **Exceptions** — Unhandled exceptions with full stack traces
- **Performance Counters** — CPU, memory, GC, and thread pool metrics
- **Logs** — ILogger output (Information level and above by default)

### Built-in Features
- **Live Metrics** — Real-time monitoring via QuickPulse
- **Adaptive Sampling** — Automatic volume control (configurable via `TracesPerSecond`)
- **JavaScript Snippet** — Browser-side telemetry injection for Razor pages
- **AAD Authentication** — Token-based auth via `TokenCredential`

## Quick Start

### 1. Install Package

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

### 2. Add to Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry();
var app = builder.Build();
```

### 3. Configure Connection String

**Environment variable (recommended):**
```bash
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=xxx;IngestionEndpoint=https://...
```

**Or in appsettings.json:**
```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=xxx;IngestionEndpoint=https://..."
  }
}
```

## Configuration Options

Use `ApplicationInsightsServiceOptions` to customize behavior:

```csharp
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
    options.EnableQuickPulseMetricStream = true;
    options.SamplingRatio = 0.5f;   // Collect 50% of telemetry
});
```

| Property | Default | Description |
|----------|---------|-------------|
| `ConnectionString` | `null` | Application Insights connection string |
| `Credential` | `null` | AAD `TokenCredential` for token-based auth |
| `EnableQuickPulseMetricStream` | `true` | Enables Live Metrics |
| `EnableDependencyTrackingTelemetryModule` | `true` | Tracks HTTP/SQL dependencies |
| `EnableRequestTrackingTelemetryModule` | `true` | Tracks incoming requests |
| `TracesPerSecond` | `5` | Rate-limited sampling target |
| `SamplingRatio` | `null` | Fixed-rate sampling (0.0–1.0); overrides `TracesPerSecond` |

## Connection String Resolution Order

1. `ApplicationInsightsServiceOptions.ConnectionString` (code)
2. Environment variable `APPLICATIONINSIGHTS_CONNECTION_STRING`
3. Config key `ApplicationInsights:ConnectionString`

## Extending with OpenTelemetry

Application Insights 3.x is built on OpenTelemetry. You can add additional sources:

```csharp
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddSource("MyApp.CustomSource"))
    .WithMetrics(m => m.AddMeter("MyApp.CustomMeter"));
```

## See Also

- AddApplicationInsightsTelemetry API (see in AddApplicationInsightsTelemetry.md)
- ASP.NET Core Setup Example (see in aspnetcore-setup.md)
- App Insights 2.x to 3.x Migration (see in appinsights-2x-to-3x-code-migration.md)
