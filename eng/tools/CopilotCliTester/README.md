# Copilot CLI Tester

E2E test runner for Azure MCP tools using GitHub Copilot SDK.

## Overview

Parses test prompts from `e2eTestPrompts.md`, executes them through a Copilot SDK agent session with Azure MCP tools, and validates that the expected tools are invoked correctly.

### How It Works

1. **PromptParser** reads the markdown file and extracts `{ namespace, tool, prompt }` tuples
2. **AgentRunner** creates a Copilot SDK session with Azure MCP server configured
3. Each prompt is sent to the agent, and tool invocation events are captured
4. **Early termination** aborts the session once the expected tool is called (saves time)
5. Results are written to markdown reports with pass/fail status

```
e2eTestPrompts.md
       │
       ▼
  PromptParser.cs  ──→  List<TestPrompt>
       │
       ▼
  AgentRunner.cs
       │
       ▼
  CopilotClient (GitHub.Copilot.SDK)
       │
       ├── mcpServers: { azure: npx -y @azure/mcp }
       └── session.SendAsync({ prompt })
              │
              ▼
         AgentSessionEvent[]
              │
              ├── tool.execution_start  ←  Does toolName match expected?
              └── session.idle
```

## Project Structure

```
CopilotCliTester/
├── src/
│   ├── Program.cs                # CLI entry point
│   ├── CopilotCliTester.csproj   # Project file (.NET 10, AOT-compatible)
│   ├── PromptParser.cs           # Parses e2eTestPrompts.md
│   ├── AgentRunner.cs            # Copilot SDK agent session management
│   ├── AgentRunnerUtils.cs       # Tool invocation detection utilities
│   ├── Models/
│   │   ├── TestPrompt.cs         # Test prompt record
│   │   ├── TestResult.cs         # Test result record + TestStatus enum
│   │   ├── AgentModels.cs        # Agent session config and event types
│   │   └── JsonContext.cs        # AOT-compatible JSON serialization
│   ├── test-context.md           # Runtime copy of test context
│   └── reports/                  # Generated test reports
└── README.md
```

## Prerequisites

### Required

1. **.NET 10 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
2. **GitHub Copilot subscription** — Required for Copilot SDK authentication
3. **GitHub Copilot CLI** — Authenticated (`gh copilot` or VS Code Copilot extension)
4. **Azure CLI** — Authenticated for MCP tool execution
   ```bash
   az login
   ```
