# Storage Operations Testing Scenario

> **IMPORTANT**: Azure MCP Server provides **read-only inspection and querying** of Azure Storage accounts. It **cannot create, modify, upload, or delete** storage resources. This scenario guides you through creating resources externally (Azure CLI or Portal) and then using Azure MCP Server to inspect them.

## Objectives

- Test storage account discovery and inspection capabilities
- Verify container listing functionality
- Test blob enumeration
- Validate storage account filtering and querying

## Prerequisites

- [ ] Azure MCP Server installed and configured
- [ ] Azure CLI installed (`az --version`)
- [ ] Authenticated to Azure (`az login`)
- [ ] Active Azure subscription
- [ ] Resource group for testing (or create one)
- [ ] GitHub Copilot with Agent mode enabled

---

## Scenario 1: Storage Account Discovery and Inspection End-to-End

**Objective**: Test complete workflow of creating a storage account externally, then discovering and inspecting it via Azure MCP Server.

### External Setup Required

#### Step 1: Create Storage Account (External - Not MCP)

> **External Setup Required**: Azure MCP Server cannot create resources. Use GitHub Copilot Chat to run Azure CLI commands or use Azure Portal.

**Option A: Prompt GitHub Copilot Chat** (Recommended):
```
Create an Azure resource group 'bugbash-storage-rg' in eastus, create a storage account with Standard_LRS SKU and Hot access tier with hierarchical namespace enabled, create two blob containers 'test-data' (private) and 'public-data' (public blob access), then upload a test file to test-data container
```

**Option B: Run Azure CLI Commands Manually**:
```bash
# Set variables
RG_NAME="bugbash-storage-rg"
LOCATION="eastus"
STORAGE_ACCOUNT="bugbashstorage$(Get-Random -Maximum 9999)"

# Create resource group
az group create --name $RG_NAME --location $LOCATION

# Create storage account with hierarchical namespace
az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group $RG_NAME \
  --location $LOCATION \
  --sku Standard_LRS \
  --access-tier Hot \
  --enable-hierarchical-namespace true

# Create blob containers
az storage container create \
  --name test-data \
  --account-name $STORAGE_ACCOUNT \
  --public-access off

az storage container create \
  --name public-data \
  --account-name $STORAGE_ACCOUNT \
  --public-access blob

# Upload sample blob
echo "Test content for bug bash" > test-file.txt
az storage blob upload \
  --account-name $STORAGE_ACCOUNT \
  --container-name test-data \
  --name test-file.txt \
  --file test-file.txt
```

**Verify External Setup**:
- [ ] Storage account created successfully
- [ ] Hierarchical namespace enabled
- [ ] Two containers created (test-data, public-data)
- [ ] Sample blob uploaded to test-data container

### Azure MCP Server Prompts

#### Step 2: Discover Storage Accounts

Open GitHub Copilot Chat in Agent mode and use these prompts:

**Prompt 2a**: List all storage accounts in subscription
```
Show me all storage accounts in my subscription
```

**Tool Verification**:
- [ ] Tool invoked: `azmcp_storage_account_get`
- [ ] Returns list of storage accounts
- [ ] Includes your new storage account

**Prompt 2b**: Get specific storage account details
```
Show me details for storage account '<storage-account-name>' in resource group 'bugbash-storage-rg'
```

**Tool Verification**:
- [ ] Tool invoked: `azmcp_storage_account_get` with `--account` and `--resource-group` parameters
- [ ] Returns single storage account
- [ ] Shows SKU as Standard_LRS
- [ ] Shows access tier as Hot
- [ ] Shows hierarchical namespace enabled
- [ ] Shows location as eastus

#### Step 3: List Blob Containers

**Prompt 3a**: List all containers
```
List all blob containers in storage account '<storage-account-name>'
```

**Tool Verification**:
- [ ] Tool invoked: `azmcp_storage_container_list`
- [ ] Returns both containers (test-data, public-data)
- [ ] Shows public access levels correctly

#### Step 4: List Blobs in Container

**Prompt 4a**: List blobs in test-data container
```
Show me all blobs in container 'test-data' from storage account '<storage-account-name>'
```

**Tool Verification**:
- [ ] Tool invoked: `azmcp_storage_blob_list`
- [ ] Returns test-file.txt blob
- [ ] Shows blob size
- [ ] Shows last modified date

**Prompt 4b**: Get specific blob details
```
Get details for blob 'test-file.txt' in container 'test-data' from storage account '<storage-account-name>'
```

**Tool Verification**:
- [ ] Tool invoked: `azmcp_storage_blob_get`
- [ ] Returns blob metadata
- [ ] Shows content type
- [ ] Shows blob properties

