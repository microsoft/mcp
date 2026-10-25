# Resilience Management tools

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
