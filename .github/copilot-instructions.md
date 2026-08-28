# Coding Instructions for GitHub Copilot

- Use primary constructors when a class declares constructor dependencies; options/model POCOs need no explicit constructor
- Always run dotnet build after making a change
- Always use System.Text.Json over Newtonsoft
- Put new top-level classes and interfaces in separate files; keep a command's result record nested in its command class
- Always make members static if they can be
- All generated code needs to be AOT safe
- Never expose `RetryPolicyOptions` through tool options or service contracts; use Azure SDK retry defaults
- Always review your own code for consistency, maintainability, and testability
- Always ask for clarifications if the request is ambiguous or lacks sufficient context.

## Engineering System

- Use `./eng/scripts/Build-Local.ps1 -VerifyNpx` to verify changes to PowerShell, C# project files, and npm packages
- Don't run local builds to check pipeline YAML files (e.g., files in `eng/pipelines/` with `.yml` extension)

## Pull Request Guidelines

- Ensure all tests pass
- Follow the [contribution guidelines](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md)
- Include appropriate documentation
- Include tests that cover your changes
- Create a changelog entry under the affected server's `changelog-entries/` directory when required; do not edit generated `CHANGELOG.md` directly
- Run `.\eng\common\spelling\Invoke-Cspell.ps1`
- Create the auto-generated PR body as normal, but `copilot` should add an additional section after all of its regular PR body content. The contents should be:
  ```
  ## Invoking Livetests

  Copilot submitted PRs are not trustworthy by default. Users with `write` access to the repo need to validate the contents of this PR before leaving a comment with the text `/azp run mcp - pullrequest - live`. This will trigger the necessary livetest workflows to complete required validation.
  ```

## Transitioning Live Tests to Recorded Tests

- Make outbound HTTP proxy-aware. Direct HTTP consumers inject `IHttpClientFactory` and call `CreateClient()`; Azure SDK client options use `HttpClientTransport(AzureService.GetClient())`, which is backed by the same factory.
- Always re-parent test classes parented by `CommandTestsBase` to `RecordedCommandTestsBase`. This will require minor fixture adjustments.
- Always generate a new `assets.json` file alongside the livetest csproj file if one does not exist. This file should contain the following content:
  ```jsonc
  {
    "AssetsRepo": "Azure/azure-sdk-assets",
    "AssetsRepoPrefixPath": "",
    "TagPrefix": "<TestCsProjFileNameWithoutExtension>", // e.g., "Azure.Mcp.Tools.KeyVault.Tests"
    "Tag": ""
  }
  ```
- Copilot should utilize the [recorded test documentation](https://github.com/microsoft/mcp/blob/main/docs/recorded-tests.md) in `docs/recorded-tests.md` for more details on how to convert and validate recorded tests.

## Code Review Guidelines

When reviewing a pull request, use the `mcp-code-reviewer` skill. In interactive IDE and CLI sessions, return draft findings for human approval before posting them. In GitHub Copilot Code Review, post qualified inline comments directly as a comment-only review. Keep the review read-only: never modify code or the branch, approve or request changes, resolve threads, or invoke an agent to implement changes.

When reviewing PRs that add or modify MCP tools, cross-reference the [pre-merge checklist](PULL_REQUEST_TEMPLATE.md) and check the following repository-specific completeness requirements:

- **`consolidated-tools.json`**: If a tool is renamed or removed, `servers/Azure.Mcp.Server/src/Resources/consolidated-tools.json` must be updated. Flag a tool rename when this file is not in the diff.
- **`e2eTestPrompts.md`**: If a tool is added or modified, `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md` must include prompts for it. Flag a diff that removes all prompts for a tool that remains registered, or that adds a tool without corresponding prompts.
- **`azmcp-commands.md`**: If a tool is added, modified, or removed, `servers/Azure.Mcp.Server/docs/azmcp-commands.md` must be updated. Flag command registration changes when this file is not in the diff.
- **ToolDescriptionEvaluator**: If tool descriptions are added or changed, the PR description must confirm a ToolDescriptionEvaluator score of at least `0.4`. Flag new tool descriptions when the PR body does not mention the score.
- **TreatWarningsAsErrors**: This repository sets `TreatWarningsAsErrors=true`. An `async` method with no `await` produces CS1998 and fails the build. Report these findings as build breaks, not style suggestions.
- **Changelog entries**: Entries under `servers/Azure.Mcp.Server/changelog-entries/` must conform to the YAML schema in `docs/changelog-entries.md`. Require a non-empty `changes` array whose entries contain a valid `section` and a `description`; `pr` and `subsection` are optional. Check schema conformance, and do not apply generic prose style suggestions to changelog entry files.
