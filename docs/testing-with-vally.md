# Testing with vally

vally is the evaluation framework used to test the performance/accuracy of Azure MCP server and its tools.

## Prerequisites

* [vally](https://microsoft.github.io/vally/get-started/install/)
* [Copilot SDK](https://docs.github.com/en/copilot/how-tos/copilot-sdk/setup/local-cli)

## Authoring an eval spec

* `.vally.yaml` at the root of the repository, it contains settings common to all vally runs.
* The `eval.yaml` should live under a `./tools/<<tool_namespace>>/tests/` folder to be picked up by our workflow runs.

To be picked up by the "vally Evaluations" workflow for automatic testing, it must contain `environment: ${ENVIRONMENT}` like the sample below.  The environments are defined in `.vally.yaml`, and instruct the framework where to find the Azure MCP server.

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

## Running vally locally

In a terminal window:

1. Execute: `copilot login`
2. Navigate to the repository root
3. Execute: `./eng/scripts/Build-Local.ps1 -ServerName <<Server Name>>` where `<<Server Name>>` is a server in [servers/](https://github.com/microsoft/mcp/tree/main/servers)
4. Execute one of the following commands depending on machine's operating system:
    * Windows: `vally eval --eval-spec ./tools/Azure.Mcp.Tools.AppConfig/tests/eval.yaml --param ENVIRONMENT=windows`
    * Linux: `vally eval --eval-spec ./tools/Azure.Mcp.Tools.AppConfig/tests/eval.yaml --param ENVIRONMENT=linux`

## Automated runs with GitHub workflow

A GitHub workflow performs vally evaluations when files under `./tools/<<ToolArea>>` are modified. Runs can be viewed by navigating to https://github.com/microsoft/mcp/actions. To diagnose failures, navigate to "Upload vally results" step and download `vally-results` artifact.