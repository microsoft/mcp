# Reflex definition

This article provides a breakdown of the definition structure for Reflex items. Use this reference when creating or updating Reflex items programmatically through the [Create Item](https://learn.microsoft.com/rest/api/fabric/core/items/create-item), [Get Item Definition](https://learn.microsoft.com/rest/api/fabric/core/items/get-item-definition), or [Update Item Definition](https://learn.microsoft.com/rest/api/fabric/core/items/update-item-definition) APIs.

> [!NOTE]
> Reflex is also known as **Activator**. Both names refer to the same item type throughout this article.

## Overview

A Reflex (Activator) item monitors streaming data and triggers notifications or actions when specific conditions are met. The definition describes a processing pipeline composed of interconnected entities:

1. **Containers** organize related components into logical groups and provide a hierarchical structure.
1. **Data sources** connect to streaming data (simulators, KQL queries, Real-time Hub, or Eventstreams).
1. **Events** filter and transform the raw data from sources.
1. **Objects** represent the real-world entities you're monitoring (for example, packages, sensors, or users).
1. **Attributes** extract or compute properties from events for each object.
1. **Rules** define conditions that trigger actions like Teams messages, emails, or Fabric item executions.

The following hierarchy illustrates the relationship between these entities:

``` ascii
Container
├── Data Source (simulator, KQL, Real-time Hub, or Eventstream)
├── Event View (selects/transforms events from the data source)
├── Object View (represents an entity, e.g., "Package")
│   ├── Attribute View (identity field, e.g., "PackageId")
│   ├── Attribute View (measured value, e.g., "Temperature")
│   └── Rule View (trigger + action, e.g., "Too hot → send Teams message")
└── Action (optional Fabric item reference, e.g., a Pipeline to invoke)
```

Entities reference each other by `uniqueIdentifier`, which you use to wire data sources to events, events to objects, and objects to rules.

## Supported formats

Reflex items support the `json` format.

## Definition parts

The definition of a Reflex item consists of the following parts:

| Definition part path | Type | Required | Description |
| -------------------- | ---- | -------- | ----------- |
| `ReflexEntities.json` | [ReflexEntities](#reflexentities-structure) (JSON) | true | Contains the complete Activator configuration including containers, data sources, objects, attributes, and rules. |
| `.platform` | Platform metadata (JSON) | false | Contains Fabric platform metadata information. |

Each definition part is constructed as follows:

* **Path**: The file name, for example: `ReflexEntities.json`
* **Payload type**: InlineBase64
* **Payload**: Base64-encoded JSON content. See the [decoded payload examples](#examples-per-entity-type) later in this article.

## ReflexEntities structure

The `ReflexEntities.json` part contains a JSON **array** of entity objects. Each entity represents a component in the streaming data pipeline. When decoded from Base64, the content has the following top-level structure:

```json
[
  { "uniqueIdentifier": "<guid>", "payload": { ... }, "type": "<entity-type>" },
  { "uniqueIdentifier": "<guid>", "payload": { ... }, "type": "<entity-type>" }
]
```

### Entity types

The following entity types are supported:

| Type | Description |
| ---- | ----------- |
| `container-v1` | Top-level container that organizes related components. |
| `simulatorSource-v1` | Simulated data source for testing and demonstrations. |
| `kqlSource-v1` | KQL query-based data source connected to an Eventhouse. |
| `realTimeHubSource-v1` | Real-time Hub data source for workspace events. |
| `eventstreamSource-v1` | Eventstream data source connected to a Fabric Eventstream. |
| `fabricItemAction-v1` | Fabric item action that rules can invoke. |
| `timeSeriesView-v1` | Core building block that defines events, objects, attributes, or rules. |

### Common entity properties

All entities share these top-level properties:

| Property | Type | Required | Description |
| -------- | ---- | -------- | ----------- |
| `uniqueIdentifier` | string (GUID) | true | Unique identifier for the entity. Other entities reference this value to establish relationships. |
| `payload` | object | true | Entity-specific configuration data. The structure varies by entity type. |
| `type` | string | true | The entity type (for example, `container-v1`, `timeSeriesView-v1`). |

### Parent references

Entities reference their parents using two properties:

| Property | Type | Description |
| -------- | ---- | ----------- |
| `parentContainer.targetUniqueIdentifier` | string (GUID) | References a `container-v1` entity. Present on all entities except the container itself. |
| `parentObject.targetUniqueIdentifier` | string (GUID) | References an `Object` time series view. Present on attribute and rule views to associate them with the object they belong to. |

Both properties use the `uniqueIdentifier` of the target entity to establish the relationship.

### Container entity (`container-v1`)

Containers organize related components into logical groups within the Reflex definition and provide a hierarchical structure. Every other entity references a container through its `parentContainer` property.

**Payload properties:**

| Property | Type | Required | Description |
| -------- | ---- | -------- | ----------- |
| `name` | string | true | Display name of the container. |
| `type` | string | true | Container classification. Examples: `samples`, `kqlQueries`. |

**Example:**

```json
{
  "uniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee",
  "payload": {
    "name": "Package delivery sample",
    "type": "samples"
  },
  "type": "container-v1"
}
```

### Data source entities

Data source entities define where the Reflex item receives its streaming data. They define the data sources to which the item connects and from which it receives data. The entity type determines the connection configuration required. The following data source types are supported:

| Source type | Description |
| ----------- | ----------- |
| [Simulator source (`simulatorSource-v1`)](#simulator-source-simulatorsource-v1) | Generates simulated data for testing. |
| [KQL source (`kqlSource-v1`)](#kql-source-kqlsource-v1) | Queries data from an Eventhouse KQL database. |
| [Real-time Hub source (`realTimeHubSource-v1`)](#real-time-hub-source-realtimehubsource-v1) | Monitors workspace events from Real-time Hub. |
| [Eventstream source (`eventstreamSource-v1`)](#eventstream-source-eventstreamsource-v1) | Streams events from a Fabric Eventstream. |

#### Simulator source (`simulatorSource-v1`)

Generates simulated streaming data for testing purposes. This source type is useful for prototyping rules without connecting to a live data source.

The following table describes the properties of the `simulatorSource-v1` payload:

| Property | Type | Required | Description |
| -------- | ---- | -------- | ----------- |
| `name` | string | true | Display name of the data source. |
| `runSettings` | object | false | Execution configuration. |
| `runSettings.startTime` | string (ISO 8601) | false | When to start generating data. |
| `runSettings.stopTime` | string (ISO 8601) | false | When to stop generating data. |
| `version` | string | false | Version identifier (for example, `V2_0`). |
| `type` | string | true | Type of simulated data (for example, `PackageShipment`). |
| `parentContainer` | object | true | Reference to parent container. |
| `parentContainer.targetUniqueIdentifier` | string (GUID) | true | `uniqueIdentifier` of the parent container entity. |

The following example shows a simulator source that generates package delivery events between October 21, 2025, and November 4, 2025:

```json
{
  "uniqueIdentifier": "11bb11bb-cc22-dd33-ee44-55ff55ff55ff",
  "payload": {
    "name": "Package delivery",
    "runSettings": {
      "startTime": "2025-10-21T12:03:31.9271568Z",
      "stopTime": "2025-11-04T15:03:31.03Z"
    },
    "version": "V2_0",
    "type": "PackageShipment",
    "parentContainer": {
      "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
    }
  },
  "type": "simulatorSource-v1"
}
```

#### KQL source (`kqlSource-v1`)

Connects to a KQL database in an Eventhouse to query streaming data on a recurring interval.

The following table describes the properties of the `kqlSource-v1` payload:

| Property | Type | Required | Description |
| -------- | ---- | -------- | ----------- |
| `name` | string | true | Display name of the data source. |
| `runSettings` | object | false | Query execution settings. |
| `runSettings.executionIntervalInSeconds` | number | false | How often to execute the query, in seconds. |
| `query.queryString` | string | true | The KQL query to execute. |
| `eventhouseItem` | object | true | Reference to the Eventhouse item. |
| `eventhouseItem.targetUniqueIdentifier` | string (GUID) | true | `uniqueIdentifier` of the Eventhouse item. |
| `parentContainer` | object | true | Reference to parent container. |
| `parentContainer.targetUniqueIdentifier` | string (GUID) | true | `uniqueIdentifier` of the parent container entity. |

The following example shows a KQL source that queries an Eventhouse every 60 seconds for recent sensor data:

```json
{
  "uniqueIdentifier": "cccccccc-2222-3333-4444-dddddddddddd",
  "payload": {
    "name": "Sensor telemetry query",
    "runSettings": {
      "executionIntervalInSeconds": 60
    },
    "query": {
      "queryString": "SensorData | where Timestamp > ago(5m)"
    },
    "eventhouseItem": {
      "targetUniqueIdentifier": "d3d3d3d3-eeee-ffff-aaaa-b4b4b4b4b4b4"
    },
    "parentContainer": {
      "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
    }
  },
  "type": "kqlSource-v1"
}
```

#### Real-time Hub source (`realTimeHubSource-v1`)

Connects to a Real-time Hub data source for monitoring workspace events such as item creation, updates, or deletions.

The following table describes the properties of the `realTimeHubSource-v1` payload:

| Property | Type | Required | Description |
| -------- | ---- | -------- | ----------- |
| `name` | string | true | Display name of the data source. |
| `connection` | object | true | Real-time Hub connection configuration. |
| `connection.scope` | string | true | Scope of events (for example, `Workspace`). |
| `connection.tenantId` | string (GUID) | true | Azure tenant identifier. |
| `connection.workspaceId` | string (GUID) | true | Fabric workspace identifier. |
| `connection.eventGroupType` | string | true | Type of event group (for example, `Microsoft.Fabric.WorkspaceEvents`). |
| `filterSettings` | object | true | Event filtering configuration. |
| `filterSettings.eventTypes` | array | true | Array of event types to monitor. |
| `filterSettings.eventTypes[].name` | string | true | Event type name (for example, `Microsoft.Fabric.ItemCreateSucceeded`). |
| `filterSettings.filters` | array | false | Additional filters to apply. |
| `parentContainer` | object | true | Reference to parent container. |
| `parentContainer.targetUniqueIdentifier` | string (GUID) | true | `uniqueIdentifier` of the parent container entity. |

The following example shows a Real-time Hub source that monitors item creation and update events in a workspace:

```json
{
  "uniqueIdentifier": "dddddddd-3333-4444-5555-eeeeeeeeeeee",
  "payload": {
    "name": "Workspace event monitor",
    "connection": {
      "scope": "Workspace",
      "tenantId": "aaaabbbb-0000-cccc-1111-dddd2222eeee",
      "workspaceId": "a0a0a0a0-bbbb-cccc-dddd-e1e1e1e1e1e1",
      "eventGroupType": "Microsoft.Fabric.WorkspaceEvents"
    },
    "filterSettings": {
      "eventTypes": [
        { "name": "Microsoft.Fabric.ItemCreateSucceeded" },
        { "name": "Microsoft.Fabric.ItemUpdateSucceeded" }
      ],
      "filters": []
    },
    "parentContainer": {
      "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
    }
  },
  "type": "realTimeHubSource-v1"
}
```

#### Eventstream source (`eventstreamSource-v1`)

Connects to a Fabric Eventstream item as a data source for streaming events.

The following table describes the properties of the `eventstreamSource-v1` payload:

| Property | Type | Required | Description |
| -------- | ---- | -------- | ----------- |
| `name` | string | true | Display name of the data source. |
| `metadata` | object | true | Eventstream connection metadata. |
| `metadata.eventstreamArtifactId` | string (GUID) | true | Identifier of the Fabric Eventstream artifact. |
| `parentContainer` | object | true | Reference to parent container. |
| `parentContainer.targetUniqueIdentifier` | string (GUID) | true | `uniqueIdentifier` of the parent container entity. |

The following example shows an eventstream source connected to a Fabric Eventstream:

```json
{
  "uniqueIdentifier": "eeeeeeee-4444-5555-6666-ffffffffffff",
  "payload": {
    "name": "IoT device stream",
    "metadata": {
      "eventstreamArtifactId": "c2c2c2c2-dddd-eeee-ffff-a3a3a3a3a3a3"
    },
    "parentContainer": {
      "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
    }
  },
  "type": "eventstreamSource-v1"
}
```

### Action entities

#### Fabric item action (`fabricItemAction-v1`)

Defines a Fabric item (such as a pipeline or notebook) that rules can invoke as an action. After you define a Fabric item action entity, rules can reference its `uniqueIdentifier` to execute the item when conditions are met.

The following table describes the properties of the `fabricItemAction-v1` payload:

| Property | Type | Required | Description |
| -------- | ---- | -------- | ----------- |
| `name` | string | true | Display name of the action. |
| `fabricItem` | object | true | Reference to the Fabric item to execute. |
| `fabricItem.itemId` | string (GUID) | true | Fabric item identifier. |
| `fabricItem.workspaceId` | string (GUID) | true | Workspace containing the item. |
| `fabricItem.itemType` | string | true | Type of Fabric item (for example, `Pipeline`). |
| `jobType` | string | true | Type of job to execute (for example, `Pipeline`). |
| `parentContainer` | object | true | Reference to parent container. |
| `parentContainer.targetUniqueIdentifier` | string (GUID) | true | `uniqueIdentifier` of the parent container entity. |

The following example shows a Fabric item action that references a pipeline to run when a rule triggers it:

```json
{
  "uniqueIdentifier": "ffffffff-5555-6666-7777-aaaaaaaaaaaa",
  "payload": {
    "name": "Run alert pipeline",
    "fabricItem": {
      "itemId": "b1b1b1b1-cccc-dddd-eeee-f2f2f2f2f2f2",
      "workspaceId": "a0a0a0a0-bbbb-cccc-dddd-e1e1e1e1e1e1",
      "itemType": "Pipeline"
    },
    "jobType": "Pipeline",
    "parentContainer": {
      "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
    }
  },
  "type": "fabricItemAction-v1"
}
```

### Time series view entities (`timeSeriesView-v1`)

Time series views are the core building blocks that define how data flows through the Activator pipeline. A time series view can represent [events](#event-views), [objects](#object-views), [attributes](#attribute-views), or [rules](#rule-views).

#### Common time series view properties

All time series view entities share these payload properties:

| Property | Type | Required | Description |
| -------- | ---- | -------- | ----------- |
| `name` | string | true | Display name of the view. |
| `parentContainer` | object | true | Reference to parent container. |
| `parentContainer.targetUniqueIdentifier` | string (GUID) | true | `uniqueIdentifier` of the parent container entity. |
| `definition` | object | true | Defines the view's behavior and logic. |
| `definition.type` | string | true | Type of view: `Event`, `Object`, `Attribute`, or `Rule`. |
| `definition.instance` | string | false | A JSON-encoded string containing template-specific configuration. See [Template instances](#template-instances). |

#### Event views

Event views filter and transform streaming data from a source to create a new eventstream. They act as the bridge between a data source entity and the rest of the pipeline.

The following table describes the properties specific to event views:

| Property | Type | Required | Description |
| -------- | ---- | -------- | ----------- |
| `definition.type` | string | true | Must be `Event`. |
| `definition.instance` | string | true | JSON-encoded configuration for event processing templates. |

The following table describes common templates used for event views:

| Template | Description |
| -------- | ----------- |
| `SourceEvent` | Selects events directly from a data source. |
| `SplitEvent` | Splits events by object identity for per-object processing. |

The following example shows an event view that selects events from a simulator source:

```json
{
  "uniqueIdentifier": "22cc22cc-dd33-ee44-ff55-66aa66aa66aa",
  "payload": {
    "name": "Package delivery events",
    "parentContainer": {
      "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
    },
    "definition": {
      "type": "Event",
      "instance": "{\"templateId\":\"SourceEvent\",\"templateVersion\":\"1.1\",\"steps\":[{\"name\":\"SourceEventStep\",\"id\":\"aaaa0000-bb11-2222-33cc-444444dddddd\",\"rows\":[{\"name\":\"SourceSelector\",\"kind\":\"SourceReference\",\"arguments\":[{\"name\":\"entityId\",\"type\":\"string\",\"value\":\"11bb11bb-cc22-dd33-ee44-55ff55ff55ff\"}]}]}]}"
    }
  },
  "type": "timeSeriesView-v1"
}
```

In the `instance` property, the `entityId` value `11bb11bb-...` references the `uniqueIdentifier` of the simulator source entity, connecting this event view to that data source.

#### Object views

Object views define the real-world entities you're monitoring (for example, packages, sensors, or users). Objects serve as the parent for attributes and rules.

The following table describes the properties specific to object views:

| Property | Type | Required | Description |
| -------- | ---- | -------- | ----------- |
| `definition.type` | string | true | Must be `Object`. |

The following example shows an object view representing a package:

```json
{
  "uniqueIdentifier": "33dd33dd-ee44-ff55-aa66-77bb77bb77bb",
  "payload": {
    "name": "Package",
    "parentContainer": {
      "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
    },
    "definition": {
      "type": "Object"
    }
  },
  "type": "timeSeriesView-v1"
}
```

#### Attribute views

Attribute views extract and compute properties from events. Attributes define what data to monitor for each object. Every attribute belongs to a parent object, referenced through `parentObject`.

The following table describes the properties specific to attribute views:

| Property | Type | Required | Description |
| -------- | ---- | -------- | ----------- |
| `definition.type` | string | true | Must be `Attribute`. |
| `definition.instance` | string | true | JSON-encoded configuration for attribute extraction and computation. |
| `parentObject` | object | true | Reference to the parent object this attribute belongs to. |
| `parentObject.targetUniqueIdentifier` | string (GUID) | true | `uniqueIdentifier` of the parent object entity. |

The following table describes common templates used for attribute views:

| Template | Description |
| -------- | ----------- |
| `IdentityPartAttribute` | Defines part of an object's identity (for example, a package ID field). |
| `IdentityTupleAttribute` | Combines multiple identity parts into a complete object identifier. |
| `BasicEventAttribute` | Extracts a simple value from an event field (for example, temperature). |

The following example shows an identity attribute that extracts `PackageId` from event data:

```json
{
  "uniqueIdentifier": "44ee44ee-ff55-aa66-bb77-88cc88cc88cc",
  "payload": {
    "name": "PackageId",
    "parentObject": {
      "targetUniqueIdentifier": "33dd33dd-ee44-ff55-aa66-77bb77bb77bb"
    },
    "parentContainer": {
      "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
    },
    "definition": {
      "type": "Attribute",
      "instance": "{\"templateId\":\"IdentityPartAttribute\",\"templateVersion\":\"1.1\",\"steps\":[{\"name\":\"IdPartStep\",\"id\":\"bbbb1111-cc22-3333-44dd-555555eeeeee\",\"rows\":[{\"name\":\"TypeAssertion\",\"kind\":\"TypeAssertion\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"Text\"},{\"name\":\"format\",\"type\":\"string\",\"value\":\"\"}]}]}]}"
    }
  },
  "type": "timeSeriesView-v1"
}
```

The following example shows a basic event attribute that extracts the `Temperature` field:

```json
{
  "uniqueIdentifier": "55ff55ff-aa66-bb77-cc88-99dd99dd99dd",
  "payload": {
    "name": "Temperature (°C)",
    "parentObject": {
      "targetUniqueIdentifier": "33dd33dd-ee44-ff55-aa66-77bb77bb77bb"
    },
    "parentContainer": {
      "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
    },
    "definition": {
      "type": "Attribute",
      "instance": "{\"templateId\":\"BasicEventAttribute\",\"templateVersion\":\"1.1\",\"steps\":[{\"name\":\"EventSelectStep\",\"id\":\"cccc2222-dd33-4444-55ee-666666ffffff\",\"rows\":[{\"name\":\"EventSelector\",\"kind\":\"Event\",\"arguments\":[{\"kind\":\"EventReference\",\"type\":\"complex\",\"arguments\":[{\"name\":\"entityId\",\"type\":\"string\",\"value\":\"aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb\"}],\"name\":\"event\"}]},{\"name\":\"EventFieldSelector\",\"kind\":\"EventField\",\"arguments\":[{\"name\":\"fieldName\",\"type\":\"string\",\"value\":\"Temperature\"}]}]},{\"name\":\"EventComputeStep\",\"id\":\"dddd3333-ee44-5555-66ff-777777aaaaaa\",\"rows\":[{\"name\":\"TypeAssertion\",\"kind\":\"TypeAssertion\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"Number\"},{\"name\":\"format\",\"type\":\"string\",\"value\":\"\"}]}]}]}"
    }
  },
  "type": "timeSeriesView-v1"
}
```

#### Rule views

Rule views define conditions (triggers) and the actions to execute when those conditions are met. You can scope rules to a specific object through `parentObject`.

**Specific properties:**

| Property | Type | Required | Description |
| -------- | ---- | -------- | ----------- |
| `definition.type` | string | true | Must be `Rule`. |
| `definition.instance` | string | true | JSON-encoded configuration for trigger logic and actions. |
| `definition.settings` | object | false | Rule execution settings. |
| `definition.settings.shouldRun` | boolean | false | Indicates whether the rule is currently active. Set to `true` to enable. |
| `definition.settings.shouldApplyRuleOnUpdate` | boolean | false | Whether to apply the rule to historical data on update. |
| `parentObject` | object | false | Reference to parent object (for object-scoped rules). |
| `parentObject.targetUniqueIdentifier` | string (GUID) | false | `uniqueIdentifier` of the parent object entity. |

#### Rule templates

The following table describes common templates used for rule views:

| Template | Description |
| -------- | ----------- |
| `EventTrigger` | Triggers whenever a specific event occurs. |
| `AttributeTrigger` | Triggers when an attribute value meets a condition (for example, exceeds a threshold). |

The following example shows a rule that sends a Teams notification when the average temperature exceeds 20°C:

```json
{
  "uniqueIdentifier": "66aa66aa-bb77-cc88-dd99-00ee00ee00ee",
  "payload": {
    "name": "Too hot for medicine",
    "parentObject": {
      "targetUniqueIdentifier": "33dd33dd-ee44-ff55-aa66-77bb77bb77bb"
    },
    "parentContainer": {
      "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
    },
    "definition": {
      "type": "Rule",
      "instance": "{\"templateId\":\"AttributeTrigger\",\"templateVersion\":\"1.1\",\"steps\":[{\"name\":\"ScalarSelectStep\",\"id\":\"eeee4444-ff55-6666-77aa-888888bbbbbb\",\"rows\":[{\"name\":\"AttributeSelector\",\"kind\":\"Attribute\",\"arguments\":[{\"kind\":\"AttributeReference\",\"type\":\"complex\",\"arguments\":[{\"name\":\"entityId\",\"type\":\"string\",\"value\":\"55ff55ff-aa66-bb77-cc88-99dd99dd99dd\"}],\"name\":\"attribute\"}]},{\"name\":\"NumberSummary\",\"kind\":\"NumberSummary\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"Average\"},{\"kind\":\"TimeDrivenWindowSpec\",\"type\":\"complex\",\"arguments\":[{\"name\":\"width\",\"type\":\"timeSpan\",\"value\":600000.0},{\"name\":\"hop\",\"type\":\"timeSpan\",\"value\":600000.0}],\"name\":\"window\"}]}]},{\"name\":\"ScalarDetectStep\",\"id\":\"ffff5555-aa66-7777-88bb-999999cccccc\",\"rows\":[{\"name\":\"NumberBecomes\",\"kind\":\"NumberBecomes\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"BecomesGreaterThan\"},{\"name\":\"value\",\"type\":\"number\",\"value\":20.0}]},{\"name\":\"OccurrenceOption\",\"kind\":\"EachTime\",\"arguments\":[]}]},{\"name\":\"DimensionalFilterStep\",\"id\":\"aaaa6666-bb77-8888-99cc-000000dddddd\",\"rows\":[{\"name\":\"AttributeSelector\",\"kind\":\"Attribute\",\"arguments\":[{\"kind\":\"AttributeReference\",\"type\":\"complex\",\"arguments\":[{\"name\":\"entityId\",\"type\":\"string\",\"value\":\"bbbbbbbb-1111-2222-3333-cccccccccccc\"}],\"name\":\"attribute\"}]},{\"name\":\"TextValueCondition\",\"kind\":\"TextValueCondition\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"IsEqualTo\"},{\"name\":\"value\",\"type\":\"string\",\"value\":\"Medicine\"}]}]},{\"name\":\"ActStep\",\"id\":\"0000aaaa-11bb-cccc-dd22-eeeeee333333\",\"rows\":[{\"name\":\"TeamsBinding\",\"kind\":\"TeamsMessage\",\"arguments\":[{\"name\":\"messageLocale\",\"type\":\"string\",\"value\":\"\"},{\"name\":\"recipients\",\"type\":\"array\",\"values\":[{\"type\":\"string\",\"value\":\"user@example.com\"}]},{\"name\":\"headline\",\"type\":\"array\",\"values\":[{\"type\":\"string\",\"value\":\"Package too hot for medicine\"}]},{\"name\":\"optionalMessage\",\"type\":\"array\",\"values\":[{\"type\":\"string\",\"value\":\"This temperature-sensitive package containing medicine has exceeded the allowed threshold.\"}]},{\"name\":\"additionalInformation\",\"type\":\"array\",\"values\":[]}]}]}]}",
      "settings": {
        "shouldRun": false,
        "shouldApplyRuleOnUpdate": false
      }
    }
  },
  "type": "timeSeriesView-v1"
}
```

In this example, the rule's `instance` JSON defines four steps:

1. **ScalarSelectStep** selects the `Temperature (°C)` attribute (by `entityId`) and computes a 10-minute rolling average.
1. **ScalarDetectStep** triggers when the average becomes greater than 20.
1. **DimensionalFilterStep** filters to packages where the item type equals `Medicine`.
1. **ActStep** sends a Teams message to `user@example.com` with the headline "Package too hot for medicine."

### Rule action types

Rules can execute different types of actions when triggered:

| Action type | Description |
| ----------- | ----------- |
| `TeamsMessage` | Sends a notification through Microsoft Teams. |
| `EmailMessage` | Sends an email notification. |
| `FabricItemInvocation` | Executes a Fabric item (for example, Pipeline, Notebook) with optional parameters. The rule references a `fabricItemAction-v1` entity by its `uniqueIdentifier`. |

#### TeamsMessage action properties

The Teams notification action has the following configuration properties:

| Property | Type | Description |
| -------- | ---- | ----------- |
| `messageLocale` | string | Language and locale for the message. |
| `recipients` | array | Array of recipient email addresses or Teams users. |
| `headline` | array | Main message title or subject. |
| `optionalMessage` | array | Additional message content. |
| `additionalInformation` | array | Extra contextual information. |

#### EmailMessage action properties

The email notification action has the following configuration properties:

| Property | Type | Description |
| -------- | ---- | ----------- |
| `messageLocale` | string | Language and locale for the email (for example, `en-us`). |
| `sentTo` | array | Array of primary recipient email addresses. |
| `copyTo` | array | Array of CC recipient email addresses. |
| `bCCTo` | array | Array of BCC recipient email addresses. |
| `subject` | string | Email subject line. |
| `headline` | string | Main message content. |
| `optionalMessage` | string | Additional message body content. |
| `additionalInformation` | string | Extra contextual information. |

## Template instances

Time series view entities use templates to define their processing logic. The `definition.instance` property stores the template configuration as a **JSON-encoded string** (not a nested JSON object). You must escape this string when including it in the `ReflexEntities.json` file.

When decoded, each template instance follows this general structure:

```json
{
  "templateId": "<template-name>",
  "templateVersion": "1.1",
  "steps": [
    {
      "name": "<step-name>",
      "id": "<step-guid>",
      "rows": [
        {
          "name": "<row-name>",
          "kind": "<row-kind>",
          "arguments": [
            { "name": "<arg-name>", "type": "<arg-type>", "value": "<arg-value>" }
          ]
        }
      ]
    }
  ]
}
```

**Key fields:**

| Field | Description |
| ----- | ----------- |
| `templateId` | Identifies the template type: `SourceEvent`, `SplitEvent`, `IdentityPartAttribute`, `IdentityTupleAttribute`, `BasicEventAttribute`, `EventTrigger`, or `AttributeTrigger`. |
| `templateVersion` | Version of the template schema (for example, `1.1`). |
| `steps` | An ordered array of processing steps. Each step contains rows that define operations. |
| `steps[].rows[].kind` | The operation type within a step (for example, `SourceReference`, `Event`, `EventField`, `TypeAssertion`, `NumberBecomes`, `TeamsMessage`). |
| `steps[].rows[].arguments` | Arguments that configure the operation. These often include `entityId` references to other entities by their `uniqueIdentifier`. |

> [!TIP]
> The easiest way to get a valid template instance is to configure a Reflex item in the Fabric UI, then use the [Get Item Definition](https://learn.microsoft.com/rest/api/fabric/core/items/get-item-definition) API to retrieve the definition. You can then modify the retrieved template instances as needed.

## Examples per entity type

This section shows individual entity examples. For a complete end-to-end example combining all entity types, see [Complete ReflexEntities.json example](#complete-reflexentitiesjson-example).

### Container example

```json
{
  "uniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee",
  "payload": {
    "name": "Package delivery sample",
    "type": "samples"
  },
  "type": "container-v1"
}
```

### Simulator source example

```json
{
  "uniqueIdentifier": "11bb11bb-cc22-dd33-ee44-55ff55ff55ff",
  "payload": {
    "name": "Package delivery",
    "runSettings": {
      "startTime": "2025-10-21T12:03:31.9271568Z",
      "stopTime": "2025-11-04T15:03:31.03Z"
    },
    "version": "V2_0",
    "type": "PackageShipment",
    "parentContainer": {
      "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
    }
  },
  "type": "simulatorSource-v1"
}
```

### Event view example

```json
{
  "uniqueIdentifier": "22cc22cc-dd33-ee44-ff55-66aa66aa66aa",
  "payload": {
    "name": "Package delivery events",
    "parentContainer": {
      "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
    },
    "definition": {
      "type": "Event",
      "instance": "{\"templateId\":\"SourceEvent\",\"templateVersion\":\"1.1\",\"steps\":[{\"name\":\"SourceEventStep\",\"id\":\"aaaa0000-bb11-2222-33cc-444444dddddd\",\"rows\":[{\"name\":\"SourceSelector\",\"kind\":\"SourceReference\",\"arguments\":[{\"name\":\"entityId\",\"type\":\"string\",\"value\":\"11bb11bb-cc22-dd33-ee44-55ff55ff55ff\"}]}]}]}"
    }
  },
  "type": "timeSeriesView-v1"
}
```

### Object view example

```json
{
  "uniqueIdentifier": "33dd33dd-ee44-ff55-aa66-77bb77bb77bb",
  "payload": {
    "name": "Package",
    "parentContainer": {
      "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
    },
    "definition": {
      "type": "Object"
    }
  },
  "type": "timeSeriesView-v1"
}
```

### Attribute view example (identity)

```json
{
  "uniqueIdentifier": "44ee44ee-ff55-aa66-bb77-88cc88cc88cc",
  "payload": {
    "name": "PackageId",
    "parentObject": {
      "targetUniqueIdentifier": "33dd33dd-ee44-ff55-aa66-77bb77bb77bb"
    },
    "parentContainer": {
      "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
    },
    "definition": {
      "type": "Attribute",
      "instance": "{\"templateId\":\"IdentityPartAttribute\",\"templateVersion\":\"1.1\",\"steps\":[{\"name\":\"IdPartStep\",\"id\":\"bbbb1111-cc22-3333-44dd-555555eeeeee\",\"rows\":[{\"name\":\"TypeAssertion\",\"kind\":\"TypeAssertion\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"Text\"},{\"name\":\"format\",\"type\":\"string\",\"value\":\"\"}]}]}]}"
    }
  },
  "type": "timeSeriesView-v1"
}
```

## Complete ReflexEntities.json example

This example shows a complete **package delivery monitoring system** decoded from Base64. The system uses a simulator data source, tracks packages by ID, monitors temperature, and sends a Teams notification when the temperature exceeds a threshold for medicine packages.

```json
[
  {
    "uniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee",
    "payload": {
      "name": "Package delivery sample",
      "type": "samples"
    },
    "type": "container-v1"
  },
  {
    "uniqueIdentifier": "11bb11bb-cc22-dd33-ee44-55ff55ff55ff",
    "payload": {
      "name": "Package delivery",
      "runSettings": {
        "startTime": "2025-10-21T12:03:31.9271568Z",
        "stopTime": "2025-11-04T15:03:31.03Z"
      },
      "version": "V2_0",
      "type": "PackageShipment",
      "parentContainer": {
        "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
      }
    },
    "type": "simulatorSource-v1"
  },
  {
    "uniqueIdentifier": "22cc22cc-dd33-ee44-ff55-66aa66aa66aa",
    "payload": {
      "name": "Package delivery events",
      "parentContainer": {
        "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
      },
      "definition": {
        "type": "Event",
        "instance": "{\"templateId\":\"SourceEvent\",\"templateVersion\":\"1.1\",\"steps\":[{\"name\":\"SourceEventStep\",\"id\":\"aaaa0000-bb11-2222-33cc-444444dddddd\",\"rows\":[{\"name\":\"SourceSelector\",\"kind\":\"SourceReference\",\"arguments\":[{\"name\":\"entityId\",\"type\":\"string\",\"value\":\"11bb11bb-cc22-dd33-ee44-55ff55ff55ff\"}]}]}]}"
      }
    },
    "type": "timeSeriesView-v1"
  },
  {
    "uniqueIdentifier": "33dd33dd-ee44-ff55-aa66-77bb77bb77bb",
    "payload": {
      "name": "Package",
      "parentContainer": {
        "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
      },
      "definition": {
        "type": "Object"
      }
    },
    "type": "timeSeriesView-v1"
  },
  {
    "uniqueIdentifier": "44ee44ee-ff55-aa66-bb77-88cc88cc88cc",
    "payload": {
      "name": "PackageId",
      "parentObject": {
        "targetUniqueIdentifier": "33dd33dd-ee44-ff55-aa66-77bb77bb77bb"
      },
      "parentContainer": {
        "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
      },
      "definition": {
        "type": "Attribute",
        "instance": "{\"templateId\":\"IdentityPartAttribute\",\"templateVersion\":\"1.1\",\"steps\":[{\"name\":\"IdPartStep\",\"id\":\"bbbb1111-cc22-3333-44dd-555555eeeeee\",\"rows\":[{\"name\":\"TypeAssertion\",\"kind\":\"TypeAssertion\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"Text\"},{\"name\":\"format\",\"type\":\"string\",\"value\":\"\"}]}]}]}"
      }
    },
    "type": "timeSeriesView-v1"
  },
  {
    "uniqueIdentifier": "55ff55ff-aa66-bb77-cc88-99dd99dd99dd",
    "payload": {
      "name": "Temperature (°C)",
      "parentObject": {
        "targetUniqueIdentifier": "33dd33dd-ee44-ff55-aa66-77bb77bb77bb"
      },
      "parentContainer": {
        "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
      },
      "definition": {
        "type": "Attribute",
        "instance": "{\"templateId\":\"BasicEventAttribute\",\"templateVersion\":\"1.1\",\"steps\":[{\"name\":\"EventSelectStep\",\"id\":\"cccc2222-dd33-4444-55ee-666666ffffff\",\"rows\":[{\"name\":\"EventSelector\",\"kind\":\"Event\",\"arguments\":[{\"kind\":\"EventReference\",\"type\":\"complex\",\"arguments\":[{\"name\":\"entityId\",\"type\":\"string\",\"value\":\"aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb\"}],\"name\":\"event\"}]},{\"name\":\"EventFieldSelector\",\"kind\":\"EventField\",\"arguments\":[{\"name\":\"fieldName\",\"type\":\"string\",\"value\":\"Temperature\"}]}]},{\"name\":\"EventComputeStep\",\"id\":\"dddd3333-ee44-5555-66ff-777777aaaaaa\",\"rows\":[{\"name\":\"TypeAssertion\",\"kind\":\"TypeAssertion\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"Number\"},{\"name\":\"format\",\"type\":\"string\",\"value\":\"\"}]}]}]}"
      }
    },
    "type": "timeSeriesView-v1"
  },
  {
    "uniqueIdentifier": "66aa66aa-bb77-cc88-dd99-00ee00ee00ee",
    "payload": {
      "name": "Too hot for medicine",
      "parentObject": {
        "targetUniqueIdentifier": "33dd33dd-ee44-ff55-aa66-77bb77bb77bb"
      },
      "parentContainer": {
        "targetUniqueIdentifier": "00aa00aa-bb11-cc22-dd33-44ee44ee44ee"
      },
      "definition": {
        "type": "Rule",
        "instance": "{\"templateId\":\"AttributeTrigger\",\"templateVersion\":\"1.1\",\"steps\":[{\"name\":\"ScalarSelectStep\",\"id\":\"eeee4444-ff55-6666-77aa-888888bbbbbb\",\"rows\":[{\"name\":\"AttributeSelector\",\"kind\":\"Attribute\",\"arguments\":[{\"kind\":\"AttributeReference\",\"type\":\"complex\",\"arguments\":[{\"name\":\"entityId\",\"type\":\"string\",\"value\":\"55ff55ff-aa66-bb77-cc88-99dd99dd99dd\"}],\"name\":\"attribute\"}]},{\"name\":\"NumberSummary\",\"kind\":\"NumberSummary\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"Average\"},{\"kind\":\"TimeDrivenWindowSpec\",\"type\":\"complex\",\"arguments\":[{\"name\":\"width\",\"type\":\"timeSpan\",\"value\":600000.0},{\"name\":\"hop\",\"type\":\"timeSpan\",\"value\":600000.0}],\"name\":\"window\"}]}]},{\"name\":\"ScalarDetectStep\",\"id\":\"ffff5555-aa66-7777-88bb-999999cccccc\",\"rows\":[{\"name\":\"NumberBecomes\",\"kind\":\"NumberBecomes\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"BecomesGreaterThan\"},{\"name\":\"value\",\"type\":\"number\",\"value\":20.0}]},{\"name\":\"OccurrenceOption\",\"kind\":\"EachTime\",\"arguments\":[]}]},{\"name\":\"DimensionalFilterStep\",\"id\":\"aaaa6666-bb77-8888-99cc-000000dddddd\",\"rows\":[{\"name\":\"AttributeSelector\",\"kind\":\"Attribute\",\"arguments\":[{\"kind\":\"AttributeReference\",\"type\":\"complex\",\"arguments\":[{\"name\":\"entityId\",\"type\":\"string\",\"value\":\"bbbbbbbb-1111-2222-3333-cccccccccccc\"}],\"name\":\"attribute\"}]},{\"name\":\"TextValueCondition\",\"kind\":\"TextValueCondition\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"IsEqualTo\"},{\"name\":\"value\",\"type\":\"string\",\"value\":\"Medicine\"}]}]},{\"name\":\"ActStep\",\"id\":\"0000aaaa-11bb-cccc-dd22-eeeeee333333\",\"rows\":[{\"name\":\"TeamsBinding\",\"kind\":\"TeamsMessage\",\"arguments\":[{\"name\":\"messageLocale\",\"type\":\"string\",\"value\":\"\"},{\"name\":\"recipients\",\"type\":\"array\",\"values\":[{\"type\":\"string\",\"value\":\"user@example.com\"}]},{\"name\":\"headline\",\"type\":\"array\",\"values\":[{\"type\":\"string\",\"value\":\"Package too hot for medicine\"}]},{\"name\":\"optionalMessage\",\"type\":\"array\",\"values\":[{\"type\":\"string\",\"value\":\"This temperature-sensitive package containing medicine has exceeded the allowed threshold.\"}]},{\"name\":\"additionalInformation\",\"type\":\"array\",\"values\":[]}]}]}]}",
        "settings": {
          "shouldRun": false,
          "shouldApplyRuleOnUpdate": false
        }
      }
    },
    "type": "timeSeriesView-v1"
  }
]
```

## Definition payload example

This example shows the complete API request body structure for creating a Reflex item by using the [Create Item](https://learn.microsoft.com/rest/api/fabric/core/items/create-item) API. The `payload` values contain the Base64-encoded content of their respective files.

```json
{
    "format": "json",
    "parts": [
        {
            "path": "ReflexEntities.json",
            "payload": "W3sKICAidW5pcXVlSWRlbnRpZmllciI6ICIwMGFhMDBhYS1iYjExLWNjMjItZGQzMy00NGVlNDRlZTQ0ZWUiLAogICJwYXlsb2FkIjogewogICAgIm5hbWUiOiAiUGFja2FnZSBkZWxpdmVyeSBzYW1wbGUiLAogICAgInR5cGUiOiAic2FtcGxlcyIKICB9LAogICJ0eXBlIjogImNvbnRhaW5lci12MSIKfV0=",
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
