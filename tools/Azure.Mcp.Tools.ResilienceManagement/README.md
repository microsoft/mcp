# Resilience Management tools

Drill report-generation and report-download implementations must also be registered in
`ResilienceManagementSetup`. The runtime tool inventory, consolidated metadata, and
evaluation prompts must agree; keeping command classes alone does not expose the tools.

## Individual Drill read-back

`resiliency drill get --service-group <group> --name <drill>` uses the same
`2026-10-31-preview` parent Drill contract as creation and update. The older SDK
default contract can omit Health Model and SLI monitoring configuration and newer
attention fields, even when creation succeeded and monitoring remains configured.
The getter preserves returned properties without adding identities or changing
permissions.

This read is GET-only. Existing endpoint and optional routing-header behavior is
unchanged. Child Drill Resources and Drill Runs keep their existing API contracts.
Listing drills remains an ID/name summary.

`ResilienceManagementServiceTests.GetDrillCommand_UsesCurrentContractAndPreservesMonitoring`
exercises command parsing, the real service/SDK pipeline, and source-generated
result serialization against a version-sensitive in-memory transport. Companion
tests show the older response omits monitoring and check child-contract isolation.
These tests require no Azure authentication, resources, or live calls.

## Regional Goal Resource read-back

Individual Goal Resource reads and participation updates use the same Regional-capable
`2026-10-31-preview` contract as Goal creation and refresh. Resource reads preserve
`regionalResiliency` from the provider response alongside existing zonal information;
the installed SDK's typed model does not expose this Regional field. Missing Regional
data stays missing, rather than inventing participation or repeatedly requesting an
update when the provider already reports `Included`.
