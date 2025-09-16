---
title: Dataflow definition
description: Learn how to structure a dataflow item definition when using the Microsoft Fabric REST API.
ms.title: Microsoft Fabric dataflow item definition
author: rnandurimsft
ms.author: rnanduri
ms.service: fabric
ms.date: 03/28/2025
---

# Dataflow definition

This article provides a breakdown of the definition structure for dataflow items.

## Definition parts

| Definition part path | type | Required | Description |
|--|--|--|--|
| `queryMetadata.json` | [Metadata ContentDetails](#metadata-contentdetails) (JSON) | true | Describes metadata related to query options in dataflow  |
| `mashup.pq`          | [Mashup ContentDetails](#mashup-contentdetails-example) (PQ) | true | Describes mashup content of payload. It contains sequence of all the steps performed in dataflow |

## Metadata ContentDetails

Describes content of payload

| Name                  | Type            | Required        | Description                         |
|-----------------------|-----------------|-----------------|-------------------------------------|
| formatVersion         | String          | true            | Version of dataflow item format. The only allowed value is `202502` |
| name                  | String          | true            | The name of the mashup |
| computeEngineSettings | [ComputeEngineSettings](#computeenginesettings-contents)    | false           | The compute engine settings    |
| queryGroups           | [QueryGroup](#querygroups-contents)[]    | false           | Query groups    |
| documentLocale        | String          | false           | The locale of the document; needs to be BCP-47 language codes |
| gatewayObjectId       | String          | false           | The gateway object ID |
| queriesMetadata       | [QueriesMetadata](#queriesmetadata-contents)    | false           | Queries metadata    |
| connections           | [Connection](#connection-contents)[]    | false           | User connections    |
| fastCombine           | Boolean         | false           | Indicates whether or not to use fast combine. True - use fast combine. False (default) - do not use fast combine |
| allowNativeQueries    | Boolean         | false           | Indicates whether or not native queries are allowed. True (default) - allow native queries. False - do not allow native queries    |
| skipAutomaticTypeAndHeaderDetection     | Boolean         | false    | Indicates whether or not to skip automatic type and header detection. True - skip detection. False (default) - do not skip detection    |
| parametric            | Boolean         | false           | Indicates whether or not parametric mode is used. True - parametric mode is used. False (default) - parametric mode is not used |

### ComputeEngineSettings Contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| allowFastCopy         | Boolean         | false           | Indicates if fast copy is enabled or not. True (default) - allow fast copy. False - Do not allow fast copy |
| maxConcurrency        | Integer         | false           | The maximum number of concurrent evaluations to use when executing the dataflow |

### QueryGroups Contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| id                    | String          | false           | The ID of the query group |
| name                  | String          | false           | The name of the query group |
| description           | String          | false           | The description of the query group |
| parentId              | String          | false           | The parent ID of the query group |
| order                 | Integer         | false           | The order of the query group |

### QueriesMetadata Contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| queryId               | String          | true            | The query ID      |
| queryName             | String          | true            | The name of the query |
| queryGroupId          | String          | false           | The query group ID |
| isHidden              | Boolean         | false           | Indicates whether or not the query is hidden. True - query is hidden. False (default) - query is not hidden |
| loadEnabled           | Boolean         | false           | Indicates whether or not load is enabled. True (default) - load is enabled. False - load is not enabled |

### Connection Contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| path                  | String          | false           | The connection path    |
| kind                  | String          | false           | The connection type    |
| connectionId          | String          | false           | The connection ID    |

### Metadata ContentDetails example

```json
{
  "formatVersion": "202502",
  "computeEngineSettings": {
    "allowFastCopy": true,
    "maxConcurrency": 1
  },
  "name": "SampleDataflowGen",
  "queryGroups": [
    
  ],
  "documentLocale": "en-US",
  "gatewayObjectId": null,
  "queriesMetadata": {
    "publicholidays": {
      "queryId": "a0a0a0a0-bbbb-cccc-dddd-e1e1e1e1e1e1",
      "queryName": "publicholidays",
      "queryGroupId": null,
      "isHidden": false,
      "loadEnabled": true
    }
  },
  "connections": [
    {
      "path": "Lakehouse",
      "kind": "Lakehouse",
      "connectionId": "{\"ClusterId\":\"b1b1b1b1-cccc-dddd-eeee-f2f2f2f2f2f2\",\"DatasourceId\":\"c2c2c2c2-dddd-eeee-ffff-a3a3a3a3a3a3\"}"
    }
  ],
  "fastCombine": false,
  "allowNativeQueries": true,
  "skipAutomaticTypeAndHeaderDetection": false
}
```

### Mashup ContentDetails example

```pq
[StagingDefinition = [Kind = "FastCopy"]]
section Section1;
shared publicholidays = 
let  Source = Lakehouse.Contents([]),  
#"Navigation 1" = Source{[workspaceId = "d3d3d3d3-eeee-ffff-aaaa-b4b4b4b4b4b4"]}[Data],  
#"Navigation 2" = #"Navigation 1"{[lakehouseId = "e4e4e4e4-ffff-aaaa-bbbb-c5c5c5c5c5c5"]}[Data],  
#"Navigation 3" = #"Navigation 2"{[Id = "publicholidays", ItemKind = "Table"]}[Data],  
#"Changed column type" = Table.TransformColumnTypes(#"Navigation 3", {{"normalizeHolidayName", type text}}),  
#"Lowercased text" = Table.TransformColumns(#"Changed column type", {{"countryRegionCode", each Text.Lower(_), type nullable text}}),  
#"Uppercased text" = Table.TransformColumns(#"Lowercased text", {{"normalizeHolidayName", each Text.Upper(_), type nullable text}}),  
#"Calculated text length" = Table.TransformColumns(#"Uppercased text", {{"countryOrRegion", each Text.Length(_), type nullable Int64.Type}})in  #"Calculated text length";
```