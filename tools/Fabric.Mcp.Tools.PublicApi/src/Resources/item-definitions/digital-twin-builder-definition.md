---
title: Digital twin builder item definition
description: Learn how to create a digital twin builder item definition when using the Microsoft Fabric REST API.
author: vidyashreeb
ms.author: vidyashreeb
ms.title: Digital twin builder item definition
ms.service: fabric
ms.date: 04/29/2025
---

# Digital twin builder definition

This article provides a breakdown of the structure for digital twin builder definition items.

## Supported formats

DigitalTwinBuilder items support the JSON format.

## Definition parts

This table lists the digital twin builder definition parts.

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `definition.json` | DefinitionDetails (JSON) | true | Describes the `LakehouseId` associated with the item. |
| `.platform`| PlatformDetails (JSON) | false | Describes common details of the item. |
| `EntityTypes` | Directory | false | Contains a list of entity type files that are part of the item. Each entity type file (JSON) describes details of the entity type. |
| `EntityTypeRelationships` | Directory | false | Contains a list of entity type relationship files that are part of the item. Each entity type relationship file (JSON) describes details of the entity type relationship. |
| `ContextualizationOperations` | Directory | false | Contains a list of contextualization operation files that are part of the item. Each contextualization operation file (JSON) describes details of the contextualization operation. |
| `MappingOperations` | Directory | false | Contains a list of mapping operation files that are part of the item. Each mapping operation file (JSON) describes details of the mapping operation. |

## Definition example

```json
{
  "parts": [
    {
      "path": "definition.json",
      "payload": "ew0KICAibGFrZWhvdXNlSWQiOiAiYjliNWQzNmYtNDQ0NS00MDNiLWFjODctMDE2YjFjZDIwMjExIg0KfQ==",
      "payloadType": "InlineBase64"
    },
    {
      "path": ".platform",
      "payload": "ZG90UGxhdGZvcm1CYXNlNjRTdHJpbmc=",
      "payloadType": "InlineBase64"
    }
  ]
}
```

### Definition example with optional definition parts

