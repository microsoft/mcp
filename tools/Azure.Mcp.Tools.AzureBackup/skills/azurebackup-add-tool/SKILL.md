---
name: azurebackup-add-tool
description: 'Add a new tool/command to the Azure Backup MCP toolset. Covers the full lifecycle: command implementation, option definitions, service layer, input validation, unit tests, live tests, recorded test playback, CI validation, spell check, changelog entry, tool description evaluation, and PR checklist. USE WHEN: add new backup command, create backup tool, implement backup operation, new azurebackup command, add MCP tool for backup, new vault operation, new policy command, new governance command.'
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

Follow [`/servers/Azure.Mcp.Server/docs/new-command.md`](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md) as the authoritative guide.
The Azure Backup toolset lives in `tools/Azure.Mcp.Tools.AzureBackup/`.

#### 1a. Create Option Definitions

File: `src/Options/{Group}/{Resource}{Operation}Options.cs`

```csharp
// Inherit from the appropriate base options class
public class MyNewOptions : BaseAzureBackupOptions
{
    public string? MyParam { get; set; }
}
```

- Use `OptionDefinitions.Common.*` for shared options (subscription, resourceGroup)
- Use `AzureBackupOptionDefinitions.Vault` and `AzureBackupOptionDefinitions.VaultType` for vault options
- Add new options to `AzureBackupOptionDefinitions` if reusable across commands
- Use `.AsRequired()` / `.AsOptional()` extension methods

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
- Inject `ILogger<T>` and `IAzureBackupService`
- Override `RegisterOptions`, `BindOptions`, `ExecuteAsync`
- Add telemetry tags via `AzureBackupTelemetryTags.AddVaultTags(context.Activity, ...)`
- Call `HandleException(context, ex)` in catch blocks

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
- [ ] Required parameters throw `ArgumentException` with clear message when missing
- [ ] Subscription format validated (GUID only) via `ValidateSubscriptionFormat`
- [ ] Vault type normalized correctly (rsv/dpp case-insensitive)
- [ ] Enum/string parameters validated against allowed values with helpful error listing
- [ ] Null/empty strings handled with `ArgumentException.ThrowIfNullOrWhiteSpace`
- [ ] ARM resource IDs parsed safely with try-catch on `new ResourceIdentifier(...)`
- [ ] Error messages are actionable (tell user what to provide, not just what failed)

### Phase 3: Unit Tests

File: `tests/Azure.Mcp.Tools.AzureBackup.UnitTests/{Group}/{Resource}{Operation}CommandTests.cs`

#### Required Test Methods

```csharp
public sealed class MyNewCommandTests : CommandUnitTestsBase<MyNewCommand, IAzureBackupService>
{
    [Fact] public void Constructor_InitializesCommandCorrectly()
    [Fact] public void BindOptions_BindsOptionsCorrectly()
    [Fact] public async Task ExecuteAsync_ValidInput_ReturnsExpectedResult()
    [Fact] public async Task ExecuteAsync_HandlesServiceErrors()
    [Fact] public async Task ExecuteAsync_DeserializationValidation()

    // Add per-parameter validation tests:
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ExecuteAsync_InvalidVault_ThrowsArgumentException(string? vault)

    // Add edge case tests specific to the command
}
```

#### Run Unit Tests

```powershell
dotnet test tools\Azure.Mcp.Tools.AzureBackup\tests\Azure.Mcp.Tools.AzureBackup.UnitTests `
  /p:NuGetAudit=false `
  --filter "FullyQualifiedName~MyNewCommandTests"
```

Verify **all tests pass** before proceeding.

### Phase 4: Live Tests

File: `tests/Azure.Mcp.Tools.AzureBackup.LiveTests/AzureBackupCommandTests.cs`

#### 4a. Add Test Methods

Azure Backup live tests use `[Fact]` on a `RecordedCommandTestsBase` subclass with `CallToolAsync`.
There is no `[RecordedTest]` attribute in this toolset.

```csharp
[Fact]
public async Task MyNewCommand_RsvVault()
{
    var result = await CallToolAsync(
        "azurebackup", "mygroup", "myop",
        new Dictionary<string, object>
        {
            ["subscription"] = SubscriptionId,
            ["resourceGroup"] = ResourceGroupName,
            ["vault"] = DeploymentOutputs["AZUREBACKUP_RSV_VAULT_NAME"],
            // add other params
        });

    Assert.NotNull(result);
    // assert on result content
}
```

- Use `[Fact]` for all live tests (not `[RecordedTest]` — that attribute is not used in Azure Backup)
- Use `[LiveTestOnly]` alongside `[Fact]` for long-running E2E tests that cannot reliably replay
- Use test resource values from `DeploymentOutputs` (set in `test-resources-post.ps1`)

#### 4b. Update Test Infrastructure (if needed)

If the new command requires new Azure resources:

1. Edit `tests/test-resources.bicep` to add the resource
2. Edit `tests/test-resources-post.ps1` to output new deployment values
3. Deploy: `./eng/scripts/Deploy-TestResources.ps1 -Paths AzureBackup`

#### 4c. Record Live Tests

