---
title: Report definition
description: Learn how to structure a report item definition when using the Microsoft Fabric REST API.
ms.title: Microsoft Fabric report item definition
author: billmath
ms.author: billmath
ms.service: fabric
ms.date: 05/17/2024
---

# Report definition

This article provides a breakdown of the definition structure for report items.

## Supported formats

Report items support the `PBIR-Legacy` and `PBIR` format.

## Definition parts

Report has different parts that make up its definition:

- [CustomVisuals\\](https://learn.microsoft.com/power-bi/developer/projects/projects-report#customvisuals)
- [StaticResources\\](https://learn.microsoft.com/power-bi/developer/projects/projects-report#registeredresources)
- [definition.pbir](https://learn.microsoft.com/power-bi/developer/projects/projects-report#definitionpbir) <sup>[1](#required1)</sup>
- [report.json](https://learn.microsoft.com/power-bi/developer/projects/projects-report#reportjson) or [\definition](https://learn.microsoft.com/power-bi/developer/projects/projects-report#definition-folder) <sup>[1](#required1)</sup>
- [semanticModelDiagramLayout.json](https://learn.microsoft.com/power-bi/developer/projects/projects-report#semanticmodeldiagramlayoutjson)
- [mobileState.json](https://learn.microsoft.com/power-bi/developer/projects/projects-report#mobilestatejson)

<a name="required1">1</a> - This file or folder is required. 

> [!IMPORTANT]
> definition.pbir holds the reference to the semantic model used by the report that can be a relative path (byPath) or a connection to a remote semantic model (byConnection). Fabric REST API only supports `byConnection` references.

For more information about the report files, refer to the Power BI Project report folder [documentation](https://learn.microsoft.com/power-bi/developer/projects/projects-report).


## Definition payload example using PBIR-Legacy

```json
 {
    "parts": [
        {
            "path": "report.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition.pbir",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "StaticResources/RegisteredResources/logo.jpg",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        }
        ,
        {
            "path": "StaticResources/RegisteredResources/Light4437032645752863.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        }
        ,
        {
            "path": "StaticResources/SharedResources/BaseThemes/CY23SU04.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        }
    ]
}
```
## Definition payload example using PBIR

```json
 {
    "parts": [
        {
            "path": "definition/report.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/version.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/pages/pages.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/pages/61481e08c8c340011ce0/page.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/pages/61481e08c8c340011ce0/visuals/3852e5607b224b8ebd1a/visual.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/pages/61481e08c8c340011ce0/visuals/3852e5607b224b8ebd1a/mobile.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/pages/61481e08c8c340011ce0/visuals/7df3763f63115a096029/visual.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition.pbir",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "StaticResources/RegisteredResources/logo.jpg",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "StaticResources/RegisteredResources/Light4437032645752863.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "StaticResources/SharedResources/BaseThemes/CY23SU04.json",
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
```
