# Connecting to Sovereign Clouds

The Azure MCP Server supports connecting to Azure sovereign clouds (national clouds) and custom cloud environments by configuring the authority host used for authentication.

## Overview

By default, the Azure MCP Server authenticates against the Azure Public Cloud (`login.microsoftonline.com`). To connect to a sovereign cloud or custom environment, you can specify the cloud environment using the `--cloud` option, configuration files, or environment variables.

## Supported Cloud Environments

### Well-Known Sovereign Clouds

The following cloud names are recognized and automatically mapped to their respective authority hosts:

| Cloud Name | Authority Host | Aliases |
|------------|----------------|---------|
| Azure Public Cloud | `https://login.microsoftonline.com` | `AzureCloud`, `AzurePublicCloud`, `Public`, `AzurePublic` |
| Azure China Cloud | `https://login.chinacloudapi.cn` | `AzureChinaCloud`, `China`, `AzureChina` |
| Azure US Government | `https://login.microsoftonline.us` | `AzureUSGovernment`, `USGov`, `AzureUSGovernmentCloud`, `USGovernment` |

*_The aliases are case insensitive._

## Configuration Methods

You can configure the cloud environment using one of the following methods. The server resolves the cloud configuration using this priority order:

| Priority | Source | Configuration Key |
|----------|--------|-------------------|
| 1 | Command line argument | `--cloud` |
| 2 | IConfiguration | `AZURE_CLOUD` |
| 3 | IConfiguration | `azure_cloud` |
| 4 | IConfiguration | `cloud` |
| 5 | IConfiguration | `Cloud` |
| 6 | Environment variable | `AZURE_CLOUD` (direct fallback) |
| Default | Fallback | `AzurePublicCloud` |

> **Note:** IConfiguration in .NET includes multiple providers in order: `appsettings.json`, `appsettings.{Environment}.json`, user secrets (development), environment variables, and command line arguments. This means environment variables set via `AZURE_CLOUD` will be found at priority 2 through the IConfiguration system before the direct fallback at priority 6.

### 1. Command Line Argument (Highest Priority)

Use the `--cloud` option when starting the server:

```bash
# Azure China Cloud
azmcp server start --cloud AzureChinaCloud

# Azure US Government
azmcp server start --cloud AzureUSGovernment
```

### 2. Configuration File (appsettings.json)

Add the `cloud` property to your `appsettings.json`:

```json
{
  "cloud": "AzureChinaCloud",
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### 3. Environment Variable (Lowest Priority)

Set the `AZURE_CLOUD` environment variable:

```bash
# PowerShell
$env:AZURE_CLOUD = "AzureChinaCloud"
azmcp server start

# Bash
export AZURE_CLOUD=AzureChinaCloud
azmcp server start

# Windows Command Prompt
set AZURE_CLOUD=AzureChinaCloud
azmcp server start
```

## Authentication Considerations

### Credential Chain

The Azure MCP Server uses a chained credential approach that tries multiple authentication methods in order. All credentials in the chain will use the configured authority host:

- Workload Identity Credential
- Managed Identity Credential
- Visual Studio Credential
- Visual Studio Code Credential
- Azure CLI Credential
- Azure PowerShell Credential
- Azure Developer CLI Credential
- Interactive Browser Credential

### Pre-Authentication Steps

Before connecting to a sovereign cloud, ensure your local tools are authenticated against the correct cloud:

#### Azure CLI

```bash
# Azure China Cloud
az cloud set --name AzureChinaCloud
az login

# Azure US Government
az cloud set --name AzureUSGovernment
az login
```

#### Azure PowerShell

```powershell
# Azure China Cloud
Connect-AzAccount -Environment AzureChinaCloud

# Azure US Government
Connect-AzAccount -Environment AzureUSGovernment
```

#### Azure Developer CLI

```bash
# Azure China Cloud
azd config set cloud.name AzureChinaCloud
azd auth login

# Azure US Government
azd config set cloud.name AzureUSGovernment
azd auth login
```

## Examples

### Example 1: Azure China Cloud with CLI

```bash
# Authenticate with Azure CLI
az cloud set --name AzureChinaCloud
az login

# Start the MCP server
azmcp server start --cloud AzureChinaCloud
```

### Example 2: Azure US Government with Environment Variable

```powershell
# Set environment variable
$env:AZURE_CLOUD = "AzureUSGovernment"

# Authenticate with PowerShell
Connect-AzAccount -Environment AzureUSGovernment

# Start the MCP server (will use AZURE_CLOUD env var)
azmcp server start
```

## Troubleshooting

### Authentication Failures

If you encounter authentication failures:

1. **Verify the cloud configuration** - Ensure the cloud name or authority host is correct
2. **Check local authentication** - Make sure you're authenticated with the correct cloud using Azure CLI, PowerShell, or other tools
3. **Verify tenant** - Confirm you're using the correct tenant ID for the sovereign cloud
4. **Check network connectivity** - Ensure you can reach the authority host URL

### Common Error Messages

| Error | Likely Cause | Solution |
|-------|--------------|----------|
| "Authentication failed" | Not authenticated locally | Run `az login` or `Connect-AzAccount` with the correct cloud |
| "Cannot connect to authority host" | Invalid authority host URL | Verify the URL is correct and accessible |
| "Invalid tenant" | Wrong tenant for the cloud | Check your tenant ID matches the cloud environment |

### Verification

To verify your configuration is working:

```bash
# Start the server with verbose logging
azmcp server start --cloud AzureChinaCloud --log-level Debug

# The logs should show which authority host is being used
```

## Additional Resources

- [Azure Sovereign Clouds Overview](https://learn.microsoft.com/azure/active-directory/develop/authentication-national-cloud)
- [Azure CLI Cloud Management](https://learn.microsoft.com/cli/azure/manage-clouds-azure-cli)
- [Azure PowerShell Environments](https://learn.microsoft.com/powershell/azure/authenticate-azureps)
- [Azure Government Documentation](https://learn.microsoft.com/azure/azure-government/)
- [Azure China Documentation](https://learn.microsoft.com/azure/china/)
