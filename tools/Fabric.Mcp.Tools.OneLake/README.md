# Fabric.Mcp.Tools.OneLake

Microsoft Fabric OneLake MCP (Model Context Protocol) Tools - Manage and interact with OneLake data lake storage through AI agents and MCP clients.

## Overview

OneLake is Microsoft Fabric's built-in data lake that provides unified storage for all analytics workloads. This MCP tool provides operations for working with OneLake resources within your Fabric tenant, enabling AI agents to:

- Manage OneLake folders and files
- Configure data access and permissions
- Monitor OneLake storage usage and performance
- Integrate with other Fabric workloads through OneLake

**Features:**
- 11 comprehensive OneLake commands with full MCP integration
- Friendly-name support for workspaces and items across data-plane commands ( `item-create` currently requires GUID IDs )
- Support for multiple Microsoft Fabric environments (PROD, DAILY, DXT, MSIT)
- Robust error handling and authentication
- Production-ready with 100% test coverage (76 tests)
- Clean, focused API design optimized for AI agent interactions

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

The OneLake MCP tools support multiple Microsoft Fabric environments. You can configure which environment to target using the `ONELAKE_ENVIRONMENT` environment variable.

### Supported Environments

| Environment | Description | Use Case |
|-------------|-------------|----------|
| **PROD** | Production Microsoft Fabric | Default environment for production workloads |
| **DAILY** | Daily build environment | Testing with latest Fabric features |
| **DXT** | Developer experience testing | Internal Microsoft testing |
| **MSIT** | Microsoft IT environment | Microsoft internal operations |

### Environment Endpoints

Each environment has specific endpoints for different OneLake services:

#### Production (PROD) - Default
```
OneLake Data Plane: https://api.onelake.fabric.microsoft.com
OneLake DFS API: https://onelake.dfs.fabric.microsoft.com
OneLake Blob API: https://onelake.blob.fabric.microsoft.com
Fabric API: https://api.fabric.microsoft.com/v1
```

#### Daily Build (DAILY)
```
OneLake Data Plane: https://daily-api.onelake.fabric.microsoft.com
OneLake DFS API: https://daily-onelake.dfs.fabric.microsoft.com
OneLake Blob API: https://daily-onelake.blob.fabric.microsoft.com
Fabric API: https://dailyapi.fabric.microsoft.com/v1
```

#### DXT Environment
```
OneLake Data Plane: https://dxt-api.onelake.fabric.microsoft.com
OneLake DFS API: https://dxt-onelake.dfs.fabric.microsoft.com
OneLake Blob API: https://dxt-onelake.blob.fabric.microsoft.com
Fabric API: https://dxt-api.fabric.microsoft.com/v1
```

#### MSIT Environment
```
OneLake Data Plane: https://msit-api.onelake.fabric.microsoft.com
OneLake DFS API: https://msit-onelake.dfs.fabric.microsoft.com
OneLake Blob API: https://msit-onelake.blob.fabric.microsoft.com
Fabric API: https://msit-api.fabric.microsoft.com/v1
```

### Setting the Environment

#### Windows (PowerShell)
```powershell
# Set environment for current session
$env:ONELAKE_ENVIRONMENT = "DAILY"

# Set environment permanently
[Environment]::SetEnvironmentVariable("ONELAKE_ENVIRONMENT", "DAILY", "User")

# Run commands with specific environment
$env:ONELAKE_ENVIRONMENT = "DAILY"; dotnet run -- onelake onelake-workspace-list
```

#### Windows (Command Prompt)
```cmd
# Set environment for current session
set ONELAKE_ENVIRONMENT=DAILY

# Set environment permanently
setx ONELAKE_ENVIRONMENT DAILY

# Run commands with specific environment
set ONELAKE_ENVIRONMENT=DAILY && dotnet run -- onelake onelake-workspace-list
```

#### Linux/macOS (Bash)
```bash
# Set environment for current session
export ONELAKE_ENVIRONMENT=DAILY

# Set environment permanently (add to ~/.bashrc or ~/.zshrc)
echo 'export ONELAKE_ENVIRONMENT=DAILY' >> ~/.bashrc

# Run commands with specific environment
ONELAKE_ENVIRONMENT=DAILY dotnet run -- onelake onelake-workspace-list
```

### Environment-Specific Usage Examples

#### Working with Production Environment (Default)
```bash
# No environment variable needed - PROD is default
dotnet run -- onelake onelake-workspace-list
dotnet run -- onelake file-read --workspace-id "your-workspace-id" --item-id "your-item-id" --file-path "data.json"
```

#### Working with Daily Build Environment
```bash
# Set environment variable
export ONELAKE_ENVIRONMENT=DAILY

# Now all commands will use DAILY endpoints
dotnet run -- onelake onelake-workspace-list
dotnet run -- onelake file-write --workspace-id "your-workspace-id" --item-id "your-item-id" --file-path "test.txt" --content "Testing daily build"
```

