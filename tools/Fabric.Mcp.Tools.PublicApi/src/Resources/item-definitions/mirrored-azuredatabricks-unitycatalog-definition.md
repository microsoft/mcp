---
title: Mirrored Azure Databricks Unity Catalog item definition
description: Learn how to structure a mirrored Azure Databricks Unity Catalog definition when using the Microsoft Fabric REST API.
author: nsarang
ms.title: Microsoft Fabric Mirrored Azure Databricks Unity Catalog item definition
ms.author: nsarang
ms.service: fabric
ms.date: 02/06/2025
---
  
# Mirrored Azure Databricks Unity Catalog definition
  
This article provides a breakdown of the definition structure for mirrored Azure Databricks Unity Catalog items.
  
## Definition parts
  
This table lists the definition parts.
  
| Definition part path | type | Required | Description |
|--|--|--|--|
| `mirroringAzureDatabricksCatalog.json` | [ContentDetails](#contentdetails) (JSON) | true  | Describes the mirroring settings of the item |
| `.platform`                            | PlatformDetails (JSON)                   | false | Describes common details of the item |

  
## ContentDetails 
  
Describes the content of the payload.
  
| Name                             | Type                                                                | Description                                                                 |
|----------------------------------|---------------------------------------------------------------------|-----------------------------------------------------------------------------|
| name                             | String                                                              | Azure databricks catalog name.                                              |
| databricksWorkspaceConnectionId  | Guid                                                                | The Azure databricks workspace connection ID.                               |
| autoSync                         | [AutoSync](#autosync)                                               | Describes the sync mode. The allowed values are: `Enabled` and `Disabled`.  |
| mirroringMode                    | [MirroringMode](#mirroringmode)                                     | Describes the mirroring mode with possible values: `Full` and `Partial`.    |
| storageConnectionId              | Guid                                                                | (Optional) The storage connection ID.                                       |
| mirrorConfiguration              | [MirrorConfiguration](#description-for-mirrorconfiguration-contents)| Replicate metadata from the source system. For example, use this setting to mirror a specific schema or a specific table.                                                          |
 
### AutoSync
  
| Name      | Description                           |
|-----------|---------------------------------------|
| Enabled   | Automatic synchronization is enabled. |
| Disabled  | Automatic synchronization is disabled.|

### MirroringMode 
  
| Name      | Description                                                                                                                                                     |
|-----------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Full      |  Replicates all objects within a catalog except explicitly excluded schemas and tables. Defaults to full synchronization if no exclusions are set.              |
| Partial   |  Only selected schemas and tables are mirrored. Nothing is synced by default.                                                                                   |

  
### Description for MirrorConfiguration contents
  
| Name      | Type                                                      | Description                                                                |
|-----------|-----------------------------------------------------------|----------------------------------------------------------------------------|
| schemas   | [Schema](#description-for-schema-contents)[]              | A list of schemas to be mirrored, each containing specific configurations. |
  
### Description for Schema contents
  
| Name          | Type                                                      | Description                                                                          |
|---------------|-----------------------------------------------------------|--------------------------------------------------------------------------------------|
| schemaName    | String                                                    | The name of the schema, relative to the parent catalog.                              |
| mirroringMode | [SchemaMirroringMode](#schemamirroringmode)               | Describes the mirroring mode. Allowed values are: `Full`, `Exclude` and `Partial`.   |
| tables        | [Table](#description-for-table-contents)[]                | List of tables within the schema to be mirrored.                                     |
  
### SchemaMirroringMode 
| Name      | Description                                                                                                                                          |
|-----------|------------------------------------------------------------------------------------------------------------------------------------------------------|
| Full      | Mirror everything inside a schema except specifically excluded tables. Everything is synced by default if no exclusion is provided.                  |
| Partial   | Only selected tables are mirrored.                                                                                                                   |
| Exclude   | Excludes selected schemas from mirroring. This option is available only when the catalog's mirroring mode is set to `Full`.                          |

### Description for Table contents
  
| Name          | Type                                                      | Description                                                                          |
|---------------|-----------------------------------------------------------|--------------------------------------------------------------------------------------|
| tableName     | String                                                    | The name of the table, relative to the parent schema.                                |
| mirroringMode | [TableMirroringMode](#tablemirroringmode)                 | Describes the mirroring mode. Allowed values are: `Full` and `Exclude`.              |

### TableMirroringMode 
  
| Name      | Description                                                                                                                                      |
|-----------|--------------------------------------------------------------------------------------------------------------------------------------------------|
| Full      | Mirror entire table.                                                                                                                             |
| Exclude   | Excludes selected tables from mirroring. This option is available only when the schema's mirroring mode is set to `Full`.                        |

### ContentDetails example 1

Example of partial catalog mirroring, where specific schemas within a catalog are mirrored.

```JSON
{
    "name": "catalogName",
    "databrickWorkspaceConnectionId": "4eb6b767-e786-45ed-b7cf-d25023e52222",
    "autoSync": true,
    "mirroringMode": "Partial",
    "mirrorConfiguration": {
      "schemas": [
        {
          "schemaName": "schema_3",
          "mirroringMode": "Full"
        },
        {
          "schemaName": "schema_2",
          "mirroringMode": "Full"
        }
      ]
    }
}
```


### ContentDetails example 2

Example of partial catalog mirroring, fully mirroring specific schemas while excluding certain tables within them.

```JSON
{
    "name": "catalogName",
    "databrickWorkspaceConnectionId": "4eb6b767-e786-45ed-b7cf-d25023e52222",
    "autoSync": true,
    "mirroringMode": "Partial",
    "mirrorConfiguration": {
      "schemas": [
        {
          "schemaName": "schema_3",
          "mirroringMode": "Full",
          "tables": [
            {
              "tableName": "table_1",
              "mirroringMode": "Exclude"
            }
          ]
        },
        {
          "schemaName": "schema_2",
          "mirroringMode": "Full",
          "tables": [
            {
              "tableName": "table_2",
              "mirroringMode": "Exclude"
            }
          ]
        }
      ]
    }
}
```

### ContentDetails example 3

Example of partial catalog mirroring, where specific tables within a selected schema are mirrored.

```JSON
{
    "name": "catalogName",
    "databrickWorkspaceConnectionId": "4eb6b767-e786-45ed-b7cf-d25023e52222",
    "autoSync": true,
    "mirroringMode": "Partial",
    "mirrorConfiguration": {
      "schemas": [
        {
          "schemaName": "schema_3",
          "mirroringMode": "Partial",
          "tables": [
            {
              "tableName": "table_1",
              "mirroringMode": "Full"
            }
          ]
        },
        {
          "schemaName": "schema_2",
          "mirroringMode": "Partial",
          "tables": [
            {
              "tableName": "table_2",
              "mirroringMode": "Full"
            }
          ]
        }
      ]
    }
}
```

### ContentDetails example 4

Example of full mirroring, where the entire catalog is mirrored.

```JSON
{
    "name": "catalogName",
    "databrickWorkspaceConnectionId": "5eb6b767-e786-45ed-b7ef-d25023e52211",
    "autoSync": true,
    "mirroringMode": "Full"
}
```

### ContentDetails example 5

Example of full catalog mirroring, with specific schemas excluded from the catalog.

```JSON

{
    "name": "catalogName",
    "databrickWorkspaceConnectionId": "5eb6b767-e786-45ed-b7ef-d25023e52211",
    "autoSync": true,
    "mirroringMode": "Full",
    "mirrorConfiguration": {
      "schemas": [
        {
          "schemaName": "schema_3",
          "mirroringMode": "Exclude"
        },
        {
          "schemaName": "schema_2",
          "mirroringMode": "Exclude"
        }
      ]
    }
}
```
### ContentDetails example 6

Example of partial catalog mirroring, where specific tables within a selected schema are mirrored, while tables from other schemas are excluded.

```JSON
{
    "name": "catalogName",
    "databrickWorkspaceConnectionId": "4eb6b767-e786-45ed-b7cf-d25023e52222",
    "autoSync": true,
    "mirroringMode": "Partial",
    "mirrorConfiguration": {
      "schemas": [
        {
          "schemaName": "schema_3",
          "mirroringMode": "Partial",
          "tables": [
            {
              "tableName": "table_1",
              "mirroringMode": "Full"
            }
          ]
        },
        {
          "schemaName": "schema_2",
          "mirroringMode": "Full",
          "tables": [
            {
              "tableName": "table_2",
              "mirroringMode": "Exclude"
            }
          ]
        }
      ]
    }
}
```
