# Azure MCP Multi-CLI Architecture

## Overview

The Azure MCP (Model Context Protocol) project implements a multi-CLI architecture that enables AI agents to interact with different Microsoft cloud services through standardized communication patterns. This architecture supports multiple CLI implementations sharing a common core while providing specialized tooling for different Microsoft cloud ecosystems.

## Architecture Principles

- **Modular Design**: Each CLI implementation is self-contained with its own package and distribution
- **Shared Core**: Common functionality is abstracted into reusable core libraries
- **Area-Based Organization**: Functionality is organized into discrete areas that can be composed into different CLI configurations
- **Extensible Command System**: Dynamic command registration allows for flexible feature composition
- **Multi-Platform Support**: Native executables for different platforms (Windows, Linux, macOS) with ARM64 support

## Multi-CLI Structure

### CLI Implementations

The project supports multiple CLI implementations, each tailored for specific Microsoft cloud ecosystems:

```
cli/
├── azure/     # Azure-focused CLI implementation (@azure/mcp -> azmcp)
├── fabric/    # Microsoft Fabric-focused CLI implementation (fabric-mcp -> fabricmcp)
└── template/  # Template/example CLI implementation (mcp-template -> mcptmp)
```

### Core Architecture

```
core/
├── microsoft/      # Shared Microsoft MCP services core
├── azure/          # Extends Microsoft.Mcp.Core with Azure-specific classes
├── fabric/         # Extends Microsoft.Mcp.Core with Microsoft Fabric-specific classes
└── template/       # Template/example core implementation
```

### Area-Based Organization

Functionality is organized into discrete areas under the `areas/` directory:

```
areas/
├── azure-acr/             # Azure Container Registry
├── azure-aks/             # Azure Kubernetes Service
├── fabric-lakehouse/      # Fabric Lakehouse service
└── workbooks/             # Azure Workbooks
```

Each area follows a consistent structure:
```
{area}/
├── src/                   # Source code
│   └── AzureMcp.{Area}/   # Area-specific implementation
│       ├── Commands/      # Command implementations
│       ├── Models/        # Data models
│       ├── Services/      # Business logic
│       └── Resources/     # Static resources (best practices, etc.)
└── tests/                 # Unit tests
```

### Area Interface

Each area implements the `IAreaSetup` interface for standardized registration:

```csharp
public interface IAreaSetup
{
    void ConfigureServices(IServiceCollection services);
    void ConfigureCommands(CommandGroup parentGroup);
}
```

## Command Execution Flow

1. **CLI Entry Point**: Platform-specific Node.js wrapper or direct .NET executable
2. **Command Parsing**: System.CommandLine parses arguments and routes to appropriate command
3. **Service Resolution**: Dependency injection resolves required services
4. **Area Discovery**: Dynamic discovery of available service areas
5. **Command Registration**: Commands are registered from active areas
6. **Execution**: Command is executed with proper context and telemetry
7. **Response Serialization**: Results are serialized using System.Text.Json with source generation

## Configuration and Deployment

### Multi-Platform Distribution

The architecture supports multiple distribution strategies:

- **Node.js Packages**: Cross-platform npm packages with native binary wrapping
- **Native Executables**: Platform-specific binaries for optimal performance
- **Platform Variants**:
  - `linux-x64/`: Linux x64 distribution
  - `linux-arm64/`: Linux ARM64 distribution
  - Windows and macOS variants (via main CLI packages)

### AOT Compatibility

All generated code is AOT (Ahead-of-Time) compilation safe:
- System.Text.Json source generators
- Minimal reflection usage
- Static constructor patterns
- Primary constructor usage in C#

### Build System Integration

The architecture integrates with a sophisticated engineering system:

```
eng/
├── common/           # Shared engineering tools
├── docker/          # Container definitions
├── npm/             # Node.js packaging tools
├── pipelines/       # CI/CD pipeline definitions
├── scripts/         # Build and validation scripts
└── tools/           # Development tools
```

## Service Integration

### Telemetry and Observability

- **OpenTelemetry**: Distributed tracing across all CLI implementations
- **Activity Tracking**: Command execution monitoring
- **Performance Metrics**: Duration and success rate tracking
- **Structured Logging**: Consistent logging patterns across areas

### Authentication and Authorization

- **Azure Identity**: Integrated Azure authentication
- **Token Management**: Automatic token refresh and caching
- **Multi-Tenant Support**: Cross-tenant resource access
- **Service Principal**: Automated authentication for CI/CD scenarios

### Caching Strategy

- **Memory Caching**: In-memory caching for frequently accessed data
- **Token Caching**: Authentication token persistence
- **Resource Metadata**: Cached resource information for performance

## Extension and Customization

### Adding New Areas

1. Create area directory structure under `areas/{new-area}/`
2. Implement `IAreaSetup` interface
3. Add commands implementing `IBaseCommand`
4. Register area in appropriate CLI Program.cs
5. Add package references to Directory.Packages.props

### Creating New CLI Implementations

1. Create new CLI directory under `cli/{new-cli}/`
2. Define package.json with appropriate binary name
3. Create corresponding core implementation under `core/{new-cli}/`
4. Implement service area selection logic
5. Configure build pipeline for distribution

# Command Development

Commands follow a consistent pattern:
```csharp
public sealed class ExampleCommand : BaseCommand
{
    public override ToolMetadata Metadata => new() {
        Destructive = false,
        ReadOnly = true
    };

    public override Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult)
    {
        // Implementation
    }
}
```


# Build Artifact Flow

- Each step of the build flow is implemented in PowerShell or Azure Pipeline Yaml.
- The PowerShell scripts will accept input and output paths, with defaults set to flow from stage to stage
- In Azure pipelines, the outputs from each stage will be uploaded to the build for use across later jobs and stages
- In each Azure Pipeline job, the required artifacts are downloaded and their paths are passed to the appropriate stage script.

## Build
- Implemented in `eng/scripts/Build-Code.ps1`
- Used by `eng/pipelines/templates/jobs/build.yml`
- Produces artifacts in `.work/build` (overridden with `-OutputPath`)
- Uses [.NET runtime identifiers](https://learn.microsoft.com/dotnet/core/rid-catalog) for OS and Architecture names
  - win, linux, osx
  - x64, arm64
- Produces a folder per platform, per cli:
  ```
  .work/
    build/
      azure/
        win-x64/
        osx-arm64/
      azure-native/
      fabric/
  ```
- Outputs are not signed or packaged

## Test
- Implemented in `eng/scripts/Test-Code.ps1`
- Used by `eng/pipelines/templates/jobs/build.yml` and `live-test.yml`
- Tests from source and does not consume Build stage artifacts

## Sign
- Implemented in `eng/pipelines/templates/jobs/sign-and-pack.yml`
- There are no powershell scripts for local reproduction
- Replaces binaries in-place, so has no effect on input -> output paths

## Package
- Implemented in format specific scripts like:
  - `eng/scripts/Pack-Modules.ps1`  (npm)
  - `eng/scripts/Build-Docker.ps1` (docker)
- Used by `eng/pipelines/templates/jobs/sign-and-pack.yml`
- Consumes Build stage artifacts from `.work/build` (overridden with `-ArtifactsPath`)
- Produces artifacts in `.work/package` (overridden with `-OutputPath`)
- Produces a folder per format, per cli:
  ```
  .work/
    package/
      azure/
        npm/
          win-x64/
        vsix/
      azure-native/
      fabric/
  ```

## Release
- Implemented in `eng/pipelines/templates/jobs/release.yml`
- There are no powershell scripts for local reproduction
- Consumes Package stage artifacts
- Publishes them to external feeds and registries