#### Testing with Multiple Environments
```bash
# Test against production
unset ONELAKE_ENVIRONMENT
dotnet run -- onelake onelake-workspace-list

# Test against daily build
export ONELAKE_ENVIRONMENT=DAILY
dotnet run -- onelake onelake-workspace-list

# Test against DXT
export ONELAKE_ENVIRONMENT=DXT
dotnet run -- onelake onelake-workspace-list
```

### PowerShell Script Environment Configuration

Update the PowerShell upload scripts to work with different environments:

```powershell
# At the top of upload_files.ps1
$env:ONELAKE_ENVIRONMENT = "DAILY"  # or "PROD", "DXT", "MSIT"

# Rest of the script remains the same
$workspaceId = "your-workspace-id"
$itemId = "your-item-id"
# ... script continues
```

### MCP Client Configuration with Environments

Configure your MCP client to use specific environments:

```json
{
  "servers": {
    "fabric-onelake-prod": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "path/to/Fabric.Mcp.Server", "--", "server", "start", "--namespace", "onelake"],
      "env": {
        "ONELAKE_ENVIRONMENT": "PROD"
      }
    },
    "fabric-onelake-daily": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "path/to/Fabric.Mcp.Server", "--", "server", "start", "--namespace", "onelake"],
      "env": {
        "ONELAKE_ENVIRONMENT": "DAILY"
      }
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

All commands except `item-create` accept either GUID identifiers or friendly names via the `--workspace` and `--item` options. The existing `--workspace-id` and `--item-id` switches remain available for scripts that already depend on them. Friendly-name inputs are sent directly to the OneLake APIs without local GUID resolution; when using names, specify the item as `<itemName>.<itemType>` (for example, `SalesLakehouse.lakehouse`). `item-create` currently requires the GUID-based `--workspace-id` option.

```bash
dotnet run -- onelake path-list --workspace "Analytics Workspace" --item "SalesLakehouse.lakehouse" --path "Files"
```

## Available Commands

**Note:** All commands support additional global options for authentication, retry policies, and tenant configuration. Use `--help` with any command to see the full list of options.

### Workspace Operations

#### List OneLake Workspaces

Lists all OneLake workspaces using the OneLake data plane API.

```bash
dotnet run -- onelake onelake-workspace-list
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
dotnet run -- onelake onelake-item-list --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5"
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
dotnet run -- onelake onelake-item-list-dfs --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --recursive
```

**Parameters:**
- `--workspace-id`: The ID of the Microsoft Fabric workspace
- `--recursive`: (Optional) Whether to perform the operation recursively

#### Create Item

Creates a new item (Lakehouse, Notebook, etc.) in a Microsoft Fabric workspace using the Fabric API.

```bash
dotnet run -- onelake item-create --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --display-name "NewLakehouse" --type "Lakehouse"
```

> **Note:** `item-create` currently requires the GUID-based `--workspace-id` switch; friendly workspace names are not supported for this command yet.

**Parameters:**
- `--workspace-id`: The ID of the Microsoft Fabric workspace
- `--display-name`: Display name for the new item
- `--type`: Type of item to create (e.g., Lakehouse, Notebook)

### File Operations

#### Read File

Reads the contents of a file from OneLake storage.

```bash
dotnet run -- onelake file-read --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "raw_data/data.json"
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
dotnet run -- onelake file-write --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "test/hello.txt" --content "Hello, OneLake!"
```

**Upload from local file:**
```bash
dotnet run -- onelake file-write --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "data/upload.json" --local-file-path "C:\local\data.json" --overwrite
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
dotnet run -- onelake file-delete --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "temp/unwanted.txt"
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

#### List Files as Blobs

Lists files and directories in OneLake storage as blobs. Browse the contents of a lakehouse or specific directory path with optional recursive listing in blob format.

```bash
dotnet run -- onelake list-blobs --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342"
```

**With path and recursive options:**
```bash
dotnet run -- onelake list-blobs --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --path "raw_data" --recursive
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

#### List Path Structure

Lists files and directories in OneLake storage using a filesystem-style hierarchical view, similar to Azure Data Lake Storage Gen2. Shows directory structure with paths, sizes, timestamps, and metadata.

```bash
dotnet run -- onelake path-list --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342"
```

**With recursive exploration:**
```bash
dotnet run -- onelake path-list --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --recursive
```

**With specific path:**
```bash
dotnet run -- onelake path-list --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --path "analytics/reports"
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

| Feature | `list-blobs` | `path-list` |
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
dotnet run -- onelake directory-create --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --directory-path "analytics/reports/2024"
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
dotnet run -- onelake directory-delete --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --directory-path "temp"
```

**Delete directory and all contents (recursive):**
```bash
dotnet run -- onelake directory-delete --workspace-id "47242da5-ff3b-46fb-a94f-977909b773d5" --item-id "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --directory-path "old_data" --recursive
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
fabmcp.exe onelake onelake-workspace-list

