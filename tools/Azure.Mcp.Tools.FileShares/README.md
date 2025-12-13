# Azure File Shares MCP Tools

This toolset provides Model Context Protocol (MCP) commands for managing Azure File Shares resources through the Azure MCP Server.

## Overview

The Azure.Mcp.Tools.FileShares toolset enables AI agents and developers to perform operations on Azure File Shares, including:

- **File Share Management**: List, get, create, update, and delete file shares
- **Snapshot Management**: Create and manage snapshots of file shares
- **Name Validation**: Check name availability for new file shares

## Commands

### File Share Commands

#### azmcp fileshares fileshare list
List all file shares in a subscription or resource group.

**Parameters:**
- `--subscription`: Azure subscription ID or name (required)
- `--resource-group`: Filter by resource group (optional)
- `--filter`: Optional filter expression (optional)

**Example:**
```bash
azmcp fileshares fileshare list --subscription mysubscription --resource-group myresourcegroup
```

#### azmcp fileshares fileshare get
Get details of a specific file share.

**Parameters:**
- `--subscription`: Azure subscription ID or name (required)
- `--resource-group`: Resource group containing the file share (required)
- `--name`: Name of the file share (required)

**Example:**
```bash
azmcp fileshares fileshare get --subscription mysubscription --resource-group myresourcegroup --name myfileshare
```

#### azmcp fileshares fileshare create
Create or update a file share.

**Parameters:**
- `--subscription`: Azure subscription ID or name (required)
- `--resource-group`: Resource group for the file share (required)
- `--name`: Name of the file share (required)
- `--location`: Azure region (required)

**Example:**
```bash
azmcp fileshares fileshare create --subscription mysubscription --resource-group myresourcegroup --name myfileshare --location eastus
```

#### azmcp fileshares fileshare delete
Delete a file share.

**Parameters:**
- `--subscription`: Azure subscription ID or name (required)
- `--resource-group`: Resource group containing the file share (required)
- `--name`: Name of the file share to delete (required)

**Example:**
```bash
azmcp fileshares fileshare delete --subscription mysubscription --resource-group myresourcegroup --name myfileshare
```

#### azmcp fileshares fileshare checkname
Check if a file share name is available.

**Parameters:**
- `--subscription`: Azure subscription ID or name (required)
- `--location`: Azure region to check (required)
- `--name`: Proposed file share name (required)

**Example:**
```bash
azmcp fileshares fileshare checkname --subscription mysubscription --location eastus --name myfileshare
```

### Snapshot Commands

#### azmcp fileshares fileshare snapshot list
List all snapshots for a file share.

**Parameters:**
- `--subscription`: Azure subscription ID or name (required)
- `--resource-group`: Resource group containing the file share (required)
- `--name`: Parent file share name (required)

**Example:**
```bash
azmcp fileshares fileshare snapshot list --subscription mysubscription --resource-group myresourcegroup --name myfileshare
```

#### azmcp fileshares fileshare snapshot get
Get details of a specific snapshot.

**Parameters:**
- `--subscription`: Azure subscription ID or name (required)
- `--resource-group`: Resource group containing the file share (required)
- `--name`: Parent file share name (required)
- `--snapshot-name`: Snapshot name (required)

**Example:**
```bash
azmcp fileshares fileshare snapshot get --subscription mysubscription --resource-group myresourcegroup --name myfileshare --snapshot-name mysnapshot
```

#### azmcp fileshares fileshare snapshot create
Create a snapshot of a file share.

**Parameters:**
- `--subscription`: Azure subscription ID or name (required)
- `--resource-group`: Resource group containing the file share (required)
- `--name`: Parent file share name (required)

**Example:**
```bash
azmcp fileshares fileshare snapshot create --subscription mysubscription --resource-group myresourcegroup --name myfileshare
```

## Implementation Details

### Architecture

The toolset follows the standard Azure MCP toolset pattern:

- **Commands**: Located in `src/Commands/`, each command inherits from `BaseFileSharesCommand`
- **Services**: `IFileSharesService` and `FileSharesService` handle Azure API interactions
- **Options**: Command-specific options classes define input parameters
- **JSON Context**: `FileSharesJsonContext` provides AOT-safe serialization

### Service Implementation

The `FileSharesService` class uses Azure Resource Manager (ARM) APIs to interact with Azure File Shares resources. It inherits from `BaseAzureService` for standard Azure authentication and error handling.

### Testing

The toolset includes comprehensive test coverage:

- **Unit Tests**: Located in `tests/Azure.Mcp.Tools.FileShares.UnitTests/`
- **Live Tests**: Located in `tests/Azure.Mcp.Tools.FileShares.LiveTests/`
- **Test Infrastructure**: Bicep template (`test-resources.bicep`) and PowerShell script (`test-resources-post.ps1`)

## Building and Testing

### Build the toolset
```powershell
dotnet build "tools/Azure.Mcp.Tools.FileShares/src/Azure.Mcp.Tools.FileShares.csproj"
```

### Run unit tests
```powershell
dotnet test "tools/Azure.Mcp.Tools.FileShares/tests/Azure.Mcp.Tools.FileShares.UnitTests/Azure.Mcp.Tools.FileShares.UnitTests.csproj"
```

### Run live tests (requires Azure resources)
```powershell
# Deploy test infrastructure
./eng/scripts/Deploy-TestResources.ps1 -Tool "FileShares"

# Run live tests
dotnet test "tools/Azure.Mcp.Tools.FileShares/tests/Azure.Mcp.Tools.FileShares.LiveTests/Azure.Mcp.Tools.FileShares.LiveTests.csproj"
```

## Contributing

When adding new commands to this toolset:

1. Follow the naming convention: `{Resource}{Operation}Command`
2. Create corresponding options class: `{Resource}{Operation}Options`
3. Add method to `IFileSharesService` interface and implementation
4. Register command in `FileSharesSetup.ConfigureServices()`
5. Add unit tests for validation and execution paths
6. Add live test for Azure API interactions
7. Update this README with command documentation

## References

- [new-command.md](../../servers/Azure.Mcp.Server/docs/new-command.md) - Complete command implementation guide
- [Commands.md](./Commands.md) - Detailed command reference
- [Azure File Shares Documentation](https://learn.microsoft.com/en-us/azure/storage/files/storage-files-introduction)
