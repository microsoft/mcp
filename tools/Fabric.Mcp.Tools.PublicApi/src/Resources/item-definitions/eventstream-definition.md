---
title: Eventstream definition
description: Learn how to create an Eventstream definition when using the Microsoft Fabric REST API.
ms.title: Eventstream definition
author: alexlzx
ms.author: zhenxilin
ms.service: fabric
ms.date: 05/14/2025
---

# Eventstream definition

This article provides a breakdown of the definition structure for Eventstream items.

## Definition parts

This table lists the Eventstream definition parts.

| Definition part path | type | Required | Description |
|--|--|--|--|
| `eventstream.json` | [Eventstream (JSON)](#eventstream) | &#9989; | Describe the topology of Eventstream item  |
| `eventstreamProperties.json` | [EventstreamProperties (JSON)](#eventstreamproperties) | &#10060; | Describe Eventstream metadata |
| `.platform` | PlatformDetails (JSON) | &#10060; | Describes the metadata of the item |

Each definition part of an Eventstream item is constructed as follows:

* **Path**: The file name, for example: `eventstream.json`
* **Payload Type**: InlineBase64
* **Payload**: See [Example of payload content decoded from Base64](#eventstreamjson-example)

## Eventstream

Describe the topology of Eventstream item.

| Name | Type | Required | Description |
|--|--|--|--|
| `sources` | Array | true | Describes the data sources that can be ingested into Eventstream for processing. |
| `destinations` | Array | true | Describes the endpoints within Fabric where processed data can be routed to, including Lakehouse, Eventhouse, Reflex, and others. |
| `operators` | Array | true | Define the event processors that handle real-time data streams, such as Filter, Aggregate, Group By, and Join. |
| `streams` | Array | true | Describes the data streams available for subscription and analysis in the Real-time Hub. There are two types of streams: default streams and derived streams. |

## `eventstream.json` example

To see how to create a JSON file describing an Eventstream item, see [Eventstream REST API](https://learn.microsoft.com/fabric/real-time-intelligence/event-streams/eventstream-rest-api).

```json
{
    "sources": [
        {
            "name": "myEventHub",
            "type": "AzureEventHub",
            "properties": {
                "dataConnectionId": "cc8271ee-8f72-473d-969c-6828f5fd0d45",
                "consumerGroupName": "$Default",
                "inputSerialization": {
                    "type": "Json",
                    "properties": {
                        "encoding": "UTF8"
                    }
                }
            }
        }
    ],
    "destinations": [
        {
            "name": "myLakehouse",
            "type": "Lakehouse",
            "properties": {
                "workspaceId": "fdf52f3a-b687-41b8-8ff8-aeeca4d1edd8",
                "itemId": "737d6a97-e88c-45e1-9c39-adf1c9c4e817",
                "schema": "",
                "deltaTable": "newTable",
                "minimumRows": 100000,
                "maximumDurationInSeconds": 120,
                "inputSerialization": {
                    "type": "Json",
                    "properties": {
                        "encoding": "UTF8"
                    }
                }
            },
            "inputNodes": [
                {
                    "name": "derivedStream"
                }
            ]
        }
    ],
    "streams": [
        {
            "name": "myEventstream-stream",
            "type": "DefaultStream",
            "properties": {},
            "inputNodes": [
                {
                    "name": "myEventHub"
                }
            ]
        },
        {
            "name": "derivedStream",
            "type": "DerivedStream",
            "properties": {
                "inputSerialization": {
                    "type": "Json",
                    "properties": {
                        "encoding": "UTF8"
                    }
                }
            },
            "inputNodes": [
                {
                    "name": "GroupBy"
                }
            ]
        }
    ],
    "operators": [
        {
            "name": "GroupBy",
            "type": "GroupBy",
            "inputNodes": [
                {
                    "name": "myEventstream-stream"
                }
            ],
            "properties": {
                "aggregations": [
                    {
                        "aggregateFunction": "Average",
                        "column": {
                            "expressionType": "ColumnReference",
                            "node": null,
                            "columnName": "payload",
                            "columnPathSegments": [
                                {
                                    "field": "ts_ms"
                                }
                            ]
                        },
                        "alias": "AVG_ts_ms"
                    }
                ],
                "groupBy": [],
                "window": {
                    "type": "Tumbling",
                    "properties": {
                        "duration": {
                            "value": 5,
                            "unit": "Minute"
                        },
                        "offset": {
                            "value": 1,
                            "unit": "Minute"
                        }
                    }
                }
            }
        }
    ],
    "compatibilityLevel": "1.0"
}

```

To construct an Eventstream item for the API payload, you can use the [GitHub template](https://github.com/microsoft/fabric-event-streams) to define an Eventstream item.

## EventstreamProperties
Describe Eventstream metadata.

| Name | Type | Required | Description |
|--|--|--|--|
| `retentionTimeInDays` | Integer | &#10060; | Describes the retention days of the Eventstream item. Default value is 1. Allowed value range from 1 to 90. |
| `eventThroughputLevel` | Enum | &#10060; | Describes the event throughput level of the Eventstream item. Default value is Low. Allowed values are `Low`, `Medium`, `High`. |

## `eventstreamProperties.json` example

```json
{
  "retentionTimeInDays": 1,
  "eventThroughputLevel": "Low"
}

```

## Platform part

The platform payload is optional. The platform part is a file that contains the Eventstream metadata information.

* [Create Item](https://learn.microsoft.com/rest/api/fabric/core/items/create-item) with definition respects the platform file if provided.
* [Get Item](https://learn.microsoft.com/rest/api/fabric/core/items/get-item) definition always returns the platform file.
* [Update Item](https://learn.microsoft.com/rest/api/fabric/core/items/update-item) definition accepts the platform file if provided, if you set a new URL parameter `updateMetadata=true`.

## Definition example

Here's an example of a Base64-encoded eventstream definition, where the content from [Example of payload content decoded from Base64](#eventstreamjson-example) is encoded in Base64 and placed in the `payload` field with the path set to `eventstream.json` :

```JSON

{
  "displayName": "myEventstream",
  "type": "Eventstream",
  "description": "Create Eventstream item with definition",
  "definition": {
    "parts": [
      {
        "path": "eventstream.json",
        "payload": "<base64 encoded string>",
        "payloadType": "InlineBase64"
      },
      {
        "path": "eventstreamProperties.json",
        "payload": "<base64 encoded string>",
        "payloadType": "InlineBase64"
      },
      {
        "path": ".platform",
        "payload": "ZG90UGxhdGZvcm1CYXNlNjRTdHJpbmc=",
        "payloadType": "InlineBase64"
      }
    ]
  }
}

```
