# Azure Managed Cleanroom MCP Toolset - Architecture

## Overview

`Azure.Mcp.Tools.ManagedCleanroom` currently provides an initial set of operations for interacting with Azure Managed Cleanroom services. The current implementation includes `collaborations list` (data plane) and `collaborationarm create` (control plane), with additional command groups planned.

Commands interact with:
- **Data Plane APIs**: Cleanroom Analytics Frontend for read operations (list collaborations, queries, datasets)
- **Control Plane APIs**: Azure Resource Manager (ARM) for write operations (create collaboration, manage resources)

---

## Command Surfaces

- **Management Plane Commands**:

| Command Group | Command | Status |
| --- | --- | --- |
| CollaborationArm | `collaborationarm create` | Completed |
| CollaborationArm | `collaborationarm get` | Pending |
| CollaborationArm | `collaborationarm add-collaborator` | Pending |
| CollaborationArm | `collaborationarm enable-workload` | Pending |
| CollaborationArm | `collaborationarm get-readonly-kubeconfig` | Pending |

- **Data Plane Commands**:

| Command Group | Command | Status |
| --- | --- | --- |
| Collaborations | `collaborations list` | Completed |
| Collaborations | `collaborations get` | Pending |
| Analytics | `analytics get` | Pending |
| Analytics | `analytics skr-policy` | Pending |
| OIDC | `oidc issuer-info` | Pending |
| OIDC | `oidc keys` | Pending |
| OIDC | `oidc set-issuer-url` | Pending |
| Invitations | `invitations list` | Pending |
| Invitations | `invitations accept` | Pending |
| Datasets | `datasets publish` | Pending |
| Datasets | `datasets get` | Pending |
| Datasets | `datasets list` | Pending |
| Consent | `consent put` | Pending |
| Queries | `queries publish` | Pending |
| Queries | `queries get` | Pending |
| Queries | `queries list` | Pending |
| Queries | `queries vote` | Pending |
| Queries | `queries run` | Pending |
| Queries | `queries runs` | Pending |
| Runs | `runs get` | Pending |
| Audit Events | `auditevents list` | Pending |

---

## Project Structure

```
Azure.Mcp.Tools.ManagedCleanroom/
├── src/
│   ├── ManagedCleanroomSetup.cs                      # DI registration & command tree
│   ├── Commands/
│   │   ├── ManagedCleanroomJsonContext.cs            # AOT-safe JSON serialization
│   │   ├── Collaboration/
│   │   │   ├── CollaborationCreateCommand.cs 
│   │   │   └── [Other collaboration commands ]
│   │   ├── Collaborations/
│   │   │   ├── CollaborationsListCommand.cs 
│   │   │   └── [Other collaboration commands ]
│   │   ├── Analytics/                              # Analytics operations
│   │   ├── Oidc/                                   # OIDC configuration
│   │   ├── Invitations/                            # Invitation management
│   │   ├── Datasets/                               # Dataset operations
│   │   ├── Consent/                                # Consent documents
│   │   ├── Queries/                                # Query operations
│   │   ├── Runs/                                   # Query run tracking
│   │   └── AuditEvents/                            # Audit event listing
│   ├── Options/
│   │   ├── ManagedCleanroomOptionDescriptions.cs
│   │   ├── Collaboration/
│   │   │   └── [Options classes - mixed status]
│   │   └── [Options for all command groups]
│   ├── Models/
│   │   └── CollaborationCreateResult.cs
│   └── Services/
│       ├── IManagedCleanroomServiceDataPlane.cs
│       ├── IManagedCleanroomServiceControlPlane.cs
│       ├── ManagedCleanroomDataPlaneService.cs
│       └── ManagedCleanroomControlPlaneService.cs
└── tests/
    └── Azure.Mcp.Tools.ManagedCleanroom.Tests/
        ├── CollaborationArm/
        │   ├── CollaborationCreateCommandTests.cs 
        │   └── [Other tests - ⏳]
        ├── Collaborations/
        │   ├── CollaborationsListCommandTests.cs 
        │   └── [Other tests - ⏳]
        └── [Tests for remaining command groups - ⏳]
```

---

## Implementation Notes

- **Completed**: `collaborations list`, `collaboration create`
- **Pending**: 25 additional commands across 9 command groups
- Commands span both data plane and control plane operations

