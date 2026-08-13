# Vally Evaluator

VallyEvaluator generates [Vally](https://microsoft.github.io/vally/) evaluation specifications from the MCP end-to-end prompts maintained in this repository. It converts each non-interactive prompt into a Vally stimulus with a `tool-calls` grader that verifies the expected MCP namespace and command.

The evaluator generates specifications; it does not execute them. Use `eng/scripts/Invoke-VallyEvalTests.ps1` to run the generated specifications with Vally.

## Prerequisites

- The .NET SDK selected by the repository's `global.json`
- [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) to use the repository scripts
- [Vally](https://microsoft.github.io/vally/get-started/install/) to run evaluations
- [GitHub Copilot CLI](https://docs.github.com/en/copilot/how-tos/copilot-sdk/setup/local-cli), authenticated with `copilot login`

CI installs the Vally CLI with:

```powershell
npm install --global @microsoft/vally-cli@0.13.0
```

See [Testing with Vally](../../../docs/testing-with-vally.md) for repository-wide setup and eval authoring guidance.

## Generate Evaluations

Generate evaluations for selected tool namespaces:

```powershell
dotnet run --project ./eng/tools/VallyEvaluator/src/VallyEvaluator.csproj -- --namespaces "storage,appconfig"
```

Generate evaluations for every namespace represented in the default prompt file:

```powershell
dotnet run --project ./eng/tools/VallyEvaluator/src/VallyEvaluator.csproj
```

By default, the tool reads `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md` and writes one file per namespace under:

```text
.work/vally/evals/<namespace>/eval.yaml
```

Existing generated `eval.yaml` files are overwritten. Prompts marked as requiring interaction are excluded because Vally runs must be unattended.

## Command-Line Options

| Option | Description | Default |
| --- | --- | --- |
| `--namespaces` | Comma-separated tool namespaces to generate, such as `storage,acr`. | All namespaces in the prompt file when build info is not supplied. |
| `--promptFile` | Path to an end-to-end prompts Markdown file. | `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md` |
| `--workingDirectory` | Root directory for generated Vally artifacts and server build artifacts. | `<repo-root>/.work` |
| `--buildInfo` | Path to a `build_info.json` file. Limits generation to namespaces represented by `pathsToTest`. | Not set. |
| `--serverName` | MCP server entry to select from the build-info file. | `Azure.Mcp.Server` |

When both `--buildInfo` and `--namespaces` are supplied, the generated set is the union of namespaces selected by both options.

### Generate from Build Information

Build-info mode is intended for CI and changed-project evaluation. It requires a build-info file and server artifacts for the current runtime under `<workingDirectory>/build` so the evaluator can map assemblies to tool namespaces.

```powershell
./eng/scripts/New-BuildInfo.ps1 -ServerName Azure.Mcp.Server
./eng/scripts/Build-Local.ps1 -ServerName Azure.Mcp.Server
dotnet run --project ./eng/tools/VallyEvaluator/src/VallyEvaluator.csproj -- --buildInfo ./.work/build_info.json
```

A custom server and working directory can be selected explicitly:

```powershell
dotnet run --project ./eng/tools/VallyEvaluator/src/VallyEvaluator.csproj -- `
  --buildInfo ./artifacts/build_info.json `
  --serverName Fabric.Mcp.Server `
  --workingDirectory ./artifacts
```

## Run Evaluations

Build the MCP server, generate the specifications, and then invoke the wrapper:

```powershell
./eng/scripts/Build-Local.ps1 -ServerName Azure.Mcp.Server
dotnet run --project ./eng/tools/VallyEvaluator/src/VallyEvaluator.csproj -- --namespaces "storage,appconfig"
./eng/scripts/Invoke-VallyEvalTests.ps1
```

The wrapper reads generated specs from `.work/vally/evals`, includes checked-in `eval.yaml` files referenced by `.work/build_info.json`, and writes results to `.work/vally/vally-results`.

Useful wrapper options include:

```powershell
./eng/scripts/Invoke-VallyEvalTests.ps1 `
  -NumberOfRuns 3 `
  -IsDebug `
  -OutputPath ./.work/vally/custom-results
```

For deterministic agent behavior in automated runs, the workflow copies `src/Resources/eval.instructions.md` to the Vally working directory as `AGENTS.md` before invoking Vally.
