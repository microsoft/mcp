# Basic Azure Monitor Setup for Node.js with MySQL

This guide shows how to add Azure Monitor OpenTelemetry to a Node.js application using MySQL.

## Prerequisites

- Node.js 14.x or higher
- npm or yarn
- Node.js application with MySQL (`mysql2` package)
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

// Initialize Azure Monitor - MySQL queries will be automatically instrumented
useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  }
});

// Now load your application code
const express = require('express');
const mysql = require('mysql2/promise');

const app = express();
const port = process.env.PORT || 3000;

// MySQL connection pool
let pool;

async function createPool() {
  pool = mysql.createPool({
    host: process.env.MYSQL_HOST || 'localhost',
    port: process.env.MYSQL_PORT || 3306,
    user: process.env.MYSQL_USER || 'root',
    password: process.env.MYSQL_PASSWORD || '',
    database: process.env.MYSQL_DATABASE || 'mydb',
    waitForConnections: true,
    connectionLimit: 10
  });
  console.log('MySQL connection pool created');
}

app.use(express.json());

app.get('/api/users', async (req, res) => {
  // This query will be automatically tracked as a dependency
  const [rows] = await pool.execute('SELECT * FROM users LIMIT 10');
  res.json(rows);
});

createPool().then(() => {
  app.listen(port, () => {
    console.log(`Server listening on port ${port}`);
  });
});
```

## Step 3: Configure Connection String

Create a `.env` file in your project root:

```env
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://...
MYSQL_HOST=localhost
MYSQL_PORT=3306
MYSQL_USER=root
MYSQL_PASSWORD=your_password
MYSQL_DATABASE=mydb
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

With Azure Monitor OpenTelemetry, the following MySQL operations are automatically tracked:

- **Queries**: All SQL queries executed via `mysql2` client
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
      
      const [result] = await pool.execute(
        'INSERT INTO users (name, email) VALUES (?, ?)',
        [req.body.name, req.body.email]
      );
      
      span.setAttribute('user.id', result.insertId);
      res.status(201).json({ id: result.insertId, ...req.body });
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

## Using with Azure Database for MySQL

For Azure Database for MySQL, update your configuration:

```env
MYSQL_HOST=your-server.mysql.database.azure.com
MYSQL_PORT=3306
MYSQL_USER=your_user@your-server
MYSQL_PASSWORD=your_password
MYSQL_DATABASE=mydb
```

And enable SSL in your connection:

```javascript
const pool = mysql.createPool({
  host: process.env.MYSQL_HOST,
  port: process.env.MYSQL_PORT,
  user: process.env.MYSQL_USER,
  password: process.env.MYSQL_PASSWORD,
  database: process.env.MYSQL_DATABASE,
  ssl: {
    rejectUnauthorized: true
  }
});
```

## Using with Sequelize ORM

If you're using Sequelize, the setup is similar:

```javascript
require('dotenv').config();
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');

useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  }
});

const { Sequelize, DataTypes } = require('sequelize');
const express = require('express');

const sequelize = new Sequelize(
  process.env.MYSQL_DATABASE,
  process.env.MYSQL_USER,
  process.env.MYSQL_PASSWORD,
  {
    host: process.env.MYSQL_HOST,
    dialect: 'mysql'
  }
);

const User = sequelize.define('User', {
  name: DataTypes.STRING,
  email: DataTypes.STRING
});

const app = express();

app.get('/api/users', async (req, res) => {
  const users = await User.findAll({ limit: 10 });
  res.json(users);
});
```

## Viewing Telemetry in Azure Portal

1. Open your Application Insights resource in Azure Portal
2. Navigate to "Application Map" to see MySQL as a dependency
3. Use "Transaction search" to find specific database operations
4. Check "Dependencies" under "Investigate" to see query performance

## Troubleshooting

### MySQL queries not appearing

1. Ensure `useAzureMonitor()` is called **before** importing `mysql2`
2. Verify the connection string is set correctly
3. Check that queries are being executed (not just connections)

### High latency in telemetry

MySQL instrumentation captures all queries. For high-throughput applications, consider:

```javascript
useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  },
  samplingRatio: 0.5 // Sample 50% of requests
});
```
