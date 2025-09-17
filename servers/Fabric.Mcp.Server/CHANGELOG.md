# Changelog

All notable changes to the Microsoft Fabric MCP Server will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [0.0.1] - 2025-09-16

### Added

Initial release of the Microsoft Fabric MCP Server in **Public Preview**.

- **Complete API Context**: Full OpenAPI specifications for all supported Fabric workloads
- **Item Definition Knowledge**: JSON schemas for every Fabric item type including Lakehouse, Warehouse, KQL Database, Eventhouse, Data Pipeline, Dataflow, Copy Job, Apache Airflow Job, Notebook, Report, Semantic Model, KQL Queryset, Eventstream, Reflex, GraphQL API, Environment, and many more specialized workloads
- **Built-in Best Practices**: Embedded guidance for pagination patterns, long-running operation handling, error handling and retry logic, authentication and security best practices
- **Local-First Security**: Runs entirely on your machine without connecting to live Fabric environments
- **Platform APIs**: Core platform operations for workspace management and common resources
- **Example-Driven Development**: Real API request/response examples for every workload

#### Tool Categories Added

**Public API Operations**:
- `publicapis list-workloads` - List all available Fabric workload types
- `publicapis get-workload-apis` - Get workload-specific API specifications  
- `publicapis get-platform-apis` - Get platform-level API specifications

**Best Practices & Examples**:
- `bestpractices get-best-practices` - Get embedded best practice documentation
- `bestpractices get-examples` - Retrieve example API request/response files
- `itemdefinition get-workload-definition` - Get JSON schema definitions for workload items

### Dependencies

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- ModelContextProtocol package
- System.CommandLine package

### Known Limitations

- **Public Preview Status**: Implementation may change significantly before General Availability
- **API Specifications**: Embedded specifications are current as of release date and updated with each release

---

For support, contributions, and feedback, see [SUPPORT](https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/SUPPORT.md).