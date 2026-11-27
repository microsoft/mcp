# Monitoring definition

This article provides a breakdown of the definition structure for Monitoring items. A Monitoring definition describes the workspace diagnostics to collect, their destinations, and the optional Operations Agent.

## Supported formats

The `MonitoringProperties.json` definition part uses JSON. The `format` value in the REST API definition envelope is `MonitoringV1`.

## Definition parts

This table lists the Monitoring definition parts.

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `MonitoringProperties.json` | [MonitoringProperties](#monitoringproperties) (JSON) | true | Defines data collection routing and the Operations Agent. Exactly one file is required. The maximum file size is 5 MiB (5,242,880 bytes). |
| `.platform` | PlatformDetails (JSON) | false | Describes the item's metadata. See [Platform file](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/item-definition-overview#platform-file). |

For REST API requests, encode each definition part's file content in Base64 and set its `payloadType` to `InlineBase64`. Use `MonitoringProperties.json` as the `path` of the Monitoring properties part. The JSON examples in this article show the decoded file content.

To update item metadata from a `.platform` part, include that part in the definition and set the Update Definition API's `updateMetadata` query parameter to `true`.

## Definition example

This definition envelope contains the [Data collection not configured](#data-collection-not-configured) example. The `payload` is the UTF-8 JSON content encoded in Base64, without whitespace.

```json
{
  "format": "MonitoringV1",
  "parts": [
    {
      "path": "MonitoringProperties.json",
      "payload": "eyIkc2NoZW1hIjoiaHR0cHM6Ly9kZXZlbG9wZXIubWljcm9zb2Z0LmNvbS9qc29uLXNjaGVtYXMvZmFicmljL2l0ZW0vbW9uaXRvcmluZy9kZWZpbml0aW9uL21vbml0b3JpbmdQcm9wZXJ0aWVzLzEuMC4wL3NjaGVtYS5qc29uIn0=",
      "payloadType": "InlineBase64"
    }
  ]
}
```

## MonitoringProperties

The root object in `MonitoringProperties.json`.

| Name | Type | Required | Description |
|---|---|---|---|
| `$schema` | String (URI) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/monitoring/definition/monitoringProperties/1.0.0/schema.json`. |
| `dataCollectionSettings` | [DataCollectionSetting](#datacollectionsetting)[] | false | Source-to-destination routing configuration. If present, the array must contain exactly one routing set. If omitted, data collection isn't configured. |
| `operationsAgent` | [OperationsAgent](#operationsagent) | false | Configures the Operations Agent. Requires `dataCollectionSettings` to be present, even when `enabled` is `false`. If omitted, the Operations Agent isn't enabled. |

Property names and enum values are case-sensitive. The Monitoring configuration objects defined in this article don't accept additional properties.

Configuring a collection route doesn't indicate whether data collection is currently enabled. The definition describes configuration, not the runtime collection state. The read-only `dataCollectionState` property returned by Get Monitoring and List Monitoring Items is changed through the enable and disable data collection operations, not by changing the definition. Disabling data collection leaves the definition unchanged.

### DataCollectionSetting

A routing set pairs the diagnostic sources with their destinations.

| Name | Type | Required | Description |
|---|---|---|---|
| `sources` | [MonitoringSources](#monitoringsources) | true | The diagnostic signals to collect. |
| `destinations` | [MonitoringDestinations](#monitoringdestinations) | true | The stores that receive the collected diagnostic signals. |

### MonitoringSources

| Name | Type | Required | Description |
|---|---|---|---|
| `workspaceDiagnostics` | [WorkspaceDiagnosticsSource](#workspacediagnosticssource) | true | Collects workspace diagnostics from supported items in the workspace. |

### WorkspaceDiagnosticsSource

| Name | Type | Required | Description |
|---|---|---|---|
| `mode` | String (enum) | true | The collection scope. The only supported value is `All`, which collects diagnostics from every supported item in the workspace. |

### MonitoringDestinations

| Name | Type | Required | Description |
|---|---|---|---|
| `eventhouse` | [EventhouseDestination](#eventhousedestination) | true | The Eventhouse (KQL) destination. |
| `eventStream` | [EventStreamDestination](#eventstreamdestination) | false | An additional Eventstream destination. Supported with both `Auto` and `Linked` Eventhouse modes. |

### EventhouseDestination

Specify either an [AutoEventhouseDestination](#autoeventhousedestination) or a [LinkedEventhouseDestination](#linkedeventhousedestination). The `mode` value selects the destination type.

Once the Eventhouse destination is configured, you can't change or remove its mode or, for `Linked` mode, its parent Monitoring reference.

### AutoEventhouseDestination

The Monitoring item provisions and owns the Eventhouse as a child item.

| Name | Type | Required | Description |
|---|---|---|---|
| `mode` | String | true | Must be `Auto`. Don't include `parentMonitoringItem` in this mode. |

### LinkedEventhouseDestination

The Monitoring item routes diagnostics to an Eventhouse owned by another Monitoring item. Deleting the linking Monitoring item doesn't delete the referenced Eventhouse.

| Name | Type | Required | Description |
|---|---|---|---|
| `mode` | String | true | Must be `Linked`. |
| `parentMonitoringItem` | [ItemReference](#itemreference) | true | Identifies the Monitoring item that owns the destination Eventhouse. The Eventhouse is resolved from that Monitoring item. Don't supply the Eventhouse item's ID. |

### ItemReference

Identifies the parent Monitoring item.

| Name | Type | Required | Description |
|---|---|---|---|
| `itemId` | String (UUID) | true | ID of the Monitoring item that owns the destination Eventhouse. |
| `workspaceId` | String (UUID) | true | ID of the workspace that contains the parent Monitoring item. |

### EventStreamDestination

| Name | Type | Required | Description |
|---|---|---|---|
| `customEndpoint` | [CustomEndpointDestination](#customendpointdestination) | true | Configures streaming to an external custom endpoint. |

### CustomEndpointDestination

| Name | Type | Required | Description |
|---|---|---|---|
| `enabled` | Boolean | true | Set to `true` to enable streaming to the external custom endpoint, or `false` to turn it off. Streaming is off when the containing `eventStream` object is absent. |

If you include `eventStream`, you must include `customEndpoint` and an explicit Boolean `enabled` value. Empty objects and `null` aren't supported.

### OperationsAgent

The Operations Agent is provisioned as a child of the Monitoring item.

| Name | Type | Required | Description |
|---|---|---|---|
| `enabled` | Boolean | true | Whether to enable the Operations Agent. Both `true` and `false` are valid values. The Operations Agent can be enabled later, but once enabled, it can't be disabled or removed from the definition. |

## MonitoringProperties examples

Each example is the complete decoded content of `MonitoringProperties.json`.

### Data collection not configured

Omit both `dataCollectionSettings` and `operationsAgent` to create a definition without data collection configuration.

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/monitoring/definition/monitoringProperties/1.0.0/schema.json"
}
```

### Automatically provisioned Eventhouse with Operations Agent

This definition configures workspace diagnostics with an Eventhouse owned by this Monitoring item and enables the Operations Agent.

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/monitoring/definition/monitoringProperties/1.0.0/schema.json",
  "dataCollectionSettings": [
    {
      "sources": {
        "workspaceDiagnostics": {
          "mode": "All"
        }
      },
      "destinations": {
        "eventhouse": {
          "mode": "Auto"
        }
      }
    }
  ],
  "operationsAgent": {
    "enabled": true
  }
}
```

### Automatically provisioned Eventhouse with Eventstream

This definition adds an Eventstream destination with custom endpoint streaming enabled.

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/monitoring/definition/monitoringProperties/1.0.0/schema.json",
  "dataCollectionSettings": [
    {
      "sources": {
        "workspaceDiagnostics": {
          "mode": "All"
        }
      },
      "destinations": {
        "eventhouse": {
          "mode": "Auto"
        },
        "eventStream": {
          "customEndpoint": {
            "enabled": true
          }
        }
      }
    }
  ]
}
```

### Linked Eventhouse with Eventstream and Operations Agent

In this example, `22222222-2222-2222-2222-222222222222` identifies the parent Monitoring item, and `11111111-1111-1111-1111-111111111111` identifies its workspace. Replace these IDs with the IDs of your parent Monitoring item and workspace.

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/monitoring/definition/monitoringProperties/1.0.0/schema.json",
  "dataCollectionSettings": [
    {
      "sources": {
        "workspaceDiagnostics": {
          "mode": "All"
        }
      },
      "destinations": {
        "eventhouse": {
          "mode": "Linked",
          "parentMonitoringItem": {
            "workspaceId": "11111111-1111-1111-1111-111111111111",
            "itemId": "22222222-2222-2222-2222-222222222222"
          }
        },
        "eventStream": {
          "customEndpoint": {
            "enabled": true
          }
        }
      }
    }
  ],
  "operationsAgent": {
    "enabled": true
  }
}
```

### Custom endpoint streaming disabled

Use an explicit `false` value to turn off custom endpoint streaming while retaining the Eventhouse route. This example uses `Auto` mode; retain your existing destination mode and parent Monitoring item when updating an already configured item.

If the Operations Agent is already enabled, also include `"operationsAgent": {"enabled": true}` in the definition.

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/monitoring/definition/monitoringProperties/1.0.0/schema.json",
  "dataCollectionSettings": [
    {
      "sources": {
        "workspaceDiagnostics": {
          "mode": "All"
        }
      },
      "destinations": {
        "eventhouse": {
          "mode": "Auto"
        },
        "eventStream": {
          "customEndpoint": {
            "enabled": false
          }
        }
      }
    }
  ]
}
```
