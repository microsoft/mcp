---
title: Lakehouse definition
description: Learn how to create a Lakehouse item definition when using the Microsoft Fabric REST API.
ms.title: Lakehouse item definition
author: anirudha-patro
ms.author: anpatro
ms.service: fabric
ms.date: 11/02/2024
---

# Lakehouse definition
  
This article provides a breakdown of the definition structure for Lakehouse items using `LakehouseDefinitionV1` format. For detailed information, see: [How to create a lakehouse with Microsoft Fabric Rest API](/fabric/data-engineering/lakehouse-api)

## Supported formats

Lakehouse items support the `LakehouseDefinitionV1` format. If no format is specified, it will default to `LakehouseDefinitionV1`.

## Definition parts
  
| Definition part path | Type | Required | Description |
|--|--|--|--|
| `lakehouse.metadata.json` | LakehouseMetadata (JSON) | true | Describes the lakehouse properties |
| `shortcuts.metadata.json` | Shortcut[] (JSON) | false | Describes the onelake shortcuts associated with the lakehouse |
| `data-access-roles.json` | DataAccessRole[] (JSON) | false | Describes the data access roles associated with the lakehouse. This part is only respected if it is enabled through `AlmSettings`. It is disabled by default. |
| `alm.settings.json` | AlmSettings (JSON) | false | Describes the ALM settings associated with the lakehouse |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item |
  
## LakehouseMetadata
  
Describes lakehouse metadata.
  
| Name      | Type            | Required | Description                                     |
|-----------|-----------------|----------|-------------------------------------------------|
| defaultSchema    | String | false     | Current status of the execution.                |

## Shortcut

| Name      | Type    | Required | Description                     |
|-----------|---------|----------|---------------------------------|
| name      | String  | true     | Name of the shortcut. |
| path      | String  | true     | A string representing the fill path where the shortcut is created, including either "Files" or "Tables".        |
| target    | Target  | true     | An object that contains the target datasource, and must specify exactly one of the supported destinations.           |

### Target

An object that contains the target datasource, and must specify exactly one of the supported destinations as described in the table below.

| Name               | Type                    | Required | Description                                                                 |
|--------------------|-------------------------|:--------:|-----------------------------------------------------------------------------|
| adlsGen2           | AdlsGen2                | false    | An object containing the properties of the target ADLS Gen2 data source.    |
| amazonS3           | AmazonS3                | false    | An object containing the properties of the target Amazon S3 data source.    |
| dataverse          | Dataverse               | false    | An object containing the properties of the target Dataverse data source.    |
| googleCloudStorage | GoogleCloudStorage      | false    | An object containing the properties of the target Google Cloud Storage data source. |
| oneLake            | OneLake                 | false    | An object containing the properties of the target OneLake data source.      |
| s3Compatible       | S3Compatible            | false    | An object containing the properties of the target S3 compatible data source.|
| type               | Type                    | true     | The type object contains properties like target shortcut account type. Additional types may be added over time. |

### Type (Enum)

The type object contains properties like target shortcut account type. Additional types may be added over time.

| Value              | Description          |
|--------------------|----------------------|
| OneLake            | OneLake              |
| AmazonS3           | AmazonS3             |
| AdlsGen2           | AdlsGen2             |
| GoogleCloudStorage | GoogleCloudStorage   |
| S3Compatible       | S3Compatible         |
| Dataverse          | Dataverse            |

### OneLake

An object containing the properties of the target OneLake data source.

| Name         | Type            | Required | Description                                                                 |
|--------------|-----------------|:--------:|-----------------------------------------------------------------------------|
| itemId       | string (uuid)   | true     | The ID of the target in OneLake. The target can be an item of Lakehouse, KQLDatabase, or Warehouse. For self-references to the current lakehouse being created/updated, use empty GUIDs. Empty GUIDs are mapped to the current lakehouse during creation/updates.  |
| path         | string          | true     | A string representing the full path to the target folder within the Item. This path should be relative to the root of the OneLake directory structure. For example: "Tables/myTablesFolder/someTableSubFolder". |
| workspaceId  | string (uuid)   | true     | The ID of the target workspace. For referring to the current workspace, use empty GUIDs. Empty GUIDs are mapped to the current workspace during creation/updates. |

