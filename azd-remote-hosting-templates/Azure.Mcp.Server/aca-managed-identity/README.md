# Azure MCP Server - ACA with Managed Identity

Reference Azure Developer CLI (azd) template for deploying Azure MCP Server to Azure Container Apps with storage tools enabled.

The MCP server storage tools uses the Container App's managed identity for outgoing authentication to Azure Storage.

AI Foundry agents access the MCP server storage tools using the AI Foundry project's managed identity for incoming authentication.

## Prerequisites

- Azure subscription with **Owner** or **User Access Administrator** permissions
- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)

## Quick Start

```bash
azd up
```

You'll be prompted for:
- **Storage Account Resource ID** - The Azure resource ID of the storage account the MCP server will access
- **AI Foundry Project Resource ID** - The Azure resource ID of the AI Foundry project for agent integration

## What Gets Deployed

- **Container App** - Runs Azure MCP Server with storage namespace
- **Role Assignments** - Container App managed identity granted roles for outbound authentication to storage:
  - Storage Account Contributor
  - Storage Blob Data Contributor
- **Entra App Registration** - For incoming OAuth 2.0 authentication from clients (e.g., agents) with `Mcp.Tools.ReadWrite.All` role
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

## Using from AI Foundry Agent

1. Get your Container App URL from `azd` output: `CONTAINER_APP_URL`
2. Get Entra App Client ID from `azd` output: `ENTRA_APP_CLIENT_ID`
3. &lt;TODO: Add one liner AI Foundry integration step later (reference to AIF documentation)&gt;

## Clean Up

```bash
azd down
```
