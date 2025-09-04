---
title: DataPipeline definition
description: Learn how to structure a data pipeline item definition when using the Microsoft Fabric REST API.
ms.title: Microsoft Fabric data pipeline item definition
author: rnandurimsft
ms.author: rnanduri
ms.service: fabric
ms.date: 02/19/2025
---

# DataPipeline definition

This article provides a breakdown of the definition structure for data pipeline items.

## Definition parts

| Definition part path | type | Required | Description |
|--|--|--|--|
| `pipeline-content.json` | ContentDetails (JSON) | true | Describes data pipeline content of payload |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item |

## ContentDetails

Describes content of payload

| Name                  | Type            | Description                         |
|-----------------------|-----------------|-------------------------------------|
| properties            | DataPipelineProperties            | DataPipeline properties. See [Description for DataPipelineProperties contents](#description-for-datapipelineproperties-contents)|

### Description for DataPipelineProperties contents

Describes the fields used to construct the DataPipelineProperties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| activities            | Activity[]      | false           | List of activities. See [Description for Activity contents](#description-for-activity-contents) |
| description           | String          | false           | Description of Data Pipeline |

### Description for Activity contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| name                  | String          | true            | Name of the activity    |
| type                  | String          | true            | Type of the activity    |
| dependsOn             | DependencyActivity[]           | false           | List of activities the activity depends on. See [Description for DependencyActivity contents](#description-for-dependencyactivity-contents) |
| typeProperties        | object(JSON)    | true            | Type properties of activity    |

### Description for DependencyActivity contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| activity              | String          | true            | Name of the activity    |
| dependencyConditions  | String[]        | true            | Dependency Condition. Accepted string values inside array are - `Succeeded`, `Skipped`, `Failed`, `Completed`    |

### ContentDetails example

```json
{
    "properties": { 
        "description": "Data pipeline description", 
        "activities": [ 
            { 
                "name": "Wait_Activity_1", 
                "type": "Wait", 
                "dependsOn": [], 
                "typeProperties": { 
                    "waitTimeInSeconds": 240 
                } 
            },
            {
                "name": "Wait_Activity_2",
                "type": "Wait",
                "dependsOn": [
                    {
                        "activity": "Wait_Activity_1",
                        "dependencyConditions": [ "Succeeded" ]
                    }
                ],
                "typeProperties": {
                    "waitTimeInSeconds": 240
                }
            }
        ]
    } 
} 
```
