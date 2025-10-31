# Interact with Agent using AI Foundry dotnet SDK

## Prerequisites

- LTS version of dotnet
- An Azure subscription
- A project in Azure AI Foundry
- The project endpoint string. This can be found in the Azure AI Foundry project overview page under "Project details" in Azure AI Foundry Portal.
- A model deployment in the project
- A user Entra ID to authenticate the client
  - The user Entra ID needs to have "Azure AI User" RBAC role
  - Azure CLI must be installed
  - The user must sign in with the Entra ID in Azure CLI using `az login` command
  - The default subscription in Azure CLI must be set to the same Azure subscription containing the AI Foundry project

## Install AI Foundry dotnet SDK

Use the dotnet command `dotnet add package Azure.AI.Agents.Persistent --prerelease`. You also need to install Azure.Identity package using this dotnet command `dotnet add package Azure.Identity`.

## Create and authenticate PersistentAgentsClient

Start by creating an `PersistentAgentsClient` with this piece of C# code

```C#
var projectEndpoint = Environment.GetEnvironmentVariable("PROJECT_ENDPOINT");
PersistentAgentsClient client = new(projectEndpoint, new DefaultAzureCredential());
```

## Create an Agent

Before creating an Agent, you need a model deployment. Ask the user for the model deployment details. If they aren't sure, show them this [documentation](https://learn.microsoft.com/azure/ai-foundry/foundry-models/how-to/create-model-deployments?pivots=ai-foundry-portal).

Once you have a model deployment, use the following code to create an Agent

```C#
PersistentAgent agent = await client.Administration.CreateAgentAsync(
    model: modelDeploymentName,
    name: "user_provided_agent_name",
    instructions: "user_provided_agent_system_instruction"
);
```

Once an Agent is created, you can reuse it by getting it from the id.
The Agent ID can be found in the Azure AI Foundry Portal.

```C#
PersistentAgent agent = await client.Administration.GetAgentAsync("user_provided_agent_id");
```

Agents can be created with tools. To learn more about what tools it supports, refer to [agent creation with tools](https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/ai/Azure.AI.Agents.Persistent#file-search).

## Run an Agent

Agents must run on a Thread. The Thread serves as the input to the Agent and will be updated to contain the output once the Agent finishes running.

### Create a Thread

Use the following code to create a new Thread

```C#
PersistentAgentThread thread = await client.Threads.CreateThreadAsync();
```

Agent references Thread by their ID. If you have an existing Thread, you can save its ID and reuse it for future Agent runs.

### Create a user Message

The user input are appended to a Thread as a 'user' message. Use the following code to append a new user message to a Thread.

```C#
PersistentThreadMessage message = await client.Messages.CreateMessageAsync(
    thread.Id,
    MessageRole.User,
    "user_provided_question_or_request");
```

### Create and process a run

Once you have a Thread and append the user input to it, create a Run using the following code

```C#
ThreadRun run = await client.Runs.CreateRunAsync(
    thread.Id,
    agent.Id,
    additionalInstructions: "user_provided_additional_instructions_for_this_run");
```

Agent runs can take a while and you will need to poll to see its status. Use the following code to poll the status of the run until it finishes.

```C#
do
{
    await Task.Delay(TimeSpan.FromMilliseconds(500));
    run = await client.Runs.GetRunAsync(thread.Id, run.Id);
}
while (run.Status == RunStatus.Queued
    || run.Status == RunStatus.InProgress);
Assert.AreEqual(
    RunStatus.Completed,
    run.Status,
    run.LastError?.Message);
```

Some tool usage needs manual approval from the client. To approve tool call requests, replace the polling code with the following code.

```C#
do
{
    await Task.Delay(TimeSpan.FromMilliseconds(500));
    run = await client.Runs.GetRunAsync(thread.Id, run.Id);

    if (run.Status == RunStatus.RequiresAction
        && run.RequiredAction is SubmitToolApprovalAction submitToolApprovalAction)
    {
        List<ToolApproval> toolApprovals = [];
        foreach (RequiredToolCall toolCall in submitToolApprovalAction.SubmitToolApproval.ToolCalls)
        {
            if (toolCall is RequiredMcpToolCall mcpToolCall)
            {
                Console.WriteLine($"Approving MCP tool call: {mcpToolCall.Name}");
                toolApprovals.Add(new ToolApproval(mcpToolCall.Id, approve: true));
            }
        }
        if (toolApprovals.Count > 0)
        {
            run = await client.Runs.SubmitToolOutputsToRunAsync(thread.Id, run.Id, toolApprovals: toolApprovals);
        }
    }
}
while (run.Status == RunStatus.Queued
    || run.Status == RunStatus.InProgress
    || run.Status == RunStatus.RequiresAction);
Assert.AreEqual(
    RunStatus.Completed,
    run.Status,
    run.LastError?.Message);
```

## View Agent output

Output of the Agent are appended to the Thread it runs on. You can list and print all the messages of the thread to view the Agent output.

```C#
AsyncPageable<PersistentThreadMessage> messages
    = client.Messages.GetMessagesAsync(
        threadId: thread.Id, order: ListSortOrder.Ascending);

await foreach (PersistentThreadMessage threadMessage in messages)
{
    Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
    foreach (MessageContent contentItem in threadMessage.ContentItems)
    {
        if (contentItem is MessageTextContent textItem)
        {
            Console.Write(textItem.Text);
        }
        else if (contentItem is MessageImageFileContent imageFileItem)
        {
            Console.Write($"<image from ID: {imageFileItem.FileId}");
        }
        Console.WriteLine();
    }
}
```
