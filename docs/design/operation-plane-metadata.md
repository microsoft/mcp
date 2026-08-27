# Azure Operation-Plane Metadata

Related issue: [microsoft/mcp#1616](https://github.com/microsoft/mcp/issues/1616)

## Status

Proposed.

## Context

Azure MCP commands can call Azure management APIs, service data APIs, or both. The existing tool metadata describes behavioral characteristics such as whether a command is read-only or destructive, but it does not identify the Azure API plane used by a command.

The strongly typed option conversion described in [option-conversion.md](../option-conversion.md) moves command identity and behavioral hints to `CommandMetadataAttribute`. The `[Option]` attributes introduced by that conversion only describe command-line parameters and therefore do not solve operation-plane classification.

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

Commands declare their classification alongside their existing metadata:

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

| Value | Meaning |
|---|---|
| `Data` | The command operates against an Azure service data-plane API. |
| `Control` | The command operates against Azure Resource Manager or another management-plane API. |
| `Both` | The command directly uses both control-plane and data-plane APIs. |
| `NotApplicable` | The command does not perform an Azure service-plane operation, such as `tools list`, `server start`, or a local-only utility. |
| `Unspecified` | The command has not yet been reviewed and classified. This is a migration state, not a final classification. |

`NotApplicable` is an explicit classification. `Unspecified` identifies migration debt.

### Default

`OperationPlane` defaults to `Unspecified`.

Defaulting existing commands to `Data` would silently misclassify ARM-based commands. An explicit migration state keeps the change source-compatible without publishing incorrect documentation. Once existing commands have been classified, repository validation can reject `Unspecified` for Azure service commands.

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

This change adds the annotation and surfaces it per command in the CLI output JSON, which is what documentation generation consumes.

It deliberately does not add the plane to MCP tool definitions. Operation plane is not a standard MCP `ToolAnnotations` hint, and no MCP client consumes it today. If a runtime consumer appears, the value can be published later under a namespaced `_meta` key such as `azure.com/operation-plane`, which keeps clients from interpreting it as part of the MCP specification.

## Aggregating a tool family

Tool families are not aggregated at runtime. A documentation generator holding the per-command values applies these rules itself:

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

## Initial rollout

1. Add the enum, the attribute property, and the CLI JSON serialization.
2. Classify Event Grid commands to validate data-, control-, and mixed-plane commands.
3. Classify remaining Azure commands incrementally with service-owner review.
4. Add repository validation requiring a non-`Unspecified` classification after migration.
5. Update documentation generation to display the classification and derive tool-family coverage.

## Compatibility

- Command syntax and option binding do not change.
- The CLI JSON change is additive.
- Consumers that ignore unknown JSON properties continue to work.
- Older serialized metadata remains readable, and unknown future values degrade to `Unspecified`.
- MCP tool definitions and standard tool annotations are unchanged.
- Serialization uses source-generated `System.Text.Json` metadata and is AOT-safe.