#### Step 5: Filter Storage Accounts by Resource Group

**Prompt 5a**: Filter by resource group
```
Show me all storage accounts in resource group 'bugbash-storage-rg'
```

**Tool Verification**:
- [ ] Tool invoked: `azmcp_storage_account_get` with `--resource-group` parameter
- [ ] Returns only storage accounts in specified resource group
- [ ] Excludes storage accounts from other resource groups

### Cleanup

**Option A: Prompt GitHub Copilot Chat**:
```
Delete the Azure resource group 'bugbash-storage-rg' and all its resources
```

**Option B: Run Azure CLI Command Manually**:
```bash
# Delete the resource group and all resources
az group delete --name bugbash-storage-rg --yes --no-wait
```

---

## Scenario 2: Multi-Container Blob Discovery End-to-End

**Objective**: Test Azure MCP Server's ability to discover and inspect multiple containers with different access levels and blob types.

### External Setup Required

#### Step 1: Create Storage Account with Multiple Containers (External - Not MCP)

> **External Setup Required**: Azure MCP Server cannot create resources. Use GitHub Copilot Chat to run Azure CLI commands or use Azure Portal.

**Option A: Prompt GitHub Copilot Chat** (Recommended):
```
Create an Azure resource group 'bugbash-multicontainer-rg' in westus2, create a storage account with Standard_GRS SKU and Cool access tier, create three containers: 'private-documents' (private), 'public-images' (blob public access), 'shared-data' (container public access), then upload sample files with different content types
```

**Option B: Run Azure CLI Commands Manually**:
```bash
# Set variables
RG_NAME="bugbash-multicontainer-rg"
LOCATION="westus2"
STORAGE_ACCOUNT="bugbashmulti$(Get-Random -Maximum 9999)"

# Create resource group and storage account
az group create --name $RG_NAME --location $LOCATION

az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group $RG_NAME \
  --location $LOCATION \
  --sku Standard_GRS \
  --access-tier Cool

# Create multiple containers with different access levels
az storage container create \
  --name private-documents \
  --account-name $STORAGE_ACCOUNT \
  --public-access off

az storage container create \
  --name public-images \
  --account-name $STORAGE_ACCOUNT \
  --public-access blob

az storage container create \
  --name shared-data \
  --account-name $STORAGE_ACCOUNT \
  --public-access container

# Upload different file types
echo "Private document content" > document.txt
echo "{ \"data\": \"test\" }" > data.json
echo "<html><body>Test</body></html>" > index.html

az storage blob upload --account-name $STORAGE_ACCOUNT --container-name private-documents --name document.txt --file document.txt
az storage blob upload --account-name $STORAGE_ACCOUNT --container-name public-images --name image.png --file document.txt --content-type "image/png"
az storage blob upload --account-name $STORAGE_ACCOUNT --container-name shared-data --name data.json --file data.json --content-type "application/json"
az storage blob upload --account-name $STORAGE_ACCOUNT --container-name shared-data --name index.html --file index.html --content-type "text/html"
```

**Verify External Setup**:
- [ ] Storage account created with Standard_GRS SKU
- [ ] Three containers with different access levels
- [ ] Multiple blobs uploaded with different content types

### Azure MCP Server Prompts

#### Step 2: Discover Storage Account with Cool Tier

**Prompt 2a**: Get storage account details
```
Show me the storage account '<storage-account-name>' in resource group 'bugbash-multicontainer-rg'
```

**Tool Verification**:
- [ ] Tool invoked: `azmcp_storage_account_get`
- [ ] Shows SKU as Standard_GRS
- [ ] Shows access tier as Cool
- [ ] Shows location as westus2

#### Step 3: List All Containers and Their Access Levels

**Prompt 3a**: List containers
```
List all blob containers in storage account '<storage-account-name>' and show their public access settings
```

**Tool Verification**:
- [ ] Tool invoked: `azmcp_storage_container_list`
- [ ] Returns three containers: private-documents, public-images, shared-data
- [ ] Shows correct public access level for each:
  - [ ] private-documents: None/Off
  - [ ] public-images: Blob
  - [ ] shared-data: Container

#### Step 4: Inspect Blobs Across Multiple Containers

**Prompt 4a**: List blobs in private-documents
```
Show me all blobs in the 'private-documents' container from storage account '<storage-account-name>'
```

**Tool Verification**:
- [ ] Tool invoked: `azmcp_storage_blob_list` with `--container` parameter
- [ ] Returns document.txt
- [ ] Shows blob size and last modified date

**Prompt 4b**: List blobs in shared-data
```
List all blobs in container 'shared-data' from storage account '<storage-account-name>'
```

