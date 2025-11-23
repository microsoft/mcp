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
      - [Cancellation plumbing](#cancellation-plumbing)
    - [End-to-end Tests](#end-to-end-tests)
    - [Testing Local Build with VS Code](#testing-local-build-with-vs-code)
      - [Build the Server](#build-the-server)
      - [Run the Azure MCP server in HTTP mode](#run-the-azure-mcp-server-in-http-mode)
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
    - [Package README](#package-readme)
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

> [!IMPORTANT]
> If you are a **Microsoft employee** then please also review our [Azure Internal Onboarding Documentation](https://aka.ms/azmcp/intake) for getting setup

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

> [!TIP]
> **Submit One Tool Per Pull Request**
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

4. **Follow implementation guidelines** in [docs/new-command.md](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md)

5. **Update documentation**:
   - Add the new command to [/servers/Azure.Mcp.Server/docs/azmcp-commands.md](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md)
   - Run `.\eng\scripts\Update-AzCommandsMetadata.ps1` to update tool metadata in azmcp-commands.md (required for CI)
   - Add test prompts for the new command in [/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md)
   - Update [README.md](https://github.com/microsoft/mcp/blob/main/README.md) to mention the new command

6. **Add CODEOWNERS entry** in [CODEOWNERS](https://github.com/microsoft/mcp/blob/main/.github/CODEOWNERS) [(example)](https://github.com/microsoft/mcp/commit/08f73efe826d5d47c0f93be5ed9e614740e82091)

7. **Add new tool to consolidated mode**:
   - Open `core/Azure.Mcp.Core/src/Areas/Server/Resources/consolidated-tools.json` file, where the tool grouping definition is stored for consolidated mode. In Agent mode, add it to the chat as context.
   - Paste the follow prompt for Copilot to generate the change to add the new tool:
      ```txt
      I have this list of tools which haven't been matched with any consolidated tools in this file. Help me add them to the one with the best matching category and exact matching toolMetadata. Update existing consolidated tools where newly mapped tools are added. If you can't find one, suggest a new consolidated tool.

      <Add new tool name here>
      ```
   - Use the following command to find out the correct tool name for your new tool
      ```
      cd servers/Azure.Mcp.Server/src/bin/Debug/net10.0
      ./azmcp[.exe] tools list --name --namespace <tool_area>
      ```
   - Commit the change.

8. **Create Pull Request**:
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

#### Cancellation plumbing

To ensure the product code and unit tests can be cancelled quickly, contributors are required to write async methods (any returning `Task`, `ValueTask`, generic variants of those, etc.) to accept and invoke async methods with a `System.Threading.CancellationToken` parameter. The latter is enforced with the [CA2016 analyzer](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2016).

Mocks created with `NSubstitute.Substitute.For<T>()` and have [methods set up](https://nsubstitute.github.io/help/set-return-value/#for-methods) should be passed `NSubstitute.Arg.Any<CancellationToken>()` for required `System.Threading.CancellationToken` parameters. The same should be used when [checking for received calls on a mocked object](https://nsubstitute.github.io/help/received-calls/index.html). If the product code is expected to do something interesting with a supplied `System.Threading.CancellationToken` parameter, such as linking with other `System.Threading.CancellationToken`s with [`System.Threading.CancellationTokenSource.CreateLinkedTokenSource`](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtokensource.createlinkedtokensource), then consider testing for that behavior.

Real product code under unit testing must be passed `Xunit.TestContext.Current.CancellationToken` when async methods are invoked. This is to ensure the tests can end to avoid possible issues with the parent process waiting indefinitely for the test runner executable to exit.

### End-to-end Tests

End-to-end tests are performed manually. Command authors must thoroughly test each command to ensure correct tool invocation and results. At least one prompt per tool is required and should be added to `/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`.

### Testing Local Build with VS Code

To run the Azure MCP server from source for local development:

#### Build the Server

Build the project at the root directory of this repository:

```bash
dotnet build
```

#### Run the Azure MCP server in HTTP mode

**Option 1: Using dotnet run (uses launchSettings.json)**

**Prerequisites: Create launchSettings.json**

> [!NOTE]
> Internal contributors may skip this step as the `launchSettings.json` file is already provided in the repository.

Before running the server in HTTP mode, you need to create the `launchSettings.json` file with the `debug-remotemcp` profile:

1. Create the directory (if it doesn't exist):
   ```bash
   mkdir -p servers/Azure.Mcp.Server/src/Properties
   ```

2. Create `servers/Azure.Mcp.Server/src/Properties/launchSettings.json` with the following content:
   ```json
   {
     "profiles": {
       "debug-remotemcp": {
         "commandName": "Project",
         "commandLineArgs": "server start --transport http --outgoing-auth-strategy UseHostingEnvironmentIdentity",
         "environmentVariables": {
           "ASPNETCORE_ENVIRONMENT": "Development",
           "ASPNETCORE_URLS": "http://localhost:<port>",
           "AzureAd__TenantId": "<your-tenant-id>",
           "AzureAd__ClientId": "<your-client-id>",
           "AzureAd__Instance": "https://login.microsoftonline.com/"
         }
       }
     }
   }
   ```

3. Replace `<your-tenant-id>` and `<your-client-id>` with your actual tenant ID and client ID.

```bash
dotnet run --project servers/Azure.Mcp.Server/src/ --launch-profile debug-remotemcp
```

**Option 2: Using the built executable directly**

Build the project first, then run the executable with the necessary environment variables:

```powershell
# Set environment variables (PowerShell)
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:<port>"
$env:AzureAd__TenantId = "<your-tenant-id>"
$env:AzureAd__ClientId = "<your-client-id>"
$env:AzureAd__Instance = "https://login.microsoftonline.com/"

# Run the executable
./servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp.exe server start --transport http --outgoing-auth-strategy UseHostingEnvironmentIdentity
```

```bash
# Set environment variables (Bash)
export ASPNETCORE_ENVIRONMENT="Development"
export ASPNETCORE_URLS="http://localhost:<port>"
export AzureAd__TenantId="<your-tenant-id>"
export AzureAd__ClientId="<your-client-id>"
export AzureAd__Instance="https://login.microsoftonline.com/"

# Run the executable
./servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp server start --transport http --outgoing-auth-strategy UseHostingEnvironmentIdentity
```

> **Note:** The environment variables listed above are taken from the `debug-remotemcp` profile in `launchSettings.json`. Replace `<your-tenant-id>` and `<your-client-id>` with your actual Azure AD tenant ID and client ID. These variables configure Azure AD authentication and the server endpoint for HTTP mode operation.
>
> For local development, when running with HTTPS (either via `ASPNETCORE_URLS` or HTTPS redirection), you must generate a self-signed development certificate:
>
>**Windows and macOS:**
> ```bash
> dotnet dev-certs https --trust
> ```
>
> **Linux:**
> ```bash
> dotnet dev-certs https
> ```
>
> On Linux, you must manually trust the generated certificate. See the [official documentation](https://learn.microsoft.com/dotnet/core/tools/dotnet-dev-certs) for instructions on how to do this.

#### Configure mcp.json

Update your mcp.json to point to the locally built azmcp executable.

**Stdio Mode:**

```json
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio",
      "command": "<absolute-path-to>/mcp/servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp[.exe]",
      "args": ["server", "start"]
    }
  }
}
```

**HTTP Mode:**

```json
{
  "servers": {
    "Azure MCP Server": {
      "url": "https://localhost:1031/",
      "type": "http"
    }
  }
}
```

> [!NOTE]
> For stdio mode, replace `<absolute-path-to>` with the full path to your built executable.
> On **Windows**, use `azmcp.exe`. On **macOS/Linux**, use `azmcp`.
> For HTTP mode, ensure the server is running on the specified port before connecting (port 1031 is the default port configured in launchSettings.json).

#### Server Modes

Optional `--namespace` and `--mode` parameters can be used to configure different server modes:

**Default Mode** (no additional parameters):

```json
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio",
      "command": "<absolute-path-to>/mcp/servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp[.exe]",
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
      "command": "<absolute-path-to>/mcp/servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp[.exe]",
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
      "command": "<absolute-path-to>/mcp/servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp[.exe]",
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
      "command": "<absolute-path-to>/mcp/servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp[.exe]",
      "args": ["server", "start", "--mode", "single"]
    }
  }
}
```

**Consolidated Mode** (grouped related operations):
It honors both --read-only and --namespace switches.

```json
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio",
      "command": "<absolute-path-to>/mcp/servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp[.exe]",
      "args": ["server", "start", "--mode", "consolidated"]
    }
  }
}
```

**Combined Mode** (filter namespaces with any mode):

```json
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio",
      "command": "<absolute-path-to>/mcp/servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp[.exe]",
      "args": ["server", "start", "--namespace", "storage", "--namespace", "keyvault", "--mode", "namespace"]
    }
  }
}
```

**Specific Tool Mode** (expose only specific tools):

```json
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio",
      "command": "<absolute-path-to>/mcp/servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp[.exe]",
      "args": ["server", "start", "--tool", "azmcp_storage_account_get", "--tool", "azmcp_subscription_list"]
    }
  }
}
```

> **Server Mode Summary:**
>
> - **Default Mode (Namespace)**: No additional parameters - collapses tools by namespace (current default)
> - **Consolidated Mode**: `--mode consolidated` - exposes consolidated tools grouping related operations, optimized for AI agents.
> - **Namespace Mode**: `--namespace <service-name>` - expose specific services
> - **Namespace Proxy Mode**: `--mode namespace` - collapse tools by namespace (useful for VS Code's 128 tool limit)
> - **All Tools Mode**: `--mode all` - expose all ~800+ individual tools
> - **Single Tool Mode**: `--mode single` - single "azure" tool with internal routing
> - **Specific Tool Mode**: `--tool <tool-name>` - expose only specific tools by name (finest granularity)
> - **Combined Mode**: Multiple options can be used together (`--namespace` + `--mode` etc.)

#### Start from IDE

With the configuration in place, you can launch the MCP server directly from your IDE or any tooling that uses `mcp.json`.

### Testing Local Build with Docker

To build a local image for testing purposes:

1. Execute: `./eng/scripts/Build-Docker.ps1 -ServerName "Azure.Mcp.Server"`.
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
            "azure-sdk/azure-mcp:<version-number-of-docker-image>",
          ]
        }
      }
    }
    ```

### Live Tests

> [!IMPORTANT]
> If you are a **Microsoft employee** with Azure source permissions then please review our [Azure Internal Onboarding Documentation](https://aka.ms/azmcp/intake). As part of reviewing community contributions, Azure team members can run live tests by adding this comment to the PR `/azp run  mcp - pullrequest - live`.

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

After deploying test resources, you should have a `.testsettings.json` file with your deployment information in the deployed paths' `/tests` directory.

Run live tests with:

```pwsh
./eng/scripts/Test-Code.ps1 -TestType Live
```

You can scope tests to specific paths:

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

1. Build the package with debug symbols: `./eng/scripts/Build-Local.ps1`
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

### Package README

A single package README.md could be used to generate context specific content for different package types (npm, nuget, vsix) using html comment annotations to mark sections for removal or insertion whem processed with script at `.\eng\scripts\Process-PackageReadMe.ps1`

Supported comment annotations:

- Section Removal
  - **Purpose:** Remove one or more lines, or parts of a line of markdown for specified package types.
  - **Example:**
  ```
  <!-- remove-section: start nuget;npm remove_various_lines -->
  ......
  various markdown lines to be removed for nuget and npm
  ......
  <!-- remove-section: end remove_various_lines -->
  ```

- Section Insert
  - **Purpose:** Insert a chunk of text into a line for a specified package type.
  - **Example:**
  `<!-- insert-section: nuget;vsix;npm {{Text to be inserted}} -->`

You can verify that your README.md was annotated correctly using the `Validate-PackageReadme` function:
```
& "eng\scripts\Process-PackageReadMe.ps1" -Command "validate" -InputReadMePath "<README.md Path>"
```

To extract README.md for a specific package, run the `Extract-PackageSpecificReadMe` function:
```
& "eng\scripts\Process-PackageReadMe.ps1" -Command "extract" `
    -InputReadMePath "<README.md Path>" `
    -OutputDirectory "<Output Directory for the package specific README.md>" `
    -PackageType "<npm, nuget, or vsix>" `
    -InsertPayload "<Package specific content to be inserted>"
```

## Advanced Configuration

### Configuring External MCP Servers

The Azure MCP Server supports connecting to external MCP servers through an embedded `registry.json` configuration file. This enables the server to act as a proxy, aggregating tools from multiple MCP servers into a single interface. The registry follows the same configuration schema as VS Code's `mcp.json`.

#### Registry Configuration

External MCP servers are defined in the embedded resource file `core/Azure.Mcp.Core/src/Areas/Server/Resources/registry.json`. This file contains server configurations that support both SSE (Server-Sent Events) and stdio transport mechanisms, following the standard MCP configuration format.

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

1. Edit `core/Azure.Mcp.Core/src/Areas/Server/Resources/registry.json`
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

**CI Validation Checks**:

All PRs automatically run the following validation checks:
- **Code formatting** - Ensures code follows project formatting standards
- **Spelling check** - Validates spelling across the codebase
- **AOT compatibility** - Checks ahead-of-time compilation compatibility
- **Tool metadata verification** - Ensures `azmcp-commands.md` is up-to-date with tool metadata (run `.\eng\scripts\Update-AzCommandsMetadata.ps1` if this fails)

## Support and Community

Please see our [support](https://github.com/microsoft/mcp/blob/main/SUPPORT.md) statement.

### Questions and Support

We're building this in the open.  Your feedback is much appreciated, and will help us shape the future of the Azure MCP server.

👉 [Open an issue in the public repository](https://github.com/microsoft/mcp/issues/new/choose).

### Additional Resources

- [Azure MCP Documentation](https://github.com/microsoft/mcp/blob/main/README.md)
- [Command Implementation Guide](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/new-command.md)
- [VS Code Insiders Download](https://code.visualstudio.com/insiders/)
- [GitHub Copilot Documentation](https://docs.github.com/en/copilot)

### Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments. By participating, you are expected to uphold this code.

### License

By contributing, you agree that your contributions will be licensed under the project's [license](https://github.com/microsoft/mcp/blob/main/LICENSE).
