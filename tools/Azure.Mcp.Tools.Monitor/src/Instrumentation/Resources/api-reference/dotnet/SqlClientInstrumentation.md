---
title: SqlClientInstrumentation
category: api-reference
applies-to: 1.x
---

# SQL Client Instrumentation

## Package

```
OpenTelemetry.Instrumentation.SqlClient
```

## Setup

```csharp
using OpenTelemetry.Trace;

builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
    tracing.AddSqlClientInstrumentation(options =>
    {
        options.SetDbStatementForText = true;   // Include SQL text
        options.RecordException = true;          // Record exception details on spans
    }));
```

## Options

| Option | Default | Description |
|---|---|---|
| `SetDbStatementForText` | `false` | Include SQL command text in `db.statement`. May contain PII. |
| `SetDbStatementForStoredProcedure` | `true` | Include stored procedure name in `db.statement`. |
| `RecordException` | `false` | Record exception details as span events when SQL commands fail. |
| `EnableConnectionLevelAttributes` | `false` | Add `server.address` and `server.port` from the connection string. |
| `Enrich` | `null` | `Action<Activity, string, object>` to enrich with custom tags. Event names: `OnCustom`. |
| `Filter` | `null` | `Func<object, bool>` — return `false` to suppress a span. |

## Semantic conventions

| Attribute | Example |
|---|---|
| `db.system` | `microsoft.sql_server` |
| `db.name` | `MyDatabase` |
| `db.statement` | `SELECT * FROM Users WHERE Id = @Id` |
| `server.address` | `myserver.database.windows.net` |

## Notes

- Works with both `System.Data.SqlClient` and `Microsoft.Data.SqlClient`.
- If using EF Core with SQL Server, you may not need this package separately — EF Core instrumentation captures the same spans. Use this when you have raw `SqlCommand` / `SqlConnection` calls alongside or instead of EF Core.
- `SetDbStatementForText = true` captures raw SQL which may contain sensitive data. Use with caution in production.
- **Non-DI usage:** Use `config.ConfigureOpenTelemetryBuilder(otel => otel.WithTracing(t => t.AddSqlClientInstrumentation()))` on `TelemetryConfiguration`. See TelemetryClient.md.
