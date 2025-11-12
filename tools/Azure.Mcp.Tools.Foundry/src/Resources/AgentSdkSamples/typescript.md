# Interact with Agent using AI Foundry TypeScript SDK

## Prerequisites:

- LTS version of Node.js
- Typescript is installed
- An Azure subscription
- A project in Azure AI Foundry
- The project endpoint string. This can be found in the Azure AI Foundry project overview page under "Project details" in Azure AI Foundry Portal.
- A model deployment in the project
- A user Entra ID to authenticate the client
  - The user Entra ID needs to have "Azure AI User" RBAC role
  - Azure CLI must be installed
  - The user must sign in with the Entra ID in Azure CLI using `az login` command
  - The default subscription in Azure CLI must be set to the same Azure subscription containing the AI Foundry project

## Install AI Foundry JavaScript SDK

Use the npm command `npm install @azure/ai-projects @azure/ai-agents @azure/identity`

## Create and authenticate AIProjectClient

Start by creating an `AIProjectClient` with this piece of Typescript code

```typescript
import { AIProjectClient } from "@azure/ai-projects";
import { DefaultAzureCredential } from "@azure/identity";

// Note: you need to provide this endpoint process.env["AZURE_AI_PROJECT_ENDPOINT_STRING"]
const endpoint = process.env["AZURE_AI_PROJECT_ENDPOINT_STRING"] || "<project endpoint string>";
const projectClient = new AIProjectClient(endpoint, new DefaultAzureCredential());
```

Once the `AIProjectClient` is created, access the `agents` for functionality related to Agents.

```typescript
const agentsClient = projectClient.agents
// <your code that uses agentsClient>
```

## Create an Agent

Before creating an Agent, you need a model deployment. Ask the user for the model deployment details. If they aren't sure, show them this [documentation](https://learn.microsoft.com/azure/ai-foundry/foundry-models/how-to/create-model-deployments?pivots=ai-foundry-portal).

Once you have a model deployment, use the following code to create an Agent

```typescript
// gpt-4o is the model deployment name which may vary
const agent = await agentsClient.createAgent("gpt-4o", {
  name: "user_provided_agent_name",
  instructions: "user_provided_agent_system_instruction",
});
```

Once an Agent is created, you can reuse it by getting it from the id. The Agent ID can be found in Azure AI Foundry Portal.

```typescript
const agent = agentsClient.getAgent("user_provided_agent_id")
```

Agents can be created with tools. To learn more about what tools it supports, refer to [agent creation with tools](https://learn.microsoft.com/javascript/api/overview/azure/ai-agents-readme?view=azure-node-latest#create-agent).

## Run an Agent

Agents must run on a Thread. The Thread serves as the input to the Agent and will be updated to contain the output once the Agent finishes running.

### Create a Thread

Use the following code to create a new Thread

```typescript
const thread = await agentsClient.threads.create();
console.log(`Created thread, thread ID: \${thread.id}`);
```

Agents references Thread by their ID. If you have an existing Thread, you can save its ID and reuse it for future Agent runs.

### Create a user Message

The user input are appended to a Thread as a 'user' message. Use the following code to append a new user message to a Thread.

```typescript
const message = await agentsClient.messages.create(thread.id, "user", "user_provided_question_or_request");
console.log(`Created message, message ID: \${message.id}`);
```

### Create and process a run

Once you have a Thread and append the user input to it, create a Run using the following code

```typescript
// Create and poll a run
console.log("Creating run...");
const run = await agentsClient.runs.createAndPoll(thread.id, agent.id, {
  pollingOptions: {
    intervalInMs: 2000,
  },
  onResponse: (response): void => {
    console.log(`Received response with status: \${response.parsedBody.status}`);
  },
});
console.log(`Run finished with status: \${run.status}`);
```

Agent runs can take a while and you will need to poll to see its status. Some tool usage needs manual approval from the client. To approve tool call requests, replace the onResponse callback with the following code

```typescript
async function onResponse(response: any): Promise<void> {
  const parsedBody =
    typeof response.parsedBody === "object" && response.parsedBody !== null
      ? response.parsedBody
      : null;

  if (!parsedBody || !("status" in parsedBody)) return;

  const run = parsedBody as ThreadRun;
  console.log(`Current Run status - \${run.status}, run ID: \${run.id}`);

  // Ensure we have a run with requires_action status and requiredAction object
  if (run.status === "requires_action" && run.requiredAction) {
    console.log("Run requires action");

    // Check if the requiredAction is of type submit_tool_outputs and has the expected structure
    if (isOutputOfType<SubmitToolOutputsAction>(run.requiredAction, "submit_tool_outputs")) {
      const submitToolOutputsActionOutput = run.requiredAction;
      const toolCalls = submitToolOutputsActionOutput.submitToolOutputs.toolCalls;
      const toolResponses: ToolOutput[] = [];

      for (const toolCall of toolCalls) {
        if (isOutputOfType<FunctionToolDefinition>(toolCall, "function")) {
          const toolResponse = functionToolExecutor.invokeTool(toolCall);
          if (toolResponse) {
            toolResponses.push(toolResponse);
          }
        }
      }
      if (toolResponses.length > 0) {
        try {
          await agentsClient.runs.submitToolOutputs(thread.id, run.id, toolResponses);
          console.log(`Submitted tool responses successfully`);
        } catch (err) {
          console.error("Error submitting tool outputs:", err);
        }
      }
    }
  }
}
```

## View Agent output

Output of the Agent are appended to the Thread it runs on. You can list and print all the messages of the thread to view the Agent output.

```typescript
const messagesIterator = agentsClient.messages.list(thread.id);
const allMessages = [];
for await (const m of messagesIterator) {
  allMessages.push(m);
}
console.log("Messages:", allMessages);

// The messages are following in the reverse order,
// we will iterate them and output only text contents.
const messages = await agentsClient.messages.list(thread.id, {
  order: "asc",
});

for await (const dataPoint of messages) {
  const textContent = dataPoint.content.find((item) => item.type === "text");
  if (textContent && "text" in textContent) {
    console.log(`\${dataPoint.role}: \${textContent.text.value}`);
  }
}
```