```powershell
# Set to Record mode
$settings = @{
    TestMode = "Record"
    SubscriptionId = "<your-sub>"
    TenantId = "<your-tenant>"
    ResourceGroupName = "<your-rg>"
    ResourceBaseName = "<your-base>"
} | ConvertTo-Json
$settings | Set-Content "tools\Azure.Mcp.Tools.AzureBackup\tests\Azure.Mcp.Tools.AzureBackup.LiveTests\.testsettings.json"

# Kill any stale proxy/server processes
Stop-Process -Name "Azure.Sdk.Tools.TestProxy","azmcp" -Force -ErrorAction SilentlyContinue

# Run tests in Record mode
dotnet test tools\Azure.Mcp.Tools.AzureBackup\tests\Azure.Mcp.Tools.AzureBackup.LiveTests `
  /p:NuGetAudit=false --filter "FullyQualifiedName~MyNewCommand"
```

#### 4d. Push Recordings

```powershell
# Push recorded sessions to azure-sdk-assets
.proxy\Azure.Sdk.Tools.TestProxy push `
  -a tools\Azure.Mcp.Tools.AzureBackup\tests\Azure.Mcp.Tools.AzureBackup.LiveTests\assets.json
```

This updates the `Tag` field in `assets.json`. **Commit the updated `assets.json`.**

#### 4e. Verify Playback

```powershell
# Switch to Playback mode
$settings = @{ TestMode = "Playback"; SubscriptionId = "..."; TenantId = "..."; ResourceGroupName = "..."; ResourceBaseName = "..." } | ConvertTo-Json
$settings | Set-Content "tools\Azure.Mcp.Tools.AzureBackup\tests\Azure.Mcp.Tools.AzureBackup.LiveTests\.testsettings.json"

Stop-Process -Name "Azure.Sdk.Tools.TestProxy","azmcp" -Force -ErrorAction SilentlyContinue

dotnet test tools\Azure.Mcp.Tools.AzureBackup\tests\Azure.Mcp.Tools.AzureBackup.LiveTests `
  /p:NuGetAudit=false --filter "FullyQualifiedName~MyNewCommand"
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
dotnet test tools\Azure.Mcp.Tools.AzureBackup\tests\Azure.Mcp.Tools.AzureBackup.UnitTests /p:NuGetAudit=false
```

#### 5d. Full Live Tests (Playback)

```powershell
dotnet test tools\Azure.Mcp.Tools.AzureBackup\tests\Azure.Mcp.Tools.AzureBackup.LiveTests /p:NuGetAudit=false
```

#### 5e. Spell Check

```powershell
.\eng\common\spelling\Invoke-Cspell.ps1
```

If new technical terms are flagged, add them to `.vscode/cspell.json`.

#### 5f. Full Build Verification

```powershell
./eng/scripts/Build-Local.ps1 -UsePaths -VerifyNpx
```

#### 5g. AOT/Native Build Verification

Azure Backup is marked `IsAotCompatible=true`, so also validate native compilation:

```powershell
./eng/scripts/Build-Local.ps1 -BuildNative
```

If this fails for a new Azure SDK dependency, the toolset may need to be excluded
from native builds (see `docs/aot-compatibility.md`).

### Phase 6: Tool Description Evaluation

Run the ToolDescriptionEvaluator to verify the new tool's description is discoverable by AI agents.

```powershell
$env:AOAI_ENDPOINT = "<your-aoai-endpoint>"
$env:TEXT_EMBEDDING_API_KEY = "<your-key>"

dotnet run --project eng/tools/ToolDescriptionEvaluator/src/ToolDescriptionEvaluator.csproj `
  -- --tool-name "azurebackup_<group>_<operation>"
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

Add 2-3 natural language prompts that should trigger the new tool, in alphabetical order.

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
- [ ] **Documentation updated** (commands.md, e2eTestPrompts.md, changelog, README.md, eng/vscode/README.md)
- [ ] **Commands metadata regenerated** via `Update-AzCommandsMetadata.ps1`
- [ ] **AOT/native build passes** (`Build-Local.ps1 -BuildNative`)
- [ ] **One tool per PR** (don't bundle unrelated changes)

#### Create the PR

```powershell
git add tools/Azure.Mcp.Tools.AzureBackup/ servers/Azure.Mcp.Server/ README.md eng/vscode/README.md
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
│   │   ├── AzureBackupOptionDefinitions.cs            # Shared options
│   │   └── {Group}/{Resource}{Operation}Options.cs    # Command options
│   ├── Services/
│   │   ├── IAzureBackupService.cs                     # Interface
│   │   ├── AzureBackupService.cs                      # Routing
│   │   ├── RsvBackupOperations.cs                     # RSV impl
│   │   └── DppBackupOperations.cs                     # DPP impl
│   └── Models/
│       └── AzureBackupTelemetryTags.cs                # Telemetry
└── tests/
    ├── Azure.Mcp.Tools.AzureBackup.UnitTests/
    │   └── {Group}/{Resource}{Operation}CommandTests.cs
    ├── Azure.Mcp.Tools.AzureBackup.LiveTests/
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
- **Unit tests:** `tests/Azure.Mcp.Tools.AzureBackup.UnitTests/Policy/PolicyCreateCommandTests.cs`
