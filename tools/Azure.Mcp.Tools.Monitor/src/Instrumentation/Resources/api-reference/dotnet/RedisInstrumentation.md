---
title: RedisInstrumentation
category: api-reference
applies-to: 1.x
---

# Redis Instrumentation (StackExchange.Redis)

## Package

```
OpenTelemetry.Instrumentation.StackExchangeRedis
```

## Setup

```csharp
using OpenTelemetry.Trace;
using OpenTelemetry.Instrumentation.StackExchangeRedis;

// Register IConnectionMultiplexer in DI
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379"));

// Add Redis instrumentation
builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
    tracing.AddRedisInstrumentation());
```

## Options with customization

```csharp
builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
    tracing.AddRedisInstrumentation(options =>
    {
        options.SetVerboseDatabaseStatements = true;
    }));
```

## Options

| Option | Default | Description |
|---|---|---|
| `SetVerboseDatabaseStatements` | `false` | Include full Redis command in `db.statement` (e.g. `GET mykey`). |
| `EnrichActivityWithTimingEvents` | `true` | Add Redis timing events (enqueue, sent, response) as Activity events. |
| `Enrich` | `null` | `Action<Activity, IProfiledCommand>` callback to add custom tags. |
| `FlushInterval` | 1 second | How often to flush profiling sessions. |

## Semantic conventions

| Attribute | Example |
|---|---|
| `db.system` | `redis` |
| `db.statement` | `GET mykey` (if verbose) |
| `server.address` | `localhost` |
| `server.port` | `6379` |
| `db.redis.database_index` | `0` |

## Notes

- The Redis instrumentation hooks into `StackExchange.Redis` profiling. It requires either DI-registered `IConnectionMultiplexer` (auto-discovered) or passing the connection explicitly.
- When using DI registration, the instrumentation automatically discovers all `IConnectionMultiplexer` instances — no need to pass the connection manually.
- **Non-DI usage:** Use `config.ConfigureOpenTelemetryBuilder(otel => otel.WithTracing(t => t.AddRedisInstrumentation(connection)))` on `TelemetryConfiguration`. See TelemetryClient.md.
