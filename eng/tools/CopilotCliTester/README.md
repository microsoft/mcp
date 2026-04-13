# Copilot CLI Tester

E2E test runner for Azure MCP tools using GitHub Copilot SDK.

## Overview

Parses test prompts from `e2eTestPrompts.md`, executes them through a Copilot SDK agent session with Azure MCP tools, and validates that the expected tools are invoked correctly.

### How It Works

1. **PromptParser** reads the markdown file and extracts `{ section, tool, prompt, namespace }` tuples
2. **AgentRunner** creates a `CopilotClient` session with the Azure MCP server configured as a stdio MCP server
3. Each prompt is sent to the agent, and tool invocation events are captured via `session.On` event handler
4. **Early termination** aborts the session once the expected tool is called (saves time and cost)
5. Results are written to both markdown reports and JSON result files

```
e2eTestPrompts.md  (from servers/Azure.Mcp.Server/docs/)
       │
       ▼
  PromptParser.cs  ──→  List<TestPrompt>
       │
       ▼
  Program.cs  (orchestrator, parallel execution, report generation)
       │
       ▼
  AgentRunner.cs
       │
       ▼
  CopilotClient (GitHub.Copilot.SDK)
       │
       ├── mcpServers: { azure: <local azmcp executable> server start }
       └── session.SendAsync({ prompt })
              │
              ▼
         session.On(event handler)
              │
              ├── tool.execution_start  ←  Does toolName match expected?
              ├── tool.execution_complete
              ├── assistant.message / assistant.reasoning
              ├── session.error
              └── session.idle
```

## Project Structure

```
CopilotCliTester/
├── README.md
└── src/
    ├── Program.cs                # CLI entry point, orchestration, report generation
    ├── CopilotCliTester.csproj   # Project file (.NET 10, AOT-compatible)
    ├── PromptParser.cs           # Parses e2eTestPrompts.md markdown tables
    ├── AgentRunner.cs            # Copilot SDK client/session lifecycle, event mapping
    ├── AgentRunnerUtils.cs       # Tool invocation detection and matching utilities
    ├── Models/
    │   ├── TestPrompt.cs         # Test prompt record (Section, Tool, Prompt, Namespace)
    │   ├── TestResult.cs         # Test result record + TestStatus enum (PASS/FAIL/ERROR)
    │   ├── AgentModels.cs        # AgentRunConfig, AgentMetadata, AgentSessionEvent, SystemPromptConfig
    │   └── JsonContext.cs        # AOT-compatible JSON serialization context
    ├── test-context.md           # Default Azure test context (subscription, resource group, etc.)
    └── reports/                  # Generated test reports (markdown + JSON)
```

## Prerequisites

1. **.NET 10 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
2. **GitHub Copilot subscription** — Required for Copilot SDK authentication
3. **GitHub Copilot CLI** — Authenticated (`gh auth login` or VS Code Copilot extension sign-in)
4. **Azure CLI** — Authenticated for MCP tool execution
   ```bash
   az login
   ```
5. **Local azmcp build** — The tool discovers or builds the local `azmcp` executable from the repo (`servers/Azure.Mcp.Server/`). No external npm/npx dependency is needed.

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
  --namespace <name>      Filter by namespace (partial match, e.g., "storage", "keyvault")
  --tool <name>           Filter by tool name (exact match, e.g., "subscription_list")
  --max <n>               Maximum number of prompts to test (0 = all)
  --retries <n>           Maximum retry attempts per prompt (default: 3)
  --one-per-tool          Test only one prompt per tool
  --output <dir>          Output directory for reports (default: reports)
  --model <name>          LLM model to use (default: claude-opus-4.6)
  --parallel <n>          Number of prompts to test concurrently (default: 4)
  --prompts-file <path>   Custom prompts file path (format needs to match the e2eprompts.md format for successful parsing)
  --list-namespaces       List all available namespace values from the prompts file and exit
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

# Run 5 prompts concurrently
dotnet run -- run --namespace storage --parallel 5

# Use a different model
dotnet run -- run --namespace keyvault --model gpt-4o

# Use a custom prompts file
dotnet run -- run --prompts-file ./my-prompts.md

# List all available namespaces (useful for finding valid --namespace values)
dotnet run -- run --list-namespaces
```

## Test Context Configuration

Edit `src/test-context.md` with your Azure environment defaults. This context is appended to every prompt as a system message to prevent the agent from wasting time discovering resources.

The default test context includes:

- **Default Azure values** — subscription, tenant, resource group, and location
- **Tool selection rules** — forces the agent to use MCP tools instead of PowerShell or built-in skills
- **Placeholder substitution** — maps common placeholders (e.g., `<storage_account_name>`) to reasonable test values so the agent never asks for clarification

```markdown
# Test Context for Copilot CLI

