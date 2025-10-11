# Azure MCP Server Configuration

Authentication configuration for Azure MCP Server.

## Configuration Model

- **InboundAuthentication**: Validates incoming HTTP requests
- **OutboundAuthentication**: Provides credentials for Azure SDK calls

## Examples

### Default (No Authentication)
Running locally, uses ambient Azure credentials (Azure CLI, Managed Identity).

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
    "type": "JwtBearerScheme",
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

### JWT Passthrough (HTTP mode)
Forwards incoming token to Azure APIs without validation.

#### Without inbound authentication:
```json
{
  "inboundAuthentication": {
    "type": "None"
  },
  "outboundAuthentication": {
    "type": "JwtPassthrough"
  }
}
```

#### With Entra ID request validation:
```json
{
  "inboundAuthentication": {
    "type": "JwtBearerScheme",
    "azureAd": {
      "instance": "https://login.microsoftonline.com/",
      "tenantId": "70a036f6-8e4d-4615-bad6-149c02e7720d",
      "clientId": "85a0b190-f927-4e27-b286-cd301d965e4a",
      "audience": "85a0b190-f927-4e27-b286-cd301d965e4a"
    }
  },
  "outboundAuthentication": {
    "type": "JwtPassthrough"
  }
}
```

### On-Behalf-Of Flow (HTTP mode)
Validates incoming token and exchanges it for Azure token using OBO flow.

```json
{
  "inboundAuthentication": {
    "type": "JwtBearerScheme",
    "azureAd": {
      "instance": "https://login.microsoftonline.com/",
      "tenantId": "70a036f6-8e4d-4615-bad6-149c02e7720d",
      "clientId": "85a0b190-f927-4e27-b286-cd301d965e4a",
      "audience": "85a0b190-f927-4e27-b286-cd301d965e4a"
    }
  },
  "outboundAuthentication": {
    "type": "JwtObo",
    "clientCredential": {
      "kind": "ClientSecret",
      "secret": "your-client-secret"
    }
  }
}
```

## Client Credential Types

The `clientCredential` configuration supports multiple credential types:

- **ClientSecret**: Uses a shared secret
- **CertificateLocal**: Uses a local certificate (not yet implemented)
- **CertificateKeyVault**: Uses a certificate from Azure Key Vault (not yet implemented)

## Validation Rules

- **Default** outbound requires **None** inbound
- **JwtObo** outbound requires **JwtBearerScheme** inbound
- **ManagedIdentity** and **JwtPassthrough** support both **None** and **JwtBearerScheme** inbound
- **JwtObo** inherits tenant/client configuration from inbound auth - no duplication needed