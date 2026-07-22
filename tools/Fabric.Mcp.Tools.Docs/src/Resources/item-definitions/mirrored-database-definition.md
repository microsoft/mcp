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
| `AzureMySql` | Represents an Azure Database for MySQL source. |
| `AzurePostgreSql` | Represents an Azure Database for PostgreSQL source. |
| `AzureSqlDatabase` | Represents an Azure SQL Database source. |
| `AzureSqlMI` | Represents an Azure SQL Managed Instance source. |
| `CosmosDb` | Represents a Cosmos DB source. |
| `GenericMirror` | Represents an open mirroring source. |
| `GoogleBigQuery` | Represents a Google BigQuery source. |
| `MSSQL` | Represents a Microsoft SQL Server 2016-2022 source. |
| `Oracle` | Represents an Oracle source. |
| `SAP` | Represents an SAP source. |
| `SharePointList` | Represents a SharePoint List source. |
| `Snowflake` | Represents a Snowflake source. |
| `SqlServer2025` | Represents a SQL Server 2025 source. |

### SourceTypeProperties

Describes the source type properties.

| Name | Type | Required | Description |
|--|--|--|--|
| `connection` | Guid | false | The connection identifier for the source database. Not required for `GenericMirror` or `SAP` source types. |
| `database` | String | false | The name of the source database. Required only for `Snowflake` and `CosmosDb` source types. |
| `externalStorages` | [ExternalStorageProperties[]](#externalstorageproperties) | false | The external storage configuration for Snowflake sources, such as Amazon S3. |
| `subType` | String | false | The SAP subtype for the source. For example, `Datasphere`. |
| `landingZone` | [LandingZoneProperties](#landingzoneproperties) | false | The landing zone configuration for SAP mirror sources. |

### ExternalStorageProperties

Describes an external storage configuration for a Snowflake mirror source.

| Name | Type | Required | Description |
|--|--|--|--|
| `type` | String | true | The external storage type. For example, `AmazonS3`. |
| `typeProperties` | [ExternalStorageTypeProperties](#externalstoragetypeproperties) | true | Properties for the external storage connection. |

### ExternalStorageTypeProperties

Describes the external storage connection properties.

| Name | Type | Required | Description |
|--|--|--|--|
| `connection` | Guid | true | The connection identifier for the external storage. |

### LandingZoneProperties

Describes the landing zone configuration for an SAP mirror source.

| Name | Type | Required | Description |
|--|--|--|--|
| `type` | String | true | The landing zone type. For example, `Lakehouse`. |
| `typeProperties` | [LandingZoneTypeProperties](#landingzonetypeproperties) | true | Properties for the landing zone connection. |

### LandingZoneTypeProperties

Describes the landing zone connection properties.

| Name | Type | Required | Description |
|--|--|--|--|
| `connection` | Guid | true | The connection identifier for the landing zone. |
| `workspaceId` | Guid | true | The Fabric workspace identifier for the landing zone. |
| `artifactId` | Guid | true | The Fabric item identifier for the landing zone. |
| `rootFolder` | String | true | The root folder path within the landing zone. |

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
| `enableDeltaChangeDataFeed` | Boolean | false | Enables the Delta Change Data Feed for the mirrored database when set to `true`. |
| `format` | String | true | The format for the target (currently only `Delta` is supported). |
| `retentionInDays` | Integer | false | The number of days to retain mirrored data. Supported values are from 1 through 30; the default value is 7. |

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
                "connection": "1d6e9b5e-bdf8-453a-a17e-372c293c7b8a",
                "database": "test",
                "externalStorages": [
                    {
                        "type": "AmazonS3",
                        "typeProperties": {
                            "connection": "71bbebff-149c-4832-920c-7e1aabdabb2a"
                        }
                    }
                ]
            }
        },
        "target": {
            "type": "MountedRelationalDatabase",
            "typeProperties": {
                "defaultSchema": "dbo",
                "format": "Delta",
                "retentionInDays": 1
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

The following example shows the SAP mirror model with `subType` and `landingZone`:

```json
{
  "properties": {
    "source": {
      "type": "SAP",
      "typeProperties": {
        "subType": "Datasphere",
        "landingZone": {
          "type": "Lakehouse",
          "typeProperties": {
            "connection": "11111111-2222-3333-4444-555555555555",
            "workspaceId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
            "artifactId": "ffffffff-0000-1111-2222-333333333333",
            "rootFolder": "Files/test"
          }
        }
      }
    },
    "target": {
      "type": "MountedRelationalDatabase",
      "typeProperties": {
        "defaultSchema": "dbo",
        "format": "Delta"
      }
    }
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

