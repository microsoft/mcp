---
title: Basic ASP.NET Core Setup
category: example
applies-to: 3.x
source: ApplicationInsightsDemo/Program.cs
---

# Basic ASP.NET Core Setup

**Category:** Example  
**Applies to:** 3.x

## Overview

Complete working example of adding Azure Monitor to a new ASP.NET Core application.

## Step 1: Add Package

```bash
dotnet add package Azure.Monitor.OpenTelemetry.AspNetCore
```

Or in `.csproj`:

```xml
<PackageReference Include="Azure.Monitor.OpenTelemetry.AspNetCore" Version="1.3.0" />
```

## Step 2: Configure in Program.cs

### Minimal Setup

```csharp
using Azure.Monitor.OpenTelemetry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Monitor - one line!
builder.Services.AddOpenTelemetry().UseAzureMonitor();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();
```

### With Configuration Options

```csharp
using Azure.Monitor.OpenTelemetry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
{
    // Connection string from configuration
    options.ConnectionString = builder.Configuration["AzureMonitor:ConnectionString"];
    
    // Sample 50% of requests in production
    if (!builder.Environment.IsDevelopment())
    {
        options.SamplingRatio = 0.5f;
    }
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
  "AzureMonitor": {
    "ConnectionString": "InstrumentationKey=xxx;IngestionEndpoint=https://..."
  }
}
```

### Option C: User Secrets (Development)

```bash
dotnet user-secrets set "AzureMonitor:ConnectionString" "InstrumentationKey=xxx;..."
```

## Complete Program.cs

```csharp
using Azure.Monitor.OpenTelemetry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Azure Monitor OpenTelemetry
builder.Services.AddOpenTelemetry().UseAzureMonitor();

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

After this setup, Azure Monitor will collect:

| Signal | Data |
|--------|------|
| **Requests** | All incoming HTTP requests with timing, status codes |
| **Dependencies** | Outgoing HTTP calls, SQL queries, Azure SDK calls |
| **Exceptions** | Unhandled exceptions with stack traces |
| **Logs** | ILogger output (Information level and above) |
| **Metrics** | Request rate, response time, CPU, memory |

## Verify It Works

1. Run your application
2. Make some requests
3. Check Application Insights in Azure Portal (may take 2-5 minutes)
4. Look for:
   - Live Metrics (immediate)
   - Transaction Search (requests, dependencies)
   - Failures (exceptions)

## See Also

- Azure Monitor Distro(see in azure-monitor-distro.md)
- UseAzureMonitor API(see in UseAzureMonitor.md)
