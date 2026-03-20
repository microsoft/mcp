---
title: Application Insights for Classic ASP.NET
category: concepts
applies-to: dotnet-framework
---

# Application Insights for Classic ASP.NET

## Overview

Classic ASP.NET applications (.NET Framework 4.6.2+) use `Microsoft.ApplicationInsights.Web` for automatic request, dependency, and exception tracking. In 3.x, the package is built on OpenTelemetry internally.

## How it works

1. **NuGet package install** creates `ApplicationInsights.config` and registers HTTP modules in `Web.config`
2. **`ApplicationInsightsHttpModule`** runs on every request — reads config, initializes `TelemetryConfiguration`, sets up the OpenTelemetry pipeline
3. **`TelemetryHttpModule`** (from `OpenTelemetry.Instrumentation.AspNet`) captures request spans via `System.Diagnostics.Activity`
4. **No code changes needed** — request tracking, dependency tracking, and exception tracking are automatic

## Key configuration files

| File | Purpose |
|---|---|
| `ApplicationInsights.config` | Connection string, sampling, feature flags |
| `Web.config` | HTTP module registration (auto-configured by NuGet) |

## ApplicationInsights.config (3.x format)

```xml
<?xml version="1.0" encoding="utf-8"?>
<ApplicationInsights xmlns="http://schemas.microsoft.com/ApplicationInsights/2013/Settings">
  <ConnectionString>InstrumentationKey=...;IngestionEndpoint=https://...</ConnectionString>
  <TracesPerSecond>5.0</TracesPerSecond>
  <EnableTraceBasedLogsSampler>true</EnableTraceBasedLogsSampler>
  <EnableQuickPulseMetricStream>true</EnableQuickPulseMetricStream>
  <EnablePerformanceCounterCollectionModule>true</EnablePerformanceCounterCollectionModule>
  <EnableDependencyTrackingTelemetryModule>true</EnableDependencyTrackingTelemetryModule>
  <EnableRequestTrackingTelemetryModule>true</EnableRequestTrackingTelemetryModule>
  <AddAutoCollectedMetricExtractor>true</AddAutoCollectedMetricExtractor>
</ApplicationInsights>
```

## Web.config HTTP modules (auto-configured)

```xml
<system.webServer>
  <modules>
    <add name="ApplicationInsightsWebTracking"
         type="Microsoft.ApplicationInsights.Web.ApplicationInsightsHttpModule, Microsoft.AI.Web"
         preCondition="managedHandler" />
    <add name="TelemetryHttpModule"
         type="OpenTelemetry.Instrumentation.AspNet.TelemetryHttpModule, OpenTelemetry.Instrumentation.AspNet.TelemetryHttpModule"
         preCondition="integratedMode,managedHandler" />
  </modules>
</system.webServer>
```

## Extensibility — Custom processors

Use `TelemetryConfiguration.ConfigureOpenTelemetryBuilder` in `Global.asax.cs`:

```csharp
using Microsoft.ApplicationInsights.Extensibility;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

protected void Application_Start()
{
    var config = TelemetryConfiguration.CreateDefault();
    config.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
    config.ConfigureOpenTelemetryBuilder(otel =>
    {
        otel.WithTracing(tracing => tracing.AddProcessor<MyCustomProcessor>());
        otel.ConfigureResource(r => r.AddService("MyServiceName"));
    });

    AreaRegistration.RegisterAllAreas();
    RouteConfig.RegisterRoutes(RouteTable.Routes);
}
```

## Requirements

- .NET Framework **4.6.2** or later (3.x raised minimum from 4.5.2)
- IIS or IIS Express
- `Microsoft.ApplicationInsights.Web` 3.x NuGet package

## What's automatic (no code needed)

- HTTP request tracking (all incoming requests)
- SQL dependency tracking
- HTTP dependency tracking (outgoing `HttpClient` / `WebRequest` calls)
- Exception tracking
- Performance counter collection
- Live Metrics (Quick Pulse)
