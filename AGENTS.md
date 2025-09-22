# AGENTS.md

## Project Overview
Microsoft MCP (Model Context Protocol) servers provide AI agents with structured access to Azure, Microsoft Fabric, and other Microsoft services. This repository contains the core libraries, multiple MCP servers, service-specific tools, and comprehensive testing infrastructure for building agent-integrated Microsoft service interactions.

**Key Components:**
- **Azure MCP Server**: Complete Azure service integration with 100+ tools
- **Microsoft Fabric MCP Server**: Fabric workspace and data platform operations  
- **Core Libraries**: Shared infrastructure for command patterns, authentication, and MCP protocol
- **Toolsets**: Individual Azure service implementations (Storage, SQL, KeyVault, etc.)
- **Engineering System**: Build pipelines, testing infrastructure, and deployment automation

## Development Environment Setup

### Prerequisites
1. **Visual Studio Code**: [VS Code Stable](https://code.visualstudio.com/download) or [Insiders](https://code.visualstudio.com/insiders)
2. **GitHub Copilot**: Install [GitHub Copilot](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) and [GitHub Copilot Chat](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot-chat) extensions
3. **Node.js**: [Node.js 20+](https://nodejs.org/en/download) (ensure `node` and `npm` are in PATH)
4. **PowerShell**: [PowerShell 7.0+](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) (required for build/test scripts)
5. **.NET SDK**: .NET 10.0.100-preview.7+ (configured in `global.json`)
6. **Azure PowerShell**: For live tests - [Install Azure PowerShell](https://learn.microsoft.com/powershell/azure/install-azure-powershell)
7. **Azure Bicep**: For test infrastructure - [Install Azure Bicep](https://learn.microsoft.com/azure/azure-resource-manager/bicep/install#install-manually)

### Quick Start Commands
```powershell
# Clone and build the project
git clone https://github.com/microsoft/mcp.git
cd mcp
dotnet build

# Verify everything works
./eng/scripts/Build-Local.ps1 -UsePaths -VerifyNpx

# Run unit tests for specific toolset
./eng/scripts/Test-Code.ps1 -Paths Storage

# Run all unit tests
./eng/scripts/Test-Code.ps1
```

## Architecture and Project Structure

### Repository Organization
```
├── core/                           # Core libraries and shared components
│   ├── Azure.Mcp.Core/            # Azure MCP core library
│   ├── Microsoft.Mcp.Core/        # Base MCP protocol implementation
│   └── Fabric.Mcp.Core/           # Fabric-specific core (extends Azure.Mcp.Core)
├── servers/                        # Individual MCP servers
│   ├── Azure.Mcp.Server/          # Azure MCP server implementation
│   ├── Fabric.Mcp.Server/         # Microsoft Fabric MCP server
│   └── Template.Mcp.Server/       # Template for new MCP servers
├── tools/                          # Service-specific toolset implementations
│   ├── Azure.Mcp.Tools.Storage/   # Azure Storage operations
│   ├── Azure.Mcp.Tools.KeyVault/  # Azure Key Vault operations
│   ├── Azure.Mcp.Tools.Sql/       # Azure SQL operations
│   └── [60+ other Azure services] # Each Azure service has its own toolset
├── eng/                           # Engineering system and build infrastructure
│   ├── scripts/                   # Build, test, and deployment scripts
│   ├── pipelines/                 # Azure DevOps pipeline definitions
│   └── tools/                     # Development and validation tools
└── docs/                          # Documentation and implementation guides
```

### Toolset Architecture Pattern
Each Azure service follows a consistent pattern:
```
Azure.Mcp.Tools.{Service}/
├── src/
│   ├── Commands/                  # Command implementations
│   │   └── {Resource}/           # Resource-specific commands
│   ├── Services/                 # Service layer implementations
│   ├── Options/                  # Command option definitions
│   ├── Models/                   # Data models and DTOs
│   └── {Service}Setup.cs         # Service registration and configuration
└── tests/
    ├── Azure.Mcp.Tools.{Service}.UnitTests/     # Unit tests (no Azure resources)
    ├── Azure.Mcp.Tools.{Service}.LiveTests/     # Integration tests (requires Azure)
    ├── test-resources.bicep                     # Test infrastructure template
    └── test-resources-post.ps1                  # Post-deployment setup script
```

### Command Naming Convention
Commands follow the pattern: `azmcp <service> <resource> <operation>`
```bash
# Examples
azmcp storage account list          # List storage accounts
azmcp sql database show            # Show SQL database details  
azmcp keyvault secret get          # Get Key Vault secret
azmcp resourcegroup list           # List resource groups
```

## Build Commands and Development Workflow

### Core Build Commands
```powershell
# Basic build (fastest for development)
dotnet build

# Full verification build (recommended before PR)
./eng/scripts/Build-Local.ps1 -UsePaths -VerifyNpx

# AOT-compatible build (tests native compilation)
./eng/scripts/Build-Local.ps1 -BuildNative

# Build with debugging symbols
./eng/scripts/Build-Local.ps1 -DebugBuild

# Docker image build
./eng/scripts/Build-Docker.ps1
```

### Testing Commands
```powershell
# Unit tests only (no Azure resources required)
./eng/scripts/Test-Code.ps1

# Specific toolset unit tests
./eng/scripts/Test-Code.ps1 -Paths Storage, KeyVault

# Live tests (requires Azure authentication and resources)
./eng/scripts/Test-Code.ps1 -TestType Live -Paths Storage

# Deploy test infrastructure for live tests
./eng/scripts/Deploy-TestResources.ps1 -Paths Storage

# Run tests from specific directory
pushd 'tools/Azure.Mcp.Tools.Storage/tests/Azure.Mcp.Tools.Storage.UnitTests'
dotnet test --filter "FullyQualifiedName~StorageAccountListCommandTests"
popd
```

### Code Quality and Validation
```powershell
# Format code and remove unused using statements
dotnet format

# Format specific toolset
dotnet format --include="tools/Azure.Mcp.Tools.Storage/**/*.cs"

# Spelling check
.\eng\common\spelling\Invoke-Cspell.ps1

# AOT compatibility analysis
./eng/scripts/Analyze-AOT-Compact.ps1

# Tool description quality validation
pushd 'eng/tools/ToolDescriptionEvaluator'
dotnet run -- --validate --tool-description "Your command description" --prompt "user query"
popd
```

## Testing Strategy and Patterns

### Unit Testing Requirements
All commands must include comprehensive unit tests:
```csharp
// Required test patterns for every command
[Fact] public void Constructor_InitializesCommandCorrectly()
[Theory] public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
[Fact] public async Task ExecuteAsync_DeserializationValidation()
[Fact] public async Task ExecuteAsync_HandlesServiceErrors()
[Fact] public void BindOptions_BindsOptionsCorrectly()
```

### Live Test Infrastructure
Azure service commands require Bicep templates for test resources:
```powershell
# Deploy test infrastructure
./eng/scripts/Deploy-TestResources.ps1 -Paths {Toolset}

# Required files for Azure service toolsets:
# - tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources.bicep
# - tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources-post.ps1
```

### Authentication for Live Tests
```powershell
# Azure authentication setup
Connect-AzAccount
az login

# Test resource deployment with proper RBAC
./eng/scripts/Deploy-TestResources.ps1 -Paths Storage -SubscriptionId {subscription-id}
```

## Code Style and Standards

### C# Coding Standards
- **Always use primary constructors** for dependency injection
- **Always use `System.Text.Json`** over Newtonsoft.Json
- **Make all command classes sealed** unless designed for inheritance
- **Always make members static** when possible for AOT compatibility
- **Put new classes and interfaces in separate files**
- **Always run `dotnet build`** after making changes
- **All generated code must be AOT-safe**

### File and Class Naming Patterns
```csharp
// Command naming: {Resource}{Operation}Command
public sealed class StorageAccountListCommand    // ✅ Correct
public sealed class ListStorageAccountCommand    // ❌ Wrong order

// Options naming: {Resource}{Operation}Options  
public class StorageAccountListOptions          // ✅ Correct

// Test naming: {Command}Tests
public class StorageAccountListCommandTests     // ✅ Correct
```

### Option Handling Pattern
```csharp
// Use extension methods for flexible option requirements
protected override void RegisterOptions(Command command)
{
    base.RegisterOptions(command);
    command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
    command.Options.Add(StorageOptionDefinitions.Account.AsOptional());
}

// Use name-based binding with type safety
protected override StorageAccountListOptions BindOptions(ParseResult parseResult)
{
    var options = base.BindOptions(parseResult);
    options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
    options.Account = parseResult.GetValueOrDefault<string>(StorageOptionDefinitions.Account.Name);
    return options;
}
```

### Parameter Naming Standards
- **Use `subscription`** (never `subscriptionId`) - supports both IDs and names
- **Use `resourceGroup`** (not `resourceGroupName`)  
- **Use singular nouns** for resource names (e.g., `server`, not `serverName`)
- **Remove unnecessary "-name" suffixes** (e.g., `--account` vs `--account-name`)

## Error Handling Patterns

### Standard Error Response Format
```csharp
// Override error handling for service-specific context
protected override string GetErrorMessage(Exception ex) => ex switch
{
    Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
        "Resource not found. Verify the resource exists and you have access.",
    Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
        $"Authorization failed accessing the resource. Details: {reqEx.Message}",
    Azure.Identity.AuthenticationFailedException =>
        "Authentication failed. Please run 'az login' to sign in.",
    _ => base.GetErrorMessage(ex)
};

protected override int GetStatusCode(Exception ex) => ex switch
{
    Azure.RequestFailedException reqEx => reqEx.Status,
    Azure.Identity.AuthenticationFailedException => 401,
    ValidationException => 400,
    _ => base.GetStatusCode(ex)
};
```

### Exception Handling in Commands
```csharp
try
{
    // Command execution logic
    var results = await service.GetResourcesAsync(options.Subscription!, options.RetryPolicy);
    context.Response.Results = ResponseResult.Create(new(results ?? []), ServiceJsonContext.Default.CommandResult);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error in {Operation}. Options: {@Options}", Name, options);
    HandleException(context, ex);  // Always call base handler
}
```

## Service Implementation Patterns

### Base Service Classes
Choose the appropriate base class based on operations:

**For Resource Graph queries (recommended):**
```csharp
public class StorageService(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureResourceService(subscriptionService, tenantService), IStorageService
{
    public async Task<List<StorageAccount>> ListAccountsAsync(string subscription, string? resourceGroup, RetryPolicyOptions? retryPolicy)
    {
        return await ExecuteResourceQueryAsync(
            "Microsoft.Storage/storageAccounts",
            resourceGroup,
            subscription,
            retryPolicy,
            ConvertToStorageAccountModel);
    }
}
```

**For direct ARM operations:**
```csharp
public class StorageService(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureService(tenantService), IStorageService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;

    public async Task<StorageAccount> GetAccountAsync(string subscription, string resourceGroup, string accountName, RetryPolicyOptions? retryPolicy)
    {
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy);
        // Use subscriptionResource for direct ARM operations
    }
}
```

### JSON Serialization Context (AOT Requirement)
```csharp
// All response models must be registered for AOT compatibility
[JsonSerializable(typeof(StorageAccountListCommand.StorageAccountListCommandResult))]
[JsonSerializable(typeof(StorageAccount))]  
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
internal partial class StorageJsonContext : JsonSerializerContext;

// Usage in commands
context.Response.Results = ResponseResult.Create(new(results), StorageJsonContext.Default.StorageAccountListCommandResult);
```

## Adding New Commands and Services

### Development Process
1. **Create issue**: "Add command: azmcp [service] [resource] [operation]"
2. **Use Copilot for generation**: Execute in Copilot Chat: `"create [service] [resource] [operation] command using #new-command.md as a reference"`
3. **Follow implementation guidelines** in `/docs/new-command.md`
4. **Create live test infrastructure** (if Azure service): Bicep template and post-deployment script
5. **Submit one tool per pull request** for faster review cycles

### Required Files for New Commands
```
tools/Azure.Mcp.Tools.{Service}/
├── src/
│   ├── Options/{Service}OptionDefinitions.cs        # Static option definitions
│   ├── Options/{Resource}/{Operation}Options.cs     # Command-specific options
│   ├── Commands/{Resource}/{Resource}{Operation}Command.cs  # Command implementation  
│   ├── Services/I{Service}Service.cs                # Service interface
│   ├── Services/{Service}Service.cs                 # Service implementation
│   └── Commands/{Service}JsonContext.cs             # JSON serialization context
└── tests/
    ├── Azure.Mcp.Tools.{Service}.UnitTests/{Resource}/{Resource}{Operation}CommandTests.cs
    ├── Azure.Mcp.Tools.{Service}.LiveTests/{Service}CommandTests.cs
    ├── test-resources.bicep                          # Test infrastructure (Azure services only)
    └── test-resources-post.ps1                       # Post-deployment script (Azure services only)
```

### Tool Description Quality Validation
```powershell
# Validate command descriptions for AI agent compatibility
pushd 'eng/tools/ToolDescriptionEvaluator'

# Single prompt validation
dotnet run -- --validate --tool-description "Lists storage accounts in a subscription" --prompt "show me my storage accounts"

# Multiple prompt validation
dotnet run -- --validate \
  --tool-description "Lists storage accounts in a subscription" \
  --prompt "show storage accounts" \
  --prompt "list my storage" \
  --prompt "what storage do I have"

# Custom files validation
dotnet run -- --tools-file my-tools.json --prompts-file my-prompts.md
popd

# Target: Top 3 ranking and confidence score ≥ 0.4
```

## Local Development and Testing

### Running Azure MCP Server Locally
```json
// mcp.json configuration for local development
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio", 
      "command": "C:/code/mcp/servers/Azure.Mcp.Server/bin/Debug/net9.0/azmcp.exe",
      "args": ["server", "start"]
    }
  }
}
```

### Server Mode Configurations
```json
// Namespace filtering (specific services only)
"args": ["server", "start", "--namespace", "storage", "--namespace", "keyvault"]

// Namespace proxy mode (groups tools, helpful for VS Code 128-tool limit)
"args": ["server", "start", "--mode", "namespace"]

// Single tool mode (single "azure" tool with internal routing)
"args": ["server", "start", "--mode", "single"]

// Combined mode (filter + proxy)
"args": ["server", "start", "--namespace", "storage", "--mode", "namespace"]
```

### Docker Development
```powershell
# Build local Docker image
./eng/scripts/Build-Docker.ps1

# Use in mcp.json
{
  "servers": {
    "Azure MCP Server": {
      "command": "docker",
      "args": ["run", "-i", "--rm", "--env-file", "/path/to/.env", "azure/azure-mcp:latest"]
    }
  }
}
```

## Performance and Compatibility

### AOT (Ahead-of-Time) Compilation
All new toolsets must be AOT-compatible or excluded from native builds:

```powershell
# Test AOT compatibility
./eng/scripts/Build-Local.ps1 -BuildNative

# If AOT fails (common for new Azure services), exclude toolset:
# 1. Move setup call in Program.cs under #if !BUILD_NATIVE
# 2. Add ProjectReference-Remove condition in Azure.Mcp.Server.csproj
```

### Caching and Performance
- Use `ICacheService` for expensive Azure operations
- Implement `BaseAzureResourceService` for efficient Resource Graph queries
- Follow retry policy patterns with `RetryPolicyOptions`

## External MCP Server Integration

The Azure MCP Server can proxy to external MCP servers via `registry.json`:

```json
// core/Azure.Mcp.Core/src/Areas/Server/Resources/registry.json
{
  "servers": {
    "documentation": {
      "url": "https://learn.microsoft.com/api/mcp",
      "description": "Search official Microsoft/Azure documentation"
    },
    "github-server": {
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-github@latest"],
      "description": "GitHub repository operations"
    }
  }
}
```

## Documentation and Compliance

### Required Documentation Updates
When adding new commands:
1. **Update `/docs/azmcp-commands.md`** with new command details
2. **Add test prompts to `/docs/e2eTestPrompts.md`** (maintain alphabetical order)
3. **Update toolset README.md** with new functionality
4. **Update CHANGELOG.md** with changes
5. **Add CODEOWNERS entry** for new toolset

### Spelling and Content Validation
```powershell
# Check spelling across codebase
.\eng\common\spelling\Invoke-Cspell.ps1

# Add new technical terms to .vscode/cspell.json if needed
```

## Git Workflow and Automation

### Git Hooks for Quality
```powershell
# Install pre-push hook (runs dotnet format automatically)
./eng/scripts/Install-GitHooks.ps1

# Remove git hooks
./eng/scripts/Remove-GitHooks.ps1
```

### Pull Request Guidelines
- **Run all tests**: `./eng/scripts/Test-Code.ps1`
- **Format code**: `dotnet format`
- **Check spelling**: `.\eng\common\spelling\Invoke-Cspell.ps1`
- **Validate tool descriptions**: Use ToolDescriptionEvaluator
- **Follow contribution guidelines**: See `CONTRIBUTING.md`
- **One tool per PR**: Submit single toolsets for faster review

## Advanced Configuration

### External Tool Integration
The server supports integration with external MCP servers through registry configuration, enabling aggregation of tools from multiple sources into a unified interface.

### Namespace-Based Tool Organization
Commands are organized by Azure service namespace, allowing for fine-grained control over exposed functionality and helping manage VS Code's 128-tool display limit.

### Telemetry and Monitoring  
The server includes comprehensive telemetry integration with proper tag propagation for monitoring tool usage and performance across different deployment scenarios.

---

This documentation provides AI agents with comprehensive guidance for working effectively with the Microsoft MCP codebase. For additional details, see `/docs/new-command.md` for implementation specifics and `CONTRIBUTING.md` for contribution workflows.