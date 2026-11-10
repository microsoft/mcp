<!--
See eng\scripts\Process-PackageReadMe.ps1 for instruction on how to annotate this README.md for package specific output
cspell:words Retriable
-->
# <!-- remove-section: start nuget;vsix remove_fabric_logo --><img height="36" width="36" src="https://learn.microsoft.com/fabric/media/fabric-icon.png" alt="Microsoft Fabric Logo" /> <!-- remove-section: end remove_fabric_logo -->Microsoft Fabric MCP Server <!-- insert-section: nuget;vsix;npm {{ToolTitle}} -->

<!-- insert-section: nuget;pypi {{MCPRepositoryMetadata}} -->

A local-first Model Context Protocol (MCP) server that provides AI agents with comprehensive access to Microsoft Fabric's public APIs, item definitions, and best practices. Documentation tools use bundled specifications without connecting to live Fabric environments; operational tools call Fabric APIs using the configured identity.
<!-- remove-section: start nuget;vsix;npm remove_install_links -->
[![Install Fabric MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_Fabric_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://marketplace.visualstudio.com/items?itemName=fabric.vscode-fabric-mcp-server) [![Install Fabric MCP in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Fabric_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect?url=vscode-insiders:extension/fabric.vscode-fabric-mcp-server)

[![GitHub](https://img.shields.io/badge/github-microsoft/mcp-blue.svg?style=flat-square&logo=github&color=6e3fa3)](https://github.com/microsoft/mcp)
[![GitHub Release](https://img.shields.io/github/v/release/microsoft/mcp?include_prereleases&filter=Fabric.Mcp.*&style=flat-square&color=6e3fa3)](https://github.com/microsoft/mcp/releases?q=Fabric.Mcp.Server-)
[![License](https://img.shields.io/badge/license-MIT-green?style=flat-square&color=6e3fa3)](https://github.com/microsoft/mcp/blob/main/LICENSE)

<!-- remove-section: end remove_install_links -->
## Table of Contents
- [Overview](#overview)
- [Installation](#installation)<!-- remove-section: start nuget;vsix;npm remove_ide_toc -->
    - [IDE](#ide)
        - [VS Code (Recommended)](#vs-code-recommended)
        - [Manual Setup](#manual-setup)<!-- remove-section: end remove_ide_toc -->
    - [Docker](#docker)
- [Usage](#usage)
    - [Getting Started](#getting-started)
    - [What can you do with the Fabric MCP Server?](#what-can-you-do-with-the-fabric-mcp-server)
        - [Capacity Discovery](#capacity-discovery)
        - [Catalog Discovery](#catalog-discovery)
        - [Fabric Item Types & APIs](#fabric-item-types--apis)
        - [Resource Definitions & Schemas](#resource-definitions--schemas)
        - [Best Practices & Examples](#best-practices--examples)
        - [Development Workflows](#development-workflows)
    - [Workspace Metadata](#workspace-metadata)
    - [Available Tools](#available-tools)
        - [Namespace Filtering](#namespace-filtering)
        - [API Documentation & Best Practices](#api-documentation--best-practices)
        - [OneLake Data Operations](#onelake-data-operations)
        - [OneLake Security — Data Access Roles](#onelake-security--data-access-roles)
        - [OneLake Shortcuts](#onelake-shortcuts)
        - [OneLake Settings](#onelake-settings)
        - [Core Fabric Operations](#core-fabric-operations)
        - [Jobs](#jobs)
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
- **Workspace Metadata Updates**: Rename a known workspace or update or clear its description without changing other properties
- **Data Factory Integration**: Pipeline and Dataflow Gen2 management with M query execution

# Installation

The manual configuration examples use `mcpServers` as the root object. In Visual Studio and Visual Studio Code, use `servers` instead.

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

When using the VS Code extension, `fabricMcp.enabledServices` selects from `core`, `jobs`, `docs`, `onelake`, and `datafactory`. Leave the list empty to enable all toolsets. Select both `core` and `jobs` for workflows combining item or definition operations with job submission, cancellation, instance listing/lookup, or schedule listing/creation/updates; selecting only `core` does not expose Jobs. If an existing setting contains `publicapis`, replace it with `docs` or clear the list; saved settings are not migrated automatically.

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
#### Option 1: Configure using the .NET tool (NuGet)<!-- remove-section: end remove_dotnet_config_sub_header -->
- Install the [.NET 11 SDK or later](https://dotnet.microsoft.com/download/dotnet/11.0). Verify it with `dotnet --version`.
- Install the published [Microsoft.Fabric.Mcp](https://www.nuget.org/packages/Microsoft.Fabric.Mcp) tool:

    ```powershell
    dotnet tool install --global Microsoft.Fabric.Mcp --source https://api.nuget.org/v3/index.json
    ```

- Ensure the .NET global tools directory is on the `PATH` available to your MCP client, then configure `mcp.json`:

    ```json
    {
        "mcpServers": {
            "Fabric MCP Server": {
                "command": "fabmcp",
                "args": [
                    "server",
                    "start",
                    "--mode",
                    "all"
                ],
                "type": "stdio"
            }
        }
    }
    ```

Alternatively, .NET 11 supports running the published package on demand with [`dnx`](https://learn.microsoft.com/dotnet/core/tools/dotnet-tool-exec), without a permanent tool installation:

```powershell
dnx Microsoft.Fabric.Mcp --source https://api.nuget.org/v3/index.json -- server start --mode all
```

Both commands use the latest published stable package unless you specify a version. To pin a released version, use `--version <version>` with `dotnet tool install`, or `Microsoft.Fabric.Mcp@<version>` with `dnx`.
<!-- remove-section: end remove_dotnet_config_sub_section -->
<!-- remove-section: start nuget remove_node_config_sub_section -->
<!-- remove-section: start npm remove_node_config_sub_header -->
#### Option 2: Configure using Node.js (npm/npx)<!-- remove-section: end remove_node_config_sub_header -->
- Install a supported Node.js LTS version **22.0.0 or later**, with `npm` and `npx` available on your system `PATH`. Verify your installation with `node --version`, `npm --version`, and `npx --version`.
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
<!-- remove-section: start nuget;npm remove_source_build_sub_section -->

#### Build from source (contributors)

Building the repository is a separate development workflow, not required to use the published packages:

```bash
git clone https://github.com/microsoft/mcp.git
cd mcp
dotnet build servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj --configuration Release
```

Use the built `servers/Fabric.Mcp.Server/src/bin/Release/fabmcp` executable as your MCP client's `command`, with `["server", "start", "--mode", "all"]` as its arguments. On Windows, use backslashes and the `.exe` extension.
<!-- remove-section: end remove_source_build_sub_section -->

**Client-Specific Configuration**
| IDE | File Location | Documentation Link |
|-----|---------------|-------------------|
| **Claude Code** | `~/.claude.json` or `.mcp.json` (project) | [Claude Code MCP Configuration](https://scottspence.com/posts/configuring-mcp-tools-in-claude-code) |
| **Claude Desktop** | `~/.claude/claude_desktop_config.json` (macOS)<br>`%APPDATA%\Claude\claude_desktop_config.json` (Windows) | [Claude Desktop MCP Setup](https://support.claude.com/en/articles/10949351-getting-started-with-local-mcp-servers-on-claude-desktop) |
| **Cursor** | `~/.cursor/mcp.json` or `.cursor/mcp.json` | [Cursor MCP Documentation](https://docs.cursor.com/context/model-context-protocol) |
| **VS Code** | `.vscode/mcp.json` (workspace)<br>`settings.json` (user) | [VS Code MCP Documentation](https://code.visualstudio.com/docs/copilot/chat/mcp-servers) |
| **Windsurf** | `~/.codeium/windsurf/mcp_config.json` | [Windsurf Cascade MCP Integration](https://docs.windsurf.com/windsurf/cascade/mcp) |
<!-- remove-section: start nuget;npm remove_closing_details -->
</details>
<!-- remove-section: end remove_closing_details -->
<!-- remove-section: end remove_entire_installation_sub_section -->

## Docker

The published container image is available from [Microsoft Container Registry](https://mcr.microsoft.com/artifact/mar/fabric/fabric-mcp) as `mcr.microsoft.com/fabric/fabric-mcp`. Use Docker with Linux containers. The `latest` tag selects the most recently published image; when pinning a version, choose a tag that already exists in the registry.

The container does **not** inherit your host's `az login` session. For operational tools, configure an identity supported by the Fabric APIs you intend to call and grant it the required Fabric permissions. For example, supply a service principal using a local environment file stored outside your repository:

```text
AZURE_TENANT_ID=<tenant-id>
AZURE_CLIENT_ID=<client-id>
AZURE_CLIENT_SECRET=<client-secret>
AZURE_TOKEN_CREDENTIALS=EnvironmentCredential
```

Never commit this file or put its secret values in `mcp.json`. See [environment credential options](https://learn.microsoft.com/dotnet/api/azure.identity.environmentcredential) for other supported credential settings.

Configure your MCP client, replacing the environment-file placeholder with an absolute path:

```json
{
    "mcpServers": {
        "fabric-mcp-server": {
            "command": "docker",
            "args": [
                "run",
                "--rm",
                "-i",
                "--env-file",
                "<absolute-path-to-env-file>",
                "mcr.microsoft.com/fabric/fabric-mcp:latest",
                "--mode",
                "all"
            ]
        }
    }
}
```

Keep stdin open with `-i`, without allocating a TTY. The image entrypoint already runs `server start`; do not repeat those arguments after the image name.

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

* "Get the metadata for Fabric item '<item-id>' in workspace '<workspace-id>' without reading its data or definition"
* "Show the display name, description, and type of Fabric item '<item-id>' in workspace '<workspace-id>'"
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

The result contains a `workspace` object with `id` and `displayName`. When returned by Fabric it also includes `type`, `description`, `capacityId`, `capacityAssignmentProgress`, `capacityRegion`, `domainId`, `workspaceIdentity`, `oneLakeEndpoints`, `apiEndpoint`, and applied `tags`. Missing/null optional fields are omitted; an explicitly empty tag list remains empty. New workspace types, capacity regions, and progress values are preserved.

Workspace identity fields contain application and service-principal IDs, not credentials. Returned endpoints are metadata only: this tool never contacts them. Workspaces with public access disabled may return workspace-specific OneLake endpoints even when the preference is omitted or false; an absent API endpoint is not inferred.

The tool does not list workspaces or items, read item data or definitions, modify resources, or poll capacity assignments. Errors retain their HTTP status without exposing backend bodies. A 429 includes validated `Retry-After` guidance when available, but the tool does not automatically retry.

**Permissions:** The caller needs Viewer-or-higher workspace access. Delegated callers also need `Workspace.Read.All` or `Workspace.ReadWrite.All`. The API supports users, service principals, and managed identities subject to their permissions and Fabric configuration. The tool preserves the server's configured credentials: stdio/hosting identity uses the host's credential provider, and HTTP/OBO uses the current caller's delegated context.

By default, the MCP text response contains the command envelope with `results.workspace`. With `--structured-output-mode compact` or `--structured-output-mode duplicated`, the server also advertises the typed output schema and returns `structuredContent.workspace`.

## Available Tools

The Fabric MCP Server exposes `core`, `jobs`, `docs`, `onelake`, and `datafactory` areas.

### Namespace Filtering

`core` and `jobs` are separate top-level areas in the same Fabric MCP server. Core manages workspaces,
items, definitions, capacities, and Fabric `/operations` LROs. It also supports logical workspace
folder listing, creation, lookup, renaming, movement, and empty-folder deletion.
Core also lists one page of workspace role assignments or gets one assignment by explicit workspace
and assignment IDs, returning available principal metadata without directory lookups, group-membership
expansion, or role changes. Each additional page or assignment lookup is a separate explicit call.
Add Workspace Role Assignment is a separate mutation requiring an explicit principal and role.
It sends one grant request without default access, name resolution, preflight, or conflict updates.
Update Workspace Role Assignment separately replaces the role on an explicitly identified existing
assignment with one PATCH. It requires Admin access and an explicit role, without identity translation,
preflight, create/upsert fallback, automatic retry, or polling.
Delete Workspace Role Assignment removes only an explicitly identified assignment with one DELETE,
not the principal, group membership, workspace, or all sources of effective access. It requires Admin
access, protects the last admin assignment, and returns a receipt only for HTTP 200 without preflight,
automatic retry, polling, or verification.
The separate Items Move operation places an item and its child items in an existing folder or
at the root of the same workspace, returning the complete moved-item metadata collection.
Jobs submits an on-demand item job,
requests cancellation of an existing job, reads one job instance, lists one page of an item's jobs
or schedules, or reads, creates, updates, or deletes one schedule in separate calls. Each listing requires an explicit continuation
token for each additional page. Schedule operations require an explicit job type.
Schedule lookup, updates, and deletion require a schedule ID; creation and updates also require explicit
enabled/timing/timezone intent and full configuration. Deletion returns a receipt, not a schedule snapshot,
and does not cancel already-running jobs.
Both areas reuse the same configured Fabric authentication and HTTP transport.

```powershell
# Core-only: workspace management and role assignments, folders, item/definition, capacity, and operation tools, but no Jobs commands.
fabmcp server start --mode all --namespace core

# Jobs-only: job submission, cancellation, instance listing/lookup, and schedule lookup/listing/creation/updates/deletion.
fabmcp server start --mode all --namespace jobs

# Item/definition work plus job and schedule operations: enable both areas.
fabmcp server start --mode all --namespace core --namespace jobs

# The same combined selection with hierarchical namespace routers or a single router.
fabmcp server start --mode namespace --namespace core --namespace jobs
fabmcp server start --mode single --namespace core --namespace jobs
```

Omit `--namespace` to enable all registered areas. Filtering restricts both discovery and invocation,
not the shared services needed by the selected area. In `all` mode, the tools are exposed directly as
`jobs_cancel-item-job-instance`, `jobs_create-item-schedule`, `jobs_delete-item-schedule`, `jobs_get-item-job-instance`, `jobs_get-item-schedule`,
`jobs_list-item-job-instances`, `jobs_list-item-schedules`, `jobs_run-on-demand-item-job`, and `jobs_update-item-schedule`.
In `namespace` mode, call the `jobs`
router with `learn=true` to obtain their schemas, then specify the command name and its `parameters`. In `single` mode,
use the `fabric` router with `tool="jobs"` and the same command/parameters; `learn=true` lists areas or
the commands for a selected area. Core-only selection never exposes or routes Jobs tools.
Read-only mode hides and denies Run, Cancel, Create Schedule, Update Schedule, and Delete Schedule while retaining Get Job Instance,
Get Schedule, and both listing tools. Schedule lookup and listing retain the host's sensitive-data confirmation
because their output may include configuration, owner information, and job execution data.
Read-only Core retains List/Get Workspace Role Assignment but hides and denies Add Workspace Role Assignment.

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

The configured Fabric Core HTTP client does not automatically follow redirects. This policy applies to all Core operations, including existing create/search and update calls as well as deletion: a returned 3xx response is surfaced as a failure instead of following its Location. Existing configured handlers, proxy settings, timeouts, credentials, and recording wrappers are retained. Workspace `type` is optional in Get, List, Create, and Update responses; omitted/null values are not invented, while present nonempty future type strings are preserved.

| Tool Name | Description |
|-----------|-------------|
| `core_add-workspace-role-assignment` | Adds one explicitly supplied principal and workspace role with one POST. Returns created assignment metadata without identity lookups, default access, conflict updates, retries, or polling. |
| `core_assign-workspace-to-capacity` | Submits one request to assign an existing workspace to a capacity. Returns an accepted/pending receipt, not confirmation of completion. |
| `core_create-folder` | Creates one logical workspace folder at the root or under an explicit parent folder UUID. Returns created metadata without lookups, retries, or polling. |
| `core_create-item` | Creates a Fabric item by workspace UUID, optionally with a definition or creation payload, an existing folder ID, definition options, and label settings. Returns completed metadata or an accepted operation receipt; optionally waits within a bounded budget. |
| `core_create-workspace` | Creates a Fabric workspace, optionally assigning an existing capacity and domain in the same request. |
| `core_delete-folder` | Deletes one explicitly identified empty logical workspace folder. Returns a deletion receipt without changing contents, preflight, retries, or polling. |
| `core_delete-item` | Deletes one known Fabric item using workspace and item UUIDs. Permanent deletion requires explicitly setting `hard-delete` to `true`. |
| `core_delete-workspace` | Deletes one explicitly identified Fabric workspace **and the items under it**. Requires a workspace UUID and workspace Admin access. |
| `core_delete-workspace-role-assignment` | Deletes one existing assignment by workspace and assignment UUIDs. Requires Admin access; returns a deletion receipt without identity lookup, cascading removal, retry, or polling. |
| `core_get-capacity` | Gets one Fabric capacity's ID, display name, SKU, region, and state using its capacity UUID. |
| `core_get-folder` | Gets one logical workspace folder's metadata and available parent relationship using explicit workspace and folder UUIDs, without related lookups. |
| `core_get-item` | Gets generic metadata for one existing Fabric item using its workspace ID and item ID. Does not read item data or definitions. |
| `core_get-item-definition` | Retrieves typed definition parts inline, optionally waits for completion, or resumes a completed definition operation. Can explicitly export to a new local directory without overwriting files. |
| `core_get-operation-result` | Reads one completed operation's JSON or empty result, up to 1 MiB. Binary results are unsupported. |
| `core_get-operation-state` | Reads one operation's current state and polling guidance, without polling or resubmitting the original request. |
| `core_get-workspace` | Gets one workspace's metadata by UUID, including optional capacity, domain, identity, tags, and endpoints. Does not read item data or modify resources. |
| `core_get-workspace-role-assignment` | Gets one role assignment and available principal metadata using explicit workspace and assignment UUIDs, without listing assignments, identity lookups, or role changes. |
| `core_list-capacities` | Lists one page of accessible Fabric capacity metadata: ID, display name, SKU, region, and state, plus available continuation information. |
| `core_list-folders` | Lists one complete page of logical workspace folders, with optional root folder and recursive filters, parent relationships, and explicit continuation. |
| `core_list-items` | Lists one page of Fabric Core item metadata in a known workspace or folder, with optional type filtering and continuation information. |
| `core_list-workspace-role-assignments` | Lists one complete page of role assignments and available principal metadata for an explicit workspace UUID, with explicit continuation and no identity enrichment or role changes. |
| `core_list-workspaces` | Lists one page of accessible workspace management metadata, optionally filtered by the caller's workspace roles. Returns continuation information and can request workspace-specific API endpoints. |
| `core_move-folder` | Moves one known logical folder and its hierarchy within the same workspace, into an existing folder or to the workspace root. Returns observed metadata without lookups, descendant enumeration, retries, or polling. |
| `core_move-item` | Moves one known item and its child items into an existing folder or to the root of the same workspace. Returns the complete ordered metadata collection without lookups, retries, or polling. |
| `core_search-catalog` | Searches one page of OneLake catalog metadata across workspaces. Continue with the returned token without repeating search or filter. |
| `core_update-folder` | Renames one known logical workspace folder with an explicit replacement display name. Returns updated metadata without moving the folder, lookups, retries, or polling. |
| `core_update-item` | Updates an existing item's display name or description and returns metadata only. |
| `core_update-item-definition` | Replaces a known item's complete definition from inline content or explicitly selected saved files. Optionally applies supplied `.platform` metadata and waits for a no-result operation. |
| `core_update-workspace` | Renames a workspace or updates or clears its description by UUID, leaving omitted properties unchanged. Requires workspace Admin access. |
| `core_update-workspace-role-assignment` | Updates one existing assignment by workspace and assignment UUIDs with an explicit replacement role and one PATCH. Requires Admin access; no identity lookup, creation fallback, retry, or polling. |

**Create Item (`core_create-item`)**

Calls the [Create Item API](https://learn.microsoft.com/rest/api/fabric/core/items/create-item) once with `display-name`, `item-type`, and the optional request properties described below. Supply a nonempty workspace UUID using `workspace-id` or the backward-compatible `workspace` alias. Workspace names are not resolved. A `workspace-id` that is not empty or whitespace takes precedence when both options are supplied, even if it is invalid; the command does not silently fall back to the alias. Invalid and all-zero UUIDs are rejected before authentication or HTTP, and accepted UUID formats are normalized in the request URL.

Missing required options are identified by their registered names, such as `--display-name` or `--item-type`, before any service call. Other parser failures return sanitized guidance without echoing supplied values or unknown option names.

This example **creates a real item** when run with authorized credentials:

```powershell
fabmcp core create-item --workspace-id cfafbeb1-8037-4d0c-896e-a46fb27ff229 --display-name "Sales" --item-type Lakehouse
```

The caller needs **Contributor or higher workspace access**. Delegated calls require `Item.ReadWrite.All` or the item-specific `ReadWrite.All` scope. Non-Power BI items require a supported Fabric capacity and [tenant/capacity settings that enable Fabric item creation](https://learn.microsoft.com/fabric/admin/fabric-switch); Power BI items require the appropriate license. Service-principal and managed-identity support depends on the item type. Item types remain strings rather than a fixed client-side list.

Display names must follow the item's naming rules, and descriptions allow at most 256 characters. Omitting the optional properties preserves the existing no-definition request; the tool never invents request fields or defaults.

**Optional creation properties**

All four options below are available locally and over HTTP:

| Argument | Request property | Behavior |
|----------|------------------|----------|
| `folder-id` | `folderId` | Nonempty UUID of an existing folder in this workspace. Passed directly and normalized as a UUID, without a lookup or folder creation. Omit to use the workspace root. |
| `creation-payload` | `creationPayload` | Item-specific properties as a JSON **object string**, not a file path or a full create-request body. Mutually exclusive with **every** inline or saved-file definition option. Consult the item's Create API for supported properties. |
| `options` | `options` | Item-specific settings as a JSON **object string**. Requires a definition source; cannot accompany creation payload alone. Consult the item's Update Definition API for supported options. This is the request's top-level `options`, **not** LRO wait settings. |
| `sensitivity-label-settings` | `sensitivityLabelSettings` | Optional typed JSON object string. When supplied, `labelId` is required and must be a nonempty UUID. `sensitivityLabelApplyStrategy` is optional; a supplied value must be exactly `ApplyOrFail` or `Ignore`. |

`creation-payload` and `options` each allow at most **1 MiB (1,048,576 UTF-8 bytes)** and **32 JSON nesting levels**. `sensitivity-label-settings` allows at most **4,096 UTF-8 bytes**. These are host input limits, not Fabric API quotas. Non-object JSON, malformed input, duplicate properties (including nested properties), and repeated occurrences of these CLI options are rejected before reading definition files or contacting Fabric. Workload property names and values, including explicit nulls, false values, and empty objects, are otherwise passed through unchanged; Fabric validates their per-item meaning. Unknown label-setting fields are errors rather than silently ignored.

When the whole label-settings object or its optional strategy is omitted, it stays omitted. A supplied object must contain a nonempty `labelId`; `{}` and strategy-only settings are rejected before file access, authentication, or the creation request. Fabric's documented default for an omitted strategy is **`ApplyOrFail`**; the tool does not insert a value. Explicit **`Ignore`** permits Fabric to proceed without applying the label if it cannot be applied, so select it only when that policy is intended. Explicit null/invalid IDs or strategies are rejected; omit the entire settings object when no label is requested. A label-related failure never causes a retry without the label, a fallback to `Ignore`, or another creation request.

For example, the documented [Lakehouse creation payload](https://learn.microsoft.com/rest/api/fabric/lakehouse/items/create-lakehouse) can enable schemas. This example creates a real item in an existing folder and requests a label with the strict policy:

```powershell
fabmcp core create-item --workspace-id cfafbeb1-8037-4d0c-896e-a46fb27ff229 --display-name "Sales" --item-type Lakehouse `
  --folder-id bbbbbbbb-1111-2222-3333-cccccccccccc `
  --creation-payload '{"enableSchemas":true}' `
  --sensitivity-label-settings '{"labelId":"b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f","sensitivityLabelApplyStrategy":"ApplyOrFail"}'
```

For definition-based creation, pass supported item-specific settings through `--options` alongside either definition source below. Keep `sync`, `max-wait-seconds`, and `early-poll` as separate tool arguments. Folder IDs, labels, and workload properties do not trigger resource lookups, preflight calls, or metadata read-backs.

[//]: # (cspell:words nbformat pbism pbir TMDL TMSL )

**Optional item definition input**

Choose exactly one source when supplying a definition:

| Source | Arguments | Availability |
|--------|-----------|--------------|
| Inline content | `definition`: a JSON **string** containing `parts` and optional `format`, not a path or a complete create-request body | Local CLI, stdio, and HTTP |
| Saved files | `definition-directory`: an existing absolute local directory; `definition-files`: a JSON **string** containing an explicit array of `{ "partPath": "...", "sourcePath": "..." }`; optional `definition-format` | Local CLI and stdio only |

Every inline part must supply `path`, Base64 `payload`, and `payloadType: "InlineBase64"`. `definition-format` applies only to saved files; for inline content put `format` inside the JSON. Neither source can be combined with `creation-payload`. Conflicting sources, incomplete selections, repeated definition options, malformed JSON, unknown definition-envelope fields, and duplicate JSON fields are errors, never silently preferred or dropped. Over HTTP, file options are omitted from discovery and rejected on invocation; the Fabric input service also checks its local-file capability before any filesystem access. The existing remote-capable tool remains available for inline, creation-payload, and no-definition creation.

For a saved notebook, first save changes in your editor. This example reads only `sales.ipynb` from the selected directory and maps it to the Fabric part path without renaming the source file:

```powershell
fabmcp core create-item --workspace-id cfafbeb1-8037-4d0c-896e-a46fb27ff229 --display-name "Sales notebook" --item-type Notebook `
  --definition-directory 'C:\Fabric\Sales.Notebook' `
  --definition-files '[{"partPath":"artifact.content.ipynb","sourcePath":"sales.ipynb"}]' `
  --definition-format ipynb
```

`sourcePath` defaults to `partPath`. Both are relative to the selected directory; logical `partPath` values use `/` separators. List every part you intend to upload, including nested model/report parts, binary assets, and optional `.platform` where appropriate. There is **no recursive scan, wildcard expansion, or automatic inclusion** of editor state, caches, secrets, or even `.platform`. Explicitly selected files are uploaded, so review that selection. Files come from the MCP host, not a remote client's filesystem, Git HEAD, or unsaved editor buffers. Save and stop editing while packaging; this is not an atomic snapshot of concurrent edits.

For content already supplied directly, including to a remote HTTP server:

```powershell
$definition = @{
  format = 'ipynb'
  parts = @(@{
    path = 'artifact.content.ipynb'
    payload = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes('{"nbformat":4,"nbformat_minor":5,"cells":[],"metadata":{}}'))
    payloadType = 'InlineBase64'
  })
} | ConvertTo-Json -Depth 4 -Compress
fabmcp core create-item --workspace-id cfafbeb1-8037-4d0c-896e-a46fb27ff229 --display-name "Empty notebook" --item-type Notebook --definition $definition
```

These are alternative **real item creation** examples, not validation-only operations. Definition preparation performs no Fabric requests; after preparation there is still only one initiating `POST /workspaces/{workspaceId}/items`. There are no workspace/name lookups, dependency creation, preflight calls, or metadata read-backs. The existing `sync`/polling options below work with either source.

Both sources share host packaging limits: by default **1,024 parts, 16 MiB decoded per part, and 32 MiB decoded total**, plus encoded-payload and input-JSON bounds. These are local safeguards, not Fabric API quotas. Portable logical paths reject traversal, absolute paths, duplicate names (case-insensitively), and file/directory collisions. Saved-file input additionally rejects linked/junction paths and UNC/device paths. Selected bytes, encodings, line endings, and empty files are preserved. Filesystem checks require a trusted local filesystem; they are not an OS sandbox against concurrent hostile file replacement.

**Fabric validates item-specific layouts and content.** The tool does not require particular filenames, notebook language files, or format-specific part combinations, even for recognized formats or when `format` is omitted. Safe part names, supplied format strings, and payload content are forwarded unchanged; an omitted format stays omitted so Fabric applies its defaults. The generic JSON/Base64/envelope checks, byte/count/depth limits, API/source exclusivity, UUID checks, and filesystem/HTTP safeguards above still apply before submission.

Use the bundled [item-definition articles](https://github.com/microsoft/mcp/tree/main/tools/Fabric.Mcp.Tools.Docs/src/Resources/item-definitions) through `docs_item-definitions` to prepare a complete definition. They are documentation for callers, not executable schemas or a local layout-validation policy. Fabric remains authoritative for supported item types/formats, required parts, content schemas, format combinations, and dependency/binding correctness. The tool does not rewrite report bindings, interpret `.platform`, or create referenced resources.

Create Item supports both outcomes documented by its endpoint: **201** returns validated metadata in the existing `results.item` shape; **202** returns an explicit `results.operation` receipt, never an empty/default item. Both modes return a synchronous 201 result immediately, without operation reads or requiring an operation ID. Other success codes, including 200 and 204, are not part of this endpoint's contract: they produce a sanitized unconfirmed-outcome error with guidance to check whether creation occurred. The shared LRO infrastructure also supports synchronous 200 for future adapters whose endpoints document it; it does not accept arbitrary success codes.

| Optional argument | Default | Behavior |
|-------------------|---------|----------|
| `sync` | `false` | False sends one POST and returns completed metadata or an accepted receipt, with zero follow-up reads. True asynchronously waits after 202 and retrieves the created item when available. It does not use blocking HTTP I/O. |
| `max-wait-seconds` | `120` | Finite positive local wait budget after 202, up to **600 seconds (10 minutes)** inclusive. Fractional seconds are allowed. Includes delays, authentication, and in-flight status/result reads. Larger values are rejected, not clamped, before submitting creation, even when `sync=false`. |
| `early-poll` | `true` | Allows one first status probe after at most 3 seconds, or sooner if the initial server hint is shorter. False honors the initial validated `Retry-After` instead. |

Choose the mode **before the single creation request**. This is an alternative to the earlier creation example, not a way to resume an already accepted creation:

```powershell
fabmcp core create-item --workspace-id cfafbeb1-8037-4d0c-896e-a46fb27ff229 --display-name "Sales" --item-type Lakehouse --sync true --max-wait-seconds 120 --early-poll false
```

The early first probe is a deliberate exception to the **initial successful 202** `Retry-After` guidance (often 20 seconds); it is not a claim that every server hint is honored. The receipt keeps `serverRetryAfterSeconds` separate from `recommendedPollAfterSeconds`. After that first probe, subsequent polls honor validated returned hints. Only safe operation GETs can be retried on HTTP 429/503, within the remaining budget; those hints are never shortened by the early-probe exception. Missing, malformed, multiple, negative, or overflowing hints use a conservative 20-second fallback. Valid HTTP-date hints are supported; zero/past hints have a one-second scheduling floor to prevent a tight loop.

The 600-second maximum is a server-enforced operational guard on **MCP waiting**, not a Fabric API requirement or a limit on how long the Fabric operation may run. The shared service validation enforces it even when invoked without the command binding layer.

Creation is **never automatically resubmitted**. Stopping MCP waiting does not cancel Fabric's operation, and no detached job continues polling. Client cancellation or a shorter HTTP/client timeout can end the wait sooner than the configured budget. A local budget expiry is not a failed mutation: resume using the operation ID, rather than increasing the budget by issuing another create call.

| Outcome | Command status and payload |
|---------|----------------------------|
| Immediate 201 completion | HTTP 200 command response with `results.item`, preserving the existing completed shape. |
| Accepted or wait budget expired while unconfirmed | HTTP 202 with `operation.status` `Accepted` or `Pending`; `lastState` is included only after a state read. Acceptance does not invent `Running`. |
| Accepted without a usable `x-ms-operation-id` | HTTP 202 with `AcceptedWithoutOperationId` and an explicit issue. Check whether creation occurred; do not automatically resubmit. `Location` is not used to recover a missing ID. |
| Wait observes `Succeeded` and retrieves valid item metadata | HTTP 200 with `item` and a `Succeeded` operation receipt. |
| Wait observes `Failed` | HTTP 502 with a `Failed` receipt and safe operation error details. No result fetch or new POST is attempted. |
| A state read fails | A sanitized error with a `TrackingStopped` receipt, preserving the ID and last-known state; this is not evidence that Fabric's operation failed. |
| Operation succeeded but the result was not retrieved | `ResultUnavailable`, preserving `lastState.status=Succeeded` when observed. Budget expiry returns HTTP 202; read/validation failures retain their error status. Do not recreate the item. |
| Caller cancels a post-acceptance wait | HTTP 408 with a receipt when the client can still receive it, distinguishing caller cancellation from budget expiry. Confirmed completion is not downgraded to running. |

Failures suppress raw backend messages, exception details, stack traces, and local paths. HTTP 403 can indicate permissions or unavailable tenant/capacity features; status-less network failures retain HTTP 503. Before acceptance is known, a timeout, cancellation, network failure, or invalid response can leave the mutation outcome uncertain; check whether creation occurred before retrying.

**Update Item Definition (`core_update-item-definition`)**

Calls [Update Item Definition](https://learn.microsoft.com/rest/api/fabric/core/items/update-item-definition)
with one `POST /workspaces/{workspaceId}/items/{itemId}/updateDefinition`. Both `workspace-id` and
`item-id` are required nonempty UUIDs. This is a **complete replacement, not a changed-files patch**:
provide all required definition parts, not just the files edited since the last upload. The tool
never fetches the old definition, merges parts, resolves names, downloads schemas, creates dependencies,
or performs a metadata read-back.

Exactly one definition source is required, reusing Create Item's input and file infrastructure:

| Source | Arguments | Availability |
|--------|-----------|--------------|
| Inline | `definition`: JSON object **string** with `parts` and optional `format` | Local CLI, stdio, and HTTP |
| Saved files | `definition-directory`: existing absolute directory; `definition-files`: explicit JSON array **string** of `partPath` and optional `sourcePath`; optional `definition-format` | Local CLI and stdio only |

The same portable-path, strict JSON, Base64, byte/count, and local-file safeguards described above
apply. Saved files come from the **MCP hosting machine**, not a remote client's filesystem or unsaved
editor buffers. Save changes and explicitly list every part; no folder scan, wildcard, automatic
`.platform` inclusion, or source-file modification occurs. Over HTTP, filesystem options are hidden
and rejected. The input service independently rechecks local-file capability before any filesystem
access, while inline updates remain available.

Validation is **generic and content-independent**: it does not enforce item-type or format-specific
filenames, required parts, or layout combinations. The supplied format, portable part paths, and
payload bytes pass through unchanged for default, known, and future formats. There is no item-type
hint, `.platform` interpretation, metadata preflight, or local completeness claim. Consult
`docs_item-definitions` for the bundled reference and supply a complete definition; Fabric validates
supported formats, required parts, content, and bindings.

`options` supplies the API's optional top-level item-specific JSON **object string**. It is limited
to **1 MiB of UTF-8 input and 32 nesting levels**, rejects duplicate properties at any depth, and
is validated before file reads. Omitted options stay omitted; `{}`, false values, null members,
and future workload properties are preserved. Fabric validates supported properties for the
actual item type. These options never control polling.

`update-metadata` requires an explicit Boolean value. Omission leaves the query parameter omitted;
`false` sends `?updateMetadata=false`; **only explicit `true` requests applying metadata from a
supplied `.platform` part**. The part's bytes are always preserved: the tool does not remove it
when metadata application is disabled, add it when absent, or rewrite its contents or bindings.
The flag can be sent without a `.platform` part; it does not cause the tool to invent metadata.
This API **does not change the sensitivity label**. There are no `sensitivity-label-settings`,
`folder-id`, `creation-payload`, display-name, or description arguments on this tool.

This example **replaces a real item's definition** when run with authorized credentials. Review
the complete file selection first. It includes `.platform` without opting into metadata application:

```powershell
fabmcp core update-item-definition `
  --workspace-id cfafbeb1-8037-4d0c-896e-a46fb27ff229 --item-id 5b218778-e7a5-4d73-8187-f10824047715 `
  --definition-directory 'C:\Fabric\Sales.Notebook' `
  --definition-files '[{"partPath":"artifact.content.ipynb","sourcePath":"sales.ipynb"},{"partPath":".platform"}]' `
  --definition-format ipynb --sync true --max-wait-seconds 120
```

For inline content, supply `--definition $definition` instead of all three filesystem options,
using a complete definition JSON string like the Create Item example above. Add
`--update-metadata true` only when the supplied `.platform` metadata should also be applied.

A documented **empty HTTP 200** returns `results.operation.status=Succeeded` immediately, in either
sync mode. A **202** returns an `Accepted` receipt without polling by default. With `sync=true`,
the shared runner polls **operation state only**; confirmed `Succeeded` finishes without a
`/result` or Get Item request because this endpoint has no result body. Its result contains only
the operation receipt, never echoed definitions, options, paths, or fabricated item metadata.

`max-wait-seconds` defaults to 120 and allows positive finite values up to 600 seconds, including
fractions. `early-poll` defaults to true; the shared initial early-probe and subsequent Retry-After
rules above apply. Only status GETs may be retried on 429/503; the initiating POST is never retried.
Budget expiry returns HTTP 202 with `Pending` and last-known state. A reported failure returns a
safe `Failed` receipt; read failures preserve a `TrackingStopped` receipt. Caller cancellation
stops only local waiting and preserves a known receipt when the client can still receive it.
Resume a pending update with `core_get-operation-state` and stop at `Succeeded`: **do not call
`core_get-operation-result` or resubmit the update to extend a wait**. Missing operation IDs,
unexpected success codes/bodies, and transport failures are surfaced without assuming success or
following response URLs.

Read **and write** item permissions are required. Delegated callers need `Item.ReadWrite.All`
or the appropriate item-specific `ReadWrite.All` scope. Service-principal and managed-identity
support depends on item type. Development coverage uses substituted HTTP and credentials; it
does not authorize live updates or establish actual OBO permissions or recorded playback.

**Long-running operation tools**

The [Get Operation State](https://learn.microsoft.com/rest/api/fabric/core/long-running-operations/get-operation-state) and [Get Operation Result](https://learn.microsoft.com/rest/api/fabric/core/long-running-operations/get-operation-result) tools each take only `operation-id`, a nonempty UUID. Each invocation performs **one GET**, with no implicit polling, retries, mutation, or cancellation. URLs are constructed from the fixed Fabric API base and the validated ID. Returned links are not followed or returned as action URLs, and a response cannot replace the requested operation ID.

An agent-driven flow starts with the receipt from the original `sync=false` creation:

```json
{
  "operation": {
    "status": "Accepted",
    "operationId": "0acd697c-1550-43cd-b998-91bfbfbd47c6",
    "serverRetryAfterSeconds": 20,
    "recommendedPollAfterSeconds": 3
  }
}
```

Wait for the recommendation, then make one state read. If still pending, wait for that response's guidance before the next read. After `Succeeded`, fetch the result for Create Item, which is a result-bearing API:

```powershell
fabmcp core get-operation-state --operation-id 0acd697c-1550-43cd-b998-91bfbfbd47c6
# Only after the state says Succeeded:
fabmcp core get-operation-result --operation-id 0acd697c-1550-43cd-b998-91bfbfbd47c6
```

A state GET returning HTTP 200 means the **read** succeeded, not necessarily the mutation. The returned `state.status` can be `Undefined`, `NotStarted`, `Running`, `Succeeded`, `Failed`, or a future string. Unknown states are preserved and handled conservatively, never interpreted as successful completion. Progress (0-100) and timestamps are optional. Failed states expose only a safe error code and request UUID, not backend error text or nested details.

The generic result is not assumed to be a Fabric item. It uses `{ "operationId": "...", "hasBody": true, "value": ... }` for JSON objects, arrays, or scalars. JSON `null` remains `hasBody=true, value=null`; an actually empty HTTP 200 body uses `hasBody=false` with `value` omitted. Nonempty bodies require `application/json`. **Binary (`application/octet-stream`) and other media are explicitly unsupported, even if empty.** JSON responses are limited to **1 MiB (1,048,576 bytes)**, including streamed responses without `Content-Length`; oversized or malformed content produces a sanitized error, never truncation or fabricated success. For a completed **Get Item Definition** operation, use `core_get-item-definition` with `operation-id` instead: its typed definition reader has its own larger bound and optional local export.

As described in the [Fabric LRO guidance](https://learn.microsoft.com/rest/api/fabric/articles/long-running-operation), not every operation has a result. For an API explicitly documented as having no result, confirmed `Succeeded` is sufficient; do not fetch `/result`. Neither HTTP 404 nor an absent `Location` proves successful no-result completion. The shared dispatcher/poller supports explicit result-bearing and no-result adapters; **workspace capacity assignment retains its existing submit-only contract** and is not retrofitted with sync waiting in this release.

Both read tools require the **same resource permissions and delegated scopes as the initiating API**. Read-only discovery is not an authorization grant. Credential resolution remains per call, including every poll and result read, with the configured caller's identity.

**Catalog Search (`core_search-catalog`)**

Calls the [Catalog Search API](https://learn.microsoft.com/rest/api/fabric/core/catalog/search) for one page of metadata visible to the caller. Delegated calls require `Catalog.Read.All`; catalog discovery does not grant access to item contents.

```powershell
# Initial page.
fabmcp core search-catalog --search "Sales" --filter "Type eq 'Report'" --page-size 25

# Next page: copy the token unchanged; do not repeat search or filter.
fabmcp core search-catalog --continuation-token '<token from the preceding page>'
```

A continuation token already carries the original search, filter, and page size. Combining `continuation-token` with `search` or `filter` is rejected locally with HTTP 400 before service, authentication, or network calls; supplied criteria are never silently dropped. `page-size` remains optional and must be between 1 and 1000 when supplied, including with a token.

Success keeps the existing nested command envelope: entries are in `results.results.value`, and the next token is in `results.results.continuationToken`. Entries retain their typed metadata and available `hierarchy.workspace` information. There is no all-pages fetch, polling, or automatic retry. HTTP failures retain their original status with sanitized messages, while status-less network failures retain HTTP 503; raw exception details are not included in public error results.

**Get Item (`core_get-item`)**

Calls the [Get Item API](https://learn.microsoft.com/rest/api/fabric/core/items/get-item) once.
Both `workspace-id` and `item-id` are required nonempty UUIDs, normalized before authentication or HTTP.
The result contains an `item` object with `id`, `displayName`, `type`, `workspaceId`, and `description`
when available. Missing/null descriptions are omitted and an explicitly empty description is preserved.
The returned item and workspace IDs must match the requested IDs. Unlike workspace metadata, item
`type` is required; future nonempty type strings are accepted.

```powershell
fabmcp core get-item --workspace-id <workspace-id> --item-id <item-id>
```

The caller needs read permission for the item. Delegated callers also need `Item.Read.All`,
`Item.ReadWrite.All`, or an appropriate item-specific read scope. Authentication uses the host's
configured credential provider and preserves the caller's context; the tool does not grant permissions.

This read-only tool does not resolve names, retrieve definitions or data, return workload-specific
properties, or request default-identity information. Use `core_search-catalog` to discover an unknown
item ID. Only the documented HTTP 200 metadata response is accepted.

For MCP clients, `--structured-output-mode compact` or `--structured-output-mode duplicated` exposes
the typed output schema and `structuredContent.item`. The default mode keeps content-only responses
with `results.item`.

Command validation uses the shared sanitized handling; service errors do not expose raw backend
bodies. HTTP 429 includes a valid `Retry-After` delay in seconds or retry time in UTC when supplied.
Missing, malformed, negative, out-of-range, or multiple headers produce generic retry guidance.
The tool does not retry automatically or follow redirects.

**Get Item Definition (`core_get-item-definition`)**

Calls the [Get Item Definition API](https://learn.microsoft.com/rest/api/fabric/core/items/get-item-definition)
with one `POST /workspaces/{workspaceId}/items/{itemId}/getDefinition`. For an initial request, supply
both `workspace-id` and `item-id` as nonempty UUIDs. Optional `format` must not be empty or whitespace
and allows at most 128 characters without control characters, encoded as one query value. Omit it to use the item's
default; the tool does not look up item metadata, resolve names, download schemas or individual parts,
or create/update any item.

```powershell
# Inline content only; a 202 response returns an operation receipt without polling.
fabmcp core get-item-definition --workspace-id cfafbeb1-8037-4d0c-896e-a46fb27ff229 --item-id 5b218778-e7a5-4d73-8187-f10824047715 --format ipynb
```

Only the documented **200** completed definition and **202** accepted receipt are supported.
Completed output contains a typed `definition` with `parts` (`path`, `payload`, `payloadType`) and
`format` only when Fabric actually returned one. Payloads remain Base64 for inline use. No item type
is required and no item-specific filename, layout, or format-combination rules are imposed on returned content.
The bundled item-definition articles remain useful editing references through `docs_item-definitions`,
not runtime schemas. Sensitivity labels are **not included** in the definition.

`sync` defaults to `false`. With `sync=true`, the existing LRO runner polls state and retrieves the
definition after success. `max-wait-seconds` defaults to **120**, accepts positive finite fractional
values, and has a **600-second** maximum. `early-poll` defaults to `true`, allowing one initial probe
after at most three seconds; set it to `false` to honor the initial server hint. Later polls and
429/503 read retries honor validated hints. There is no automatic resubmission of the initial POST.

**Explicit local export**

Supply `output-directory` only when files are wanted. It must name a **new absolute local directory
below an existing parent**. The existing definition file service preserves every returned part's
bytes, encodings, line endings, relative path, binary assets, empty payloads, and `.platform`.
It writes a temporary sibling directory and publishes only the completed directory; existing files
and directories are never overwritten, merged, or deleted. Export errors and cancellation do not
fall back to inline success or claim that a partial definition was saved.

```powershell
# C:\Fabric already exists; Sales.Notebook must not exist.
fabmcp core get-item-definition --workspace-id cfafbeb1-8037-4d0c-896e-a46fb27ff229 --item-id 5b218778-e7a5-4d73-8187-f10824047715 `
  --format ipynb --sync true --output-directory 'C:\Fabric\Sales.Notebook'
```

On completed export, the result still includes `definition` and adds the published `outputDirectory`.
Keep any returned format when packaging edited files later; the tool does not invent a format or
write an extra manifest. Omission of `output-directory` means **no filesystem access or output**.
The option is hidden and rejected over HTTP, and a separate service-level capability check rejects
file access even when command-schema checks are bypassed. File paths are on the local MCP host,
not the remote client's machine. Linked/junction, UNC/device, and unsafe part paths are rejected.
These checks assume a trusted local filesystem, not an OS sandbox against concurrent hostile changes.

**Resuming without restarting**

Acceptance, local wait expiry, and failed tracking never imply a saved definition. A receipt retains
its operation ID and last-known state when available; no background poller or file writer continues
after the call ends. Caller cancellation stops local waiting, not the Fabric operation.
Check state using the receipt, respecting its polling guidance. When state is `Succeeded`, select
the tool's mutually exclusive resume mode:

```powershell
fabmcp core get-operation-state --operation-id 0acd697c-1550-43cd-b998-91bfbfbd47c6
# Only after Succeeded; omit output-directory to retrieve inline content instead.
fabmcp core get-item-definition --operation-id 0acd697c-1550-43cd-b998-91bfbfbd47c6 --output-directory 'C:\Fabric\Sales.Notebook'
```

Resume sends exactly **one GET** to `/operations/{operationId}/result`, without a state preflight,
polling, or another POST. Do not combine `operation-id` with workspace/item IDs, `format`, or
`sync=true`. The result endpoint does not identify its source item; use an operation ID retained
from the intended Get Item Definition request, rather than assuming item provenance was verified.
Only returned format information is preserved. A missing, malformed, not-ready, or oversized result
is an explicit error. If Fabric reports success but result retrieval fails, the receipt preserves
completion and the operation ID without pretending that the definition was retrieved or exported.

**Bounds, permissions, and metadata**

Definition JSON has a separate **64 MiB (67,108,864 byte)** wire limit, also enforced for streams
without `Content-Length`. The existing Create Item, state, and generic operation-result limits stay
at **1 MiB**. Generic definition validation requires a nonempty part list, unique portable paths
without file/directory collisions, valid `InlineBase64`, and the shared host budgets (defaults:
**1,024 parts, 16 MiB decoded per part, 32 MiB decoded total**). Invalid or oversized content is
rejected before export, never truncated or silently skipped. These are host safeguards, not Fabric quotas.

Fabric requires **read AND write item permissions**, plus `Item.ReadWrite.All` or the item-specific
`ReadWrite.All` delegated scope. Service-principal and managed-identity support depends on item type.
Encrypted sensitivity labels can require additional usage rights. Returning definition contents can
expose sensitive code or configuration, so the tool advertises `Secret=true` and uses the existing
MCP consent flow; clients without elicitation support are rejected by default. It remains remote-capable
for inline results (`LocalRequired=false`). Because it can optionally write local files, it conservatively
advertises `ReadOnly=false` and is **excluded from read-only server mode in every transport**, even
when no export was requested. No-overwrite export is non-destructive and repeated calls cannot replace
existing files. Errors omit raw backend bodies, definition contents, and filesystem details.

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

**List Workspace Role Assignments (`core_list-workspace-role-assignments`)**

Calls [List Workspace Role Assignments](https://learn.microsoft.com/rest/api/fabric/core/workspaces/list-workspace-role-assignments)
with one `GET /v1/workspaces/{workspaceId}/roleAssignments` and no request body. This lists workspace
role assignments, not effective permissions or OneLake data access roles.

| Argument | Required | Behavior |
|---|---|---|
| `workspace-id` | Yes | Nonempty workspace UUID; names are not resolved. |
| `continuation-token` | No | Unchanged token from a previous page for the same workspace. Omit for the first page. |

```powershell
fabmcp core list-workspace-role-assignments --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb
fabmcp core list-workspace-role-assignments --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --continuation-token '<returned-token>'
```

Returns `roleAssignments` in service order, preserving repeated entries without a row-count cap.
Each assignment has its own `id`, `role`, and `principal` with `id` and `type`; assignment and principal
IDs are not assumed equal. The API does not include a required workspace ID on each row.
Optional principal `displayName`, `userDetails.userPrincipalName`, `groupDetails.groupType`,
`servicePrincipalDetails.aadAppId`, and `servicePrincipalProfileDetails.parentPrincipal` are retained
when supplied, including documented parent-principal metadata. Missing/null optional fields are not
invented; explicit empty display names and user principal names are preserved. Future role, principal
type, and group type strings remain strings. Unknown extension fields are omitted.

Each invocation returns **one complete page**, including an empty page with a continuation token.
Following [Fabric pagination guidance](https://learn.microsoft.com/rest/api/fabric/articles/pagination),
absent or JSON-null continuation fields mark a terminal page; a valid token with an absent/null URI
still enables a separate call. A supplied URI must match the fixed origin, workspace collection, and
token; URI-only, wrong-type, or contradictory metadata fails rather than silently ending enumeration.
`continuationUri` is never returned or followed. Raw and already percent-escaped tokens are accepted
unchanged, with no trimming or double encoding. Tokens must be valid Unicode, not blank, without
control characters, and at most 8,192 UTF-8 bytes.

The caller needs **workspace Member or higher** access. Delegated calls require **Workspace.Read.All**
or **Workspace.ReadWrite.All**. The API supports users, service principals, and managed identities;
the latter two also need the tenant setting permitting service principals to use Fabric APIs.
The tool reuses the configured authentication and is available in read-only mode over stdio and HTTP.
It performs no Graph/directory lookup, group-membership expansion, identity enrichment, per-row request,
automatic paging, retry, polling, or role mutation.

Only HTTP 200 is a successful page. Failures preserve HTTP status and validated `Retry-After` guidance
without exposing raw backend errors. JSON is bounded to 1 MiB (1,048,576 bytes) and 64 input nesting levels;
retained output is limited to 61 levels to leave room for MCP envelopes, including recursive profile
parents. Malformed required fields, ambiguous duplicates, invalid Unicode, or an over-budget page fail
safely without partial results or truncation. No live workspace, real OBO/RBAC, or recorded coverage is claimed.

**Get Workspace Role Assignment (`core_get-workspace-role-assignment`)**

Calls [Get Workspace Role Assignment](https://learn.microsoft.com/rest/api/fabric/core/workspaces/get-workspace-role-assignment)
with one `GET /v1/workspaces/{workspaceId}/roleAssignments/{workspaceRoleAssignmentId}` and no body or query.
This reads an existing workspace role assignment, not effective permissions or OneLake data access roles.

| Argument | Required | Behavior |
|---|---|---|
| `workspace-id` | Yes | Nonempty workspace UUID; names are not resolved. |
| `workspace-role-assignment-id` | Yes | Nonempty assignment UUID, not a principal-name or principal-ID lookup. |

```powershell
fabmcp core get-workspace-role-assignment --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --workspace-role-assignment-id bbbbbbbb-1111-2222-3333-cccccccccccc
```

Returns `roleAssignment` with its matching `id`, `role`, and `principal` (`id` and `type`), plus validated
`retryAfterSeconds` or `retryAfterUtc` when available. Assignment IDs and principal IDs are independent;
the tool neither requires them to match nor lists assignments to translate them. No workspace ID is
required on the returned assignment. The same optional principal details as List are retained, including
recursive profile parents. Missing/null optional metadata stays omitted, empty names stay empty, and
future role, principal type, and group type strings are preserved. Unknown extension fields are omitted.

The caller needs **workspace Member or higher** access and, for delegated calls, **Workspace.Read.All**
or **Workspace.ReadWrite.All**. Users, service principals, and managed identities are supported;
service principals and managed identities also need the tenant setting permitting Fabric API access.
The existing configured authentication is unchanged. The tool is available in read-only mode over
stdio and HTTP, with no preliminary List/Get, Graph or directory lookup, membership expansion,
enrichment, pagination, retry, polling, or role mutation.

Only HTTP 200 is a successful snapshot. Errors preserve HTTP status and validated `Retry-After`
without reading or exposing backend error bodies. JSON is bounded to 1 MiB (1,048,576 bytes) and
64 input levels. The single-result payload allows 61 retained levels, leaving three for the namespace,
tool-response, and JSON-RPC envelopes. For example, 30 recursively nested principals without leaf
detail objects fit; adding leaf details exceeds that budget and fails without truncating metadata.
Malformed or missing required data, mismatched assignment IDs, duplicates, invalid Unicode, and
over-budget responses fail without partial results. No live workspace, real OBO/RBAC, or recorded
coverage is claimed without an approved fixture and authorization.

**Add Workspace Role Assignment (`core_add-workspace-role-assignment`)**

Calls [Add Workspace Role Assignment](https://learn.microsoft.com/rest/api/fabric/core/workspaces/add-workspace-role-assignment)
with one `POST /v1/workspaces/{workspaceId}/roleAssignments`. This grants workspace access; it does not
calculate effective permissions or manage OneLake data access roles.

| Argument | Required | Behavior |
|---|---|---|
| `workspace-id` | Yes | Nonempty workspace UUID; names are not resolved. |
| `principal` | Yes | JSON object with explicit nonempty `id` UUID and `type`, plus any documented writable details below. |
| `role` | Yes | Explicit `Admin`, `Member`, `Contributor`, or `Viewer` access intent. No default, trimming, upgrade, or fallback. |

```powershell
# Example only: use a separately authorized workspace, principal and role.
fabmcp core add-workspace-role-assignment --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --principal '{"id":"cccccccc-2222-3333-4444-dddddddddddd","type":"User"}' --role Viewer
```

The request contains only `principal` and `role`. The documented principal types are `User`, `Group`,
`ServicePrincipal`, `ServicePrincipalProfile`, and `EntireTenant`. Supply the identity's principal UUID:
an Entra object ID for a user, group or service principal (not the application's client ID), or the
profile ID for a service principal profile. **EntireTenant targets all tenant users** and must be an
explicit caller choice with its principal UUID; the tool never infers it or substitutes a tenant ID.
No email, display-name, application-ID, principal-to-assignment translation, or directory/Graph lookup
is performed. Missing principal/type/role information must come from the caller.

For `Group`, optional `groupDetails.groupType` accepts the documented `Unknown`, `SecurityGroup`,
and `DistributionList` values. For `ServicePrincipalProfile`, optional
`servicePrincipalProfileDetails.parentPrincipal` accepts another principal object with the same
writable contract. Omitted fields remain omitted. When supplied, details and parent principals must
be objects and groupType must be a nonempty string; explicit null is rejected because the Add
request schema does not declare these fields nullable. This differs from optional response metadata.
Principal, group-type, and role strings are extensible in the API:
explicit nonempty future strings are passed unchanged for Fabric to validate, not interpreted locally.
Read-only `displayName`, `userDetails`, and `servicePrincipalDetails` (including `aadAppId`), unknown
fields, duplicates, malformed Unicode, and inapplicable details are rejected before authentication.
Principal JSON is limited to 1 MiB and 32 nesting levels; the complete serialized request, including
escaping and role, is also limited to 1 MiB. No input fields are stripped to make a rejected grant succeed.

The caller needs **workspace Member or higher** and delegated **Workspace.ReadWrite.All**.
Members can grant **Member, Contributor, or Viewer**; granting **Admin requires an Admin caller**.
Members cannot change existing users' roles. Fabric enforces these restrictions and its documented
maximum of 1,000 users/groups in workspace roles, including external guests; group membership size is
not limited by that rule. The tool does not preflight permissions, count assignments, expand groups,
change tenant settings, remove an existing assignment, or retry a conflict as an update.
User, service-principal, and managed-identity callers use the existing authentication composition.
Service principals and managed identities also require the tenant setting permitting Fabric API access.

Only **HTTP 201** confirms creation. The result contains `creationStatus: "Created"`, `roleAssignment`
with its new assignment `id`, matching requested `role` and principal `id`/`type`, available principal
metadata, and validated retry guidance. **Assignment and principal IDs are independent**.
The response reuses the same optional/future principal metadata projection as List/Get; no workspace
field is required. Returned Location/operation headers are not followed or exposed as navigation.
This endpoint has no documented standard LRO, so there is no polling or result-retrieval option.

Response JSON is bounded to 1 MiB and 64 input levels, with 61 retained result levels reserved for
the real namespace, tool-response, and JSON-RPC envelopes. No metadata is truncated to fit.
If 201 is received but metadata is malformed, oversized, mismatched, unreadable, canceled, or too
deep to serialize, the error **still says creation was confirmed**: do not resubmit the grant.
Unsent validation/authentication failures, explicit rejections, and uncertain sends are distinct.
After a timeout, network failure, unexpected success status, or server error with an uncertain
outcome, inspect assignments separately before considering a new grant; never replay blindly.
Known HTTP status and validated `Retry-After` survive unusable error metadata. Only safe error
codes are projected, never backend messages or raw payloads. Retry timing is not replay authorization.

This additive permission mutation is remote-capable and non-read-only/non-idempotent, using the
repository's normal additive permission-management annotations rather than a custom confirmation
gate or invented destructive/secret classification. Read-only mode hides and denies it while retaining
List/Get. No live grant, real token exchange/OBO/RBAC, or recorded coverage is claimed without an
approved fixture and grant authorization.

**Update Workspace Role Assignment (`core_update-workspace-role-assignment`)**

Calls [Update Workspace Role Assignment](https://learn.microsoft.com/rest/api/fabric/core/workspaces/update-workspace-role-assignment)
with one `PATCH /v1/workspaces/{workspaceId}/roleAssignments/{workspaceRoleAssignmentId}`.
This changes an existing workspace permission assignment, not effective permissions or OneLake data access roles.

| Argument | Required | Behavior |
|---|---|---|
| `workspace-id` | Yes | Nonempty workspace UUID; names are not resolved. |
| `workspace-role-assignment-id` | Yes | Nonempty existing assignment UUID, independent of its principal ID. |
| `role` | Yes | Explicit replacement `Admin`, `Member`, `Contributor`, or `Viewer` role. No default, trimming, upgrade, or fallback. |

```powershell
# Example only: use a separately authorized workspace, existing assignment, and replacement role.
fabmcp core update-workspace-role-assignment --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --workspace-role-assignment-id bbbbbbbb-1111-2222-3333-cccccccccccc --role Viewer
```

The request contains **only `role`**, not Add's principal JSON. The Update request requires this
non-null, nonempty string; optional/null principal response metadata does not change that requirement.
The API's role strings are extensible, so explicit nonempty future values are forwarded unchanged for
Fabric to validate. Invalid Unicode and requests exceeding the 1 MiB (1,048,576 byte) serialized
budget, including JSON escaping, are rejected before authentication. No principal ID, name, email,
application ID, or group membership is resolved or translated into an assignment ID.

The caller must have **workspace Admin** access, and delegated calls require **Workspace.ReadWrite.All**.
**The last admin's assignment cannot be changed.** Fabric enforces these restrictions without a
permission preflight. Users, service principals, and managed identities use the unchanged configured
authentication; service principals and managed identities also need the tenant setting permitting
Fabric API access. The tool does not alter settings, switch identities, choose a different role,
create a missing assignment, or resolve conflicts automatically.

Only **HTTP 200** confirms update. The result contains `updateStatus: "Updated"`, `roleAssignment`
with the matching assignment `id` and requested `role`, available principal metadata, and validated
`retryAfterSeconds` or `retryAfterUtc`. Assignment and principal IDs need not match. The existing
List/Get response projection retains optional and future principal details, including recursive
profile parents, without inventing missing/null fields, requiring a workspace field on the response,
or exposing unknown extensions. No returned Location/operation URL is followed.
This is not a documented standard LRO; there is no polling or result-retrieval option.

Response JSON is limited to 1 MiB and 64 input levels. The complete result has a 61-level retained
budget to accommodate namespace, tool-response, and JSON-RPC envelopes; no metadata is truncated.
After HTTP 200, malformed, oversized, mismatched, unreadable, canceled, or unserializable metadata
produces an error that **still reports confirmed update** and says not to resubmit. Validation and
authentication failures before sending, explicit rejections, uncertain sends, and confirmed updates
remain distinct. After an uncertain network failure, timeout, unexpected success status, or server
error, inspect the known assignment with a separately authorized Get before considering another
update; never replay blindly. Known HTTP status and validated retry timing survive unusable error
metadata. Only safe error codes are projected, never backend messages or raw payloads.

This remote-capable, destructive/idempotent permission replacement can increase or reduce access.
It uses the repository's normal mutation confirmation, not a custom consent gate. Read-only mode
hides and denies it while preserving List/Get. Idempotence is not authorization for automatic replay.
Every invocation sends at most one initiating request, without hidden List/Get, enrichment, creation,
verification, retry, or polling. Offline fixtures and discovery do not demonstrate live token exchange,
OBO/RBAC authorization, or recorded coverage; no live update is run without an approved fixture and
separate permission-change authorization.

**Delete Workspace Role Assignment (`core_delete-workspace-role-assignment`)**

Calls [Delete Workspace Role Assignment](https://learn.microsoft.com/rest/api/fabric/core/workspaces/delete-workspace-role-assignment)
with one `DELETE /v1/workspaces/{workspaceId}/roleAssignments/{workspaceRoleAssignmentId}`.
There is **no request body or query option**. Supply the explicit nonempty `workspace-id` and
`workspace-role-assignment-id` UUIDs. Assignment IDs and principal IDs are independent; names, email
addresses, application IDs, or principal IDs are not resolved or translated into assignment IDs.

```powershell
# Example only: requires separate authorization to remove this specific assignment.
fabmcp core delete-workspace-role-assignment --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --workspace-role-assignment-id bbbbbbbb-1111-2222-3333-cccccccccccc
```

The caller must have **workspace Admin** access, and delegated calls require **Workspace.ReadWrite.All**.
**The last admin's assignment cannot be deleted.** Fabric enforces these restrictions without a local
permission preflight. Users, service principals, and managed identities use the unchanged configured
authentication. Service principals and managed identities also need the tenant setting permitting
Fabric API access; this tool does not change settings or select another identity on failure.

Only **HTTP 200** confirms deletion. The result is a receipt with `deletionStatus: "Deleted"`,
`workspaceId`, `workspaceRoleAssignmentId`, and `guidance`, not a returned assignment snapshot.
The response body is irrelevant and is not read, even if malformed, oversized, or unavailable.
Late cancellation cannot erase confirmation already received in the headers. If local receipt
completion fails after 200, the error still reports **confirmed deletion** and says not to resubmit.
HTTP 201, 202, 204, redirects, and other undocumented responses do not confirm deletion.
A 404 remains a failure, including on an explicit repeated deletion.

Deletion removes **only the specified assignment**, not the principal, group membership, workspace,
or other assignments. It does not establish that the principal has no remaining effective access.
There is no directory/Graph lookup, membership expansion, preflight List/Get, cascade, bulk removal,
fallback, retry, or hidden verification. This is not a documented standard Fabric LRO; Location and
operation headers are not followed, and no polling or result-retrieval option is exposed.

Validation/authentication failures before sending, explicit rejections, uncertain sends, and confirmed
deletions remain distinct. After an uncertain network failure, timeout, server error, or undocumented
success status, inspect the known assignment separately before considering a new deletion; never
replay blindly. Known HTTP status and validated `Retry-After` survive unusable optional error metadata.
Only safe error codes are projected, never backend messages or raw payloads. Retry timing is not
authorization to replay a permission change.

This remote-capable, destructive/idempotent permission mutation uses normal repository confirmation,
not a custom gate. Read-only mode hides and denies it while preserving List/Get. Idempotence does not
authorize replay. Offline HTTP fixtures and actual-host discovery do not demonstrate live token
exchange, OBO/RBAC authorization, or recorded coverage. No live removal is performed without an
approved workspace/assignment fixture and separate revocation authorization.

**Workspace folders**

Fabric's [Folder REST APIs](https://learn.microsoft.com/rest/api/fabric/core/folders) are currently in Preview and may change.

**List Folders (`core_list-folders`)**

`core_list-folders` calls [List Folders](https://learn.microsoft.com/rest/api/fabric/core/folders/list-folders)
with one `GET /v1/workspaces/{workspaceId}/folders` and no request body. These are logical Fabric workspace
folders, not OneLake directories or local filesystem paths.

| Argument | Required | Behavior |
|---|---|---|
| `workspace-id` | Yes | Nonempty workspace UUID; names are not resolved. |
| `root-folder-id` | No | Nonempty folder UUID. Omit to use the workspace root. |
| `recursive` | No | Explicit `true` or `false`. Omission uses the API default of `true`; `false` lists direct children only. |
| `continuation-token` | No | Unchanged token from a previous page with the same workspace, root folder, and recursive options. Omit for the first page. |

```powershell
fabmcp core list-folders --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb
fabmcp core list-folders --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --root-folder-id bbbbbbbb-1111-2222-3333-cccccccccccc --recursive false
# Request another page explicitly, preserving the filters and the returned token.
fabmcp core list-folders --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --root-folder-id bbbbbbbb-1111-2222-3333-cccccccccccc --recursive false --continuation-token '<returned-token>'
```

The result contains `folders` with `id`, `displayName`, `workspaceId`, and optional `parentFolderId`.
Order and repeated entries are preserved; parents may be outside the page. No paths or ancestors
are reconstructed. An empty page can still have a `continuationToken`. Absent or null terminal
continuation fields are valid. Copy tokens without trimming, decoding, or changing percent escapes.
Returned continuation URLs are neither exposed nor followed; if present, they must match the
collection, token, and any supplied filter metadata. URI-only or contradictory metadata fails safely.
There is no folder walking, automatic pagination, per-folder enrichment, retry, polling, or mutation.

The caller needs workspace **Viewer or higher** access; delegated calls require `Workspace.Read.All`
or `Workspace.ReadWrite.All`. Users, service principals, and managed identities are supported,
subject to the existing Fabric tenant and authentication settings. This read-only tool is available
in stdio and HTTP modes without sensitive-content consent. Errors retain HTTP status and validated
`Retry-After` guidance, not backend bodies. Pages are bounded to 1 MiB and 64 JSON nesting levels;
tokens must be valid Unicode, not blank, without control characters, and at most 8,192 UTF-8 bytes.
Invalid or oversized pages fail as a whole, with no row-count cap or partial result.

**Create Folder (`core_create-folder`)**

Calls [Create Folder](https://learn.microsoft.com/rest/api/fabric/core/folders/create-folder) with exactly
one `POST /v1/workspaces/{workspaceId}/folders`. It creates one logical workspace folder, not a local
directory, OneLake path, or recursive folder hierarchy.

| Argument | Required | Behavior |
|---|---|---|
| `workspace-id` | Yes | Nonempty workspace UUID; names are not resolved. |
| `display-name` | Yes | The unchanged name of one folder, subject to Fabric's naming rules. |
| `parent-folder-id` | No | Existing nonempty folder UUID. Omission or JSON null omits `parentFolderId` from the POST and creates at the workspace root. |

```powershell
fabmcp core create-folder --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --display-name "Sales"
fabmcp core create-folder --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --display-name "Q3" --parent-folder-id bbbbbbbb-1111-2222-3333-cccccccccccc
```

Names must meet the [folder name requirements](https://learn.microsoft.com/fabric/get-started/workspaces-folders#folder-name-requirements):
at most 255 Unicode characters, no leading/trailing spaces or C0/C1 control codes, none of
`~ " # . & * : < > ? / { | }`, and no reserved `$recycle.bin`, `recycled`, or `recycler` names.
Invalid inputs fail before authentication or HTTP. Names are never trimmed, sanitized, or interpreted
using host filesystem rules. Fabric enforces name uniqueness, folder depth/count, and workspace support;
the tool does not check these through extra requests. A duplicate-name error remains a failure, not
get-or-create success with an existing folder ID.

**Only HTTP 201 confirms creation.** The typed result contains `creationStatus: "Created"`, `folder`
with `id`, `displayName`, `workspaceId`, and available `parentFolderId`, plus `guidance` and validated
`Retry-After` metadata when supplied. Returned workspace and non-null parent IDs must match the request.
Missing/null optional parent metadata stays omitted, not filled from the request. Unknown fields are
not projected. Responses are limited to **1 MiB (1,048,576 bytes)** and **64 JSON nesting levels**;
malformed, duplicate, invalid-Unicode, or contradictory metadata fails safely.

If HTTP 201 is received but its metadata cannot be read or validated, the error explicitly preserves
**confirmed creation** and says not to resubmit. Authentication/pre-send failures report that the
request was not sent. Network failures after sending may leave the outcome unknown: inspect the
workspace separately before any new creation, never retry blindly. Error responses preserve HTTP
status and validated `Retry-After` guidance even when optional error metadata is unusable, without
exposing backend messages. There is no automatic retry, parent/name lookup, follow-up Get/List, or
LRO handling; Location and operation headers are ignored. An agent may explicitly use
`core_list-folders` separately, but that orchestration is not hidden inside creation.

The caller needs **Contributor or higher** workspace access; delegated callers require
`Workspace.ReadWrite.All`. Users, service principals, and managed identities are supported, subject to
Fabric permissions and existing tenant/authentication settings. Like other additive Core-create tools,
this is non-destructive, non-idempotent, not secret-bearing, remote-capable, and **excluded from
read-only mode**. It does not introduce an additional elicitation requirement. Offline tests do not
establish live workspace, OBO, or RBAC authorization.

**Get Folder (`core_get-folder`)**

Calls [Get Folder](https://learn.microsoft.com/rest/api/fabric/core/folders/get-folder) with exactly
one `GET /v1/workspaces/{workspaceId}/folders/{folderId}` and no request body. Both `workspace-id`
and `folder-id` are required nonempty UUIDs; names and paths are not resolved.

```powershell
fabmcp core get-folder --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --folder-id bbbbbbbb-1111-2222-3333-cccccccccccc
```

Only HTTP 200 returns a typed `folder` snapshot containing `id`, `displayName`, `workspaceId`,
and `parentFolderId` when available, plus validated `retryAfterSeconds` or `retryAfterUtc` guidance
when supplied. Returned folder and workspace IDs must match the request. Absent/null parent metadata
stays omitted; a parent need not have been listed or fetched. Display names are preserved as returned
strings, without applying creation naming rules, trimming, or filesystem interpretation.

These are logical workspace folders, not OneLake directories or local paths. There is no List
preflight, ancestor lookup, child/item enumeration, enrichment, pagination, automatic retry,
mutation, or LRO/polling behavior. Location and operation headers are not followed. Unknown JSON
fields are validated but not returned. Missing/invalid metadata is an error, not an empty success.
Responses are bounded to **1 MiB (1,048,576 bytes)** and **64 JSON nesting levels**; duplicate
properties and invalid Unicode fail safely.

The caller needs **Viewer or higher** workspace access; delegated calls require `Workspace.Read.All`
or `Workspace.ReadWrite.All`. Users, service principals, and managed identities are supported under
Fabric's existing tenant and authentication settings. Like List Folders, this metadata read is
available in read-only mode over stdio and HTTP without sensitive-content consent.
Errors preserve known HTTP status and validated `Retry-After`, even if optional error metadata cannot
be read. Only a safe error code is projected, never backend messages or nested details.
Offline tests do not establish live Fabric, OBO/RBAC, or recorded-playback behavior.

**Update Folder (`core_update-folder`)**

Calls [Update Folder](https://learn.microsoft.com/rest/api/fabric/core/folders/update-folder) with exactly
one `PATCH /v1/workspaces/{workspaceId}/folders/{folderId}`. The required `workspace-id` and `folder-id`
are nonempty UUIDs; `display-name` is also required, not an optional PATCH field. The JSON body contains
only `displayName`. This renames a logical workspace folder; it does not move it, change items or
permissions, or act on OneLake directories or local paths.

```powershell
fabmcp core update-folder --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --folder-id bbbbbbbb-1111-2222-3333-cccccccccccc --display-name "Quarterly Sales"
```

The replacement uses the same Fabric naming rules as Create Folder above: 1 to 255 Unicode
characters, without trimming, sanitizing, or host filesystem rules. Invalid inputs fail before
authentication or HTTP. Fabric enforces name uniqueness; a duplicate-name error remains a failure,
with no fallback name. There is no description, parent, or path input, Get/List/name-resolution
preflight, metadata merge, automatic retry, follow-up read, or LRO/polling behavior.

**Only HTTP 200 confirms the update.** The typed result contains `updateStatus: "Updated"`, the
returned `folder` snapshot, `guidance`, and validated `retryAfterSeconds` or `retryAfterUtc` when
supplied. Folder and workspace identities must match the request. Available parent metadata is
preserved; absent/null parents remain omitted, not inferred. Unknown fields are not returned.
Responses are bounded to **1 MiB (1,048,576 bytes)** and **64 JSON nesting levels**; malformed,
duplicate, invalid-Unicode, or mismatched metadata is an error, not a synthesized successful snapshot.

If HTTP 200 is followed by unreadable or invalid metadata, the error preserves **confirmed update**
and says not to resubmit. Authentication/pre-send failures report that the request was not sent.
An uncertain send can leave the update applied: inspect the known folder separately before another
update, never retry blindly. Error responses retain observed HTTP status and validated `Retry-After`
when optional error metadata is unusable, and expose only a safe error code, not backend messages.
Location and operation headers are ignored.

The caller needs **Contributor or higher** workspace access; delegated callers require
`Workspace.ReadWrite.All`. Users, service principals, and managed identities use the existing
Fabric tenant and authentication settings. Like Core Item/Workspace Update, replacing existing
metadata is marked destructive and idempotent, not secret-bearing, remote-capable, and **excluded
from read-only mode**. The host's normal mutation confirmation applies. Offline tests do not
establish live Fabric, OBO/RBAC, or recorded-playback behavior.

**Move Folder (`core_move-folder`)**

Calls [Move Folder](https://learn.microsoft.com/rest/api/fabric/core/folders/move-folder) with exactly
one `POST /v1/workspaces/{workspaceId}/folders/{folderId}/move`. This moves an existing logical folder
within the **same workspace**. Fabric handles the folder hierarchy, including contained folders and
items, server-side; the tool never enumerates descendants or sends individual item moves.
These are workspace organizational folders, not OneLake directories or local filesystem paths.

| Argument | Required | Behavior |
|---|---|---|
| `workspace-id` | Yes | Nonempty UUID of the containing workspace. |
| `folder-id` | Yes | Nonempty UUID of the existing folder to move. |
| `target-folder-id` | No | Existing nonempty folder UUID in the same workspace. Omission or MCP JSON null omits `targetFolderId` from the body and moves to the workspace root. |

```powershell
fabmcp core move-folder --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --folder-id bbbbbbbb-1111-2222-3333-cccccccccccc --target-folder-id aaaaaaaa-6666-7777-8888-bbbbbbbbbbbb
# Omit the target to move to the workspace root; the request body is {}.
fabmcp core move-folder --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --folder-id bbbbbbbb-1111-2222-3333-cccccccccccc
```

The request contains only optional `targetFolderId`, never a replacement name or target workspace.
Invalid UUIDs and a target equal to the folder being moved fail before authentication or HTTP.
Names and paths are not resolved, and destinations are not created. Fabric enforces descendant-loop,
depth, and duplicate-name constraints; failures remain failures, with no alternate target, rename,
fallback to root, or preflight lookup. The tool does not copy items, alter their contents or permissions,
or implement cross-workspace movement.

**Only HTTP 200 confirms movement.** The typed result contains `moveStatus: "Moved"`, the returned
`folder` snapshot, `guidance`, and validated `retryAfterSeconds` or `retryAfterUtc` when supplied.
Folder and workspace identities must match the request. A non-null returned parent must match the
requested destination and is invalid for a move to root. Absent/null optional parent metadata stays
omitted, not filled from the request; confirmation comes from HTTP 200, not an inferred parent.
Returned display names are preserved without applying creation naming restrictions.
Unknown fields are validated but not returned. Responses are bounded to **1 MiB (1,048,576 bytes)**
and **64 JSON nesting levels**; malformed, duplicate, invalid-Unicode, or contradictory metadata fails safely.

If HTTP 200 is followed by unreadable or invalid metadata, the error preserves **confirmed movement**
and says not to resubmit. Authentication/pre-send failures report that the request was not sent.
An uncertain send can leave the folder moved: inspect the known folder separately before another
move, never retry blindly. Error responses retain observed HTTP status and validated `Retry-After`
even when optional error metadata is unusable, and expose only a safe error code, not backend messages.
There is no automatic retry, hidden Get/List verification, or LRO/polling behavior; Location and
operation headers are ignored. Existing List/Get tools can be used in separate explicit calls to
choose IDs or inspect an uncertain result.

The caller needs **Contributor or higher** workspace access; delegated callers require
`Workspace.ReadWrite.All`. Users, service principals, and managed identities use the existing
Fabric tenant and authentication settings. Changing an existing folder's placement is marked
destructive and idempotent (repeating the same destination does not add another effect), not
secret-bearing, remote-capable, and **excluded from read-only mode**. Normal mutation consent applies.
Offline tests do not establish live Fabric, OBO/RBAC, or recorded-playback behavior.

**Delete Folder (`core_delete-folder`)**

Calls [Delete Folder](https://learn.microsoft.com/rest/api/fabric/core/folders/delete-folder) with exactly
one `DELETE /v1/workspaces/{workspaceId}/folders/{folderId}`, no request body, and no query parameters.
Both `workspace-id` and `folder-id` are required nonempty UUIDs; names and paths are not resolved.
These are workspace organizational folders, not OneLake directories or local filesystem paths.

**Only empty folders can be deleted.** Fabric rejects a folder containing items or nested folders
with `FolderNotEmpty`; the tool returns that failure without emptying, moving, or deleting its
contents. [Soft-deleted items also prevent folder deletion](https://learn.microsoft.com/fabric/admin/retention-recovery#limitations-of-soft-delete-and-recovery).
There is no recursive, force, or hard-delete option, emptiness preflight, descendant enumeration,
or fallback target. The API does not document folder recovery or a retention period; do not assume
the item or workspace recovery rules apply, and do not promise rollback.

```powershell
fabmcp core delete-folder --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --folder-id bbbbbbbb-1111-2222-3333-cccccccccccc
```

**Only HTTP 200 confirms deletion.** The typed receipt contains the requested `workspaceId` and
`folderId`, `deletionStatus: "Deleted"`, and `guidance`; it is not a returned folder snapshot.
An empty response is sufficient. Irrelevant response data is not read, and late request cancellation
does not undo confirmed deletion. If local receipt completion fails after HTTP 200, the error still
states that deletion was confirmed and must not be resubmitted.

Authentication/pre-send failures report that no DELETE was sent. After an uncertain network failure,
timeout, or cancellation during sending, inspect the folder separately before considering another
deletion; never retry blindly. Existing `core_get-folder` and `core_list-folders` can be used in
separate explicit calls to select IDs or inspect uncertainty, not as hidden preflight or verification.
Repeating deletion targets the same resource but can fail: **404 and `FolderNotFound` are not
converted to success**. Error responses retain observed HTTP status and validated `Retry-After`
even when optional error metadata is malformed, unreadable, or exceeds the shared 1 MiB/64-level
JSON budget. Only a safe error code is projected, not backend messages or nested details.
There is no automatic retry or LRO/polling behavior; 202/204 are not documented success responses,
and Location and operation headers are ignored.

The caller needs **Contributor or higher** workspace access; delegated callers require
`Workspace.ReadWrite.All`. Users, service principals, and managed identities use the existing
Fabric tenant and authentication settings. Deletion is destructive, idempotent by its effect on
the same folder, not secret-bearing, remote-capable, and **excluded from read-only mode**.
Explain the empty-folder deletion scope before the host's normal mutation confirmation.
Offline tests do not establish live Fabric, OBO/RBAC, or recorded-playback behavior; no live
deletion fixture or mutation authorization is assumed.

**Workspace and folder item inventory**

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

**Move Item (`core_move-item`)**

Calls the [Items Move Item API](https://learn.microsoft.com/rest/api/fabric/core/items/move-item)
with exactly one `POST /v1/workspaces/{workspaceId}/items/{itemId}/move`. Supply nonempty
`workspace-id` and `item-id` UUIDs and optionally `target-folder-id`, the UUID of an existing
destination folder in that same workspace. UUIDs are normalized, not resolved from names or paths.
Omitting the target or passing MCP `null` sends the required empty object `{}` for the workspace
root; a supplied target sends only `{"targetFolderId":"<folder-id>"}`. Invalid targets fail before
authentication, never fall back to root, and repeated option keys are rejected.

Fabric moves **child items with their parent**, such as a Lakehouse and its SQL endpoint.
A child cannot be moved independently; `CannotMoveChildOnly` remains a failure, without discovering
or moving its parent automatically. This does not claim that arbitrary related items or dependencies
move together. There is no client-side workload blacklist, cross-workspace move, data copy, rename,
folder creation, OneLake operation, or local filesystem access. This operation is separate from
`core_move-folder`.

These examples mutate real item placement when invoked with authorized credentials:

```powershell
fabmcp core move-item --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --item-id cccccccc-2222-3333-4444-dddddddddddd --target-folder-id bbbbbbbb-1111-2222-3333-cccccccccccc

# Move the item and its child items to the workspace root.
fabmcp core move-item --workspace-id aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb --item-id cccccccc-2222-3333-4444-dddddddddddd
```

**Only HTTP 200 confirms movement.** The typed result contains `moveStatus: "Moved"`, `items`,
`guidance`, and validated `retryAfterSeconds` or `retryAfterUtc` when returned. The API's
`MovedItems.value` is a required array, not a single item. Every entry is preserved in service
order, including child and repeated entries; nothing is deduplicated, truncated, or enriched.
There is no minimum row count or requirement to find the requested item in the response.
An empty array is valid, but a missing/null array or unusable entry is not.

Only each item's extensible `type` is required by the API schema. Available ID, workspace ID,
folder ID, logical ID, display name, description, tags, and sensitivity-label metadata are
projected; omitted/null optional fields are not inferred. Present UUIDs must be nonempty,
workspace IDs must match the request, and non-null folder IDs must match the destination
(a non-null folder conflicts with a root move). Child IDs need not equal the requested item.
No data, definitions, default identities, or arbitrary backend properties are exposed.
The complete JSON response is bounded to **1 MiB and 64 levels** before projection; no arbitrary
row cap is imposed and output remains shallow for the MCP envelope.

If HTTP 200 is followed by malformed, oversized, unreadable, canceled, or conflicting metadata,
the tool reports **confirmed movement with unusable metadata**, not that nothing moved.
Do not resubmit it. Pre-send/authentication failures report no request sent; an uncertain send
requires separate inspection before another move, never blind retry. Known HTTP failures retain
their status and validated `Retry-After`, even when optional error metadata is unusable.
Only safe error codes are exposed. Existing `core_get-item` and `core_list-items` support separate,
explicit selection/inspection; no hidden Get/List, per-child request, verification, automatic
retry, pagination, or LRO polling occurs.

The caller needs **Contributor or higher** workspace access; delegated callers require
`Workspace.ReadWrite.All`. Users, service principals, and managed identities retain the server's
configured authentication and applicable Fabric tenant controls. Placement changes are destructive,
idempotent by their placement effect, not secret-bearing, remote-capable, and excluded from read-only
mode. The host's normal mutation confirmation applies without an additional gate.
In `all` mode, text contains `results.items`; compact/duplicated output also provides
`structuredContent.items`. Namespace mode uses `core` with `command="core_move-item"` and `parameters`,
and its structured payload is under `result`. Single mode uses `fabric` with `tool="core"`.
Offline tests do not establish live Fabric, OBO/RBAC, or recorded-playback coverage.

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

**Updating workspace metadata**

* "Rename Fabric workspace \<workspace-id> to 'Finance Analytics' without changing its description"
* "Set the description of Fabric workspace \<workspace-id> to 'Quarterly reporting' without renaming it"
* "Clear the description of Fabric workspace \<workspace-id> and keep its display name"

`core_update-workspace` (`fabmcp core update-workspace`) implements the synchronous
[Update Workspace API](https://learn.microsoft.com/rest/api/fabric/core/workspaces/update-workspace).
It requires a nonempty workspace UUID and at least one update:

| Option | Behavior |
|--------|----------|
| `workspace-id` | Required workspace UUID; names and name resolution are not supported. |
| `display-name` | Optional name, up to 256 characters; cannot be empty or whitespace-only. Names must be unique within the tenant; `Admin monitoring` is reserved. |
| `description` | Optional description, up to 4000 characters. An empty string clears it. |

Omitted or null optional values are not sent and leave those properties unchanged. Supplied text is
not trimmed. For example, MCP arguments to clear only the description are:

```json
{ "workspace-id": "<workspace-id>", "description": "" }
```

The caller needs the **workspace Admin role** and, for delegated access, **Workspace.ReadWrite.All**.
This mutating tool is unavailable in read-only mode and uses the server's standard destructive-operation
confirmation. Its result contains `workspace` with only `id`, `displayName`, `type`, and `description`
when returned by Fabric; an empty returned description remains empty.

The tool makes one application-level PATCH with no application retries, prefetch, follow-up requests,
polling, or enrichment. The Fabric Core HTTP client does not automatically follow redirects.
Response URLs are not followed by application code. Capacity, domain,
identity, permissions, tags, endpoints, items, and data cannot be updated through this tool.
Failures retain the HTTP status and expose only sanitized messages and validated `Retry-After`
guidance. If a request times out or returns an invalid success payload, the update might already
have been applied; verify the workspace before manually retrying.

**Updating item metadata**

`core_update-item` implements [Update Item](https://learn.microsoft.com/rest/api/fabric/core/items/update-item):
one application-level `PATCH /v1/workspaces/{workspaceId}/items/{itemId}` with a synchronous `200` Item response.
It requires `workspace-id` and `item-id` as nonempty UUIDs, plus at least one of `display-name` or `description`.
It does not resolve names, fetch the item first, read item data, or change definitions, permissions, tags, or identity.

Omitted properties remain unchanged. An explicitly empty description clears it; JSON `null` is treated as omitted.
Descriptions may contain at most 256 characters and are not trimmed. Display names must not be empty or whitespace-only and must follow the item's
type-specific naming rules; there is no universal display-name length limit enforced by this tool.
For example, these MCP arguments clear only the description:

```json
{
  "workspace-id": "cfafbeb1-8037-4d0c-896e-a46fb27ff229",
  "item-id": "5b218778-e7a5-4d73-8187-f10824047715",
  "description": ""
}
```

The result contains an `item` with `id`, `displayName`, `type`, `workspaceId`, and an optional `description`.
Definitions, data, identities, tags, and workload-specific properties are not returned. Existing server output modes apply:
the default returns text content; compact and duplicated modes additionally expose the typed output schema and structured content.

The caller needs read and write permission on the item. Delegated callers need `Item.ReadWrite.All` or the corresponding
item-specific scope, such as `Notebook.ReadWrite.All`. Service-principal and managed-identity support depends on the item type.
The tool is mutating and idempotent, is unavailable in read-only mode, and uses the existing host authentication and confirmation policies.
It performs no automatic retry or long-running-operation polling. HTTP failures preserve their status and suppress backend details;
valid nonnegative integer `Retry-After` values provide wait guidance. After a timeout or invalid response, the update may already
have completed: verify the item's state before retrying.
The Fabric Core HTTP client does not automatically follow redirects.

#### Delete a Fabric item

`core_delete-item` sends one [Delete Item API](https://learn.microsoft.com/rest/api/fabric/core/items/delete-item) request. It does not resolve names, inspect item types, retry requests, or poll operations.
The Fabric Core HTTP client does not automatically follow redirects, including 307/308 responses that would otherwise repeat the DELETE.

| Parameter | Required | Behavior |
|-----------|----------|----------|
| `workspace-id` | Yes | Nonempty UUID of the workspace containing the item. |
| `item-id` | Yes | Nonempty UUID of the item to delete. |
| `hard-delete` | No | Omit to leave the API parameter unspecified, explicitly pass `false` to send `hardDelete=false`, or explicitly pass `true` to request permanent deletion. |

**Deletion is destructive in every mode.** Omission or `false` uses Fabric's default deletion behavior, which soft-deletes only supported item types. It does not guarantee that the item can be recovered. Rejected default/soft deletions are returned as errors; the tool never falls back to permanent deletion.

**Permanent deletion cannot be recovered.** The CLI requires an explicit value: `--hard-delete true`. A bare `--hard-delete`, invalid value, or repeated occurrence is rejected before authentication or deletion. MCP advertises an optional Boolean input and preserves the existing argument conversion behavior: an explicitly supplied string `"true"` or string array `["true"]` can also bind as one explicit true value. The command does not enforce the original JSON type. Omission, `null`, an empty value, and `false` never enable hard deletion; empty/invalid values are rejected.

Illustrative CLI syntax (replace the placeholders only when intentionally deleting an item):

```powershell
# Default API deletion behavior; recovery is not guaranteed.
fabmcp core delete-item --workspace-id "<workspace-uuid>" --item-id "<item-uuid>"

# Explicitly do not request permanent deletion.
fabmcp core delete-item --workspace-id "<workspace-uuid>" --item-id "<item-uuid>" --hard-delete false

# Explicitly request irreversible permanent deletion.
fabmcp core delete-item --workspace-id "<workspace-uuid>" --item-id "<item-uuid>" --hard-delete true
```

Soft deletion requires write permission on the item; hard/permanent deletion requires the workspace **Admin** role. Delegated callers need `Item.ReadWrite.All` or the corresponding item-specific write scope. Service principal and managed identity support depends on the item type. Existing host authentication and destructive-operation consent requirements apply; the tool is unavailable in read-only mode.

On the documented empty HTTP 200 response, the tool returns only `workspaceId`, `itemId`, and `hardDeleteRequested`. These identify the requested target and mode, not item metadata or a recovery guarantee. A 404 or other failure is an error, not a successful deletion. An upstream 400 reports Fabric rejection with item-state, soft-delete support, and tenant-setting guidance, not a missing-ID diagnosis. Throttling responses retain status 429 and include validated `Retry-After` guidance when available, without automatic retries. A genuinely canceled or timed-out request does not confirm deletion, but cancellation arriving after a completed HTTP 200 response does not discard that confirmation.

**Delete Workspace** implements the [Fabric Core Delete Workspace API](https://learn.microsoft.com/rest/api/fabric/core/workspaces/delete-workspace).

> [!WARNING]
> Deleting a workspace also deletes the items under it. Verify the exact workspace UUID and obtain authorization for that specific deletion before invoking this tool. No recovery, retention, or permanent-deletion semantics are promised by this tool.

The CLI command is `fabmcp core delete-workspace --workspace-id <workspace-uuid>`. Its only option, `workspace-id`, is required and must be a nonempty UUID. Invalid IDs are rejected before authentication or HTTP. Names, fuzzy lookup, implicit target selection, and bulk deletion are not supported.

The caller must have the **Admin** workspace role. Delegated callers additionally require **Workspace.ReadWrite.All**. The API documents support for users, service principals, and managed identities; this does not imply those identity flows have been exercised by the offline tests.

The tool sends one `DELETE /v1/workspaces/{workspaceId}` request with no body or automatic redirects. Only the documented synchronous, empty `200 OK` produces a typed acknowledgement: `{"workspaceId":"<canonical-workspace-uuid>","deleted":true}`. It does not read or deserialize a workspace response body, enumerate or individually delete items, change permissions, retry automatically, or poll. Failures retain their HTTP status and expose sanitized messages; 403 and 404 never count as successful deletion. An upstream 400 advises checking workspace state and tenant restrictions rather than claiming the validated UUID is missing. Validated `Retry-After` guidance can be returned for throttling. A genuinely canceled or timed-out request does not confirm the outcome; check the workspace state before attempting another deletion. A returned HTTP 200 still confirms success if cancellation arrives afterward.

The tool is marked destructive, not read-only, and idempotent in the no-additional-effects sense; repeated requests can still return 404. Read-only MCP servers hide and reject this tool. The existing MCP elicitation gate requires consent and rejects unsupported or declined consent unless explicitly disabled by the host's dangerous configuration. Annotations alone do not guarantee human confirmation, and direct CLI execution does not use the MCP elicitation gate. Implementation or test approval does not authorize real deletion; API-level tests use substituted HTTP and credentials only.

**Workspace capacity assignment**

Example prompt: "Submit a request to assign workspace \<workspace-id> to capacity \<capacity-id>; do not wait for completion."

`fabmcp core assign-workspace-to-capacity` requires both `--workspace-id` and `--capacity-id` as nonempty GUIDs. It uses [Assign To Capacity](https://learn.microsoft.com/rest/api/fabric/core/workspaces/assign-to-capacity) to send one POST with `{ "capacityId": "<target-capacity-id>" }`.

The documented response is **202 Accepted**, with no response body: assignment is in progress. The tool preserves status 202 and returns its own receipt:

```json
{
  "workspaceId": "cfafbeb1-8037-4d0c-896e-a46fb27ff512",
  "capacityId": "0f084df7-c13d-451b-af5f-ed0c466403b2",
  "accepted": true,
  "state": "Pending"
}
```

`capacityId` is the requested target, not a verified current assignment. The tool never reports completion, invents an operation handle, polls, or follows up automatically. A caller can later inspect `capacityId` and `capacityAssignmentProgress` through [Get Workspace](https://learn.microsoft.com/rest/api/fabric/core/workspaces/get-workspace). Assignment can still fail after acceptance.

The caller needs **workspace Admin** and **capacity Contributor or Admin** permissions, with both **Capacity.ReadWrite.All** and **Workspace.ReadWrite.All** scopes. The API supports users, service principals, and managed identities. Authentication uses the server's configured identity provider; remote OBO calls use the caller's permissions.

Workspaces containing Fabric items other than Power BI items cannot migrate across regions. Such workspaces can only be assigned to Fabric, Fabric trial, or Power BI Premium capacity. These restrictions are enforced by Fabric: the tool makes no preflight calls or eligibility guarantees. It does not create workspaces or provision, resize, or activate capacities; creating a workspace with a `capacityId` is a separate API operation.

Failures preserve the service's HTTP status and return sanitized guidance. On throttling, a valid `Retry-After` delay is reported without automatically retrying. If a timeout or network failure prevents confirmation, inspect the workspace before submitting again. The tool never chooses a different target capacity.

### Jobs

| Tool | Description |
|------|-------------|
| `jobs_cancel-item-job-instance` | Requests cancellation of one known item job with one POST. Returns cancellation requested, not a Cancelled job state, with known IDs and validated guidance for a separate Get. Never automatically retries or polls. |
| `jobs_create-item-schedule` | Creates one schedule with explicit enabled state, UTC bounds, time zone, configuration, and optional static execution data. Returns the created schedule, not job execution or success. No discovery, preflight, polling, or automatic retry. |
| `jobs_delete-item-schedule` | Deletes one explicitly identified schedule with one DELETE and no body. HTTP 200 returns a deletion receipt, not a snapshot or job cancellation. No discovery, follow-up verification, polling, or automatic retry. |
| `jobs_get-item-job-instance` | Reads one known item's job instance, including status, timing, safe failure metadata, and available retry guidance. Does not poll, run, cancel, or schedule jobs. |
| `jobs_get-item-schedule` | Reads one schedule by explicit workspace/item/schedule IDs and job type, preserving available configuration, owner, and execution data after disclosure consent. Returns a snapshot, not job status or a mutation receipt. No discovery, enrichment, polling, or automatic retry. |
| `jobs_list-item-job-instances` | Reads one complete page of an item's job snapshots, with safe failure metadata and an unchanged token for an explicitly requested next page. Does not fetch individual jobs or infer full history. |
| `jobs_list-item-schedules` | Reads one complete schedule page for an explicit item and job type, preserving available settings, owner, and execution data. Requires explicit continuation and never changes or executes schedules. |
| `jobs_run-on-demand-item-job` | Submits exactly one on-demand item job and returns an accepted receipt, a trustworthy job-instance ID when available, and validated first-status-read guidance. Never automatically retries or polls. |
| `jobs_update-item-schedule` | Updates one known schedule with explicit enabled state and full configuration in one PATCH. Returns the updated schedule, not job execution or cancellation. No hidden read/merge, polling, or automatic retry. |

**Cancel Item Job Instance (`jobs_cancel-item-job-instance`)**

Calls the [Cancel Item Job Instance API](https://learn.microsoft.com/rest/api/fabric/core/job-scheduler/cancel-item-job-instance)
with exactly one `POST /v1/workspaces/{workspaceId}/items/{itemId}/jobs/instances/{jobInstanceId}/cancel`.
The explicit `workspace-id`, `item-id`, and `job-instance-id` options must be nonempty UUIDs.
They are validated and normalized before authentication or HTTP. There is **no job-type or request
body**, no discovery or preflight Get, and no polling, waiting, automatic retry, or result retrieval.
Instance and schedule listing are separate read-only tools; schedule creation and updates require their own authorization.

Cancel is **destructive and not read-only**: obtain explicit cancellation authorization and normal
mutation consent. It can stop useful work and does not promise to roll back work already performed.
The idempotent hint describes the effect of repeatedly targeting the **same existing job**, not a
promise that repeated requests succeed; `JobAlreadyCompleted` remains a failure. The tool never
automatically retries, even for throttling or a backend `isRetriable` hint. Read-only mode hides and
rejects Cancel, Run, Create Schedule, and Update Schedule while retaining Get and List.

The caller needs item execution permission and, for delegated access, `Item.Execute.All` or the
corresponding item-specific scope such as `Notebook.Execute.All`. The API supports users, service
principals, and managed identities, subject to Fabric authorization. The existing request-aware
host credential provider is used in both stdio and HTTP modes, without granting permissions or
bypassing caller identity.

**HTTP 202 means cancellation was requested, not that the job is `Cancelled`.** The typed receipt
contains `cancellationStatus: "Requested"`, the requested `workspaceId`, `itemId`, `jobInstanceId`,
and `guidance`. Those IDs come from the request: the tool ignores `Location` and
`x-ms-operation-id`, never follows or echoes response URLs, and does not read the acceptance body.
Missing or malformed response metadata and cancellation arriving after response headers do not
erase confirmed acceptance.

A single valid `Retry-After` becomes `retryAfterSeconds` or `retryAfterUtc`. Wait at least that
delay, or until that UTC date, before the first separate status read. Missing, invalid, negative,
out-of-range, or multiple guidance produces `retryAfterIssue: "MissingOrInvalidRetryAfter"`;
acceptance remains intact and no fallback delay is invented.

Failures preserve HTTP status and validated retry guidance. `JobAlreadyCompleted` means the
cancellation was not accepted because the job already completed; it does **not** establish the
job's final success or failure status. Error-code extraction requires unambiguous JSON bounded
to **1 MiB (1,048,576 bytes)** and a safe identifier-shaped code. Malformed, oversized, unreadable,
or canceled error-body reads retain the known HTTP failure and explicitly report unavailable
metadata. Raw backend messages, nested details, exception text, and stack traces are not returned.

Network failure, timeout, or cancellation after a possible POST can leave the outcome uncertain.
Check the **known job** with `jobs_get-item-job-instance` before considering another cancellation
request. Canceling the MCP request itself is not canceling a Fabric job. Job-instance IDs are not
Fabric `/operations` IDs; never use the operation-state/result tools for this flow.

Cancel-to-Get handoff as **separate calls**, using only an explicitly authorized existing job:

```powershell
$cancellation = fabmcp jobs cancel-item-job-instance --workspace-id <workspace-id> --item-id <item-id> --job-instance-id <job-instance-id> | ConvertFrom-Json
$receipt = $cancellation.results
$receipt
```

After an accepted receipt, wait at least its valid Retry-After guidance. If timing is unavailable,
do not invent an immediate-read delay. Then make a separate status call:

```powershell
fabmcp jobs get-item-job-instance --workspace-id $receipt.workspaceId --item-id $receipt.itemId --job-instance-id $receipt.jobInstanceId
```

The snapshot may still report `InProgress`; Get reports the observed state of that known job.
Both calls work with `--namespace jobs`; select both `core` and `jobs` when also using Core tools.
Namespace routing uses `command="jobs_cancel-item-job-instance"` with the three IDs in `parameters`;
single routing additionally uses `tool="jobs"`. Default, compact, and duplicated output retain the
same receipt semantics.

Example prompts (require explicit cancellation authorization):

- "Request cancellation once for job `<job-instance-id>` of item `<item-id>` in workspace `<workspace-id>`. Return the receipt without polling or retrying."
- "After the cancellation receipt's minimum Retry-After wait, read that known job once using Get. Do not infer that it is already Cancelled."

**Get Item Job Instance (`jobs_get-item-job-instance`)**

Calls the [Get Item Job Instance API](https://learn.microsoft.com/rest/api/fabric/core/job-scheduler/get-item-job-instance)
with exactly one `GET /workspaces/{workspaceId}/items/{itemId}/jobs/instances/{jobInstanceId}`.
The three options `workspace-id`, `item-id`, and `job-instance-id` are required nonempty UUIDs,
validated and normalized before authentication or HTTP.

```powershell
fabmcp jobs get-item-job-instance --workspace-id <workspace-id> --item-id <item-id> --job-instance-id <job-instance-id>
```

The result contains `jobInstance` with matching `id` and `itemId`, plus nonempty `jobType`, `invokeType`,
and `status`. Status values include `NotStarted`, `InProgress`, `Completed`, `Failed`, `Cancelled`, and
`Deduped`; future status, invocation, and job-type strings are preserved without an allowlist.
**HTTP 200 means the snapshot was read, not that the job succeeded.** A failed job stays `Failed`.

Optional `rootActivityId`, `startTimeUtc`, `endTimeUtc`, and `failureReason` are returned when available;
absent/null values are omitted rather than fabricated. The API declares timestamps as plain UTC
strings and demonstrates values without a timezone suffix. They are returned verbatim, including
fractional precision or empty strings, without interpreting them in the host's local timezone.
Failure information is restricted to a safe identifier-shaped `errorCode` (up to 128 ASCII letters,
digits, underscores, hyphens, or periods), a nonempty `requestId` UUID, and `isRetriable` when provided.
Backend messages, nested details, parameters, related resources, and unknown fields are not exposed.

A valid `Retry-After` header becomes `retryAfterSeconds`, or `retryAfterUtc` for an HTTP date.
Missing, invalid, negative, out-of-range, or multiple headers are omitted; no fallback delay is
invented. HTTP failures retain their status and valid retry guidance with sanitized messages,
without reading backend error bodies. Only HTTP 200 with a valid `application/json` snapshot is
accepted. Responses are bounded to **1 MiB (1,048,576 bytes)**, including streaming responses
without `Content-Length`; malformed or oversized responses fail explicitly.

This read-only tool works in stdio and HTTP modes. In `all` mode, compact and duplicated structured
output expose `structuredContent.jobInstance`; default content-only responses use `results.jobInstance`.
The caller needs item read access and the API's delegated `Item.Read.All`, `Item.ReadWrite.All`,
or applicable item-specific read/read-write scope. The API supports users, service principals,
and managed identities; the tool uses the host's configured credential without granting permissions.

Job instances are **not** Fabric `/operations` long-running operations. Do not pass a job-instance ID
to `core_get-operation-state` or `core_get-operation-result`. This tool does not discover items,
jobs, or job types; poll or wait; fetch results; follow response URLs; retry automatically; or run,
cancel, or schedule a job.

For an item-metadata-to-job workflow, start the server with
`fabmcp server start --mode all --namespace core --namespace jobs`. Call `core_get-item` with the known
workspace and item UUIDs, then pass the returned `workspaceId` and `id` to `jobs_get-item-job-instance`
together with a separately supplied existing `job-instance-id`. Item metadata does not discover or
create a job-instance ID. The same combined selection is required when using Core definition tools
and Jobs in one session, including in namespace or single mode.

Example prompts:

- "Read job instance `<job-instance-id>` for item `<item-id>` in workspace `<workspace-id>` once."
- "Show that job instance's status and safe failure information; do not wait or retrieve results."
- "Get its latest snapshot and any Retry-After guidance without starting another job."

**List Item Job Instances (`jobs_list-item-job-instances`)**

Calls the [List Item Job Instances API](https://learn.microsoft.com/rest/api/fabric/core/job-scheduler/list-item-job-instances)
with exactly one `GET /v1/workspaces/{workspaceId}/items/{itemId}/jobs/instances`.
Explicit `workspace-id` and `item-id` are required nonempty UUIDs, validated and normalized before
authentication. There is no job-type, status filter, page-size/limit option, or request body.

```powershell
fabmcp jobs list-item-job-instances --workspace-id <workspace-id> --item-id <item-id>

# A separate invocation for the next page, using the same IDs and unchanged returned token.
fabmcp jobs list-item-job-instances --workspace-id <workspace-id> --item-id <item-id> --continuation-token '<returned-token>'
```

The result contains `jobInstances`, including every row in the response page in its original order.
An empty array is valid. Rows are neither truncated, deduplicated, nor enriched with per-job GETs.
Each instance has a nonempty UUID and must match the requested item. Future job-type, invocation,
and status strings, optional UTC timestamp strings, trace IDs, and safe failure metadata follow
the Get projection described above. **A successful list read does not mean the jobs succeeded.**
No raw failure messages, nested details, parameters, related resources, or backend payloads are exposed.

When Fabric returns a `continuationToken`, pass it unchanged with the same IDs in a separate call.
Raw query characters are encoded safely; existing percent escapes are preserved rather than
double-encoded. Tokens are never trimmed or logged. The generic tool input budget is **8,192 UTF-8
bytes**; blank tokens, control characters, and invalid Unicode are rejected before authentication.
These are resource/URI safeguards, not a workload-specific token format. The tool accepts no
continuation-URL option, never follows or returns `continuationUri`, and validates a supplied
response URI against the fixed Fabric origin, requested collection, and token.

When both continuation fields are absent or JSON null, in any combination, the
[Fabric pagination contract](https://learn.microsoft.com/rest/api/fabric/articles/pagination)
marks the final page. Empty and nonempty final pages are preserved. A valid token with an absent
or null URI is sufficient to continue, even when the current page is empty. A non-null URI without
a token, wrong-type fields, invalid tokens, duplicate fields, or contradictory non-null continuation
metadata fail explicitly, rather than silently appearing exhausted. Missing/null/wrong-type `value`,
malformed rows, duplicate or ambiguously cased JSON properties, invalid Unicode (including ignored
fields), and responses exceeding **1 MiB (1,048,576 bytes)** or **64 JSON nesting levels** also fail
explicitly without returning a partial page. The shared response limit is unchanged.

For most items, the API reports **up to 100 recently completed instances**, with no corresponding
limit for active instances. This documented listing limit is **not a 100-row page cap** or a guarantee
of complete job history. It does not establish physical retention or whether older instance IDs
remain accessible through Get. The tool adds no count limit, total count, or ordering guarantee.
Validated `Retry-After` seconds or a UTC HTTP date are returned when present. HTTP failures keep
their status and retry guidance with safe messages; error bodies are not read and requests are not retried.

List is read-only, non-destructive, idempotent, and available in stdio/HTTP and read-only mode without
a mutation-consent prompt. The API requires item read access and delegated `Item.Read.All`,
`Item.ReadWrite.All`, or the corresponding item-specific scope. User, service-principal, and managed
identities use the existing host credential and authorization behavior.
In `all` mode, default responses use `results.jobInstances`; compact/duplicated output also exposes
`structuredContent.jobInstances`. Namespace routing uses `command="jobs_list-item-job-instances"`
and the IDs/token in `parameters`; single mode adds `tool="jobs"`. Both routers support `learn=true`.

A caller can explicitly choose a returned `id` and use it with the same workspace and returned
`itemId` in a separate `jobs_get-item-job-instance` call. Selecting a result for cancellation still
requires explicit authorization and the separate Cancel tool's consent.
Listing does not poll, discover names or job types, retrieve results, run, cancel, or schedule jobs.
**Do not assume the latest listed instance identifies an earlier ambiguous Run submission or one
without a tracking ID.** Listing history does not correlate such a receipt or justify resubmission.

Example prompts:

- "List one page of jobs for item `<item-id>` in workspace `<workspace-id>`, without per-job reads."
- "Get the next page using that unchanged continuation token and the same workspace/item IDs."
- "Read the explicitly selected job ID from this page with Get; do not start or cancel any jobs."

**Create Item Schedule (`jobs_create-item-schedule`)**

Calls the [Create Item Schedule API](https://learn.microsoft.com/rest/api/fabric/core/job-scheduler/create-item-schedule)
with exactly one `POST /v1/workspaces/{workspaceId}/items/{itemId}/jobs/{jobType}/schedules`.
Supply nonempty `workspace-id` and `item-id` UUIDs, an explicit `job-type`, `enabled` as **true or
false**, and a `configuration` JSON object. There is no default enabled state, cadence, time zone,
job type, name lookup, schedule-count preflight, or automatic retry. Fabric enforces its documented
maximum of 20 schedules per item; this tool does not list or delete schedules to make room.

**Before requesting mutation consent, confirm the timing and enabled intent: an enabled schedule
whose start time is in the past may trigger a job immediately, consuming compute or changing data.**
The tool is destructive, non-idempotent, sensitive, and excluded from read-only mode. Repeating a
creation can create another schedule. The host's existing mutation/sensitive-data confirmation
applies; do not treat these examples as permission to create a live schedule.

Configuration requires `type`, `startDateTime`, `endDateTime`, and `localTimeZoneId`. Supply UTC
bounds with end later than start; prefer `Z`. Zero UTC offsets and offset-less timestamps from
the API examples are accepted and preserved as UTC text, never interpreted in the host's local
time zone. Nonzero offsets are rejected rather than rewritten. Supply the intended **Windows
time-zone identifier** explicitly; the tool neither looks it up on the host nor converts times.

| Configuration type | Additional required fields |
|--------------------|----------------------------|
| `Cron` | Integer `interval` from 1 to 5,270,400 **minutes**, not a Unix cron-expression field |
| `Daily` | `times`: 1-100 strings in `HH:mm` format |
| `Weekly` | `times` as above and `weekdays`: 1-7 day strings such as `Monday` |
| `Monthly` | `times`, integer `recurrence` from 1 to 12, and `occurrence` |

Monthly `occurrence` requires `occurrenceType`: `DayOfMonth` includes integer `dayOfMonth` from
1 to 31; `OrdinalWeekday` includes `weekIndex` (for example `First` or `Fifth`) and `weekday`.
Fabric skips dates that do not occur in a month; the tool does not simulate calendars, reject
February 31, or predict the next run. Future configuration/occurrence and string-enum values,
extension properties, and array ordering remain intact; Fabric decides workload support.

Optional `execution-data` is a JSON object passed as **fixed static** `executionData` for the
specific item/job type. Omission omits the property; `{}` remains an empty object. Its contents
are opaque, including case-sensitive keys and nested values. There is no job-parameters input,
parameter stripping, or retry with a reduced body. Each JSON input has a **1 MiB (1,048,576-byte)
UTF-8 / 32-level** budget; the combined serialized body also must fit 1 MiB. Duplicate properties,
malformed Unicode, ambiguous configuration metadata, and invalid documented shapes fail before
authentication. Job types use the existing 1,024-byte budget and single-path-component encoding.

```powershell
# Example only: use explicitly approved IDs and timing. This creates a disabled schedule.
$configuration = '{"type":"Daily","startDateTime":"2030-01-01T00:00:00Z","endDateTime":"2030-02-01T00:00:00Z","localTimeZoneId":"UTC","times":["09:00","15:00"]}'
fabmcp jobs create-item-schedule --workspace-id <workspace-id> --item-id <item-id> --job-type DefaultJob --enabled false --configuration $configuration
```

The caller needs the appropriate Fabric permissions and, for delegated access, **both
`Item.Execute.All` and `Item.ReadWrite.All`**. The API supports users, service principals, and
managed identities subject to Fabric authorization. Existing host/OBO credentials are reused;
offline tests do not establish live permissions or real OBO behavior.

**HTTP 201 confirms schedule creation, not job execution or success.** The typed result contains
`creationStatus: "Created"`, requested `workspaceId`, `itemId`, `jobType`, `schedule`, and `guidance`.
The schedule requires a nonempty `id` and Boolean `enabled`. Available `createdDateTime`,
`configuration`, `owner`, and `executionData` are preserved without inventing absent/null fields.
Owner and execution data may be sensitive. Unknown top-level fields are not projected.
The response body budget is **1 MiB / 64 JSON levels**. The retained schedule snapshot is limited to
**60 levels including its root**, reserving four levels for the result, namespace, tool-response,
and JSON-RPC envelopes. This is checked inside the confirmed-creation boundary, before any output
serializer can lose that confirmation. Unknown top-level fields that are not returned retain the
64-level body budget. Optional `Location` and operation headers are
ignored: this endpoint is not a standard Fabric operation LRO and never triggers a follow-up read.
Schedule IDs must not be used as job-instance or operation IDs.

If a 201 response body is unusable, the tool explicitly reports **creation confirmed but metadata
unavailable**, not that nothing was created. Network failures, timeouts, or cancellation before
confirmation leave creation uncertain. Inspect existing schedules separately before any new
creation; **do not blindly resubmit**. HTTP failures retain their status, safe error code when
available (including `ScheduleExceedsLimit`), and valid Retry-After seconds/date guidance.
Unreadable optional error metadata never replaces that status or retry guidance. Raw backend
messages, nested error details, and exception traces are not returned.

Default output uses `results.schedule`; compact/duplicated output includes `structuredContent.schedule`.
Namespace mode calls `jobs` with `command="jobs_create-item-schedule"` and all options in
`parameters`; single mode calls `fabric` with `tool="jobs"` and the same command/parameters.
Inline JSON options work in stdio and HTTP modes. All routes require the same consent and
explicit intent, and none performs a follow-up job run.

**Delete Item Schedule (`jobs_delete-item-schedule`)**

Calls the [Delete Item Schedule API](https://learn.microsoft.com/rest/api/fabric/core/job-scheduler/delete-item-schedule)
with exactly one `DELETE /v1/workspaces/{workspaceId}/items/{itemId}/jobs/{jobType}/schedules/{scheduleId}`.
Supply explicit nonempty `workspace-id`, `item-id`, and `schedule-id` UUIDs and the workload `job-type`.
UUIDs are validated and normalized before authentication; job types are validated within the existing
**1,024 UTF-8 byte** tool budget and encoded as one path component without discovery or defaults.
There is **no request body**, configuration, enabled flag, execution data, or name lookup.

The tool is destructive, requires MCP mutation consent, and is hidden and denied in read-only mode.
Its idempotent hint describes deleting the same resource, not a guarantee of repeated success:
**404 and other failures are never converted to confirmed deletion.** The caller needs appropriate
Fabric permissions and delegated **`Item.ReadWrite.All` or the corresponding item-type
`ReadWrite.All` scope**. This endpoint does not require the Create/Update combination of
`Item.Execute.All` and `Item.ReadWrite.All`. Users, service principals, and managed identities reuse
the host's existing authentication, subject to Fabric authorization.

```powershell
# Example only: use an explicitly authorized schedule. This does not cancel running jobs.
fabmcp jobs delete-item-schedule --workspace-id <workspace-id> --item-id <item-id> --job-type DefaultJob --schedule-id <schedule-id>
```

**Only HTTP 200 confirms deletion.** The result is a receipt containing `deletionStatus: "Deleted"`,
the requested `workspaceId`, `itemId`, `jobType`, `scheduleId`, and `guidance`; it does not fabricate a
returned schedule snapshot. The documented empty body is sufficient. Success bodies, Location,
operation headers, and late request cancellation do not erase confirmation or trigger another call.
Unexpected 2xx responses, including 202 and 204, do not establish this endpoint's documented success.
This synchronous API does not use the Fabric operation runner.

Invalid inputs, failed authentication, and cancellation before sending do not send a DELETE.
After a possible send with no confirmation, the outcome may be uncertain: inspect the target with
Get or List Schedules **in a separate invocation** before considering another deletion; never retry
blindly. A later snapshot does not prove which earlier request changed the schedule.
HTTP failures retain their status, safe identifier-shaped error code when available, and valid
Retry-After seconds/date guidance. Optional error JSON is bounded to **1 MiB**; malformed Unicode,
oversized data, failed reads, and non-JSON bodies do not replace known status or retry guidance.
Raw backend messages, nested details, and exception traces are not exposed.

Deleting a schedule **does not cancel already-running jobs or promise rollback of completed work**.
Job cancellation remains a separate tool and separate authorization. Canceling the MCP request
does not undo a deletion. There is no hidden Get/List, verification, dependency operation, retry,
polling, result retrieval, or job run/cancellation. Schedule IDs are not job-instance or operation IDs.

In `all` mode, default responses expose the receipt under `results`; compact/duplicated output
uses `structuredContent`. Namespace mode calls `jobs` with `command="jobs_delete-item-schedule"`
and the four options in `parameters`; its structured payload is under `result`. Single mode calls
`fabric` with `tool="jobs"` and the same command/parameters. Both stdio and HTTP use the same consent
and read-only restrictions. Direct CLI execution does not itself use MCP elicitation; these examples
and offline tests do not authorize a live deletion.

**Update Item Schedule (`jobs_update-item-schedule`)**

Calls the [Update Item Schedule API](https://learn.microsoft.com/rest/api/fabric/core/job-scheduler/update-item-schedule)
with exactly one `PATCH /v1/workspaces/{workspaceId}/items/{itemId}/jobs/{jobType}/schedules/{scheduleId}`.
Supply explicit nonempty `workspace-id`, `item-id`, and `schedule-id` UUIDs and a workload `job-type`.
**Both `enabled` (true or false) and full `configuration` JSON are required, despite the PATCH verb.**
To change only enabled state, still supply the full intended configuration. If you need existing
settings, call List Schedules separately and explicitly select the target schedule; Update never
performs a hidden Get/List, lookup, read/merge/PATCH, or schedule-count preflight.

Configuration uses the same documented shapes and bounds as Create above: explicit type, ordered UTC
start/end, Windows time-zone ID, and the type's timing fields. **Cron means an interval in minutes,
not a Unix cron expression.** Offset-less API examples remain UTC text; no machine time zone,
cadence, start/end, or enabled defaults are inferred. Future types/fields, array order, and opaque
workload data are preserved. No local calendar simulation or workload-specific scheduling policy
is added. Job types have the same 1,024-byte UTF-8 single-path-component budget.

Optional `execution-data` is a fixed static, workload/job-type-defined JSON object. Omission omits
`executionData` from the PATCH; `{}` remains an empty object, while JSON `null`, arrays, and scalars
are rejected. **The API does not document whether omission preserves or clears stored execution data.**
The tool makes neither guarantee and never fetches, fills in, or strips fields to compensate.
Each JSON input has a 1 MiB UTF-8 / 32-level budget; the complete serialized request also must fit
1 MiB. Invalid, duplicate, ambiguous, or malformed Unicode inputs fail before authentication.

Before requesting consent, confirm the calling identity and explicit enabled/timing intent.
**An enabled schedule with a past start may trigger a job immediately, consuming compute or changing
data. Disabling a schedule does not cancel an existing job instance or roll back work.** The
[schedule owner](https://learn.microsoft.com/fabric/fundamentals/job-scheduler#schedule-ownership-and-item-takeover)
is the identity that created or last modified the schedule, separately from item ownership.
Updating a schedule must not be treated as an ownership-neutral read or proof that its future jobs
can authenticate successfully. The API exposes no owner override here.

The tool is destructive, sensitive, not read-only, and does not advertise an idempotent replay
guarantee. It is hidden and denied in read-only mode. The normal MCP mutation/sensitive-data
confirmation applies; direct CLI execution does not itself use that MCP elicitation gate.
The caller needs appropriate Fabric permissions and both delegated **`Item.Execute.All` and
`Item.ReadWrite.All`** scopes. Users, service principals, and managed identities are supported subject
to workload/tenant authorization. Existing host and HTTP/OBO credentials are reused.

```powershell
# Example only: use approved IDs and full timing intent. This does not cancel running jobs.
$configuration = '{"type":"Daily","startDateTime":"2030-01-01T00:00:00Z","endDateTime":"2030-02-01T00:00:00Z","localTimeZoneId":"UTC","times":["09:00","15:00"]}'
fabmcp jobs update-item-schedule --workspace-id <workspace-id> --item-id <item-id> --job-type DefaultJob --schedule-id <schedule-id> --enabled false --configuration $configuration
```

**HTTP 200 confirms the update, not job execution or success.** The typed result contains
`updateStatus: "Updated"`, the requested `workspaceId`, `itemId`, `jobType`, `schedule`, and `guidance`.
The returned schedule ID must match the requested ID; an example response with a different ID is
not an identity rule. Available configuration, owner, execution data, and UTC creation time are
preserved without synthesizing absent/null metadata from the input. Sensitive fields are returned
only through the existing consent-controlled tool. Schedule IDs are not job-instance or operation IDs.

The response body retains the 1 MiB / 64-level budget. Retained schedule output has the same
60-level limit as Create, reserving four levels for result/routing/protocol envelopes. Identity,
Unicode, and retained-output validation occur **inside the confirmed-200 boundary**: if metadata
is invalid, too deep, too large, or unreadable, the error still reports confirmed update and warns
against blind resubmission. Cancellation after headers does not erase that confirmation.
Loss of confirmation during sending remains uncertain; inspect the target schedule separately
before another update. HTTP failures keep their status, safe error code when available, and valid
Retry-After seconds/date guidance even when optional error metadata is unreadable. Raw backend
messages and exception traces are not returned.

This synchronous endpoint never uses the Fabric operation runner or follows Location/operation
headers. There is no automatic retry, field-stripping fallback, polling, result retrieval, or
follow-up job execution. Default output uses `results.schedule`; compact/duplicated `all` output
uses `structuredContent.schedule`. Namespace mode calls `jobs` with
`command="jobs_update-item-schedule"` and options in `parameters`; its structured payload is under
`result`. Single mode calls `fabric` with `tool="jobs"` and the same command/parameters.
Inline JSON works in stdio and HTTP. Examples and offline validation do not authorize live changes
or establish real OBO/RBAC behavior.

**Get Item Schedule (`jobs_get-item-schedule`)**

Calls the [Get Item Schedule API](https://learn.microsoft.com/rest/api/fabric/core/job-scheduler/get-item-schedule)
with exactly one `GET /v1/workspaces/{workspaceId}/items/{itemId}/jobs/{jobType}/schedules/{scheduleId}`.
Supply explicit nonempty `workspace-id`, `item-id`, and `schedule-id` UUIDs and the workload `job-type`.
IDs are normalized before authentication; job type is encoded as one path component with the existing
**1,024 UTF-8 byte** budget. Blank job types, invalid Unicode, control characters, and complete `.`
or `..` path segments are rejected. There is no name lookup, List preflight, job-type discovery,
per-field enrichment, pagination, polling, automatic retry, request body, or mutation.

```powershell
# Read only the explicitly authorized schedule; these placeholders are not live fixtures.
fabmcp jobs get-item-schedule --workspace-id <workspace-id> --item-id <item-id> --job-type DefaultJob --schedule-id <schedule-id>
```

The typed result contains the requested `workspaceId`, `itemId`, `jobType`, and matching `schedule`.
Only the schedule's nonempty `id` and Boolean `enabled` are required. Available `createdDateTime`,
`configuration`, `owner`, and `executionData` are preserved; absent/null fields stay absent without
default settings or creation-time input requirements. Present configuration and owner metadata are
validated against their documented shapes, with future types and nested extension fields retained.
Other top-level properties are not projected. UTC timestamp text (including offset-less examples)
and time-zone IDs are not converted. Cron `interval` means minutes, not a cron expression.

**Configuration, owner, and static execution data can be sensitive.** Get is read-only, idempotent,
non-destructive, and remote-capable, but still requires the host's disclosure confirmation in
read-only mode. Declined or unavailable confirmation makes no Fabric request. Direct CLI invocation
does not itself use MCP elicitation, so authorize the read and disclosure before executing it.
The caller needs Fabric item read permission and delegated `Item.Read.All`, `Item.ReadWrite.All`,
or the corresponding item-specific scope, such as `Notebook.Read.All`. Users, service principals,
and managed identities use the existing configured host/HTTP credentials, subject to Fabric and
tenant authorization. Offline tests are not proof of real OBO/RBAC access.

**A schedule snapshot is not job execution status or proof of a previous ambiguous create/update.**
An enabled schedule does not prove a job ran or succeeded. Owner identifies the creator or last
modifier, independently of item ownership. Reading neither changes ownership nor authorizes an
update, deletion, job run, or cancellation. Schedule IDs are not job-instance or operation IDs;
Location and operation headers are ignored, never followed.

The response must be JSON within **1 MiB (1,048,576 bytes)** and **64 raw nesting levels**.
Retained schedule JSON is limited to **60 levels**, reserving space for result/routing/JSON-RPC
envelopes in every output mode. Malformed or duplicate metadata, invalid Unicode, a wrong schedule
ID, exceeded budgets, and unreadable bodies fail without returning a partial or unrelated snapshot.
Get errors never claim a confirmed creation or update. HTTP failures preserve status, safe error
codes when available, and validated `Retry-After` seconds/date guidance even if optional error
metadata cannot be read. Raw backend messages and exception traces are not returned.

Default `all` output uses `results.schedule`; compact/duplicated output includes
`structuredContent.schedule`. In namespace mode, call `jobs` with
`command="jobs_get-item-schedule"` and the four options in `parameters`; the structured payload is
under `result`. Single mode calls `fabric` with `tool="jobs"` and the same command/parameters.
Both routers support non-executing `learn=true`, and all routes support stdio and HTTP.

**List Item Schedules (`jobs_list-item-schedules`)**

Calls the [List Item Schedules API](https://learn.microsoft.com/rest/api/fabric/core/job-scheduler/list-item-schedules)
with exactly one `GET /v1/workspaces/{workspaceId}/items/{itemId}/jobs/{jobType}/schedules`.
Explicit `workspace-id` and `item-id` must be nonempty UUIDs; `job-type` is a supplied workload
string, not a discovered value. Inputs are validated before authentication. The job type is encoded
as one path component and limited to **1,024 UTF-8 bytes**, matching the existing Jobs input budget.
Blank strings, invalid Unicode, control characters, and the complete dot segments `.` and `..`
are rejected. No status/enabled filter, page-size/limit, schedule ID, or request body is accepted.

```powershell
fabmcp jobs list-item-schedules --workspace-id <workspace-id> --item-id <item-id> --job-type DefaultJob

# Request another page explicitly with the same IDs, job type, and unchanged returned token.
fabmcp jobs list-item-schedules --workspace-id <workspace-id> --item-id <item-id> --job-type DefaultJob --continuation-token '<returned-token>'
```

The result contains `schedules` in the response's original order, without truncation, deduplication,
or per-schedule GETs. Empty pages are valid. Each entry requires a nonempty `id` and Boolean `enabled`.
The current List schema makes `createdDateTime`, `configuration`, `owner`, and `executionData`
optional; absent/null fields remain absent, rather than receiving invented defaults.
Configuration, owner, and execution data retain their JSON, including future types and nested
extension fields. Other top-level row fields are not projected.
**Owner information and execution data may be sensitive**; the tool is marked sensitive and uses
the host's existing disclosure confirmation. Declined confirmation sends no Fabric request.

Available configuration must have its documented type, start/end timestamps, and time-zone ID.
Known Cron, Daily, Weekly, and Monthly configurations are checked for their documented field
shapes and bounds, while future configuration, occurrence, and principal types stay extensible.
Cron `interval` is **minutes**, not a Unix cron expression. UTC timestamp strings, including
offset-less API examples, and the supplied `localTimeZoneId` are preserved without conversion or
local time-zone lookup. The tool does not evaluate calendars, predict the next run, or infer that
an enabled schedule has executed successfully. Schedule IDs are not job-instance or operation IDs.

For another page, pass the unchanged `continuationToken` with the same workspace, item, and job type.
Raw token spans are encoded and existing percent escapes are preserved, with the existing
**8,192 UTF-8 byte** token budget. Tokens are never trimmed or logged. A response `continuationUri`
is validated against the fixed origin, exact requested route/job type, and token, but is never
returned or followed. Opaque cursor `/./` and `/../` values remain query data, not path traversal.
Absent or null continuation fields in any combination mark the final page. A token with an absent
or null URI permits explicit continuation even from an empty page. Non-null URI-only, wrong-type,
duplicate, or contradictory continuation metadata fails explicitly, never as silent exhaustion.

The unchanged response budget is **1 MiB (1,048,576 bytes)** and **64 JSON nesting levels**.
Malformed required fields, invalid Unicode, duplicate properties, and ambiguously cased metadata
fail without returning a partial page. Case-sensitive keys in opaque execution data remain distinct.
The tool adds **no schedule-count or job-history limit**. Valid `Retry-After` seconds or a UTC
HTTP date are returned when available. HTTP errors preserve status and retry guidance without
reading or exposing backend error bodies, retrying, or following redirects.

List Schedules is read-only, non-destructive, idempotent, remote-capable, and discoverable in
read-only mode; sensitive-data confirmation is independent of mutation permissions.
The API requires item read access and delegated `Item.Read.All`, `Item.ReadWrite.All`, or the
corresponding item-specific scope. User, service-principal, and managed identities reuse the
existing Fabric host authentication. No real OBO/RBAC or live Fabric validation is claimed.
Default `all` output uses `results.schedules`; compact/duplicated output includes
`structuredContent.schedules`. In namespace mode, call `jobs` with
`command="jobs_list-item-schedules"` and the explicit context in `parameters`; single mode adds
`tool="jobs"` to the `fabric` router. Both support `learn=true` without a Fabric request.

Example prompts:

- "List one page of schedules for this workspace/item and job type, without individual lookups."
- "Use the same IDs and job type with the unchanged returned token for one more page."
- "Show the returned schedule configuration without converting time zones or starting a job."

**Run On-Demand Item Job (`jobs_run-on-demand-item-job`)**

Calls the [Run On Demand Item Job API](https://learn.microsoft.com/rest/api/fabric/core/job-scheduler/run-on-demand-item-job)
with one `POST /v1/workspaces/{workspaceId}/items/{itemId}/jobs/{jobType}/instances`.
Explicit `workspace-id`, `item-id` (nonempty UUIDs), and `job-type` are required. The job type is encoded
as one path component, not a query parameter. There is no default job type, workload allowlist,
item/job discovery, or preflight request.

Run **may consume compute or change data**. It is destructive, non-read-only, and non-idempotent;
calling it again can start another job. The caller needs item execution permission and, for delegated
access, `Item.Execute.All` or the corresponding item-specific scope such as `Notebook.Execute.All`.
The API supports users, service principals, and managed identities, subject to Fabric and workload
authorization. The host's existing credential provider is used in both stdio and HTTP modes; the
tool does not grant permissions or bypass OBO. Read-only mode excludes and rejects Run without
changing Get's or List's read-only behavior.

Optional inputs:

| Option | API field | Contract |
|--------|-----------|----------|
| `execution-data` | `executionData` | A JSON object containing opaque workload-specific execution data. |
| `job-parameters` | `parameters` | A JSON array of objects with required string `name` and `type` fields and a `value` field. Names are case-insensitively unique and at most 256 characters. Future type strings, opaque values (including scalars), and extension properties are preserved. |

Omitted inputs stay omitted; omitting both sends **no body**. Explicit `{}` execution data and `[]`
parameters are retained, not replaced with defaults. The tool rejects malformed/duplicate JSON,
invalid envelopes, and parameter-name violations before authentication or HTTP. Each JSON option has
a **1 MiB UTF-8 input budget and 32-level nesting limit**; the complete serialized request, including
its envelope and escaping, also has a **1 MiB (1,048,576 byte) budget**. The job-type tool budget is
1,024 UTF-8 bytes; blank values, control characters, invalid Unicode, and `.`/`..` path segments are
rejected. These are generic tool resource/URI safeguards, not workload-specific content rules.
As with other commands, the shared MCP argument binder treats an option whose RPC value is JSON
`null` as omitted. To provide a JSON payload, use the documented string option; the JSON text `"null"`
is rejected rather than sent as an execution-data object or parameter array.

Parameter support varies by item and job type. `FeatureNotAvailable` or any other failure does **not**
cause parameters to be removed or the POST to be retried. The option is named `job-parameters`
deliberately: in namespace/single mode, the router's outer `parameters` remains the argument object,
and its `job-parameters` member contains the JSON array string.

**HTTP 202 means accepted, not running, completed, or succeeded.** The receipt has
`submissionStatus: "Accepted"`, the requested `workspaceId`, `itemId`, and `jobType`, and `guidance`.
When a single Location identifies the documented job-instance path on the expected Fabric HTTPS
origin and matches the requested workspace/item, `jobInstanceId` is included. The Location is never
followed or echoed. Missing, invalid, multiple, mismatched, or collection-only Locations produce an
accepted receipt with `trackingIssue` and no invented ID. Some public API examples
return a collection Location: this does not justify discovery, resubmission, or treating
`x-ms-operation-id` as a job-instance ID.

A single valid `Retry-After` becomes `retryAfterSeconds` or `retryAfterUtc`. **Wait at least the supplied
delay (or until that UTC date) before the first separate status read.** No early-poll shortcut or
fallback delay is applied. Missing/invalid/multiple guidance is reported as `retryAfterIssue`, leaving
acceptance intact and the first-read delay unknown.

Run never polls, waits, retrieves results, resubmits, or uses the `/operations` LRO runner. A network
error, timeout, or cancellation can leave submission uncertain: **a job may already have started**.
Inspect the item's job history before deciding on another submission; do not retry blindly or
assume the latest listed instance is the job from that ambiguous submission.
Confirmed acceptance is retained if cancellation arrives after response headers. Canceling the MCP
request does not cancel a submitted Fabric job. Backend response/failure bodies and execution inputs
are not returned or logged by the command.

Run-to-Get handoff, as separate calls with an explicitly authorized item and supported job type:

```powershell
# Submit once. Choose the supported job type explicitly; this example does not discover one.
$submission = fabmcp jobs run-on-demand-item-job --workspace-id <workspace-id> --item-id <item-id> --job-type <supported-job-type> | ConvertFrom-Json
$receipt = $submission.results
$receipt
```

Only if `submissionStatus` is `Accepted` and `jobInstanceId` is present, wait at least
`retryAfterSeconds` or until `retryAfterUtc`. Then make a separate call using the receipt's IDs:

```powershell
fabmcp jobs get-item-job-instance --workspace-id $receipt.workspaceId --item-id $receipt.itemId --job-instance-id $receipt.jobInstanceId
```

If tracking or timing metadata is unavailable, inspect the receipt's guidance and Fabric job history
instead of inventing an ID, an early-read delay, or a retry. The Get response is a snapshot, not an
instruction to start another job. Both calls work with `--namespace jobs`; add `--namespace core`
only when also using Core item/definition tools. In namespace mode, learn the `jobs` schema and invoke
`command="jobs_run-on-demand-item-job"`; single mode additionally uses `tool="jobs"`.

Example prompts (require explicit execution authorization):

- "Submit one job of type `<job-type>` for this item using the workspace/item UUIDs I supplied. Return acceptance only; do not poll or retry."
- "Run this authorized job once with my execution-data JSON and job-parameters array. Preserve both fields; do not retry without parameters if unsupported."
- "After the receipt's minimum Retry-After wait, use its trusted job-instance ID in one separate Get call; do not submit another job."

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

`core_update-item` modifies an existing item's name or description; review its arguments and item permissions before use.

`core_move-item` changes placement within one workspace and also moves the item's child items. Review the exact item and destination before mutation consent.

`core_delete-workspace` can delete a workspace and its items. Review the exact UUID, permissions, and destructive-operation controls before invoking it.

`core_assign-workspace-to-capacity` is a mutating control-plane operation. Verify both target IDs and permissions before invoking it; acceptance does not confirm completion.

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
