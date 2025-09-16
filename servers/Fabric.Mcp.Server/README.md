# <img height="36" width="36" src="https://learn.microsoft.com/fabric/media/fabric-icon.svg" alt="Microsoft Fabric Logo" /> Microsoft Fabric MCP Server

The Microsoft Fabric MCP Server implements the [Model Context Protocol specification](https://modelcontextprotocol.io) to provide AI agents with comprehensive access to Microsoft Fabric's public APIs, item definitions, and best practices. This server packages Fabric's complete OpenAPI specifications into a single context layer for AI-assisted development.

> **📢 Public Preview Notice**: The Fabric MCP Server is currently in public preview. Implementation may change significantly prior to General Availability.

## 🌟 What is the Fabric MCP Server?

The Fabric MCP Server is a **local-first**, **AI-ready** Model Context Protocol server that gives AI agents the knowledge they need to generate robust, production-ready code for Microsoft Fabric without directly accessing your environment. It provides:

- **Complete API Context**: Full OpenAPI specifications for all supported Fabric workloads
- **Item Definition Knowledge**: JSON schemas for every Fabric item type (Lakehouses, pipelines, semantic models, notebooks, etc.)
- **Built-in Best Practices**: Embedded guidance on pagination, error handling, and recommended patterns
- **Local-First Security**: Runs entirely on your machine - never connects to your Fabric environment
- **Open Source & Extensible**: Part of the Microsoft MCP initiative with community contributions welcome

## Table of Contents
- [Quick Start](#quick-start)
- [Installation](#installation)
- [What Can You Do?](#what-can-you-do-with-the-fabric-mcp-server)
- [Available Tools](#available-tools)
- [Key Features](#key-features)
- [Best Practices](#best-practices)
- [Contributing](#contributing)
- [Support](#support)

## <a id="quick-start"></a> 🚀 Quick Start

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- An MCP-compatible client (e.g., [Claude Desktop](https://claude.ai/download), [VS Code](https://code.visualstudio.com/) with [GitHub Copilot](https://github.com/features/copilot))

### Build and Run

1. **Clone the repository**:
   ```bash
   git clone https://github.com/microsoft/mcp.git
   cd mcp
   ```

2. **Build the Fabric MCP Server**:
   ```bash
   dotnet build servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj
   ```

3. **Run the server**:
   ```bash
   dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj
   ```

The server runs locally and provides read-only access to Fabric API specifications without requiring live connections to your Fabric environment.

## Installation

### Manual Installation

Add the Fabric MCP Server to your MCP client configuration:

```json
{
  "servers": {
    "Microsoft Fabric MCP": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "path/to/servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj"
      ]
    }
  }
}
```

### VS Code Configuration

For [VS Code with GitHub Copilot](https://code.visualstudio.com/docs/copilot/overview), add to `.vscode/mcp.json`:

```json
{
  "servers": {
    "Microsoft Fabric MCP": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj"
      ]
    }
  }
}
```

## <a id="what-can-you-do-with-the-fabric-mcp-server"></a> ✨ What Can You Do with the Fabric MCP Server?

The Fabric MCP Server enables powerful AI-assisted scenarios:

### 🏗️ AI-Assisted Item Authoring
* "Generate a Lakehouse definition with proper schema constraints"
* "Create a Data Factory pipeline that processes CSV files from OneLake"
* "Build a semantic model definition for my sales data"

### 🔌 Intelligent Integration Code
* "Write a Python script to create and configure a workspace using Fabric APIs"
* "Generate code to upload data to a Lakehouse with proper error handling"
* "Create a script to manage semantic model refresh operations"

### 📚 API Discovery and Learning
* "What are the available Fabric workload types?"
* "Show me the API specification for notebook management"
* "What are the required properties for creating an Eventhouse?"

### 🎯 Best Practice Guidance
* "How should I handle pagination in Fabric REST API calls?"
* "What's the recommended pattern for long-running operations?"
* "Show me examples of proper authentication with Fabric APIs"

## Available Tools

The Fabric MCP Server provides the following tool categories:

### 📋 Platform APIs
- [`platform list-workloads`](tools/Fabric.Mcp.Tools.PublicApi/src/Commands/PublicApis/ListWorkloadsCommand.cs:11) - List all available Fabric workload types
- [`platform get-platform-apis`](tools/Fabric.Mcp.Tools.PublicApi/src/Commands/PublicApis/GetPlatformApisCommand.cs:11) - Get platform-level API specifications
- [`platform get-workload-apis`](tools/Fabric.Mcp.Tools.PublicApi/src/Commands/PublicApis/GetWorkloadApisCommand.cs:14) - Get workload-specific API specifications

### 🏗️ Best Practices & Examples
- [`bestpractices get-best-practices`](tools/Fabric.Mcp.Tools.PublicApi/src/Commands/BestPractices/GetBestPracticesCommand.cs:13) - Get embedded best practice documentation
- [`examples get-examples`](tools/Fabric.Mcp.Tools.PublicApi/src/Commands/BestPractices/GetExamplesCommand.cs:13) - Retrieve example API request/response files
- [`itemdefinition get-workload-definition`](tools/Fabric.Mcp.Tools.PublicApi/src/Commands/BestPractices/GetWorkloadDefinitionCommand.cs:13) - Get JSON schema definitions for workload items

### 🗂️ Supported Workloads

The server provides complete API context for these Fabric workloads:

<details>
<summary>📊 Data Analytics & Storage</summary>

* **Lakehouse** - Big data analytics and storage
* **Warehouse** - Cloud data warehouse solutions  
* **KQL Database** - Real-time analytics database
* **Eventhouse** - Event streaming and analytics
* **Semantic Model** - Business intelligence data models

</details>

<details>
<summary>🔄 Data Integration & Processing</summary>

* **Data Pipeline** - ETL/ELT workflow orchestration
* **Dataflow** - Data transformation workflows
* **Copy Job** - Data movement operations
* **Apache Airflow Job** - Workflow scheduling and orchestration
* **Spark Job Definition** - Big data processing jobs

</details>

<details>
<summary>📈 Analytics & Reporting</summary>

* **Report** - Power BI reports and dashboards
* **Notebook** - Collaborative data science notebooks
* **KQL Queryset** - Real-time query definitions
* **KQL Dashboard** - Real-time analytics dashboards

</details>

<details>
<summary>🌊 Real-Time & Streaming</summary>

* **Eventstream** - Real-time data streaming
* **Reflex** - Real-time data processing

</details>

<details>
<summary>🔗 Integration & APIs</summary>

* **GraphQL API** - GraphQL endpoint definitions
* **Environment** - Development environment configurations
* **Variable Library** - Shared variable management

</details>

<details>
<summary>🏥 Specialized Workloads</summary>

* **Mirrored Database** - Database mirroring solutions
* **Mirrored Azure Databricks Unity Catalog** - Databricks integration
* **Digital Twin Builder** - IoT digital twin modeling
* **Digital Twin Builder Flow** - Digital twin workflows
* **HLS Cohort** - Healthcare analytics cohorts
* **Mounted Data Factory** - External data factory integration

</details>

## Key Features

### 🔒 Local-First Security
- **No Live Connections**: Never connects to your Fabric environment - operates entirely offline
- **Credential Safety**: No authentication required or credentials stored locally
- **Generated Code Review**: You maintain full control over when and how to execute generated code
- **Offline Access**: Complete API documentation available without internet connectivity

### 📖 Comprehensive API Knowledge
- **Complete Coverage**: All public Fabric REST APIs included with full specifications
- **Up-to-Date**: Embedded from official [Microsoft Fabric API repository](https://github.com/microsoft/fabric-docs)
- **Detailed Schemas**: Request/response models with complete property definitions and constraints
- **Authentication Guidance**: Built-in patterns for secure API access using [Azure Identity](https://docs.microsoft.com/azure/developer/intro/azure-developer-cli)

### 🎯 Built-in Best Practices
- **Pagination Patterns**: Proper handling of large result sets with continuation tokens
- **Long-Running Operations**: Recommended polling and monitoring approaches for async operations
- **Error Handling**: Robust error management strategies with retry logic
- **Rate Limiting**: Guidance on API throttling, quotas, and backoff strategies

### 🔧 Developer Experience
- **IntelliSense Ready**: JSON schemas enable rich IDE support and auto-completion
- **Example-Driven**: Real request/response examples for every operation and workload
- **Cross-Platform**: Runs on Windows, macOS, and Linux with .NET 9+
- **Extensible**: Easy to add custom templates, guidance, and workload definitions

## Best Practices

### 🔄 Long-Running Operations
Many Fabric operations are asynchronous. The server includes guidance on:
- Polling operation status endpoints
- Handling operation timeouts
- Managing concurrent operations
- Status code interpretation

### 📄 Pagination
For operations returning large datasets:
- Use `continuationToken` for result paging
- Implement proper page size limits
- Handle continuation token expiration
- Cache results when appropriate

### 🛡️ Authentication & Security
- Use Azure Identity SDK for credential management
- Implement proper token refresh logic
- Follow least-privilege access principles
- Secure credential storage patterns

### ⚡ Performance Optimization
- Batch operations when possible
- Use appropriate filters to limit result sets
- Implement client-side caching strategies
- Monitor API rate limits and quotas

## Contributing

We welcome contributions to the Fabric MCP Server! This is an open-source project and part of the broader Microsoft MCP initiative.

See our [CONTRIBUTING Guide](../../CONTRIBUTING.md) and [SUPPORT.md](SUPPORT.md) for detailed information on how to contribute, report issues, and get help.

## Support

For comprehensive support information, troubleshooting guides, and community resources, see [SUPPORT.md](SUPPORT.md).

For common issues and solutions, see [TROUBLESHOOTING.md](TROUBLESHOOTING.md).

## What's Next

The Fabric MCP Server is evolving rapidly. Upcoming features include:

- **🌐 Hosted Experiences**: Cloud-based server options
- **🔗 Cross-Platform Integration**: Enhanced compatibility with other MCP servers
- **📊 Extended Templates**: More workload types and scenarios
- **⚡ Performance Improvements**: Faster API response times
- **🎯 Smart Recommendations**: AI-powered best practice suggestions

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.

---

**Ready to supercharge your Fabric development with AI?** 

Star ⭐ this repository to stay updated, try the server with your favorite AI assistant, and join our community of developers building the future of data analytics automation!