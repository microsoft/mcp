# Azure MCP Server Configuration

Auth Configuration in Azure MCP Server.

## Configuration Model

- **InboundAuthentication**: Validates incoming HTTP requests
- **OutboundAuthentication**: Provides credentials for Azure SDK calls

## Examples

### Default (No Authentication)
Suitable for local development with stdio mode. Uses ambient Azure credentials (Azure CLI, Managed Identity).

```json
{
  "inboundAuthentication": {
    "type": "None"
  },
  "outboundAuthentication": {
    "type": "Default"
  }
}
```

### Managed Identity (HTTP mode)

#### Without inbound authentication:
```json
{
  "inboundAuthentication": {
    "type": "None"
  },
  "outboundAuthentication": {
    "type": "ManagedIdentity"
  }
}
```

#### With Entra ID request validation:
```json
{
  "inboundAuthentication": {
    "type": "EntraIDAccessToken",
    "azureAd": {
      "instance": "https://login.microsoftonline.com/",
      "tenantId": "70a036f6-8e4d-4615-bad6-149c02e7720d",
      "clientId": "85a0b190-f927-4e27-b286-cd301d965e4a",
      "audience": "85a0b190-f927-4e27-b286-cd301d965e4a"
    }
  },
  "outboundAuthentication": {
    "type": "ManagedIdentity"
  }
}
```

### Bearer Token (HTTP mode)

#### Without inbound authentication:
```json
{
  "inboundAuthentication": {
    "type": "None"
  },
  "outboundAuthentication": {
    "type": "BearerToken",
    "headerName": "X-Azure-Token"
  }
}
```

#### With Entra ID request validation:
```json
{
  "inboundAuthentication": {
    "type": "EntraIDAccessToken",
    "azureAd": {
      "instance": "https://login.microsoftonline.com/",
      "tenantId": "70a036f6-8e4d-4615-bad6-149c02e7720d",
      "clientId": "85a0b190-f927-4e27-b286-cd301d965e4a",
      "audience": "85a0b190-f927-4e27-b286-cd301d965e4a"
    }
  },
  "outboundAuthentication": {
    "type": "BearerToken",
    "headerName": "X-Azure-Token"
  }
}
```

### On-Behalf-Of Flow (HTTP mode)
Validates incoming token and exchanges it for Azure token via OBO flow.

```json
{
  "inboundAuthentication": {
    "type": "EntraIDAccessToken",
    "azureAd": {
      "instance": "https://login.microsoftonline.com/",
      "tenantId": "70a036f6-8e4d-4615-bad6-149c02e7720d",
      "clientId": "85a0b190-f927-4e27-b286-cd301d965e4a",
      "audience": "85a0b190-f927-4e27-b286-cd301d965e4a"
    }
  },
  "outboundAuthentication": {
    "type": "OnBehalfOf",
    "azureAd": {
      "instance": "https://login.microsoftonline.com/",
      "tenantId": "70a036f6-8e4d-4615-bad6-149c02e7720d",
      "clientId": "85a0b190-f927-4e27-b286-cd301d965e4a",
      "audience": "85a0b190-f927-4e27-b286-cd301d965e4a",
      "clientSecret": "your-client-secret"
    }
  }
}
```

**Note**: For OBO flow, the `instance`, `tenantId`, `clientId`, and `audience` in `outboundAuthentication.azureAd` must match `inboundAuthentication.azureAd`. Only `clientSecret` is unique to outbound configuration.

## Validation Rules (validated at config load time)

- **Default** outbound requires **None** inbound
- **OnBehalfOf** outbound requires **EntraIDAccessToken** inbound and matching Azure AD properties
- **ManagedIdentity** and **BearerToken** support both **None** and **EntraIDAccessToken** inbound