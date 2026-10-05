<!--
See eng\scripts\Process-PackageReadMe.ps1 for instruction on how to annotate this README.md for package specific output
-->
# <!-- remove-section: start nuget;vsix remove_fabric_logo --><img height="36" width="36" src="https://learn.microsoft.com/fabric/media/fabric-icon.png" alt="Microsoft Fabric Logo" /> <!-- remove-section: end remove_fabric_logo -->Microsoft Fabric MCP Server <!-- insert-section: nuget;vsix;npm {{ToolTitle}} -->

<!-- insert-section: nuget;pypi {{MCPRepositoryMetadata}} -->

A local-first Model Context Protocol (MCP) server that provides AI agents with comprehensive access to Microsoft Fabric's public APIs, item definitions, and best practices. Documentation tools use bundled specifications without connecting to live Fabric environments; operational tools call Fabric APIs using the configured identity.
<!-- remove-section: start nuget;vsix;npm remove_install_links -->
[![Install Fabric MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_Fabric_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://marketplace.visualstudio.com/items?itemName=fabric.vscode-fabric-mcp-server) [![Install Fabric MCP in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Fabric_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect?url=vscode-insiders:extension/ms-fabric.vscode-fabric-mcp-server)

[![GitHub](https://img.shields.io/badge/github-microsoft/mcp-blue.svg?style=flat-square&logo=github&color=6e3fa3)](https://github.com/microsoft/mcp)
[![GitHub Release](https://img.shields.io/github/v/release/microsoft/mcp?include_prereleases&filter=Fabric.Mcp.*&style=flat-square&color=6e3fa3)](https://github.com/microsoft/mcp/releases?q=Fabric.Mcp.Server-)
[![License](https://img.shields.io/badge/license-MIT-green?style=flat-square&color=6e3fa3)](https://github.com/microsoft/mcp/blob/main/LICENSE)

<!-- remove-section: end remove_install_links -->
## Table of Contents
- [Overview](#overview)
- [Installation](#installation)
  - [IDE](#ide)
    - [VS Code (Recommended)](#vs-code-recommended)
    - [Manual Setup](#manual-setup)
- [Usage](#usage)
  - [Getting Started](#getting-started)
  - [What can you do with the Fabric MCP Server?](#what-can-you-do-with-the-fabric-mcp-server)
    - [Catalog Discovery](#catalog-discovery)
    - [Fabric Item Types & APIs](#fabric-item-types--apis)
    - [Resource Definitions & Schemas](#resource-definitions--schemas)
    - [Best Practices & Examples](#best-practices--examples)
    - [Development Workflows](#development-workflows)
  - [Workspace Metadata](#workspace-metadata)
  - [Available Tools](#available-tools)
    - [API Documentation & Best Practices](#api-documentation--best-practices)
    - [OneLake Data Operations](#onelake-data-operations)
    - [OneLake Security — Data Access Roles](#onelake-security--data-access-roles)
    - [OneLake Shortcuts](#onelake-shortcuts)
    - [OneLake Settings](#onelake-settings)
    - [Core Fabric Operations](#core-fabric-operations)
    - [Data Factory Operations](#data-factory-operations)
- [Support and Reference](#support-and-reference)
  - [Documentation](#documentation)
  - [Feedback and Support](#feedback-and-support)
  - [Security](#security)
  - [Data Collection](#data-collection)
  - [Contributing](#contributing)
  - [Code of Conduct](#code-of-conduct)
- [License](#license)

# Overview

**Microsoft Fabric MCP Server** gives your AI agents the knowledge they need to generate robust, production-ready code for Microsoft Fabric, alongside authenticated tools for working with Fabric resources.

Key capabilities:
- **Complete API Context**: Full OpenAPI specifications for all supported Fabric item types
- **Item Definition Knowledge**: JSON schemas for every Fabric item type (Lakehouses, pipelines, semantic models, notebooks, etc.)
- **Built-in Best Practices**: Embedded guidance on pagination, error handling, and recommended patterns
- **Local-First Documentation**: Documentation tools read bundled resources locally; operational tools access Fabric under the configured identity
- **Data Factory Integration**: Pipeline and Dataflow Gen2 management with M query execution

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
- You must have the latest [.NET 10 SDK LTS](https://dotnet.microsoft.com/download/dotnet) installed.
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
- To use Fabric MCP server from node one must have Node.js (LTS) installed and available on your system PATH — this provides both `npm` and `npx`. We recommend the latest Node.js LTS version. To verify your installation run: `node --version`, `npm --version`, and `npx --version`.
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
1. Try a prompt that uses Fabric context, such as `What Fabric item types are available?`
    - The agent should be able to use the Fabric MCP Server tools to complete your query
1. Check out the [Microsoft Fabric documentation](https://learn.microsoft.com/fabric/) and review the [troubleshooting guide](https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/TROUBLESHOOTING.md) for commonly asked questions
1. We're building this in the open. Your feedback is much appreciated!
    - [Open an issue in the public repository](https://github.com/microsoft/mcp/issues/new/choose)

## What can you do with the Fabric MCP Server?

The Fabric MCP Server supercharges your agents with Microsoft Fabric context. Here are some prompts you can try:

### Capacity Discovery

* "List Fabric capacities where I am an administrator or contributor"
* "Show the IDs, SKUs, regions, and states of my accessible Fabric capacities"
* "Get the next page of Fabric capacities using the continuation token from the previous response"

`core_list-capacities` returns exactly one page of capacity metadata in `capacities`, with `continuationToken` and `continuationUri` when supplied by Fabric. Pass the token unchanged through `--continuation-token` to request another page. Returned URIs are metadata only and are never followed; the tool does not retrieve all pages automatically.

The [Fabric List Capacities API](https://learn.microsoft.com/rest/api/fabric/core/capacities/list-capacities) lists capacities where the calling principal is an administrator or contributor. It supports users, service principals, and managed identities. Delegated calls require `Capacity.Read.All` or `Capacity.ReadWrite.All`; this is not an Azure subscription inventory and takes no subscription parameter.

### Catalog Discovery

* "Search the OneLake catalog for items related to 'sales revenue'"
* "Find a Lakehouse with 'customer' in the name across my workspaces"
* "Discover all Report items in the catalog and show which workspace they live in"
* "Filter the catalog to Warehouse and Notebook items"
* "List the Fabric workspaces I can access and show their capacity and domain metadata"
* "List my Fabric workspaces where I am an Admin or Member"
* "List accessible Fabric workspaces and include their workspace-specific API endpoints"
* "List item metadata in my Fabric workspace, including nested folders"
* "Show only the Lakehouses directly in this Fabric folder"

### Fabric Item Types & APIs

* "What are the available Fabric item types I can work with?"
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
* "Create a Microsoft Fabric workspace called Sales Planning"
* "Create a Fabric workspace called Capacity Analytics using the existing capacity with ID \<capacity-id>"
* "Show me how to handle long-running operations in Fabric APIs"
* "What's the recommended error handling pattern for Fabric API calls?"

## Workspace Metadata

Use `core_get-workspace` to retrieve metadata for one known workspace through the [Get Workspace REST API](https://learn.microsoft.com/rest/api/fabric/core/workspaces/get-workspace).

* "Get metadata for Fabric workspace `aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa`."
* "Show the capacity, domain, and workspace identity for Fabric workspace `aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa`."
* "Get Fabric workspace `aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa` with workspace-specific API and OneLake endpoints."

| Argument | Required | Description |
|----------|----------|-------------|
| `workspace-id` | Yes | Nonempty workspace UUID, not a name or URL. Accepted UUID representations are normalized before the request. |
| `prefer-workspace-specific-endpoints` | No | Boolean endpoint preference. Omit to preserve the service default, or explicitly pass `true` or `false`. |

```powershell
fabmcp core get-workspace --workspace-id aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa
fabmcp core get-workspace --workspace-id aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa --prefer-workspace-specific-endpoints true
```

The result contains a `workspace` object with `id`, `displayName`, and `type`. When returned by Fabric it also includes `description`, `capacityId`, `capacityAssignmentProgress`, `capacityRegion`, `domainId`, `workspaceIdentity`, `oneLakeEndpoints`, `apiEndpoint`, and applied `tags`. Missing/null optional fields are omitted; an explicitly empty tag list remains empty. New workspace types, capacity regions, and progress values are preserved.

Workspace identity fields contain application and service-principal IDs, not credentials. Returned endpoints are metadata only: this tool never contacts them. Workspaces with public access disabled may return workspace-specific OneLake endpoints even when the preference is omitted or false; an absent API endpoint is not inferred.

The tool does not list workspaces or items, read item data or definitions, modify resources, or poll capacity assignments. Errors retain their HTTP status without exposing backend bodies. A 429 includes validated `Retry-After` guidance when available, but the tool does not automatically retry.

**Permissions:** The caller needs Viewer-or-higher workspace access. Delegated callers also need `Workspace.Read.All` or `Workspace.ReadWrite.All`. The API supports users, service principals, and managed identities subject to their permissions and Fabric configuration. The tool preserves the server's configured credentials: stdio/hosting identity uses the host's credential provider, and HTTP/OBO uses the current caller's delegated context.

By default, the MCP text response contains the command envelope with `results.workspace`. With `--structured-output-mode compact` or `--structured-output-mode duplicated`, the server also advertises the typed output schema and returns `structuredContent.workspace`.

<!-- remove-section: start vsix remove_available_tools_section -->
## Available Tools

The Fabric MCP Server exposes tools organized into three categories:

### API Documentation & Best Practices

| Tool Name | Description |
|-----------|-------------|
| `docs_list-item-types` | Lists the Fabric item types that have public API specifications available, plus the non-item API areas (platform, admin, spark, realTimeIntelligence). |
| `docs_item-api-spec` | Retrieves the complete OpenAPI specification for a specific Fabric item type. |
| `docs_platform-api-spec` | Retrieves the OpenAPI specification for core Fabric platform APIs. |
| `docs_item-definitions` | Retrieves the JSON schema definition for a Fabric item type. |
| `docs_best-practices` | Retrieves best practice documentation and guidance for a specific topic. |
| `docs_api-examples` | Retrieves example API request/response files for a specific item type. |

### OneLake Data Operations

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

### OneLake Security — Data Access Roles

| Tool Name | Description |
|-----------|-------------|
| `onelake_list-data-access-roles` | Lists all data access roles defined on a single item. |
| `onelake_get-data-access-role` | Gets the full definition of a single data access role (members, permissions, decision rules). |
| `onelake_create-or-update-data-access-role` | Upserts a single data access role on a single item. |
| `onelake_delete-data-access-role` | Deletes a single data access role from an item. |

### OneLake Shortcuts

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

### OneLake Settings

| Tool Name | Description |
|-----------|-------------|
| `onelake_get-settings` | Gets OneLake settings for a workspace (diagnostics + immutability policy). |
| `onelake_modify-diagnostics` | Modifies diagnostic logging configuration (status, destination lakehouse) at workspace scope. |
| `onelake_modify-immutability-policy` | Modifies the workspace-level OneLake immutability policy (scope, retention days). |

### Core Fabric Operations

| Tool Name | Description |
|-----------|-------------|
| `core_create-item` | Creates new Fabric items (Lakehouses, Notebooks, etc.). |
| `core_create-workspace` | Creates a Fabric workspace, optionally assigning an existing capacity and domain in the same request. |
| `core_get-capacity` | Gets one Fabric capacity's ID, display name, SKU, region, and state using its capacity UUID. |
| `core_get-workspace` | Gets one workspace's metadata by UUID, including optional capacity, domain, identity, tags, and endpoints. Does not read item data or modify resources. |
| `core_list-capacities` | Lists one page of accessible Fabric capacity metadata: ID, display name, SKU, region, and state, plus available continuation information. |
| `core_list-items` | Lists one page of Fabric Core item metadata in a known workspace or folder, with optional type filtering and continuation information. |
| `core_list-workspaces` | Lists one page of accessible workspace management metadata, optionally filtered by the caller's workspace roles. Returns continuation information and can request workspace-specific API endpoints. |
| `core_search-catalog` | Searches the OneLake catalog for items across workspaces by name, description, or workspace name. Optionally filter by item type. |

**Get Capacity (`core_get-capacity`)**

Calls [Get Capacity](https://learn.microsoft.com/rest/api/fabric/core/capacities/get-capacity):
`GET https://api.fabric.microsoft.com/v1/capacities/{capacityId}`.
The required `capacity-id` argument (`--capacity-id` in the CLI) must be a nonempty UUID, not a capacity name or Azure resource ID. Invalid IDs are rejected before authentication or network access.

```powershell
fabmcp core get-capacity --capacity-id 96f3f0ff-4fe2-4712-b61b-05a456ba9357
```

The typed result contains only capacity metadata:

```json
{
  "capacity": {
    "id": "96f3f0ff-4fe2-4712-b61b-05a456ba9357",
    "displayName": "F4 Capacity",
    "sku": "F4",
    "region": "West Central US",
    "state": "Active"
  }
}
```

All five fields are required. SKU, region, and state remain strings so new service values are preserved. The tool does not list capacities, access ARM or billing, read item data, change capacity settings, assign workspaces, or start or poll long-running operations.

The caller needs **Administrator or Contributor** permission on the capacity. Delegated access requires **Capacity.Read.All** or **Capacity.ReadWrite.All**. The API documents support for users, service principals, and managed identities; this tool uses the server's configured credential without changing authentication behavior.

Failures preserve the service status with sanitized messages. For throttling, valid `Retry-After` guidance is returned without automatic retries. The operation expects a synchronous HTTP 200 response and rejects incomplete or invalid metadata instead of returning an empty success.

Example prompts: "Get metadata for Fabric capacity 96f3f0ff-4fe2-4712-b61b-05a456ba9357" or "Show the SKU, region, and state of Fabric capacity 96f3f0ff-4fe2-4712-b61b-05a456ba9357."

**Listing workspace management metadata**

`core_list-workspaces` calls the [Core List Workspaces REST API](https://learn.microsoft.com/rest/api/fabric/core/workspaces/list-workspaces). Use it to discover workspace IDs and inspect management metadata. Unlike `onelake_list-workspaces`, it does not use the OneLake storage/data-plane listing. Unlike `core_search-catalog`, it enumerates workspaces rather than searching catalog items. It does not read item data or definitions, modify resources, or perform long-running operations.

| Optional parameter | Behavior |
|---|---|
| `--roles` | Comma-separated Admin, Member, Contributor, or Viewer roles for the calling principal. Case and surrounding whitespace are normalized; duplicate roles are removed. Unknown roles and empty entries are rejected. Omit for no role filter. |
| `--continuation-token` | Token returned by the preceding page. Pass it unchanged, including any percent escapes, and repeat the same roles and endpoint preference. Omit for the first page. Empty or whitespace-only tokens are rejected. |
| `--prefer-workspace-specific-endpoints` | Set `true` to request each workspace's `apiEndpoint` metadata, or `false` to exclude it. Omission preserves the API default. Returned endpoints are not contacted by this tool. |

```powershell
fabmcp core list-workspaces
fabmcp core list-workspaces --roles Admin,Member --prefer-workspace-specific-endpoints true
fabmcp core list-workspaces --roles Admin,Member --prefer-workspace-specific-endpoints true --continuation-token '<token from the preceding page>'
```

Each call returns **one server-sized page** in `results.workspaces`, plus `results.continuationToken` and `results.continuationUri` when supplied by Fabric. Structured MCP output uses the same payload without the command envelope. An empty workspace array can still have continuation information; no continuation information means enumeration is complete. There is no page-size option or automatic all-pages fetch. `continuationUri` is informational only and is never followed.

Workspace entries contain `id`, `displayName`, and `type`, with `description`, `capacityId`, `capacityRegion`, `domainId`, `tags` (ID and display name), and `apiEndpoint` when available. Missing optional metadata is omitted rather than guessed or fetched separately; explicit empty descriptions and tag arrays are preserved. The role filter does not add a role field to the response. Requesting endpoint metadata does not configure private-link connectivity or change the listing endpoint.

The API supports users, service principals, and managed identities. Delegated access requires `Workspace.Read.All` or `Workspace.ReadWrite.All`; service principals and managed identities also require the Fabric tenant setting permitting service principals to use Fabric APIs. Results are limited to workspaces accessible to the configured calling identity, including the authenticated caller when using HTTP/OBO. The tool does not change authentication configuration.

Failures preserve the service's HTTP status without exposing raw backend error bodies. For HTTP 429, the tool returns parsed `Retry-After` guidance when available, or a generic wait-before-retrying message. It does not automatically retry or sleep.

**Workspace and folder inventory**

`core_list-items` calls the [Fabric Core List Items API](https://learn.microsoft.com/rest/api/fabric/core/items/list-items). Use it for a scoped metadata inventory, not for OneLake file/blob listing (`onelake_list-items`) or cross-workspace discovery (`core_search-catalog`).

| Argument | Required | Description |
|----------|----------|-------------|
| `workspace-id` | Yes | Nonempty workspace UUID; workspace names are not resolved. |
| `type` | No | Fabric item type, such as `Lakehouse` or `Notebook`. Omit for all types. New service-defined types are accepted without a client-side enum restriction. |
| `recursive` | No | Defaults to `true`, including nested folders. Set `false` for direct items only. |
| `root-folder-id` | No | Nonempty folder UUID. Omit to use the workspace root. |
| `continuation-token` | No | Token from a previous response, copied unchanged. Keep the same workspace and filters. |

```powershell
# First page of all workspace items, including nested folders.
fabmcp core list-items --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb

# Direct Lakehouses in one folder, excluding its nested folders.
fabmcp core list-items --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --root-folder-id bbbbbbbb-1111-2222-3333-cccccccccccc --type Lakehouse --recursive false
```

Each invocation returns exactly one page: an `items` array and any `continuationToken` and `continuationUri` supplied by Fabric. An empty array can still have a next page. Call again with `continuation-token` and unchanged filters to continue; this tool never fetches all pages automatically or follows the returned URI. Copy the token without decoding Base64 or changing existing percent escapes.

Items contain ID, display name, type, workspace ID, and available description, folder ID, logical ID, tags, and sensitivity-label ID. Optional null properties are omitted. No item data, definitions, or workload-specific properties are returned. The optional REST `include=DefaultIdentity` expansion is intentionally unsupported: it is neither requested nor serialized.

The caller needs workspace **Viewer** access. Delegated calls require `Workspace.Read.All` or `Workspace.ReadWrite.All`; the API supports user, service-principal, and managed identities. The tool preserves the server's configured authentication strategy, including its existing HTTP/OBO credential path. A `429` response retains its status and provides retry guidance when a valid `Retry-After` header is available; the tool does not retry automatically.

**Creating a workspace**

`core_create-workspace` implements [Create Workspace](https://learn.microsoft.com/rest/api/fabric/core/workspaces/create-workspace) with one `POST /v1/workspaces`, expecting a synchronous `201 Created` response.

| Parameter | Required | Behavior |
|-----------|----------|----------|
| `display-name` | Yes | Workspace display name, not empty or whitespace, at most 256 characters. Only unused names are allowed. `Admin monitoring` is reserved, including case variations and surrounding whitespace. |
| `description` | No | At most 4000 characters. An explicitly empty string is preserved. |
| `capacity-id` | No | Nonempty UUID of an existing capacity to assign during creation. Names are not resolved. |
| `domain-id` | No | Nonempty UUID of an existing domain to assign during creation. Names are not resolved. |

Omitted optional fields are not sent. The tool does not provision a capacity or domain, check name availability, call `assignToCapacity`, create items, or poll an operation.

These examples **create real workspaces** when run with authorized credentials. Replace the placeholders with existing resource IDs before using the second example:

```powershell
fabmcp core create-workspace --display-name "Sales Planning"

fabmcp core create-workspace --display-name "Capacity Analytics" `
  --description "Analytics workspace" `
  --capacity-id "<capacity-id>" `
  --domain-id "<domain-id>"
```

The typed result contains `workspace` and, when supplied by Fabric, `location`. Workspace metadata includes ID, display name, type, and any returned description, capacity ID/region, domain ID, API endpoint, and applied tags. Missing optional metadata remains absent; the result does not infer capacity-assignment completion. Location and API endpoints are metadata only and are never followed. A valid workspace body succeeds even if the Location header is absent.

**Permissions:** The caller needs [workspace-creation permission](https://learn.microsoft.com/fabric/admin/portal-workspace#create-workspaces) granted by a Fabric administrator. When assigning a capacity, the caller needs [capacity contributor or admin permission](https://learn.microsoft.com/fabric/admin/capacity-settings#details). Domain assignment also requires the relevant domain permission. Delegated callers need `Workspace.ReadWrite.All`. For service principals, the administrator must enable [Service principals can create workspaces, connections, and deployment pipelines](https://learn.microsoft.com/fabric/admin/service-admin-portal-developer#service-principals-can-create-workspaces-connections-and-deployment-pipelines) for an allowed security group containing the principal. The separate **Service principals can call Fabric public APIs** setting alone does not grant workspace-creation permission. Admin access on an existing workspace is not the creation prerequisite.

**Failure handling:** Creation is mutating and not idempotent. The tool never automatically retries, including after throttling, and preserves failure status with sanitized messages and valid Retry-After guidance. A timeout, cancellation, network failure, or invalid success response can leave the creation outcome uncertain. Check whether the workspace exists before making another creation request. The tool is unavailable in read-only server mode.

### Data Factory Operations

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

# Support and Reference

## Documentation

- See the [Microsoft Fabric documentation](https://learn.microsoft.com/fabric/) to learn about the Microsoft Fabric platform.
- For MCP server-specific troubleshooting, see the [Troubleshooting Guide](https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/TROUBLESHOOTING.md).

## Feedback and Support

- Support for this server implementation is primarily provided through community channels and GitHub repositories. Customers with qualifying Microsoft enterprise support agreements may have access to limited support for broader Microsoft Fabric and platform scenarios; review the [Microsoft Support Policy](https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/SUPPORT.md#microsoft-support-policy) section of this project for more details.
- Check the [Troubleshooting guide](https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/TROUBLESHOOTING.md) to diagnose and resolve common issues.
- We're building this in the open. Your feedback is much appreciated!
    - [Open an issue](https://github.com/microsoft/mcp/issues) in the public GitHub repository — we'd love to hear from you!

## Security

The Fabric MCP Server is a **local-first** tool. Its documentation tools provide API specifications, schemas, and best practices without connecting to live Fabric environments. Operational tools, including Get Workspace, make authenticated requests using the server's configured identity and permissions.

`core_create-workspace` creates a real workspace and can assign it to an existing capacity and domain; review those arguments and use an appropriately authorized identity before invoking it.

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
