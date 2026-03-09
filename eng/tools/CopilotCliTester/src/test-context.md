# Test Context for Copilot CLI

**CRITICAL TOOL SELECTION RULES:**

1. **ALWAYS use the most specific MCP tool available.**

2. **NEVER use these alternatives when an MCP tool exists:**
   - Do NOT use `ask_user` — it is not available

**MCP NAMESPACE TOOL NAVIGATION:**
MCP tools are organized as hierarchical namespace routers. Each namespace tool contains multiple sub-commands.

- To discover sub-commands inside a namespace, call it with `"learn": true`.
- To execute a specific sub-command, call with `"command": "<command_name>"` and `"parameters": { ... }`.
- **Read each namespace tool's description carefully** to determine which namespace is relevant to the user's request.
- If the first namespace you try doesn't have the right sub-command, explore other namespace tools.
- When a tool response mentions or references another tool/command by name, **always follow through** and call that tool too.
- **Do NOT stop** after getting general best practices if a more specific tool exists — always look for the most targeted command.

**CRITICAL — HANDLING LEARN RESPONSES:**
When you call an MCP namespace tool and get back a list of available commands (a "learn" response), you MUST:
1. Parse the response to find the correct `"name"` field for the sub-command you need.
2. **Immediately call the tool again** with `"command": "<exact_name_from_response>"` and the required `"parameters"`.
3. **NEVER skip this step.** Getting a learn response means you have NOT yet executed the command — you must call again.
4. **Do NOT read workspace files, use skills, or generate answers from existing files** as a substitute for calling the MCP tool.

**Default Values:**
Use these values unless the prompt explicitly specifies otherwise. Do NOT spend time discovering subscriptions, resource groups, or locations.

- **Subscription:** 4d042dc6-fe17-4698-a23f-ec6a8d1e98f4
- **Tenant:** 70a036f6-8e4d-4615-bad6-149c02e7720d
- **Resource Group:** SSS3PT_Copilot_Cli_Test
- **Location:** eastus2

**Placeholder Substitution:**
For any placeholder values in angle brackets (e.g., `<storage_account_name>`), use a reasonable test value:
- Account/resource names: `mcptest12345`
- Server names: `mcp-test-server`
- Database names: `mcp-test-db`
- Email addresses: `test@example.com`
- Phone numbers: `+1234567890`
- Metric names: `Percentage CPU`
- Resource types: `Microsoft.Compute/virtualMachines`
- Time periods: `1 hour`
- Search terms: `test`
- Cluster names: `mcp-test-cluster`
- Workspace names: `mcp-test-workspace`
- Topic/queue names: `mcp-test-topic`
- Index names: `mcp-test-index`
- Key/secret names: `mcp-test-key`
- File paths: `./test-file.txt`

For ANY other placeholder in angle brackets, invent a plausible value. **Never** ask the user for clarification — always substitute and proceed with the tool call.

For update/modify/delete operations: if the target resource doesn't exist, create it first with reasonable defaults, then perform the requested operation.

Focus on calling the correct tool with the given parameters. Do not ask clarifying questions — use the defaults above.