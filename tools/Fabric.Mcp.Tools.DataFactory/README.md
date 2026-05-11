# Fabric.Mcp.Tools.DataFactory

Microsoft Fabric Data Factory MCP (Model Context Protocol) Tools — Manage pipelines and dataflows through AI agents and MCP clients.

## Overview

Data Factory is Microsoft Fabric's cloud-scale data integration service for creating, scheduling, and orchestrating ETL/ELT workflows. This MCP tool provides operations for working with Data Factory resources, enabling AI agents to:

- List, create, and get pipeline details
- Run pipelines on demand
- List and create Dataflow Gen2 items
- Execute M (Power Query) queries against dataflows

**Features:**
- 7 Data Factory commands with full MCP integration
- Pipeline management: list, create, get, run
- Dataflow Gen2 management: list, create
- Dataflow query execution with M/Power Query support
- Robust error handling via `ToolResult<T>` pattern
- Unit tested with 45 tests covering metadata, constructors, and options

## Prerequisites

- Microsoft Fabric workspace with Data Factory capabilities
- Azure authentication (Azure CLI or managed identity)
- Access to the target Fabric workspace

## Authentication

The tool uses Azure authentication via the Fabric MCP Server's authentication infrastructure. Ensure you're logged in:

```bash
az login
```

## Available Commands

### Pipeline Commands

| Command | Description | Read Only |
|---------|-------------|-----------|
| `datafactory_list-pipelines` | Lists all pipelines in a workspace | ✓ |
| `datafactory_create-pipeline` | Creates a new pipeline | ✗ |
| `datafactory_get-pipeline` | Gets details of a specific pipeline | ✓ |
| `datafactory_run-pipeline` | Runs a pipeline on demand | ✗ |

### Dataflow Commands

| Command | Description | Read Only |
|---------|-------------|-----------|
| `datafactory_list-dataflows` | Lists all Dataflow Gen2 items in a workspace | ✓ |
| `datafactory_create-dataflow` | Creates a new Dataflow Gen2 item | ✗ |
| `datafactory_execute-query` | Executes an M query against a dataflow | ✓ |

## Example Prompts

- "List all pipelines in my workspace"
- "Create a new pipeline called 'Daily ETL'"
- "Run the pipeline with ID abc-123 in workspace xyz"
- "List all dataflows in my workspace"
- "Execute this M query against my dataflow: `let Source = Sql.Database(\"server\", \"db\") in Source`"

## Architecture

The DataFactory tools are built on the [Microsoft.DataFactory.MCP.Core](https://www.nuget.org/packages/Microsoft.DataFactory.MCP.Core) NuGet package, which provides:

- **Handlers**: `PipelineHandler`, `DataflowHandler`, `DataflowQueryHandler` — business logic with comprehensive error handling
- **Services**: HTTP clients for Fabric REST API communication
- **Models**: Strongly-typed DTOs for API request/response

Commands follow the MCP framework pattern:
```
Command (MCP schema + validation) → Handler (business logic + error handling) → Service (HTTP API call)
```

## Development

### Building

```bash
cd tools/Fabric.Mcp.Tools.DataFactory
dotnet build src/Fabric.Mcp.Tools.DataFactory.csproj
```

### Testing

```bash
dotnet test tests/Fabric.Mcp.Tools.DataFactory.UnitTests/Fabric.Mcp.Tools.DataFactory.UnitTests.csproj
```

### Project Structure

```
tools/Fabric.Mcp.Tools.DataFactory/
├── src/
│   ├── Commands/
│   │   ├── Pipeline/         # Pipeline CRUD + run commands
│   │   └── Dataflow/         # Dataflow CRUD + query commands
│   ├── Models/               # Result DTOs and JSON context
│   ├── Options/              # Command option definitions
│   └── DataFactoryAreaSetup.cs  # DI registration
└── tests/
    └── Fabric.Mcp.Tools.DataFactory.UnitTests/
        ├── DataFactoryAreaSetupTests.cs
        └── Commands/         # Per-command unit tests
```

## Related

- [Microsoft Fabric Data Factory documentation](https://learn.microsoft.com/fabric/data-factory/)
- [DataFactory.MCP.Core NuGet package](https://www.nuget.org/packages/Microsoft.DataFactory.MCP.Core)
- [Fabric MCP Server](https://github.com/microsoft/mcp/tree/main/servers/Fabric.Mcp.Server)
