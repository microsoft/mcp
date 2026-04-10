# Basic Azure Monitor Setup for Node.js with PostgreSQL

This guide shows how to add Azure Monitor OpenTelemetry to a Node.js application using PostgreSQL.

## Prerequisites

- Node.js 14.x or higher
- npm or yarn
- Node.js application with PostgreSQL (`pg` package)
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

// Initialize Azure Monitor - PostgreSQL queries will be automatically instrumented
useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  }
});

// Now load your application code
const express = require('express');
const { Pool } = require('pg');

const app = express();
const port = process.env.PORT || 3000;

// PostgreSQL connection pool
const pool = new Pool({
  connectionString: process.env.DATABASE_URL
});

app.use(express.json());

app.get('/api/users', async (req, res) => {
  // This query will be automatically tracked as a dependency
  const result = await pool.query('SELECT * FROM users LIMIT 10');
  res.json(result.rows);
});

app.listen(port, () => {
  console.log(`Server listening on port ${port}`);
});
```

## Step 3: Configure Connection String

Create a `.env` file in your project root:

```env
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://...
DATABASE_URL=postgresql://user:password@localhost:5432/mydb
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

With Azure Monitor OpenTelemetry, the following PostgreSQL operations are automatically tracked:

- **Queries**: All SQL queries executed via `pg` client
- **Query duration**: Time taken for each database operation
- **Query results**: Success/failure status
- **Connection info**: Database name and server details

## Step 4: Add Custom Telemetry (Optional)

```javascript
const { trace } = require('@opentelemetry/api');

app.post('/api/users', async (req, res) => {
  const tracer = trace.getTracer('my-app');
  
  await tracer.startActiveSpan('create-user', async (span) => {
    try {
      span.setAttribute('user.email', req.body.email);
      
      const result = await pool.query(
        'INSERT INTO users (name, email) VALUES ($1, $2) RETURNING *',
        [req.body.name, req.body.email]
      );
      
      span.setAttribute('user.id', result.rows[0].id);
      res.status(201).json(result.rows[0]);
    } catch (error) {
      span.recordException(error);
      span.setStatus({ code: 2, message: error.message });
      res.status(500).json({ error: 'Database error' });
    } finally {
      span.end();
    }
  });
});
```

## Viewing Telemetry in Azure Portal

1. Open your Application Insights resource in Azure Portal
2. Navigate to "Application Map" to see PostgreSQL as a dependency
3. Use "Transaction search" to find specific database operations
4. Check "Dependencies" under "Investigate" to see query performance

## Troubleshooting

### PostgreSQL queries not appearing

1. Ensure `useAzureMonitor()` is called **before** importing `pg`
2. Verify the connection string is set correctly
3. Check that queries are being executed (not just connections)

### High latency in telemetry

PostgreSQL instrumentation captures all queries. For high-throughput applications, consider:

```javascript
useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  },
  samplingRatio: 0.5 // Sample 50% of requests
});
```
