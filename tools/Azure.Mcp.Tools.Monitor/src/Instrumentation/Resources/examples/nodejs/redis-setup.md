# Basic Azure Monitor Setup for Node.js with Redis

This guide shows how to add Azure Monitor OpenTelemetry to a Node.js application using Redis.

## Prerequisites

- Node.js 14.x or higher
- npm or yarn
- Node.js application with Redis (`redis` or `ioredis` package)
- Azure Application Insights resource

## Step 1: Install Package

```bash
npm install @azure/monitor-opentelemetry
```

## Step 2: Initialize at Startup

Create or update your main entry point (typically `index.js` or `server.js`):

```javascript
// IMPORTANT: This must be the first line, before any other imports
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');

// Initialize Azure Monitor - Redis operations will be automatically instrumented
useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  }
});

// Now load your application code
const express = require('express');
const { createClient } = require('redis');

const app = express();
const port = process.env.PORT || 3000;

// Redis client
const redisUrl = process.env.REDIS_URL || 'redis://localhost:6379';
const redisClient = createClient({ url: redisUrl });

async function connectToRedis() {
  redisClient.on('error', err => console.error('Redis Client Error:', err));
  await redisClient.connect();
  console.log('Connected to Redis');
}

app.use(express.json());

app.get('/api/cache/:key', async (req, res) => {
  // This operation will be automatically tracked as a dependency
  const value = await redisClient.get(req.params.key);
  if (value === null) {
    return res.status(404).json({ error: 'Key not found' });
  }
  res.json({ key: req.params.key, value: JSON.parse(value) });
});

app.post('/api/cache', async (req, res) => {
  const { key, value, ttl } = req.body;
  const options = ttl ? { EX: ttl } : {};
  await redisClient.set(key, JSON.stringify(value), options);
  res.status(201).json({ key, value });
});

connectToRedis().then(() => {
  app.listen(port, () => {
    console.log(`Server listening on port ${port}`);
  });
});
```

## Step 3: Configure Connection String

Create a `.env` file in your project root:

```env
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://...
REDIS_URL=redis://localhost:6379
PORT=3000
```

Install `dotenv` to load environment variables:

```bash
npm install dotenv
```

Load it at the very top of your entry file:

```javascript
require('dotenv').config();
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');
// ... rest of code
```

## What Gets Instrumented Automatically

With Azure Monitor OpenTelemetry, the following Redis operations are automatically tracked:

- **Commands**: GET, SET, DEL, HGET, HSET, LPUSH, etc.
- **Command duration**: Time taken for each operation
- **Database index**: Which Redis database was used
- **Success/failure**: Operation status

## Using with ioredis

If you're using `ioredis`, the setup is the same:

```javascript
require('dotenv').config();
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');

useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  }
});

const Redis = require('ioredis');
const express = require('express');

const redis = new Redis(process.env.REDIS_URL);

const app = express();

app.get('/api/cache/:key', async (req, res) => {
  const value = await redis.get(req.params.key);
  res.json({ key: req.params.key, value });
});
```

## Step 4: Add Custom Telemetry (Optional)

```javascript
const { trace } = require('@opentelemetry/api');

app.post('/api/session', async (req, res) => {
  const tracer = trace.getTracer('my-app');
  
  await tracer.startActiveSpan('create-session', async (span) => {
    try {
      const sessionId = `session:${Date.now()}`;
      span.setAttribute('session.id', sessionId);
      
      await redisClient.set(sessionId, JSON.stringify(req.body), { EX: 3600 });
      
      res.status(201).json({ sessionId });
    } catch (error) {
      span.recordException(error);
      span.setStatus({ code: 2, message: error.message });
      res.status(500).json({ error: 'Cache error' });
    } finally {
      span.end();
    }
  });
});
```

## Using with Azure Cache for Redis

For Azure Cache for Redis, update your connection string:

```env
REDIS_URL=rediss://:YOUR_ACCESS_KEY@YOUR_REDIS_NAME.redis.cache.windows.net:6380
```

Note the `rediss://` protocol for SSL connections (required by Azure Cache for Redis).

## Viewing Telemetry in Azure Portal

1. Open your Application Insights resource in Azure Portal
2. Navigate to "Application Map" to see Redis as a dependency
3. Use "Transaction search" to find specific cache operations
4. Check "Dependencies" under "Investigate" to see operation performance

## Troubleshooting

### Redis operations not appearing

1. Ensure `useAzureMonitor()` is called **before** importing `redis` or `ioredis`
2. Verify the connection string is set correctly
3. Check that operations are being executed (not just connections)

### High volume of telemetry

Redis is often used for high-frequency operations. Consider sampling:

```javascript
useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  },
  samplingRatio: 0.1 // Sample 10% of requests
});
```
