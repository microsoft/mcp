---
title: Overview of item definitions
description: This article explains item definitions and definition based Microsoft Fabric REST APIs.
ms.title: Microsoft Fabric item definition overview
author: billmath
ms.author: billmath
ms.service: fabric
ms.date: 11/12/2024
---

# Item definition overview

A Microsoft Fabric item definition is an object that represents the structure and format from which an item is built. A Fabric item definition includes the mandatory system files that define the item's characteristics. Each item type in Fabric has different supported formats and required files (parts) that make up its definition.

## Definition based APIs

Definition based APIs return a definition within the response or support providing the definition in the payload.

Definition based APIs include `Get Item Definition`, `Update Item Definition`, and `Create Item with Definition`.

## Platform file

The platform file is a definition part that contains the item's metadata information.

* [Create Item](https://learn.microsoft.com/rest/api/fabric/core/items/create-item) with definition - Respects the platform file if provided.
* [Get Item Definition](https://learn.microsoft.com/rest/api/fabric/core/items/get-item-definition) - Always returns the platform file.
* [Update Item Definition](https://learn.microsoft.com/rest/api/fabric/core/items/update-item-definition) - Accepts the platform file if provided, but only if you set a `updateMetadata=true` URL parameter.

For more information, see [Automatically generated system files](https://learn.microsoft.com/fabric/cicd/git-integration/source-code-format?tabs=v2#automatically-generated-system-files)

### Definition Details for Supported Item Types

* [Copy job definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/copyjob-definition)
* [Dataflow definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/dataflow-definition)
* [Eventhouse definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/eventhouse-definition)
* [API for GraphQL definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/graphql-api-definition)
* [Dataflow definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/dataflow-definition)
* [DataPipeline definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/datapipeline-definition)
* [HLSCohort definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/hlscohort-definition)
* [KQL Database definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/kql-database-definition)
* [KQL Dashboard definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/kql-dashboard-definition)
* [KQL Queryset definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/kql-queryset-definition)
* [Mirrored Azure Databricks Catalog definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/mirrored-azuredatabricks-unitycatalog-definition)
* [Mirrored database definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/mirrored-database-definition)
* [Mounted Data Factory definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/mounted-data-factory-definition)
* [Environment definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/environment-definition)
* [Notebook definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/notebook-definition)
* [Report definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/report-definition)
* [Semantic model definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/semantic-model-definition)
* [KQL Dashboard definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/kql-dashboard-definition)
* [Eventstream definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/eventstream-definition)
* [Reflex definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/reflex-definition)
* [Spark job definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/spark-job-definition)
* [Variable Library definition](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/variable-library-definition)
