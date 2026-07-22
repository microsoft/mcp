# Dataflow definition

This article provides a breakdown of the definition structure for dataflow items.

## Definition parts

| Definition part path | type | Required | Description |
|--|--|--|--|
| `queryMetadata.json` | [Metadata ContentDetails](#metadata-contentdetails) (JSON) | true | Describes metadata related to query options in dataflow  |
| `mashup.pq`          | [Mashup ContentDetails](#mashup-contentdetails-example) (PQ) | true | Describes mashup content of payload. It contains sequence of all the steps performed in dataflow |
| `<mdfTransformName>.mdf` | [MDFTransform ContentDetails](#mdftransform-contentdetails) (JSON) | false | Describes Mapping Data Flow (MDF) transform content of the payload. There can be multiple MDF transforms within a dataflow |

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
    },
    "MDF transform": {
      "queryId": "9c6d7c87-83db-4718-b382-c0c1aa1636d7",
      "queryName": "MDF transform",
      "loadEnabled": false
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
[ItemType = "MDF"]
shared #"MDF transform" = "___ExternalResource-Placeholder___";
```

## MDFTransform ContentDetails

Describes the content of a Mapping Data Flow payload.

| Name                  | Type            | Required        | Description                         |
|-----------------------|-----------------|-----------------|-------------------------------------|
| name                  | String          | true            | The name of the mapping data flow |
| properties            | [Properties](#mdftransform-properties-contents)      | true            | The properties of the mapping data flow |

### MDFTransform Properties Contents

| Name                  | Type            | Required        | Description                         |
|-----------------------|-----------------|-----------------|-------------------------------------|
| type                  | String          | true            | The type of the dataflow. The allowed value is `MappingDataFlow` |
| typeProperties        | [TypeProperties](#mdftransform-typeproperties-contents)  | true            | The type properties of the mapping data flow |

### MDFTransform TypeProperties Contents

| Name                  | Type            | Required        | Description                         |
|-----------------------|-----------------|-----------------|-------------------------------------|
| sources               | [Source](#mdftransform-source-contents)[]          | false           | The list of sources in the mapping data flow |
| sinks                 | [Sink](#mdftransform-sink-contents)[]            | false           | The list of sinks in the mapping data flow |
| transformations       | [Transformation](#mdftransform-transformation-contents)[]  | false           | The list of transformations in the mapping data flow |
| scriptLines           | String[]        | true            | The script lines that define the mapping data flow logic |

### MDFTransform Source Contents

| Name                  | Type            | Required        | Description                         |
|-----------------------|-----------------|-----------------|-------------------------------------|
| name                  | String          | true            | The name of the source |
| externalReferences    | [ExternalReferences](#mdftransform-externalreferences-contents) | false           | The external references for the source |

### MDFTransform Sink Contents

| Name                  | Type            | Required        | Description                         |
|-----------------------|-----------------|-----------------|-------------------------------------|
| name                  | String          | true            | The name of the sink |
| externalReferences    | [ExternalReferences](#mdftransform-externalreferences-contents) | false           | The external references for the sink |

### MDFTransform Transformation Contents

| Name                  | Type            | Required        | Description                         |
|-----------------------|-----------------|-----------------|-------------------------------------|
| name                  | String          | true            | The name of the transformation |

### MDFTransform ExternalReferences Contents

| Name                  | Type            | Required        | Description                         |
|-----------------------|-----------------|-----------------|-------------------------------------|
| connection            | String          | false           | The connection ID for the external reference |

### MDFTransform ContentDetails example

```json
{
  "name": "MDF",
  "properties": {
    "type": "MappingDataFlow",
    "typeProperties": {
      "sources": [
        {
          "name": "source1",
          "externalReferences": {
            "connection": "<source1ConnectionId>"
          }
        }
      ],
      "sinks": [
        {
          "name": "sink1",
          "externalReferences": {
            "connection": "<sink1ConnectionId>"
          }
        }
      ],
      "transformations": [
        {
          "name": "derivedColumn1"
        }
      ],
      "scriptLines": [
        "source(output(",
        "          Column1 as short,",
        "          Column2 as string,",
        "          Column3 as date,",
        "          Column4 as boolean,",
        "          Column5 as string,",
        "          Column6 as double",
        "     ),",
        "     useSchema: false,",
        "     allowSchemaDrift: true,",
        "     validateSchema: false,",
        "     ignoreNoFilesFound: false,",
        "     format: 'delimited',",
        "     fileSystem: 'folder1',",
        "     fileName: 'file1.csv',",
        "     columnDelimiter: ',',",
        "     escapeChar: '\\\\',",
        "     quoteChar: '\\\"',",
        "     columnNamesAsHeader: true) ~> source1",
        "source1 derive(isColumn6GreaterThan10 = Column6>10) ~> derivedColumn1",
        "derivedColumn1 sink(allowSchemaDrift: true,",
        "     validateSchema: false,",
        "     format: 'json',",
        "     fileSystem: 'output1',",
        "     umask: 0022,",
        "     preCommands: [],",
        "     postCommands: [],",
        "     skipDuplicateMapInputs: true,",
        "     skipDuplicateMapOutputs: true) ~> sink1"
      ]
    }
  }
}
```
