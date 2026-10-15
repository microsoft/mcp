# SDK Upgrade Analysis: Usage Plan & Recovery Plan (preview → GA)

> Status: **Analysis only — no code changes made yet.**
> Scope: `Azure.ResourceManager.ResilienceManagement` SDK changes that affect the Usage Plan and Recovery Plan MCP tools only.

## SDK version change

| | Preview (current MCP dependency) | GA (target) |
|---|---|---|
| Package version | `1.0.0-beta.1` | `1.0.0` |
| API version | `2026-04-01-preview` | `2026-10-01` |

`Directory.Packages.props` in this repo still pins `Azure.ResourceManager.ResilienceManagement` to `1.0.0-beta.1`. Bumping to `1.0.0` is the trigger for all changes below.

## Usage Plan changes

| # | SDK change | Affected MCP tool(s) | Location | Notes / decision needed |
|---|---|---|---|---|
| 1 | `UsagePlanType.Basic` enum member **removed** (only `Standard` remains) | `usageplan_create` (and any command that maps `UsagePlanKind` → SDK `UsagePlanType`) | `src/Services/ResilienceManagementService.cs` — `CreateUsagePlanAsync`, switch expression around line 2192 (`UsagePlanKind.Basic => UsagePlanType.Basic`) | MCP's own `UsagePlanKind` enum (`src/Models/UsagePlanKind.cs`) still exposes both `Basic` and `Standard`. Need to decide: (a) remove `Basic` from `UsagePlanKind` entirely (breaking for MCP tool callers), (b) keep `Basic` in MCP's enum but map it to `Standard` under the hood, or (c) keep `Basic` and throw a clear "unsupported in GA" error. |
| 2 | `EnrollmentProperties` renamed to `UsagePlanEnrollmentProperties` | `usageplan_enrollment_create` | `src/Services/ResilienceManagementService.cs` — `CreateUsagePlanEnrollmentAsync`, around line 2248 | Pure rename, no behavior change. Update type reference only. |

No Usage Plan command/option/model files outside the service layer reference these SDK types directly.

## Recovery Plan / Recovery Job changes

GA renamed 16 types for disambiguation (prefixing with `Resilience`/`Recovery` to avoid collisions across the wider SDK surface). `JobType`, `ExecutionConfigurations`, `RecoveryJobProperties`, and `RecoveryJobResourceProperties` were **not** renamed.

| # | Old (preview) type | New (GA) type |
|---|---|---|
| 1 | `JobErrorInfo` | `ResilienceManagementJobErrorInfo` |
| 2 | `OperationQualificationDetails` | `RecoveryOperationQualificationDetails` |
| 3 | `RecoveryOperationNames` | `RecoveryOperationName` |
| 4 | `FailoverRequestProperties` | `ResilienceManagementFailoverProperties` |
| 5 | `UserConsent` | `RecoveryActionUserConsent` |
| ... | *(remaining 11 renames follow the same disambiguation pattern; see SDK changelog for full list)* | |

### Affected MCP tools and files

| File | Method(s) | Approx. lines | What needs to change |
|---|---|---|---|
| `src/Services/ResilienceManagementService.cs` | `CreateFailoverRequestContent` | ~670–685 | `FailoverRequestProperties` → `ResilienceManagementFailoverProperties`; `UserConsent` → `RecoveryActionUserConsent`. Shared by `recoveryplan_failover` and `recoveryplan_validateforfailover`. |
| | `CreateRecoveryPlanValidateForFailoverResult` | ~776–786 | `OperationQualificationDetails` → `RecoveryOperationQualificationDetails` |
| | `ValidateRecoveryPlanForOperationAsync` / core | ~840, ~849 | `RecoveryOperationNames` → `RecoveryOperationName` |
| | `CheckRecoveryPlanReadinessCoreAsync` chain | ~949–1225 | Multiple renamed types referenced across the readiness-check chain (largest concentration of changes, 21 matched references across 6 methods total in this file) |
| | `CreateUsagePlanAsync` | ~2175–2206 | See Usage Plan change #1 above |
| | `CreateUsagePlanEnrollmentAsync` | ~2234–2257 | See Usage Plan change #2 above |
| `src/Services/IResilienceManagementService.cs` | interface signature | line 54 | `RecoveryOperationNames` → `RecoveryOperationName` |
| `src/Commands/Recovery/Plans/RecoveryPlanValidateForOperationCommand.cs` | command logic | lines 51, 74, 78, 84, 90, 96, 102 | Rename updates matching the service-layer changes |
| `src/Models/UsagePlanKind.cs` | enum | n/a | Decision needed re: `Basic` member (see Usage Plan #1) |
| `src/Common/ResilienceManagementJsonContext.cs` | — | — | **No changes needed** — zero matches against renamed types |

### Confirmed false positives (no change needed)

These files matched renamed-type names in a naive search but were manually verified to reference **MCP's own types**, not the SDK types being renamed:
- `src/Models/RecoveryPlanFailoverOption.cs`
- `src/Models/RecoveryPlanValidateForFailoverOption.cs`
- `src/Models/RecoveryPlanFailoverQualification.cs`
- `src/Commands/Recovery/Plans/RecoveryPlanFailoverCommand.cs`
- `src/Commands/Recovery/Plans/RecoveryPlanValidateForFailoverCommand.cs`

### Test impact

`tests/Azure.Mcp.Tools.ResilienceManagement.Tests/Services/ResilienceManagementServiceTests.cs` directly constructs/reads the renamed SDK type `JobErrorInfo` via `ModelReaderWriter.Read<JobErrorInfo>(...)` at lines **146** and **495**. These will need to change to `ResilienceManagementJobErrorInfo` once the SDK is bumped. (References to `result.FailoverRequestProperties` at lines 518–521 are MCP's own result-model property name, not the SDK type, so no change is needed there purely from the rename — only if the property's declared type itself changes to reference the renamed SDK type.)

## Summary

- **Usage Plan**: 2 changes — one enum member removal requiring a behavioral decision, one pure rename.
- **Recovery Plan**: 16 type renames (disambiguation only, no behavior change) rippling through 1 service file (6 methods / 21 reference sites), 1 interface, 1 command file, and at least 1 test file.
- No implementation has been performed yet; this document captures the analysis so it can be used as the basis for a follow-up implementation pass (SDK bump in `Directory.Packages.props`, service/command/model updates, and test updates).
