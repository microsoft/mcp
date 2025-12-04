# Azure Device Registry MCP Tool

This tool provides capabilities to interact with Azure Device Registry Service through the Model Context Protocol (MCP).

## Features

### Namespace Management
- **List Namespaces**: List all Device Registry namespaces in a subscription or resource group
- **Get Namespace**: Get detailed information about a specific Device Registry namespace
- **Create Namespace**: Create a new Device Registry namespace in a resource group

## Commands

### namespace list
Lists all Device Registry namespaces in a subscription or within a specific resource group.

**Parameters:**
- `--subscription` (required): Azure subscription ID
- `--resource-group` (optional): Resource group name to filter namespaces
- `--tenant` (optional): Azure tenant ID

**Example:**
```bash
# List all namespaces in a subscription
azure-mcp deviceregistry namespace list --subscription <subscription-id>

# List namespaces in a specific resource group
azure-mcp deviceregistry namespace list --subscription <subscription-id> --resource-group <resource-group-name>
```

**Returns:**
```json
{
  "namespaces": [
    {
      "name": "namespace-name",
      "id": "/subscriptions/.../resourceGroups/.../providers/Microsoft.DeviceRegistry/namespaces/...",
      "location": "eastus",
      "provisioningState": "Succeeded",
      "resourceUid": "..."
    }
  ]
}
```

### namespace get
Gets detailed information about a specific Device Registry namespace.

**Parameters:**
- `--subscription` (required): Azure subscription ID
- `--resource-group` (required): Resource group name where the namespace exists
- `--name` (required): Name of the Device Registry namespace to retrieve
- `--tenant` (optional): Azure tenant ID

**Example:**
```bash
# Get a specific namespace
azure-mcp deviceregistry namespace get --subscription <subscription-id> --resource-group <resource-group-name> --name <namespace-name>
```

**Returns:**
```json
{
  "namespace": {
    "name": "namespace-name",
    "id": "/subscriptions/.../resourceGroups/.../providers/Microsoft.DeviceRegistry/namespaces/...",
    "location": "eastus",
    "provisioningState": "Succeeded",
    "uuid": "...",
    "tags": {
      "environment": "production",
      "team": "iot"
    }
  }
}
```

### namespace create
Creates a new Device Registry namespace in the specified resource group.

**Parameters:**
- `--subscription` (required): Azure subscription ID
- `--resource-group` (required): Resource group name where the namespace will be created
- `--name` (required): Name of the Device Registry namespace to create
- `--location` (required): Azure region where the namespace will be created (e.g., 'eastus', 'westus2')
- `--tags` (optional): Tags for the namespace in key=value format. Can be specified multiple times
- `--enable-system-assigned-identity` (optional): Enable system-assigned managed identity for the namespace
- `--tenant` (optional): Azure tenant ID

**Example:**
```bash
# Create a namespace with basic parameters
azure-mcp deviceregistry namespace create --subscription <subscription-id> --resource-group <resource-group-name> --name <namespace-name> --location eastus

# Create a namespace with tags
azure-mcp deviceregistry namespace create --subscription <subscription-id> --resource-group <resource-group-name> --name <namespace-name> --location eastus --tags environment=production team=iot

# Create a namespace with system-assigned managed identity
azure-mcp deviceregistry namespace create --subscription <subscription-id> --resource-group <resource-group-name> --name <namespace-name> --location eastus --enable-system-assigned-identity
```

**Returns:**
```json
{
  "namespace": {
    "name": "namespace-name",
    "id": "/subscriptions/.../resourceGroups/.../providers/Microsoft.DeviceRegistry/namespaces/...",
    "location": "eastus",
    "provisioningState": "Succeeded"
  }
}
```

## Installation

This tool is included as part of the Azure MCP Server. No separate installation is required.

## Requirements

- Azure subscription with Device Registry resources
- Appropriate Azure RBAC permissions to:
  - List Device Registry namespaces (for `namespace list` command)
  - Get Device Registry namespace details (for `namespace get` command)
  - Create Device Registry namespaces (for `namespace create` command)

## Related Documentation

- [Azure Device Registry Documentation](https://learn.microsoft.com/en-us/azure/iot-operations/discover-manage-assets/overview-manage-assets)
- [Azure Device Registry REST API](https://learn.microsoft.com/en-us/rest/api/deviceregistry/)