5. **Node.js 20+** — Required to run Azure MCP via npx
   - [Download Node.js](https://nodejs.org/)
   - The tool uses `npx -y @azure/mcp server start` to invoke the Azure MCP server

## Usage

### Quick Start

```bash
# Navigate to src directory
cd eng/tools/CopilotCliTester/src

# Build
dotnet build

# Run tests for a specific namespace
dotnet run -- run --namespace storage --max 5
```

### CLI Options

```
Usage:
  CopilotCliTester run [options]

Options:
  --namespace <name>   Filter by namespace (partial match, e.g., "storage", "keyvault")
  --tool <name>        Filter by tool name (exact match, e.g., "subscription_list")
  --max <n>            Maximum number of prompts to test (0 = all)
  --retries <n>        Maximum retry attempts per prompt (default: 3)
  --one-per-tool       Test only one prompt per tool
  --output <dir>       Output directory for reports (default: reports)
  --model <name>       LLM model to use (default: claude-sonnet-4.5)
```

### Examples

```bash
# From the src directory:
cd eng/tools/CopilotCliTester/src

# Test all Redis prompts
dotnet run -- run --namespace redis

# Test first 3 storage prompts
dotnet run -- run --namespace storage --max 3

# Test a specific tool
dotnet run -- run --tool subscription_list

# One prompt per tool (quick validation)
dotnet run -- run --one-per-tool --max 20

# Use a different model
dotnet run -- run --namespace keyvault --model gpt-4o
```

## Test Context Configuration

Edit `config/test-context.md` (or `src/test-context.md`) with your Azure environment defaults. This context is prepended to every prompt to prevent the agent from wasting time discovering resources.

```markdown
# Test Context for Copilot CLI

- **Subscription:** your-subscription-id
- **Tenant:** your-tenant-id
- **Resource Group:** your-test-resource-group
- **Location:** eastus2
```

The context also instructs the agent to substitute placeholder values (e.g., `<storage_account_name>`) with reasonable test values instead of asking for clarification.

## Output

### Reports

Reports are written to `src/reports/` by default:

```
reports/
├── test-run-{timestamp}/
│   ├── {namespace}/
│   │   ├── {tool}-{time}.md    # Individual test transcript
│   │   └── ...
│   └── ...
└── e2e-report-{namespace}-{timestamp}.md   # Summary report
```

### Test Status

| Status | Description |
|--------|-------------|
| ✅ **PASS** | Expected tool was invoked |
| ❌ **FAIL** | Expected tool was not invoked (after all retry attempts) |

### Console Output

```
--------------------------------------------
Azure MCP E2E Test Runner (Copilot SDK)
--------------------------------------------

SUCCESS: Loaded test context
SUCCESS: Loading prompts from: .../e2eTestPrompts.md
  Found 620 total prompts
  → Filtered to namespace "redis": 8 prompts

Testing 8 prompts across 1 namespaces
Retries: 3, Model: claude-sonnet-4.5
--------------------------------------------------------------------------------

Live report: reports\e2e-report-redis-20260227-123456.md

------ redis (8 prompts) -----
  [redis_cache_get] - Show me ...  ✓ PASS [12.3s]
  [redis_cache_list] - List ...    ✓ PASS [8.7s]
  [redis_cache_keys] - Get ...     RETRYING (attempt 2)...  ✓ PASS (attempt 2) [45.2s]

════════════════════════════════════════════════════════════════
SUMMARY
  Total:  8
  Passed: 8 (100.0%)
  Failed: 0
  Time:   2m 15s
════════════════════════════════════════════════════════════════
```

## Architecture

### Key Components

| File | Purpose |
|------|---------|
| `Program.cs` | CLI argument parsing, test orchestration, report generation |
| `PromptParser.cs` | Parses `e2eTestPrompts.md` markdown tables into `TestPrompt` records |
| `AgentRunner.cs` | Manages Copilot SDK client and session lifecycle |
| `AgentRunnerUtils.cs` | Tool invocation detection and event processing |

### Models

| Type | Description |
|------|-------------|
| `TestPrompt` | Parsed prompt with namespace, tool name, and prompt text |
| `TestResult` | Test execution result with status, duration, attempts, and tool calls |
| `AgentRunConfig` | Configuration for a single agent session |
| `AgentMetadata` | Collected events from an agent session |
| `AgentSessionEvent` | Normalized event from Copilot SDK (tool calls, messages, errors) |

### Early Termination

Once the expected tool is invoked, the agent session is terminated early to save time. This is controlled by the `ShouldEarlyTerminate` callback in `AgentRunConfig`.

### Retry Logic

Prompts are retried up to `--retries` times (default: 3) to handle LLM's non-determinism. Each retry creates a fresh agent session.

## Namespace Examples

The `--namespace` option filters prompts by their section in `e2eTestPrompts.md`:

| Namespace | Description |
|-----------|-------------|
| `storage` | Azure Storage (blobs, queues, tables, file shares) |
| `keyvault` | Key Vault (secrets, keys, certificates) |
| `redis` | Azure Cache for Redis |
| `sql` | Azure SQL Database |
| `cosmos` | Cosmos DB |
| `appservice` | App Service and Function Apps |
| `monitor` | Azure Monitor (metrics, logs, alerts) |
| `extension` | Azure CLI extensions |

## Troubleshooting

### "Copilot SDK authentication failed"

Ensure you're authenticated with GitHub Copilot:
- VS Code: Sign in to GitHub Copilot extension
- CLI: Run `gh auth login` and ensure Copilot access


### "Test context not found"

Copy `config/test-context.md` to `src/test-context.md` or run from the `src/` directory.

### Tool not invoked (false negatives)

- Increase `--retries` for flaky prompts
- Check if the prompt is ambiguous — the agent might call a different tool
- Review the generated transcript in `reports/` for debugging