- **Subscription:** your-subscription-id
- **Tenant:** your-tenant-id
- **Resource Group:** your-test-resource-group
- **Location:** eastus2
```

## Output

### Reports

Reports are written to `src/reports/` by default:

```
reports/
├── test-run-{timestamp}/
│   ├── {namespace}/
│   │   ├── {tool}-{time}.md           # Individual test transcript (prompt, tool calls, responses)
│   │   └── ...
│   └── ...
├── e2e-report-{namespace}-{timestamp}.md   # Summary report (markdown table)
└── e2e-results-{namespace}-{timestamp}.json # Machine-readable results (JSON array)
```

Individual transcripts include the user prompt, assistant messages/reasoning, tool calls with arguments and responses, and any session errors. Secrets (JWTs, bearer tokens, API keys) are automatically redacted.

### Test Status

| Status | Description |
|--------|-------------|
| ✅ **PASS** | Expected tool was invoked |
| ❌ **FAIL** | Expected tool was not invoked after all retry attempts |
| ⚠️ **ERROR** | Agent session threw an exception on all attempts |

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
Retries: 3, Model: claude-opus-4.6
--------------------------------------------------------------------------------

Report: reports/e2e-report-redis-20260227-123456.md
Parallel workers: 1

  [redis_cache_get] Show me ...      ✓ PASS [12.3s]
  [redis_cache_list] List ...        ✓ PASS [8.7s]
  [redis_cache_keys] Get ...         RETRY (attempt 2)...  ✓ PASS (attempt 2) [45.2s]

════════════════════════════════════════════════════════════════
SUMMARY
────────────────────────────────────────────────────────────────
  Total:     8
  Passed:    8
  Failed:    0
  Skipped:   0
  Pass Rate: 100.0%
  Duration:  135.0s
════════════════════════════════════════════════════════════════

✓ Final results: reports/e2e-results-redis-20260227-123456.json
✓ Report finalized: reports/e2e-report-redis-20260227-123456.md
```

## Architecture

### Key Components

| File | Purpose |
|------|---------|
| `Program.cs` | CLI argument parsing, parallel test orchestration, markdown/JSON report generation |
| `PromptParser.cs` | Parses `e2eTestPrompts.md` markdown tables into `TestPrompt` records. Extracts namespace from tool name prefix. |
| `AgentRunner.cs` | Creates `CopilotClient` and session, maps SDK events to `AgentSessionEvent`, writes per-test markdown transcripts with secret redaction |
| `AgentRunnerUtils.cs` | Tool invocation detection: matches by exact name, namespace-prefixed name, `mcpToolName`, or `command` argument |

### Models

| Type | Description |
|------|-------------|
| `TestPrompt` | Parsed prompt: `Section`, `Tool`, `Prompt`, `Namespace` |
| `TestResult` | Execution result: `Tool`, `Prompt`, `Duration`, `ToolsCalled`, `Attempts`, `Status`, `Error` |
| `TestStatus` | Enum: `PASS`, `FAIL`, `ERROR` |
| `AgentRunConfig` | Session config: `Prompt`, `ToolName`, `Namespace`, `ShouldEarlyTerminate`, `SystemPrompt`, `Debug`, `Model` |
| `AgentMetadata` | Collected `AgentSessionEvent` list from a session |
| `AgentSessionEvent` | Normalized event with `Type` and `Data` dictionary. Types: `session.idle`, `session.error`, `tool.execution_start`, `tool.execution_complete`, `assistant.message`, `assistant.message_delta`, `assistant.reasoning`, `assistant.reasoning_delta` |
| `SystemPromptConfig` | System prompt with `Mode` (`Append`/`Replace`) and `Content` |
| `JsonContext` | AOT-compatible `JsonSerializerContext` for `TestPrompt`, `TestResult`, and supporting types |

### Tool Matching

`AgentRunnerUtils.WasToolInvoked` checks multiple matching strategies to handle different MCP server modes:

1. **Exact match** on `mcpToolName` (from SDK event data)
2. **Exact match** on `toolName`
3. **Namespace-prefixed match** — e.g., `azure_storage_account_list` matches `storage_account_list`
4. **Command argument match** — checks if `arguments.command` equals the expected tool name (for namespace/single modes)

Internal tools like `report_intent` are excluded from matching.

### Early Termination

Once the expected tool is invoked, `session.AbortAsync()` is called to terminate the session early. This is controlled by the `ShouldEarlyTerminate` callback in `AgentRunConfig`, reducing both time and token cost.

### Retry Logic

Prompts are retried up to `--retries` times (default: 3) to handle LLM non-determinism. Each retry creates a fresh agent session. Individual attempts have a 5-minute timeout.

### Parallelism

The `--parallel` flag controls concurrent test execution using a `SemaphoreSlim`. Each parallel worker gets its own `AgentRunner` instance. Console and report file writes are thread-safe via locks.

## Namespace Examples

The `--namespace` option filters prompts by the namespace derived from tool name prefixes (e.g., `storage_account_list` → `storage`):

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
- VS Code: Sign in to the GitHub Copilot extension
- CLI: Run `gh auth login` and ensure Copilot access

### "Prompts file not found"

The tool looks for `e2eTestPrompts.md` at `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md` relative to the repo root. Run from the `src/` directory so the repo root is auto-detected, or pass `--prompts-file` with an explicit path.

### Tool not invoked (false negatives)

- Increase `--retries` for flaky prompts
- Check if the prompt is ambiguous — the agent might call a different tool
- Review the generated transcript in `reports/test-run-*/` for debugging
- Set the `DEBUG` environment variable to see raw SDK event types in console output
