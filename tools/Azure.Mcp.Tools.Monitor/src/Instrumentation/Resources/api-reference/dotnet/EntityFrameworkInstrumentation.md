---
title: EntityFrameworkInstrumentation
category: api-reference
applies-to: 1.x
---

# Entity Framework Core Instrumentation

## Package

```
OpenTelemetry.Instrumentation.EntityFrameworkCore
```

## Setup

```csharp
using OpenTelemetry.Trace;

// EF Core is auto-instrumented via DiagnosticSource — no additional setup needed.
// To customize, use ConfigureOpenTelemetryTracerProvider:
builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
    tracing.AddEntityFrameworkCoreInstrumentation(options =>
    {
        options.SetDbStatementForText = true;  // Include SQL text (be careful with PII)
        options.SetDbStatementForStoredProcedure = true;
    }));
```

## Options

| Option | Default | Description |
|---|---|---|
| `SetDbStatementForText` | `false` | Include raw SQL text in `db.statement` attribute. May contain PII. |
| `SetDbStatementForStoredProcedure` | `true` | Include stored procedure names in `db.statement`. |
| `EnrichWithIDbCommand` | `null` | `Action<Activity, IDbCommand>` callback to enrich spans with custom tags from the command. |
| `Filter` | `null` | `Func<string, string, bool>` — filter by provider name and command text. Return `false` to suppress. |

## Semantic conventions

EF Core spans use the [OpenTelemetry Database semantic conventions](https://opentelemetry.io/docs/specs/semconv/database/):

| Attribute | Example |
|---|---|
| `db.system` | `microsoft.sql_server`, `postgresql`, `sqlite` |
| `db.name` | `MyDatabase` |
| `db.statement` | `SELECT * FROM Orders WHERE Id = @p0` (if `SetDbStatementForText = true`) |
| `server.address` | `localhost` |
| `server.port` | `5432` |

## Notes

- EF Core instrumentation relies on `DiagnosticSource` events emitted by EF Core itself — no additional packages needed for basic span collection.
- The `OpenTelemetry.Instrumentation.EntityFrameworkCore` package is only needed to customize options (SQL text capture, enrichment, filtering).
- Works with all EF Core providers (SQL Server, PostgreSQL, SQLite, MySQL, etc.).
- **Non-DI usage:** If not using ASP.NET Core DI, use `config.ConfigureOpenTelemetryBuilder(otel => otel.WithTracing(t => t.AddEntityFrameworkCoreInstrumentation()))` on `TelemetryConfiguration`. See TelemetryClient.md.