```json
{
  "parts": [
    {
      "path": "definition.json",
      "payload": "eyAKICAiTGFrZWhvdXNlSWQiOiAiMjIzMDE0NGItZjQ4Ni04YjZmLTQ5NmMtM2U4ZTI4NzZhYTBkIiwgICAKfSA=",
      "payloadType": "InlineBase64"
    },
    {
      "path": ".platform",
      "payload": "ZG90UGxhdGZvcm1CYXNlNjRTdHJpbmc=",
      "payloadType": "InlineBase64"
    },
    {
      "path": "EntityTypes/139950578358348.json",
      "payload": "ew0KICAiSWQiOiAiMTM5OTUwNTc4MzU4MzQ4IiwNCiAgIk5hbWVzcGFjZSI6ICJ1c2VydHlwZXMiLA0KICAiQmFzZUVudGl0eVR5cGVJZCI6ICIyIiwNCiAgIk5hbWUiOiAiRXF1aXBtZW50MSIsDQogICJQcm9wZXJ0aWVzIjogWw0KICAgIHsNCiAgICAgICJJZCI6ICI5MTcxODAxMTAzMjkyNjk0NTI4IiwNCiAgICAgICJOYW1lIjogIkRpc3BsYXlOYW1lIiwNCiAgICAgICJWYWx1ZVR5cGUiOiAiU3RyaW5nIg0KICAgIH0sDQogICAgew0KICAgICAgIklkIjogIjkxNzE4MDExMDMyOTI2OTQ1MjkiLA0KICAgICAgIk5hbWUiOiAiU2VyaWFsTnVtYmVyIiwNCiAgICAgICJWYWx1ZVR5cGUiOiAiU3RyaW5nIg0KICAgIH0sDQogICAgew0KICAgICAgIklkIjogIjkxNzE4MDExMDMyOTI2OTQ1MzAiLA0KICAgICAgIk5hbWUiOiAiTWFudWZhY3R1cmVyIiwNCiAgICAgICJWYWx1ZVR5cGUiOiAiU3RyaW5nIg0KICAgIH0NCiAgXSwNCiAgIlRpbWVzZXJpZXNQcm9wZXJ0aWVzIjogW10NCn0=",
      "payloadType": "InlineBase64"
    },
    {
      "path": "EntityTypes/31864156952988.json",
      "payload": "ew0KICAiSWQiOiAiMzE4NjQxNTY5NTI5ODgiLA0KICAiTmFtZXNwYWNlIjogInVzZXJ0eXBlcyIsDQogICJCYXNlRW50aXR5VHlwZUlkIjogIjIiLA0KICAiTmFtZSI6ICJFcXVpcG1lbnQyIiwNCiAgIlByb3BlcnRpZXMiOiBbDQogICAgew0KICAgICAgIklkIjogIjIwODgyNDkzOTAwNzEwMjE1NjgiLA0KICAgICAgIk5hbWUiOiAiRGlzcGxheU5hbWUiLA0KICAgICAgIlZhbHVlVHlwZSI6ICJTdHJpbmciDQogICAgfSwNCiAgICB7DQogICAgICAiSWQiOiAiMjA4ODI0OTM5MDA3MTAyMTU2OSIsDQogICAgICAiTmFtZSI6ICJTZXJpYWxOdW1iZXIiLA0KICAgICAgIlZhbHVlVHlwZSI6ICJTdHJpbmciDQogICAgfSwNCiAgICB7DQogICAgICAiSWQiOiAiMjA4ODI0OTM5MDA3MTAyMTU3MCIsDQogICAgICAiTmFtZSI6ICJNYW51ZmFjdHVyZXIiLA0KICAgICAgIlZhbHVlVHlwZSI6ICJTdHJpbmciDQogICAgfQ0KICBdLA0KICAiVGltZXNlcmllc1Byb3BlcnRpZXMiOiBbXQ0KfQ==",
      "payloadType": "InlineBase64"
    },
    {
      "path": "EntityTypeRelationships/95745415684647936.json",
      "payload": "ew0KICAiSWQiOiAiOTU3NDU0MTU2ODQ2NDc5MzYiLA0KICAiTmFtZXNwYWNlIjogInVzZXJ0eXBlcyIsDQogICJSZWxhdGlvbnNoaXBDYXJkaW5hbGl0eSI6ICJNYW55VG9PbmUiLA0KICAiTmFtZSI6ICJjb250YWlucyIsDQogICJGaXJzdEVudGl0eVR5cGVJZCI6ICIzMTg2NDE1Njk1Mjk4OCIsDQogICJTZWNvbmRFbnRpdHlUeXBlSWQiOiAiMTM5OTUwNTc4MzU4MzQ4Ig0KfQ==",
      "payloadType": "InlineBase64"
    },
    {
      "path": "ContextualizationOperations/30f6380c-9643-4284-a5bd-f100ac08866f.json",
      "payload": "ewogICJPcGVyYXRpb25JZCI6ICIzMGY2MzgwYy05NjQzLTQyODQtYTViZC1mMTAwYWMwODg2NmYiLAogICJEaXNwbGF5TmFtZSI6ICJFcXVpcG1lbnQyX2NvbnRhaW5zX0VxdWlwbWVudDFfQ29udGV4dHVhbGl6YXRpb24iLAogICJPcGVyYXRpb25UeXBlIjogIkNvbnRleHR1YWxpemF0aW9uIiwKICAiRW50aXR5VHlwZVJlbGF0aW9uc2hpcElkIjogIjk1NzQ1NDE1Njg0NjQ3OTM2IiwKICAiSm9pbkNvbHVtbnMiOiB7CiAgICAiRmlyc3RDb2x1bW4iOiB7CiAgICAgICJFbnRpdHlJZCI6ICIzMTg2NDE1Njk1Mjk4OCIsCiAgICAgICJBdHRyaWJ1dGVOYW1lIjogIlNlcmlhbE51bWJlciIKICAgIH0sCiAgICAiU2Vjb25kQ29sdW1uIjogewogICAgICAiRW50aXR5SWQiOiAiMTM5OTUwNTc4MzU4MzQ4IiwKICAgICAgIkF0dHJpYnV0ZU5hbWUiOiAiU2VyaWFsTnVtYmVyIgogICAgfQogIH0KfQ==",
      "payloadType": "InlineBase64"
    },
    {
      "path": "MappingOperations/ce9d0ef9-d8f6-4391-9e37-8bdb91b1fc16.json",
      "payload": "ewogICJPcGVyYXRpb25JZCI6ICJjZTlkMGVmOS1kOGY2LTQzOTEtOWUzNy04YmRiOTFiMWZjMTYiLAogICJEaXNwbGF5TmFtZSI6ICJFcXVpcG1lbnQxX2VudGl0eXR5cGUiLAogICJPcGVyYXRpb25UeXBlIjogIk1hcHBpbmciLAogICJFbnRpdHlUeXBlSWQiOiAiMTM5OTUwNTc4MzU4MzQ4IiwKICAiTWFwcGluZ09wZXJhdGlvblByb3BlcnRpZXMiOiB7CiAgICAiTWFwcGluZ1R5cGUiOiAiTm9uVGltZVNlcmllcyIsCiAgICAiTWFwcGVkUHJvcGVydGllcyI6IFsKICAgICAgewogICAgICAgICJTb3VyY2VDb2x1bW4iOiAiTmFtZSIsCiAgICAgICAgIkVudGl0eVR5cGVQcm9wZXJ0eU5hbWUiOiAiRGlzcGxheU5hbWUiCiAgICAgIH0KICAgIF0sCiAgICAiUHJvY2Vzc2luZ1R5cGUiOiAiSXRlcmF0aXZlIiwKICAgICJFbnRpdHlJbnN0YW5jZUlkU2NoZW1hIjogWwogICAgICAiSWQiCiAgICBdLAogICAgIlRpbWVzZXJpZXNFbnRpdHlMaW5rUHJvcGVydGllcyI6IG51bGwKICB9LAogICJTb3VyY2VUYWJsZVByb3BlcnRpZXMiOiB7CiAgICAiU291cmNlVHlwZSI6ICJMYWtlaG91c2VUYWJsZXMiLAogICAgIldvcmtzcGFjZUlkIjogImFlMzU5YmI4LTJmZmEtNGUzZi1hMGI1LTJmOGJjZWVmNmQyOSIsCiAgICAiSXRlbUlkIjogIjU5ZTViMDk4LWYzZDMtNDViNi1hY2RkLWQ3ZjRjOTZjNmNjZCIsCiAgICAiU291cmNlVGFibGVOYW1lIjogImVudGl0eXR5cGUiLAogICAgIlNvdXJjZVNjaGVtYSI6IG51bGwKICB9LAogICJGaWx0ZXJzIjogbnVsbAp9",
      "payloadType": "InlineBase64"
    }
  ]
}
```

