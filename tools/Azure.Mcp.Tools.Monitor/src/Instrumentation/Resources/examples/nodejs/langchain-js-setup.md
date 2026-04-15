# Basic Azure Monitor Setup for LangChain.js

This guide shows how to add Azure Monitor OpenTelemetry to a LangChain.js application for observability into LLM calls, chains, and agents.

## Prerequisites

- Node.js 18.x or higher
- npm or yarn
- LangChain.js application
- Azure Application Insights resource

## Step 1: Install Package

```bash
npm install @azure/monitor-opentelemetry
```

## Step 2: Create Tracing File

Create a separate tracing file to ensure OpenTelemetry initializes before LangChain imports. This is critical for proper instrumentation.

**For CommonJS projects** - create `tracing.js`:

```javascript
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');

// Enable Azure Monitor integration
// This must be called before any other imports to ensure proper instrumentation
useAzureMonitor({
    azureMonitorExporterOptions: {
        connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
    }
});
```

**For ES Module projects** - create `tracing.mjs`:

```javascript
import { useAzureMonitor } from '@azure/monitor-opentelemetry';

// Enable Azure Monitor integration
// This must be called before any other imports to ensure proper instrumentation
useAzureMonitor({
    azureMonitorExporterOptions: {
        connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
    }
});
```

## Step 3: Import Tracing First

Update your main entry point to import tracing **as the very first line**:

**For CommonJS** (`index.js`):

```javascript
require('./tracing'); // MUST be the first import

const { ChatOpenAI } = require('@langchain/openai');
const { PromptTemplate } = require('@langchain/core/prompts');
const { StringOutputParser } = require('@langchain/core/output_parsers');

// Your LangChain application code
async function main() {
    const model = new ChatOpenAI({
        modelName: 'gpt-4',
        temperature: 0.7
    });

    const prompt = PromptTemplate.fromTemplate(
        'Tell me a short joke about {topic}'
    );

    const chain = prompt.pipe(model).pipe(new StringOutputParser());

    const result = await chain.invoke({ topic: 'programming' });
    console.log(result);
}

main().catch(console.error);
```

**For ES Modules** (`index.mjs`):

```javascript
import './tracing.mjs'; // MUST be the first import

import { ChatOpenAI } from '@langchain/openai';
import { PromptTemplate } from '@langchain/core/prompts';
import { StringOutputParser } from '@langchain/core/output_parsers';

// Your LangChain application code
async function main() {
    const model = new ChatOpenAI({
        modelName: 'gpt-4',
        temperature: 0.7
    });

    const prompt = PromptTemplate.fromTemplate(
        'Tell me a short joke about {topic}'
    );

    const chain = prompt.pipe(model).pipe(new StringOutputParser());

    const result = await chain.invoke({ topic: 'programming' });
    console.log(result);
}

main().catch(console.error);
```

## Step 4: Configure Connection String

Create a `.env` file in your project root:

```env
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://...
OPENAI_API_KEY=your-openai-key
```

Install `dotenv` to load environment variables:

```bash
npm install dotenv
```

Update your tracing file to load environment variables first:

```javascript
require('dotenv').config();
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');
// ... rest of tracing setup
```

## Step 5: Add Custom Telemetry (Optional)

Track custom attributes for LLM operations:

```javascript
const { trace } = require('@opentelemetry/api');

async function processWithTelemetry(userQuery) {
    const span = trace.getActiveSpan();
    
    // Add custom attributes
    span?.setAttribute('llm.query.length', userQuery.length);
    span?.setAttribute('llm.model', 'gpt-4');
    span?.setAttribute('operation.type', 'chat-completion');
    
    try {
        const result = await chain.invoke({ query: userQuery });
        
        // Track response metrics
        span?.setAttribute('llm.response.length', result.length);
        span?.setAttribute('llm.success', true);
        
        return result;
    } catch (error) {
        span?.recordException(error);
        span?.setAttribute('llm.success', false);
        throw error;
    }
}
```

## What Gets Tracked Automatically

