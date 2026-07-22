---
title: Classic ASP.NET Setup
category: examples
applies-to: dotnet-framework
---

# Classic ASP.NET — Application Insights Setup

## Prerequisites

- .NET Framework 4.6.2+ project (ASP.NET MVC, WebForms, or generic ASP.NET)
- Visual Studio or MSBuild for building

## Step 1 — Install the NuGet package

In Visual Studio, open the **Package Manager Console** (View → Other Windows → Package Manager Console) and run:

```
Install-Package Microsoft.ApplicationInsights.Web
```

This automatically:
- Creates `ApplicationInsights.config` with default 3.x settings
- Adds `ApplicationInsightsHttpModule` and `TelemetryHttpModule` to `Web.config`
- Adds all required assembly references to the project
- Updates `packages.config`

## Step 2 — Set connection string

In `ApplicationInsights.config`, replace the placeholder `<ConnectionString>` with your actual connection string:

```xml
<ConnectionString>InstrumentationKey=your-key;IngestionEndpoint=https://dc.applicationinsights.azure.com/</ConnectionString>
```

Or set the `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable.

## Step 3 — Run and verify

1. Build and run the application (F5 in Visual Studio)
2. Make a few HTTP requests
3. Check Azure Portal → Application Insights → Live Metrics to see incoming telemetry
4. Check Transaction Search for request and dependency data

## What's collected automatically

- All incoming HTTP requests (path, status code, duration)
- SQL and HTTP outgoing dependency calls
- Unhandled exceptions
- Performance counters (CPU, memory, request rate)
- Live Metrics stream

## Optional: Custom telemetry

To track custom events or metrics, use `TelemetryClient`:

```csharp
var client = new TelemetryClient(TelemetryConfiguration.CreateDefault());
client.TrackEvent("OrderCreated");
client.TrackMetric("ProcessingTime", elapsed);
```

## Optional: Custom processors

In `Global.asax.cs`:

```csharp
protected void Application_Start()
{
    var config = TelemetryConfiguration.CreateDefault();
    config.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
    config.ConfigureOpenTelemetryBuilder(otel =>
    {
        otel.WithTracing(tracing => tracing.AddProcessor<MyCustomProcessor>());
    });

    AreaRegistration.RegisterAllAreas();
    RouteConfig.RegisterRoutes(RouteTable.Routes);
}
```
