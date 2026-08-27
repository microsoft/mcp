# Testing with Vally

Vally is the evaluation framework used to test the performance and accuracy of Azure MCP Server and its tools.

## Prerequisites

* [Vally](https://microsoft.github.io/vally/get-started/install/)
* [Copilot SDK](https://docs.github.com/en/copilot/how-tos/copilot-sdk/setup/local-cli)
* Run `copilot login` before running Vally

## Authoring an eval spec

* `.vally.yaml` at the repository root contains settings common to all Vally runs.
* Store a checked-in `eval.yaml` under `./tools/Azure.Mcp.Tools.<ToolArea>/tests/` so the workflow can discover it.

To be picked up by the Vally evaluations workflow, a specification must contain `environment: ${ENVIRONMENT}` as shown below. The environments in `.vally.yaml` tell Vally where to find Azure MCP Server for the current operating system.

```yaml
name: Azure ACR evaluations
description: Prompts for Azure ACR
type: capability
stimuli:
  - name: List stores
    prompt: What Azure Container Registries do I have? 
    environment: ${ENVIRONMENT}
    tags: 
    graders:
      - type: tool-calls
        config:
          required:
            - name: acr
              command: acr_registry_list
```

* [Writing Eval Specs](https://microsoft.github.io/vally/guides/writing-eval-specs/) contains comprehensive information on authoring an eval.
* [Grader Catalog](https://microsoft.github.io/vally/reference/graders/) contains all graders shipped with vally.

## Run a checked-in specification locally

In a terminal window, navigate to the repository root:

1. Run `./eng/scripts/Build-Local.ps1 -ServerName Azure.Mcp.Server`.
2. Run one of the following commands depending on machine's operating system:
    * Windows: `vally eval --eval-spec ./tools/Azure.Mcp.Tools.AppConfig/tests/eval.yaml --param ENVIRONMENT=windows`
    * Linux: `vally eval --eval-spec ./tools/Azure.Mcp.Tools.AppConfig/tests/eval.yaml --param ENVIRONMENT=linux`

The repository's `.vally.yaml` currently defines environments only for Azure MCP Server on Windows and Linux. Running another MCP server requires adding an environment that launches that server and passing its environment name through `ENVIRONMENT`.

## Run generated end-to-end prompt evaluations

VallyEvaluator accepts any server that provides `servers/<ServerName>/docs/e2eTestPrompts.md`. It converts the file's non-interactive prompts into Vally specifications. The following example uses Azure MCP Server because the repository's `.vally.yaml` already defines environments that launch it.

From the repository root:

1. Run `./eng/scripts/Build-Local.ps1 -ServerName Azure.Mcp.Server`.
2. Generate specifications for every eligible namespace:

   ```powershell
   dotnet run --project ./eng/tools/VallyEvaluator/src/VallyEvaluator.csproj -- --serverName Azure.Mcp.Server
   ```

   To generate only selected namespaces, append `--namespaces "storage,appconfig"`.

3. Run `./eng/scripts/Invoke-VallyEvalTests.ps1`.

Generated specifications are written to `.work/vally/evals/<namespace>/eval.yaml`. Vally results are written to `.work/vally/vally-results`.

The invocation script also includes checked-in specifications selected by `.work/build_info.json`. If it finds no generated or checked-in specifications, it exits successfully without starting Vally.

To generate specifications for another server, pass its directory name to `--serverName`. For example, `--serverName Fabric.Mcp.Server` reads `servers/Fabric.Mcp.Server/docs/e2eTestPrompts.md`. That prompt file must exist. To execute the generated specifications, `.vally.yaml` must also define an environment that launches the selected server.

### Agent instructions during evaluation

Before invoking Vally, `Invoke-VallyEvalTests.ps1` temporarily replaces `<WorkDirectory>/AGENTS.md` with `eng/tools/VallyEvaluator/src/Resources/eval.instructions.md`. The script restores the original file in a `finally` block after Vally exits. The default working directory is the repository root; a custom `-WorkDirectory` must contain an `AGENTS.md` file.

## Automated workflow

A GitHub workflow runs Vally evaluations for pull requests that modify files matching `tools/Azure.Mcp.*/**`. It uses `.work/build_info.json` to generate and run evaluations for affected Azure tool namespaces. The workflow can also be started manually with a configurable run count and verbose logging.

Runs are available in [GitHub Actions](https://github.com/microsoft/mcp/actions). To diagnose failures, open the **Upload vally results** step and download the `vally-results` artifact.