# <img height="36" width="36" src="https://learn.microsoft.com/fabric/media/fabric-icon.png" alt="Microsoft Fabric Logo" /> Microsoft Fabric MCP Server

[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](https://github.com/microsoft/mcp/blob/main/LICENSE)
[![Repo](https://img.shields.io/badge/repo-microsoft/mcp-blue)](https://github.com/microsoft/mcp)

A local, AI-friendly Model Context Protocol (MCP) server that packages Microsoft Fabric's OpenAPI specifications, schema definitions, examples, and curated guidance into a single context layer for AI agents and developer tools.

Why this project?
- Provide a reliable, local-first source of Fabric API context for AI asistentes and code generation tools.
- Reduce the risk of leaking production credentials while enabling rich, example-driven development.
- Make Fabric API discovery, schema lookup, and best-practice retrieval reproducible and scriptable.

---

## Table of Contents
- [What Can You Do?](#what-can-you-do)
- [Getting Started](#getting-started)
- [Available Tools](#available-tools)
- [Development & Contributing](#development--contributing)
- [Support](#support)
- [License](#license)

---

## What Can You Do?
The Fabric MCP Server unlocks practical developer workflows by providing local access to Fabric API context:

- Generate or scaffold Fabric resource definitions (Lakehouse, data pipelines, notebooks, reports).
- Retrieve official OpenAPI specs and JSON schema for validation and code generation.
- Get example request/response payloads to accelerate integration.
- Query curated best-practice guidance (pagination, LROs, authentication patterns).

<details>
<summary>Example prompts</summary>

- "Create a Lakehouse resource definition with a schema that enforces a string column and a datetime column."  
- "Show me the OpenAPI operations for 'notebook' and give a sample creation body."  
- "List recommended retry/backoff behavior for Fabric APIs when rate-limited."

</details>

---

## Getting Started

### Prerequisites
- .NET 9.x SDK is recommended. Check `global.json` at the repository root for any pinned SDK version.
  - If `global.json` pins a preview SDK not installed locally, either install the requested preview SDK or update `global.json` for local development.
- An MCP-compatible client (VS Code with an MCP extension, Claude Desktop, etc.).

3. **Publish the executable:**
   ```bash
    # For Windows
   dotnet publish -c Release -r win-x64 --self-contained
   
      # For Linux
   dotnet publish -c Release -r linux-x64 --self-contained

   # For Apple Silicon Macs
   dotnet publish -c Release -r osx-arm64 --self-contained
   
   # For Intel Macs
   dotnet publish -c Release -r osx-x64 --self-contained

   ```

4. **Find your executable:**
   ```
   # The executable will be in:
   bin/Release/net9.0/{your-rid}/publish/Fabric.Mcp.Server
   
   # For example, on Apple Silicon Mac:
   bin/Release/net9.0/osx-arm64/publish/Fabric.Mcp.Server
   ```

5. **Configure your MCP client:**

Template MCP client configuration (replace placeholders):

```json
{
  "servers": {
    "<SERVER_NAME>": {
      "command": "<ABSOLUTE_PATH_TO_PUBLISHED_EXECUTABLE>",
      "args": ["server", "start", "--mode", "all"],
      "env": {
        "PATH": "<PATH_WITH_DOTNET_AND_TOOLS>"
      }
    }
  }
}
```

### For contributors — run from source
If you're contributing or iterating quickly, run from source:

```json
{
  "servers": {
    "Fabric MCP (dev)": {
      "command": "dotnet",
      "args": ["run", "--project", "servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj"]
    }
  }
}
```

**Development notes:**
- `dotnet run` automatically builds and starts the server; convenient for iterative development.
- Use `dotnet publish` to test the published experience.

### Common issues
- SDK mismatch: If `dotnet` outputs an SDK resolution error, inspect `global.json` and align local SDKs or update the file.

---

## Available Tools
Use the server's CLI to query embedded data and examples. Commands are organized under a `publicapis` command group in code.

| Command | Purpose | Implementation |
|---|---|---|
| `publicapis list` | List supported workload names (e.g. notebook, report) | tools/Fabric.Mcp.Tools.PublicApi/src/Commands/PublicApis/ListWorkloadsCommand.cs |
| `publicapis get --workload-type <workload>` | Fetch OpenAPI & examples for a workload | tools/Fabric.Mcp.Tools.PublicApi/src/Commands/PublicApis/GetWorkloadApisCommand.cs |
| `publicapis platform get` | Retrieve platform-level API specs | tools/Fabric.Mcp.Tools.PublicApi/src/Commands/PublicApis/GetPlatformApisCommand.cs |
| `publicapis bestpractices get --workload-type <workload>` | Retrieve best-practice guidance for a workload | tools/Fabric.Mcp.Tools.PublicApi/src/Commands/BestPractices/GetBestPracticesCommand.cs |
| `publicapis examples get --workload-type <workload>` | Retrieve example request/response files for a workload | tools/Fabric.Mcp.Tools.PublicApi/src/Commands/BestPractices/GetExamplesCommand.cs |
| `publicapis itemdefinition get --workload-type <workload>` | Get JSON schema definitions for a workload | tools/Fabric.Mcp.Tools.PublicApi/src/Commands/BestPractices/GetWorkloadDefinitionCommand.cs |

> Always verify the available commands in your build via `--help` before scripting against them; command names and availability are code-driven and may change between releases.

---

## Development & Contributing
We welcome contributions. Please follow the repository's contribution guidelines and the checklist below when preparing a PR.

**Contributor checklist**
- Create a focused branch for your changes.
- Run a local build and unit tests for affected projects.
- Update `CHANGELOG.md` for user-visible changes.
- Run `eng` validation scripts where applicable (spelling, linters).
- Provide a clear PR description and link relevant issues.

See [CONTRIBUTING.md](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md) for full guidance.

---

## Support
If you encounter issues:
1. Search existing issues and discussions.
2. If none match, file a new issue with:
   - OS and `.NET` SDK version (`dotnet --info`).
   - The command used to start the server.
   - Server logs and MCP client config (redact secrets).
   - Steps to reproduce.

For troubleshooting steps, consult `TROUBLESHOOTING.md`.

---

## License
This project is licensed under the MIT License — see the [LICENSE](https://github.com/microsoft/mcp/blob/main/LICENSE) file for details.

---

If you'd like additional polishing (graphics, badges, screenshots, or example walkthroughs), tell me which assets and I will add them next.