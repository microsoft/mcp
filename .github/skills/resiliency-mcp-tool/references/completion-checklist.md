# Resilience Management Completion Checklist

## Implementation

- [ ] One command and one clear command path are in scope.
- [ ] Correct `AuthenticatedCommand` or `SubscriptionCommand` base selected.
- [ ] Options use `[Option]`; filename follows the local singular `Option.cs` convention.
- [ ] Resource-specific validation and payload limits are covered.
- [ ] Command metadata matches actual mutation/idempotency behavior.
- [ ] Service interface and implementation propagate tenant and cancellation.
- [ ] LRO uses `WaitUntil.Started` plus `WaitForLroCompletionAsync` where applicable.
- [ ] Mutating LRO result is hydrated with a follow-up GET when required.
- [ ] Response types are registered in `ResilienceManagementJsonContext`.
- [ ] Command is registered in DI and the correct command-group node.
- [ ] Logging contains only individually selected safe values.
- [ ] Catch blocks call `HandleException`.

## Tests and Fixtures

- [ ] Focused unit tests cover binding, validation, service arguments, serialization, and errors.
- [ ] Test base matches the command base.
- [ ] Live test invokes the emitted MCP tool name.
- [ ] Destructive/mutually exclusive tests use isolated resources.
- [ ] Bicep and post-deploy outputs cover every dynamic test input.
- [ ] Cleanup orders children before parents and accepts already-absent resources.
- [ ] Live fixture reaches the exact backend operation before recording.
- [ ] Recording is inspected for secrets and unstable data.
- [ ] Focused playback passes.
- [ ] Full ResilienceManagement playback passes in the CI-equivalent configuration.
- [ ] Test proxy `push` generated the final `assets.json` tag.
- [ ] Combined tag retains both `main` and branch recordings.

## Discovery and Documentation

- [ ] `consolidated-tools.json` maps the exact emitted tool name.
- [ ] Consolidated metadata and description agree with the command.
- [ ] `azmcp-commands.md` contains the command and options.
- [ ] `e2eTestPrompts.md` contains at least two natural-language prompts.
- [ ] README capability/examples updated when applicable.
- [ ] ToolDescriptionEvaluator ranks the tool in the top three with score at least `0.4` for each prompt.
- [ ] Changelog entry has a non-empty `changes` array, valid section, and description.
- [ ] Legitimate new domain words are added to the toolset `cspell.yaml`.

## Ordered Validation

Run from the repository root and use the exact test-platform syntax detected in the project:

```powershell
dotnet build tools/Azure.Mcp.Tools.ResilienceManagement/src
dotnet test tools/Azure.Mcp.Tools.ResilienceManagement/tests/Azure.Mcp.Tools.ResilienceManagement.Tests --filter "FullyQualifiedName~<CommandTests>"
dotnet format Microsoft.Mcp.slnx --verify-no-changes --include "tools/Azure.Mcp.Tools.ResilienceManagement/**"
dotnet build servers/Azure.Mcp.Server/src/Azure.Mcp.Server.csproj
servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp.exe tools list --namespace resilience --mode all
dotnet test core/Azure.Mcp.Core/tests/Azure.Mcp.Core.Tests/Azure.Mcp.Core.Tests.csproj -- --filter-class '*ConsolidatedToolDiscoveryStrategyTests'
./eng/scripts/Build-Local.ps1 -VerifyNpx
./eng/scripts/Build-Local.ps1 -BuildNative
./eng/common/spelling/Invoke-Cspell.ps1
```

Also run the ResilienceManagement focused/full playback checks from the live-test TSG. Do not run live tests during ordinary validation unless recording or an explicit live check is required.

## Repository Hygiene

- [ ] `git diff --check` passes.
- [ ] No `.proxy`, `.assets`, `.artifacts`, bin/obj, credentials, or local settings are staged.
- [ ] Unrelated user changes are untouched.
- [ ] Generated docs/metadata are updated only through the repository script when required.
- [ ] PR contains the repository's required `## Invoking Livetests` section.
- [ ] Any skipped command or external service blocker is documented with exact evidence.

## Source Map

- General lifecycle: `.github/skills/add-azure-mcp-tools/SKILL.md`
- Contributor onboarding: `.github/agents/onboarding.agent.md`, `CONTRIBUTING.md`, `AGENTS.md`
- Recorded testing: `docs/recorded-tests.md`
- Server troubleshooting: `servers/Azure.Mcp.Server/TROUBLESHOOTING.md`
- AOT guidance: `docs/aot-compatibility.md`
- Sovereign clouds: `docs/sovereign-clouds.md`
- Changelog schema: `docs/changelog-entries.md`
- Tool descriptions: `eng/tools/ToolDescriptionEvaluator/README.md`
- PR checklist: `.github/PULL_REQUEST_TEMPLATE.md`
- Team onboarding/TSG: `ResiliencyHub-Common/docs/Goals/SDKAndMCP/MCPRelease.md`
- Current fixture truth: `tools/Azure.Mcp.Tools.ResilienceManagement/tests/test-resources.bicep`, `test-resources-post.ps1`, and `remove-test-resources-pre.ps1`