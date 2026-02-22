<!--
See eng\scripts\Process-PackageReadMe.ps1 for instruction on how to annotate this README.md for package specific output
-->
# <!-- remove-section: start nuget;vsix remove_fabric_logo --><img height="36" width="36" src="https://learn.microsoft.com/fabric/media/fabric-icon.png" alt="Microsoft Fabric Logo" /> <!-- remove-section: end remove_fabric_logo -->Microsoft Fabric MCP Server <!-- insert-section: nuget;vsix;npm {{ToolTitle}} -->
<!-- remove-section: start nuget;vsix;npm remove_note_preview -->
> [!NOTE]
> Microsoft Fabric MCP Server is currently in **Public Preview**.
<!-- remove-section: end remove_note_preview -->

<!-- insert-section: nuget {{MCPRepositoryMetadata}} -->

A local-first Model Context Protocol (MCP) server that provides AI agents with comprehensive access to Microsoft Fabric's public APIs, item definitions, and best practices. The Fabric MCP Server packages complete OpenAPI specifications into a single context layer for AI-assisted development—without connecting to live Fabric environments.
<!-- remove-section: start nuget;vsix;npm remove_install_links -->
[![Install Fabric MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_Fabric_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://marketplace.visualstudio.com/items?itemName=fabric.vscode-fabric-mcp-server) [![Install Fabric MCP in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Fabric_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect?url=vscode-insiders:extension/ms-fabric.vscode-fabric-mcp-server)

[![GitHub](https://img.shields.io/badge/github-microsoft/mcp-blue.svg?style=flat-square&logo=github&color=6e3fa3)](https://github.com/microsoft/mcp)
[![GitHub Release](https://img.shields.io/github/v/release/microsoft/mcp?include_prereleases&filter=Fabric.Mcp.*&style=flat-square&color=6e3fa3)](https://github.com/microsoft/mcp/releases?q=Fabric.Mcp.Server-)
[![License](https://img.shields.io/badge/license-MIT-green?style=flat-square&color=6e3fa3)](https://github.com/microsoft/mcp/blob/main/LICENSE)

<!-- remove-section: end remove_install_links -->
## Table of Contents
- [ Microsoft Fabric MCP Server ](#-microsoft-fabric-mcp-server-)
  - [Table of Contents](#table-of-contents)
- [Overview](#overview)
- [Installation](#installation)
  - [IDE](#ide)
    - [VS Code (Recommended)](#vs-code-recommended)
    - [Manual Setup](#manual-setup)
      - [Option 1: Configure using .NET (build from source)](#option-1-configure-using-net-build-from-source)
      - [Option 2: Configure using Node.js (npm/npx)](#option-2-configure-using-nodejs-npmnpx)
- [Usage](#usage)
  - [Getting Started](#getting-started)
  - [What can you do with the Fabric MCP Server?](#what-can-you-do-with-the-fabric-mcp-server)
    - [Fabric Workloads \& APIs](#fabric-workloads--apis)
    - [Resource Definitions \& Schemas](#resource-definitions--schemas)
    - [Best Practices \& Examples](#best-practices--examples)
    - [Development Workflows](#development-workflows)
  - [Available Tools](#available-tools)
    - [docs - API Documentation \& Best Practices](#docs---api-documentation--best-practices)
    - [onelake - OneLake Data Operations](#onelake---onelake-data-operations)
    - [core - Core Fabric Operations](#core---core-fabric-operations)
- [Support and Reference](#support-and-reference)
  - [Documentation](#documentation)
  - [Feedback and Support](#feedback-and-support)
  - [Security](#security)
  - [Data Collection](#data-collection)
  - [Contributing](#contributing)
  - [Code of Conduct](#code-of-conduct)
- [License](#license)

# Overview

**Microsoft Fabric MCP Server** gives your AI agents the knowledge they need to generate robust, production-ready code for Microsoft Fabric—all without directly accessing your environment.

Key capabilities:
- **Complete API Context**: Full OpenAPI specifications for all supported Fabric workloads
- **Item Definition Knowledge**: JSON schemas for every Fabric item type (Lakehouses, pipelines, semantic models, notebooks, etc.)
- **Built-in Best Practices**: Embedded guidance on pagination, error handling, and recommended patterns
- **Local-First Security**: Runs entirely on your machine—never connects to your Fabric environment

# Installation
<!-- insert-section: vsix {{- Install the [Fabric MCP Server Visual Studio Code extension](https://marketplace.visualstudio.com/items?itemName=fabric.vscode-fabric-mcp-server)}} -->
<!-- insert-section: vsix {{- Start (or Auto-Start) the MCP Server}} -->
<!-- insert-section: vsix {{   > **VS Code (version 1.103 or above):** You can configure MCP servers to start automatically using the `chat.mcp.autostart` setting.}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{   #### **Enable Autostart**}} -->
<!-- insert-section: vsix {{   1. Open **Settings** in VS Code.}} -->
<!-- insert-section: vsix {{   2. Search for `chat.mcp.autostart`.}} -->
<!-- insert-section: vsix {{   3. Select **newAndOutdated** to automatically start MCP servers without manual refresh.}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{   #### **Manual Start (if autostart is off)**}} -->
<!-- insert-section: vsix {{   1. Open Command Palette (`Ctrl+Shift+P` / `Cmd+Shift+P`).}} -->
<!-- insert-section: vsix {{   2. Run `MCP: List Servers`.}} -->
<!-- insert-section: vsix {{   3. Select `Fabric MCP Server`, then click **Start Server**.}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{   4. **Check That It's Running**}} -->
<!-- insert-section: vsix {{      - Go to the **Output** tab in VS Code.}} -->
<!-- insert-section: vsix {{      - Look for log messages confirming the server started successfully.}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{- (Optional) Configure server behavior in VS Code settings (search for "Fabric MCP")}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{You're all set! Fabric MCP Server is now ready to help you work smarter with Microsoft Fabric in VS Code.}} -->
<!-- remove-section: start vsix remove_entire_installation_sub_section -->
<!-- remove-section: start nuget;npm remove_ide_sub_section -->

## IDE

Start using Fabric MCP with your favorite IDE. We recommend VS Code:

### VS Code (Recommended)
Compatible with both the [Stable](https://code.visualstudio.com/download) and [Insiders](https://code.visualstudio.com/insiders) builds of VS Code.

1. Install the [GitHub Copilot Chat](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot-chat) extension.
1. Install the [Fabric MCP Server](https://marketplace.visualstudio.com/items?itemName=fabric.vscode-fabric-mcp-server) extension.

### Manual Setup
Fabric MCP Server can also be configured across other IDEs, CLIs, and MCP clients:

<details>
<summary>Manual setup instructions</summary>

Use one of the following options to configure your `mcp.json`:
<!-- remove-section: end remove_ide_sub_section -->
<!-- remove-section: start npm remove_dotnet_config_sub_section -->
<!-- remove-section: start nuget remove_dotnet_config_sub_header -->
#### Option 1: Configure using .NET (build from source)<!-- remove-section: end remove_dotnet_config_sub_header -->
- You must have [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later installed.
  To verify the .NET version, run: `dotnet --version`
- Clone and build the repository:

    ```bash
    git clone https://github.com/microsoft/mcp.git
    cd mcp
    dotnet build servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj --configuration Release
    ```

- Configure the `mcp.json` file with the following:

    ```json
    {
        "mcpServers": {
            "Fabric MCP Server": {
                "command": "/path/to/repo/servers/Fabric.Mcp.Server/src/bin/Release/fabmcp",
                "args": [
                    "server",
                    "start"
                ],
                "type": "stdio"
            }
        }
    }
    ```

> **Platform Notes:**
> - **macOS/Linux**: Use the path as-is
> - **Windows**: Use backslashes and add `.exe` extension: `C:\path\to\repo\servers\Fabric.Mcp.Server\src\bin\Release\fabmcp.exe`
<!-- remove-section: end remove_dotnet_config_sub_section -->
<!-- remove-section: start nuget remove_node_config_sub_section -->
<!-- remove-section: start npm remove_node_config_sub_header -->
#### Option 2: Configure using Node.js (npm/npx)<!-- remove-section: end remove_node_config_sub_header -->
- To use Fabric MCP server from node one must have Node.js (LTS) installed and available on your system PATH — this provides both `npm` and `npx`. We recommend Node.js 20 LTS or later. To verify your installation run: `node --version`, `npm --version`, and `npx --version`.
-  Configure the `mcp.json` file with the following:

    ```json
    {
        "mcpServers": {
            "fabric-mcp-server": {
            "command": "npx",
            "args": [
                "-y",
                "@microsoft/fabric-mcp@latest",
                "server",
                "start",
                "--mode",
                "all"
                ]
            }
        }
    }
    ```
<!-- remove-section: end remove_node_config_sub_section -->
<!-- remove-section: start nuget remove_custom_client_config_table -->
**Note:** When manually configuring Visual Studio and Visual Studio Code, use `servers` instead of `mcpServers` as the root object.

**Client-Specific Configuration**
| IDE | File Location | Documentation Link |
|-----|---------------|-------------------|
| **Claude Code** | `~/.claude.json` or `.mcp.json` (project) | [Claude Code MCP Configuration](https://scottspence.com/posts/configuring-mcp-tools-in-claude-code) |
| **Claude Desktop** | `~/.claude/claude_desktop_config.json` (macOS)<br>`%APPDATA%\Claude\claude_desktop_config.json` (Windows) | [Claude Desktop MCP Setup](https://support.claude.com/en/articles/10949351-getting-started-with-local-mcp-servers-on-claude-desktop) |
| **Cursor** | `~/.cursor/mcp.json` or `.cursor/mcp.json` | [Cursor MCP Documentation](https://docs.cursor.com/context/model-context-protocol) |
| **VS Code** | `.vscode/mcp.json` (workspace)<br>`settings.json` (user) | [VS Code MCP Documentation](https://code.visualstudio.com/docs/copilot/chat/mcp-servers) |
| **Windsurf** | `~/.codeium/windsurf/mcp_config.json` | [Windsurf Cascade MCP Integration](https://docs.windsurf.com/windsurf/cascade/mcp) |
<!-- remove-section: end remove_custom_client_config_table -->
<!-- remove-section: start nuget;npm remove_closing_details -->
</details>
<!-- remove-section: end remove_closing_details -->
<!-- remove-section: end remove_entire_installation_sub_section -->

# Usage

## Getting Started

1. Open GitHub Copilot in [VS Code](https://code.visualstudio.com/docs/copilot/chat/chat-agent-mode) and switch to Agent mode.
1. Click `refresh` on the tools list
    - You should see the Fabric MCP Server in the list of tools
1. Try a prompt that uses Fabric context, such as `What Fabric workload types are available?`
    - The agent should be able to use the Fabric MCP Server tools to complete your query
1. Check out the [Microsoft Fabric documentation](https://learn.microsoft.com/fabric/) and review the [troubleshooting guide](https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/TROUBLESHOOTING.md) for commonly asked questions
1. We're building this in the open. Your feedback is much appreciated!
    - [Open an issue in the public repository](https://github.com/microsoft/mcp/issues/new/choose)

## What can you do with the Fabric MCP Server?

The Fabric MCP Server supercharges your agents with Microsoft Fabric context. Here are some prompts you can try:

### Fabric Workloads & APIs

* "What are the available Fabric workload types I can work with?"
* "Show me the OpenAPI operations for 'notebook' and give a sample creation body"
* "Get the platform-level API specifications for Microsoft Fabric"
* "List all supported Fabric item types"

### Resource Definitions & Schemas

* "Create a Lakehouse resource definition with a schema that enforces a string column and a datetime column"
* "Show me the JSON schema for a Data Pipeline item definition"
* "Generate a Semantic Model configuration with sample measures"
* "What properties are required for creating a KQL Database?"

### Best Practices & Examples

* "Show me best practices for handling API throttling in Fabric"
* "How should I implement retry logic for Fabric API rate limits?"
* "List recommended retry/backoff behavior for Fabric APIs when rate-limited"
* "Show me best practices for authenticating with Fabric APIs"
* "Get example request/response payloads for creating a Notebook"
* "What are the pagination patterns for Fabric REST APIs?"

### Development Workflows

* "Generate a data pipeline configuration with sample data sources"
* "Help me scaffold a Fabric workspace with Lakehouse and notebooks"
* "Show me how to handle long-running operations in Fabric APIs"
* "What's the recommended error handling pattern for Fabric API calls?"

<!-- remove-section: start vsix remove_available_tools_section -->
## Available Tools

The Fabric MCP Server exposes tools organized into three categories:

### docs - API Documentation & Best Practices

| Tool Name | Description |
|-----------|-------------|
| `docs_list_workloads` | Lists Fabric workload types that have public API specifications available. |
| `docs_get_api_spec` | Retrieves the complete OpenAPI specification for a specific Fabric workload. |
| `docs_get_platform_api_spec` | Retrieves the OpenAPI specification for core Fabric platform APIs. |
| `docs_get_item_definition` | Retrieves JSON schema definitions for items in a Fabric workload API. |
| `docs_get_best_practices` | Retrieves best practice documentation and guidance for a specific topic. |
| `docs_get_examples` | Retrieves example API request/response files for a specific workload. |

### onelake - OneLake Data Operations

| Tool Name | Description |
|-----------|-------------|
| `onelake_list_workspaces` | Lists available Microsoft Fabric workspaces. |
| `onelake_list_items` | Lists workspace items with high-level metadata. |
| `onelake_list_items_dfs` | Lists Fabric items via the DFS endpoint. |
| `onelake_list_files` | Lists files using the hierarchical file-list endpoint. |
| `onelake_download_file` | Downloads a OneLake file. |
| `onelake_upload_file` | Uploads a file to OneLake storage. |
| `onelake_delete_file` | Deletes individual files from OneLake storage. |
| `onelake_create_directory` | Creates a directory via the DFS endpoint. |
| `onelake_delete_directory` | Deletes a directory (optionally recursive). |
| `onelake_get_table_config` | Retrieves table API configuration for a workspace item. |
| `onelake_list_table_namespaces` | Lists table namespaces (schemas) exposed through the table API. |
| `onelake_get_table_namespace` | Retrieves metadata for a specific namespace. |
| `onelake_list_tables` | Lists tables published within a namespace. |
| `onelake_get_table` | Retrieves the definition for a specific table. |

### core - Core Fabric Operations

| Tool Name | Description |
|-----------|-------------|
| `core_create_item` | Creates new Fabric items (Lakehouses, Notebooks, etc.). |

> Always verify available commands via `--help`. Command names and availability may change between releases.
<!-- remove-section: end remove_available_tools_section -->

# Support and Reference

## Documentation

- See the [Microsoft Fabric documentation](https://learn.microsoft.com/fabric/) to learn about the Microsoft Fabric platform.
- For MCP server-specific troubleshooting, see the [Troubleshooting Guide](https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/TROUBLESHOOTING.md).

## Feedback and Support

- Check the [Troubleshooting guide](https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/TROUBLESHOOTING.md) to diagnose and resolve common issues.
- We're building this in the open. Your feedback is much appreciated!
    - [Open an issue](https://github.com/microsoft/mcp/issues) in the public GitHub repository — we'd love to hear from you!

## Security

The Fabric MCP Server is a **local-first** tool that runs entirely on your machine. It provides API specifications, schemas, and best practices without connecting to live Microsoft Fabric environments.

MCP as a phenomenon is very novel and cutting-edge. As with all new technology standards, consider doing a security review to ensure any systems that integrate with MCP servers follow all regulations and standards your system is expected to adhere to.

## Data Collection

<!-- remove-section: start vsix remove_data_collection_section_content -->
The software may collect information about you and your use of the software and send it to Microsoft. Microsoft may use this information to provide services and improve our products and services. You may turn off the telemetry as described in the repository. There are also some features in the software that may enable you and Microsoft to collect data from users of your applications. If you use these features, you must comply with applicable law, including providing appropriate notices to users of your applications together with a copy of Microsoft's [privacy statement](https://www.microsoft.com/privacy/privacystatement). You can learn more about data collection and use in the help documentation and our privacy statement. Your use of the software operates as your consent to these practices.
<!-- remove-section: end remove_data_collection_section_content -->
<!-- insert-section: vsix {{The software may collect information about you and your use of the software and send it to Microsoft. Microsoft may use this information to provide services and improve our products and services. You may turn off the telemetry by following the instructions [here](https://code.visualstudio.com/docs/configure/telemetry#_disable-telemetry-reporting).}} -->

## Contributing

We welcome contributions to the Fabric MCP Server! Whether you're fixing bugs, adding new features, or improving documentation, your contributions are welcome.

Please read our [Contributing Guide](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md) for guidelines on:

* Setting up your development environment
* Adding new commands
* Code style and testing requirements
* Making pull requests

## Code of Conduct
This project has adopted the
[Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information, see the
[Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [open@microsoft.com](mailto:open@microsoft.com)
with any additional questions or comments.

---

# License

This project is licensed under the MIT License — see the [LICENSE](https://github.com/microsoft/mcp/blob/main/LICENSE) file for details.
