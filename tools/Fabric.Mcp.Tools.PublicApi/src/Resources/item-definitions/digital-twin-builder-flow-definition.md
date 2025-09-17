--- 
title: Digital twin builder flow item definition
description: Learn how to create a digital twin builder flow item definition when using the Microsoft Fabric REST API.
author: vidyashreeb
ms.author: vidyashreeb
ms.title: Digital twin builder flow item definition
ms.service: fabric
ms.date: 04/25/2025
---

# Digital twin builder flow definition

This article provides a breakdown of the structure for digital twin builder flow definition items.

## Supported formats

DigitalTwinBuilderFlow items support the JSON format.

## Definition parts

This table lists the digital twin builder flow definition parts.

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `definition.json` | DefinitionDetails (JSON) | true | Describes the `DigitalTwinBuilderId` associated with the item. |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item. |

The DigitalTwinBuilderFlow gets deleted when DigitalTwinBuilder is deleted.

## Definition example

```json
{
  "parts": [
    {
      "path": "definition.json",
      "payload": "eyAKICAiRGlnaXRhbFR3aW5CdWlsZGVySWQiOiAiNTZhMGU2Y2EtMTAxZS1iYzA1LTQ2NDktNjAzOTMzYWUxMjcwIiwgCiAgIk9wZXJhdGlvbklkcyI6IFsgCiAgICAiY2U5ZDBlZjktZDhmNi00MzkxLTllMzctOGJkYjkxYjFmYzE2IiAKICBdLCAKICAiSXNPbkRlbWFuZCI6IGZhbHNlIAp9IA==",
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
 
## DefinitionDetails

| Property | Type | Required | Description |
|---|---|---|---|
| `DigitalTwinBuilderId` | Guid  | true | Item ID of the parent DigitalTwinBuilder that exists in the workspace. |
| `OperationIds` | Guid[] | true | List of operations (mapping or contextualization operations) that are assigned to this digital twin builder flow. No operations are assigned if `IsOnDemand` is set to true. |
| `IsOnDemand` | bool | true | The value is set to true only for creating on-demand DigitalTwinBuilderFlow items. There is only one on-demand DigitalTwinBuilderFlow associated with each DigitalTwinBuilder. |

### Definition file example

```json
{
  "DigitalTwinBuilderId": "56a0e6ca-101e-bc05-4649-603933ae1270",
  "OperationIds": [
    "ce9d0ef9-d8f6-4391-9e37-8bdb91b1fc16"
  ],
  "IsOnDemand": false
}
```

```json
{
  "DigitalTwinBuilderId": "56a0e6ca-101e-bc05-4649-603933ae1270",
  "OperationIds": [],
  "IsOnDemand": true
}
```
 
## PlatformDetails

The platform part is a file that contains the environment metadata information.
* [Create Item](https://learn.microsoft.com/rest/api/fabric/core/items/create-item) with definition respects the platform file if provided
* [Get Item](https://learn.microsoft.com/rest/api/fabric/core/items/get-item) definition always returns the platform file.
* [Update Item](https://learn.microsoft.com/rest/api/fabric/core/items/update-item) definition accepts the platform file if provided, but only if you set a new URL parameter `updateMetadata=true`.
