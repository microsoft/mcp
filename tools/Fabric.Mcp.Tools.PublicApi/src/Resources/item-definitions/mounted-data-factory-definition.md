---
title: Mounted Data Factory definition
description: Learn how to create a Mounted Data Factory item definition when using the Microsoft Fabric REST API.
ms.title: Mounted Data Factory item definition
author: rnandurimsft
ms.author: rnanduri
ms.service: fabric
ms.date: 01/16/2025
---
  
# Mounted Data Factory definition
  
This article provides a breakdown of the definition structure for Mounted Data Factory items
  
## Definition parts
  
| Definition part path | type | Required | Description |
|--|--|--|--|
| `mountedDataFactory-content.json` | ContentDetails (JSON) | true | Describes content of payload |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item |
  
## ContentDetails
  
Describes content of payload
  
| Name                  | Type            | Description                         |
|-----------------------|-----------------|-------------------------------------|
| dataFactoryResourceId | String          | Resource Id for Azure Data Factory. Refer [Description for dataFactoryResourceId Contents](#description-for-datafactoryresourceid-contents) and [ContentDetails Example](#contentdetails-example) for more details. |

### Description for dataFactoryResourceId Contents 

Describes the fields used to construct the dataFactoryResourceId. 

| Name                  | Type            | Description                         |
|-----------------------|-----------------|-------------------------------------|
| subscriptionId        | String (uuid)   | Subscription Id under which the data factory to be mounted exists in Azure Data Factory. |
| resourceGroup         | String          | Name of the resource group of which the data factory is a part in Azure Data Factory. |
| factoryName           | String          | Name of the already existing data factory to be mounted |

### ContentDetails example

```JSON
{
  "dataFactoryResourceId": "/subscriptions/<subscriptionId>/resourceGroups/<resourceGroup>/providers/Microsoft.DataFactory/factories/<factoryName>"
}
```
