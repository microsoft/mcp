# Azure Device Registry MCP Tool

This tool provides capabilities to interact with Azure Device Registry Service through the Model Context Protocol (MCP).

## Features

### Namespace Management
- **List Namespaces**: List all Device Registry namespaces in a subscription or resource group

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

## Installation

This tool is included as part of the Azure MCP Server. No separate installation is required.

## Requirements

- Azure subscription with Device Registry resources
- Appropriate Azure RBAC permissions to list Device Registry namespaces

## Related Documentation

- [Azure Device Registry Documentation](https://learn.microsoft.com/en-us/azure/iot-operations/discover-manage-assets/overview-manage-assets)
- [Azure Device Registry REST API](https://learn.microsoft.com/en-us/rest/api/deviceregistry/)