**Tool Verification**:
- [ ] Tool invoked: `azmcp_storage_blob_list`
- [ ] Returns both blobs: data.json, index.html
- [ ] Shows correct content types for each blob

**Prompt 4c**: Get specific blob details
```
Get details for blob 'data.json' in container 'shared-data' from storage account '<storage-account-name>'
```

**Tool Verification**:
- [ ] Tool invoked: `azmcp_storage_blob_get`
- [ ] Shows content type as application/json
- [ ] Shows blob properties and metadata

#### Step 5: Test Cross-Subscription Discovery (if available)

**Prompt 5a**: List storage accounts across all accessible subscriptions
```
Show me all storage accounts I have access to across all my subscriptions
```

**Tool Verification**:
- [ ] Tool invoked: `azmcp_storage_account_get` without subscription filter
- [ ] Returns storage accounts from multiple subscriptions (if you have access)
- [ ] Groups results by subscription

### Cleanup

**Option A: Prompt GitHub Copilot Chat**:
```
Delete the Azure resource group 'bugbash-multicontainer-rg' and all its resources
```

**Option B: Run Azure CLI Command Manually**:
```bash
# Delete the resource group and all resources
az group delete --name bugbash-multicontainer-rg --yes --no-wait
```

---

## Common Issues to Watch For

| Issue | Description | Resolution |
|-------|-------------|------------|
| **Storage Account Not Found** | MCP can't locate storage account | Verify account name spelling and ensure you have access permissions |
| **Container List Empty** | No containers returned | Ensure containers exist and storage account name is correct |
| **Blob List Empty** | No blobs returned for container | Verify container name and that blobs have been uploaded |
| **Authentication Errors** | Can't access storage resources | Run `az login` and verify RBAC permissions (Storage Blob Data Reader/Contributor) |
| **SKU Mismatch** | Unexpected SKU displayed | Check actual storage account configuration in Azure Portal |
| **Access Tier Confusion** | Hot vs Cool tier not displayed correctly | Verify account tier in Azure Portal matches MCP output |
| **Hierarchical Namespace** | Data Lake Gen2 feature not shown | Confirm feature was enabled during storage account creation |
| **Cross-Subscription Issues** | Storage accounts missing from list | Verify you have appropriate permissions in all subscriptions |

## What to Report

When logging issues, include:
- [ ] Exact prompt used
- [ ] Tool invoked (from MCP tool output: `azmcp_storage_account_get`, `azmcp_storage_container_list`, `azmcp_storage_blob_list`, etc.)
- [ ] Expected vs actual results
- [ ] Error messages (if any)
- [ ] Storage account name and resource group
- [ ] Container names and public access levels
- [ ] Blob names and content types
- [ ] Screenshots of unexpected behavior
- [ ] Output from Azure CLI showing actual resource state

## Related Resources

- [Azure Storage Documentation](https://learn.microsoft.com/azure/storage/)
- [Azure Blob Storage Best Practices](https://learn.microsoft.com/azure/storage/blobs/storage-blobs-introduction)
- [Data Lake Storage Gen2](https://learn.microsoft.com/azure/storage/blobs/data-lake-storage-introduction)
- [MCP Command Reference](../../servers/Azure.Mcp.Server/docs/azmcp-commands.md)
- [E2E Test Prompts](../../servers/Azure.Mcp.Server/docs/e2eTestPrompts.md)
- [Report Issues](https://github.com/microsoft/mcp/issues)

## Quick Reference: Supported MCP Tools

### Storage Account Operations
- `azmcp_storage_account_get` - Get storage account details (list all or specific account)
  - Parameters: `--subscription`, `--resource-group`, `--account`
  - Returns: Storage account properties (SKU, access tier, location, hierarchical namespace)

### Container Operations
- `azmcp_storage_container_list` - List blob containers in a storage account
  - Parameters: `--account`, `--resource-group`
  - Returns: Container names and public access levels

### Blob Operations
- `azmcp_storage_blob_list` - List blobs in a container
  - Parameters: `--account`, `--container`, `--resource-group`
  - Returns: Blob names, sizes, last modified dates
  
- `azmcp_storage_blob_get` - Get specific blob details
  - Parameters: `--account`, `--container`, `--blob`, `--resource-group`
  - Returns: Blob metadata, content type, properties

### Important Notes
- **Read-Only**: All MCP tools are read-only inspection tools
- **No Write Operations**: Cannot create, upload, modify, or delete resources
- **Authentication**: Requires Azure RBAC permissions (Reader or Storage Blob Data Reader minimum)
- **Filtering**: Supports filtering by subscription, resource group, and specific resource names

---

**Next**: [Agent Building](agent-building.md)
