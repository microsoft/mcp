# PaaS Services Testing Scenario

> **MCP Tool Support Notice**
> Azure MCP Server provides **App Service inspection, Function App discovery, and database connection management** capabilities. Resource creation and application deployment require Azure CLI or Portal. This scenario guides you through complete end-to-end workflows, clearly marking when to use MCP tools vs external tools.

## Objectives

- Test App Service discovery and inspection
- Verify Function App listing and details retrieval
- Test database connection addition to App Services
- Validate end-to-end PaaS resource workflows

## Prerequisites

- [ ] Azure MCP Server installed and configured
- [ ] Azure CLI installed (`az --version`)
- [ ] Authenticated to Azure (`az login`)
- [ ] Active Azure subscription
- [ ] GitHub Copilot with Agent mode enabled

## Scenario 1: App Service Discovery and Database Integration End-to-End

**Objective**: Complete workflow for App Service inspection and database connection setup

### Step 1: Create App Service Resources (External - Not MCP)

> **External Setup Required**: Azure MCP Server cannot create resources. Use GitHub Copilot Chat to run Azure CLI commands or use Azure Portal.

**Option A: Prompt GitHub Copilot Chat** (Recommended):
```
Create an Azure resource group 'bugbash-paas-rg' in eastus, create an App Service Plan with B1 SKU (Linux), create a Web App with Node.js 18 runtime, create an Azure SQL server with a database named 'bugbash-db' using S0 service objective
```

**Option B: Run Azure CLI Commands Manually**:
```bash
# Create resource group
az group create --name bugbash-paas-rg --location eastus

# Create App Service Plan
az appservice plan create \
  --name bugbash-plan \
  --resource-group bugbash-paas-rg \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --name bugbash-webapp-$RANDOM \
  --resource-group bugbash-paas-rg \
  --plan bugbash-plan \
  --runtime "NODE:18-lts"

# Create Azure SQL for database connection testing
az sql server create \
  --name bugbash-sqlserver-$RANDOM \
  --resource-group bugbash-paas-rg \
  --location eastus \
  --admin-user sqladmin \
  --admin-password <secure-password>

az sql db create \
  --name bugbash-db \
  --resource-group bugbash-paas-rg \
  --server <server-name-from-above> \
  --service-objective S0
```

### Step 2: Discover App Services with Azure MCP Server

**2.1 List all App Services** (uses `azmcp_appservice_get`):
```
List all App Services in my subscription
```

**Verify**:
- [ ] Tool invoked: `azmcp_appservice_get`
- [ ] Your web app appears in the list
- [ ] App properties shown (name, location, state, URL)

**2.2 Get specific App Service details**:
```
Show me details for App Service '<webapp-name>' in resource group 'bugbash-paas-rg'
```

**Verify**:
- [ ] Detailed properties displayed
- [ ] Runtime stack shown (Node.js 18)
- [ ] App Service Plan details included
- [ ] Default hostname visible

### Step 3: Add Database Connection with Azure MCP Server

**3.1 Add SQL Server database connection** (uses `azmcp_appservice_database_add`):
```
Add a SQL Server database connection to App Service '<webapp-name>' in resource group 'bugbash-paas-rg' for database '<database-name>' on server '<server-name>'
```

**Verify**:
- [ ] Tool invoked: `azmcp_appservice_database_add`
- [ ] Connection string created
- [ ] Database connection configured
- [ ] Success message displayed

**3.2 Alternative connection types**:
```
Add a PostgreSQL database connection to App Service '<webapp-name>' for database '<pg-database>' on server '<pg-server>'
```

**Verify**:
- [ ] PostgreSQL connection added
- [ ] Connection string format correct

### Step 4: Verify Configuration (External - Not MCP)

> **External Verification**: Use Azure CLI to check configuration

```bash
# List app settings including database connection
az webapp config appsettings list \
  --name <webapp-name> \
  --resource-group bugbash-paas-rg \
  --query "[?name=='SQLAZURECONNSTR_Database']"

# Check connection strings
az webapp config connection-string list \
  --name <webapp-name> \
  --resource-group bugbash-paas-rg
```

### Step 5: Cleanup (External - Not MCP)

```bash
# Delete resource group (removes all resources)
az group delete --name bugbash-paas-rg --yes --no-wait
```

**Expected Results**:
- App Service discovery works correctly
- Detailed inspection successful
- Database connection addition functional
- Connection strings created properly

---

## Scenario 2: Azure Function App Discovery End-to-End

**Objective**: Complete workflow for Function App inspection and discovery

### Step 1: Create Function App Resources (External - Not MCP)

> **External Setup Required**: Use Azure CLI to create Function App

