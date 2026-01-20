# Reflex definition

This article provides a breakdown of the definition structure for Reflex items.

>[!NOTE]
>Reflex is also known as Activator.

## Overview

A Reflex (Activator) item defines a set of rules on streaming data. It monitors event streams and generates notifications or actions when specific conditions are met. The definition structure describes data sources, object models, attributes, and rules that form a pipeline for processing streaming events.

## Supported formats

Reflex items support the `json` format.

## Definition parts

The definition of a Reflex item consists of the following parts:

| Definition part path | Type | Required | Description |
|--|--|--|--|
| `ReflexEntities.json` | [Reflex Entities](#reflex-entities-structure) (JSON) | true | Contains the complete Activator configuration including containers, data sources, objects, attributes, and rules |
| `.platform` | Platform metadata | false | Contains Fabric platform metadata information |

## Reflex Entities structure

The `ReflexEntities.json` part contains an array of entity objects that define the complete Activator configuration. Each entity represents a component in the streaming data pipeline.

### Entity types

Reflex definitions support the following entity types:

| Type | Description |
|--|--|
| `container-v1` | Top-level container that organizes related components |
| `simulatorSource-v1` | Simulated data source for testing and demonstrations |
| `kqlSource-v1` | KQL query-based data source connected to an Eventhouse |
| `realTimeHubSource-v1` | Real-time hub data source for workspace events |
| `eventstreamSource-v1` | Event stream data source connected to a Fabric Eventstream |
| `fabricItemAction-v1` | Fabric item action that can be invoked by rules |
| `timeSeriesView-v1` | Core building block that defines events, objects, attributes, or rules |

### Common entity properties

All entities share these common properties:

| Property | Type | Required | Description |
|--|--|--|--|
| `uniqueIdentifier` | string | true | Unique identifier for the entity |
| `payload` | object | true | Entity-specific configuration data |
| `type` | string | true | The entity type (e.g., `container-v1`, `timeSeriesView-v1`) |

### Container entity (`container-v1`)

Containers organize related components and provide a hierarchical structure.

| Property | Type | Required | Description |
|--|--|--|--|
| `name` | string | true | Display name of the container |
| `type` | string | true | Container type (e.g., `samples`, `kqlQueries`) |

### Data source entities

Data source entities define the data sources to which the item is connected and from which it receives its data.

#### Simulator source (`simulatorSource-v1`)

Generates simulated streaming data for testing purposes.

| Property | Type | Required | Description |
|--|--|--|--|
| `name` | string | true | Display name of the data source |
| `runSettings` | object | false | Execution configuration |
| `runSettings.startTime` | string (ISO 8601) | false | When to start generating data |
| `runSettings.stopTime` | string (ISO 8601) | false | When to stop generating data |
| `version` | string | false | Version identifier (e.g., `V2_0`) |
| `type` | string | true | Type of simulated data (e.g., `PackageShipment`) |
| `parentContainer` | object | true | Reference to parent container |
| `parentContainer.targetUniqueIdentifier` | string | true | Unique identifier of the parent container entity |

#### KQL source (`kqlSource-v1`)

Connects to a KQL database to query streaming data.

| Property | Type | Required | Description |
|--|--|--|--|
| `name` | string | true | Display name of the data source |
| `runSettings` | object | false | Query execution settings |
| `runSettings.executionIntervalInSeconds` | number | false | How often to execute the query |
| `query.queryString` | string | true | KQL query to execute |
| `eventhouseItem` | object | true | Reference to Eventhouse item |
| `eventhouseItem.targetUniqueIdentifier` | string | true | Unique identifier of the Eventhouse item |
| `parentContainer` | object | true | Reference to parent container |
| `parentContainer.targetUniqueIdentifier` | string | true | Unique identifier of the parent container entity |

#### Real-time hub source (`realTimeHubSource-v1`)

Connects to a Real-time hub data source for monitoring workspace events.

| Property | Type | Required | Description |
|--|--|--|--|
| `name` | string | true | Display name of the data source |
| `connection` | object | true | Real-time hub connection configuration |
| `connection.scope` | string | true | Scope of events (e.g., `Workspace`) |
| `connection.tenantId` | string | true | Azure tenant identifier |
| `connection.workspaceId` | string | true | Fabric workspace identifier |
| `connection.eventGroupType` | string | true | Type of event group (e.g., `Microsoft.Fabric.WorkspaceEvents`) |
| `filterSettings` | object | true | Event filtering configuration |
| `filterSettings.eventTypes` | array | true | Array of event types to monitor |
| `filterSettings.eventTypes[].name` | string | true | Event type name (e.g., `Microsoft.Fabric.ItemCreateSucceeded`) |
| `filterSettings.filters` | array | false | Additional filters to apply |
| `parentContainer` | object | true | Reference to parent container |
| `parentContainer.targetUniqueIdentifier` | string | true | Unique identifier of the parent container entity |

#### Event stream source (`eventstreamSource-v1`)

Connects to a Fabric Eventstream data source for streaming events.

| Property | Type | Required | Description |
|--|--|--|--|
| `name` | string | true | Display name of the data source |
| `metadata` | object | true | Eventstream connection metadata |
| `metadata.eventstreamArtifactId` | string | true | Identifier of the Fabric Eventstream artifact |

### Action entities

#### Fabric item action (`fabricItemAction-v1`)

Defines a Fabric item that can be invoked as an action by rules.

| Property | Type | Required | Description |
|--|--|--|--|
| `name` | string | true | Display name of the action |
| `fabricItem` | object | true | Reference to the Fabric item to execute |
| `fabricItem.itemId` | string | true | Fabric item identifier |
| `fabricItem.workspaceId` | string | true | Workspace containing the item |
| `fabricItem.itemType` | string | true | Type of Fabric item (e.g., `Pipeline`) |
| `jobType` | string | true | Type of job to execute (e.g., `Pipeline`) |
| `parentContainer` | object | true | Reference to parent container |
| `parentContainer.targetUniqueIdentifier` | string | true | Unique identifier of the parent container entity |

### Time series view entities (`timeSeriesView-v1`)

Time series views are the core building blocks that define how data flows through the Activator pipeline. They can represent [events](#event-views), [objects](#object-views), [attributes](#attribute-views), or [rules](#rule-views).

#### Common time series view properties

| Property | Type | Required | Description |
|--|--|--|--|
| `name` | string | true | Display name of the view |
| `parentContainer` | object | true | Reference to parent container |
| `parentContainer.targetUniqueIdentifier` | string | true | Unique identifier of the parent container entity |
| `definition` | object | true | Defines the view's behavior and logic |
| `definition.type` | string | true | Type of view: `Event`, `Object`, `Attribute`, or `Rule` |
| `definition.instance` | string | false | JSON string containing template-specific configuration |

#### Event views

Event views filter and transform streaming data to create new event streams.

| Property | Type | Required | Description |
|--|--|--|--|
| `definition.type` | string | true | Must be `Event` |
| `definition.instance` | string | true | JSON configuration for event processing templates |

Common event templates:

- `SourceEvent`: Selects events from a data source
- `SplitEvent`: Splits events by object identity for per-object processing

#### Object views

Object views define the entities being monitored (e.g., packages, sensors, users).

| Property | Type | Required | Description |
|--|--|--|--|
| `definition.type` | string | true | Must be `Object` |

#### Attribute views

Attribute views extract and compute properties from events, defining what data to monitor.

| Property | Type | Required | Description |
|--|--|--|--|
| `definition.type` | string | true | Must be `Attribute` |
| `definition.instance` | string | true | JSON configuration for attribute extraction and computation |
| `parentObject` | object | true | Reference to the parent object this attribute belongs to |
| `parentObject.targetUniqueIdentifier` | string | true | Unique identifier of the parent object entity |

Common attribute templates:

- `IdentityPartAttribute`: Defines part of an object's identity
- `IdentityTupleAttribute`: Combines identity parts into a complete object identifier
- `BasicEventAttribute`: Extracts simple values from events

#### Rule views

Rule views define triggers and actions that execute when conditions are met.

| Property | Type | Required | Description |
|--|--|--|--|
| `definition.type` | string | true | Must be `Rule` |
| `definition.instance` | string | true | JSON configuration for trigger logic and actions |
| `definition.settings` | object | false | Rule execution settings |
| `definition.settings.shouldRun` | boolean | false | Whether the rule is currently active |
| `definition.settings.shouldApplyRuleOnUpdate` | boolean | false | Whether to apply rule to historical data |
| `parentObject` | object | false | Reference to parent object (for object-scoped rules) |
| `parentObject.targetUniqueIdentifier` | string | false | Unique identifier of the parent object entity |

Common rule templates:

- `EventTrigger`: Triggers on specific events
- `AttributeTrigger`: Triggers based on attribute values or changes

#### Rule action types

Rules can execute different types of actions when triggered:

- **TeamsMessage**: Sends notifications via Microsoft Teams
- **EmailMessage**: Sends email notifications  
- **FabricItemInvocation**: Executes a Fabric item (e.g., Pipeline, Notebook) with optional parameters

##### Notification actions configuration

Both Teams and Email notification actions share similar configuration parameters:

**TeamsMessage configuration:**

- `messageLocale`: Language/locale for the message
- `recipients`: Array of recipient email addresses or Teams users
- `headline`: Main message title/subject
- `optionalMessage`: Additional message content
- `additionalInformation`: Extra contextual information

**EmailMessage configuration:**

- `messageLocale`: Language/locale for the email (e.g., `en-us`)
- `sentTo`: Array of primary recipient email addresses
- `copyTo`: Array of CC recipient email addresses  
- `bCCTo`: Array of BCC recipient email addresses
- `subject`: Email subject line
- `headline`: Main message content
- `optionalMessage`: Additional message body content
- `additionalInformation`: Extra contextual information

### Parent references

Entities reference their parents using these structures:

| Property | Type | Description |
|--|--|--|
| `parentContainer.targetUniqueIdentifier` | string | References a container entity |
| `parentObject.targetUniqueIdentifier` | string | References an object entity |

### Template instances

The `instance` property contains JSON-encoded template configurations. These templates define the specific logic for processing events, extracting attributes, or triggering actions. Common template types include:

- **Event templates**: `SourceEvent`, `SplitEvent`
- **Attribute templates**: `IdentityPartAttribute`, `IdentityTupleAttribute`, `BasicEventAttribute`  
- **Rule templates**: `EventTrigger`, `AttributeTrigger`

Each template has its own specific schema for configuring steps, arguments, and processing logic. Rules can reference `fabricItemAction-v1` entities in their `FabricItemInvocation` actions to execute Fabric items with parameters derived from the triggering events.

## Example of ReflexEntities.json content decoded from Base64

This example shows a package delivery monitoring system with various triggers and notifications:

```json
[
  {
    "uniqueIdentifier": "c845c472-8d8e-498d-bdf4-9e12ab456668",
    "payload": {
      "name": "Package delivery sample",
      "type": "samples"
    },
    "type": "container-v1"
  },
  {
    "uniqueIdentifier": "85693be9-e8ed-49b3-b792-9dcb6750a49a",
    "payload": {
      "name": "Package delivery",
      "runSettings": {
        "startTime": "2025-10-21T12:03:31.9271568Z",
        "stopTime": "2025-11-04T15:03:31.03Z"
      },
      "version": "V2_0",
      "type": "PackageShipment",
      "parentContainer": {
        "targetUniqueIdentifier": "c845c472-8d8e-498d-bdf4-9e12ab456668"
      }
    },
    "type": "simulatorSource-v1"
  },
  {
    "uniqueIdentifier": "c4a451a0-6b1e-4cc7-b5b8-93d58816a85b",
    "payload": {
      "name": "Package delivery events",
      "parentContainer": {
        "targetUniqueIdentifier": "c845c472-8d8e-498d-bdf4-9e12ab456668"
      },
      "definition": {
        "type": "Event",
        "instance": "{\"templateId\":\"SourceEvent\",\"templateVersion\":\"1.1\",\"steps\":[{\"name\":\"SourceEventStep\",\"id\":\"c5b4fdfd-559a-4bf7-912f-705617cd1d86\",\"rows\":[{\"name\":\"SourceSelector\",\"kind\":\"SourceReference\",\"arguments\":[{\"name\":\"entityId\",\"type\":\"string\",\"value\":\"85693be9-e8ed-49b3-b792-9dcb6750a49a\"}]}]}]}"
      }
    },
    "type": "timeSeriesView-v1"
  },
  {
    "uniqueIdentifier": "a18d312c-c855-4e76-bb75-ce473ae59e8c",
    "payload": {
      "name": "Package",
      "parentContainer": {
        "targetUniqueIdentifier": "c845c472-8d8e-498d-bdf4-9e12ab456668"
      },
      "definition": {
        "type": "Object"
      }
    },
    "type": "timeSeriesView-v1"
  },
  {
    "uniqueIdentifier": "47f2a2ab-e2e9-4dc1-88f7-953f3b5cfd37",
    "payload": {
      "name": "PackageId",
      "parentObject": {
        "targetUniqueIdentifier": "a18d312c-c855-4e76-bb75-ce473ae59e8c"
      },
      "parentContainer": {
        "targetUniqueIdentifier": "c845c472-8d8e-498d-bdf4-9e12ab456668"
      },
      "definition": {
        "type": "Attribute",
        "instance": "{\"templateId\":\"IdentityPartAttribute\",\"templateVersion\":\"1.1\",\"steps\":[{\"name\":\"IdPartStep\",\"id\":\"ce6358bf-e1e4-4f16-8453-181b58ec0a4d\",\"rows\":[{\"name\":\"TypeAssertion\",\"kind\":\"TypeAssertion\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"Text\"},{\"name\":\"format\",\"type\":\"string\",\"value\":\"\"}]}]}]}"
      }
    },
    "type": "timeSeriesView-v1"
  },
  {
    "uniqueIdentifier": "07564441-30f5-4a67-8df2-4f13ba85a2da",
    "payload": {
      "name": "Temperature (°C)",
      "parentObject": {
        "targetUniqueIdentifier": "a18d312c-c855-4e76-bb75-ce473ae59e8c"
      },
      "parentContainer": {
        "targetUniqueIdentifier": "c845c472-8d8e-498d-bdf4-9e12ab456668"
      },
      "definition": {
        "type": "Attribute",
        "instance": "{\"templateId\":\"BasicEventAttribute\",\"templateVersion\":\"1.1\",\"steps\":[{\"name\":\"EventSelectStep\",\"id\":\"f986dac6-a843-43c4-9335-e5950148d41e\",\"rows\":[{\"name\":\"EventSelector\",\"kind\":\"Event\",\"arguments\":[{\"kind\":\"EventReference\",\"type\":\"complex\",\"arguments\":[{\"name\":\"entityId\",\"type\":\"string\",\"value\":\"f9c9e86e-0389-4128-bcd8-65cbe2352d75\"}],\"name\":\"event\"}]},{\"name\":\"EventFieldSelector\",\"kind\":\"EventField\",\"arguments\":[{\"name\":\"fieldName\",\"type\":\"string\",\"value\":\"Temperature\"}]}]},{\"name\":\"EventComputeStep\",\"id\":\"10553199-f8bc-4275-80a8-7996a255f6a3\",\"rows\":[{\"name\":\"TypeAssertion\",\"kind\":\"TypeAssertion\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"Number\"},{\"name\":\"format\",\"type\":\"string\",\"value\":\"\"}]}]}]}"
      }
    },
    "type": "timeSeriesView-v1"
  },
  {
    "uniqueIdentifier": "2b587dc3-5dfc-4838-ad4c-35e1586c58d5",
    "payload": {
      "name": "Too hot for medicine",
      "parentObject": {
        "targetUniqueIdentifier": "a18d312c-c855-4e76-bb75-ce473ae59e8c"
      },
      "parentContainer": {
        "targetUniqueIdentifier": "c845c472-8d8e-498d-bdf4-9e12ab456668"
      },
      "definition": {
        "type": "Rule",
        "instance": "{\"templateId\":\"AttributeTrigger\",\"templateVersion\":\"1.1\",\"steps\":[{\"name\":\"ScalarSelectStep\",\"id\":\"a63dcd75-3149-452b-a49c-ab4973c3dd82\",\"rows\":[{\"name\":\"AttributeSelector\",\"kind\":\"Attribute\",\"arguments\":[{\"kind\":\"AttributeReference\",\"type\":\"complex\",\"arguments\":[{\"name\":\"entityId\",\"type\":\"string\",\"value\":\"07564441-30f5-4a67-8df2-4f13ba85a2da\"}],\"name\":\"attribute\"}]},{\"name\":\"NumberSummary\",\"kind\":\"NumberSummary\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"Average\"},{\"kind\":\"TimeDrivenWindowSpec\",\"type\":\"complex\",\"arguments\":[{\"name\":\"width\",\"type\":\"timeSpan\",\"value\":600000.0},{\"name\":\"hop\",\"type\":\"timeSpan\",\"value\":600000.0}],\"name\":\"window\"}]}]},{\"name\":\"ScalarDetectStep\",\"id\":\"a1bc1c0c-6651-46ac-976f-9a1fcb42a269\",\"rows\":[{\"name\":\"NumberBecomes\",\"kind\":\"NumberBecomes\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"BecomesGreaterThan\"},{\"name\":\"value\",\"type\":\"number\",\"value\":20.0}]},{\"name\":\"OccurrenceOption\",\"kind\":\"EachTime\",\"arguments\":[]}]},{\"name\":\"DimensionalFilterStep\",\"id\":\"9445c334-ebd3-47b7-b958-198f7e53f53e\",\"rows\":[{\"name\":\"AttributeSelector\",\"kind\":\"Attribute\",\"arguments\":[{\"kind\":\"AttributeReference\",\"type\":\"complex\",\"arguments\":[{\"name\":\"entityId\",\"type\":\"string\",\"value\":\"25079a06-01f7-4dd9-a72c-761684ce0d37\"}],\"name\":\"attribute\"}]},{\"name\":\"TextValueCondition\",\"kind\":\"TextValueCondition\",\"arguments\":[{\"name\":\"op\",\"type\":\"string\",\"value\":\"IsEqualTo\"},{\"name\":\"value\",\"type\":\"string\",\"value\":\"Medicine\"}]}]},{\"name\":\"ActStep\",\"id\":\"3dec7f63-ab86-411d-b35c-cce1db9f124e\",\"rows\":[{\"name\":\"TeamsBinding\",\"kind\":\"TeamsMessage\",\"arguments\":[{\"name\":\"messageLocale\",\"type\":\"string\",\"value\":\"\"},{\"name\":\"recipients\",\"type\":\"array\",\"values\":[{\"type\":\"string\",\"value\":\"user@example.com\"}]},{\"name\":\"headline\",\"type\":\"array\",\"values\":[{\"type\":\"string\",\"value\":\"Package too hot for medicine\"}]},{\"name\":\"optionalMessage\",\"type\":\"array\",\"values\":[{\"type\":\"string\",\"value\":\"This temperature-sensitive package containing medicine has exceeded the allowed threshold.\"}]},{\"name\":\"additionalInformation\",\"type\":\"array\",\"values\":[]}]}]}]}",
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

## Definition example

```json
{
    "format": "json",
    "parts": [
        {
            "path": "ReflexEntities.json",
            "payload": "W3sKICAidW5pcXVlSWRlbnRpZmllciI6ICJjODQ1YzQ3Mi04ZDhlLTQ5OGQtYmRmNC05ZTEyYWI0NTY2NjgiLAogICJwYXlsb2FkIjogewogICAgIm5hbWUiOiAiUGFja2FnZSBkZWxpdmVyeSBzYW1wbGUiLAogICAgInR5cGUiOiAic2FtcGxlcyIKICB9LAogICJ0eXBlIjogImNvbnRhaW5lci12MSIKfV0=",
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

