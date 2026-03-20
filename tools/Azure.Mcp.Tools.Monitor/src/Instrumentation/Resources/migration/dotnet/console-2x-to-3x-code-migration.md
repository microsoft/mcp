---
title: Console App Application Insights 2.x to 3.x Migration
category: migration
applies-to: 3.x
---

# Console App — AI SDK 2.x → 3.x Migration

## What changed

Console apps use `TelemetryConfiguration` directly (non-DI). In 3.x, the core SDK is rebuilt on OpenTelemetry internally. Several APIs on `TelemetryConfiguration` and `TelemetryClient` are removed or changed.

Key changes:
- `TelemetryConfiguration.Active` — **Removed**. Use `TelemetryConfiguration.CreateDefault()`. Returns a static singleton in 3.x.
- `config.InstrumentationKey` — **Removed**. Use `config.ConnectionString` (required — throws if not set).
- `config.TelemetryInitializers` collection — **Removed**. Use `config.ConfigureOpenTelemetryBuilder(b => b.WithTracing(t => t.AddProcessor<T>()))`.
- `config.TelemetryProcessors` / `TelemetryProcessorChainBuilder` — **Removed**. Use OpenTelemetry processors.
- `config.TelemetryChannel` / `TelemetrySinks` — **Removed**. Export pipeline managed by OpenTelemetry internally.
- `DependencyTrackingTelemetryModule` manual init — **No longer needed**. Dependency tracking is automatic in 3.x.
- `new TelemetryClient()` (parameterless) — **Removed**. Use `new TelemetryClient(config)`.
- `SetAzureTokenCredential(object)` — Signature changed to `SetAzureTokenCredential(TokenCredential)` (strongly typed).

## Before / after

**2.x**
```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;

var config = TelemetryConfiguration.Active; // or CreateDefault()
config.InstrumentationKey = "your-ikey";
config.TelemetryInitializers.Add(new MyInitializer());

var dtModule = new DependencyTrackingTelemetryModule();
dtModule.Initialize(config);

var client = new TelemetryClient(config);
client.TrackEvent("Started");
// ...
client.Flush();
```

**3.x**
```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using OpenTelemetry;

var config = TelemetryConfiguration.CreateDefault();
config.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";

// Extensibility via OpenTelemetry builder
config.ConfigureOpenTelemetryBuilder(builder =>
{
    builder.WithTracing(tracing => tracing.AddProcessor<MyActivityProcessor>());
});

var client = new TelemetryClient(config);
client.TrackEvent("Started");
// ...
client.Flush();
```

## Removed APIs

| API | Status | Replacement |
|---|---|---|
| `TelemetryConfiguration.Active` | **Removed** | `TelemetryConfiguration.CreateDefault()` (static singleton) |
| `TelemetryConfiguration(string ikey)` | **Removed** | `CreateDefault()` + set `ConnectionString` |
| `config.InstrumentationKey` | **Removed** | `config.ConnectionString` |
| `config.TelemetryInitializers` | **Removed** | `config.ConfigureOpenTelemetryBuilder(...)` with `AddProcessor<T>()` |
| `config.TelemetryProcessors` | **Removed** | `config.ConfigureOpenTelemetryBuilder(...)` with `AddProcessor<T>()` |
| `config.TelemetryChannel` | **Removed** | Managed internally by OpenTelemetry |
| `config.TelemetrySinks` | **Removed** | Use OpenTelemetry exporters via `ConfigureOpenTelemetryBuilder` |
| `new TelemetryClient()` | **Removed** | `new TelemetryClient(config)` |
| `client.InstrumentationKey` | **Removed** | Use `config.ConnectionString` |
| `DependencyTrackingTelemetryModule` | **Not needed** | Dependency tracking is automatic |
| `OperationCorrelationTelemetryInitializer` | **Not needed** | Correlation is automatic via OpenTelemetry |
| `HttpDependenciesParsingTelemetryInitializer` | **Not needed** | Parsing is automatic |

## New APIs

| API | Description |
|---|---|
| `config.ConnectionString` | Required. Set before creating `TelemetryClient`. |
| `config.ConfigureOpenTelemetryBuilder(Action<IOpenTelemetryBuilder>)` | Hook for adding processors, exporters, instrumentation. **Requires `using OpenTelemetry;`** — the `WithTracing()`, `WithLogging()`, `WithMetrics()`, and `ConfigureResource()` extension methods are in the root `OpenTelemetry` namespace. |
| `config.SetAzureTokenCredential(TokenCredential)` | AAD auth (strongly typed — was `object` in 2.x) |
| `config.SamplingRatio` | Fixed-rate sampling (0.0–1.0) |
| `config.TracesPerSecond` | Rate-limited sampling |

## Migration steps

1. Replace `TelemetryConfiguration.Active` with `TelemetryConfiguration.CreateDefault()`
2. Replace `config.InstrumentationKey = "..."` with `config.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=..."`
3. Remove manual `DependencyTrackingTelemetryModule` initialization — dependency tracking is automatic
4. Remove `OperationCorrelationTelemetryInitializer` and `HttpDependenciesParsingTelemetryInitializer` — correlation and parsing are automatic
5. Migrate custom `ITelemetryInitializer` implementations to `BaseProcessor<Activity>` — register via `config.ConfigureOpenTelemetryBuilder(b => b.WithTracing(t => t.AddProcessor<T>()))`
6. Update `SetAzureTokenCredential((object)cred)` to `SetAzureTokenCredential(cred)` (remove cast — parameter is now `TokenCredential`)
7. Update the NuGet package: `Microsoft.ApplicationInsights` to `3.*`, remove `Microsoft.ApplicationInsights.DependencyCollector`

## See also

- [TelemetryConfigurationBuilder API reference](learn://api-reference/dotnet/TelemetryConfigurationBuilder.md)
- [TelemetryClient API reference](learn://api-reference/dotnet/TelemetryClient.md)
- [AAD Authentication Migration](learn://migration/dotnet/aad-authentication-migration.md)
