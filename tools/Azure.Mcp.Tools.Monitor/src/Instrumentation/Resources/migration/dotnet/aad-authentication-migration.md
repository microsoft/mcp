---
title: AAD Authentication Migration (2.x to 3.x)
category: migration
applies-to: 3.x
---

# AAD Authentication Migration (2.x → 3.x)

## What changed

In 2.x, `SetAzureTokenCredential(object)` existed on `TelemetryConfiguration` but accepted an `object` parameter — it used reflection internally to avoid a hard dependency on `Azure.Core`. Some teams also used `IConfigureOptions<TelemetryConfiguration>` workarounds to configure credentials in DI scenarios.

In 3.x, the method signature changed to **strongly typed** and new DI-friendly options were added:
- `TelemetryConfiguration.SetAzureTokenCredential(TokenCredential)` — **Signature changed** from `object` to `TokenCredential`. Must be called before a `TelemetryClient` is created from that configuration.
- `ApplicationInsightsServiceOptions.Credential` — **New in 3.x**. Preferred for DI scenarios (ASP.NET Core / Worker Service). Set your `TokenCredential` directly in the options lambda.

Other key changes:
- `TelemetryConfiguration.Active` — **Removed**. Use `TelemetryConfiguration.CreateDefault()` instead. Note: `CreateDefault()` now returns an internal static configuration (singleton-like) rather than a new instance each time.

---

## DI Scenario (ASP.NET Core / Worker Service)

In 2.x, AAD auth in DI apps was commonly configured via `IConfigureOptions<TelemetryConfiguration>` workarounds. In 3.x, use the new `Credential` property on `ApplicationInsightsServiceOptions` directly — simpler and no extra class needed.

### Before (2.x — IConfigureOptions pattern)

```csharp
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;
using Azure.Identity;

public class TelemetryConfigurationEnricher : IConfigureOptions<TelemetryConfiguration>
{
    public void Configure(TelemetryConfiguration options)
    {
        // 2.x signature: SetAzureTokenCredential(object) — accepts object, uses reflection
        object credential = new DefaultAzureCredential();
        options.SetAzureTokenCredential(credential);
    }
}

// In Startup.cs / Program.cs:
services.AddSingleton<IConfigureOptions<TelemetryConfiguration>, TelemetryConfigurationEnricher>();
services.AddApplicationInsightsTelemetry();
```

### After (3.x — Credential property on options)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Azure.Identity;

builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
    options.Credential = new DefaultAzureCredential();
});
```

### Migration steps (DI)

1. **Remove the `IConfigureOptions<TelemetryConfiguration>` class** that calls `SetAzureTokenCredential()` (e.g., `TelemetryConfigurationEnricher`). Delete the entire class file if it only handled AAD auth.

2. **Remove the DI registration** for the configurator:
   ```csharp
   // Delete this line:
   services.AddSingleton<IConfigureOptions<TelemetryConfiguration>, TelemetryConfigurationEnricher>();
   ```

3. **Set the `Credential` property** in your `AddApplicationInsightsTelemetry()` options:
   ```csharp
   builder.Services.AddApplicationInsightsTelemetry(options =>
   {
       options.Credential = new DefaultAzureCredential();
   });
   ```

4. For **Worker Service** apps, the same applies to `AddApplicationInsightsTelemetryWorkerService()`:
   ```csharp
   services.AddApplicationInsightsTelemetryWorkerService(options =>
   {
       options.Credential = new DefaultAzureCredential();
   });
   ```

---

## Non-DI Scenario (Console apps, manual TelemetryConfiguration)

For apps that create `TelemetryConfiguration` directly (console apps, batch jobs, etc.), the new `SetAzureTokenCredential` method provides built-in AAD support. The main breaking change is that `TelemetryConfiguration.Active` is removed.

### Before (2.x — TelemetryConfiguration.Active)

```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Azure.Identity;

var config = TelemetryConfiguration.Active;
config.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
// 2.x signature: SetAzureTokenCredential(object) — accepts object, uses reflection
config.SetAzureTokenCredential((object)new DefaultAzureCredential());

var client = new TelemetryClient(config);
```

### After (3.x — CreateDefault)

```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Azure.Identity;

var config = TelemetryConfiguration.CreateDefault();
config.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
// 3.x signature: SetAzureTokenCredential(TokenCredential) — strongly typed
config.SetAzureTokenCredential(new DefaultAzureCredential());

var client = new TelemetryClient(config);
```

### Migration steps (non-DI)

1. **Replace `TelemetryConfiguration.Active`** with `TelemetryConfiguration.CreateDefault()` — the `Active` static property is removed in 3.x. Note that `CreateDefault()` returns an internal static configuration (singleton-like) rather than creating a new instance.

2. **Use `SetAzureTokenCredential(TokenCredential)`** — the parameter type changed from `object` (2.x) to strongly typed `TokenCredential` (3.x). If your code previously cast to `object`, remove the cast. Call it **before** creating a `TelemetryClient` from that configuration.

3. If using a custom `TokenCredential` (e.g., `ManagedIdentityCredential` with a specific client ID), pass it directly:
   ```csharp
   var credential = new ManagedIdentityCredential("your-client-id");
   var config = TelemetryConfiguration.CreateDefault();
   config.SetAzureTokenCredential(credential);
   var client = new TelemetryClient(config);
   ```

---

## Notes

- The `Azure.Identity` package is still required — no change there.
- If you previously used `SetAzureTokenCredential` conditionally (e.g., only in certain environments), apply the same condition when setting `options.Credential` (DI) or calling `SetAzureTokenCredential` (non-DI).
- Both paths ultimately set `AzureMonitorExporterOptions.Credential` under the hood.

## See also

- [AddApplicationInsightsTelemetry API reference](learn://api-reference/dotnet/AddApplicationInsightsTelemetry.md) — full list of `ApplicationInsightsServiceOptions` properties
- [UseAzureMonitor API reference](learn://api-reference/dotnet/UseAzureMonitor.md) — if migrating to Azure Monitor OpenTelemetry Distro instead
- [App Insights 2.x → 3.x code migration](learn://migration/dotnet/appinsights-2x-to-3x-code-migration.md) — full migration guide
