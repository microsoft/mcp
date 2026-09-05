# Operation-Plane Metadata

Related issue: [microsoft/mcp#1616](https://github.com/microsoft/mcp/issues/1616)

## Status

Proposed.

## Context

MCP tools in this repository can call management APIs, service data APIs, or both. The existing tool metadata describes behavioral characteristics such as whether a tool is read-only or destructive, but it does not identify the API plane a tool acts against.

Documentation generation needs this classification in `tools list` JSON so it can accurately describe the coverage of individual tools and tool families.

## Decision

Add a `ToolOperationPlane` value to `CommandMetadataAttribute` and propagate it through `ToolMetadata`.

```csharp
public enum ToolOperationPlane
{
    Unspecified,
    Data,
    Control,
    Both,
    NotApplicable
}
```

Tools declare their classification alongside their existing metadata:

```csharp
[CommandMetadata(
    Id = "...",
    Name = "list",
    Title = "List Event Grid Topics",
    Description = "...",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
```

### Classification

A tool's plane is the plane of the API it acts against to produce the result the user asked for.

| Value | Meaning |
|---|---|
| `Data` | The tool performs its action(s) against a service data-plane API. |
| `Control` | The tool performs its action(s) against Azure Resource Manager or another management-plane API. |
| `Both` | The tool can act on either plane, so a single plane cannot be inferred from the tool alone. |
| `NotApplicable` | The tool performs no service-plane operation, such as `tools list`, `server start`, or a local-only utility. |
| `Unspecified` | The tool has not been classified. This is an unset marker, not a valid answer. |

`NotApplicable` is a deliberate classification meaning "reviewed, and no service plane applies". `Unspecified` means "not yet reviewed" and fails validation.

Classify by the API the work ultimately reaches, not by how the tool reaches it. A tool that shells out to an external CLI is not `NotApplicable` if that CLI calls Azure on the user's behalf; `extension azqr` runs the Azure Quick Review CLI, which scans the subscription through ARM, so it is `Control`. `extension cli generate` and `extension cli install` call no service at all and are genuinely `NotApplicable`.

#### Setup does not count

Resolving a subscription, tenant, resource ID, or service endpoint is **setup** the tool does before it can act. It never contributes a plane.

This exclusion is what makes the annotation useful. Nearly every data-plane tool resolves its target through ARM before it can issue the workload call, so if setup counted as control-plane work, almost every tool would be `Both` and the label would stop discriminating.

`azmcp eventgrid events publish` is the worked example. It calls ARM to look up the topic and read its endpoint, then sends the events to that endpoint. The ARM traffic exists only to find where to publish, so the tool is `Data`.

Applying the rule:

| Tool | What the tool acts against | Plane |
|---|---|---|
| `eventgrid topic list` | The ARM enumeration is the answer. | `Control` |
| `eventgrid subscription list` | The ARM enumeration is the answer. | `Control` |
| `eventgrid events publish` | The publish is the answer; the ARM lookup is setup. | `Data` |

#### When `Both` applies

`Both` means a caller cannot infer a single plane from the tool alone. Two shapes qualify.

The first is a tool that performs two distinct actions the user asked for, on different planes, in one call. Creating a topic and publishing a seed event to it would qualify: the user wants the topic *and* the event, and neither call is setup for the other.

The second is a tool that dispatches to one plane or the other depending on the arguments it is given. `cosmos list` is the example in this repository: with no `--account` it lists accounts through ARM, but with `--account` it lists databases through the Cosmos DB data plane. Only one branch runs per invocation, so no single call is on both planes, but the tool as a declared capability reaches both. `mysql list` and `postgres list` have the same shape.

Both shapes are treated identically because consumers of this metadata reason about a tool before it is invoked. A policy that permits only data-plane tools must reject `cosmos list`, because it cannot know which branch a given call will take.

A tool that merely reaches ARM on its way to a data-plane call is `Data`, not `Both`.

### Default

`OperationPlane` defaults to `Unspecified`, which is an unset marker rather than a shipping state.

Defaulting to `Data` would silently misclassify the many ARM-based tools. Making the default an unset marker instead means a tool that is never classified fails loudly rather than publishing a wrong answer.

`CommandOperationPlaneTests.AllCommands_DeclareAnOperationPlane` enforces this: it walks every registered tool and fails on any that is still `Unspecified`. There is no allowlist, so a new tool must be classified before it can ship. A tool that calls no service is `NotApplicable`, which is an explicit classification and satisfies the test.

## JSON representation

The `tools list` and `--learn` output reports the plane as a string on the existing `metadata` object:

```json
{
  "metadata": {
    "operationPlane": "control",
    "destructive": { "value": false, "description": "..." },
    "readOnly": { "value": true, "description": "..." }
  }
}
```

The behavioral flags use a `{ value, description }` shape because `"destructive": true` needs explanation. `"operationPlane": "control"` is self-describing, so it is emitted as a plain string rather than carrying a description that is a fixed function of the value.

Stable serialized values are `unspecified`, `data`, `control`, `both`, and `notApplicable`. Unrecognized values, including any added by a future version, deserialize as `Unspecified` so older binaries keep reading newer metadata.

Metadata without `operationPlane` also deserializes as `Unspecified`.

## Scope

This change adds the annotation and surfaces it per tool in the CLI output JSON, which is what documentation generation consumes.

It deliberately does not add the plane to MCP tool definitions. Operation plane is not a standard MCP `ToolAnnotations` hint, and no MCP client consumes it today. If a runtime consumer appears, the value can be published later under a namespaced `_meta` key such as `azure.com/operation-plane`, which keeps clients from interpreting it as part of the MCP specification.

## Aggregating a tool family

Tool families are not aggregated at runtime. A documentation generator holding the per-tool values applies these rules itself:

| Child classifications | Aggregate |
|---|---|
| All `Data` | `Data` |
| All `Control` | `Control` |
| Any `Both` | `Both` |
| A mixture of `Data` and `Control` | `Both` |
| Only `NotApplicable` | `NotApplicable` |
| Any `Unspecified` | `Unspecified` |

`NotApplicable` does not change an otherwise applicable aggregate. For example, `Data` plus `NotApplicable` aggregates to `Data`.

Keeping aggregation in the consumer avoids duplicating the rule across the namespace, consolidated, proxy, and single-tool loading paths for a value none of them currently expose.

## Rollout

Delivered in this change:

1. The enum, the attribute property, and the CLI JSON serialization.
2. A classification for every tool in the repository.
3. A test rejecting `Unspecified`, so a new tool cannot be added without a classification.

Left to a consumer:

4. Documentation generation displaying the classification and deriving tool-family coverage.

Classifying everything in one pass, rather than incrementally, keeps `Unspecified` from becoming a permanent resting state and means the annotation is useful the day it ships.

## Compatibility

- Tool syntax and option binding do not change.
- The CLI JSON change is additive.
- Consumers that ignore unknown JSON properties continue to work.
- Older serialized metadata remains readable, and unknown future values degrade to `Unspecified`.
- MCP tool definitions and standard tool annotations are unchanged.
- Serialization uses source-generated `System.Text.Json` metadata and is AOT-safe.
