
# Contributing to Template Model Context Protocol (MCP) VS Code Extension

Thank you for your interest in contributing to the Template MCP extension for Visual Studio Code!

## Building and Installation

You need to have `Node.js`, `npm` and `powershell` installed. Node 20 (LTS) or later is recommended for development.

1. **Build the VSIX file containing the extension**
    ```powershell
    ./eng/scripts/New-BuildInfo.ps1
    ./eng/scripts/Build-Code.ps1 -Server Template.Mcp.Server
    ./eng/scripts/Pack-Vsix.ps1 -Server Template.Mcp.Server
    ```
2. **Install the extension into VS Code**
    Open VS Code Extensions view, click on the "..." menu and choose "Install from VSIX...". Point to the VSIX file created in step 1.

## Debugging

To debug extension code, use the "Run Extension" debug configuration provided in the `launch.json` file in the repo. This configuration will (re)build the extension as necessary—no need to build anything manually.

## Submitting a Change

Before submitting a PR, make sure that the unit tests pass (`npm run unit-test`) and that the code linter does not produce any errors or warnings (`npm run lint`).

You might find [ESLint for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=dbaeumer.vscode-eslint) handy for keeping the code error-free as you are making changes.

## Legal

Before we can accept your pull request you will need to sign a **Contribution License Agreement**. All you need to do is to submit a pull request, then the PR will get appropriately labelled (e.g. `cla-required`, `cla-norequired`, `cla-signed`, `cla-already-signed`). If you already signed the agreement we will continue with reviewing the PR, otherwise system will tell you how you can sign the CLA. Once you sign the CLA all future PR's will be labeled as `cla-signed`.
