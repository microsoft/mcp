# Operations Agent definition

This article provides a breakdown of the definition structure for operations agent items.

> [!WARNING]
> As of June 2026, we have updated the Operations Agent artifact definition schema to account for several feature enhancements for GA release. Among these updates are **two breaking changes**; importing an Operations Agent artifact definition with these two legacy fields may not work as expected. Please update the definition per the following guidance.
>
> **1. `Configuration.goals` property is deprecated.** To simplify the configuration, we have removed the `goals` property completely. You now provide the goals and instructions together in the `instructions` property.
>
> **2. `Configuration.recipient` property is deprecated and replaced with `messageDestination`.** We have recently enabled Teams channels as a destination for agent notifications. To support this, you can now specify whether the recipient should be a `Recipient` upn string, or a `teamId` and `channelId` for Teams channels. An example of `messageDestination` that continues to use a upn recipient:
>
> ```json
> { "messageDestination": { "kind": "Recipient", "recipient": "contoso@microsoft.com" } }
> ```


## Definition parts

| Definition part path | type | Required | Description |
|--|--|--|--|
| `Configurations.json` | [OperationsAgentDefinition](#operationsagentdefinition-contents) (JSON) | true | Root definition containing configuration (instructions, data sources, actions), playbook, and run state |

## OperationsAgentDefinition contents

Root shape of the definition payload.

| Name | Type | Required | Description |
|------|------|----------|-------------|
| configuration | [OperationsAgentConfiguration](#operationsagentconfiguration-contents) | true | User-authored configuration: instructions, data sources, and actions |
| playbook | Object | true | Playbook definition (reserved for future expansion). Provide an empty object `{}` if not authored |
| shouldRun | Boolean | true | Whether the agent should currently run (`true`) or stay stopped (`false`) |

### OperationsAgentConfiguration contents

| Name | Type | Required | Description |
|------|------|----------|-------------|
| instructions | String | true | Explicit instructions / procedures the agent should follow |
| dataSources | Object (dictionary of [DataSource](#datasource-contents) or [variable reference](#variable-references-in-datasources)) | true | Map of user-chosen data source aliases to data source objects (or variable reference strings) |
| actions | Object (dictionary of [Action](#action-contents)) | true | Map of user-chosen action aliases to action objects |
| messageDestination | [MessageDestination](#messagedestination-contents) | false | Where the agent sends notifications. Polymorphic — use the `kind` discriminator to select the destination type |
| identity | [AgentIdentity](#agentidentity-contents) | false | Identity information used when the agent executes |

Notes:

- The keys inside `dataSources` and `actions` are user-defined aliases (for example, `primaryKusto`, `notifyFlow`).
- Provide at least one data source and one action for a meaningful agent.

### DataSource contents

Represents a Fabric data source the agent can read from.

| Name | Type | Required | Description |
|------|------|----------|-------------|
| id | String (GUID) | true | Unique identifier for the data source reference |
| type | String (enum) | true | Data source type. See supported values below |
| workspaceId | String (GUID) | true | Workspace ID containing the data source |

Supported `type` values:

- `KustoDatabase`
- `Ontology`

#### Variable references in dataSources

In place of an inline `DataSource` object, a `dataSources` entry value can be a variable reference string that points at a variable in a Fabric variable library. The string must use the format `$(/<WorkspaceName>/<LibraryName>/<VariableName>)`.

Example:

```json
"dataSources": {
  "primaryKusto": "$(/MyWorkspace/MyLibrary/MyKustoDb)"
}
```

You can mix inline data source objects and variable references in the same `dataSources` map. The variable library item itself isn't part of the operations agent definition — deploy it separately (for example, through a Fabric deployment pipeline) so the referenced variables resolve in the target workspace.

### Action contents

Represents an action the agent can invoke. Action is polymorphic — the concrete shape is selected by the `kind` discriminator.

Common properties:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| id | String (GUID) | true | Unique identifier for the action reference |
| displayName | String | true | The display name for the action |
| description | String | true | The description for the action |
| kind | String (enum) | true | Action kind. See supported values below |
| connection | [FabricItemConnection](#fabricitemconnection-contents) | conditional | Required when `kind` is `FabricJobAction`. Specifies the Fabric item and job to invoke. Not used for other kinds |
| parameters | [Parameter](#parameter-contents)[] | false | Optional parameter metadata list |

Supported `kind` values:

- `PowerAutomateAction` — a Power Automate flow action. No extra required fields.
- `FabricJobAction` — runs a job on a Fabric item. Requires an additional `connection` object of type [FabricItemConnection](#fabricitemconnection-contents).

#### FabricItemConnection contents

Specifies the Fabric item and job to invoke for a `FabricJobAction`.

| Name | Type | Required | Description |
|------|------|----------|-------------|
| jobArtifactId | String (GUID) | true | Item ID of the Fabric item to run the job on |
| jobWorkspaceId | String (GUID) | true | Workspace ID containing the item |
| itemType | String | true | The Fabric item type (for example, `DataPipeline`, `Notebook`) |
| jobType | String | true | The job type to invoke on the item (for example, `Pipeline`, `RunNotebook`) |
| subItemId | String | false | Identifier for the sub-item being targeted by the job, when applicable |

### Parameter contents

Metadata describing an action parameter (name & optional description). Doesn't include value binding (values are supplied operationally when the action runs or elsewhere in configuration not shown here).

| Name | Type | Required | Description |
|------|------|----------|-------------|
| name | String | true | Parameter name |
| description | String | false | Human-friendly description |

### MessageDestination contents

Polymorphic destination for agent notifications. Use the `kind` discriminator to select the concrete type.

Supported `kind` values:

- `Recipient` — a specific user, identified by UPN.

  | Name | Type | Required | Description |
  |------|------|----------|-------------|
  | kind | String (const `Recipient`) | true | Discriminator |
  | recipient | String | true | The UPN of the recipient |

- `TeamsChannel` — a Microsoft Teams channel.

  | Name | Type | Required | Description |
  |------|------|----------|-------------|
  | kind | String (const `TeamsChannel`) | true | Discriminator |
  | teamId | String | true | The Teams team ID |
  | channelId | String | true | The Teams channel ID |

### AgentIdentity contents

Identity information for the agent. Used to support agent OBO / app-only scenarios on ALM import/export.

| Name | Type | Required | Description |
|------|------|----------|-------------|
| principalId | String | false | The principal ID for the agent identity |
| blueprintId | String | false | The agent identity blueprint ID |
| mode | String (enum) | false | Execution mode. Currently `Delegated` |
| sponsor | String | false | The UPN of the sponsor user. Required when `messageDestination` is a non-`Recipient` kind (for example, `TeamsChannel`), since the agent needs a user identity to act on behalf of |

## OperationsAgentDefinition JSON skeleton

Minimal structural skeleton (showing required properties):

```json
{
  "configuration": {
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
    "instructions": "Monitor our database metrics. If query timeouts exceed 5 in a minute, send an alert through Power Automate. If no new data arrives in a 24 hour period, 
run the remediation pipeline.",
    "dataSources": {
      "primaryKusto": {
        "id": "11111111-2222-3333-4444-555555555555",
        "type": "KustoDatabase",
        "workspaceId": "66666666-7777-8888-9999-aaaaaaaaaaaa"
      },
      "domainOntology": {
        "id": "22222222-3333-4444-5555-666666666666",
        "type": "Ontology",
        "workspaceId": "66666666-7777-8888-9999-aaaaaaaaaaaa"
      },
      "variableSourced": "$(/MyWorkspace/MyLibrary/MyKustoDb)"
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
      },
      "runPipeline": {
        "id": "cccccccc-dddd-eeee-ffff-000000000000",
        "kind": "FabricJobAction",
        "displayName": "Run remediation pipeline",
        "description": "Triggers a Fabric data pipeline job for remediation",
        "connection": {
          "jobArtifactId": "11111111-2222-3333-4444-555555555555",
          "jobWorkspaceId": "66666666-7777-8888-9999-aaaaaaaaaaaa",
          "itemType": "DataPipeline",
          "jobType": "Pipeline",
          "subItemId": "sub-1"
        },
        "parameters": [
          { "name": "incidentId", "description": "The incident to remediate" }
        ]
      }
    },
    "messageDestination": {
      "kind": "TeamsChannel",
      "teamId": "19:abcd1234@thread.tacv2",
      "channelId": "19:xyz5678@thread.tacv2"
    },
    "identity": {
      "principalId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
      "blueprintId": "ffffffff-0000-1111-2222-333333333333",
      "mode": "Delegated",
      "sponsor": "AdminUser01@contoso.onmicrosoft.com"
    }
  },
  "playbook": {},
  "shouldRun": true
}
```

## Notes

- All GUID strings must be valid UUIDs.
- Provide an empty object for `playbook` until extended schema details are published.
- Use concise, action-oriented, and imperative steps in `instructions`.
- Keys under `dataSources` and `actions` should be unique within their respective objects.
- Future schema versions may introduce more data source `type` values and action `kind` values; validation rejects unknown values.
