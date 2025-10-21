---
title: Spark job item definition
description: Learn how to structure a Spark job definition when using the Microsoft Fabric REST API.
ms.title: Microsoft Fabric spark job definition item definition
author: billmath
ms.author: billmath
ms.service: fabric
ms.date: 05/17/2024
---

# Spark Job definition

This article provides a breakdown of the structure for Spark job definition items. For detailed information, see: [How to create and update a Spark Job Definition with Microsoft Fabric Rest API](https://learn.microsoft.com/fabric/data-engineering/spark-job-definition-api).

## Supported formats

SparkJobDefinition items support the `SparkJobDefinitionV1` format.

## Definition parts

The definition of a Spark job item is made out of a single part, and is constructed as follows:

* **Path** - The file name, for example: `SparkJobDefinitionV1.json`

* **Payload Type** - InlineBase64

* **Payload** See [Example of payload content decoded from Base64](#example-of-payload-content-decoded-from-base64)

### Example of payload content decoded from Base64

```json
{
    "executableFile": null,
    "defaultLakehouseArtifactId": "",
    "mainClass": "",
    "additionalLakehouseIds": [],
    "retryPolicy": null,
    "commandLineArguments": "",
    "additionalLibraryUris": [],
    "language": "",
    "environmentArtifactId": null
}
```

## Definition example

Here's an example of an item definition for a Spark job.

```json
{
    "format": "SparkJobDefinitionV1",
    "parts": [
        {
            "path": "SparkJobDefinitionV1.json",
            "payload": "eyJleGVjdXRhYmxlRmlsZSI6bnVsbCwiZGVmYXVsdExha2Vob3VzZUFydGlmYWN0SWQiOiIiLCJtYWluQ2xhc3MiOiIiLCJhZGRpdGlvbmFsTGFrZWhvdXNlSWRzIjpbXSwicmV0cnlPbGljYXR5IjpudWxsLCJjb21tYW5kTGluZUFyZ3VtZW50c2I6bnVsbCwiY29tbWFuZExpbmVBYnJndW1lbnRzIjpbXSwibGFuZ3VhZ2UiOiIiLCJlbm52ZW1lbnRBYnJndW1lbnRzIjpbXSwibGFuZ3VhZ2UiOiIiLCJlbm52ZW1lbnRBYnJndW1lbnRzIjpbXSwiZW52aXJvbm1lbnRBcnRpZmFjdElkIjpudWxsfQ==",
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
