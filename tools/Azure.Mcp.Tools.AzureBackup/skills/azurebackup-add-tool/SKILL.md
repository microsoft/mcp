---
name: azurebackup-add-tool
description: 'Add a new tool/command to the Azure Backup MCP toolset. Covers the full lifecycle: command implementation, options, service layer, input validation, unit tests, live tests, recorded test playback, CI validation, spell check, changelog entry, tool description evaluation, and PR checklist. USE WHEN: add new backup command, create backup tool, implement backup operation, new azurebackup command, add MCP tool for backup, new vault operation, new policy command, new governance command.'
argument-hint: 'Describe the new Azure Backup tool to add (e.g., "add security configure-mua command")'
---

# Add a New Tool to Azure Backup MCP

## Purpose

Step-by-step workflow for adding a new command to the Azure Backup MCP toolset,
ensuring it passes all validation gates before PR submission.

## When to Use

- Adding a new `azmcp azurebackup <group> <operation>` command
- Extending an existing command group (vault, policy, protecteditem, etc.)
- Adding a new command group (security, compliance, etc.)

## Prerequisites

- `.NET 10 SDK` installed (see `global.json`)
- Azure authentication configured (`az login` / `Connect-AzAccount`)
- Repository cloned with `upstream` remote pointing to `microsoft/mcp`
- Branch created from `upstream/main`

## Procedure

### Phase 1: Implementation

