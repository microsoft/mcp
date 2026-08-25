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

The `tools list` and `--learn` output uses the existing descriptive metadata shape:

```json
{
  "metadata": {
    "operationPlane": {
      "value": "control",
      "description": "This tool operates against an Azure management or control-plane API."
    }
  }
}
```

Stable serialized values are `unspecified`, `data`, `control`, `both`, and `notApplicable`.

Older metadata without `operationPlane` deserializes as `Unspecified`.

## MCP representation

Operation plane is not a standard MCP `ToolAnnotations` hint. It must not be mapped to an unrelated standard hint.

MCP tools expose the value through namespaced custom metadata:

```json
{
  "_meta": {
    "azure.com/operation-plane": "control"
  }
}
```

The namespace prevents clients from interpreting the value as part of the MCP specification.

## Tool-family aggregation

Namespace and consolidated MCP tools derive their plane from their child commands. Documentation generators can apply the same rules to the individual commands returned by `tools list`:

| Child classifications | Aggregate |
|---|---|
| All `Data` | `Data` |
| All `Control` | `Control` |
| Any `Both` | `Both` |
| A mixture of `Data` and `Control` | `Both` |
| Only `NotApplicable` | `NotApplicable` |
| Any `Unspecified` | `Unspecified` |

`NotApplicable` does not change an otherwise applicable aggregate. For example, `Data` plus `NotApplicable` aggregates to `Data`.

Operation-plane aggregation is intentionally separate from consolidated behavioral-metadata equality. A consolidated tool may validly contain a mixture of control-plane and data-plane commands.

## Initial rollout

1. Add the enum, metadata model, JSON serialization, and MCP custom metadata.
2. Classify Event Grid commands to validate data-, control-, and mixed-family behavior.
3. Classify remaining Azure commands incrementally with service-owner review.
4. Add repository validation requiring a non-`Unspecified` classification after migration.
5. Update documentation generation to display the individual classification and derive tool-family coverage.

## Compatibility

- Command syntax and option binding do not change.
- The CLI JSON change is additive.
- Consumers that ignore unknown JSON properties continue to work.
- Older serialized metadata remains readable.
- Standard MCP tool annotations remain unchanged.
- Serialization uses source-generated `System.Text.Json` metadata and is AOT-safe.
