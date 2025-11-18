# Azure MCP Server - ACA with Managed Identity

Deploy the [Azure MCP Server 2.0-beta](https://mcr.microsoft.com/product/azure-sdk/azure-mcp) as a remote MCP server accessible over HTTPS. This enables AI agents from [Microsoft Foundry](https://azure.microsoft.com/products/ai-foundry) and [Microsoft Copilot Studio](https://www.microsoft.com/microsoft-copilot/microsoft-copilot-studio) to securely invoke MCP tool calls that perform Azure operations on your behalf.

This reference Azure Developer CLI (azd) template shows how to host the server on Azure Container Apps with storage tools enabled, using managed identity authentication for secure access to Azure Storage.

## Prerequisites

- Azure subscription with **Owner** or **User Access Administrator** permissions
- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- The list of Azure MCP Server tool areas (namespaces) you wish to enable (see [azmcp-commands.md](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md)). This reference template uses the `storage` namespace

## Quick Start

This reference template deploys the Azure MCP Server with **read-only** Azure Storage tools enabled, accessible over HTTPS transport. For details on customizing server startup flags and configuration, see [Azure MCP Server documentation](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md).

```bash
azd up
```

You'll be prompted for:
- **Storage Account Resource ID** - The Azure resource ID of the storage account the MCP server will access
- **Microsoft Foundry Project Resource ID** - The Azure resource ID of the Microsoft Foundry project for agent integration

## What Gets Deployed

- **Container App** - Runs Azure MCP Server with storage namespace
- **Role Assignments** - Container App managed identity granted roles for outbound authentication to the storage account specified by the input storage resource ID:
  - Reader (read-only access to storage account properties)
  - Storage Blob Data Reader (read-only access to blob data)
- **Entra App Registration** - For incoming OAuth 2.0 authentication from clients (e.g., agents) with `Mcp.Tools.ReadWrite.All` role. This role is assigned to the managed identity of the Microsoft Foundry project specified by the input Microsoft Foundry resource ID
- **Application Insights** - Telemetry and monitoring

### Deployment Outputs

After deployment, retrieve `azd` outputs:

```bash
azd env get-values
```

Example output:
```
CONTAINER_APP_URL="https://azure-mcp-storage-server.wonderfulazmcp-a9561afd.eastus2.azurecontainerapps.io"
ENTRA_APP_CLIENT_ID="c3248eaf-3bdd-4ca7-9483-4fcf213e4d4d"
ENTRA_APP_IDENTIFIER_URI="api://c3248eaf-3bdd-4ca7-9483-4fcf213e4d4d"
ENTRA_APP_OBJECT_ID="a89055df-ccfc-4aef-a7c6-9561bc4c5386"
ENTRA_APP_ROLE_ID="3e60879b-a1bd-5faf-bb8c-cb55e3bfeeb8"
ENTRA_APP_SERVICE_PRINCIPAL_ID="31b42369-583b-40b7-a535-ad343f75e463"
```

## Using from Microsoft Foundry Agent

1. Get your Container App URL from `azd` output: `CONTAINER_APP_URL`
2. Get Entra App Client ID from `azd` output: `ENTRA_APP_CLIENT_ID`
3. <TODO: Add one liner Microsoft Foundry integration step later (reference to Microsoft Foundry documentation)>

## Clean Up

```bash
azd down
```

## Template Structure

The `azd` template consists of the following Bicep modules:

- **`main.bicep`** - Orchestrates the deployment of all resources
- **`aca-infrastructure.bicep`** - Deploys Container App hosting the Azure MCP Server
- **`aca-role-assignment-resource-storage.bicep`** - Assigns Azure storage RBAC roles to the Container App managed identity on the storage account specified by the input storage resource ID
- **`entra-app.bicep`** - Creates Entra App registration with custom app role for OAuth 2.0 authentication
- **`aif-role-assignment-entraapp.bicep`** - Assigns Entra App role to the managed identity of the Microsoft Foundry project specified by the input Microsoft Foundry resource ID for the Azure MCP Server access
- **`application-insights.bicep`** - Deploys Application Insights for telemetry and monitoring (conditional deployment)

