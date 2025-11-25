---
title: Operations agent definition
description: Learn how to structure an operations agent item definition when using the Microsoft Fabric REST API.
ms.title: Microsoft Fabric operations agent item definition
author: jeromez
ms.author: jeromez
ms.service: fabric
ms.date: 09/18/2025
---

# Operations Agent definition

This article provides a breakdown of the definition structure for operations agent items.

## Definition parts

| Definition part path | type | Required | Description |
|--|--|--|--|
| `Configurations.json` | [OperationsAgentDefinition](#operationsagentdefinition-contents) (JSON) | true | Root definition containing configuration (goals, instructions, data sources, actions), playbook, and run state |

## OperationsAgentDefinition contents

Root shape of the definition payload.

| Name | Type | Required | Description |
|------|------|----------|-------------|
| configuration | [OperationsAgentConfiguration](#operationsagentconfiguration-contents) | true | User-authored configuration: goals, instructions, data sources, and actions |
| playbook | Object | true | Playbook definition (reserved for future expansion). Provide an empty object `{}` if not authored |
| shouldRun | Boolean | true | Whether the agent should currently run (`true`) or stay stopped (`false`) |

### OperationsAgentConfiguration contents

| Name | Type | Required | Description |
|------|------|----------|-------------|
| goals | String | true | Business goals for the agent to accomplish |
| instructions | String | true | Explicit instructions / procedures the agent should follow |
| dataSources | Object (dictionary of [DataSource](#datasource-contents)) | true | Map of user-chosen data source aliases to data source objects |
| actions | Object (dictionary of [Action](#action-contents)) | true | Map of user-chosen action aliases to action objects |
| recipient | String | false | Recipient of the operation |

Notes:

- The keys inside `dataSources` and `actions` are user-defined aliases (for example, `primaryKusto`, `notifyFlow`).
- Provide at least one data source and one action for a meaningful agent.

### DataSource contents

Represents a Fabric data source the agent can read from.

| Name | Type | Required | Description |
|------|------|----------|-------------|
| id | String (GUID) | true | Unique identifier for the data source reference |
| type | String (enum) | true | Data source type. Currently only `KustoDatabase` is supported |
| workspaceId | String (GUID) | true | Workspace ID containing the data source |

Supported `type` values:

- `KustoDatabase`

### Action contents

Represents an action the agent can invoke (for example, a Power Automate flow action).

| Name | Type | Required | Description |
|------|------|----------|-------------|
| id | String (GUID) | true | Unique identifier for the action reference |
| displayName | String | true | The display name for the action |
| description | String | true | The description for the action |
| kind | String (enum) | true | Action kind. Currently only `PowerAutomateAction` is supported |
| parameters | [Parameter](#parameter-contents)[] | false | Optional parameter metadata list |

Supported `kind` values:

- `PowerAutomateAction`

### Parameter contents

Metadata describing an action parameter (name & optional description). Doesn't include value binding (values are supplied operationally when the action runs or elsewhere in configuration not shown here).

| Name | Type | Required | Description |
|------|------|----------|-------------|
| name | String | true | Parameter name |
| description | String | false | Human-friendly description |

## OperationsAgentDefinition JSON skeleton

Minimal structural skeleton (showing required properties):

```json
{
  "configuration": {
    "goals": "<business goals>",
    "instructions": "<agent instructions>",
    "dataSources": {},
    "actions": {}
  },
  "playbook": {},
  "shouldRun": true
}
```

## OperationsAgentDefinition example

```json
{
  "configuration": {
    "goals": "Reduce manual triage of operations logs.",
    "instructions": "Monitor Kusto database metrics and trigger flow on thresholds.",
    "dataSources": {
      "primaryKusto": {
        "id": "11111111-2222-3333-4444-555555555555",
        "type": "KustoDatabase",
        "workspaceId": "66666666-7777-8888-9999-aaaaaaaaaaaa"
      }
    },
    "actions": {
      "notifyFlow": {
        "id": "bbbbbbbb-cccc-dddd-eeee-ffffffffffff",
        "kind": "PowerAutomateAction",
        "displayName": "PowerAutomateAlert",
        "description": "A PowerAutomate action to send out alert",
        "parameters": [
          { "name": "Severity", "description": "Alert severity level" }
        ]
      }
    },
    "recipient": "test@consoto.com"
  },
  "playbook": {},
  "shouldRun": true
}
```

## Notes

- All GUID strings must be valid UUIDs.
- Provide an empty object for `playbook` until extended schema details are published.
- Use concise, action-oriented statements in `goals` and imperative steps in `instructions`.
- Keys under `dataSources` and `actions` should be unique within their respective objects.
- Future schema versions may introduce more data source `type` values and action `kind` values; validation rejects unknown values.