```bash
# Create resource group
az group create --name bugbash-functions-rg --location eastus

# Create storage account (required for Functions)
az storage account create \
  --name bugbashfuncstorage$RANDOM \
  --resource-group bugbash-functions-rg \
  --location eastus \
  --sku Standard_LRS

# Create Function App
az functionapp create \
  --name bugbash-funcapp-$RANDOM \
  --resource-group bugbash-functions-rg \
  --storage-account <storage-account-name-from-above> \
  --consumption-plan-location eastus \
  --runtime node \
  --runtime-version 18 \
  --functions-version 4

# Create another Function App for comparison
az functionapp create \
  --name bugbash-funcapp2-$RANDOM \
  --resource-group bugbash-functions-rg \
  --storage-account <storage-account-name> \
  --consumption-plan-location eastus \
  --runtime python \
  --runtime-version 3.11 \
  --functions-version 4
```

### Step 2: Discover Function Apps with Azure MCP Server

**2.1 List all Function Apps** (uses `azmcp_functionapp_get`):
```
List all Function Apps in my subscription
```

**Verify**:
- [ ] Tool invoked: `azmcp_functionapp_get`
- [ ] Both Function Apps appear
- [ ] Function App properties shown (name, location, runtime, plan)

**2.2 Get specific Function App details**:
```
Show me details for Function App '<funcapp-name>' in resource group 'bugbash-functions-rg'
```

**Verify**:
- [ ] Detailed properties displayed
- [ ] Runtime stack shown (Node.js 18)
- [ ] Consumption plan indicated
- [ ] Storage account connection visible
- [ ] Default hostname shown

**2.3 Filter by resource group**:
```
List all Function Apps in resource group 'bugbash-functions-rg'
```

**Verify**:
- [ ] Only Function Apps in specified resource group listed
- [ ] Filter works correctly

### Step 3: Compare Function App Configurations

**3.1 Compare runtimes**:
- Note Node.js Function App properties
- Note Python Function App properties
- Verify runtime differences are visible

**3.2 Identify configuration differences**:
```
Compare the configurations of Function Apps '<funcapp1-name>' and '<funcapp2-name>'
```

**Verify**:
- [ ] Runtime versions differ
- [ ] Both use Consumption plan
- [ ] Storage accounts may differ

### Step 4: Storage Account Verification with Azure MCP Server

**4.1 List storage accounts** (uses `azmcp_storage_account_get`):
```
List all storage accounts in my subscription
```

**Verify**:
- [ ] Tool invoked: `azmcp_storage_account_get`
- [ ] Function storage account appears
- [ ] Storage properties shown

**4.2 Verify Function App storage connection**:
- Check that Function App uses the created storage account
- Verify storage account location matches Function App

### Step 5: Cleanup (External - Not MCP)

```bash
# Delete resource group (removes all resources)
az group delete --name bugbash-functions-rg --yes --no-wait
```

**Expected Results**:
- Function App discovery works correctly
- Multiple Function Apps listed successfully
- Detailed inspection shows runtime and configuration
- Resource group filtering functional
- Storage account relationship visible

---
## Common Issues to Watch For

| Issue | Description | Resolution |
|-------|-------------|------------|
| **App Service Not Found** | MCP can't locate App Service | Verify resource name and resource group are correct |
| **Function App Missing** | Function App not listed | Ensure Function App exists and you have access permissions |
| **Database Connection Fails** | Can't add database connection | Verify database server exists and firewall allows connections |
| **Runtime Mismatch** | Unexpected runtime shown | Check App Service configuration in Azure Portal |
| **Storage Account Missing** | Function App storage not found | Verify storage account exists in same subscription |
| **Resource Group Filter** | Filtering doesn't work | Ensure resource group name is exact match |

## What to Report

When logging issues, include:
- [ ] Exact prompt used
- [ ] Tool invoked (from MCP tool output)
- [ ] Expected vs actual results
- [ ] Error messages (if any)
- [ ] Resource names and resource group
- [ ] Runtime and plan tier
- [ ] Screenshots of unexpected behavior

## Related Resources

- [Azure App Service Documentation](https://learn.microsoft.com/azure/app-service/)
- [Azure Functions Documentation](https://learn.microsoft.com/azure/azure-functions/)
- [MCP Command Reference](../../servers/Azure.Mcp.Server/docs/azmcp-commands.md)
- [E2E Test Prompts](../../servers/Azure.Mcp.Server/docs/e2eTestPrompts.md)
- [Report Issues](https://github.com/microsoft/mcp/issues)

## Quick Reference: Supported MCP Tools

### App Service
- `azmcp_appservice_get` - Get App Service details (list all or specific app)
- `azmcp_appservice_database_add` - Add database connection to App Service

### Function Apps
- `azmcp_functionapp_get` - Get Function App details (list all or specific function app)

### Storage
- `azmcp_storage_account_get` - List storage accounts

### Database Support
- SQL Server (`SqlServer`)
- PostgreSQL (`PostgreSQL`)
- MySQL (`MySQL`)
- Cosmos DB (`CosmosDB`)

---

**Next**: [Storage Operations Testing](storage-operations.md)
