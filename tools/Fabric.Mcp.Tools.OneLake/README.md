# Fabric.Mcp.Tools.OneLake

Microsoft Fabric OneLake MCP (Model Context Protocol) Tools - Manage and interact with OneLake data lake storage through AI agents and MCP clients.

## Overview

OneLake is Microsoft Fabric's built-in data lake that provides unified storage for all analytics workloads. This MCP tool provides operations for working with OneLake resources within your Fabric tenant, enabling AI agents to:

- Manage OneLake folders and files
- Browse items, tables and namespaces
- Configure OneLake data access security (role-based)
- Create, list and manage shortcuts (per-target typed tools + cache reset)
- Read and modify workspace-level OneLake settings (diagnostics, immutability)

**Features:**
- 40 comprehensive OneLake commands with full MCP integration
- Complete coverage for OneLake table APIs: configuration, namespace discovery, and table metadata
- Data access security management: list, get, create/update, and delete roles
- Shortcut management: list, get, create (8 per-target typed tools), delete, and cache reset
- Workspace-level settings: diagnostics and immutability policy configuration
- Friendly-name support for workspaces and items across all commands
- Robust error handling and authentication
- Production-ready with 100% test coverage (309 tests)
- Clean, focused API design optimized for AI agent interactions

> **Note:** Item creation has moved to the [Fabric.Mcp.Tools.Core](https://github.com/microsoft/mcp/tree/main/tools/Fabric.Mcp.Tools.Core) toolset as `core_create_item`. See [Core tools](https://github.com/microsoft/mcp/tree/main/tools/Fabric.Mcp.Tools.Core) for item creation operations.

## Prerequisites

- Microsoft Fabric workspace with OneLake enabled
- Azure authentication (Azure CLI or managed identity)
- Access to the target Fabric workspace and lakehouse

## Authentication

The tool uses Azure authentication. Ensure you're logged in using Azure CLI:

```bash
az login
```

## Environment Configuration

The OneLake MCP tools are configured to use the Microsoft Fabric production environment.

### Production Environment Endpoints

```
OneLake Data Plane: https://api.onelake.fabric.microsoft.com
OneLake DFS API:    https://onelake.dfs.fabric.microsoft.com
OneLake Blob API:   https://onelake.blob.fabric.microsoft.com
OneLake Table API:  https://onelake.table.fabric.microsoft.com
Fabric Core API:    https://api.fabric.microsoft.com/v1
```

> **Endpoint routing:** Security, shortcut and settings tools call the **Fabric Core API** (`api.fabric.microsoft.com`). All other tools target the OneLake data plane / DFS / Blob / Table endpoints.

### Getting Started

Simply use the commands without any environment configuration:

```bash
# List workspaces
dotnet run -- onelake workspace list

# Read files
dotnet run -- onelake file read --workspace-id "your-workspace-id" --item-id "your-item-id" --file-path "data.json"

# Write files
dotnet run -- onelake file write --workspace-id "your-workspace-id" --item-id "your-item-id" --file-path "test.txt" --content "Hello OneLake"
```

### MCP Client Configuration

Configure your MCP client to use OneLake tools:

```json
{
  "servers": {
    "fabric-onelake": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "path/to/Fabric.Mcp.Server", "--", "server", "start", "--namespace", "onelake"]
    }
  }
}
```

### Environment Verification

You can verify which environment you're targeting by checking the endpoints in the logs or by listing workspaces and comparing the results with what you expect in each environment.

**Important Notes:**
- Each environment may have different workspaces and items available
- Authentication requirements may vary between environments
- Daily and DXT environments are primarily for testing and development
- Production environment should be used for production workloads

### Workspace and Item Identifiers

The Fabric Core REST APIs used by the Security, Shortcuts, and Settings commands require GUID identifiers. Use `--workspace-id` for workspace scope and `--item-id` for item scope. Table-based commands additionally accept schema identifiers through `--namespace` or its alias `--schema`.

```bash
dotnet run -- onelake shortcut list --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342"
```

## Available Commands

**Note:** All commands support additional global options for authentication, retry policies, and tenant configuration. Use `--help` with any command to see the full list of options.

### Workspace Operations

#### List OneLake Workspaces

Lists all OneLake workspaces using the OneLake data plane API.

```bash
dotnet run -- onelake workspace list
```

**Example Output:**
```json
{
  "status": 200,
  "message": "Success",
  "results": {
    "workspaces": [
      {
        "id": "47242da5-ff3b-46fb-a94f-977909b773d5",
        "displayName": "My Workspace",
        "description": "Primary analytics workspace"
      }
    ]
  }
}
```

### Item Operations

#### List OneLake Items

Lists OneLake items in a workspace using the OneLake data plane API.

```bash
dotnet run -- onelake item list --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5"
```

**Parameters:**
- `--workspace-id`: The ID of the Microsoft Fabric workspace

**Example Output:**
```json
{
  "status": 200,
  "message": "Success",
  "results": {
    "items": [
      {
        "id": "0e67ed13-2bb6-49be-9c87-a1105a4ea342",
        "displayName": "MyLakehouse",
        "type": "Lakehouse",
        "workspaceId": "47242da5-ff3b-46fb-a94f-977909b773d5"
      }
    ]
  }
}
```

#### List OneLake Items (DFS API)

Lists OneLake items in a workspace using the OneLake DFS (Data Lake File System) API.

```bash
dotnet run -- onelake item list-data --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --recursive
```

**Parameters:**
- `--workspace-id`: The ID of the Microsoft Fabric workspace
- `--recursive`: (Optional) Whether to perform the operation recursively

#### Create Item

> **Moved:** The `item create` command has been moved to [Fabric.Mcp.Tools.Core](https://github.com/microsoft/mcp/tree/main/tools/Fabric.Mcp.Tools.Core) as `core_create_item`. Use the Core toolset for creating new items (Lakehouse, Notebook, etc.) in a Microsoft Fabric workspace.

### File Operations

#### Read File

Reads the contents of a file from OneLake storage.

```bash
dotnet run -- onelake file read --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "raw_data/data.json"
```

**Parameters:**
- `--workspace-id`: The ID of the Microsoft Fabric workspace
- `--item-id`: The ID of the Fabric item (e.g., Lakehouse)
- `--file-path`: Path to the file within the item storage

**Example Output:**
```json
{
  "status": 200,
  "message": "Success",
  "results": {
    "filePath": "raw_data/data.json",
    "content": "{ \"message\": \"Hello from OneLake!\" }"
  }
}
```

#### Write File

Writes content to a file in OneLake storage. Can write text content directly or upload from a local file.

**Write text content directly:**
```bash
dotnet run -- onelake file write --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "test/hello.txt" --content "Hello, OneLake!"
```

**Upload from local file:**
```bash
dotnet run -- onelake file write --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "data/upload.json" --local-file-path "C:\local\data.json" --overwrite
```

**Parameters:**
- `--workspace-id`: The ID of the Microsoft Fabric workspace
- `--item-id`: The ID of the Fabric item (e.g., Lakehouse)
- `--file-path`: Path where the file will be stored in OneLake
- `--content`: (Optional) Text content to write directly
- `--local-file-path`: (Optional) Path to local file to upload
- `--overwrite`: (Optional) Overwrite existing file if it exists

**Example Output:**
```json
{
  "status": 200,
  "message": "Success",
  "results": {
    "filePath": "test/hello.txt",
    "contentLength": 15,
    "message": "File written successfully"
  }
}
```

#### Delete File

Deletes a file from OneLake storage.

```bash
dotnet run -- onelake file delete --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "temp/unwanted.txt"
```

**Parameters:**
- `--workspace-id`: The ID of the Microsoft Fabric workspace
- `--item-id`: The ID of the Fabric item (e.g., Lakehouse)
- `--file-path`: Path to the file to delete

**Example Output:**
```json
{
  "status": 200,
  "message": "Success",
  "results": {
    "filePath": "temp/unwanted.txt",
    "message": "File deleted successfully"
  }
}
```

#### Upload File (Blob Endpoint)

Uploads binary content to OneLake using the native blob endpoint. Supports inline content, local files, and content-type overrides while returning rich service metadata.

```bash
dotnet run -- onelake upload file --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "Files/data/archive.bin" --local-file-path "C:\data\archive.bin" --overwrite --content-type application/octet-stream
```

**Parameters:**
- `--workspace-id`: The ID or friendly name of the Microsoft Fabric workspace
- `--item-id`: The ID or friendly name of the Fabric item (e.g., Lakehouse)
- `--file-path`: Blob path under the `Files/` container
- `--content`: (Optional) Inline content to upload
- `--local-file-path`: (Optional) Local file to stream to OneLake
- `--overwrite`: (Optional) Overwrite the blob if it already exists
- `--content-type`: (Optional) Explicit MIME type for the blob

**Example Output:**
```json
{
  "status": 201,
  "message": "Success",
  "results": {
    "workspaceId": "47242da5-ff3b-46fb-a94f-977909b773d5",
    "itemId": "0e67ed13-2bb6-49be-9c87-a1105a4ea342",
    "blobPath": "Files/data/archive.bin",
    "contentLength": 1048576,
    "contentType": "application/octet-stream",
    "etag": "\"0x8DB63C58E54196C\"",
    "lastModified": "2025-11-25T18:14:03.5123456+00:00",
    "requestId": "72f62f01-6d92-4c66-8a7a-d5dd24ff1c9d",
    "version": "2023-11-03",
    "requestServerEncrypted": true,
    "contentCrc64": "i8K5MTc=",
    "encryptionScope": "onelake-default",
    "clientRequestId": "c0a7efbb-fbd7-484e-95c1-e606f9387e0d",
    "rootActivityId": "1c95a779-7d14-4596-9b54-81197cda059b",
    "message": "File uploaded successfully."
  }
}
```

#### Download File (Blob Endpoint)

Downloads a file via the OneLake blob endpoint, returning metadata, a base64 representation, and decoded text content when applicable.

```bash
dotnet run -- onelake download file --workspace "Analytics Workspace" --item "SalesLakehouse.lakehouse" --file-path "Files/data/archive.bin"
```

**Parameters:**
- `--workspace`/`--workspace-id`: Workspace friendly name or ID
- `--item`/`--item-id`: Item friendly name or ID
- `--file-path`: Path to the file under `Files/`

**Example Output:**
```json
{
  "status": 200,
  "message": "Success",
  "results": {
    "blob": {
      "workspaceId": "47242da5-ff3b-46fb-a94f-977909b773d5",
      "itemId": "0e67ed13-2bb6-49be-9c87-a1105a4ea342",
      "path": "Files/data/archive.bin",
      "contentLength": 1048576,
      "contentType": "application/json",
      "charset": "utf-8",
      "contentBase64": "eyJtZXNzYWdlIjogIkhlbGxvIE9uZUxha2UhIn0=",
      "contentText": "{\"message\": \"Hello OneLake!\"}",
      "etag": "\"0x8DB63C58E54196C\"",
      "lastModified": "2025-11-25T18:14:03.5123456+00:00",
      "requestServerEncrypted": true,
      "clientRequestId": "c0a7efbb-fbd7-484e-95c1-e606f9387e0d",
      "rootActivityId": "1c95a779-7d14-4596-9b54-81197cda059b"
    },
    "message": "File retrieved successfully."
  }
}
```

#### Delete Blob (Blob Endpoint)

Removes a blob using the OneLake blob endpoint and returns the request identifiers emitted by the platform.

```bash
dotnet run -- onelake blob delete --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "Files/data/archive.bin"
```

**Parameters:**
- `--workspace-id` / `--workspace`: Workspace identifier or friendly name
- `--item-id` / `--item`: Item identifier or friendly name
- `--file-path`: Path to the blob to remove

**Example Output:**
```json
{
  "status": 200,
  "message": "Success",
  "results": {
    "result": {
      "workspaceId": "47242da5-ff3b-46fb-a94f-977909b773d5",
      "itemId": "0e67ed13-2bb6-49be-9c87-a1105a4ea342",
      "path": "Files/data/archive.bin",
      "version": "2023-11-03",
      "requestId": "b2aa28ff-96ff-4afc-8d0f-6a2151ad5e3e",
      "clientRequestId": "8a347b58-d80a-46f3-b4f2-0efd8e6a60a1",
      "rootActivityId": "0c828f9e-348c-4d7a-9c61-34dfe0f4e279"
    },
    "message": "Blob deleted successfully."
  }
}
```

#### List Files as Blobs

Lists files and directories in OneLake storage as blobs. Browse the contents of a lakehouse or specific directory path with optional recursive listing in blob format.

```bash
dotnet run -- onelake blob list --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342"
```

**With path and recursive options:**
```bash
dotnet run -- onelake blob list --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --path "raw_data" --recursive
```

**Parameters:**
- `--workspace-id`: The ID of the Microsoft Fabric workspace (GUID)
- `--item-id`: The ID of the Fabric item (GUID)
- `--path`: (Optional) The path to list in OneLake storage (defaults to root)
- `--recursive`: (Optional) Whether to perform the operation recursively

**Example Output:**
```json
{
  "status": 200,
  "message": "Success",
  "results": {
    "files": [
      {
        "name": "data.json",
        "path": "0e67ed13-2bb6-49be-9c87-a1105a4ea342/Files/raw_data/data.json",
        "isDirectory": false,
        "size": 76443,
        "lastModified": "2025-10-28T19:23:21+00:00",
        "contentType": "application/octet-stream",
        "etag": null
      },
      {
        "name": "reports",
        "path": "0e67ed13-2bb6-49be-9c87-a1105a4ea342/Files/raw_data/reports",
        "isDirectory": true,
        "size": 0,
        "lastModified": "2025-10-28T18:10:09+00:00",
        "contentType": "application/x-directory",
        "etag": null
      }
    ],
    "basePath": ""
  }
}
```

**Use Case:** Best for discovering all files in a flat structure, similar to Azure Blob Storage. Provides a comprehensive list of all files and directories with basic metadata.

#### List File Structure (DFS API)

Lists files and directories in OneLake storage using a filesystem-style hierarchical view, similar to Azure Data Lake Storage Gen2. Shows directory structure with paths, sizes, timestamps, and metadata.

```bash
dotnet run -- onelake file list --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342"
```

**With recursive exploration:**
```bash
dotnet run -- onelake file list --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --recursive
```

**With specific path:**
```bash
dotnet run -- onelake file list --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --path "analytics/reports"
```

**Parameters:**
- `--workspace-id`: The ID of the Microsoft Fabric workspace (GUID)
- `--item-id`: The ID of the Fabric item (GUID)
- `--path`: (Optional) The path to list in OneLake storage (defaults to root)
- `--recursive`: (Optional) Whether to perform the operation recursively

**Example Output:**
```json
{
  "status": 200,
  "message": "Success",
  "results": {
    "items": [
      {
        "name": "analytics",
        "path": "0e67ed13-2bb6-49be-9c87-a1105a4ea342/Files/analytics",
        "type": "directory",
        "size": null,
        "lastModified": "2025-10-29T18:51:17+00:00",
        "contentType": "application/x-directory",
        "etag": "0x8DE171C24C7962C",
        "permissions": "rwxr-x---",
        "owner": "7cc5a2eb-b514-4973-ab35-81f11a1d043f",
        "group": "7cc5a2eb-b514-4973-ab35-81f11a1d043f",
        "isDirectory": true,
        "children": null
      },
      {
        "name": "report.json",
        "path": "0e67ed13-2bb6-49be-9c87-a1105a4ea342/Files/report.json",
        "type": "file",
        "size": 80,
        "lastModified": "2025-10-29T18:50:49+00:00",
        "contentType": "application/octet-stream",
        "etag": "0x8DE171C13EFD42D",
        "permissions": "rw-r-----",
        "owner": "7cc5a2eb-b514-4973-ab35-81f11a1d043f",
        "group": "7cc5a2eb-b514-4973-ab35-81f11a1d043f",
        "isDirectory": false,
        "children": null
      }
    ]
  }
}
```

**Use Case:** Best for exploring OneLake content in a filesystem format with rich metadata including POSIX-style permissions, ownership, and ETags. Ideal for traditional file system navigation patterns.

#### API Comparison: Blob vs Path Listing

| Feature | `blob list` | `file list` |
|---------|-------------|-------------|
| **API Endpoint** | OneLake Blob Storage | OneLake DFS (Data Lake File System) |
| **Output Style** | Flat blob listing | Hierarchical filesystem view |
| **Metadata Depth** | Basic (size, modified, contentType) | Rich (permissions, owner, group, etag) |
| **Best For** | File discovery, bulk operations | Navigation, permissions management |
| **Recursive Default** | Shows all files when recursive | Shows directory structure when recursive |
| **Path Format** | Full blob paths | Filesystem-style paths |

### Directory Operations

#### Create Directory

Creates a directory in OneLake storage. Can create nested directory structures.

```bash
dotnet run -- onelake directory create --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --directory-path "analytics/reports/2024"
```

**Parameters:**
- `--workspace-id`: The ID of the Microsoft Fabric workspace
- `--item-id`: The ID of the Fabric item (e.g., Lakehouse)
- `--directory-path`: Path of the directory to create

**Example Output:**
```json
{
  "status": 200,
  "message": "Success",
  "results": {
    "workspaceId": "47242da5-ff3b-46fb-a94f-977909b773d5",
    "itemId": "0e67ed13-2bb6-49be-9c87-a1105a4ea342",
    "directoryPath": "analytics/reports/2024",
    "success": true,
    "message": "Directory 'analytics/reports/2024' created successfully"
  }
}
```

#### Delete Directory

Deletes a directory from OneLake storage. Use `--recursive` to delete non-empty directories.

**Delete empty directory:**
```bash
dotnet run -- onelake directory delete --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --directory-path "temp"
```

**Delete directory and all contents (recursive):**
```bash
dotnet run -- onelake directory delete --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --directory-path "old_data" --recursive
```

**Parameters:**
- `--workspace-id`: The ID of the Microsoft Fabric workspace
- `--item-id`: The ID of the Fabric item (e.g., Lakehouse)
- `--directory-path`: Path of the directory to delete
- `--recursive`: (Optional) Delete directory and all its contents

**Example Output:**
```json
{
  "status": 200,
  "message": "Success",
  "results": {
    "directoryPath": "old_data",
    "message": "Directory and all contents deleted successfully"
  }
}
```

### Table Operations

#### Get Table Configuration

Retrieves the OneLake table API configuration payload for a warehouse or lakehouse endpoint.

```bash
dotnet run -- onelake table config get --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342"
```

**Parameters:**
- `--workspace-id`/`--workspace`: Workspace identifier (GUID or name)
- `--item-id`/`--item`: Item identifier (GUID or `<name>.<type>`)

#### List Table Namespaces

Enumerates the namespaces (schemas) exposed by the table API. Accepts either GUIDs or friendly names.

```bash
dotnet run -- onelake table namespace list --workspace "Analytics Workspace" --item "SalesLakehouse.lakehouse"
```

#### Get Table Namespace Details

Fetches metadata for a specific namespace. Use `--namespace` (or `--schema`) to target the schema.

```bash
dotnet run -- onelake table namespace get --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --namespace "sales"
```

#### List Tables within a Namespace

Returns the tables published under a namespace. The `--namespace` and `--schema` switches are interchangeable.

```bash
dotnet run -- onelake table list --workspace "Analytics Workspace" --item "SalesLakehouse.lakehouse" --schema "sales"
```

#### Get Table Metadata

Retrieves the full table definition for a specific namespace/table combination.

```bash
dotnet run -- onelake table get --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --namespace "sales" --table "transactions"
```

**Parameters:**
- `--workspace-id`/`--workspace`: Workspace identifier
- `--item-id`/`--item`: Item identifier
- `--namespace`/`--schema`: Namespace (schema) name
- `--table`: Table name to retrieve

### Security — Data Access Roles

These tools manage role-based data access policies on OneLake items (Lakehouse / Warehouse). All security tools call the **Fabric Core API** and require the caller to be a workspace Admin or Member.

#### List Data Access Roles

Lists all data access roles defined on a single item. Supports pagination via `--continuation-token`.

```bash
dotnet run -- onelake security list --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342"
```

**Parameters:**
- `--workspace-id`: Workspace ID (GUID)
- `--item-id`: Item ID (GUID)
- `--continuation-token`: (Optional) Token for retrieving the next page of results

#### Get Data Access Role

Gets the full definition of a single data access role — members, permissions, decision rules.

```bash
dotnet run -- onelake security get --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --role-name "DataAnalysts"
```

**Parameters:**
- `--workspace-id`: Workspace ID (GUID)
- `--item-id`: Item ID (GUID)
- `--role-name`: Name of the data access role to retrieve

#### Create or Update Data Access Role

Upserts a single data access role on a single item. Scoped to one role per call — does not affect other roles. The role name is derived from the `name` field in the JSON definition.

```bash
dotnet run -- onelake security create-or-update --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --role-definition '{"name":"DataAnalysts","members":{"fabricItemMembers":[{"itemAccessType":"ReadAll"}]},"decisionRules":[{"effect":"Permit","permission":[{"attributeName":"Path","attributeValueIncludedIn":["Tables/*"]}]}]}'
```

**Parameters:**
- `--workspace-id`: Workspace ID (GUID)
- `--item-id`: Item ID (GUID)
- `--role-definition`: JSON definition of the role (must include `name`, `members`, and `decisionRules`)

#### Delete Data Access Role

Deletes a single data access role from a single item. Destructive — principals that gained access only via this role lose it.

```bash
dotnet run -- onelake security delete --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --role-name "TempRole"
```

**Parameters:**
- `--workspace-id`: Workspace ID (GUID)
- `--item-id`: Item ID (GUID)
- `--role-name`: Name of the data access role to delete

### Shortcut Operations

Shortcuts are references to data stored in external or internal locations (ADLS Gen2, S3, GCS, OneLake, Dataverse, etc.). These tools call the **Fabric Core API**.

#### List Shortcuts

Lists shortcuts defined within an item, recursing through subfolders. Supports pagination via `--continuation-token`. By default, DW-managed shortcuts (which can number in the hundreds of thousands per Warehouse) are hidden — use `--include-managed` to show them.

```bash
dotnet run -- onelake shortcut list --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342"
```

**Parameters:**
- `--workspace-id`: Workspace ID (GUID)
- `--item-id`: Item ID (GUID)
- `--parent-path`: (Optional) Parent path to scope the listing
- `--continuation-token`: (Optional) Token for retrieving the next page of results
- `--include-managed`: (Optional) Include DW-managed shortcuts in results (default: false)

#### Get Shortcut

Gets the properties of a single shortcut (name, path, target, configuration).

```bash
dotnet run -- onelake shortcut get --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --shortcut-name "ExternalData" --shortcut-path "Tables/ExternalData"
```

**Parameters:**
- `--workspace-id`: Workspace ID (GUID)
- `--item-id`: Item ID (GUID)
- `--shortcut-name`: Name of the shortcut
- `--shortcut-path`: Path of the shortcut within the item

#### Create Shortcut (Per-Target — Recommended for AI agents)

Eight per-target tools provide flat, typed options instead of requiring a JSON blob. Use the tool matching your target type:

| Tool | Target Type |
|------|-------------|
| `create_shortcut_onelake` | Another OneLake location |
| `create_shortcut_adls_gen2` | Azure Data Lake Storage Gen2 |
| `create_shortcut_amazon_s3` | Amazon S3 |
| `create_shortcut_azure_blob` | Azure Blob Storage |
| `create_shortcut_gcs` | Google Cloud Storage |
| `create_shortcut_s3_compatible` | S3-compatible storage |
| `create_shortcut_dataverse` | Dataverse environment |
| `create_shortcut_onedrive_sharepoint` | OneDrive / SharePoint Online |

**Common parameters (all per-target tools):**
- `--workspace-id`: Workspace ID (GUID)
- `--item-id`: Item ID (GUID)
- `--shortcut-path`: Path within the item where the shortcut lives
- `--shortcut-name`: Display name for the shortcut
- `--shortcut-conflict-policy`: (Optional) Abort, CreateOrOverwrite, OverwriteOnly, GenerateUniqueName

**OneLake target additional parameters:**
- `--target-workspace-id`: Target workspace ID
- `--target-item-id`: Target item ID
- `--target-path`: Path within the target item (required)
- `--target-connection-id`: (Optional) Connection ID

**ADLS Gen2 / Amazon S3 / Azure Blob / GCS target parameters:**
- `--target-location`: Storage URL
- `--target-subpath`: Subpath within the location (required for ADLS Gen2)
- `--target-connection-id`: Connection ID for authentication

**S3-compatible target additional parameters:**
- `--target-bucket`: Bucket name (in addition to location, subpath, connection-id)

**Dataverse target parameters:**
- `--target-environment-domain`: Dataverse environment URI
- `--target-connection-id`: Connection ID
- `--target-deltalake-folder`: Delta Lake folder path
- `--target-table-name`: (Optional) Table name

**OneDrive/SharePoint target parameters:**
- `--target-location`: Storage URL
- `--target-subpath`: (Optional) Subpath
- `--target-connection-id`: Connection ID
- `--target-update-fabric-item-sensitivity`: (Optional) Update sensitivity label from source

**Example — Create an ADLS Gen2 shortcut:**
```bash
dotnet run -- onelake shortcut create-adls-gen2 \
  --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" \
  --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" \
  --shortcut-path "Tables" --shortcut-name "ExternalData" \
  --target-location "https://myaccount.dfs.core.windows.net/container" \
  --target-subpath "/data/tables" \
  --target-connection-id "a1b2c3d4-5678-9012-3456-789012345678"
```

#### Delete Shortcut

Deletes a single shortcut from an item. The destination data is preserved — only the shortcut reference is removed.

```bash
dotnet run -- onelake shortcut delete --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --shortcut-name "ExternalData" --shortcut-path "Tables/ExternalData"
```

**Parameters:**
- `--workspace-id`: Workspace ID (GUID)
- `--item-id`: Item ID (GUID)
- `--shortcut-name`: Name of the shortcut to delete
- `--shortcut-path`: Path of the shortcut

#### Reset Shortcut Cache

Drops cached shortcut reads for a workspace, forcing the next read to re-resolve from the destination. Use sparingly — primarily for debugging stale-cache issues.

```bash
dotnet run -- onelake shortcut reset-cache --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5"
```

**Parameters:**
- `--workspace-id`: Workspace ID (GUID)

### Settings Operations

Workspace-level OneLake settings for diagnostics and immutability policies. These tools call the **Fabric Core API**.

#### Get Settings

Gets the OneLake settings for a workspace — diagnostics configuration and immutability policy.

```bash
dotnet run -- onelake settings get --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5"
```

**Parameters:**
- `--workspace-id`: Workspace ID (GUID)

#### Modify Diagnostics

Modifies the diagnostic logging configuration for OneLake at the workspace scope using flat options.

```bash
dotnet run -- onelake settings modify-diagnostics --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --status "Enabled" --destination-lakehouse-workspace-id "ws-guid" --destination-lakehouse-item-id "item-guid"
```

**Parameters:**
- `--workspace-id`: Workspace ID (GUID)
- `--status`: Diagnostic status (Enabled or Disabled)
- `--destination-lakehouse-workspace-id`: (Optional) Workspace ID of the destination lakehouse
- `--destination-lakehouse-item-id`: (Optional) Item ID of the destination lakehouse

#### Modify Immutability Policy

Modifies the workspace-level OneLake immutability policy. **Warning:** Once enabled, immutability cannot be disabled — confirm with the user before applying.

```bash
dotnet run -- onelake settings modify-immutability --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --scope "Workspace" --retention-days 30
```

**Parameters:**
- `--workspace-id`: Workspace ID (GUID)
- `--scope`: Immutability scope (e.g. Workspace)
- `--retention-days`: (Optional) Retention period in days

## Quick Reference - fabmcp.exe Commands

For users with the compiled `fabmcp.exe` executable, here are ready-to-use commands:

### Authentication
```cmd
# Authenticate with Microsoft tenant (required first step)
az login --tenant "72f988bf-86f1-41af-91ab-2d7cd011db47" --scope "https://management.core.windows.net//.default"
```

### Workspace Operations
```cmd
# List all OneLake workspaces
fabmcp.exe onelake workspace list

# List items in a specific workspace
fabmcp.exe onelake item list --workspace "47242da5-ff3b-46fb-a94f-977909b773d5"
```

### Path & Directory Operations
```cmd
# List files and directories with filesystem view
fabmcp.exe onelake file list --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --path "Files" --recursive

# Create a new directory
fabmcp.exe onelake directory create --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --directory-path "mcpdir"

# Delete a directory (with recursive option for non-empty directories)
fabmcp.exe onelake directory delete --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --directory-path "mcpdir" --recursive
```

### File Operations
```cmd
# Write content to a file
fabmcp.exe onelake file write --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "mcpdir/hello.txt" --content "Hello, OneLake!"

# Read file contents
fabmcp.exe onelake file read --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "mcpdir/hello.txt"

# Delete a file
fabmcp.exe onelake file delete --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "mcpdir/hello.txt"

# Upload a file (Blob endpoint)
fabmcp.exe onelake upload file --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "Files/data/archive.bin" --local-file-path "C:\data\archive.bin" --overwrite

# Download a file with metadata (Blob endpoint)
fabmcp.exe onelake download file --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "Files/data/archive.bin"

# Delete a blob (Blob endpoint)
fabmcp.exe onelake blob delete --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "Files/data/archive.bin"
```

### Table Operations
```cmd
# Inspect table API configuration
fabmcp.exe onelake table config get --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342"

# List available namespaces (schemas)
fabmcp.exe onelake table namespace list --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342"

# Get namespace details
fabmcp.exe onelake table namespace get --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --namespace "sales"

# List tables published under a namespace
fabmcp.exe onelake table list --workspace "Analytics Workspace" --item "SalesLakehouse.lakehouse" --schema "sales"

# Retrieve a specific table definition
fabmcp.exe onelake table get --workspace "Analytics Workspace" --item "SalesLakehouse.lakehouse" --schema "sales" --table "transactions"
```

### Security Operations
```cmd
# List data access roles on an item
fabmcp.exe onelake security list --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342"

# Get a specific role definition
fabmcp.exe onelake security get --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --role-name "DataAnalysts"

# Delete a data access role
fabmcp.exe onelake security delete --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --role-name "TempRole"
```

### Shortcut Operations
```cmd
# List shortcuts on an item
fabmcp.exe onelake shortcut list --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342"

# Get a specific shortcut
fabmcp.exe onelake shortcut get --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --shortcut-name "ExternalData" --shortcut-path "Tables/ExternalData"

# Delete a shortcut
fabmcp.exe onelake shortcut delete --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --shortcut-name "ExternalData" --shortcut-path "Tables/ExternalData"

# Reset shortcut cache
fabmcp.exe onelake shortcut reset-cache --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5"
```

### Settings Operations
```cmd
# Get workspace OneLake settings
fabmcp.exe onelake settings get --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5"

# Modify diagnostics configuration
fabmcp.exe onelake settings modify-diagnostics --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --diagnostics-config '{"logAnalyticsWorkspaceId":"<id>","level":"Verbose"}'

# Modify immutability policy
fabmcp.exe onelake settings modify-immutability --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --immutability-policy '{"state":"Enabled"}'
```

**Note:** Replace the workspace ID (`47242da5-ff3b-46fb-a94f-977909b773d5`) and item ID (`0e67ed13-2bb6-49be-9c87-a1105a4ea342`) with your actual Fabric GUIDs.

## Common Usage Patterns

### Bulk File Upload

Use the provided PowerShell scripts for bulk operations:

```powershell
# Upload all files from a local directory (configure variables in script)
.\upload_files.ps1

# Simple upload script for quick operations
.\upload_files_simple.ps1
```

**Note:** Edit the PowerShell scripts to configure your source folder, workspace ID, item ID, and target path before running.

### Data Pipeline Integration

```bash
# 1. Create a structured directory for your data pipeline
dotnet run -- onelake directory create --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --directory-path "pipelines/etl/raw"
dotnet run -- onelake directory create --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --directory-path "pipelines/etl/processed"

# 2. Upload raw data
dotnet run -- onelake file write --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --file-path "pipelines/etl/raw/source_data.csv" --local-file-path "C:\data\source.csv"

# 3. Process and write results
dotnet run -- onelake file write --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --file-path "pipelines/etl/processed/clean_data.parquet" --local-file-path "C:\processed\clean.parquet"

# 4. Clean up temporary files
dotnet run -- onelake directory delete --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --directory-path "pipelines/etl/temp" --recursive
```

### Analytics Workflow

```bash
# 1. List available workspaces
dotnet run -- onelake workspace list

# 2. List items in a workspace
dotnet run -- onelake item list --workspace-id "WORKSPACE_ID"

# 3. Explore data structure using hierarchical view
dotnet run -- onelake file list --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --path "data/reports"

# 4. Get comprehensive file inventory with blob listing
dotnet run -- onelake blob list --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --path "data" --recursive

# 5. Read analysis results
dotnet run -- onelake file read --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --file-path "reports/monthly_summary.json"

# 6. Archive old reports
dotnet run -- onelake directory create --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --directory-path "archive/2024"
# Move files to archive (requires multiple file operations)
dotnet run -- onelake directory delete --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --directory-path "reports/old" --recursive
```

### Data Discovery and Exploration

```bash
# 1. Quick overview of lakehouse structure using DFS API (shows directories and permissions)
dotnet run -- onelake file list --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --path "Tables"

# 2. Comprehensive file search using Blob API (finds all files including nested)
dotnet run -- onelake blob list --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --path "Files" --recursive

# 3. Compare structure views - hierarchical vs flat
dotnet run -- onelake file list --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --path "data/processed"
dotnet run -- onelake blob list --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --path "data/processed"

# 4. Find specific file types across the entire lakehouse
dotnet run -- onelake blob list --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --recursive | grep "\.parquet"
dotnet run -- onelake blob list --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --recursive | grep "\.delta"
```

## Error Handling

The tool provides detailed error messages and proper HTTP status codes:

- **401**: Authentication failed - run `az login`
- **403**: Access denied - check workspace permissions
- **404**: Resource not found - verify workspace/item IDs and file paths
- **409**: Conflict - file already exists (use `--overwrite` for `file write`)

## Environment Variables

You can set these environment variables for convenience:

```bash
# Frequently used IDs to avoid repetitive typing
export FABRIC_WORKSPACE_ID="47242da5-ff3b-46fb-a94f-977909b773d5"
export FABRIC_ITEM_ID="0e67ed13-2bb6-49be-9c87-a1105a4ea342"
```

Then use them in commands:
```bash
dotnet run -- onelake file read --workspace-id "$FABRIC_WORKSPACE_ID" --item-id "$FABRIC_ITEM_ID" --file-path "data.json"
```

## Integration with MCP Clients

This tool is designed to work with MCP clients like Claude Desktop, VS Code with MCP extensions, or custom applications. Add to your MCP configuration:

```json
{
  "servers": {
    "fabric-onelake": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "path/to/Fabric.Mcp.Server", "--", "server", "start", "--namespace", "onelake"]
    }
  }
}
```

## Support and Documentation

- [Microsoft Fabric Documentation](https://docs.microsoft.com/fabric/)
- [OneLake Documentation](https://docs.microsoft.com/fabric/onelake/)
- [Azure CLI Authentication](https://docs.microsoft.com/cli/azure/authenticate-azure-cli)

## Contributing

This tool is part of the Microsoft MCP (Model Context Protocol) project. Please follow the established patterns for command implementation and ensure proper error handling and logging.

### Development and Testing

The OneLake MCP Tools include a comprehensive test suite with 100% command coverage:

#### Test Structure
- **Total Tests**: 309 tests (all passing)
- **Command Tests**: Covering all OneLake MCP commands including security, shortcuts, and settings
- **Service Architecture Tests**: Demonstrating testable patterns with dependency injection

#### Running Tests
```bash
# Run all tests
dotnet test tools/Fabric.Mcp.Tools.OneLake/tests/

# Run with verbose output
dotnet test tools/Fabric.Mcp.Tools.OneLake/tests/ --verbosity normal
```

#### Test Coverage
All commands have comprehensive test coverage including:
- Constructor validation and dependency injection
- Command properties and metadata verification
- ExecuteAsync testing with service mocking
- Error handling and exception scenarios
- Option binding and parameter validation

#### Architecture Patterns
The test suite includes examples of testable service architecture patterns following dependency injection principles. See `OneLakeServiceTests.cs` for demonstrations of:
- Parameter validation before API calls
- Mockable dependencies for unit testing
- Comprehensive error handling scenarios

### Development Guidelines
- Maintain 100% test coverage for new commands
- Follow the established command patterns (see existing commands as examples)
- Use proper error handling with meaningful HTTP status codes
- Include comprehensive parameter validation
- Write tests that verify both success and error scenarios
