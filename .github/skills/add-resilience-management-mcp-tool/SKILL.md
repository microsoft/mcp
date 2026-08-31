---
name: add-resilience-management-mcp-tool
description: 'Create or extend Azure Resilience Management MCP commands end to end. USE WHEN: add resilience tool, add drill command, drill run tool, recovery plan command, goal command, usage plan command, record ResilienceManagement live tests, deploy resilience test resources, troubleshoot drill fixtures, or prepare a Resilience Management MCP PR.'
argument-hint: 'Describe one command, for example: add resilience drill run resume'
---

# Add a Resilience Management MCP Tool

Use this workflow for commands under `tools/Azure.Mcp.Tools.ResilienceManagement`.
It specializes the repository-wide [Azure MCP tool skill](../add-azure-mcp-tools/SKILL.md) with the service-group hierarchy, test fixtures, and blockers already encountered in this toolset.

## Operating Rules

- Implement one MCP tool per PR unless the user explicitly groups inseparable operations.
- Read current neighboring code before copying a pattern. Code and current SDK APIs override older guides.
- Do not modify shared deployment infrastructure to bypass a local policy or permission failure.
- Do not invent API shapes. Verify the generated SDK, service interface, existing REST fixtures, or the Drills service source/TSG.
- Never hand-edit the `Tag` in `assets.json`; the test proxy `push` command owns it.
- Never discard existing recordings when reconciling a branch with `main`.
- Use Azure best-practices tooling before planning or generating Azure code when it is available.

## Required Reading

Read only the sections needed for the requested operation:

1. Repository rules: [AGENTS.md](../../../AGENTS.md) and [CONTRIBUTING.md](../../../CONTRIBUTING.md).
2. General command lifecycle: [add-azure-mcp-tools](../add-azure-mcp-tools/SKILL.md).
3. Resilience patterns and decision table: [implementation guide](./references/implementation-guide.md).
4. Any live or recorded test work: [live-test TSG](./references/live-test-tsg.md) and [recorded-tests.md](../../../docs/recorded-tests.md).
5. Final integration and PR work: [completion checklist](./references/completion-checklist.md).
6. Team onboarding source when accessible: [ResiliencyHub MCP Toolset Guide](https://msazure.visualstudio.com/One/_git/ResiliencyHub-Common?path=/docs/Goals/SDKAndMCP/MCPRelease.md&version=GBwiki&_a=preview).

## Workflow

### 1. Establish the Contract

Before editing, state and verify:

- Exact command path: `azmcp resilience <resource hierarchy> <operation>`.
- Whether it is tenant/service-group scoped or subscription/resource-group scoped.
- SDK method or REST operation, API version, request shape, LRO behavior, and response shape.
- Metadata truth table: `Destructive`, `Idempotent`, `OpenWorld`, `ReadOnly`, `Secret`, and `LocalRequired`.
- Required permissions, supported drill/run state, and whether the operation mutates shared state.
- Live fixture prerequisites and cleanup behavior.

Stop and ask only when the public contract or destructive intent cannot be verified locally.

### 2. Choose the Owning Pattern

- Service-group, goal, recovery-plan, drill, and drill-run operations normally use `AuthenticatedCommand<TOptions, TResult>`.
- Subscription/resource-group usage-plan operations use `SubscriptionCommand<TOptions, TResult>` and `ISubscriptionResolver`.
- Use a flat options class with `[Option]` attributes. In this toolset, filenames use singular `Option.cs` while class names use plural `Options`.
- Put new models and enums in separate files. Register every response type in `ResilienceManagementJsonContext`.
- Extend `IResilienceManagementService` and `ResilienceManagementService`; propagate tenant and cancellation tokens.
- Register the command singleton and command-group path in `ResilienceManagementSetup`.

Follow the concrete anchors in the [implementation guide](./references/implementation-guide.md).

### 3. Implement in a Narrow Loop

Make the smallest complete slice in this order:

1. Options and semantic validation.
2. Model/result types and AOT JSON registration.
3. Service interface and implementation.
4. Command metadata, execution, safe logging, and `HandleException`.
5. Setup registration.
6. Focused unit tests using the base class that matches the command base.

After the first substantive edit, run the narrowest build or unit test that can falsify the implementation. Do not defer compile feedback until all files are complete.

### 4. Add an Isolated Live Fixture

For Azure-facing commands, add or extend the recorded integration test and fixture.

- Resource-group resources belong in `tests/test-resources.bicep`.
- Tenant/service-group resources and ordered post-actions belong in `tests/test-resources-post.ps1`.
- Destructive or lifecycle tests must have dedicated names and, when service constraints require it, dedicated service groups or recovery plans.
- Cleanup belongs in `tests/remove-test-resources-pre.ps1` and must tolerate already-absent resources.
- Export every dynamic fixture value through deployment outputs and retrieve it with `RegisterOrRetrieveDeploymentOutputVariable`.
- Use at least one behavior-bearing assertion, not presence checks alone.

Do not start recording until fixture provisioning succeeds and a live invocation of the exact new command reaches the intended backend operation.

### 5. Record and Prove Playback

Follow the [live-test TSG](./references/live-test-tsg.md) exactly:

1. Rebase or merge the intended `main` baseline before recording.
2. Authenticate Azure CLI and Azure PowerShell to the same tenant/subscription.
3. Deploy the ResilienceManagement test resources.
4. Record only the affected tests first.
5. Inspect recordings for secrets and unstable values.
6. Switch to Playback and rerun the same tests.
7. Run the full ResilienceManagement playback suite.
8. Push once with the test proxy and include the resulting `assets.json` change.

If deployment or recording fails, classify it before editing code: contract bug, fixture bug, RBAC/propagation, backend regression, test-proxy mismatch, or stale branch/assets baseline.

### 6. Complete Every Discovery Surface

Update and verify all applicable surfaces:

- `ResilienceManagementSetup.cs`
- `ResilienceManagementJsonContext.cs`
- `servers/Azure.Mcp.Server/src/Resources/consolidated-tools.json`
- `servers/Azure.Mcp.Server/docs/azmcp-commands.md`
- `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`
- `servers/Azure.Mcp.Server/README.md` when the capability list or examples change
- One changelog entry under `servers/Azure.Mcp.Server/changelog-entries/`
- `tools/Azure.Mcp.Tools.ResilienceManagement/cspell.yaml` for legitimate domain terms

Add at least two natural-language selection prompts for each new tool. Validate top-three selection with confidence at least `0.4`.

### 7. Run Gates and Report Blockers

Run the ordered checks in the [completion checklist](./references/completion-checklist.md). Keep generated artifacts, unrelated dirty files, and local `.testsettings.json` changes out of the PR.

When blocked, report:

- Exact failing phase and command.
- First actionable error code/message.
- Evidence showing whether the request reached the service.
- What was already ruled out.
- Smallest owner/action needed to unblock it.
- Whether unit tests and existing playback remain green.

Do not repeatedly redeploy or re-record a confirmed backend failure.

## Definition of Done

The command is implemented, registered, AOT-serializable, unit tested, represented in consolidated mode and docs, evaluated for selection, recorded, playback-verified, and covered by a valid changelog entry. Any skipped gate must be explicit and attributable to a named external blocker.