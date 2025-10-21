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

* [Create Item](/rest/api/fabric/core/items/create-item) with definition - Respects the platform file if provided.
* [Get Item Definition](/rest/api/fabric/core/items/get-item-definition) - Always returns the platform file.
* [Update Item Definition](/rest/api/fabric/core/items/update-item-definition) - Accepts the platform file if provided, but only if you set a `updateMetadata=true` URL parameter.

For more information, see [Automatically generated system files](/fabric/cicd/git-integration/source-code-format?tabs=v2#automatically-generated-system-files)

### Definition Details for Supported Item Types

* [Copy job definition](copyjob-definition.md)
* [Dataflow definition](dataflow-definition.md)
* [Eventhouse definition](eventhouse-definition.md)
* [API for GraphQL definition](graphql-api-definition.md)
* [Dataflow definition](dataflow-definition.md)
* [DataPipeline definition](datapipeline-definition.md)
* [HLSCohort definition](hlscohort-definition.md)
* [KQL Database definition](kql-database-definition.md)
* [KQL Dashboard definition](kql-dashboard-definition.md)
* [KQL Queryset definition](kql-queryset-definition.md)
* [Mirrored Azure Databricks Catalog definition](mirrored-azuredatabricks-unitycatalog-definition.md)
* [Mirrored database definition](mirrored-database-definition.md)
* [Mounted Data Factory definition](mounted-data-factory-definition.md)
* [Environment definition](environment-definition.md)
* [Notebook definition](notebook-definition.md)
* [Report definition](report-definition.md)
* [Semantic model definition](semantic-model-definition.md)
* [KQL Dashboard definition](kql-dashboard-definition.md)
* [Eventstream definition](eventstream-definition.md)
* [Reflex definition](reflex-definition.md)
* [Spark job definition](spark-job-definition.md)
* [Variable Library definition](variable-library-definition.md)