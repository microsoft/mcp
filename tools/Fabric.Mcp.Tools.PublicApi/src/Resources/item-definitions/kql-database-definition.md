---
title: KQL Database item definition
description: Learn how to structure a KQL Database definition when using the Microsoft Fabric REST API.
author: yaelschuster
ms.title: Microsoft Fabric KQL Database item definition
ms.author: yaschust
ms.service: fabric
ms.date: 11/12/2024
---
# KQL Database definition

This article provides a breakdown of the structure for KQL Database definition items.

## Supported formats

KQLDatabaseDefinition items support the `JSON` format.

## Definition parts

The definition of a KQL Database item is constructed from three parts: the item part, the platform part, and the KQL database schema. Each part contains the following:

* **Path**: The path to the file that contains the JSON definition.
* **Payload**: For the platform part, see [Platform part](#platform-part). For the DatabaseProperties part, see [Example of DatabaseProperties.json part decoded from Base64](#example-of-databasepropertiesjson-part-decoded-from-base64). For the DatabaseSchema part, see [Example of DatabaseSchema.kql part decoded from Base64](#example-of-databaseschemakql-part-decoded-from-base64).
* **PayloadType**: InlineBase64

## Example of DatabaseProperties.json part decoded from Base64

The JSON file describing the KQL database has the following properties:

| Property                       | Type   | Required | Description                                                                                                                                                            |
| ------------------------------ | ------ | -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `databaseType`                 | string | &#9989;  | Kind of the database, currently only ReadWrite is supported.                                                                                                           |
| `parentEventhouseItemId`       | guid   | &#9989;  | Item ID of the parent eventhouse.                                                                                                                                      |
| `oneLakeCachingPeriod`         | string | &#10060; | The amount of time the data is cached in OneLake, making it available for faster queries.  The expected format is ISO 8601 Time span, where P356D represents 356 days. |
| `oneLakeStandardStoragePeriod` | string | &#10060; | The amount of time the data is stored in OneLake storage, making it available for queries. The expected format is ISO 8601 Time span, where P356D represents 356 days. |

```json
{
  "databaseType": "ReadWrite",
  "parentEventhouseItemId": "eff8ccbe-b44b-4101-9fd2-a99fc33543d0", 
  "oneLakeCachingPeriod": "P36500D", 
  "oneLakeStandardStoragePeriod": "P365000D" 
}
```

## Platform part

The platform payload is optional. The platform part is a file that contains the Eventhouse metadata information.

* [Create Item](https://learn.microsoft.com/rest/api/fabric/core/items/create-item) with definition respects the platform file if provided.
* [Get Item](https://learn.microsoft.com/rest/api/fabric/core/items/get-item) definition always returns the platform file.
* [Update Item](https://learn.microsoft.com/rest/api/fabric/core/items/update-item) definition accepts the platform file if provided, if you set a new URL parameter `updateMetadata=true`.

## Example of DatabaseSchema.kql part decoded from Base64

The KQL database schema is a KQL script that defines the [tables](https://learn.microsoft.com/kusto/management/tables?view=microsoft-fabric&preserve-view=true), [functions](https://learn.microsoft.com/kusto/management/functions?view=microsoft-fabric&preserve-view=true), [materialized views](https://learn.microsoft.com/kusto/management/materialized-views/materialized-view-overview?view=microsoft-fabric&preserve-view=true), and more.


```kusto

.create-merge table MyLogs2 (Level:string, Timestamp:datetime, UserId:string, TraceId:string, Message:string, ProcessId:int) 
.create-merge table MyLogs3 (Level:string, Timestamp:datetime, UserId:string, TraceId:string, Message:string, ProcessId:int) 
.create-merge table MyLogs7 (Level:string, Timestamp:datetime, UserId:string, TraceId:string, Message:string, ProcessId:int) 
```

## Definition example

```json
{
"parts": [
    {
        "path": "DatabaseProperties.json",
        "payload": "ewogICJkYXRhYmFzZVR5cGUiOiAiUmVhZFdyaXRlIiwKICAicGFyZW50RXZlbnRob3VzZUl0ZW1JZCI6ICI1YjIxODc3OC1lN2E1LTRkNzMtODE4Ny1mMTA4MjQwNDc4MzYiLAogICJvbmVMYWtlQ2FjaGluZ1BlcmlvZCI6ICJQMzY1MDBEIiwKICAib25lTGFrZVN0YW5kYXJkU3RvcmFnZVBlcmlvZCI6ICJQMzY1MDBEIgp9",
        "payloadType": "InlineBase64"
    },
    {
        "path": "DatabaseSchema.kql",
        "payload": "Ly8gS1FMIHNjcmlwdAovLyBVc2UgbWFuYWdlbWVudCBjb21tYW5kcyBpbiB0aGlzIHNjcmlwdCB0byBjb25maWd1cmUgeW91ciBkYXRhYmFzZSBpdGVtcywgc3VjaCBhcyB0YWJsZXMsIGZ1bmN0aW9ucywgbWF0ZXJpYWxpemVkIHZpZXdzLCBhbmQgbW9yZS4KCi5jcmVhdGUtbWVyZ2UgdGFibGUgTXlMb2dzIChMZXZlbDpzdHJpbmcsIFRpbWVzdGFtcDpkYXRldGltZSwgVXNlcklkOnN0cmluZywgVHJhY2VJZDpzdHJpbmcsIE1lc3NhZ2U6c3RyaW5nLCBQcm9jZXNzSWQ6aW50KQ==",
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
