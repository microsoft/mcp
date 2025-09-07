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

* [Create Item](https://learn.microsoft.com/en-us/rest/api/fabric/core/items/create-item) with definition - Respects the platform file if provided.
* [Get Item Definition](https://learn.microsoft.com/en-us/rest/api/fabric/core/items/get-item-definition) - Always returns the platform file.
* [Update Item Definition](https://learn.microsoft.com/en-us/rest/api/fabric/core/items/update-item-definition) - Accepts the platform file if provided, but only if you set a `updateMetadata=true` URL parameter.

For more information, see [Automatically generated system files](https://learn.microsoft.com/en-us/fabric/cicd/git-integration/source-code-format?tabs=v2#automatically-generated-system-files)

### Definition Details for Supported Item Types

* [Copy job definition](https://learn.microsoft.com/en-us/copyjob-definition.md)
* [Dataflow definition](https://learn.microsoft.com/en-us/dataflow-definition.md)
* [Eventhouse definition](https://learn.microsoft.com/en-us/eventhouse-definition.md)
* [API for GraphQL definition](https://learn.microsoft.com/en-us/graphql-api-definition.md)
* [Dataflow definition](https://learn.microsoft.com/en-us/dataflow-definition.md)
* [DataPipeline definition](https://learn.microsoft.com/en-us/datapipeline-definition.md)
* [HLSCohort definition](https://learn.microsoft.com/en-us/hlscohort-definition.md)
* [KQL Database definition](https://learn.microsoft.com/en-us/kql-database-definition.md)
* [KQL Dashboard definition](https://learn.microsoft.com/en-us/kql-dashboard-definition.md)
* [KQL Queryset definition](https://learn.microsoft.com/en-us/kql-queryset-definition.md)
* [Mirrored Azure Databricks Catalog definition](https://learn.microsoft.com/en-us/mirrored-azuredatabricks-unitycatalog-definition.md)
* [Mirrored database definition](https://learn.microsoft.com/en-us/mirrored-database-definition.md)
* [Mounted Data Factory definition](https://learn.microsoft.com/en-us/mounted-data-factory-definition.md)
* [Environment definition](https://learn.microsoft.com/en-us/environment-definition.md)
* [Notebook definition](https://learn.microsoft.com/en-us/notebook-definition.md)
* [Report definition](https://learn.microsoft.com/en-us/report-definition.md)
* [Semantic model definition](https://learn.microsoft.com/en-us/semantic-model-definition.md)
* [KQL Dashboard definition](https://learn.microsoft.com/en-us/kql-dashboard-definition.md)
* [Eventstream definition](https://learn.microsoft.com/en-us/eventstream-definition.md)
* [Reflex definition](https://learn.microsoft.com/en-us/reflex-definition.md)
* [Spark job definition](https://learn.microsoft.com/en-us/spark-job-definition.md)
* [Variable Library definition](https://learn.microsoft.com/en-us/variable-library-definition.md)
