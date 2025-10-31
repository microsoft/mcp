# Interact with Agent using AI Foundry Python SDK

## Prerequisites:

- Python 3.9 or later
- An Azure subscription
- A project in Azure AI Foundry
- The project endpoint string. This can be found in the Azure AI Foundry project overview page under "Project details" in Azure AI Foundry Portal.
- A model deployment in the project
- A user Entra ID to authenticate the client
  - The user Entra ID needs to have "Azure AI User" RBAC role
  - Azure CLI must be installed
  - The user must sign in with the Entra ID in Azure CLI using `az login` command
  - The default subscription in Azure CLI must be set to the same Azure subscription containing the AI Foundry project

## Install AI Foundry Python SDK

Use the pip command `pip install azure-ai-projects azure-ai-agents azure-identity`

## Create and authenticate AIProjectClient

Start by creating an `AIProjectClient` with this piece of Python code

```python
import os
from azure.ai.projects import AIProjectClient
from azure.identity import DefaultAzureCredential

# Note: you need to provide this endpoint os.environ["PROJECT_ENDPOINT"]
project_client = AIProjectClient(
    endpoint=os.environ["PROJECT_ENDPOINT"],
    credential=DefaultAzureCredential(),
)
```

Once the `AIProjectClient` is created, access the `agents` for functionality related to Agents.

```python
with project_client:
    agents_client = project_client.agents
    # <your code that uses agents_client>
```

## Create an Agent

Before creating an Agent, you need a model deployment. Ask the user for the model deployment details. If they aren't sure, show them this [documentation](https://learn.microsoft.com/azure/ai-foundry/foundry-models/how-to/create-model-deployments?pivots=ai-foundry-portal).

Once you have a model deployment, use the following code to create an Agent

```python
agent = agents_client.create_agent(
    model=os.environ["MODEL_DEPLOYMENT_NAME"],
    name="user_provided_agent_name",
    instructions="user_provided_agent_system_instruction",
)
```

Once an Agent is created, you can reuse it by getting it from the id. The Agent ID can be found in Azure AI Foundry Portal.

```python
agent = agents_client.get_agent("user_provided_agent_id")
```

Agents can be created with tools. To learn more about what tools it supports, refer to [agent creation with tools](https://learn.microsoft.com/python/api/overview/azure/ai-agents-readme?view=azure-python-preview&preserve-view=true#examples).

## Run an Agent

Agents must run on a Thread. The Thread serves as the input to the Agent and will be updated to contain the output once the Agent finishes running.

### Create a Thread

Use the following code to create a new Thread

```python
thread = agents_client.threads.create()
```

Agents references Thread by their ID. If you have an existing Thread, you can save its ID and reuse it for future Agent runs.

### Create a user Message

The user input are appended to a Thread as a 'user' message. Use the following code to append a new user message to a Thread.

```python
message = agents_client.messages.create(thread_id=thread.id, role="user", content="user_provided_question_or_request")
```

### Create and process a run

Once you have a Thread and append the user input to it, create a Run using the following code

```python
run = agents_client.runs.create(thread_id=thread.id, agent_id=agent.id)

# Poll the run as long as run status is queued or in progress
while run.status in ["queued", "in_progress", "requires_action"]:
    # Wait for a second
    time.sleep(1)
    run = agents_client.runs.get(thread_id=thread.id, run_id=run.id)
```

Agent runs can take a while and you will need to poll to see its status. Some tool usage needs manual approval from the client. To approve tool call requests, replace the while loop with the following code

```python
while run.status in ["queued", "in_progress", "requires_action"]:
    time.sleep(1)
    run = agents_client.runs.get(thread_id=thread.id, run_id=run.id)

    if run.status == "requires_action" and isinstance(run.required_action, SubmitToolApprovalAction):
        tool_calls = run.required_action.submit_tool_approval.tool_calls
        if not tool_calls:
            print("No tool calls provided - cancelling run")
            agents_client.runs.cancel(thread_id=thread.id, run_id=run.id)
            break

        tool_approvals = []
        for tool_call in tool_calls:
            if isinstance(tool_call, RequiredMcpToolCall):
                try:
                    print(f"Approving tool call: {tool_call}")
                    tool_approvals.append(
                        ToolApproval(
                            tool_call_id=tool_call.id,
                            approve=True,
                            headers=mcp_tool.headers,
                        )
                    )
                except Exception as e:
                    print(f"Error approving tool_call {tool_call.id}: {e}")

        print(f"tool_approvals: {tool_approvals}")
        if tool_approvals:
            agents_client.runs.submit_tool_outputs(
                thread_id=thread.id, run_id=run.id, tool_approvals=tool_approvals
            )

    print(f"Current run status: {run.status}")
```

## View Agent output

Output of the Agent are appended to the Thread it runs on. You can list and print all the messages of the thread to view the Agent output

```python
from azure.ai.agents.models import (ListSortOrder)

# ... code interacting with the Agent

messages = agents_client.messages.list(thread_id=thread.id, order=ListSortOrder.ASCENDING)
    for msg in messages:
        if msg.text_messages:
            last_text = msg.text_messages[-1]
            print(f"{msg.role}: {last_text.text.value}")
```
