<!-- cspell:ignore RHDS -->

# Resilience Management Live-Test TSG

This TSG extends `docs/recorded-tests.md` for the Resilience Management fixture.

## Preflight

1. Build the source and pass the focused unit-test class.
2. Ensure the branch contains the intended latest `main` recording baseline before recording.
3. Sign in to both clients using the same tenant and subscription:

```powershell
Connect-AzAccount
az login --tenant <tenant-id>
az account set --subscription <subscription-id-or-name>
```

4. Confirm PowerShell 7, the repository-pinned .NET SDK, Azure PowerShell, Azure CLI, and Bicep are available.
5. Keep local credentials and `.testsettings.json` out of commits.

## Fixture Architecture

- `tests/test-resources.bicep` creates resource-group-scoped resources and exports stable names/IDs.
- `tests/test-resources-post.ps1` creates tenant/service-group-scoped resources with direct ARM REST calls, because a resource-group deployment cannot create the tenant-scoped service-group graph.
- Provision in dependency order: service group, memberships, usage-plan enrollment, goal resources, recovery plan, drill, included drill resources, then drill runs/actions.
- `tests/remove-test-resources-pre.ps1` removes children before parents and accepts 404 as success.
- The post script must poll provisioning and tolerate transient 404/403 while service-group authorization propagates.

Use separate fixtures for destructive or mutually exclusive lifecycle tests. The service has enforced constraints such as one concurrent Zonal drill and one resource of a given plan/drill type per service group. A delete test must never consume the drill used by list/get/update tests.

## Deploy

From the repository root:

```powershell
./eng/scripts/Deploy-TestResources.ps1 `
  -Paths ResilienceManagement `
  -ResourceGroupName <resource-group> `
  -BaseName <unique-base-name>
```

Use a fresh compliant resource group when policy tags or stale resources make reuse unsafe. Do not patch `eng/common/TestResources/New-TestResources.ps1` to bypass policy. If the resource group already exists, verify its required policy tags before deployment.

The deployment is not ready merely because Bicep succeeded. The post script must finish and populate every output required by the focused live test.

## Resilience-Specific Provisioning Blockers

### Authentication and authorization

- `az login` does not authenticate Azure PowerShell; `Connect-AzAccount` does not guarantee the Azure CLI subprocess has the same context.
- Enrollment links a subscription usage plan to a tenant-scoped service group. The caller must be able to read the service group and write the enrollment.
- Service-group automatic administrator and linked authorization can take time to propagate. A correct assignment may still return transient 403.
- If the test application differs from the signed-in provisioning user, assign `Azure Resilience Management Recovery Contributor` at the service-group scope.
- Do not repeatedly add an existing assignment; detect and tolerate the conflict.

### REST and fixture correctness

- Resilience post-actions requiring `operation-id` expect it as an HTTP header, not a query parameter.
- Use `drillResources` for drill resources and `drillRunTargets` for run resources; verify paths against the current SDK/RP contract.
- A discovered resource is not automatically a valid fault target. Confirm it exposes a supported fault and reaches `Included` before starting a run.
- Preserve service-discovered `faultProperties` when including a drill resource.
- A recovery plan must be ready, have a valid default recovery-group ID, and belong to the same service group as the drill.
- Drill monitoring identity, chaos identity, recovery-plan identity, subscription, region, and resource group must match the expected service contract.
- A drill PUT is create-or-update. Reusing a drill with a different supporting resource group can fail because its automation account already exists in the original group.

### Service/backend blockers

- Service groups can enforce one concurrent Zonal drill. Deletion visibility can lag, so verify absence and backend count before recreating.
- `RHDSUserErrorServiceGroupConcurrentDrillsLimitExceeded` is environmental, not a command parser failure.
- `RHDSUserErrorDrillEntityNotFound` after ARM shows the drill can indicate Drills data-plane/control-plane synchronization or rollout issues.
- `ResourcePostActionFailed` with a backend 500 after `addOrUpdateResources` means the request reached the service; inspect the operation result before changing MCP code.
- If the same valid fixture fails across fresh names, regions, and supported resource types, compare the current Drills deployment/rollout with the last successful recording. Recordings cannot be produced until a backend regression is fixed or rolled forward.

## Record

Set `TestMode` to `Record` in `tests/.testsettings.json`, then run the smallest affected test filter. Use the repository's current MTP/VSTest syntax; inspect the test project before choosing arguments.

During test authoring:

- Always pass `tenant` when the subscription is in a non-default tenant.
- Use `RegisterOrRetrieveDeploymentOutputVariable` for deployment outputs.
- Use `RegisterOrRetrieveVariable` for generated or discovered dynamic values.
- Sanitize `operation-id`, `x-ms-operation-identifier`, `Location`, and any newly observed unstable identifiers.
- Assertions must still be meaningful after sanitization.

Locate and inspect the generated recording:

```powershell
./.proxy/Azure.Sdk.Tools.TestProxy.exe config locate `
  -a tools/Azure.Mcp.Tools.ResilienceManagement/tests/Azure.Mcp.Tools.ResilienceManagement.Tests/assets.json
```

Confirm no access tokens, tenant-specific identity data, real subscription IDs, unstable timestamps, or unregistered random values remain.

## Playback and Push

1. Set `TestMode` back to `Playback`.
2. Rerun the focused tests.
3. Run the full ResilienceManagement playback suite, including x64/CI-equivalent settings when the pipeline uses them.
4. Push only after all playback tests pass:

```powershell
./.proxy/Azure.Sdk.Tools.TestProxy.exe push `
  -a tools/Azure.Mcp.Tools.ResilienceManagement/tests/Azure.Mcp.Tools.ResilienceManagement.Tests/assets.json
```

5. Verify `assets.json` changed to the new generated tag and remains valid JSON.

An assets tag is one snapshot, not a merge pointer. If `main` and the branch both added recordings, first integrate `main`, restore the resulting baseline, add/re-record the branch sessions on top of it, run full playback, and push one combined snapshot. Never choose one tag and silently lose sessions from the other.

## Failure Classification

| Symptom | Likely class | First check |
|---|---|---|
| No HTTP interaction recorded | Test setup/auth/argument binding | Test output and server startup |
| 401/403 before resource call | Tenant or RBAC | CLI and PowerShell context, service-group access |
| 404 immediately after create | Eventual consistency or wrong path/API | Polling and current SDK resource path |
| LRO succeeds but response ID/name is empty | Hydration/mapping | Follow-up GET and `ModelReaderWriterOptions("J")` |
| Playback request mismatch | Dynamic request material | Variables, headers, body matcher, sanitizers |
| Existing unrelated playback tests disappear | Assets baseline loss | Compare sessions in old and new tags |
| Only CI/x64 playback fails | Recording/build configuration drift | Reproduce exact framework, platform, and full suite |
| Repeated post-action 500 after valid provisioning | Service regression | Operation-status payload and known-good deployment |

Stop retrying after evidence identifies an external service blocker. Preserve the focused unit/playback evidence and report the required service owner or rollout action.