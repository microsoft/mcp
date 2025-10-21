---
title: SemanticModel definition
description: Learn how to structure a semantic model item definition when using the Microsoft Fabric REST API.
ms.title: Microsoft Fabric semantic model item definition
author: billmath
ms.author: billmath
ms.service: fabric
ms.date: 05/17/2024
---

# SemanticModel definition

This article provides a breakdown of the definition structure for semantic model items.

## Supported formats

Semantic Model items support the `TMSL` and `TMDL` formats.

## Definition parts

SematicModel has different parts that make up its definition:

* [model.bim](https://learn.microsoft.com/power-bi/developer/projects/projects-dataset#modelbim) or [\definition](https://learn.microsoft.com/power-bi/developer/projects/projects-dataset#definition-folder) <sup>[1](#required1)</sup>
* [definition.pbism](https://learn.microsoft.com/power-bi/developer/projects/projects-dataset#definitionpbism) <sup>[1](#required1)</sup>
* [diagramLayout.json](https://learn.microsoft.com/power-bi/developer/projects/projects-dataset#diagramlayoutjson)

<a name="required1">1</a> - This file or folder is required.  

For more information about the semantic model files, refer to the Power BI Project semantic model folder [documentation](https://learn.microsoft.com/power-bi/developer/projects/projects-dataset).


## Definition payload example using TMSL

```json
 {
    "parts": [
        {
            "path": "model.bim",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition.pbism",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "diagramLayout.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        }
    ]
}
```
## Definition payload example using TMDL

```json
 {
    "parts": [
        {
            "path": "definition/database.tmdl",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/model.tmdl",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/tables/Table1.tmdl",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition.pbism",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "diagramLayout.json",
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
