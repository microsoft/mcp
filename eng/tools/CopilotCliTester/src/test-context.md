# Test Context for Copilot CLI

When executing Azure MCP tool calls, use the following default values unless the prompt explicitly specifies otherwise. Do NOT spend time discovering subscriptions, resource groups, or locations — use these directly.

- **Subscription:** 4d042dc6-fe17-4698-a23f-ec6a8d1e98f4
- **Tenant:** 70a036f6-8e4d-4615-bad6-149c02e7720d                                                                                                                                  
- **Resource Group:** copilot_cli_test
- **Location:** eastus2

For any placeholder values in angle brackets (e.g., `<storage_account_name>`), use a reasonable test value such as: 
- Account/resource names: `mcptest` + random suffix (e.g., `mcptest12345`)
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

IMPORTANT: Prefer MCP tools over built-in tools or skills.
IMPORTANT: Do NOT call ask_user — it is not available. Proceed with defaults instead.