Follow [`/.github/skills/add-azure-mcp-tools/SKILL.md`](https://github.com/microsoft/mcp/blob/main/.github/skills/add-azure-mcp-tools/SKILL.md) as the authoritative guide.
The Azure Backup toolset lives in `tools/Azure.Mcp.Tools.AzureBackup/`.

#### 1a. Create Options

File: `src/Options/{Group}/{Resource}{Operation}Options.cs`

```csharp
public sealed class MyNewOptions : ISubscriptionOption
{
  [Option(Description = "The command-specific value.")]
  public required string MyParam { get; set; }

  [Option(Description = OptionDescriptions.ResourceGroup)]
  public required string ResourceGroup { get; set; }

  [Option(Description = OptionDescriptions.Subscription)]
  public string? Subscription { get; set; }

  [Option(Description = OptionDescriptions.Tenant)]
  public string? Tenant { get; set; }
}
```

- Put `[Option(Description = ...)]` on every exposed property
- Use `OptionDescriptions` for common descriptions and `AzureBackupOptionDefinitions` for reusable Azure Backup descriptions or explicit option-name constants
- Use the C# `required` modifier for required command options; use nullable properties for optional values
- Implement `ISubscriptionOption`; `SubscriptionCommand` resolves subscription IDs or names
- New commands should use flat options. Existing inherited options classes are transitional code and do not require new commands to add more inheritance.

#### 1b. Add Service Method

File: `src/Services/IAzureBackupService.cs` and `src/Services/AzureBackupService.cs`

- Add the interface method first
- Route to `rsvOps` or `dppOps` based on vault type using `ResolveVaultTypeAsync`
- For RSV-only operations, add to `IRsvBackupOperations` / `RsvBackupOperations`
- For DPP-only operations, add to `IDppBackupOperations` / `DppBackupOperations`

#### 1c. Implement the Command

File: `src/Commands/{Group}/{Resource}{Operation}Command.cs`

Required patterns:
- Use `[CommandMetadata(...)]` attribute (not property overrides)
- Sealed class with primary constructor
- Inject `ILogger<T>`, `IAzureBackupService`, and `ISubscriptionResolver`
- Inherit from `SubscriptionCommand<TOptions, TResult>`; use `BaseAzureBackupCommand<TOptions, TResult>` only when deliberately extending its existing shared vault-type validation
- Override `ExecuteAsync` and, when semantic validation is needed, `ValidateOptions`
- Do not implement `RegisterOptions` or `BindOptions`; `OptionBinder` handles attributed options
- Add telemetry tags via `AzureBackupTelemetryTags.AddVaultTags(context.Activity, ...)`
- Call `HandleException(context, ex)` in catch blocks
- Keep the public sealed result record nested in the command class and register it in `AzureBackupJsonContext`

#### 1d. Register the Command

File: `src/AzureBackupSetup.cs`

- Add `services.AddSingleton<MyNewCommand>()` in `ConfigureServices`
- Add `group.AddCommand<MyNewCommand>(serviceProvider)` in `RegisterCommands`
- Create a new `CommandGroup` if needed for a new group

#### 1e. Register JSON Serialization Context

File: `src/Commands/AzureBackupJsonContext.cs`

- Add `[JsonSerializable(typeof(MyNewCommand.MyResultType))]` for AOT safety

### Phase 2: Input Validation

Before writing tests, validate all inputs are handled correctly:

**Checklist:**
- [ ] Required command options use the C# `required` modifier
- [ ] Semantic and conditional rules are implemented in `ValidateOptions`
- [ ] Subscription is delegated to `ISubscriptionResolver`; do not require GUID format because subscription names are supported
- [ ] Vault type normalized correctly (rsv/dpp case-insensitive)
- [ ] Enum/string parameters validated against allowed values with helpful error listing
- [ ] Service entry points validate required values defensively when they can be called outside a command
- [ ] ARM resource IDs parsed safely with try-catch on `new ResourceIdentifier(...)`
- [ ] Error messages are actionable (tell user what to provide, not just what failed)

### Phase 3: Unit Tests

File: `tests/Azure.Mcp.Tools.AzureBackup.Tests/{Group}/{Resource}{Operation}CommandTests.cs`

#### Required Test Methods

```csharp
public class MyNewCommandTests : SubscriptionCommandUnitTestsBase<MyNewCommand, IAzureBackupService>
{
    [Fact] public void Constructor_InitializesCommandCorrectly()
    [Fact] public async Task ExecuteAsync_ValidInput_ReturnsExpectedResult()
    [Fact] public async Task ExecuteAsync_HandlesServiceErrors()
    [Fact] public async Task ExecuteAsync_DeserializationValidation()

  // Add missing/invalid option tests through the command parser:
    [Theory]
    [InlineData("")]
  [InlineData("--subscription sub123")]
  public async Task ExecuteAsync_MissingRequiredOptions_ReturnsBadRequest(string args)

    // Add edge case tests specific to the command
}
```

#### Run Unit Tests

```powershell
dotnet test tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.Tests `
  --filter "FullyQualifiedName~MyNewCommandTests"
```

Verify **all tests pass** before proceeding.

### Phase 4: Live Tests

File: `tests/Azure.Mcp.Tools.AzureBackup.Tests/AzureBackupCommandTests.cs`

#### 4a. Add Test Methods

Azure Backup live tests use `[Fact]` on a `RecordedCommandTestsBase` subclass with `CallToolAsync`.
There is no `[RecordedTest]` attribute in this toolset.

```csharp
[Fact]
public async Task MyNewCommand_RsvVault()
{
    var result = await CallToolAsync(
      "azurebackup_mygroup_myop",
      new()
        {
        { "subscription", Settings.SubscriptionId },
        { "resource-group", Settings.ResourceGroupName },
        { "vault", $"{Settings.ResourceBaseName}-rsv" },
            // add other params
        });

    var value = result.AssertProperty("{result-property}");
    // Assert the recorded response structure and stable values.
}
```

- Use `[Fact]` for all live tests (not `[RecordedTest]` — that attribute is not used in Azure Backup)
- Use `[LiveTestOnly]` alongside `[Fact]` for long-running E2E tests that cannot reliably replay
- Use `Settings.ResourceBaseName` and `Settings.ResourceGroupName` for common deployed names
- Use `RegisterOrRetrieveDeploymentOutputVariable` for Bicep outputs needed during playback
- Wrap generated names and timestamps with `RegisterOrRetrieveVariable`

#### 4b. Update Test Infrastructure (if needed)

If the new command requires new Azure resources:

1. Edit `tests/test-resources.bicep` to add the resource
2. Edit `tests/test-resources-post.ps1` when deterministic data seeding or post-deployment setup is needed
3. Deploy: `./eng/scripts/Deploy-TestResources.ps1 -Paths "AzureBackup"`

#### 4c. Record Live Tests

```powershell
# Deploy resources and generate .testsettings.json
./eng/scripts/Deploy-TestResources.ps1 -Paths "AzureBackup"

# Change TestMode in the generated .testsettings.json from Live to Record.

# Run tests in Record mode
dotnet test tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.Tests `
  --filter "FullyQualifiedName~MyNewCommand"
```

#### 4d. Push Recordings

```powershell
# Push recorded sessions to azure-sdk-assets
./.proxy/Azure.Sdk.Tools.TestProxy.exe push `
  -a tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.Tests/assets.json
```

On Unix, omit the `.exe` suffix.

This updates the `Tag` field in `assets.json`. **Commit the updated `assets.json`.**

#### 4e. Verify Playback

```powershell
# Change TestMode in .testsettings.json from Record to Playback, then rerun.

dotnet test tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.Tests `
  --filter "FullyQualifiedName~MyNewCommand"
```

All recorded tests **must pass in Playback mode**.

### Phase 5: CI Validation Gates

Run these checks in order. **All must pass before creating a PR.**

#### 5a. Build

```powershell
dotnet build tools\Azure.Mcp.Tools.AzureBackup\src\Azure.Mcp.Tools.AzureBackup.csproj /p:NuGetAudit=false
```

#### 5b. Format Check

```powershell
dotnet format Microsoft.Mcp.slnx --verify-no-changes `
  --include "tools/Azure.Mcp.Tools.AzureBackup/**" `
  --exclude-diagnostics IL2026 IL3050
```

If it fails, fix with:
```powershell
dotnet format Microsoft.Mcp.slnx `
  --include "tools/Azure.Mcp.Tools.AzureBackup/**" `
  --exclude-diagnostics IL2026 IL3050
```

#### 5c. Full Unit Tests

```powershell
./eng/scripts/Test-Code.ps1 -TestType Unit -Paths "AzureBackup"
```

#### 5d. Full Live Tests (Playback)

```powershell
./eng/scripts/Test-Code.ps1 -TestType Recorded -Paths "AzureBackup"
```

#### 5e. Spell Check

```powershell
.\eng\common\spelling\Invoke-Cspell.ps1
```

If new Azure Backup-specific technical terms are flagged, add them to `tools/Azure.Mcp.Tools.AzureBackup/cspell.yaml`. Add cross-cutting terms used by multiple projects to `.vscode/cspell.json`.

#### 5f. Full Build Verification

```powershell
./eng/scripts/Build-Local.ps1 -VerifyNpx
```

#### 5g. AOT/Native Build Verification

Azure Backup is marked `IsAotCompatible=true`, so also validate native compilation:

```powershell
./eng/scripts/Build-Local.ps1 -BuildNative
```

If this fails, follow `docs/aot-compatibility.md`. Do not modify the fixed
`#if !BUILD_NATIVE` block or conditionally remove the toolset from native builds.

### Phase 6: Tool Description Evaluation

Run the ToolDescriptionEvaluator to verify the new tool's description is discoverable by AI agents.

```powershell
$env:AOAI_ENDPOINT = "<your-aoai-endpoint>"
$env:TEXT_EMBEDDING_API_KEY = "<your-key>"

dotnet run --project eng/tools/ToolDescriptionEvaluator/src -- --test-single-tool `
  --tool-description "<the CommandMetadata description>" `
  --prompt "<a representative user request>" `
  --prompt "<an alternate user request>"
```

**Target:** Top 3 ranking with confidence score >= 0.4.

If the score is low, improve the command's `Description` in the `[CommandMetadata]` attribute:
- Include key verbs users would say ("configure", "enable", "list", "show")
- Mention specific resource types ("vault", "policy", "protected item")
- Describe what the output looks like
- Re-run until the score meets the threshold

### Phase 7: Documentation

#### 7a. Update Command Reference

File: `servers/Azure.Mcp.Server/docs/azmcp-commands.md`

Add the new command in alphabetical order within the azurebackup section.

Then regenerate the commands metadata:
```powershell
./eng/scripts/Update-AzCommandsMetadata.ps1
```
This is required for CI validation.

#### 7b. Add Test Prompts

File: `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`

Add 2-3 natural language prompts that should trigger the new tool, in alphabetical order. Include the `Interaction` column using the values defined at the top of that file.

#### 7c. Create Changelog Entry

Follow `docs/changelog-entries.md` instructions. Use the `-ChangelogPath` parameter pointing to
`servers/Azure.Mcp.Server/CHANGELOG.md`.

### Phase 8: PR Submission

#### Final Checklist

Before creating the PR, verify:

- [ ] **Build passes:** `dotnet build` succeeds with 0 errors
- [ ] **Format clean:** `dotnet format --verify-no-changes` passes
- [ ] **All unit tests pass** (including existing ones — no regressions)
- [ ] **All live tests pass in Playback mode**
- [ ] **Recordings pushed** and `assets.json` updated
- [ ] **Spell check passes:** `Invoke-Cspell.ps1` clean
- [ ] **ToolDescriptionEvaluator:** Score >= 0.4, top 3 ranking
- [ ] **Command registered** in `AzureBackupSetup.cs`
- [ ] **JSON context registered** for AOT safety
- [ ] **Telemetry tags added** via `AzureBackupTelemetryTags`
- [ ] **Documentation updated** (`azmcp-commands.md`, `e2eTestPrompts.md`, changelog entry, and `servers/Azure.Mcp.Server/README.md` when applicable)
- [ ] **Commands metadata regenerated** via `Update-AzCommandsMetadata.ps1`
- [ ] **AOT/native build passes** (`Build-Local.ps1 -BuildNative`)
- [ ] **One tool per PR** (don't bundle unrelated changes)

#### Create the PR

```powershell
git add <changed-files>
git commit -m "feat(azurebackup): Add <group> <operation> command

<description of what the command does>"
git push origin <branch-name>
```

## Reference: File Locations

```
tools/Azure.Mcp.Tools.AzureBackup/
├── src/
│   ├── AzureBackupSetup.cs                           # Register here
│   ├── Commands/
│   │   ├── AzureBackupJsonContext.cs                  # AOT registration
│   │   └── {Group}/{Resource}{Operation}Command.cs    # Command impl
│   ├── Options/
│   │   ├── AzureBackupOptionDefinitions.cs            # Shared descriptions and explicit option names
│   │   └── {Group}/{Resource}{Operation}Options.cs    # Command options
│   ├── Services/
│   │   ├── IAzureBackupService.cs                     # Interface
│   │   ├── AzureBackupService.cs                      # Routing
│   │   ├── RsvBackupOperations.cs                     # RSV impl
│   │   └── DppBackupOperations.cs                     # DPP impl
│   └── Models/
│       └── AzureBackupTelemetryTags.cs                # Telemetry
└── tests/
    ├── Azure.Mcp.Tools.AzureBackup.Tests/
    │   ├── {Group}/{Resource}{Operation}CommandTests.cs
    │   ├── AzureBackupCommandTests.cs                 # Add tests here
    │   └── assets.json                                # Recording tag
    ├── test-resources.bicep                           # Azure infra
    └── test-resources-post.ps1                        # Post-deploy
```

## Reference: Good Examples

Study these existing implementations as templates:

- **Simple get/list:** `Commands/Vault/VaultGetCommand.cs`
- **Create with validation:** `Commands/Policy/PolicyCreateCommand.cs`
- **Governance toggle:** `Commands/Governance/GovernanceSoftDeleteCommand.cs`
- **Security command:** `Commands/Security/SecurityConfigureMuaCommand.cs`
- **Unit tests:** `tests/Azure.Mcp.Tools.AzureBackup.Tests/Policy/PolicyCreateCommandTests.cs`
