# Contributing to Azure MCP

There are many ways to contribute to the Azure MCP project: reporting bugs, submitting pull requests, and creating suggestions.
After cloning and building the repo, check out the [GitHub project](https://github.com/orgs/Azure/projects/812/views/13) and [issues list](https://github.com/microsoft/mcp/issues). Issues labeled [help wanted](https://github.com/microsoft/mcp/labels/help%20wanted) are good issues to submit a PR for. Issues labeled [good first issue](https://github.com/microsoft/mcp/labels/good%20first%20issue) are great candidates to pick up if you are in the code for the first time.

>[!IMPORTANT]
If you are contributing significant changes, or if the issue is already assigned to a specific milestone, please discuss with the assignee of the issue first before starting to work on the issue.

## Table of Contents

- [Contributing to Azure MCP](#contributing-to-azure-mcp)
  - [Table of Contents](#table-of-contents)
  - [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Project Structure](#project-structure)
  - [Development Workflow](#development-workflow)
    - [Development Process](#development-process)
    - [Adding a New Command](#adding-a-new-command)
  - [Testing](#testing)
    - [Unit Tests](#unit-tests)
    - [End-to-end Tests](#end-to-end-tests)
    - [Testing Local Build with VS Code](#testing-local-build-with-vs-code)
      - [Build the Server](#build-the-server)
      - [Configure mcp.json](#configure-mcpjson)
      - [Server Modes](#server-modes)
      - [Start from IDE](#start-from-ide)
    - [Testing Local Build with Docker](#testing-local-build-with-docker)
    - [Live Tests](#live-tests)
    - [NPX Live Tests](#npx-live-tests)
    - [Debugging Live Tests](#debugging-live-tests)
  - [Quality and Standards](#quality-and-standards)
    - [Code Style](#code-style)
      - [Spelling Check](#spelling-check)
      - [Requirements](#requirements)
    - [AOT Compatibility Analysis](#aot-compatibility-analysis)
      - [Running the Analysis](#running-the-analysis)
      - [Installing Git Hooks](#installing-git-hooks)
    - [Model Context Protocol (MCP)](#model-context-protocol-mcp)
  - [Advanced Configuration](#advanced-configuration)
    - [Configuring External MCP Servers](#configuring-external-mcp-servers)
      - [Registry Configuration](#registry-configuration)
      - [Transport Types](#transport-types)
      - [Server Discovery and Namespace Filtering](#server-discovery-and-namespace-filtering)
      - [Adding New External MCP Servers](#adding-new-external-mcp-servers)
      - [Example External Servers](#example-external-servers)
  - [Project Management](#project-management)
    - [Pull Request Process](#pull-request-process)
    - [Builds and Releases (Internal)](#builds-and-releases-internal)
      - [PR Validation](#pr-validation)
  - [Support and Community](#support-and-community)
    - [Questions and Support](#questions-and-support)
    - [Additional Resources](#additional-resources)
    - [Code of Conduct](#code-of-conduct)
    - [License](#license)

## Getting Started

> ⚠️ If you are a Microsoft employee then please also review our [Azure Internal Onboarding Documentation](https://aka.ms/azmcp/intake) for getting setup

### Prerequisites

1. **VS Code**: Install either [stable](https://code.visualstudio.com/download) or [Insiders](https://code.visualstudio.com/insiders) release
2. **GitHub Copilot**: Install [GitHub Copilot](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) and [GitHub Copilot Chat](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot-chat) extensions
3. **Node.js**: Install [Node.js](https://nodejs.org/en/download) 20 or later (ensure `node` and `npm` are in your PATH)
4. **PowerShell**: Install [PowerShell](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) 7.0 or later (required for build and test scripts)

### Project Structure

- `core\`
  - `Azure.Mcp.Core` - Azure.Mcp.Core core library, depends on Microsoft.Mcp.Core
  - `Fabric.Mcp.Core` - Fabric.Mcp.Core, depends on Azure.Mcp.Core (fabric uses azure)
  - `Microsoft.Mcp.Core` - Microsoft.Mcp.Core library
- `servers\`
  - `{server}.Mcp.Server - Individual servers (e.g. `Azure.Mcp.Server`, `Fabric.Mcp.Server`)
    - `src` - Source for the server
    - `tests` - Any unit or live tests for the server
    - `README.md` - Specific readme for this server
    - `CHANGELOG.md` - Specific changelog for this server
- `tools/` - Service-specific implementations
  - `{server}.Mcp.Tools.{tool-name}/` - Individual server tools (e.g., `Azure.Mcp.Tools.KeyVault`, `Fabric.Mcp.Tools.Admin`)
    - `src` - Service specific code
      - `Commands/` - Command implementations
      - `Models/` - Service specific models
      - `Services/` - Service implementations and interfaces
      - `Options/` - Service specific command options
    - `tests/` - Service specific tests
      - `{server}.Mcp.Tools.{tool-name}.UnitTests/` - Unit tests require no authentication or test resources
      - `{server}.Mcp.Tools.{tool-name}.LiveTests/` - Live tests depend on Azure resources and authentication
      - `test-resources.bicep` - Infrastructure templates for testing
- `eng/` - Shared tools, templates, CLI helpers
- `docs/` - Central documentation and onboarding materials
## Development Workflow

### Development Process

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Write or update tests
5. Test locally
6. Submit a pull request

### Adding a New Command

> **⚠️ Important: Submit One Tool Per Pull Request**
>
> We strongly recommend submitting **one tool per pull request** to streamline the review process and provide better onboarding experience. This approach results in:
>
> - **Faster reviews**: Single tools are easier and quicker to review
> - **Better feedback**: More focused discussions on individual tool implementation
> - **Easier iteration**: Smaller changes mean faster iteration cycles
> - **Incremental progress**: Get your first tool merged to establish baseline, then build upon it
>
> If you're planning to contribute multiple tools, please:
> 1. Submit your most important or representative tool as your first PR to establish the code patterns.
> 2. Use that baseline to inform your subsequent tool PRs.

1. **Create an issue** with title: "Add command: azmcp [namespace] [resource] [operation]" and detailed description

2. **Set up development environment**:
   - Open VS Code Insiders
   - Open the Copilot Chat view
   - Select "Agent" mode

3. **Generate the command** using Copilot:

   ```txt
   Execute in Copilot Chat:
   "create [namespace] [resource] [operation] command using #new-command.md as a reference"
   ```

4. **Follow implementation guidelines** in [docs/new-command.md](https://github.com/microsoft/mcp/blob/main/docs/new-command.md)

5. **Update documentation**:
   - Add the new command to [/docs/azmcp-commands.md](https://github.com/microsoft/mcp/blob/main/docs/azmcp-commands.md)
   - Add test prompts for the new command in [/docs/e2eTestPrompts.md](https://github.com/microsoft/mcp/blob/main/docs/e2eTestPrompts.md)
   - Update [README.md](https://github.com/microsoft/mcp/blob/main/README.md) to mention the new command

6. **Add CODEOWNERS entry** in [CODEOWNERS](https://github.com/microsoft/mcp/blob/main/.github/CODEOWNERS) [(example)](https://github.com/microsoft/mcp/commit/08f73efe826d5d47c0f93be5ed9e614740e82091)

7. **Create Pull Request**:
   - Reference the issue you created
   - Include tests in the `/tests` folder
   - Ensure all tests pass
   - Follow code style requirements
   - Run [`ToolDescriptionEvaluator`](https://github.com/microsoft/mcp/blob/main/eng/tools/ToolDescriptionEvaluator/Quickstart.md) for the new tool description and obtain a score of `0.4` or more and a top 3 ranking for all related test prompts

## Testing

Command authors must provide both unit tests and end-to-end test prompts.

### Unit Tests

Unit tests live under the `/tests` folder. To run tests:

```pwsh
./eng/scripts/Test-Code.ps1
```

To scope the test run to path substring matches, use:

```pwsh
./eng/scripts/Test-Code.ps1 -Paths KeyVault, core/Azure
```

Requirements:

- Each command should have unit tests
- Tests should cover success and error scenarios
- Mock external service calls
- Test argument validation

### End-to-end Tests

End-to-end tests are performed manually. Command authors must thoroughly test each command to ensure correct tool invocation and results. At least one prompt per tool is required and should be added to `/docs/e2eTestPrompts.md`.

### Testing Local Build with VS Code

To run the Azure MCP server from source for local development:

#### Build the Server

Build the project at the root directory of this repository:

```bash
dotnet build
```

#### Configure mcp.json

Update your mcp.json to point to the locally built azmcp executable:

```json
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio",
      "command": "<absolute-path-to>/azure-mcp/core/src/AzureMcp.Cli/bin/Debug/net9.0/azmcp[.exe]",
      "args": ["server", "start"]
    }
  }
}
```

> **Note:** Replace `<absolute-path-to>` with the full path to your built executable.
> On **Windows**, use `azmcp.exe`. On **macOS/Linux**, use `azmcp`.

#### Server Modes

Optional `--namespace` and `--mode` parameters can be used to configure different server modes:

**Default Mode** (no additional parameters):

```json
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio",
      "command": "<absolute-path-to>/azure-mcp/core/src/AzureMcp.Cli/bin/Debug/net9.0/azmcp[.exe]",
      "args": ["server", "start"]
    }
  }
}
```

**Namespace Mode** (expose specific services):

```json
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio",
      "command": "<absolute-path-to>/azure-mcp/core/src/AzureMcp.Cli/bin/Debug/net9.0/azmcp[.exe]",
      "args": ["server", "start", "--namespace", "storage", "--namespace", "keyvault"]
    }
  }
}
```

**Namespace Proxy Mode** (collapse tools by namespace):

```json
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio",
      "command": "<absolute-path-to>/azure-mcp/core/src/AzureMcp.Cli/bin/Debug/net9.0/azmcp[.exe]",
      "args": ["server", "start", "--mode", "namespace"]
    }
  }
}
```

**Single Tool Proxy Mode** (single "azure" tool with internal routing):

```json
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio",
      "command": "<absolute-path-to>/azure-mcp/core/src/AzureMcp.Cli/bin/Debug/net9.0/azmcp[.exe]",
      "args": ["server", "start", "--mode", "single"]
    }
  }
}
```

**Combined Mode** (filter namespaces with proxy mode):

```json
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio",
      "command": "<absolute-path-to>/azure-mcp/core/src/AzureMcp.Cli/bin/Debug/net9.0/azmcp[.exe]",
      "args": ["server", "start", "--namespace", "storage", "--namespace", "keyvault", "--mode", "namespace"]
    }
  }
}
```

> **Server Mode Summary:**
>
> - **Default Mode**: No additional parameters - exposes all tools individually
> - **Namespace Mode**: `--namespace <service-name>` - expose specific services
> - **Namespace Proxy Mode**: `--mode namespace` - collapse tools by namespace (useful for VS Code's 128 tool limit)
> - **Single Tool Mode**: `--mode single` - single "azure" tool with internal routing
> - **Combined Mode**: Both `--namespace` and `--mode` can be used together

#### Start from IDE

With the configuration in place, you can launch the MCP server directly from your IDE or any tooling that uses `mcp.json`.

### Testing Local Build with Docker

To build a local image for testing purposes:

1. Execute: `./eng/scripts/Build-Docker.ps1`.
2. Update `mcp.json` to point to locally built Docker image:

    ```json
    {
      "servers": {
        "Azure MCP Server": {
          "command": "docker",
          "args": [
            "run",
            "-i",
            "--rm",
            "--env-file",
            "/full/path/to/.env"
            "azure/azure-mcp:<insert-version-here>",
          ]
        }
      }
    }
    ```

### Live Tests

> ⚠️ If you are a Microsoft employee with Azure source permissions then please review our [Azure Internal Onboarding Documentation](https://aka.ms/azmcp/intake). As part of reviewing community contributions, Azure team members can run live tests by adding this comment to the PR `/azp run  mcp - pullrequest - live`.

Before running live tests:

- [Install Azure PowerShell](https://learn.microsoft.com/powershell/azure/install-azure-powershell)
- [Install Azure Bicep](https://learn.microsoft.com/azure/azure-resource-manager/bicep/install#install-manually)
- Login to Azure PowerShell: [`Connect-AzAccount`](https://learn.microsoft.com/powershell/azure/authenticate-interactive?view=azps-13.4.0)
- Deploy test resources:

```pwsh
./eng/scripts/Deploy-TestResources.ps1 -Paths KeyVault
./eng/scripts/Deploy-TestResources.ps1 -Paths Storage, core/Azure
```

**Deploy-TestResources.ps1 Parameters:**

| Parameter           | Type   | Description                                                                                                                                |
|---------------------|--------|--------------------------------------------------------------------------------------------------------------------------------------------|
| `Paths`             | string[] | REQUIRED. Path filters to apply (e.g., `Storage`, `core/`). All filters are applied as substring matches. (e.g., `*Storage*`, `*core/*`) |
| `SubscriptionId`    | string | Target subscription ID. If omitted, the current Azure context subscription (from `Get-AzContext`) is used.                                 |
| `ResourceGroupName` | string | Resource group name. Defaults to `{username}-mcp{hash(username)}`.                                                                         |
| `BaseName`          | string | Base name prefix for resources. Defaults to `mcp{hash}`.                                                                                   |
| `Unique`            | switch | Use a unique GUID-based hash for this invocation instead of the stable username+subscription hash.                                         |
| `DeleteAfterHours`  | int    | Hours after which resources are tagged for deletion. Defaults to `12`.                                                                     |

Examples:

```pwsh
# Deploy Storage test resources using current Azure context subscription
./eng/scripts/Deploy-TestResources.ps1 -Paths Storage

# Deploy Key Vault test resources to a specific subscription and keep for one week
./eng/scripts/Deploy-TestResources.ps1 -Paths KeyVault -SubscriptionId <subId> -DeleteAfterHours 168 -Unique
```

After deploying test resources, you should have a `.testsettings.json` file with your deployment information in the deployed areas' `/tests` directory.

Run live tests with:

```pwsh
./eng/scripts/Test-Code.ps1 -TestType Live
```

You can scope tests to specific areas:

```pwsh
./eng/scripts/Test-Code.ps1 -TestType Live -Paths Storage, KeyVault
```

### NPX Live Tests

You can set the `TestPackage` parameter in `.testsettings.json` to have live tests run `npx` targeting an arbitrary Azure MCP package:

```json
{
  "TenantId": "a20062a8-ff76-41c2-8a6d-5e843da7b051",
  "TenantName": "Your Tenant",
  "SubscriptionId": "cd27afdc-9976-4f08-96e9-cad120a91560",
  "SubscriptionName": "Your Subscription",
  "ResourceGroupName": "rg-abcdefg",
  "ResourceBaseName": "t1234567890",
  "TestPackage": "@azure/mcp@0.0.10"
}
```

To run live tests against the local build of an npm module:

```pwsh
./eng/scripts/Build-Local.ps1
```

This will produce .tgz files in the `.dist` directory and set the `TestPackage` parameter in the `.testsettings.json` file:

```json
"TestPackage": "file://D:\\repos\\azure-mcp\\.dist\\wrapper\\azure-mcp-0.0.12-alpha.1746488279.tgz"
```

### Debugging Live Tests

This section assumes that the necessary Azure resources for live tests are already deployed and that the `.testsettings.json` file with deployment information is located in the area's `/tests/` directory.

To debug the Azure MCP Server (`azmcp`) when running live tests in VS Code:

1. Build the package with debug symbols: `./eng/scripts/Build-Local.ps1 -DebugBuild`
2. Set a breakpoint in a command file (e.g., [`KeyValueListCommand.ExecuteAsync`](https://github.com/microsoft/mcp/blob/4ed650a0507921273acc7b382a79049809ef39c1/src/Commands/AppConfig/KeyValue/KeyValueListCommand.cs#L48))
3. In VS Code, navigate to a test method (e.g., [`AppConfigCommandTests::Should_list_appconfig_kvs()`](https://github.com/microsoft/mcp/blob/4ed650a0507921273acc7b382a79049809ef39c1/tests/Client/AppConfigCommandTests.cs#L56)), add a breakpoint to `CallToolAsync` call in the test method, then right-click and select **Debug Test**
4. Find the `azmcp` process ID:

    ```shell
    pgrep -fl azmcp
    ```

    ```powershell
    Get-Process | Where-Object { $_.ProcessName -like "*azmcp*" } | Select-Object Id, ProcessName, Path
    ```

5. Open the Command Palette (`Cmd+Shift+P` on Mac, `Ctrl+Shift+P` on Windows/Linux), select **Debug: Attach to .NET 5+ or .NET Core process**, and enter the `azmcp` process ID
6. Hit F5 to "Continue" debugging, the debugger should attach to `azmcp` and hit the breakpoint in command file

## Quality and Standards

### Code Style

To ensure consistent code quality, code format checks will run during all PR and CI builds. Run `dotnet format` before submitting to catch format errors early.

#### Spelling Check

To ensure consistent spelling across the codebase, run the spelling check before submitting:

```pwsh
.\eng\common\spelling\Invoke-Cspell.ps1
```

This will check all files for spelling errors using the project's dictionary. Add any new technical terms or proper nouns to `.vscode/cspell.json` if needed.

#### Requirements

- Follow C# coding conventions
- No comments in implementation code (code should be self-documenting)
- Use descriptive variable and method names
- Follow the exact file structure and naming conventions
- Use proper error handling patterns
- XML documentation for public APIs
- Follow Model Context Protocol (MCP) patterns

### AOT Compatibility Analysis

The AOT compatibility analysis helps identify potential issues that might prevent the Azure MCP Server from working correctly when compiled with AOT or when trimming is enabled.

#### Running the Analysis

To run the AOT compatibility analysis locally:

```pwsh
./eng/scripts/Analyze-AOT-Compact.ps1
```

The HTML report will be generated at `.work/aotCompactReport/aot-compact-report.html` and automatically opened in your default browser.

To output the report to console, run the analysis with `-OutputFormat Console` argument.

AOT compatibility warnings typically indicate:

- Use of reflection without proper annotations
- Serialization of types that might be trimmed
- Dynamic code generation
- Use of `RequiresUnreferencedCodeAttribute` methods without proper precautions

#### Installing Git Hooks

You can install our pre-push hook to catch code format issues by automatically running `dotnet format` before each `git push`:

- `./eng/scripts/Install-GitHooks.ps1` - Installs the pre-push hook into your local repo
- `./eng/scripts/Remove-GitHooks.ps1` - Disables any git hooks in your local repo

### Model Context Protocol (MCP)

The Azure MCP Server implements the [Model Context Protocol specification](https://modelcontextprotocol.io). When adding new commands:

- Follow MCP JSON schema patterns
- Implement proper context handling
- Use standardized response formats
- Handle errors according to MCP specifications
- Provide proper argument suggestions

## Advanced Configuration

### Configuring External MCP Servers

The Azure MCP Server supports connecting to external MCP servers through an embedded `registry.json` configuration file. This enables the server to act as a proxy, aggregating tools from multiple MCP servers into a single interface. The registry follows the same configuration schema as VS Code's `mcp.json`.

#### Registry Configuration

External MCP servers are defined in the embedded resource file `core/src/Azure.Mcp.Core/Areas/Server/Resources/registry.json`. This file contains server configurations that support both SSE (Server-Sent Events) and stdio transport mechanisms, following the standard MCP configuration format.

The registry structure follows this format:

```json
{
  "servers": {
    "documentation": {
      "url": "https://learn.microsoft.com/api/mcp",
      "description": "Search official Microsoft/Azure documentation..."
    },
    "another-server": {
      "type": "stdio",
      "command": "path/to/executable",
      "args": ["arg1", "arg2"],
      "env": {
        "ENV_VAR": "value"
      },
      "description": "Another MCP server using stdio transport"
    }
  }
}
```

#### Transport Types

**SSE (Server-Sent Events) Transport:**

- Use the `url` property to specify the endpoint
- Supports HTTP-based communication with automatic transport mode detection
- Best for web-based MCP servers and remote endpoints

**Stdio Transport:**

- Use `type: "stdio"` with the `command` property
- Supports launching external processes that communicate via standard input/output
- Use `args` array for command-line arguments
- Use `env` object for environment variables
- Best for local executables, command-line tools, and local MCP servers

#### Server Discovery and Namespace Filtering

External servers are automatically discovered when the Azure MCP Server starts. They can be filtered using the same namespace mechanisms as built-in commands:

```bash
# Include only specific external servers
azmcp server start --namespace documentation --namespace another-server

# Use namespace mode to group tools exposed by external servers
azmcp server start --mode namespace
```

#### Adding New External MCP Servers

To add a new external MCP server to the registry:

1. Edit `core/src/Azure.Mcp.Core/Areas/Server/Resources/registry.json`
2. Add your server configuration under the `servers` object using VS Code's MCP configuration schema
3. Use a unique identifier as the key
4. Provide either a `url` for SSE transport or `type: "stdio"` with `command` for stdio transport
5. Include a descriptive `description` field
6. Rebuild the project to embed the updated registry

#### Example External Servers

The current registry includes:

- **documentation**: Microsoft Learn documentation search via SSE transport
- Additional external servers can be added following the same pattern as VS Code's mcp.json

External servers integrate seamlessly with the Azure MCP Server's tool aggregation, appearing alongside native Azure commands in the unified tool interface. This allows you to combine local MCP servers, remote MCP endpoints, and Azure-specific tools in a single interface.

## Project Management

### Pull Request Process

1. Update documentation reflecting any changes
2. Add or update tests as needed
3. Reference the original issue
4. Wait for review and address any feedback

### Builds and Releases (Internal)

The internal pipeline [azure-mcp](https://dev.azure.com/azure-sdk/internal/_build?definitionId=7571) is used for all official releases and CI builds. On every merge to main, a build will run and will produce a dynamically named prerelease package on the public dev feed, e.g. [@azure/mcp@0.0.10-beta.4799791](https://dev.azure.com/azure-sdk/public/_artifacts/feed/azure-sdk-for-js/Npm/@azure%2Fmcp/overview/0.0.10-beta.4799791).

Only manual runs of the pipeline sign and publish packages. Building `main` or `hotfix/*` will publish to `npmjs.com`, all other refs will publish to the [public dev feed](https://dev.azure.com/azure-sdk/public/_artifacts/feed/azure-sdk-for-js).

Packages published to npmjs.com will always use the `@latest` [dist-tag](https://docs.npmjs.com/downloading-and-installing-packages-locally#installing-a-package-with-dist-tags).

Packages published to the dev feed will use:

- `@latest` for the latest official/release build
- `@dev` for the latest CI build of main
- `@pre` for any arbitrary pipeline run or feature branch build

#### PR Validation

To run live tests for a PR, inspect the PR code for any suspicious changes, then add the comment `/azp run  mcp - pullrequest - live` to the pull request. This will queue a PR triggered run which will build, run unit tests, deploy test resources and run live tests.

If you would like to see the product of a PR as a package on the dev feed, after thoroughly inspecting the change, create a branch in the main repo and manually trigger an [azure - mcp](https://dev.azure.com/azure-sdk/internal/_build?definitionId=7571) pipeline run against that branch. This will queue a manually triggered run which will build, run unit tests, deploy test resources, run live tests, sign and publish the packages to the dev feed.

Instructions for consuming the package from the dev feed can be found in the "Extensions" tab of the pipeline run page.

## Support and Community

Please see our [support](https://github.com/microsoft/mcp/blob/main/SUPPORT.md) statement.

### Questions and Support

We're building this in the open.  Your feedback is much appreciated, and will help us shape the future of the Azure MCP server.

👉 [Open an issue in the public repository](https://github.com/microsoft/mcp/issues/new/choose).

### Additional Resources

- [Azure MCP Documentation](https://github.com/microsoft/mcp/blob/main/README.md)
- [Command Implementation Guide](https://github.com/microsoft/mcp/blob/main/docs/new-command.md)
- [VS Code Insiders Download](https://code.visualstudio.com/insiders/)
- [GitHub Copilot Documentation](https://docs.github.com/en/copilot)

### Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments. By participating, you are expected to uphold this code.

### License

By contributing, you agree that your contributions will be licensed under the project's [license](https://github.com/microsoft/mcp/blob/main/LICENSE).
