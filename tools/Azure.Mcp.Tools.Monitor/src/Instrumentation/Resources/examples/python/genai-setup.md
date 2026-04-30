# Basic Azure Monitor Setup for GenAI Applications

This guide shows how to add Azure Monitor OpenTelemetry to applications using GenAI libraries like OpenAI, LangChain, or Anthropic.

## Prerequisites

- Python 3.8 or higher
- GenAI library (openai, langchain, anthropic, etc.)
- Azure Application Insights resource

## Step 1: Install Packages

For OpenAI applications:
```bash
pip install azure-monitor-opentelemetry openai opentelemetry-instrumentation-openai
```

For LangChain applications:
```bash
pip install azure-monitor-opentelemetry langchain opentelemetry-instrumentation-langchain
```

For Anthropic applications:
```bash
pip install azure-monitor-opentelemetry anthropic opentelemetry-instrumentation-anthropic
```

Or add to your `requirements.txt`:
```
azure-monitor-opentelemetry>=1.8.3
openai
opentelemetry-instrumentation-openai
```

## Step 2: Initialize at Startup

Update your main application file (e.g., `app.py`):

```python
# IMPORTANT: Configure Azure Monitor BEFORE importing GenAI libraries
from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

# Now import your GenAI libraries
from openai import OpenAI

client = OpenAI()

def generate_completion(prompt: str):
    """Generate a completion - automatically traced by Azure Monitor."""
    response = client.chat.completions.create(
        model="gpt-4",
        messages=[
            {"role": "system", "content": "You are a helpful assistant."},
            {"role": "user", "content": prompt}
        ]
    )
    return response.choices[0].message.content

if __name__ == "__main__":
    result = generate_completion("What is the capital of France?")
    print(f"Response: {result}")
```

## Step 3: Configure Connection String

Create a `.env` file:
```env
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://...
OPENAI_API_KEY=sk-...
```

Load environment variables:
```python
from dotenv import load_dotenv
load_dotenv()

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

from openai import OpenAI
# ... rest of your app
```

## What Gets Auto-Instrumented

The Azure Monitor Distro with GenAI instrumentations automatically captures:
- ✅ All LLM API calls (OpenAI, Anthropic, etc.)
- ✅ Request duration and latency
- ✅ Token usage (prompt tokens, completion tokens, total)
- ✅ Model names and parameters (temperature, max_tokens, etc.)
- ✅ Prompt and completion content (configurable)
- ✅ Error details and exceptions
- ✅ Chain execution in LangChain applications
- ✅ Agent interactions and tool calls

## Advanced: Custom Tracing

Add custom spans for business logic:

```python
from opentelemetry import trace

tracer = trace.get_tracer(__name__)

def process_user_query(user_id: str, query: str):
    with tracer.start_as_current_span("process_query") as span:
        span.set_attribute("user.id", user_id)
        span.set_attribute("query.length", len(query))
        
        # Your LLM call is automatically traced as a child span
        response = generate_completion(query)
        
        span.set_attribute("response.length", len(response))
        return response
```

## Supported GenAI Libraries

| Library | Instrumentation Package | What's Traced |
|---------|------------------------|---------------|
| OpenAI | `opentelemetry-instrumentation-openai` | Chat completions, embeddings, fine-tuning |
| Anthropic | `opentelemetry-instrumentation-anthropic` | Messages API, Claude models |
| LangChain | `opentelemetry-instrumentation-langchain` | Chains, agents, tools, retrievers |
| OpenAI Agents | `opentelemetry-instrumentation-openai-agents` | Agent runs, function calls |

## Example: LangChain Application

```python
from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

from langchain.chat_models import ChatOpenAI
from langchain.chains import LLMChain
from langchain.prompts import PromptTemplate

llm = ChatOpenAI(model="gpt-4")
template = PromptTemplate(
    input_variables=["topic"],
    template="Tell me a joke about {topic}"
)
chain = LLMChain(llm=llm, prompt=template)

# This entire chain execution is traced
result = chain.run(topic="programming")
print(result)
```

## Example: OpenAI Agents

```python
from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

from openai import OpenAI
from openai_agents import Agent, function_tool

@function_tool
def get_weather(location: str) -> dict:
    """Get weather for a location."""
    return {"location": location, "temperature": 72}

agent = Agent(
    name="Weather Assistant",
    instructions="You are a helpful weather assistant.",
    tools=[get_weather],
    model="gpt-4"
)

# Agent runs and tool calls are automatically traced
response = agent.run("What's the weather in San Francisco?")
print(response)
```

## Configuration Options

### Disable Content Logging

To avoid logging prompt/completion content (for privacy):

```python
import os
os.environ["OTEL_PYTHON_DISABLED_INSTRUMENTATIONS"] = "openai-v2"

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()
```

### Control Sampling

To sample only 10% of traces (reduce costs):

```python
import os
os.environ["OTEL_TRACES_SAMPLER"] = "traceidratio"
os.environ["OTEL_TRACES_SAMPLER_ARG"] = "0.1"

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()
```

## Viewing Telemetry

Once configured, view your GenAI telemetry in Azure Portal:
1. Go to your Application Insights resource
2. Navigate to **Performance** → **Dependencies** to see LLM calls
3. Check **Transaction search** for individual requests
4. Use **Application Map** to visualize your application topology
5. Create custom dashboards to track token usage and costs

## Next Steps

- OpenTelemetry Pipeline Concepts(see in opentelemetry-pipeline-python.md)
- Azure Monitor Python Overview(see in azure-monitor-python.md)
