<!-- Copyright (c) Microsoft Corporation. -->
<!-- Licensed under the MIT License. -->

# Onboarding Guide: Azure MCP Server

This guide is the entry point for new contributors to the **Azure MCP Server**. It walks
you through the prerequisites and the most common onboarding tasks:

1. [Prerequisites](#1-prerequisites)
2. [Quick start](#2-quick-start)
3. [Add a new namespace (toolset) to Azure MCP Server](#3-add-a-new-namespace-toolset)
4. [Add a new tool to an existing namespace](#4-add-a-new-tool-to-an-existing-namespace)
5. [Integrate an external MCP server](#5-integrate-an-external-mcp-server)
6. [Pull request checklist](#6-pull-request-checklist)

> [!TIP]
> Prefer interactive help? Invoke the [`@onboarding` agent](https://github.com/microsoft/mcp/blob/main/.github/agents/onboarding.agent.md)
> in GitHub Copilot Chat. It routes you to the right section of this guide and the deeper docs.

This document links to the authoritative deep-dive docs rather than duplicating them. The
two most important references are:

- [`CONTRIBUTING.md`](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md) — full contribution workflow and standards
- [`servers/Azure.Mcp.Server/docs/new-command.md`](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md) — the authoritative guide for implementing commands

---

## 1. Prerequisites

> [!IMPORTANT]
> If you are a **Microsoft employee**, also review the
> [Azure Internal Onboarding Documentation](https://aka.ms/azmcp/intake) for setup.

Install the following tooling before you build:

| Tool | Notes |
| --- | --- |
| [VS Code](https://code.visualstudio.com/download) or [Insiders](https://code.visualstudio.com/insiders) | Recommended editor. Insiders is required for some agent-mode features. |
| [GitHub Copilot](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) + [Copilot Chat](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot-chat) | Used heavily for command scaffolding. |
| [Node.js](https://nodejs.org/en/download) 20 or later | Ensure `node` and `npm` are on your `PATH`. |
| [PowerShell](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) 7.0 or later | Required for build and test scripts in `eng/scripts`. |
| .NET SDK | Version is pinned in [`global.json`](https://github.com/microsoft/mcp/blob/main/global.json). |

For **live tests** against real Azure resources you also need:

| Tool | Notes |
| --- | --- |
| [Azure PowerShell](https://learn.microsoft.com/powershell/azure/install-azure-powershell) | `Connect-AzAccount` for live test deployments. |
| [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) | `az login` for authentication. |
| [Azure Bicep](https://learn.microsoft.com/azure/azure-resource-manager/bicep/install) | Builds `test-resources.bicep` templates. |

### NuGet feed

This repo uses a single Azure DevOps package feed (configured in
[`nuget.config`](https://github.com/microsoft/mcp/blob/main/nuget.config)) with an upstream to nuget.org. **External contributors**
cannot authenticate as a feed collaborator; if you add a package that is not already cached,
temporarily add nuget.org as an extra source locally and revert before submitting your PR.
See [CONTRIBUTING.md → Central NuGet Feed](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md#central-nuget-feed) for details.

---

## 2. Quick start

```powershell
# 1. Fork microsoft/mcp, then clone your fork
git clone https://github.com/<your-username>/mcp.git
cd mcp

# 2. Build the solution
dotnet build

# 3. Verify everything works (build + npx package smoke test)
./eng/scripts/Build-Local.ps1 -UsePaths -VerifyNpx

# 4. Run unit tests for a specific toolset
./eng/scripts/Test-Code.ps1 -Paths Storage
```

### Development workflow

1. **Fork** `microsoft/mcp` to your account (for example `RickWinter/mcp`).
2. **Create a feature branch** off `main`.
3. **Make your changes** (and write/update tests).
4. **Test locally** with the scripts above.
5. **Open a pull request** from `<your-fork>:<branch>` into `microsoft/mcp:main`.

> [!TIP]
> **Submit one tool per pull request.** Smaller PRs review faster and iterate more easily.
> See [CONTRIBUTING.md → Adding a New Command](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md#adding-a-new-command).

### Run the server locally

Point your `mcp.json` at the freshly built binary:

```json
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio",
      "command": "<repo>/servers/Azure.Mcp.Server/bin/Debug/net10.0/azmcp.exe",
      "args": ["server", "start"]
    }
  }
}
```

Useful `server start` flags:

- `--namespace storage --namespace keyvault` — expose only specific namespaces.
- `--mode namespace` — group each namespace's tools behind a single proxy tool (helps with
  VS Code's tool-count limit).
- `--mode single` — expose one `azure` tool that routes internally.

---

## 3. Add a new namespace (toolset)

A **namespace** is a top-level command group (for example `storage`, `keyvault`, `sql`),
implemented as a **toolset** project under `tools/Azure.Mcp.Tools.{Toolset}`. Each toolset
provides an `IAreaSetup` implementation that registers its services and commands.

> The authoritative, end-to-end guide is
> [`new-command.md`](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md). The steps below are the
> onboarding-level summary of what it takes to stand up a brand-new namespace.

### Steps

1. **Create the toolset project** following the standard layout:

   ```text
   tools/Azure.Mcp.Tools.{Toolset}/
   ├── src/
   │   ├── Commands/{Resource}/        # Command implementations
   │   ├── Services/                   # Service interface + implementation
   │   ├── Options/                    # Option definitions and per-command options
   │   ├── Models/                     # DTOs / response models
   │   ├── {Toolset}JsonContext.cs     # AOT-safe JSON serialization context
   │   └── {Toolset}Setup.cs           # IAreaSetup implementation
   └── tests/
       ├── Azure.Mcp.Tools.{Toolset}.Tests/   # Unit + integration tests
       ├── test-resources.bicep               # Live test infra (Azure services only)
       └── test-resources-post.ps1            # Post-deployment script (Azure services only)
   ```

2. **Add required packages** to [`Directory.Packages.props`](https://github.com/microsoft/mcp/blob/main/Directory.Packages.props) first
   (central package management), then reference them from the toolset `.csproj`.

3. **Implement `{Toolset}Setup.cs`** as an `IAreaSetup`. It must expose `Name` (the namespace,
   lowercase, no dashes), `Title`, register services in `ConfigureServices`, and build the
   command tree in `RegisterCommands`. Use
   [`StorageSetup.cs`](https://github.com/microsoft/mcp/blob/main/tools/Azure.Mcp.Tools.Storage/src/StorageSetup.cs) as the reference:

   ```csharp
   public class WidgetSetup : IAreaSetup
   {
       public string Name => "widget";
       public string Title => "Manage Azure Widgets";

       public void ConfigureServices(IServiceCollection services)
       {
           services.AddSingleton<IWidgetService, WidgetService>();
           services.AddSingleton<WidgetListCommand>();
       }

       public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
       {
           var widget = new CommandGroup(Name, "Widget operations - ...", Title);
           widget.AddCommand<WidgetListCommand>(serviceProvider);
           return widget;
       }
   }
   ```

4. **Register the toolset** in
   [`Program.cs`](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/src/Program.cs) `RegisterAreas()`, keeping the
   list **alphabetically sorted** (excluding the fixed `#if !BUILD_NATIVE` block):

   ```csharp
   new Azure.Mcp.Tools.Widget.WidgetSetup(),
   ```

5. **Add the new projects** to the solution files
   [`Microsoft.Mcp.slnx`](https://github.com/microsoft/mcp/blob/main/Microsoft.Mcp.slnx) and
   [`Azure.Mcp.Server.slnx`](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/Azure.Mcp.Server.slnx).

6. **Implement at least one command** in the namespace — follow
   [Add a new tool](#4-add-a-new-tool-to-an-existing-namespace).

7. **Verify AOT compatibility** (required for new toolsets):

   ```powershell
   ./eng/scripts/Build-Local.ps1 -BuildNative
   ```

   If AOT fails, follow [`aot-compatibility.md`](https://github.com/microsoft/mcp/blob/main/docs/aot-compatibility.md) to either fix it or
   exclude the toolset from the native build — do **not** edit the fixed exclusion block.

8. **Add a CODEOWNERS entry** and update docs (see the [PR checklist](#6-pull-request-checklist)).

---

## 4. Add a new tool to an existing namespace

A **tool** is a single command, named `azmcp <namespace> <resource> <operation>` (for example
`azmcp storage container get`). Adding one to an existing toolset is the most common
contribution.

> [!TIP]
> The fastest path is Copilot Chat in Agent mode:
> `"create [namespace] [resource] [operation] command using #new-command.md as a reference"`.
> Always review the generated code against [`new-command.md`](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md).

### Required files

For each new command (see
[new-command.md → Required Files](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md#required-files)):

1. Option definitions: `src/Options/{Toolset}OptionDefinitions.cs`
2. Per-command options: `src/Options/{Resource}/{Operation}Options.cs`
3. Command class: `src/Commands/{Resource}/{Resource}{Operation}Command.cs`
4. Service interface + implementation: `src/Services/I{Service}Service.cs`, `src/Services/{Service}Service.cs`
5. JSON context entry: register response models in `{Toolset}JsonContext.cs` (AOT requirement)
6. Unit tests: `tests/.../{Resource}/{Resource}{Operation}CommandTests.cs`
7. Live tests: `tests/.../{Toolset}CommandTests.cs` (for Azure resource commands)
8. Command registration: add to `RegisterCommands()` in `{Toolset}Setup.cs`

### Conventions

- **Class/command naming:** `{Resource}{Operation}Command` (for example `AccountGetCommand`),
  never `GetAccountCommand`. Commands are `sealed` unless designed for inheritance.
- **Use primary constructors** and inject `ILogger` + the service interface.
- **Parameter naming:** use `subscription` (not `subscriptionId`), `resourceGroup` (not
  `resourceGroupName`), and singular resource nouns.
- **Use `OptionDefinitions` constants**, not hardcoded option strings. Use `.AsRequired()` /
  `.AsOptional()` and bind with `parseResult.GetValueOrDefault(Option<T>)`.
- **Always call `HandleException(context, ex)`** in catch blocks; never log `{@Options}`.
- **System.Text.Json only** (never Newtonsoft). All response models must be in the toolset's
  `JsonSerializerContext` for AOT safety.

### Validate your tool

```powershell
# Build just your toolset
dotnet build tools/Azure.Mcp.Tools.{Toolset}/src

# Run unit tests for your command
dotnet test --filter "FullyQualifiedName~{Resource}{Operation}CommandTests"

# Format and verify
dotnet format --include="tools/Azure.Mcp.Tools.{Toolset}/**/*.cs"
./eng/scripts/Build-Local.ps1 -UsePaths -VerifyNpx
```

Azure resource commands additionally **require recorded live tests**. See
[`recorded-tests.md`](https://github.com/microsoft/mcp/blob/main/docs/recorded-tests.md) for the record/playback workflow and
[CONTRIBUTING.md → Live Tests](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md#live-tests) for deploying test resources.

---

## 5. Integrate an external MCP server

The Azure MCP Server can act as a **proxy** that aggregates tools from external MCP servers
into a single interface. External servers are declared in the embedded registry file
[`servers/Azure.Mcp.Server/src/Resources/registry.json`](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/src/Resources/registry.json).

### Steps

1. **Edit `registry.json`** and add an entry under `servers`, keyed by a unique identifier.
2. **Choose a transport**:
   - **HTTP / SSE** — provide a `url`. Optionally add `title`, `toolPrefix` (unique prefix for
     the server's tools), and `oauthScopes` if the endpoint requires Entra authentication.
   - **stdio** — set `"type": "stdio"` with a `command`, plus optional `args` and `env`.
3. **Include a descriptive `description`** — it is surfaced to agents as the namespace tool
   description, so make it clear when the tool should be used.
4. **Rebuild** the project to embed the updated registry.

```jsonc
{
  "servers": {
    "documentation": {
      "url": "https://learn.microsoft.com/api/mcp",
      "title": "Microsoft Documentation Search",
      "description": "Search official Microsoft/Azure documentation..."
    },
    "my-stdio-server": {
      "type": "stdio",
      "command": "path/to/executable",
      "args": ["arg1", "arg2"],
      "env": { "ENV_VAR": "value" },
      "description": "An external MCP server using stdio transport"
    },
    "my-http-server": {
      "url": "<server_endpoint>",
      "title": "<server_title>",
      "description": "An external MCP server that offers X, Y, Z",
      "toolPrefix": "uniqueprefix_",
      "oauthScopes": ["<entra-client-id>/<identifier-uri>"]
    }
  }
}
```

### Discovery and filtering

External servers are discovered automatically at startup and respect the same namespace
filters as built-in commands:

```bash
# Include only specific external servers
azmcp server start --namespace documentation --namespace my-http-server

# Group external server tools behind a namespace proxy tool
azmcp server start --mode namespace
```

### Authentication notes

For Entra-protected HTTP endpoints, the external server needs an Entra app registration that
accepts authorization/token requests from common clients (Azure CLI, VS Code). Azure MCP can
pass user-principal tokens (stdio), service-principal tokens (stdio), or On-Behalf-Of tokens
(remote HTTP mode). **Test the app registration** against all supported user types (personal,
organizational, member, guest) and document any unsupported scenarios in the server's entry in
[README.md](https://github.com/microsoft/mcp/blob/main/README.md). Full details:
[CONTRIBUTING.md → Configuring External MCP Servers](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md#configuring-external-mcp-servers).

---

## 6. Pull request checklist

Before opening your PR from your fork into `microsoft/mcp:main`:

- [ ] **Format & build:** `dotnet format` and `dotnet build` are clean.
- [ ] **Unit tests** pass and cover your changes.
- [ ] **Live tests** included and **recorded** for Azure resource commands
      ([`recorded-tests.md`](https://github.com/microsoft/mcp/blob/main/docs/recorded-tests.md)).
- [ ] **AOT check** for new toolsets: `./eng/scripts/Build-Local.ps1 -BuildNative`.
- [ ] **Spelling:** `./eng/common/spelling/Invoke-Cspell.ps1` (add new terms to `.vscode/cspell.json`).
- [ ] **Docs updated:** [`azmcp-commands.md`](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md)
      and test prompts in [`e2eTestPrompts.md`](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md).
- [ ] **Changelog entry** added for user-facing changes
      ([`changelog-entries.md`](https://github.com/microsoft/mcp/blob/main/docs/changelog-entries.md)).
- [ ] **CODEOWNERS** updated for a new toolset.
- [ ] **One tool per PR** where possible.

---

## More resources

- [Command reference (`azmcp-commands.md`)](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md)
- [Implementing a new command (`new-command.md`)](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md)
- [Recorded tests guide (`recorded-tests.md`)](https://github.com/microsoft/mcp/blob/main/docs/recorded-tests.md)
- [AOT compatibility (`aot-compatibility.md`)](https://github.com/microsoft/mcp/blob/main/docs/aot-compatibility.md)
- [Authentication (`Authentication.md`)](https://github.com/microsoft/mcp/blob/main/docs/Authentication.md)
- [Changelog entries (`changelog-entries.md`)](https://github.com/microsoft/mcp/blob/main/docs/changelog-entries.md)
- [Contributing guide (`CONTRIBUTING.md`)](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md)
