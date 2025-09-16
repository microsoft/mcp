---
title: Mirrored database item definition
description: Learn how to structure a mirrored database definition when using the Microsoft Fabric REST API.
author: xuyangit1
ms.title: Microsoft Fabric Mirrored database item definition
ms.author: xuyan
ms.service: fabric
ms.date: 10/24/2024
---

# Mirrored database definition

This article provides a breakdown of the definition structure for mirrored database items.

## Definition parts

This table lists the mirrored database definition parts.

| Definition part path | Type | Required | Description |
|--|--|--|--|
| `mirroring.json` | [MirroredDatabase](#mirroreddatabase) | ✅ | Describes the mirrored database item |
| `.platform` | PlatformDetails | ❌ | Describes the metadata of the item |

Definition part of a mirrored database item is constructed as follows:

* **Path**: The file name, for example: `mirroring.json`
* **Payload Type**: InlineBase64
* **Payload**: See [Example of payload content decoded from Base64](#mirroringjson-example)

## MirroredDatabase

Describes the mirrored database item.

| Name | Type | Required | Description |
|--|--|--|--|
| `source` | [SourceProperties](#sourceproperties) | true | Describes the source type properties. |
| `target` | [TargetProperties](#targetproperties) | true | Describes the target type properties. |
| `mountedTables` | [MountedTable[]](#mountedtable) | false | Lists the tables to be mirrored from the source database. (If this property is not specified, all tables will be mirrored. The new tables will also be automatically added to replication.) |

### SourceProperties

Describes the source database to be mirrored.

| Name | Type | Required | Description |
|--|--|--|--|
| `type` | [SourceType](#sourcetype) | true | The type of the source database. |
| `typeProperties` | [SourceTypeProperties](#sourcetypeproperties) | true | Properties for the source connection, such as `connection`, `database` and etc. |

### SourceType

Latest values for the source type (Additional source types may be added over time.):

| Name | Description |
|--|--|
| `Snowflake` | Represents a Snowflake source. |
| `AzureSqlDatabase` | Represents an Azure SQL Database source. |
| `AzureSqlMI` | Represents an Azure SQL Managed Instance source. |
| `AzurePostgreSql` | Represents an Azure PostgreSQL source. |
| `CosmosDb` | Represents a Cosmos DB source. |
| `SqlServer2025` | Represents a SQL Server 2025 source. |
| `MSSQL` | Represents a Microsoft SQL Server 2016-2022 source. |
| `GenericMirror` | Represents an open mirroring source. |

### SourceTypeProperties

Describes the source type properties.

| Name | Type | Required | Description |
|--|--|--|--|
| `connection` | Guid | false | The connection identifier for the source database. Not required for `GenericMirror` source type. |
| `database` | String | false | The name of the source database. Not required for `GenericMirror`, `AzureSqlDatabase`, `AzureSqlMI`, or `AzurePostgreSql` source types. |

### TargetProperties

Describes the target type properties.

| Name | Type | Required | Description |
|--|--|--|--|
| `type` | String | true | The type of the target (currently only `MountedRelationalDatabase` is supported). |
| `typeProperties` | [TargetTypeProperties](#targettypeproperties) | true | Properties for the target, such as `defaultSchema` and `format`. |

### TargetTypeProperties

Describes the properties for the target.

| Name | Type | Required | Description |
|--|--|--|--|
| `defaultSchema` | String | false | The default schema for the target. |
| `format` | String | true | The format for the target (currently only `Delta` is supported). |

### MountedTable

Describes a table to be mirrored from the source database.

| Name | Type | Required | Description |
|--|--|--|--|
| `source` | [MountedTableSourceProperties](#mountedtablesourceproperties) | true | Properties for the source table, such as `schemaName` and `tableName`. |

### MountedTableSourceProperties

Describes the properties for the source table.

| Name | Type | Required | Description |
|--|--|--|--|
| `typeProperties` | [MountedTableSourceTypeProperties](#mountedtablesourcetypeproperties) | true | Type properties for the source table. |

### MountedTableSourceTypeProperties

Describes the type properties for the source table.

| Name | Type | Required | Description |
|--|--|--|--|
| `schemaName` | String | true | The schema name of the source table. |
| `tableName` | String | true | The table name of the source table. |

### `mirroring.json` example

To see how to create a JSON file describing a mirrored database item for various sources, see [mirrored database definitions for various sources](https://learn.microsoft.com/fabric/database/mirrored-database/mirrored-database-rest-api#create-mirrored-database).

```json
{
    "properties": {
        "source": {
            "type": "Snowflake",
            "typeProperties": {
                "connection": "a0a0a0a0-bbbb-cccc-dddd-e1e1e1e1e1e1",
                "database": "TESTDB"
            }
        },
        "target": {
            "type": "MountedRelationalDatabase",
            "typeProperties": {
                "defaultSchema": "dbo",
                "format": "Delta"
            }
        },
        "mountedTables": [
            {
                "source": {
                    "typeProperties": {
                        "schemaName": "dbo",
                        "tableName": "testtable"
                    }
                }
            }
        ]
    }
}
```

## Definition example

Here's an example of a Base64-encoded mirrored database definition, where the content from [`mirroring.json` example](#mirroringjson-example) is encoded in Base64 and placed in the `payload` field with the path set to `mirroring.json`:

```json
{
  "displayName": "myMirroredDatabase",
  "type": "MirroredDatabase",
  "description": "Create Mirrored Database item with definition",
  "definition": {
    "parts": [
      {
        "path": "mirroring.json",
        "payload": "<base64 encoded string>",
        "payloadType": "InlineBase64"
      },
      {
        "path": ".platform",
        "payload": "<base64 encoded string>",
        "payloadType": "InlineBase64"
      }
    ]
  }
}
```