## DefinitionDetails

| Property | Type | Required | Description |
|---|---|---|---|
| `LakehouseId` | Guid  | true | Item ID of the parent lakehouse that exists in the workspace. |

The lakehouse cannot be deleted if DigitalTwinBuilder still exists. 

### Definition file example
  
```json
{ 

  "LakehouseId": "2230144b-f486-8b6f-496c-3e8e2876aa0d"   

} 
```

## PlatformDetails

The platform part is a file that contains the environment metadata information.
* [Create Item](https://learn.microsoft.com/rest/api/fabric/core/items/create-item) with definition respects the platform file if provided
* [Get Item](https://learn.microsoft.com/rest/api/fabric/core/items/get-item) definition always returns the platform file.
* [Update Item](https://learn.microsoft.com/rest/api/fabric/core/items/update-item) definition accepts the platform file if provided, but only if you set a new URL parameter `updateMetadata=true`.

## EntityTypes directory: EntityType file

The EntityType file name is the entity type ID. 

| Property | Type | Required | Description |
|---|---|---|---|
| `Id` | BigInt | true | Unique ID of the entity type. This value is always greater than 10,000. |
| `Namespace` | string | true | Namespace of the entity type. Allowed value: `usertypes`. |
| `BaseEntityTypeId` | BigInt | true | Unique ID of the base entity type. |
| `Name` | string | true  | Name of the entity type. |
| `Properties` | EntityTypeProperty[] | true | List of entity type properties. |
| `TimeseriesProperties` | EntityTypeProperty[] | true | List of entity type properties. |

### EntityTypeProperty

| Property | Type | Required | Description |
|----|---|---|---|
| `Id` | long | true | Unique ID of the entity type property. |
| `Name` | string | true | Name of the entity type property. |
| `ValueType` | string | true | Describes the value type of the entity type property. Allowed values: `BigInt`, `Float`, `Double`, `String`, `Bool`, `DateTime`. |

 
### EntityType file example

```json
{
  "Id": "139950578358348",
  "Namespace": "usertypes",
  "BaseEntityTypeId": "2",
  "Name": "Equipment1",
  "Properties": [
    { "Id": "9171801103292694528", "Name": "DisplayName", "ValueType": "String" },
    { "Id": "9171801103292694529", "Name": "SerialNumber", "ValueType": "String" },
    { "Id": "9171801103292694530", "Name": "Manufacturer", "ValueType": "String" }
  ],
  "TimeseriesProperties": []
}
```

## EntityTypeRelationships directory: EntityTypeRelationship file

The EntityTypeRelationship file name is the entity type relationship ID.

| Property | Type | Required | Description |
|---|---|---|---|
| `Id` | BigInt | true | Unique ID of the entity type relationship. Its value is always greater than 10,000. |
| `Namespace` | string  | true | Namespace of the entity type relationship. Allowed value: `usertypes`. |
| `RelationshipCardinality` | BigInt  | true | The relationship cardinality of the entity type relationship. Allowed values: `ManyToOne`, `OneToMany`. |
| `Name` | string  | true | Name of the entity type relationship. |
| `FirstEntityTypeId` | BigInt  | true | Unique ID of the first entity type that exists in the workspace. |
| `SecondEntityTypeId` | BigInt  | true | Unique ID of the second entity type that exists in the workspace. |

### EntityTypeRelationship file example

```json
{
  "Id": "95745415684647936",
  "Namespace": "usertypes",
  "RelationshipCardinality": "ManyToOne",
  "Name": "contains",
  "FirstEntityTypeId": "31864156952988",
  "SecondEntityTypeId": "139950578358348"
}
```

## ContextualizationOperations directory: ContextualizationOperation file

The ContextualizationOperation file name is the contextualization operation ID. A ContextualizationOperation can be run using a [DigitalTwinBuilderFlow](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/digital-twin-builder-flow-definition) artifact.

| Property | Type | Required | Description |
|---|---|---|---|
| `OperationId` | Guid | true | Unique ID of the contextualization operation. |
| `DisplayName` | string | true | Display name of the contextualization operation. |
| `OperationType` | string | true | Operation type. Allowed value: `Contextualization`. |
| `EntityTypeRelationshipId`  | string  | true | Unique ID of the entity type relationship that exists in the workspace. |
| `JoinColumns` | JoinColumnsConfiguration | true | Join details of entity types. |

### JoinColumnsConfiguration

| Property | Type | Required | Description |
|---|---|---|----|
| `FirstColumn` | JoinColumn | true | Join details of the first entity type that exists in the workspace. |
| `SecondColumn` | JoinColumn | true | Join details of the second entity type that exists in the workspace. |

#### JoinColumn

| Property | Type | Required | Description |
|---|---|---|---|
| `EntityId` | BigInt | true  | Unique ID of the entity type. |
| `AttributeName` | string | true | Attribute name of the entity type. |

### Contextualization Operation file example

```json
{
  "OperationId": "30f6380c-9643-4284-a5bd-f100ac08866f",
  "DisplayName": "Equipment2_contains_Equipment1_Contextualization",
  "OperationType": "Contextualization",
  "EntityTypeRelationshipId": "95745415684647936",
  "JoinColumns": {
    "FirstColumn": {
      "EntityId": "31864156952988",
      "AttributeName": "SerialNumber"
    },
    "SecondColumn": {
      "EntityId": "139950578358348",
      "AttributeName": "SerialNumber"
    }
  }
}
```

## MappingOperations directory: MappingOperation file

The MappingOperation file name is the mapping operation ID. A MappingOperation can be run using a [DigitalTwinBuilderFlow](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/digital-twin-builder-flow-definition) artifact.

| Property | Type | Required | Description |
|---|---|---|---|
| `OperationId`     | Guid  | true | Unique ID of the mapping operation. |
| `DisplayName`     | string     | true | Display name of the mapping operation.   |
| `OperationType`   | string     | true | Operation type. Allowed value: `Mapping`.   |
| `EntityTypeId`    | string     | true | Unique ID of the entity type that exists in the workspace.  |
| `MappingOperationProperties` | MappingOperationProperties | true | Model that contains the properties of a mapping operation.   |
| `SourceTableProperties`   | SourceTableProperties   | true | Properties of the source table.     |
| `Filters`| FilterOperationNode | true | Filter object used to filter the table source data. Allowed values: FilterOperationNode can be null if there are no filters to be applied, or otherwise can be a ComparisonFilterOperationNode or LogicalFilterOperationNode. |

### MappingOperationProperties

| Property | Type | Required | Description |
|---|---|---|---|
| `MappingType` | string | true | Allowed values: `non-timeseries`, `timeseries`.  |
| `MappedProperties` | SourceColumnDefinition[]  | true | The column mappings of the source table to the target table.  |
| `ProcessingType`   | string | true | Indicates the type of processing of the mapping operation. Allowed values: `iterative`, `incremental`. |
| `EntityInstanceIdSchema`   | string[]   | true | Applies to non-timeseries properties. List of strings that represent the source columns that compose the unique identifier of an entity. |
| `TimeseriesEntityLinkProperties` | TimeseriesEntityLinkProperties | true | Applies to time series properties. Source column property used to link the entity to the time series data. |

#### SourceColumnDefinition

| Property | Type | Required | Description |
|---|---|---|---|
| `SourceColumn` | string | true | The name of the source column.    |
| `EntityTypePropertyName` | string | true | Custom name assigned by the customer to the entity type property. |

#### TimeseriesEntityLinkProperties

| Property | Type | Required | Description |
|---|---|---|---|
| `EntityProperty` | string | true  | Entity property used to link the entity to the timeseries data. |
| `TimeseriesProperty` | string | true | Column from timeseries data that matches the values of the entity property. |

### SourceTableProperties

| Property | Type | Required | Description |
|---|---|---|---|
| `SourceType` | string | true | Allowed value: `LakehouseTables`. |
| `WorkspaceId` | Guid | true | The workspace where the customer's lakehouse is located. |
| `ItemId` | Guid | true | ArtifactId of the customer's lakehouse. |
| `SourceTableName` | string | true | The ingested data table name. |
| `SourceSchema` | string | true | The ingested data table schema. |

### ComparisonFilterOperationNode

| Property | Type | Required | Description |
|---|---|---|---|
| `type` | string | true | Type of filter operation. Allowed value: `comparison`. |
| `SourceColumn` | string  | true | Source column name of data being filtered. |
| `ComparisonOperatorKind` | string | true | Represents the kind of comparison operator of the node. Allowed values: `Ge`, `Gt`, `Le`, `Lt`, `Eq`, `NotEq`, `Contains`, `NotContains`, `IsNull`, `IsNotNull`. |
| `Value` | string | true | Value used to filter. |
| `ValueType` | string | true | Type of the value used to filter. Allowed values: `BigInt`, `Float`, `Double`, `String`, `Bool`, `DateTime`. |

### LogicalFilterOperationNode

| Property | Type | Required | Description  |
|---|---|---|---|
| `type` | string | true | Type of filter operation. Allowed value: `logical`. |
| `LogicalOperatorKind` | string | true | Represents the type of logical operator of the node. Allowed values: `And`, `Or`. |
| `FilterOperations` | FilterOperationNode[] | true | Filter operations used by the logical operator. |

### MappingOperations file example

```json
{
  "OperationId": "ce9d0ef9-d8f6-4391-9e37-8bdb91b1fc16",
  "DisplayName": "Equipment1_entitytype",
  "OperationType": "Mapping",
  "EntityTypeId": "139950578358348",
  "MappingOperationProperties": {
    "MappingType": "NonTimeSeries",
    "MappedProperties": [
      {
        "SourceColumn": "Name",
        "EntityTypePropertyName": "DisplayName"
      }
    ],
    "ProcessingType": "Iterative",
    "EntityInstanceIdSchema": ["Id"],
    "TimeseriesEntityLinkProperties": null
  },
  "SourceTableProperties": {
    "SourceType": "LakehouseTables",
    "WorkspaceId": "ae359bb8-2ffa-4e3f-a0b5-2f8bceef6d29",
    "ItemId": "59e5b098-f3d3-45b6-acdd-d7f4c96c6ccd",
    "SourceTableName": "entitytype",
    "SourceSchema": null
  },
  "Filters": {
    "type": "comparison",
    "SourceColumn": "Name",
    "ComparisonOperatorKind": "Contains",
    "Value": "abc",
    "ValueType": "String"
  }
}
```
