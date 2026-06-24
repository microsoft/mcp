# Onboarding Guide for Azure MCP

Welcome to the Azure MCP project! This guide will help you set up your development environment, understand the codebase, and contribute your first command.

> **New to MCP?** Start with this guide, then use the onboarding agent by invoking `@onboarding` in GitHub Copilot Chat for interactive help.

## Prerequisites

| Tool | Notes |
|------|-------|
| [VS Code](https://code.visualstudio.com/download) or [Insiders](https://code.visualstudio.com/insiders) | Recommended editor. Insiders required for some agent-mode features. |
| [GitHub Copilot](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) + [Copilot Chat](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot-chat) | Used for command scaffolding via skills. |
| [Node.js 20+](https://nodejs.org/en/download) | Ensure `node` and `npm` are on PATH. |
| [PowerShell 7.0+](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) | Required for build/test scripts in `eng/scripts`. |
| .NET SDK | Version pinned in `global.json`. |

For **live tests** against real Azure resources you also need:

| Tool | Notes |
|------|-------|
| [Azure PowerShell](https://learn.microsoft.com/powershell/azure/install-azure-powershell) | `Connect-AzAccount` for live test deployments. |
| [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) | `az login` for authentication. |
| [Azure Bicep](https://learn.microsoft.com/azure/azure-resource-manager/bicep/install) | Builds `test-resources.bicep` templates. |

### NuGet Feed

This repo uses a single Azure DevOps package feed (configured in `nuget.config`) with an upstream to nuget.org. **External contributors** cannot authenticate as a feed collaborator; if you add a package that is not already cached, temporarily add nuget.org as an extra source locally and revert before submitting your PR. See [CONTRIBUTING.md](../CONTRIBUTING.md) → "Central NuGet Feed" for details.

## Quick Start

```powershell
# 1. Fork microsoft/mcp, then clone your fork
git clone https://github.com/<your-username>/mcp.git
cd mcp

# 2. Build the solution
dotnet build

# 3. Verify everything works (build + npx package smoke test)
./eng/scripts/Build-Local.ps1 -VerifyNpx

# 4. Run unit tests for a specific toolset
./eng/scripts/Test-Code.ps1 -Paths Storage

# 5. Run all unit tests
./eng/scripts/Test-Code.ps1
```

## Project Structure

```
├── servers/Azure.Mcp.Server/        # Main MCP server application
├── tools/Azure.Mcp.Tools.{Service}/ # Individual toolset implementations
├── core/                            # Shared libraries (commands, auth, protocol)
├── eng/                             # Build pipelines, scripts, testing infrastructure
├── docs/                            # Documentation and onboarding materials
└── .github/agents/                  # Copilot agent definitions
```

Each toolset follows this layout:

```
Azure.Mcp.Tools.{Service}/
├── src/
│   ├── Commands/{Resource}/       # {Resource}{Operation}Command pattern
│   ├── Services/                  # Service implementations
│   ├── Options/                   # Option classes with [Option] attributes
│   ├── Models/                    # Data models
│   └── {Service}Setup.cs          # DI registration
└── tests/
    └── Azure.Mcp.Tools.{Service}.Tests/
        ├── test-resources.bicep       # Test infrastructure (Azure commands only)
        └── test-resources-post.ps1    # Post-deployment (Azure commands only)
```

## Adding a New Command

Commands follow the pattern: `azmcp <service> <resource> <operation>`

### Using the Copilot Skill (Recommended)

The fastest way to add a new command is with the Copilot skill:

```
/skills add-azure-mcp-tools "add [namespace] [resource] [operation] command"
```

This provides the complete phased workflow: scaffolding → implementation → testing → documentation → PR checklist.

### Manual Steps

1. **Create an issue** titled: "Add command: azmcp [namespace] [resource] [operation]"

2. **Create the command class** — inherit from `SubscriptionCommand<TOptions, TResult>` (Azure commands) or `BaseCommand<TOptions, TResult>` (non-Azure):

   ```csharp
   [Description("Brief tool description for AI agents")]
   internal sealed class StorageAccountGetCommand(IStorageService service)
       : SubscriptionCommand<StorageAccountGetOptions, StorageAccountGetResult>
   {
       protected override async Task<StorageAccountGetResult> ExecuteAsync(
           CommandContext context,
           StorageAccountGetOptions options,
           CancellationToken cancellationToken)
       {
           // Implementation
       }
   }
   ```

3. **Create the options class** using `[Option]` attributes:

   ```csharp
   internal sealed class StorageAccountGetOptions : SubscriptionOptions
   {
       [Option("--account-name", Description = "Name of the storage account")]
       public string AccountName { get; set; } = string.Empty;
   }
   ```

4. **Register the command** in `{Toolset}Setup.cs` → `RegisterCommands()`

5. **Register the project** in solution files:
   ```powershell
   eng/scripts/Update-Solutions.ps1 -All
   ```

6. **Write unit tests** — extend `SubscriptionCommandUnitTestsBase<TCommand, TService>` for subscription commands or `CommandUnitTestsBase<TCommand, TService>` for non-subscription commands.

7. **Update documentation**:
   - Add command to `servers/Azure.Mcp.Server/docs/azmcp-commands.md`
   - Run `.\eng\scripts\Update-AzCommandsMetadata.ps1`
   - Add test prompts to `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`

8. **Create a changelog entry**:
   ```powershell
   ./eng/scripts/New-ChangelogEntry.ps1 -ChangelogPath "servers/Azure.Mcp.Server/CHANGELOG.md" -Description "<description>" -Section "<section>" -PR <pr-number>
   ```

9. **Add CODEOWNERS entry** in `.github/CODEOWNERS`

10. **Add to consolidated mode** — update `servers/Azure.Mcp.Server/src/Resources/consolidated-tools.json`

## Adding a New Namespace (Toolset)

A **namespace** is a top-level command group (e.g., `storage`, `keyvault`, `sql`).

1. **Create the toolset project** following the standard layout above
2. **Implement `{Toolset}Setup.cs`** as an `IAreaSetup` — exposes `Name` (lowercase, no dashes), `Title`, registers services in `ConfigureServices`, builds command tree in `RegisterCommands`
3. **Register in `Program.cs`** `RegisterAreas()` — keep alphabetically sorted
4. **Add to solution files**: `eng/scripts/Update-Solutions.ps1 -All`
5. **Verify AOT compatibility**: `./eng/scripts/Build-Local.ps1 -BuildNative`

## Integrating External MCP Servers

The Azure MCP Server can proxy tools from external MCP servers into a single interface. External servers are declared in `servers/Azure.Mcp.Server/src/Resources/registry.json`.

1. **Edit `registry.json`** — add an entry under `servers`
2. **Choose a transport**: HTTP/SSE (provide `url`) or stdio (set `"type": "stdio"` with `command`)
3. **Include a descriptive `description`** — surfaced to agents as the namespace tool description
4. **Rebuild** the project to embed the updated registry

See [CONTRIBUTING.md](../CONTRIBUTING.md) → "Configuring External MCP Servers" for full details.

## Testing

### Unit Tests (Required for all commands)

```powershell
# Run all unit tests
./eng/scripts/Test-Code.ps1

# Run tests for specific toolsets
./eng/scripts/Test-Code.ps1 -Paths Storage, KeyVault

# Run a specific test class
dotnet test --filter "FullyQualifiedName~StorageAccountGetCommandTests"
```

### Live Tests (Azure service commands only)

```powershell
# Deploy test resources
eng/common/TestResources/New-TestResources.ps1 `
  -TestResourcesDirectory tools/Azure.Mcp.Tools.{Toolset}

# Run live tests
./eng/scripts/Test-Code.ps1 -TestType Live -Paths {Toolset}
```

Azure resource commands **require recorded live tests**. See [recorded-tests.md](recorded-tests.md) for the record/playback workflow.

### Testing Your Local Build

Point your `mcp.json` at the freshly built binary:

```json
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio",
      "command": "<repo>/servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp[.exe]",
      "args": ["server", "start"]
    }
  }
}
```

### Server Start Modes

| Mode | Args | Description |
|------|------|-------------|
| Default | (none) | Collapses tools by namespace |
| Namespace filter | `--namespace storage --namespace keyvault` | Expose specific services only |
| Namespace proxy | `--mode namespace` | Group each namespace behind a single proxy tool |
| Single tool | `--mode single` | One `azure` tool that routes internally |
| All tools | `--mode all` | Expose all 800+ individual tools |

## Quality Checklist Before Submitting a PR

- [ ] `dotnet build` — passes
- [ ] `dotnet format` — code is formatted
- [ ] `.\eng\common\spelling\Invoke-Cspell.ps1` — no spelling errors
- [ ] `./eng/scripts/Test-Code.ps1` — unit tests pass
- [ ] `.\eng\scripts\Update-AzCommandsMetadata.ps1` — metadata up-to-date
- [ ] Tool descriptions validated with `ToolDescriptionEvaluator` (score ≥ 0.4)
- [ ] Live tests recorded and passing in playback (Azure commands)
- [ ] AOT check for new toolsets: `./eng/scripts/Build-Local.ps1 -BuildNative`
- [ ] Changelog entry created if applicable
- [ ] CODEOWNERS entry added for new toolsets
- [ ] One tool per PR

## Coding Standards

Refer to [`AGENTS.md`](../AGENTS.md) as the authoritative source of coding conventions — it covers all Do/Don't rules, naming conventions, and architectural patterns.

Key highlights:
- Use `[Option]` attributes on flat POCO option classes (not legacy `OptionDefinitions`)
- Commands inherit `SubscriptionCommand<TOptions, TResult>` or `BaseCommand<TOptions, TResult>`
- Make command classes **sealed**, use **primary constructors**
- Use **`System.Text.Json`** (never Newtonsoft)
- Use `subscription` (never `subscriptionId`), `resourceGroup` (never `resourceGroupName`)
- Always call `HandleException(context, ex)` in catch blocks

## Getting Help

- [Open an issue](https://github.com/microsoft/mcp/issues/new/choose) for bugs or questions
- Invoke `@onboarding` in Copilot Chat for interactive onboarding help
- Invoke `/skills add-azure-mcp-tools` for detailed implementation guidance
- Check [`AGENTS.md`](../AGENTS.md) for coding conventions
- See [CONTRIBUTING.md](../CONTRIBUTING.md) for the full contribution workflow
- See [recorded-tests.md](recorded-tests.md) for live test record/playback
- Review the [Code of Conduct](https://opensource.microsoft.com/codeofconduct/)