### AdlsGen2

An object containing the properties of the target ADLS Gen2 data source.

| Name         | Type          | Required | Description                                                                 |
|--------------|---------------|:--------:|-----------------------------------------------------------------------------|
| connectionId | string (uuid) | true     | A string representing the connection that is bound with the shortcut. The connectionId is a unique identifier used to establish a connection between the shortcut and the target datasource. To find this connection ID, first create a cloud connection to be used by the shortcut when connecting to the ADLS data location. Open the cloud connection's Settings view and copy the connection ID; this is a GUID. |
| location     | string (uri)  | true     | Specifies the location of the target ADLS container. The URI must be in the format https://[account-name].dfs.core.windows.net where [account-name] is the name of the target ADLS account. |
| subpath      | string        | true     | Specifies the container and subfolder within the ADLS account where the target folder is located. Must be of the format [container]/[subfolder] where [container] is the name of the container that holds the files and folders; [subfolder] is the name of the subfolder within the container (optional). For example: /mycontainer/mysubfolder |

### AmazonS3

An object containing the properties of the target Amazon S3 data source.

| Name         | Type          | Required | Description                                                                 |
|--------------|---------------|:--------:|-----------------------------------------------------------------------------|
| connectionId | string (uuid) | true     | A string representing the connection that is bound with the shortcut. The connectionId is a unique identifier used to establish a connection between the shortcut and the target datasource. To find this connection ID, first create a cloud connection to be used by the shortcut when connecting to the Amazon S3 data location. Open the cloud connection's Settings view and copy the connection ID; this is a GUID. |
| location     | string (uri)  | true     | HTTP URL that points to the target bucket in S3. The URL should be in the format https://[bucket-name].s3.[region-code].amazonaws.com, where "bucket-name" is the name of the S3 bucket you want to point to, and "region-code" is the code for the region where the bucket is located. For example: <https://my-s3-bucket.s3.us-west-2.amazonaws.com> |
| subpath      | string        | true     | Specifies a target folder or subfolder within the S3 bucket. |

### Dataverse

An object containing the properties of the target Dataverse data source.

| Name            | Type           | Required | Description                                                                 |
|-----------------|----------------|:--------:|-----------------------------------------------------------------------------|
| connectionId    | string (uuid)  | true     | A string representing the connection that is bound with the shortcut. The connectionId is a unique identifier used to establish a connection between the shortcut and the target datasource. To find this connection ID, first create a cloud connection to be used by the shortcut when connecting to the Dataverse data location. Open the cloud connection's Settings view and copy the connection ID; this is a GUID. |
| deltaLakeFolder | string         | true     | Specifies the DeltaLake folder path where the target data is stored. |
| environmentDomain | string (uri) | true     | URI that indicates the Dataverse target environment's domain name. The URI should be formatted as `https://[orgname].crm[xx].dynamics.com`, where `[orgname]` represents the name of your Dataverse organization. |
| tableName       | string         | true     | Specifies the name of the target table in Dataverse. |

### S3Compatible

An object containing the properties of the target S3 compatible data source.

| Name         | Type          | Required | Description                                                                 |
|--------------|---------------|:--------:|-----------------------------------------------------------------------------|
| bucket       | string        | true     | Specifies the target bucket within the S3 compatible location. |
| connectionId | string (uuid) | true     | A string representing the connection that is bound with the shortcut. The connectionId is a unique identifier used to establish a connection between the shortcut and the target datasource. |
| location     | string (uri)  | true     | HTTP URL of the S3 compatible endpoint. This endpoint must be able to receive ListBuckets S3 API calls. The URL must be in the non-bucket specific format; no bucket should be specified here. For example: `https://s3endpoint.contoso.com` |
| subpath      | string        | true     | Specifies a target folder or subfolder within the S3 compatible bucket. For example: `/folder` |

## DataAccessRole

| Name | Type | Description |
| --- | --- | --- |
| decisionRules | DecisionRule[] | The array of permission rules that make up the data access role. |
| id | string (uuid) | The unique id for the data access role. |
| members | Members | The members object which contains the members of the role as arrays of different member types. |
| name | string | The name of the data access role. |