✅ **HTTP Requests**: Outgoing calls to LLM APIs (OpenAI, Azure OpenAI, etc.)  
✅ **Dependencies**: External service calls and database queries  
✅ **Exceptions**: Errors from LLM providers, rate limits, timeouts  
✅ **Performance**: Latency of LLM calls and chains  
✅ **Token Usage**: When using supported providers

## Using with Different LLM Providers

### Azure OpenAI

```javascript
const { AzureChatOpenAI } = require('@langchain/openai');

const model = new AzureChatOpenAI({
    azureOpenAIApiDeploymentName: process.env.AZURE_OPENAI_DEPLOYMENT,
    azureOpenAIApiVersion: '2024-02-15-preview',
});
```

### Anthropic Claude

```javascript
const { ChatAnthropic } = require('@langchain/anthropic');

const model = new ChatAnthropic({
    modelName: 'claude-3-opus-20240229',
});
```

## Using with Agents

```javascript
require('./tracing');

const { ChatOpenAI } = require('@langchain/openai');
const { AgentExecutor, createOpenAIToolsAgent } = require('langchain/agents');
const { TavilySearchResults } = require('@langchain/community/tools/tavily_search');
const { trace } = require('@opentelemetry/api');

async function runAgent(input) {
    const span = trace.getActiveSpan();
    span?.setAttribute('agent.input', input);
    
    const tools = [new TavilySearchResults()];
    const model = new ChatOpenAI({ modelName: 'gpt-4' });
    
    const agent = await createOpenAIToolsAgent({ llm: model, tools, prompt });
    const executor = new AgentExecutor({ agent, tools });
    
    const result = await executor.invoke({ input });
    
    span?.setAttribute('agent.steps', result.intermediateSteps?.length || 0);
    return result;
}
```

## Verify It Works

1. Start your application:
   ```bash
   node index.js
   ```

2. Run some LLM operations and check Azure Portal:
   - Navigate to your Application Insights resource
   - Go to "Transaction search" or "Application map"
   - You should see outgoing requests to LLM APIs
   - Check "Dependencies" to see LLM call latencies

## Complete package.json Example

**CommonJS:**
```json
{
  "name": "langchain-azure-monitor-demo",
  "version": "1.0.0",
  "main": "index.js",
  "scripts": {
    "start": "node index.js"
  },
  "dependencies": {
    "@azure/monitor-opentelemetry": "^1.0.0",
    "@langchain/core": "^0.2.0",
    "@langchain/openai": "^0.2.0",
    "dotenv": "^16.0.0"
  }
}
```

**ES Modules:**
```json
{
  "name": "langchain-azure-monitor-demo",
  "version": "1.0.0",
  "type": "module",
  "main": "index.mjs",
  "scripts": {
    "start": "node index.mjs"
  },
  "dependencies": {
    "@azure/monitor-opentelemetry": "^1.0.0",
    "@langchain/core": "^0.2.0",
    "@langchain/openai": "^0.2.0",
    "dotenv": "^16.0.0"
  }
}
```

## Project Structure

```
my-langchain-app/
├── tracing.js          ← Azure Monitor setup (load first)
├── index.js            ← Main entry point
├── chains/
│   └── qa-chain.js     ← Your LangChain chains
├── .env                ← Connection strings and API keys
└── package.json
```

## Troubleshooting

**No telemetry appearing?**
- Verify the tracing import is the FIRST line in your entry file
- Check that connection string is correct
- Ensure `dotenv.config()` is called in tracing file before `useAzureMonitor()`

**LLM calls not tracked?**
- Make sure OpenTelemetry initializes BEFORE importing LangChain
- HTTP instrumentation should capture LLM API calls automatically

**ES Module vs CommonJS issues?**
- Check your `package.json` for `"type": "module"`
- Use `.mjs` extension for ES modules or `.cjs` for CommonJS
- Match import/require syntax to your module system

## Next Steps

- Add custom metrics for token usage and costs
- Set up alerts for LLM error rates or latency
- Create dashboards for LLM operation insights
- Enable distributed tracing for multi-service architectures
- Track RAG pipeline performance with custom spans
