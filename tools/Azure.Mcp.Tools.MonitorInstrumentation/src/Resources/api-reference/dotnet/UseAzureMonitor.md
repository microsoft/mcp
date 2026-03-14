---
title: UseAzureMonitor
category: api-reference
applies-to: 3.x
source: Azure.Monitor.OpenTelemetry.AspNetCore/OpenTelemetryBuilderExtensions.cs
---

# UseAzureMonitor

**Namespace:** `Azure.Monitor.OpenTelemetry.AspNetCore`

## Overview

Extension method on `IOpenTelemetryBuilder` that configures OpenTelemetry with Azure Monitor exporters and automatic instrumentation.

## Signature

```csharp
public static IOpenTelemetryBuilder UseAzureMonitor(
    this IOpenTelemetryBuilder builder);

public static IOpenTelemetryBuilder UseAzureMonitor(
    this IOpenTelemetryBuilder builder,
    Action<AzureMonitorOptions> configureAzureMonitor);
```

## Usage

### Basic (environment variable connection string)

```csharp
using Azure.Monitor.OpenTelemetry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().UseAzureMonitor();
```

### With options

```csharp
using Azure.Monitor.OpenTelemetry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
{
    options.ConnectionString = "InstrumentationKey=...";
    options.SamplingRatio = 0.5f;
});
```

## AzureMonitorOptions Properties

| Property | Type | Description |
|----------|------|-------------|
| `ConnectionString` | `string?` | Azure Monitor connection string. If not set, reads from `APPLICATIONINSIGHTS_CONNECTION_STRING` env var |
| `SamplingRatio` | `float` | Sampling ratio (0.0 to 1.0). Default: 1.0 (100%) |
| `EnableLiveMetrics` | `bool` | Enable Live Metrics stream. Default: true |
| `Credential` | `TokenCredential?` | Azure credential for AAD authentication |
| `Resource` | `Resource?` | Custom OpenTelemetry resource |

## What It Configures

Calling `UseAzureMonitor()` automatically adds:

**Tracing:**
- ASP.NET Core instrumentation
- HttpClient instrumentation
- SQL Client instrumentation
- Azure SDK instrumentation
- Azure Monitor trace exporter

**Metrics:**
- ASP.NET Core instrumentation
- HttpClient instrumentation
- Runtime instrumentation
- Azure Monitor metric exporter

**Logging:**
- Azure Monitor log exporter

## Chaining with Additional Configuration

```csharp
builder.Services.AddOpenTelemetry()
    .UseAzureMonitor()
    .WithTracing(tracing => tracing
        .AddSource("MyCustomSource")
        .AddProcessor<MyCustomProcessor>())
    .WithMetrics(metrics => metrics
        .AddMeter("MyCustomMeter"));
```

## See Also

- AddOpenTelemetry(see in AddOpenTelemetry.md)
- Azure Monitor Distro concept(see in azure-monitor-distro.md)
