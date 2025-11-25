# Infrastructure as Code Testing Scenario

> **MCP Tool Support Notice**
> Azure MCP Server provides **Bicep schema retrieval and IaC generation rules** capabilities. Actual Bicep template generation and infrastructure deployment require external tools (GitHub Copilot for code generation, Azure CLI/Bicep for deployment). This scenario guides you through complete end-to-end workflows, clearly marking when to use MCP tools vs external tools.

## Objectives

- Test Bicep schema retrieval for Azure resources
- Verify IaC generation rules and recommendations
- Test Terraform best practices guidance
- Validate end-to-end IaC workflow with MCP assistance

## Prerequisites

- [ ] Azure MCP Server installed and configured
- [ ] Azure CLI installed (`az --version`)
- [ ] Bicep CLI installed (`az bicep version`)
- [ ] Authenticated to Azure (`az login`)
- [ ] Active Azure subscription
- [ ] GitHub Copilot with Agent mode enabled

---

## Scenario 1: Bicep Schema and Template Generation End-to-End

**Objective**: Complete workflow using Bicep schema assistance and deployment

### Step 1: Get Bicep Schema with Azure MCP Server

**1.1 Get storage account schema** (uses `azmcp_bicepschema_get`):
```
Show me the Bicep schema for Azure Storage Account resource type
```

**Verify**:
- [ ] Tool invoked: `azmcp_bicepschema_get`
- [ ] Schema definition returned
- [ ] Resource properties documented
- [ ] Property types shown
- [ ] Required vs optional properties identified

**1.2 Get App Service schema**:
```
Show me the Bicep schema for Microsoft.Web/sites resource type
```

**Verify**:
- [ ] App Service schema retrieved
- [ ] Configuration properties listed
- [ ] Schema is accurate and complete

**1.3 Get SQL Database schema**:
```
Get the Bicep schema for Azure SQL Database
```

**Verify**:
- [ ] SQL Database schema returned
- [ ] All properties documented

### Step 2: Get IaC Generation Rules with Azure MCP Server

**2.1 Get rules for Storage Account** (uses `azmcp_deploy_iac_rules_get`):
```
Show me the IaC generation rules for Azure Storage Account using Bicep
```

**Verify**:
- [ ] Tool invoked: `azmcp_deploy_iac_rules_get`
- [ ] Best practices provided
- [ ] Recommended parameters shown
- [ ] Configuration guidance included
- [ ] Security recommendations mentioned

**2.2 Get rules for App Service**:
```
Show me the IaC generation rules for Azure App Service and App Service Plan using Bicep
```

**Verify**:
- [ ] Rules for both resources provided
- [ ] Dependency relationships explained
- [ ] SKU recommendations included

### Step 3: Generate Bicep Template (GitHub Copilot - Not MCP)

> **External Tool Required**: Use GitHub Copilot for template generation

**Generate Bicep template** (use GitHub Copilot Chat in VS Code):
```
Generate a Bicep template to create:
- Azure Storage Account with Standard_LRS redundancy
- Blob container for data storage
- App Service Plan (B1 tier)
- App Service web app with storage connection

Include parameters for resource names and location.
```

**Save template** as `infrastructure.bicep`

### Step 4: Deploy Infrastructure (Azure CLI/Bicep - Not MCP)

> **External Deployment Required**: Use Azure CLI

```bash
# Create resource group
az group create --name bugbash-iac-rg --location eastus

# Deploy Bicep template
az deployment group create \
  --resource-group bugbash-iac-rg \
  --template-file infrastructure.bicep \
  --parameters storageAccountName=bugbashstorage$RANDOM \
               appServiceName=bugbash-app-$RANDOM \
               location=eastus
```

### Step 5: Verify Deployment with Azure MCP Server

**5.1 List storage accounts** (uses `azmcp_storage_account_get`):
```
List all storage accounts in my subscription
```

**Verify**:
- [ ] Tool invoked: `azmcp_storage_account_get`
- [ ] Your deployed storage account appears
- [ ] Account properties match template

**5.2 List blob containers** (uses `azmcp_storage_blob_container_get`):
```
List all containers in storage account '<account-name>'
```

**Verify**:
- [ ] Tool invoked: `azmcp_storage_blob_container_get`
- [ ] Container from template is listed

**5.3 Get App Service details** (uses `azmcp_appservice_get`):
```
Show me details for App Service '<app-name>' in resource group 'bugbash-iac-rg'
```

**Verify**:
- [ ] Tool invoked: `azmcp_appservice_get`
- [ ] App Service configuration shown
- [ ] Properties match template specification

### Step 6: Cleanup (External - Not MCP)

```bash
# Delete resource group (removes all resources)
az group delete --name bugbash-iac-rg --yes --no-wait
```

**Expected Results**:
- Bicep schema retrieval successful
- IaC generation rules comprehensive
- Template generation guided by schema
- Deployment verification through MCP tools


## Common Issues to Watch For

| Issue | Description | Resolution |
|-------|-------------|------------|
| **Invalid Resource Type** | Bicep schema not found | Verify resource type format (e.g., Microsoft.Storage/storageAccounts) |
| **Schema Version Mismatch** | API version differences | Use latest stable API version for resources |
| **Deployment Tool Confusion** | Wrong tool used | Specify "Bicep" or "Terraform" explicitly in prompts |
| **Missing Best Practices** | Generic guidance returned | Be specific about resource type and deployment tool |
| **Template Syntax Errors** | Generated template invalid | Use retrieved schema to validate template structure |

## What to Report

When logging issues, include:
- [ ] Exact prompt used
- [ ] Tool invoked (from MCP tool output)
- [ ] Expected vs actual results
- [ ] Resource type requested
- [ ] Deployment tool (Bicep/Terraform)
- [ ] Error messages (if any)
- [ ] Screenshots of unexpected behavior

## Related Resources

- [Azure Bicep Documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
- [Azure Terraform Documentation](https://learn.microsoft.com/azure/developer/terraform/)
- [Azure Resource Manager](https://learn.microsoft.com/azure/azure-resource-manager/)
- [MCP Command Reference](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md)
- [E2E Test Prompts](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md)
- [Report Issues](https://github.com/microsoft/mcp/issues)

## Quick Reference: Supported MCP Tools

### Bicep Support
- `azmcp_bicepschema_get` - Get Bicep schema for resource types

### IaC Guidance
- `azmcp_deploy_iac_rules_get` - Get IaC generation rules for resource types
- `azmcp_azureterraformbestpractices_get` - Get Terraform best practices
- `azmcp_bestpractices_get` - Get general Azure best practices

### Resource Verification
- `azmcp_storage_account_get` - List storage accounts
- `azmcp_storage_blob_container_get` - List blob containers
- `azmcp_appservice_get` - Get App Service details
- `azmcp_sql_server_list` - List SQL servers
- `azmcp_cosmos_account_list` - List Cosmos DB accounts

---

**Next**: [PaaS Services Testing](https://github.com/microsoft/mcp/tree/main/docs/bug-bash/scenarios/paas-services.md)
