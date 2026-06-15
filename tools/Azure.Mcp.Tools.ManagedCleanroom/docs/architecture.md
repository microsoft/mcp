# Azure Managed Cleanroom MCP Toolset - Architecture

## Overview

`Azure.Mcp.Tools.ManagedCleanroom` currently implements two commands under the `azmcp managedcleanroom` namespace:

- `collaborations list` (data plane)
- `collaboration create` (control plane)

## Implemented Command Surfaces

| Surface | Command | Tool Name | Purpose |
|---------|---------|-----------|---------|
| Data plane (Analytics Frontend) | `collaborations list` | `managedcleanroom_collaborations_list` | Lists collaborations the caller can access from the configured endpoint. |
| Control plane (ARM) | `collaboration create` | `managedcleanroom_collaboration_create` | Creates an ARM collaboration resource in the specified resource group and subscription. |

## Project Structure (Current Scope)

```
Azure.Mcp.Tools.ManagedCleanroom/
├── src/
│   ├── ManagedCleanroomSetup.cs
│   ├── Commands/
│   │   ├── ManagedCleanroomJsonContext.cs
│   │   ├── Collaboration/CollaborationCreateCommand.cs
│   │   └── Collaborations/CollaborationsListCommand.cs
│   ├── Options/
│   │   ├── ManagedCleanroomOptionDescriptions.cs
│   │   ├── Collaboration/CollaborationCreateOptions.cs
│   │   └── Collaborations/CollaborationsListOptions.cs
│   └── Services/
│       ├── IManagedCleanroomService.cs
│       └── ManagedCleanroomService.cs
└── tests/
    └── Azure.Mcp.Tools.ManagedCleanroom.Tests/
        ├── Collaboration/CollaborationCreateCommandTests.cs
        └── Collaborations/CollaborationsListCommandTests.cs
```

## Notes

- Commands not listed above are intentionally out of scope for this branch.
- The setup registers only the two commands above so tool discovery is restricted to this initial implementation slice.
