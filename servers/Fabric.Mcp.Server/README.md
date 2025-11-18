<!--
See eng\scripts\Process-PackageReadMe.ps1 for instruction on how to annotate this README.md for package specific output
-->
# <!-- remove-section: start nuget;vsix remove_fabric_logo --><img height="36" width="36" src="https://learn.microsoft.com/fabric/media/fabric-icon.png" alt="Microsoft Fabric Logo" /> <!-- remove-section: end remove_fabric_logo -->Microsoft Fabric MCP Server <!-- insert-section: nuget;vsix;npm {{ToolTitle}} -->

<!-- insert-section: nuget {{MCPRepositoryMetadata}} -->

<!-- remove-section: start nuget;vsix;npm remove_install_links -->
[![Install Fabric MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_Fabric_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://marketplace.visualstudio.com/items?itemName=fabric.vscode-fabric)

[![GitHub](https://img.shields.io/badge/github-microsoft/mcp-blue.svg?style=flat-square&logo=github&color=6e3fa3)](https://github.com/microsoft/mcp)
[![GitHub Release](https://img.shields.io/github/v/release/microsoft/mcp?include_prereleases&filter=Fabric.Mcp.*&style=flat-square&color=6e3fa3)](https://github.com/microsoft/mcp/releases?q=Fabric.Mcp.Server-)
[![License](https://img.shields.io/badge/license-MIT-green?style=flat-square&color=6e3fa3)](https://github.com/microsoft/mcp/blob/main/LICENSE)
<!-- remove-section: end remove_install_links -->

<!-- insert-section: vsix {{A Visual Studio Code extension that provides AI agents with access to Microsoft Fabric's API specifications, schemas, examples, and best practices through the Model Context Protocol (MCP).}} -->

<!-- remove-section: start vsix -->
A local, AI-friendly Model Context Protocol (MCP) server that packages Microsoft Fabric's OpenAPI specifications, schema definitions, examples, and curated guidance into a single context layer for AI agents and developer tools.

Why this project?
- Provide a reliable, local-first source of Fabric API context for AI assistants and code generation tools.
- Reduce the risk of leaking production credentials while enabling rich, example-driven development.
- Make Fabric API discovery, schema lookup, and best-practice retrieval reproducible and scriptable.
<!-- remove-section: end -->

---

## Table of Contents
- [What Can You Do?](#what-can-you-do)
- [Getting Started](#getting-started)<!-- remove-section: start vsix -->
- [Available Tools](#available-tools)<!-- remove-section: end -->
- [Development and Contributing](#development-and-contributing)
- [Support](#support)
- [License](#license)

---

# What Can You Do?
<!-- insert-section: vsix {{Use the Fabric MCP Server to accelerate your Microsoft Fabric development with AI-powered assistance:}} -->

<!-- remove-section: start vsix -->
The Fabric MCP Server unlocks practical developer workflows by providing local access to Fabric API context:
<!-- remove-section: end -->

- Generate or scaffold Fabric resource definitions (Lakehouse, data pipelines, notebooks, reports).
- Retrieve official OpenAPI specs and JSON schema for validation and code generation.
- Get example request/response payloads to accelerate integration.
- Query curated best-practice guidance (pagination, LROs, authentication patterns).

<details>
<summary>Example prompts</summary>

- "Create a Lakehouse resource definition with a schema that enforces a string column and a datetime column."  
- "Show me the OpenAPI operations for 'notebook' and give a sample creation body."  
- "List recommended retry/backoff behavior for Fabric APIs when rate-limited."
- "What are the available Fabric workload types I can work with?"
- "Generate a data pipeline configuration with sample data sources."
- "Show me best practices for authenticating with Fabric APIs."

</details>

---

# Getting Started

<!-- remove-section: start vsix -->
## Prerequisites
- .NET 9.x SDK is recommended. Check `global.json` at the repository root for any pinned SDK version.
  - If `global.json` pins a preview SDK not installed locally, either install the requested preview SDK or update `global.json` for local development.
- An MCP-compatible client (VS Code with an MCP extension, Claude Desktop, etc.).

## Installation Steps

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
         "args": ["server", "start", "--mode", "all"]
       }
     }
   }
   ```

   > **Notes:** 
   > - Replace `/path/to/executable` with the actual path from step 3
   > - The `--mode all` argument enables all available tools


## Common Issues
- **SDK mismatch:** If `dotnet` outputs an SDK resolution error, inspect `global.json` and align local SDKs or update the file.
- **Path issues:** Always use absolute paths in MCP configuration to avoid path resolution problems.
<!-- remove-section: end -->

<!-- insert-section: vsix {{## Installation

The Fabric MCP Server extension is already installed! Simply start the server to begin using it.

### Starting the Server

1. Open the Command Palette (`Ctrl+Shift+P` / `Cmd+Shift+P`)
2. Run **MCP: List Servers**
3. Find **Fabric MCP Server** in the list and click the **Start** button

Once started, the Fabric MCP tools will be available in GitHub Copilot Chat.

### Configuration (Optional)

You can customize the server behavior in VS Code settings (search for "Fabric MCP"):

- **Server Mode**: Control how tools are exposed (`all` by default)
- **Server Arguments**: Add custom arguments to the server startup

Changes require restarting the MCP server from the **MCP: List Servers** view.
}} -->

---

<!-- remove-section: start vsix -->
# Available Tools
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
<!-- remove-section: end -->

# Development and Contributing

<!-- remove-section: start vsix -->
We welcome contributions. Please follow the repository's contribution guidelines and the checklist below when preparing a PR.

**Contributor checklist**
- Create a focused branch for your changes.
- Run a local build and unit tests for affected projects.
- Update `CHANGELOG.md` for user-visible changes.
- Run `eng` validation scripts where applicable (spelling, linters).
- Provide a clear PR description and link relevant issues.

See [CONTRIBUTING](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md) for full guidance.
<!-- remove-section: end -->

<!-- insert-section: vsix {{Interested in contributing to the Fabric MCP Server? Visit the [GitHub repository](https://github.com/microsoft/mcp) to get started.

See [CONTRIBUTING](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md) for full contribution guidelines.
}} -->

---

# Support
If you encounter issues:
1. Search existing issues.
2. If none match, file a new issue with:
<!-- remove-section: start vsix -->
   - OS and `.NET` SDK version (`dotnet --info`).
   - The command used to start the server.
   - Server logs and MCP client config (redact secrets).
   - Steps to reproduce.
<!-- remove-section: end -->
<!-- insert-section: vsix {{   - Your OS and VS Code version
   - Server logs from the **MCP: List Servers** view
   - Steps to reproduce the issue
}} -->

For troubleshooting steps, see [TROUBLESHOOTING](https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/TROUBLESHOOTING.md).

---

# License
This project is licensed under the MIT License — see the [LICENSE](https://github.com/microsoft/mcp/blob/main/LICENSE) file for details.