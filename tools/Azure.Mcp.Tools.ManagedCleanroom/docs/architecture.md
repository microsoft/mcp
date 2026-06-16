# Azure Managed Cleanroom MCP Toolset - Architecture

## Overview

`Azure.Mcp.Tools.ManagedCleanroom` provides comprehensive operations for interacting with Azure Managed Cleanroom services. Commands are organized into logical groups for managing collaborations, analytics workloads, OIDC configuration, datasets, queries, consent documents, and audit events.

Commands interact with:
- **Data Plane APIs**: Cleanroom Analytics Frontend for read operations (list collaborations, queries, datasets)
- **Control Plane APIs**: Azure Resource Manager (ARM) for write operations (create collaboration, manage resources)

---

## Command Surfaces

| Command Group | Command | Plane | Status |
| --- | --- | --- | --- |
| Collaborations | `collaborations list` | Data Plane | Completed |
| Collaborations | `collaborations get` | Data Plane | Pending |
| Analytics | `analytics get` | Data Plane | Pending |
| Analytics | `analytics skr-policy` | Data Plane | Pending |
| OIDC | `oidc issuer-info` | Data Plane | Pending |
| OIDC | `oidc keys` | Data Plane | Pending |
| OIDC | `oidc set-issuer-url` | Data Plane | Pending |
| Collaboration | `collaboration create` | Control Plane | Completed |
| Collaboration | `collaboration get` | Control Plane | Pending |
| Collaboration | `collaboration add-collaborator` | Control Plane | Pending |
| Collaboration | `collaboration enable-workload` | Control Plane | Pending |
| Collaboration | `collaboration get-readonly-kubeconfig` | Control Plane | Pending |
| Invitations | `invitations list` | Data Plane | Pending |
| Invitations | `invitations accept` | Data Plane | Pending |
| Datasets | `datasets publish` | Data Plane | Pending |
| Datasets | `datasets get` | Data Plane | Pending |
| Datasets | `datasets list` | Data Plane | Pending |
| Consent | `consent put` | Data Plane | Pending |
| Queries | `queries publish` | Data Plane | Pending |
| Queries | `queries get` | Data Plane | Pending |
| Queries | `queries list` | Data Plane | Pending |
| Queries | `queries vote` | Data Plane | Pending |
| Queries | `queries run` | Data Plane | Pending |
| Queries | `queries runs` | Data Plane | Pending |
| Runs | `runs get` | Data Plane | Pending |
| Audit Events | `auditevents list` | Data Plane | Pending |

---

## Project Structure

```
Azure.Mcp.Tools.ManagedCleanroom/
├── src/
│   ├── ManagedCleanroomSetup.cs                      # DI registration & command tree
│   ├── Commands/
│   │   ├── ManagedCleanroomJsonContext.cs            # AOT-safe JSON serialization
│   │   ├── Collaboration/
│   │   │   ├── CollaborationCreateCommand.cs (✅)
│   │   │   └── [Other collaboration commands - ⏳]
│   │   ├── Collaborations/
│   │   │   ├── CollaborationsListCommand.cs (✅)
│   │   │   └── [Other collaboration commands - ⏳]
│   │   ├── Analytics/                              # ⏳ Analytics operations
│   │   ├── Oidc/                                   # ⏳ OIDC configuration
│   │   ├── Invitations/                            # ⏳ Invitation management
│   │   ├── Datasets/                               # ⏳ Dataset operations
│   │   ├── Consent/                                # ⏳ Consent documents
│   │   ├── Queries/                                # ⏳ Query operations
│   │   ├── Runs/                                   # ⏳ Query run tracking
│   │   └── AuditEvents/                            # ⏳ Audit event listing
│   ├── Options/
│   │   ├── ManagedCleanroomOptionDefinitions.cs
│   │   ├── Collaboration/
│   │   │   └── [Options classes - mixed status]
│   │   └── [Options for all command groups]
│   └── Services/
│       ├── IManagedCleanroomService.cs
│       └── ManagedCleanroomService.cs
└── tests/
    └── Azure.Mcp.Tools.ManagedCleanroom.Tests/
        ├── Collaboration/
        │   ├── CollaborationCreateCommandTests.cs (✅)
        │   └── [Other tests - ⏳]
        ├── Collaborations/
        │   ├── CollaborationsListCommandTests.cs (✅)
        │   └── [Other tests - ⏳]
        └── [Tests for remaining command groups - ⏳]
```

---

## Implementation Notes

- **Completed**: `collaborations list`, `collaboration create`
- **Pending**: 25 additional commands across 9 command groups
- Commands span both data plane and control plane operations

