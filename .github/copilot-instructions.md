# Coding Instructions for GitHub Copilot

- Always use primary constructors in C#
- Always run dotnet build after making a change
- Always use System.Text.Json over Newtonsoft
- Always put new classes and interfaces in separate files
- Always make members static if they can be
- All generated code needs to be AOT safe
- Always review your own code for consistency, maintainability, and testability
- Always ask for clarifications if the request is ambiguous or lacks sufficient context.

## Engineering System

- Use `./eng/scripts/Build-Local.ps1 -UsePaths -VerifyNpx` to verify changes to powershell, c# project files and npm packages
- Don't run local builds to check pipeline YAML files (e.g., files in `eng/pipelines/` with `.yml` extension)

## Pull Request Guidelines

- Ensure all tests pass
- Follow the [contribution guidelines](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md)
- Include appropriate documentation
- Include tests that cover your changes
- Update CHANGELOG.md with your changes
- Run `.\eng\common\spelling\Invoke-Cspell.ps1`
- Create the auto-generated PR body as normal, but `copilot` should add an additional section after all of its regular PR body content. The contents should be:
  ```
  ## Invoking Livetests

  Copilot submitted PRs are not trustworthy by default. Users with `write` access to the repo need to validate the contents of this PR before leaving a comment with the text `/azp run mcp - pullrequest - live`. This will trigger the necessary livetest workflows to complete required validation.
  ```