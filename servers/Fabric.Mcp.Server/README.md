<!--
See eng\scripts\Process-PackageReadMe.ps1 for instruction on how to annotate this README.md for package specific output
-->
# <!-- remove-section: start nuget;vsix remove_fabric_logo --><img height="36" width="36" src="https://learn.microsoft.com/fabric/media/fabric-icon.png" alt="Microsoft Fabric Logo" /> <!-- remove-section: end remove_fabric_logo -->Microsoft Fabric MCP Server <!-- insert-section: nuget;vsix;npm {{ToolTitle}} -->

<!-- insert-section: nuget;pypi {{MCPRepositoryMetadata}} -->

A local-first Model Context Protocol (MCP) server that provides AI agents with Microsoft Fabric API documentation, item definitions, best practices, and tools for live Fabric operations. API documentation and best-practices tools work offline. OneLake, core Fabric, and Data Factory tools use configured credentials to access your Fabric environment.
<!-- remove-section: start nuget;vsix;npm remove_install_links -->
[![Install Fabric MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_Fabric_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://marketplace.visualstudio.com/items?itemName=fabric.vscode-fabric-mcp-server) [![Install Fabric MCP in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Fabric_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect?url=vscode-insiders:extension/ms-fabric.vscode-fabric-mcp-server)

[![GitHub](https://img.shields.io/badge/github-microsoft/mcp-blue.svg?style=flat-square&logo=github&color=6e3fa3)](https://github.com/microsoft/mcp)
[![GitHub Release](https://img.shields.io/github/v/release/microsoft/mcp?include_prereleases&filter=Fabric.Mcp.*&style=flat-square&color=6e3fa3)](https://github.com/microsoft/mcp/releases?q=Fabric.Mcp.Server-)
[![License](https://img.shields.io/badge/license-MIT-green?style=flat-square&color=6e3fa3)](https://github.com/microsoft/mcp/blob/main/LICENSE)

<!-- remove-section: end remove_install_links -->
## Table of contents
- [Overview](#overview)
- [Installation](#installation)
    <!-- remove-section: start nuget;vsix;npm remove_installation_toc -->
    - [IDE](#ide)
        - [VS Code (recommended)](#vs-code-recommended)
        - [Manual setup](#manual-setup)
    <!-- remove-section: end remove_installation_toc -->
- [Usage](#usage)
    - [Getting started](#getting-started)
    - [What can you do with the Fabric MCP Server?](#what-can-you-do-with-the-fabric-mcp-server)
        - [Catalog discovery](#catalog-discovery)
        - [Fabric item types & APIs](#fabric-item-types--apis)
        - [Resource definitions & schemas](#resource-definitions--schemas)
        - [Best practices & examples](#best-practices--examples)
        - [Development workflows](#development-workflows)
    <!-- remove-section: start vsix remove_tools_toc -->
    - [Available tools](#available-tools)
        - [API documentation & best practices](#api-documentation--best-practices)
        - [OneLake data operations](#onelake-data-operations)
        - [OneLake security — Data access roles](#onelake-security--data-access-roles)
        - [OneLake shortcuts](#onelake-shortcuts)
        - [OneLake settings](#onelake-settings)
        - [Core Fabric operations](#core-fabric-operations)
        - [Data Factory operations](#data-factory-operations)
    <!-- remove-section: end remove_tools_toc -->
- [Support and reference](#support-and-reference)
    - [Documentation](#documentation)
    - [Feedback and support](#feedback-and-support)
    - [Security](#security)
        - [Authentication for live operations](#authentication-for-live-operations)
        - [Permissions and risk](#permissions-and-risk)
        - [Auditability](#auditability)
        - [Compliance responsibility](#compliance-responsibility)
    - [Third-party components](#third-party-components)
    - [Export control](#export-control)
    - [No warranty and limitation of liability](#no-warranty-and-limitation-of-liability)
    - [Data collection](#data-collection)
    - [Contributing](#contributing)
    - [Code of conduct](#code-of-conduct)
- [License](#license)

# Overview

**Microsoft Fabric MCP Server** gives your AI agents context for developing Fabric solutions and tools for working with Fabric resources. Use documentation tools without a live Fabric connection, or configure credentials for live operations.

Key capabilities:
- **API context**: OpenAPI specifications for supported Fabric item types
- **Item definitions**: Documentation and schemas for supported Fabric item definitions
- **Built-in guidance**: Recommendations for pagination, error handling, and other API patterns
- **Local execution**: Documentation tools work offline. Live operations require Fabric access.
- **Data Factory integration**: Pipeline and Dataflow Gen2 tools, including Power Query M execution

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
<!-- insert-section: vsix {{   3. Select `Fabric MCP Server`, then select **Start Server**.}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{   4. **Check That It's Running**}} -->
<!-- insert-section: vsix {{      - Go to the **Output** tab in VS Code.}} -->
<!-- insert-section: vsix {{      - Look for log messages confirming the server started successfully.}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{- (Optional) Configure server behavior in VS Code settings (search for "Fabric MCP")}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{For live Fabric operations, configure [authentication](#authentication-for-live-operations).}} -->
<!-- remove-section: start vsix remove_entire_installation_sub_section -->
<!-- remove-section: start nuget;npm remove_ide_sub_section -->

## IDE

Start using Fabric MCP with your favorite IDE. We recommend VS Code:

### VS Code (recommended)
Compatible with both the [Stable](https://code.visualstudio.com/download) and [Insiders](https://code.visualstudio.com/insiders) builds of VS Code.

1. Install the [GitHub Copilot Chat](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot-chat) extension.
1. Install the [Fabric MCP Server](https://marketplace.visualstudio.com/items?itemName=fabric.vscode-fabric-mcp-server) extension.

### Manual setup
Fabric MCP Server can also be configured across other IDEs, CLIs, and MCP clients:

<details>
<summary>Manual setup instructions</summary>

Use one of the following options to configure your `mcp.json`:
<!-- remove-section: end remove_ide_sub_section -->
<!-- remove-section: start npm remove_dotnet_config_sub_section -->
<!-- remove-section: start nuget remove_dotnet_config_sub_header -->
#### Option 1: Configure using .NET (build from source)<!-- remove-section: end remove_dotnet_config_sub_header -->
- Install the .NET SDK specified in the repository's [global.json](https://github.com/microsoft/mcp/blob/main/global.json). Download the required SDK from the [.NET downloads page](https://dotnet.microsoft.com/download/dotnet).
  Run `dotnet --version` from the repository directory to check the selected SDK.
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
> - **Windows**: Add the `.exe` extension and escape backslashes in JSON: `C:\\path\\to\\repo\\servers\\Fabric.Mcp.Server\\src\\bin\\Release\\fabmcp.exe`
<!-- remove-section: end remove_dotnet_config_sub_section -->
<!-- remove-section: start nuget remove_node_config_sub_section -->
<!-- remove-section: start npm remove_node_config_sub_header -->
#### Option 2: Configure using Node.js (npm/npx)<!-- remove-section: end remove_node_config_sub_header -->
- Install the latest [Node.js LTS release](https://nodejs.org/en/download), which includes `npm` and `npx`. Ensure they are available on your system PATH. Check your installation with `node --version`, `npm --version`, and `npx --version`.
- Configure the `mcp.json` file with the following:

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

**Client-specific configuration**
| Client | Configuration | Documentation |
|-----|---------------|-------------------|
| **Claude Code** | `~/.claude.json` or `.mcp.json` (project) | [Claude Code MCP configuration](https://code.claude.com/docs/en/mcp) |
| **Claude Desktop** | Use the configuration instructions for your platform. | [Claude Desktop MCP setup](https://support.claude.com/en/articles/10949351-getting-started-with-local-mcp-servers-on-claude-desktop) |
| **Cursor** | `~/.cursor/mcp.json` or `.cursor/mcp.json` | [Cursor MCP Documentation](https://docs.cursor.com/context/model-context-protocol) |
| **VS Code** | `.vscode/mcp.json` (workspace); run **MCP: Open User Configuration** for user settings. | [VS Code MCP documentation](https://code.visualstudio.com/docs/agent-customization/mcp-servers) |
| **Windsurf** | `~/.codeium/windsurf/mcp_config.json` | [Windsurf Cascade MCP Integration](https://docs.windsurf.com/windsurf/cascade/mcp) |
<!-- remove-section: end remove_custom_client_config_table -->
<!-- remove-section: start nuget;npm remove_closing_details -->
</details>
<!-- remove-section: end remove_closing_details -->
<!-- remove-section: end remove_entire_installation_sub_section -->

# Usage

## Getting started

1. Open GitHub Copilot Chat in VS Code and select **Agent** mode.
1. Run **MCP: List Servers** from the Command Palette, select **Fabric MCP Server**, and start it if needed.
1. Select **Configure Tools** in chat and enable the Fabric tools you want to use.
1. Ask a documentation question, such as `What Fabric item types are available?`

Documentation tools don't require Fabric authentication. Before using live operations, follow [Authentication for live operations](#authentication-for-live-operations). If the server doesn't connect, see the [troubleshooting guide](https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/TROUBLESHOOTING.md).

## What can you do with the Fabric MCP Server?

Use these prompts to explore the server's capabilities:

### Catalog discovery

* "Search the OneLake catalog for items related to 'sales revenue'"
* "Find a Lakehouse with 'customer' in the name across my workspaces"
* "Discover all Report items in the catalog and show which workspace they live in"
* "Filter the catalog to Warehouse and Notebook items"

### Fabric item types & APIs

* "What are the available Fabric item types I can work with?"
* "Show me the OpenAPI operations for 'notebook' and give a sample creation body"
* "Get the platform-level API specifications for Microsoft Fabric"
* "List all supported Fabric item types"

### Resource definitions & schemas

* "Explain the notebook item definition format and show an example"
* "Show me the definition format for a Data Pipeline item"
* "Generate a Semantic Model configuration with sample measures"
* "What properties are required for creating a KQL Database?"

### Best practices & examples

* "Show me best practices for handling API throttling in Fabric"
* "How should I implement retry logic for Fabric API rate limits?"
* "List recommended retry/backoff behavior for Fabric APIs when rate-limited"
* "Show me best practices for authenticating with Fabric APIs"
* "Get example request/response payloads for creating a Notebook"
* "What are the pagination patterns for Fabric REST APIs?"

### Development workflows

* "Generate a data pipeline configuration with sample data sources"
* "Help me scaffold a Fabric workspace with Lakehouse and notebooks"
* "Show me how to handle long-running operations in Fabric APIs"
* "What's the recommended error handling pattern for Fabric API calls?"

<!-- remove-section: start vsix remove_available_tools_section -->
## Available tools

The Fabric MCP Server includes API documentation, OneLake, core Fabric, and Data Factory tools. The following sections group them by capability.

### API documentation & best practices

| Tool Name | Description |
|-----------|-------------|
| `docs_list-item-types` | Lists the Fabric item types that have public API specifications available, plus the non-item API areas (platform, admin, spark, realTimeIntelligence). |
| `docs_item-api-spec` | Retrieves the complete OpenAPI specification for a specific Fabric item type. |
| `docs_platform-api-spec` | Retrieves the OpenAPI specification for core Fabric platform APIs. |
| `docs_item-definitions` | Retrieves item definition documentation for a Fabric item type. |
| `docs_best-practices` | Retrieves best practice documentation and guidance for a specific topic. |
| `docs_api-examples` | Retrieves example API request/response files for a specific item type. |

### OneLake data operations

| Tool Name | Description |
|-----------|-------------|
| `onelake_list-workspaces` | Lists available Microsoft Fabric workspaces. |
| `onelake_list-items` | Lists workspace items with high-level metadata. |
| `onelake_list-items-dfs` | Lists Fabric items via the DFS endpoint. |
| `onelake_list-files` | Lists files using the hierarchical file-list endpoint. |
| `onelake_download-file` | Downloads a OneLake file. |
| `onelake_upload-file` | Uploads a file to OneLake storage. |
| `onelake_delete-file` | Deletes a file from OneLake storage. |
| `onelake_create-directory` | Creates a directory via the DFS endpoint. |
| `onelake_delete-directory` | Deletes a directory (optionally recursive). |
| `onelake_get-table-config` | Retrieves table API configuration for a workspace item. |
| `onelake_list-table-namespaces` | Lists table namespaces (schemas) exposed through the table API. |
| `onelake_get-table-namespace` | Retrieves metadata for a specific namespace. |
| `onelake_list-tables` | Lists tables published within a namespace. |
| `onelake_get-table` | Retrieves the definition for a specific table. |

### OneLake security — Data access roles

| Tool Name | Description |
|-----------|-------------|
| `onelake_list-data-access-roles` | Lists all data access roles defined on a single item. |
| `onelake_get-data-access-role` | Gets the full definition of a single data access role (members, permissions, decision rules). |
| `onelake_create-or-update-data-access-role` | Upserts a single data access role on a single item. |
| `onelake_delete-data-access-role` | Deletes a single data access role from an item. |

### OneLake shortcuts

| Tool Name | Description |
|-----------|-------------|
| `onelake_list-shortcuts` | Lists shortcuts defined within an item. Hides DW-managed shortcuts by default (`--include-managed` to show). |
| `onelake_get-shortcut` | Gets the properties of a single shortcut. |
| `onelake_create-shortcut-onelake` | Creates a shortcut pointing to another OneLake location. |
| `onelake_create-shortcut-adls-gen2` | Creates a shortcut pointing to Azure Data Lake Storage Gen2. |
| `onelake_create-shortcut-amazon-s3` | Creates a shortcut pointing to Amazon S3. |
| `onelake_create-shortcut-azure-blob` | Creates a shortcut pointing to Azure Blob Storage. |
| `onelake_create-shortcut-gcs` | Creates a shortcut pointing to Google Cloud Storage. |
| `onelake_create-shortcut-s3-compatible` | Creates a shortcut pointing to S3-compatible storage. |
| `onelake_create-shortcut-dataverse` | Creates a shortcut pointing to a Dataverse environment. |
| `onelake_create-shortcut-onedrive-sharepoint` | Creates a shortcut pointing to OneDrive/SharePoint Online. |
| `onelake_delete-shortcut` | Deletes a single shortcut from an item (preserves destination data). |
| `onelake_reset-shortcut-cache` | Drops cached shortcut reads, forcing re-resolution from destination. |

### OneLake settings

| Tool Name | Description |
|-----------|-------------|
| `onelake_get-settings` | Gets OneLake settings for a workspace (diagnostics + immutability policy). |
| `onelake_modify-diagnostics` | Modifies diagnostic logging configuration (status, destination lakehouse) at workspace scope. |
| `onelake_modify-immutability-policy` | Modifies the workspace-level OneLake immutability policy (scope, retention days). |

### Core Fabric operations

| Tool Name | Description |
|-----------|-------------|
| `core_search-catalog` | Searches the OneLake catalog for items across workspaces by name, description, or workspace name. Optionally filter by item type. |
| `core_create-item` | Creates new Fabric items (Lakehouses, Notebooks, etc.). |

### Data Factory operations

| Tool Name | Description |
|-----------|-------------|
| `datafactory_list-pipelines` | Lists all pipelines in a Microsoft Fabric workspace. |
| `datafactory_create-pipeline` | Creates a new pipeline in a workspace. |
| `datafactory_get-pipeline` | Gets details of a specific pipeline. |
| `datafactory_run-pipeline` | Runs a pipeline on demand. |
| `datafactory_list-dataflows` | Lists all Dataflow Gen2 items in a workspace. |
| `datafactory_create-dataflow` | Creates a new Dataflow Gen2 item. |
| `datafactory_execute-query` | Executes an M (Power Query) query against a dataflow. |

> Always verify available commands via `--help`. Command names and availability may change between releases.
<!-- remove-section: end remove_available_tools_section -->

# Support and reference

## Documentation

- See the [Microsoft Fabric documentation](https://learn.microsoft.com/fabric/) to learn about the Microsoft Fabric platform.
- For MCP server-specific troubleshooting, see the [Troubleshooting Guide](https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/TROUBLESHOOTING.md).

## Feedback and support

- Support for this server implementation is primarily provided through community channels and GitHub repositories. Customers with qualifying Microsoft enterprise support agreements may have access to limited support for broader Microsoft Fabric and platform scenarios; review the [Microsoft Support Policy](https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/SUPPORT.md#microsoft-support-policy) section of this project for more details.
- Check the [Troubleshooting guide](https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/TROUBLESHOOTING.md) to diagnose and resolve common issues.
- We're building this in the open. Your feedback is much appreciated!
    - [Open an issue](https://github.com/microsoft/mcp/issues) in the public GitHub repository — we'd love to hear from you!

## Security

The Fabric MCP Server (local) is a **local-first** tool whose server process runs on your machine. API documentation and best-practices tools work offline without Fabric authentication. OneLake data operations, core Fabric operations, and Data Factory operations connect to your live Fabric environment using configured credentials.

Review the security of systems that integrate with MCP servers and confirm that they meet your organization's requirements. Include the Fabric MCP Server, MCP hosts, clients, agents, and model providers in that review.

Follow Microsoft security guidance for identity and access control, secure token management, and network security. See [Security in Microsoft Fabric](https://learn.microsoft.com/fabric/security/security-overview). Running the server locally doesn't mean that your MCP client or model processes data locally.

### Authentication for live operations

For OneLake and core Fabric tools, configure an Azure identity that the local server process can use. These tools use Azure Identity credentials, including an Azure CLI sign-in or a service principal configured through environment variables.

- **User identity**: Sign in with `az login` using an account that has permission to access the target Fabric resources.
- **Service principal**: Make `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, and `AZURE_CLIENT_CERTIFICATE_PATH` available to the server process for certificate authentication. For client-secret authentication, use `AZURE_CLIENT_SECRET` instead of the certificate path. See [EnvironmentCredential configuration](https://learn.microsoft.com/dotnet/api/azure.identity.environmentcredential) for details. Use your organization's approved credential storage and delivery mechanism. Don't put secrets in source control or agent prompts.

For Fabric REST API operations with a service principal, a Fabric administrator must enable the **Service principals can use Fabric APIs** tenant setting. Grant the service principal the permissions required by the target resources. Identity support varies by API and item type; see [Fabric REST API identity support](https://learn.microsoft.com/rest/api/fabric/articles/identity-support).

The identity selected by the configured credentials might differ from the person interacting with the MCP client. Confirm which identity the server uses before allowing access to live resources.

For Data Factory authentication, see the [Data Factory tools reference](https://github.com/microsoft/mcp/blob/main/tools/Fabric.Mcp.Tools.DataFactory/README.md#authentication).

### Permissions and risk

MCP clients can invoke operations based on the authenticated identity's (user or service principal) Fabric role-based access control (RBAC) permissions. Autonomous or misconfigured clients may perform destructive actions.

You should review and apply least-privilege RBAC roles and implement safeguards before deployment. Certain safeguards, such as flags to prevent destructive operations, are not standardized in the MCP specification and may not be supported by all clients.

Review agent-generated tool calls and your client's approval settings before allowing changes to live resources.

### Auditability

The local MCP server doesn't itself emit Fabric audit logs. Its diagnostic logs and telemetry aren't a replacement for a Fabric audit trail. Offline API documentation and best-practices calls don't create Fabric audit events.

Live operations may be recorded by the underlying Fabric service, depending on the operation and logging configuration. Don't assume that every MCP tool call appears in Fabric audit logs. For supported Fabric audit events and access requirements, see [Track user activities in Microsoft Fabric](https://learn.microsoft.com/fabric/admin/track-user-activities).

### Compliance responsibility

This MCP server may interact with clients and services that process data outside Microsoft Fabric's compliance boundaries. Data is processed in accordance with your chosen client's or service's applicable terms and data handling policies.

You are responsible for ensuring that any integration complies with your applicable organizational, regulatory, and contractual requirements.

## Third-party components

This MCP server may use or depend on third-party components. You are responsible for reviewing and complying with the licenses and security posture of any third-party components.

## Export control

Use of this software must comply with all applicable export laws and regulations, including U.S. Export Administration Regulations and local jurisdiction requirements.

## No warranty and limitation of liability

This software is provided "as is" without warranties or conditions of any kind, either express or implied. Microsoft shall not be liable for any damages arising from use, misuse, or misconfiguration of this software.

## Data collection

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

## Code of conduct
This project has adopted the
[Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information, see the
[Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [open@microsoft.com](mailto:open@microsoft.com)
with any additional questions or comments.

---

# License

This project is licensed under the MIT License — see the [LICENSE](https://github.com/microsoft/mcp/blob/main/LICENSE) file for details.
