---
title: Eventhouse item definition
description: Learn how to structure an Eventhouse definition when using the Microsoft Fabric REST API.
author: yaelschuster
ms.title: Microsoft Fabric Eventhouse item definition
ms.author: yaschust
ms.service: fabric
ms.date: 11/12/2024
---
# Eventhouse definition

This article provides a breakdown of the structure for Eventhouse definition items.

## Supported formats

EventhouseDefinition items support the `JSON` format.

## Definition parts

The definition of an Eventhouse item is constructed from two parts: the item part and the [platform part](#platform-part). Each part contains the following:

* **Path**: The path to the file that contains the JSON definition.
* **Payload**: For the platform part, see [Platform part](#platform-part). For the EventhouseProperties part, see [Example of EventhouseProperties.json definition part decoded from Base64](#example-of-eventhousepropertiesjson-definition-part-decoded-from-base64).
* **PayloadType**: InlineBase64

## Platform part

The platform payload is optional. The platform part is a file that contains the Eventhouse metadata information.

* [Create Item](https://learn.microsoft.com/rest/api/fabric/core/items/create-item) with definition respects the platform file if provided.
* [Get Item](https://learn.microsoft.com/rest/api/fabric/core/items/get-item) definition always returns the platform file.
* [Update Item](https://learn.microsoft.com/rest/api/fabric/core/items/update-item) definition accepts the platform file if provided, if you set a new URL parameter `updateMetadata=true`.

## Example of EventhouseProperties.json definition part decoded from Base64

There aren't any properties described in the Eventhouse, so an empty object is given.

```json
{}
```

## Definition example

```JSON
{
"parts": [
    {
        "path": "EventhouseProperties.json",
        "payload": "e30=",
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
