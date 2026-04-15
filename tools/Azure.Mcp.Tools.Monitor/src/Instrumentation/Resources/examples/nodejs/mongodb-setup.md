# Basic Azure Monitor Setup for Node.js with MongoDB

This guide shows how to add Azure Monitor OpenTelemetry to a Node.js application using MongoDB.

## Prerequisites

- Node.js 14.x or higher
- npm or yarn
- Node.js application with MongoDB (`mongodb` package)
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

// Initialize Azure Monitor - MongoDB operations will be automatically instrumented
useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  }
});

// Now load your application code
const express = require('express');
const { MongoClient } = require('mongodb');

const app = express();
const port = process.env.PORT || 3000;

// MongoDB connection
const mongoUrl = process.env.MONGODB_URL || 'mongodb://localhost:27017';
const dbName = process.env.MONGODB_DB || 'mydb';
let db;

async function connectToDatabase() {
  const client = new MongoClient(mongoUrl);
  await client.connect();
  db = client.db(dbName);
  console.log('Connected to MongoDB');
}

app.use(express.json());

app.get('/api/users', async (req, res) => {
  // This query will be automatically tracked as a dependency
  const users = await db.collection('users').find({}).limit(10).toArray();
  res.json(users);
});

connectToDatabase().then(() => {
  app.listen(port, () => {
    console.log(`Server listening on port ${port}`);
  });
});
```

## Step 3: Configure Connection String

Create a `.env` file in your project root:

```env
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://...
MONGODB_URL=mongodb://localhost:27017
MONGODB_DB=mydb
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

With Azure Monitor OpenTelemetry, the following MongoDB operations are automatically tracked:

- **Operations**: find, insert, update, delete, aggregate, etc.
- **Operation duration**: Time taken for each database operation
- **Collection name**: Which collection was accessed
- **Database name**: Which database was used
- **Success/failure**: Operation status

## Step 4: Add Custom Telemetry (Optional)

```javascript
const { trace } = require('@opentelemetry/api');

app.post('/api/users', async (req, res) => {
  const tracer = trace.getTracer('my-app');
  
  await tracer.startActiveSpan('create-user', async (span) => {
    try {
      span.setAttribute('user.email', req.body.email);
      
      const result = await db.collection('users').insertOne({
        name: req.body.name,
        email: req.body.email,
        createdAt: new Date()
      });
      
      span.setAttribute('user.id', result.insertedId.toString());
      res.status(201).json({ _id: result.insertedId, ...req.body });
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

## Using with Mongoose

If you're using Mongoose ODM, the setup is the same:

```javascript
require('dotenv').config();
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');

useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  }
});

const mongoose = require('mongoose');
const express = require('express');

// Mongoose operations will automatically be instrumented
mongoose.connect(process.env.MONGODB_URL);

const UserSchema = new mongoose.Schema({
  name: String,
  email: String
});

const User = mongoose.model('User', UserSchema);

const app = express();

app.get('/api/users', async (req, res) => {
  const users = await User.find().limit(10);
  res.json(users);
});
```

## Viewing Telemetry in Azure Portal

1. Open your Application Insights resource in Azure Portal
2. Navigate to "Application Map" to see MongoDB as a dependency
3. Use "Transaction search" to find specific database operations
4. Check "Dependencies" under "Investigate" to see operation performance

## Troubleshooting

### MongoDB operations not appearing

1. Ensure `useAzureMonitor()` is called **before** importing `mongodb` or `mongoose`
2. Verify the connection string is set correctly
3. Check that operations are being executed (not just connections)

### High cardinality in telemetry

MongoDB instrumentation captures collection names. For applications with dynamic collections, consider using a consistent naming pattern.
