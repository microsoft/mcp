# Changelog

All notable changes to the Microsoft Fabric MCP Server will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 0.0.0-beta.6 (Unreleased)

### Features Added

### Breaking Changes

### Bugs Fixed

### Other Changes

## 0.0.0-beta.5 (2026-01-05)

### Features Added

- Added comprehensive API throttling best practices guide with production-ready retry patterns, exponential backoff, circuit breakers, and code examples in C#, Python, and TypeScript
- Added Admin APIs usage guidelines to help LLMs understand when to use admin APIs, request explicit user permission, and implement graceful fallbacks to standard APIs
- Update Fabric REST API specifications and examples.

### Breaking Changes

### Bugs Fixed

### Other Changes

## 0.0.0-beta.4 (2025-12-16)

### Features Added

- **OneLake Toolset**: Added comprehensive support for OneLake operations including:
    - File operations: Read, write, delete, and list files in OneLake.
    - Directory operations: Create and delete directories.
    - Item management: Create items and list OneLake items.
    - Workspace integration: List OneLake workspaces.
    - Multi-environment support and extensive documentation. [[#1113](https://github.com/microsoft/mcp/pull/1113)]
- **Public APIs Toolset**: Updated Fabric public APIs with latest information:
    - Added API specifications for **Cosmos DB Database**, **Operations Agent**, **Graph Model**, and **Snowflake Database**.
    - Updated API specifications for multiple items.


## 0.0.0-beta.3 (2025-12-04)

### Features Added

- Added Docker image release for Fabric MCP Server. [[#1241](https://github.com/microsoft/mcp/pull/1241)]
- Added new item definitions for Lakehouse, Ontology, and Snowflake Database workloads. [[#1240](https://github.com/microsoft/mcp/pull/1240)]
- Enhanced README documentation for released packages.

### Bugs Fixed

- Fixed UI for server help messages and display to show Fabric.Mcp.Server. [[#1269](https://github.com/microsoft/mcp/pull/1269)]


## [0.0.0-beta.2] (2025-11-21)

### Features Added

Initial release of the Microsoft Fabric MCP Server in **Public Preview**.

- **Complete API Context**: Full OpenAPI specifications for all supported Fabric workloads
- **Item Definition Knowledge**: JSON schemas for every Fabric item type including Lakehouse, Warehouse, KQL Database, Eventhouse, Data Pipeline, Dataflow, Copy Job, Apache Airflow Job, Notebook, Report, Semantic Model, KQL Queryset, Eventstream, Reflex, GraphQL API, Environment, and many more specialized workloads
- **Built-in Best Practices**: Embedded guidance for pagination patterns, long-running operation handling, error handling and retry logic, authentication and security best practices
- **Local-First Security**: Runs entirely on your machine without connecting to live Fabric environments
- **Platform APIs**: Core platform operations for workspace management and common resources
- **Example-Driven Development**: Real API request/response examples for every workload

#### Tool Categories Added

**Public API Operations**:
- `publicapis bestpractices examples get` - Retrieve example API request/response files
- `publicapis bestpractices get` - Get embedded best practice documentation
- `publicapis bestpractices itemdefinition get` - Get JSON schema definitions for workload items
- `publicapis get` - Get workload-specific API specifications  
- `publicapis list` - List all available Fabric workload types
- `publicapis platform get` - Get platform-level API specifications


### Known Limitations

- **Public Preview Status**: Implementation may change significantly before General Availability
- **API Specifications**: Embedded specifications are current as of release date and updated with each release

---

For support, contributions, and feedback, see [SUPPORT](https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/SUPPORT.md).
