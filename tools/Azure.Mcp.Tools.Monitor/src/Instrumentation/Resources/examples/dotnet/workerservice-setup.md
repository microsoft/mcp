---
title: Basic Worker Service Setup
category: example
applies-to: 3.x
---

# Basic Worker Service Setup

**Category:** Example  
**Applies to:** 3.x

## Overview

Complete working example of adding Application Insights telemetry to a .NET Worker Service application using `Microsoft.ApplicationInsights.WorkerService`.

Worker Services are long-running background services that don't handle HTTP requests directly. Common examples include:
- Background job processors
- Message queue consumers
- Scheduled task runners
- Windows Services / Linux daemons

## Step 1: Add Package

```bash
dotnet add package Microsoft.ApplicationInsights.WorkerService --version 3.0.0-rc1
```

Or in `.csproj`:

```xml
<PackageReference Include="Microsoft.ApplicationInsights.WorkerService" Version="3.0.0-rc1" />
```

## Step 2: Configure in Program.cs

### Using Host.CreateDefaultBuilder (Traditional Pattern)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Add Application Insights telemetry
        services.AddApplicationInsightsTelemetryWorkerService();
        
        // Add your worker service
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
```

### Using Host.CreateApplicationBuilder (Modern Pattern)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetryWorkerService();

// Add your worker service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
```

### With Configuration Options

```csharp
services.AddApplicationInsightsTelemetryWorkerService(options =>
{
    // Explicitly set connection string
    options.ConnectionString = configuration["ApplicationInsights:ConnectionString"];

    // Enable dependency tracking
    options.EnableDependencyTrackingTelemetryModule = true;
});
```

## Step 3: Configure Connection String

### Option A: Environment Variable (Recommended for Production)

```bash
export APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=xxx;IngestionEndpoint=https://..."
```

### Option B: appsettings.json

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=xxx;IngestionEndpoint=https://..."
  }
}
```

## What Gets Instrumented Automatically

The `AddApplicationInsightsTelemetryWorkerService()` method configures:

- **Dependency Tracking**: HTTP client calls, SQL queries, Azure SDK calls
- **Performance Counters**: CPU, memory, GC metrics
- **Exception Tracking**: Unhandled exceptions
- **Custom Telemetry**: Via `TelemetryClient` injection

## Adding Custom Telemetry

Inject `TelemetryClient` into your worker:

```csharp
public class Worker : BackgroundService
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<Worker> _logger;

    public Worker(TelemetryClient telemetryClient, ILogger<Worker> logger)
    {
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Track custom events
            _telemetryClient.TrackEvent("WorkerIteration", new Dictionary<string, string>
            {
                ["timestamp"] = DateTime.UtcNow.ToString("O")
            });

            // Track custom metrics
            _telemetryClient.TrackMetric("ItemsProcessed", processedCount);

            await Task.Delay(1000, stoppingToken);
        }
    }
}
```

## Best Practices

1. **Use structured logging**: ILogger integration sends logs to Application Insights automatically
2. **Track operation context**: Use `TelemetryClient.StartOperation` for long-running operations
3. **Flush on shutdown**: Call `TelemetryClient.Flush()` before application exits
4. **Configure sampling**: For high-volume services, configure adaptive sampling to control costs
