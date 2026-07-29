# Mirrored Catalog definition

This article provides a breakdown of the definition structure for Mirrored Catalog items. A Mirrored Catalog mirrors tables from an external Iceberg REST Catalog (IRC) API-compliant provider into Fabric by creating delegated shortcuts to provider storage. Metadata is mirrored; data is not copied.

## Supported formats

Mirrored Catalog items support the following format:

- **MirroredCatalogDefinition**

## Definition parts

This table lists the Mirrored Catalog definition parts.

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `MirroredCatalogDefinition.json` | [MirroredCatalogDefinition](#mirroredcatalogdefinition-properties) (JSON) | true | The Mirrored Catalog configuration including source connection, scope, and optional table filters. |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item. |

## Definition example

Here is an example of an item definition for Mirrored Catalog:

```json
{
  "displayName": "myMirroredCatalog",
  "type": "MirroredCatalog",
  "description": "Mirror selected schemas and tables",
  "definition": {
    "format": "MirroredCatalogDefinition",
    "parts": [
      {
        "path": "MirroredCatalogDefinition.json",
        "payload": "eyIkc2NoZW1hIjoiaHR0cHM6Ly9kZXZlbG9wZXIubWljcm9zb2Z0LmNvbS9qc29uLXNjaGVtYXMvZmFicmljL2l0ZW0vbWlycm9yZWRDYXRhbG9nL2RlZmluaXRpb24vbWlycm9yZWRDYXRhbG9nRGVmaW5pdGlvbi8xLjAuMC9zY2hlbWEuanNvbiIsInByb3BlcnRpZXMiOnsic291cmNlIjp7InR5cGUiOiJEcmVtaW9JY2ViZXJnQ2F0YWxvZyIsInR5cGVQcm9wZXJ0aWVzIjp7ImNvbm5lY3Rpb25JZCI6IjRlYjZiNzY3LWU3ODYtNDVlZC1iN2NmLWQyNTAyM2U1MjIyMiIsInNjb3BlIjpbIkFjY291bnRpbmciLCJVUyJdfX0sIm1vdW50ZWRUYWJsZXMiOlt7InNvdXJjZSI6eyJ0eXBlUHJvcGVydGllcyI6eyJmdWxseVF1YWxpZmllZFNjb3BlIjpbIkFjY291bnRpbmciLCJVUyIsIlRheGVzIl0sInRhYmxlTmFtZSI6IlRheEluZm8ifX19LHsic291cmNlIjp7InR5cGVQcm9wZXJ0aWVzIjp7ImZ1bGx5UXVhbGlmaWVkU2NvcGUiOlsiQWNjb3VudGluZyIsIlVTIiwiUmVjZWl2YWJsZSJdLCJ0YWJsZU5hbWUiOiJJbnZvaWNlcyJ9fX1dfX0=",
        "payloadType": "InlineBase64"
      },
      {
        "path": ".platform",
        "payload": "eyIkc2NoZW1hIjoiaHR0cHM6Ly9kZXZlbG9wZXIubWljcm9zb2Z0LmNvbS9qc29uLXNjaGVtYXMvZmFicmljL2dpdEludGVncmF0aW9uL3BsYXRmb3JtUHJvcGVydGllcy8yLjAuMC9zY2hlbWEuanNvbiIsIm1ldGFkYXRhIjp7ImRpc3BsYXlOYW1lIjoibXlNaXJyb3JlZENhdGFsb2ciLCJkZXNjcmlwdGlvbiI6Ik1pcnJvciBzZWxlY3RlZCBzY2hlbWFzIGFuZCB0YWJsZXMifSwiY29uZmlnIjp7InZlcnNpb24iOiIyLjAiLCJsb2dpY2FsSWQiOiIwMDAwMDAwMC0wMDAwLTAwMDAtMDAwMC0wMDAwMDAwMDAwMDAifX0=",
        "payloadType": "InlineBase64"
      }
    ]
  }
}
```

## MirroredCatalogDefinition properties

The MirroredCatalogDefinition.json file contains the configuration for the Mirrored Catalog item.

| Name | Type | Required | Description |
|---|---|---|---|
| $schema | String | true | A URL identifying the schema used for the Mirrored Catalog definition. |
| properties | [Properties](#properties) | true | The Mirrored Catalog configuration properties. |

### Properties

| Name | Type | Required | Description |
|---|---|---|---|
| source | [SourceProperties](#sourceproperties) | true | Describes the source catalog properties. |
| mountedTables | [MountedTable[]](#mountedtable) | false | Lists the tables to be mirrored from the source catalog. If not specified, all tables under the scope are mirrored and new tables are automatically added. |

### SourceProperties

Describes the source catalog connection and scope configuration.

| Name | Type | Required | Description |
|---|---|---|---|
| type | [SourceType](#sourcetype) | true | The type of the source catalog provider. Additional source types may be added over time. |
| typeProperties | [SourceTypeProperties](#sourcetypeproperties) | true | Properties for the source connection, such as connectionId and scope. |

### SourceType

| Name | Description |
|---|---|
| `DremioIcebergCatalog` | Represents a Dremio Iceberg Catalog source. |

### SourceTypeProperties

Connection and scope properties for the source catalog.

| Name | Type | Required | Description |
|---|---|---|---|
| connectionId | String (UUID) | true | The connection identifier for the source catalog. |
| scope | String[] | true | The namespace hierarchy path that scopes the mirroring. Must be a selectable scope. A selectable scope is a namespace that is a parent of one or more leaf namespaces containing tables. |

### MountedTable

Defines a table to be mirrored from the source catalog.

| Name | Type | Required | Description |
|---|---|---|---|
| source | [MountedTableSourceProperties](#mountedtablesourceproperties) | true | Properties for the source table. |

### MountedTableSourceProperties

Source properties for a mounted table.

| Name | Type | Required | Description |
|---|---|---|---|
| typeProperties | [MountedTableSourceTypeProperties](#mountedtablesourcetypeproperties) | true | Type properties for the source table. |

### MountedTableSourceTypeProperties

Fully qualified scope and table name identifying a table in the source catalog.

| Name | Type | Required | Description |
|---|---|---|---|
| fullyQualifiedScope | String[] | true | The fully qualified namespace hierarchy for this table (for example, `["Accounting", "US", "Taxes"]`). |
| tableName | String | true | The table name of the source table. |

### MirroredCatalogDefinition.json example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/mirroredCatalog/definition/mirroredCatalogDefinition/1.0.0/schema.json",
  "properties": {
    "source": {
      "type": "DremioIcebergCatalog",
      "typeProperties": {
        "connectionId": "4eb6b767-e786-45ed-b7cf-d25023e52222",
        "scope": ["Accounting", "US"]
      }
    },
    "mountedTables": [
      {
        "source": {
          "typeProperties": {
            "fullyQualifiedScope": ["Accounting", "US", "Taxes"],
            "tableName": "TaxInfo"
          }
        }
      },
      {
        "source": {
          "typeProperties": {
            "fullyQualifiedScope": ["Accounting", "US", "Receivable"],
            "tableName": "Invoices"
          }
        }
      }
    ]
  }
}
```