### DecisionRule

Specifies a rule for matching the requested action. Contains effect (Permit) and permission which determine whether a user or entity is authorized to perform a specific action on a resource.

| Name | Type | Description |
| --- | --- | --- |
| constraints | Constraints | Any constraints such as row or column level security that are applied to tables as part of this role. |
| effect | Effect | The effect that a role has on access to the data resource. Currently the only supported value is `Permit`. |
| permission | PermissionScope[] | The `permission` property is an array that specifies the scope and level of access for a requested action. The array must contain exactly two PermissionScope objects: `Path` and `Action`. |

### PermissionScope

Defines a set of attributes that determine the scope and level of access to a resource.

| Name | Type | Description |
| --- | --- | --- |
| attributeName | AttributeName | Specifies the name of the attribute that is being evaluated for access permissions (Path or Action). |
| attributeValueIncludedIn | string[] | Specifies a list of values for the `attributeName` to define the scope and the level of access to a resource. |

### Constraints

Any constraints such as row or column level security that are applied to tables as part of this role.

| Name | Type | Description |
| --- | --- | --- |
| columns | ColumnConstraint[] | The array of column constraints applied to one or more tables in the data access role. |
| rows | RowConstraint[] | The array of row constraints applied to one or more tables in the data access role. |

### ColumnConstraint

Indicates a constraint that determines the permissions and visibility a user has on columns within a table.

| Name | Type | Description |
| --- | --- | --- |
| columnAction | ColumnAction[] | The array of actions applied to the columnNames. The allowed value is `Read`. |
| columnEffect | ColumnEffect | The effect given to the columnNames. The only allowed value is `Permit`. |
| columnNames | string[] | An array of case sensitive column names. Use `*` to indicate all columns in the table. |
| tablePath | string | A relative file path specifying which table the column constraint applies to (e.g. `/Tables/{optionalSchema}/{tableName}`). |

### RowConstraint

Indicates a constraint that determines the rows in a table that users can see. Roles defined with RowConstraints use T-SQL to define a predicate that filters data in a table.

| Name | Type | Description |
| --- | --- | --- |
| tablePath | string | A relative file path specifying which table the row constraint applies to. |
| value | string | A T-SQL expression that is used to evaluate which rows the role members can see. |

### Members

The members object which contains the members of the role as arrays of different member types.

