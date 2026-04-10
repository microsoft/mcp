---
title: ASP.NET Core Setup with Application Insights
category: example
applies-to: 3.x
---

# ASP.NET Core Setup with Application Insights

**Category:** Example
**Applies to:** 3.x

## Overview

Complete working example of adding Application Insights to a new ASP.NET Core application using `Microsoft.ApplicationInsights.AspNetCore`.

## Step 1: Add Package

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

Or in `.csproj`:

```xml
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="3.*" />
```

## Step 2: Configure in Program.cs

### Minimal Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Application Insights - one line!
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();
```

### With Configuration Options

```csharp
using Microsoft.ApplicationInsights.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableQuickPulseMetricStream = true;
    options.SamplingRatio = 0.5f;   // Collect 50% of telemetry
});

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();
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

### Option C: User Secrets (Development)

```bash
dotnet user-secrets set "ApplicationInsights:ConnectionString" "InstrumentationKey=xxx;..."
```

## Complete Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## What You Get Automatically

After this setup, Application Insights will collect:

| Signal | Data |
|--------|------|
| **Requests** | All incoming HTTP requests with timing, status codes |
| **Dependencies** | Outgoing HTTP calls, SQL queries, Azure SDK calls |
| **Exceptions** | Unhandled exceptions with stack traces |
| **Logs** | ILogger output (Information level and above) |
| **Metrics** | Request rate, response time, CPU, memory |
| **Live Metrics** | Real-time monitoring via QuickPulse |
| **Performance Counters** | GC, thread pool, process metrics |

## Extending with OpenTelemetry

Application Insights 3.x is built on OpenTelemetry. You can add custom sources and meters:

```csharp
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddSource("MyApp.CustomSource"))
    .WithMetrics(m => m.AddMeter("MyApp.CustomMeter"));
```

## Verify It Works

1. Run your application
2. Make some requests
3. Check Application Insights in Azure Portal (may take 2-5 minutes)
4. Look for:
   - Live Metrics (immediate)
   - Transaction Search (requests, dependencies)
   - Failures (exceptions)

## See Also

- Application Insights for ASP.NET Core (see in appinsights-aspnetcore.md)
- AddApplicationInsightsTelemetry API (see in AddApplicationInsightsTelemetry.md)
- App Insights 2.x to 3.x Migration (see in appinsights-2x-to-3x-code-migration.md)
