---
title: EventSchemaSet definition
description: Learn how to create a EventSchemaSet item definition when using the Microsoft Fabric REST API.
ms.title: EventSchemaSet item definition
author: Vanson230
ms.author: seanvovan
ms.service: fabric
ms.date: 7/7/2025
---
  
# EventSchemaSet definition
  
This article provides a breakdown of the definition structure for EventSchemaSet items.
  
## Definition parts
  
This table lists the EventSchemaSet definition parts.
  
<!-- Replace the info in this example table with the definitions of the new item. List all the parts the definition has in the table -->
  
| Definition part path | type | Required | Description |
|--|--|--|--|
| `EventSchemaSetDefinition.json` | EventSchemaSet (JSON) | true | Describes topology of EventSchemaSet item.
  
## EventSchemaSet

### Structure of EventSchemaSet item

Describes a collection of Event metadata and payload, modelled using Schemas.

| Name | Type | Required | Description |
|--|--|--|--|
| `eventTypes` | EventType[] | false | Collection of messages that are stored in Catalog. |
| `schemas` | Schema[] | false | Collection of the formats that are used to represent the messages stored in Catalog. |

### Structure of EventType item

Describes Event metadata, used to communicate between source, Eventstream, and destination items.

| Name | Type | Required | Description |
|--|--|--|--|
| `id` | String | true | Unique id of the eventType |
| `description` | String | false | Description of the eventType |
| `format` | String | true | Serialization format. |
| `envelopeMetadata` | Dictionary<string, object> | false | Collection of messages that are stored in Catalog. |
| `schemaUrl` | string | false | Url to the schemaGroup and Schema. SchemaUrl and Schema are mutually exclusive. If Schema is defined in EventType, then SchemaUrl cannot be included.|
| `schemaFormat` | string | false | Format of the schema. Ex: JsonSchema |
| `schema` | string | false | Schema of the EventType. SchemaUrl and Schema are mutually exclusive. If Schema is defined in EventType, then SchemaUrl cannot be included.|
| `protocol` | string | false | Gets or sets the protocol according to https://github.com/xregistry/spec/blob/main/message/spec.md#http11-http2-http3-protocols |
| `protocolOptions` | object | false | Gets or sets the protocol options according to https://github.com/xregistry/spec/blob/main/message/spec.md#http11-http2-http3-protocols |


### Structure of Schemas item

The Schema item groups all versions of an AVRO Schema declaration.

| Name | Type | Required | Description |
|--|--|--|--|
| `id` | string | true | Unique id of the Schema |
| `description` | string | false | Description of the schema |
| `format` | string | true | Format of the schema. Ex: JsonSchema |
| `versions` | Array | false | Collection of different iterations of the schema. A new version is created when a new schema is provided to the EventType item|

### Structure of SchemaVersion item

Describe the Event payload using supported AVRO versions. Refer to the [AVRO Specification][1] for more details.

| Name | Type | Required | Description |
|--|--|--|--|
| `id` | string | true | Unique id of the SchemaVersion |
| `description` | string | false | Description of the schemaVersion |
| `format` | string | true | Format of the schema. Ex: JsonSchema |
| `schema` | string | false | The schema association with the schema version. |

### EventSchemaSet example <!-- an example of an EventSchemaSet JSON -->

```JSON
{
  "eventTypes": [
    {
      "id": "BicycleSchema",
      "description": "V1 Schema for Bicycles ",
      "format": "CloudEvents/1.0",
      "envelopeMetadata": {
        "id": {
          "type": "string",
          "required": true
        },
        "type": {
          "type": "string",
          "required": true,
          "value": "BicycleSchema"
        },
        "source": {
          "type": "string",
          "required": true
        },
        "specversion": {
          "type": "string",
          "required": true
        }
      },
      "schemaUrl": "#/schemas/BicycleSchema",
      "schemaFormat": "Avro/1.12.0"
    }
  ],
  "schemas": [
    {
      "id": "BicycleSchema",
      "format": "Avro/1.12.0",
      "versions": [
        {
          "id": "v1",
          "format": "Avro/1.12.0",
          "schema": "{\n \"fields\": [\n  {\n   \"name\": \"manufacturer\",\n   \"type\": \"string\",\n   \"doc\": \"company name\"\n  }\n ],\n \"type\": \"record\",\n \"name\": \"BicycleSchema\"\n}"
        }
      ]
    }
  ]
}
```

### EventType examples <!-- Examples of EventType JSON -->
With schema provided:
```Json
 {
      "id": "BicycleSchema",
      "description": "V1 Schema for Bicycles ",
      "format": "CloudEvents/1.0",
      "envelopeMetadata": {
        "id": {
          "type": "string",
          "required": true
        },
        "type": {
          "type": "string",
          "required": true,
          "value": "BicycleSchema"
        },
        "source": {
          "type": "string",
          "required": true
        },
        "specversion": {
          "type": "string",
          "required": true
        }
      },
      "schema": "{\n \"fields\": [\n  {\n   \"name\": \"manufacturer\",\n   \"type\": \"string\",\n   \"doc\": \"company name\"\n  }\n ],\n \"type\": \"record\",\n \"name\": \"BicycleSchema\"\n}",
      "schemaFormat": "Avro/1.12.0"
    }
```
With no schema provided:
```Json
{
      "id": "BicycleSchema",
      "description": "V1 Schema for Bicycles ",
      "format": "CloudEvents/1.0",
      "envelopeMetadata": {
        "id": {
          "type": "string",
          "required": true
        },
        "type": {
          "type": "string",
          "required": true,
          "value": "BicycleSchema"
        },
        "source": {
          "type": "string",
          "required": true
        },
        "specversion": {
          "type": "string",
          "required": true
        }
      },
      "schemaUrl": "#/schemas/BicycleSchema",
      "schemaFormat": "Avro/1.12.0"
    }
```
### Schema example <!-- an example of an Schema JSON -->
```Json
{
      "id": "BicycleSchema",
      "format": "Avro/1.12.0",
      "versions": [
        {
          "id": "v1",
          "format": "Avro/1.12.0",
          "schema": "{\n \"fields\": [\n  {\n   \"name\": \"manufacturer\",\n   \"type\": \"string\",\n   \"doc\": \"company name\"\n  }\n ],\n \"type\": \"record\",\n \"name\": \"BicycleSchema\"\n}"
        }
      ]
}
```
### SchemaVersion example <!-- an example of an SchemaVersion -->
```Json
{
    "id": "v1",
    "format": "Avro/1.12.0",
    "schema": "{\n \"fields\": [\n  {\n   \"name\": \"manufacturer\",\n   \"type\": \"string\",\n   \"doc\": \"company name\"\n  }\n ],\n \"type\": \"record\",\n \"name\": \"BicycleSchema\"\n}"
}
```
[1]: https://avro.apache.org/docs/++version++/specification/