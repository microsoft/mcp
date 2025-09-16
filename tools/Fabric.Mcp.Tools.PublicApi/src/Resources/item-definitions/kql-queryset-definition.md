---
title: KQL Queryset definition
description: Learn how to create a KQL Queryset (Real-Time Queryset) item definition when using the Microsoft Fabric REST API.
author: shsagir
ms.title: KQL Queryset item definition
ms.author: shsagir
ms.reviewer: rotamir
ms.service: fabric
ms.date: 03/11/2025
---

# KQL Queryset definition

This article provides a breakdown of the structure for KQL Queryset (Real-Time Queryset) definition items.

## Definition parts

This table lists the KQL Queryset definition parts.

| Definition part path | type | Required | Description |
|--|--|--|--|
| `RealTimeQueryset.json` | RealTimeQueryset (JSON) | true | Describes the content of the payload. |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item |

## Definition example

```json
{
  "parts": [
    {
      "path": "RealTimeQueryset.json",
      "payload": "ewogICAgInF1ZXJ5c2V0IjogewogICAgICAgICJ2ZXJzaW9uIjogIjEuMC4wIiwKICAgICAgICAiZGF0YVNvdXJjZXMiOiBbewogICAgICAgICAgICAgICAgImlkIjogImMyNDM0YmY4LTI1YmItNGFhMC04NzQ2LWRiNDcwNTMzYWRhZiIsCiAgICAgICAgICAgICAgICAiY2x1c3RlclVyaSI6ICJodHRwczovL2hlbHAua3VzdG8ud2luZG93cy5uZXQvIiwKICAgICAgICAgICAgICAgICJ0eXBlIjogIkF6dXJlRGF0YUV4cGxvcmVyIiwKICAgICAgICAgICAgICAgICJkYXRhYmFzZU5hbWUiOiAiU2FtcGxlcyIKICAgICAgICAgICAgfQogICAgICAgIF0sCiAgICAgICAgInRhYnMiOiBbewogICAgICAgICAgICAgICAgImlkIjogImNjZDdiOTBjLTUxZmUtNDI5Zi1hODUzLTM4NWIwMmJkNzRjOSIsCiAgICAgICAgICAgICAgICAiY29udGVudCI6ICJTdG9ybUV2ZW50c1xcXFxufCBjb3VudCIsCiAgICAgICAgICAgICAgICAidGl0bGUiOiAiVGFiMU5hbWUiLAogICAgICAgICAgICAgICAgImRhdGFTb3VyY2VJZCI6ICJjMjQzNGJmOC0yNWJiLTRhYTAtODc0Ni1kYjQ3MDUzM2FkYWYiCiAgICAgICAgICAgIH0KICAgICAgICBdCiAgICB9Cn0=",
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

## RealTimeQueryset

The `payload` property contains the content of the definition, which is Base64-encoded. The content is a JSON object that describes the Queryset. The JSON object contains a root object called `queryset`, which has several properties that define the Queryset.

### Queryset properties

Describes the fields used to construct the `Queryset` object.

| Definition part path | type | Required | Description |
|--|--|--|--|
| `version` | string | true | The version of the Queryset. |
| `dataSources` | DataSource (Array) | true | The list of data sources used in the Queryset. |
| `tabs` | Tab (Array) | true | The list of tabs in the Queryset. |

### DataSource

Describes the fields used to construct the `DataSource` object.

| Definition part path | type | Required | Description |
|--|--|--|--|
| `id` | string | true | The unique identifier for the data source. |
| `clusterUri` | string | true | The URI of the data source. |
| `type` | string | true | The type of the data source. Valid values: `AzureDataExplorer` |
| `databaseName` | string | true | The name of the database in the data source. |

### Tab

Describes the fields used to construct the `Tab` object.

| Definition part path | type | Required | Description |
|--|--|--|--|
| `id` | string | true | The unique identifier for the tab. |
| `content` | string | true | The content of the tab, which is a KQL query. |
| `title` | string | true | The title of the tab. |
| `dataSourceId` | string | true | The unique identifier of the data source used in the tab. |

## RealTimeQueryset example

The following example payload is a JSON object that describes a Queryset containing a single tab with the query `StormEvents | count`.

This example shows the decoded JSON object for the `RealTimeQueryset` payload.

```json
{
  "queryset": {
    "version": "1.0.0",
    "dataSources": [
      {
        "id": "c2434bf8-25bb-4aa0-8746-db470533adaf",
        "clusterUri": "https://help.kusto.windows.net/",
        "type": "AzureDataExplorer",
        "databaseName": "Samples"
      }
    ],
    "tabs": [
      {
        "id": "ccd7b90c-51fe-429f-a853-385b02bd74c9",
        "content": "StormEvents\\\\n| count",
        "title": "Tab1Name",
        "dataSourceId": "c2434bf8-25bb-4aa0-8746-db470533adaf"
      }
    ]
  }
}
```