In addition to FabricItemMembers, Onelake security supports adding a list of [MicrosoftEntraMembers](/rest/api/fabric/core/onelake-data-access-security/create-or-update-data-access-roles?tabs=HTTP#microsoftentramember). This is not supported in lakehouse item definition APIs, and lakehouse CICD and will be ignored if specified.

| Name | Type | Description |
| --- | --- | --- |
| fabricItemMembers | FabricItemMember[] | A list of members who have a certain permission set in Microsoft Fabric. |

### FabricItemMember

| Name | Type | Description |
| --- | --- | --- |
| itemAccess | ItemAccess[] | A list specifying the access permissions for Fabric users to be automatically included in the role members. |
| sourcePath | string | The path to the Fabric item having the specified item access (format: `{workspaceId}/{itemId}`). This is always referencing the same lakehouse and only supports empty GUIDs (00000000-0000-0000-0000-000000000000/00000000-0000-0000-0000-000000000000) |

### Enumerations

#### AttributeName

| Value | Description |
| --- | --- |
| Path | Attribute name Path |
| Action | Attribute name Action |

#### ColumnAction

| Value | Description |
| --- | --- |
| Read | The ColumnAction value Read |

#### ColumnEffect

| Value | Description |
| --- | --- |
| Permit | The ColumnEffect type Permit |

#### Effect

| Value | Description |
| --- | --- |
| Permit | The effect type Permit |

## ALMSettings

Describes the object type settings for Lakehouse Deployment pipelines and git operations. This can be used to enable or disable git integration for object types.

| Name | Type | Required | Description |
| --- | --- | ---:| --- |
| version | String | true | The ALM settings version (Only supported version is "1.0.1"). |
| objectTypes | Object[] | true | A list of object types controlled by ALM settings. Each entry contains a `name`, `state`, and optional `subObjectTypes`. |

### ObjectType

Object types that can be tracked for ALM Operations.

| Name | Type | Required | Description |
| --- | --- | ---:| --- |
| name | String | true | The object type name. Currently supported object types are `Shortcuts`, `DataAccessRoles`. `Shortcuts` are enabled by default and `DataAccessRoles` are disabled by default. |
| state | String | true | The enabled state for the object type (`Enabled` or `Disabled`). |
| subObjectTypes | Object[] | false | Optional list of sub-object types with their own `name` and `state`. Only supported for `Shortcuts` |

#### SubObjectType

Sub object types that can be tracked for ALM operations. Supported for `Shortcuts` object type.

| Name | Type | Required | Description |
| --- | --- | ---:| --- |
| name | String | true | The sub-object type name (for example: `Shortcuts.OneLake`). See example for full list of supported values. |
| state | String | true | The enabled state for the sub-object type (`Enabled` or `Disabled`). |

### LakehouseMetadata Example

```JSON
{
  "defaultSchema": "dbo"
}
```

### Shortcuts Example

```json
[
  {
      "name": "TestShortcut",
      "path": "/Tables/dbo",
      "target": {
        "type": "OneLake",
        "oneLake": {
          "path": "Tables/dbo/publicholidays",
          "itemId": "00000000-0000-0000-0000-000000000000",
          "workspaceId": "00000000-0000-0000-0000-000000000000"
      }
    }
  }
]
```

### DataAccessRoles Example

```json
[
    {
        "name": "dimensionrulerename",
        "kind": "Policy",
        "decisionRules": [
          {
            "effect": "Permit",
            "permission": [
              {
                "attributeName": "Path",
                "attributeValueIncludedIn": [
                  "/Tables/dbo/dimension_city",
                  "/Tables/dbo/dimension_customer",
                  "/Tables/dbo/dimension_date",
                  "/Tables/dbo/dimension_employee",
                  "/Tables/dbo/dimension_stock_item",
                  "/Tables/bronze"
                ]
              },
              {
                "attributeName": "Action",
                "attributeValueIncludedIn": [
                  "Read"
                ]
              }
            ],
            "constraints": {}
          }
        ],
        "members": {
          "fabricItemMembers": [],
            "microsoftEntraMembers": [
            {
              "tenantId": "1e85000e-ee35-4112-bd46-2f00b3d94154",
              "objectId": "5f3efd37-a6d2-432f-99fa-6adc2848a40a"
            }
          ]
        }
      },
      {
        "name": "DefaultReader",
        "kind": "Policy",
        "decisionRules": [
          {
            "effect": "Permit",
            "permission": [
              {
                "attributeName": "Action",
                "attributeValueIncludedIn": [
                  "Read"
                ]
              },
              {
                "attributeName": "Path",
                "attributeValueIncludedIn": [
                  "*"
                ]
              }
            ]
          }
        ],
        "members": {
          "fabricItemMembers": [
            {
              "itemAccess": [
                "ReadAll"
              ],
              "sourcePath": "00000000-0000-0000-0000-000000000000/00000000-0000-0000-0000-000000000000"
            }
          ]
        }
      }
    ]
```

### ALMSettings Example

```json
{
  "version": "1.0.1",
  "objectTypes": [
    {
      "name": "Shortcuts",
      "state": "Enabled",
      "subObjectTypes": [
        {
          "name": "Shortcuts.OneLake",
          "state": "Enabled"
        },
        {
          "name": "Shortcuts.AdlsGen2",
          "state": "Enabled"
        },
        {
          "name": "Shortcuts.Dataverse",
          "state": "Enabled"
        },
        {
          "name": "Shortcuts.AmazonS3",
          "state": "Enabled"
        },
        {
          "name": "Shortcuts.S3Compatible",
          "state": "Enabled"
        },
        {
          "name": "Shortcuts.GoogleCloudStorage",
          "state": "Enabled"
        }
      ]
    },
    {
      "name": "DataAccessRoles",
      "state": "Enabled"
    }
  ]
}
```
