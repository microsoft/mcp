---
title: Reflex definition
description: Learn how to structure an Reflex item definition when using the Microsoft Fabric REST API.
ms.title: Microsoft Fabric Reflex item definition
author: JeromeZhao
ms.author: jeromez
ms.service: fabric
ms.date: 11/26/2024
---

# Reflex definition

This article provides a breakdown of the definition structure for Reflex items.

>[!NOTE]
>Reflex is also known as Activator.

## Supported formats

Reflex items support the `json` format.

## Definition parts

The definition of an Reflex item is made out of a single part, and is constructed as follows:

* **Path** - The file name, for example `ReflexEntities.json`.
* **Payload type** - InlineBase64
* **Payload** See: [Example of payload content decoded from Base64](#example-of-payload-content-decoded-from-base64).

### Example of payload content decoded from Base64

```json
[
    {
        "uniqueIdentifier": "6876bc8e-60a7-46d2-94db-500a46d88437",
        "payload": {
            "name": "KQL",
            "type": "kqlQueries"
        },
        "type": "container-v1"
    },
    {
        "uniqueIdentifier": "db648ca7-6911-4e39-9394-a9d05b582041",
        "payload": {
            "name": "EndTime",
            "runSettings": {
                "executionIntervalInSeconds": 300
            },
            "query": {
                "queryString": "['Weather'] | take 10"
            },
            "connection": {
                "databaseName": "c2375aaa",
                "clusterHostName": "c2375aaa.kusto.windows.net"
            },
            "eventhouseItem": {
                "itemType": "KustoDatabase",
                "itemId": "399b2e34-0392-4f87-90f1-efb820a781d3",
                "workspaceId": "974f8c44-20af-46b3-bc11-4a5e1515d21e"
            },
            "queryParameters": [],
            "parentContainer": {
                "targetUniqueIdentifier": "6876bc8e-60a7-46d2-94db-500a46d88437"
            }
        },
        "type": "kqlSource-v1"
    }
]
```

## Definition example

```json
{
    "format": "json",
    "parts": [
        {
            "path": "ReflexEntities.json",
            "payload": "W10=",
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
