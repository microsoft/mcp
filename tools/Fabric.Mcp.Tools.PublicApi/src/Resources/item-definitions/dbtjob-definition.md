---
title: DataBuildToolJob item definition
description: Learn how to create a dbt job item definition when using the Microsoft Fabric REST API.
ms.title: DataBuildToolJob item definition
author: dylanb-microsoft
ms.author: dylanboliske
ms.service: fabric
ms.date: 10/28/2024
---
  
# DataBuildToolJob item definition

This article provides a breakdown of the definition structure for dbt job (DataBuildToolJob) items.

## Definition parts
  
This table lists the definition parts.
  
| Definition part path | type | Required | Description |
|--|--|--|--|
| `dbtjob-content.json` | [ContentDetails](#contentdetails) (JSON) | true | Describes properties and settings of the item like profile and operation |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item |
  
## ContentDetails
  
Describes content of payload
  
| Name                  | Type                | Description                         |
|-----------------------|---------------------|-------------------------------------|
| project            | [DbtJobProject](#description-for-dbtjobproject-contents) | dbt job item project settings.|
| profile            | [DbtJobProfile](#description-for-dbtjobprofile-contents) | dbt job item profile settings.|
| command            | [DbtJobCommand](#description-for-dbtjobcommand-contents) | dbt job item command settings.|

### Description for DbtJobProject contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| projectType           | String          | true            | Type of dbt project. Possible values: `OneLake`, `Lakehouse`. |
| folderPath            | String          | false           | Path to the dbt project folder. |
| connectionSettings    | [DbtJobConnectionSettings](#description-for-dbtjobconnectionsettings-contents) | true | Connection settings for the dbt project. |

### Description for DbtJobProfile contents

Describes the fields for the dbt profile. Here, depending on the type, either connectionSettings or externalReferences can be used.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| profileType           | String          | true            | Type of dbt profile. |
| schema                | String          | false           | Specifies the schema. |
| database              | String          | false           | Name of the database. |
| externalReferences     | [ExternalReferences](#description-for-externalreferences-contents) | false | Connection settings for the dbt profile. |
| connectionSettings     | [DbtJobConnectionSettings](#description-for-dbtjobconnectionsettings-contents) | false | Connection settings for the dbt profile. |

### Description for DbtJobCommand contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| operation           | String          | true            | Type of dbt command. Possible values: `run`, `build`, `show`, `seed`, `compile`, `test`, `snapshot`. |
| arguments              | [DbtJobCommandArgument](#description-for-dbtjobcommandargument-contents)         | false           | Other arguments for the dbt command. |

### Description for DbtJobCommandArgument contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| select              | String         | false           | Comma separated list of models to include. |
| exclude              | String         | false           | Comma separated list of models to exclude. |
| fullRefresh          | Boolean         | false           | Specifies if dbt should rebuild all models. |
| failFast              | Boolean         | false           | Specifies if dbt should exit as soon as a model fails. |
| threads              | Integer         | false           | Specifies the number of threads to use. |
| selectorName              | String         | false           | Specifies the selector to use. |

### Description for ExternalReferences contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| connection            | String (Guid)   | true            | Specifies the ID of the connection.|

### Description for DbtJobConnectionSettings contents

Describes the fields for Connection Settings.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| type                  | String          | true            | Describes the type of connection. |
| properties   | [DbtJobConnectionTypeProperties](#description-for-dbtjobconnectiontypeproperties-contents)          | true            | Describes the properties for the connection.|

### Description for DbtJobConnectionTypeProperties contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| workspaceId           | String (Guid)   | true           | Specifies the ID for the workspace in which the connected item exists.|
| artifactId            | String (Guid)   | true           | Specifies the ID for the connected item.|
| rootFolder            | String          | false           | Specifies the root folder.|
| endpoint              | String          | false           | Specifies the endpoint.|

### ContentDetails Example 1

```json
{
    "project": {
        "projectType": "OneLake",
        "connectionSettings": {
            "type": "OneLake"
        }
    },
    "profile": {
        "profileType": "DataWarehouse",
        "schema": "analytics_schema",
        "connectionSettings": {
            "type": "DataWarehouse",
            "properties": {
                "workspaceId": "00000000-0000-0000-0000-000000000000",
                "artifactId": "cccccccc-3333-4444-5555-dddddddddddd"
            }
        }
    },
    "command": {
        "operation": "run",
        "arguments": {
            "select": "model_a,model_b",
            "fullRefresh": true,
            "threads": 4
        }
    }
}
```

### ContentDetails Example 2

```json
{
    "project": {
        "projectType": "OneLake",
        "connectionSettings": {
            "type": "OneLake"
        }
    },
    "profile": {
        "profileType": "PostgreSql",
        "schema": "analytics_schema",
        "externalReferences": {
            "connection": "bbbbbbbb-9999-aaaa-bbbb-cccccccccccc"
        }
    },
    "command": {
        "operation": "build",
        "arguments": {
            "fullRefresh": true,
            "failFast": true
        }
    }
}
```