# List items in a specific workspace
fabmcp.exe onelake onelake-item-list --workspace "47242da5-ff3b-46fb-a94f-977909b773d5"
```

### Path & Directory Operations
```cmd
# List files and directories with filesystem view
fabmcp.exe onelake path-list --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --path "Files" --recursive

# Create a new directory
fabmcp.exe onelake directory-create --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --directory-path "mcpdir"

# Delete a directory (with recursive option for non-empty directories)
fabmcp.exe onelake directory-delete --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --directory-path "mcpdir" --recursive
```

### File Operations
```cmd
# Write content to a file
fabmcp.exe onelake file-write --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "mcpdir/hello.txt" --content "Hello, OneLake!"

# Read file contents
fabmcp.exe onelake file-read --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "mcpdir/hello.txt"

# Delete a file
fabmcp.exe onelake file-delete --workspace "47242da5-ff3b-46fb-a94f-977909b773d5" --item "0e67ed13-2bb6-49be-9c87-a1105a4ea342" --file-path "mcpdir/hello.txt"
```

**Note:** Replace the workspace identifier (`47242da5-ff3b-46fb-a94f-977909b773d5`) and item identifier (`0e67ed13-2bb6-49be-9c87-a1105a4ea342`) with your actual Fabric workspace and item values (names or IDs).

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
dotnet run -- onelake directory-create --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --directory-path "pipelines/etl/raw"
dotnet run -- onelake directory-create --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --directory-path "pipelines/etl/processed"

# 2. Upload raw data
dotnet run -- onelake file-write --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --file-path "pipelines/etl/raw/source_data.csv" --local-file-path "C:\data\source.csv"

# 3. Process and write results
dotnet run -- onelake file-write --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --file-path "pipelines/etl/processed/clean_data.parquet" --local-file-path "C:\processed\clean.parquet"

# 4. Clean up temporary files
dotnet run -- onelake directory-delete --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --directory-path "pipelines/etl/temp" --recursive
```

### Analytics Workflow

```bash
# 1. List available workspaces
dotnet run -- onelake onelake-workspace-list

# 2. List items in a workspace
dotnet run -- onelake onelake-item-list --workspace-id "WORKSPACE_ID"

# 3. Explore data structure using hierarchical view
dotnet run -- onelake path-list --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --path "data/reports"

# 4. Get comprehensive file inventory with blob listing
dotnet run -- onelake list-blobs --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --path "data" --recursive

# 5. Read analysis results
dotnet run -- onelake file-read --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --file-path "reports/monthly_summary.json"

# 6. Archive old reports
dotnet run -- onelake directory-create --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --directory-path "archive/2024"
# Move files to archive (requires multiple file operations)
dotnet run -- onelake directory-delete --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --directory-path "reports/old" --recursive
```

### Data Discovery and Exploration

```bash
# 1. Quick overview of lakehouse structure using DFS API (shows directories and permissions)
dotnet run -- onelake path-list --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --path "Tables"

# 2. Comprehensive file search using Blob API (finds all files including nested)
dotnet run -- onelake list-blobs --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --path "Files" --recursive

# 3. Compare structure views - hierarchical vs flat
dotnet run -- onelake path-list --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --path "data/processed"
dotnet run -- onelake list-blobs --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --path "data/processed"

# 4. Find specific file types across the entire lakehouse
dotnet run -- onelake list-blobs --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --recursive | grep "\.parquet"
dotnet run -- onelake list-blobs --workspace-id "WORKSPACE_ID" --item-id "ITEM_ID" --recursive | grep "\.delta"
```

## Error Handling

The tool provides detailed error messages and proper HTTP status codes:

- **401**: Authentication failed - run `az login`
- **403**: Access denied - check workspace permissions
- **404**: Resource not found - verify workspace/item IDs and file paths
- **409**: Conflict - file already exists (use `--overwrite` for file-write)

## Environment Variables

You can set these environment variables for convenience:

```bash
# Target environment (PROD, DAILY, DXT, MSIT)
export ONELAKE_ENVIRONMENT=DAILY

# Frequently used IDs to avoid repetitive typing
export FABRIC_WORKSPACE_ID="47242da5-ff3b-46fb-a94f-977909b773d5"
export FABRIC_ITEM_ID="0e67ed13-2bb6-49be-9c87-a1105a4ea342"
```

Then use them in commands:
```bash
dotnet run -- onelake file-read --workspace-id "$FABRIC_WORKSPACE_ID" --item-id "$FABRIC_ITEM_ID" --file-path "data.json"
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
- **Total Tests**: 76 tests (all passing)
- **Command Tests**: 70 tests covering all 11 OneLake MCP commands
- **Service Architecture Tests**: 6 tests demonstrating testable patterns with dependency injection

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