# <img height="36" width="36" src="https://learn.microsoft.com/fabric/media/fabric-icon.png" alt="Microsoft Fabric Logo" /> Microsoft Fabric MCP Server

[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](https://github.com/microsoft/mcp/blob/main/LICENSE)
[![Repo](https://img.shields.io/badge/repo-microsoft/mcp-blue)](https://github.com/microsoft/mcp)

A local, AI-friendly Model Context Protocol (MCP) server that packages Microsoft Fabric's OpenAPI specifications, schema definitions, examples, and curated guidance into a single context layer for AI agents and developer tools.

Why this project?
- Provide a reliable, local-first source of Fabric API context for AI assistants and code generation tools.
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

### Installation Steps

1. **Clone the repository:**
   ```bash
   git clone https://github.com/microsoft/mcp.git
   cd mcp
   ```

2. **Build the project:**
   ```bash
   dotnet build servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj --configuration Release
   ```

3. **Locate your executable:**
   The executable `fabmcp` will be created at:
   ```
   servers/Fabric.Mcp.Server/src/bin/Release/fabmcp
   ```
   
   > **Platform Notes:**
   > - **macOS/Linux**: Use the path as-is: `/path/to/repo/servers/Fabric.Mcp.Server/src/bin/Release/fabmcp`
   > - **Windows**: Use backslashes and may need `.exe` extension: `C:\path\to\repo\servers\Fabric.Mcp.Server\src\bin\Release\fabmcp`
   > - For published builds, executables will be in platform-specific subdirectories with `.exe` extension on Windows

4. **Configure your MCP client:**

   Example configuration for VS Code (.vscode/mcp.json):
   ```json
   {
     "servers": {
       "Microsoft Fabric MCP": {
         "command": "/path/to/executable",
         "args": ["server start", "--mode all"]
       }
     }
   }
   ```

   > **Notes:** 
   > - Replace `/path/to/executable` with the actual path from step 3
   > - The `--mode all` argument enables all available tools


### Common Issues
- **SDK mismatch:** If `dotnet` outputs an SDK resolution error, inspect `global.json` and align local SDKs or update the file.
- **Path issues:** Always use absolute paths in MCP configuration to avoid path resolution problems.

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

See [CONTRIBUTING](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md) for full guidance.

---

## Support
If you encounter issues:
1. Search existing issues.
2. If none match, file a new issue with:
   - OS and `.NET` SDK version (`dotnet --info`).
   - The command used to start the server.
   - Server logs and MCP client config (redact secrets).
   - Steps to reproduce.

For troubleshooting steps, see [TROUBLESHOOTING](https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/TROUBLESHOOTING.md).

---

## License
This project is licensed under the MIT License — see the [LICENSE](https://github.com/microsoft/mcp/blob/main/LICENSE) file for details.