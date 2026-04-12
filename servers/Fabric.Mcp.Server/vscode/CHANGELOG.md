# Release History

## 1.0.0 (2026-04-12)

**First Stable Release**

We're excited to announce the first stable release of the Microsoft Fabric MCP Server VS Code extension! The Fabric MCP Server is now generally available, providing AI agents with comprehensive context about Microsoft Fabric through the Model Context Protocol (MCP).

### Added

- Add `--disable-caching` to server start options to disable caching. [[#2330](https://github.com/microsoft/mcp/pull/2330)]

### Changed

- Updated `ModelContextProtocol` and `ModelContextProtocol.AspNetCore` dependencies to version 1.1.0. [[#1963](https://github.com/microsoft/mcp/pull/1963)]

### Fixed

- Updated `HttpRequestException` handling to return more specific HTTP status codes for better troubleshooting. [[#2172](https://github.com/microsoft/mcp/pull/2172)]

## 0.0.0-beta.10 (2026-03-24) (pre-release)

### Changed

- **Breaking:** Changed Fabric tool names to use dash instead of underscore. `create-item`, `api-examples`, `best-practices`, `item-definitions`, `platform-api-spec`, and `workload-api-spec` have dashes now.
- Reintroduced capturing error information in telemetry with standard `exception.message`, `exception.type`, and `exception.stacktrace` telemetry tags, replacing `ErrorDetails` tag.
- Updated Fabric REST API specifications and examples. Updated item definition documentation.

### Fixed

- Added filtering on `LocalRequired` when running in remote mode.
- Fixed directory traversal vulnerability in OneLake file operations. Paths containing `..` sequences are now rejected before any HTTP request is made.

## 0.0.0-beta.9 (2026-03-03) (pre-release)

### Added

- Added OneLake table API commands for configuration, namespace management, and table metadata retrieval:
  - `onelake table config get`
  - `onelake table namespace list`
  - `onelake table namespace get`
  - `onelake table list`
  - `onelake table get`

## 0.0.0-beta.8 (2026-02-10) (pre-release)

_No user-facing changes._

## 0.0.0-beta.7 (2026-02-09) (pre-release)

### Changed

- Updated Fabric REST API specifications and examples.
- Updated item definition documentation.

## 0.0.0-beta.6 (2026-01-22) (pre-release)

### Changed

- Updated Microsoft Fabric REST API specifications with new connection credential features: KeyPair credential type with identifier/private key support, Key Vault secret references for Basic/Key/ServicePrincipal/SharedAccessSignature credentials, SQL endpoint `recreateTables` option for metadata refresh, updated connection examples, and corrected rate limiting documentation for tags APIs.

## 0.0.0-beta.5 (2026-01-05) (pre-release)

### Added

- Added comprehensive API throttling best practices guide with production-ready retry patterns, exponential backoff, circuit breakers, and code examples in C#, Python, and TypeScript.
- Added Admin APIs usage guidelines to help LLMs understand when to use admin APIs, request explicit user permission, and implement graceful fallbacks to standard APIs.

### Changed

- Updated Fabric REST API specifications and examples.

## 0.0.0-beta.4 (2025-12-16) (pre-release)

### Added

- **OneLake Toolset**: Added comprehensive support for OneLake operations including file read/write/delete/list, directory create/delete, item create/list, workspace listing, and multi-environment support. [[#1113](https://github.com/microsoft/mcp/pull/1113)]
- **Public APIs Toolset**: Added API specifications for Cosmos DB Database, Operations Agent, Graph Model, and Snowflake Database. Updated API specifications for multiple items.

## 0.0.0-beta.3 (2025-12-04) (pre-release)

### Added

- Added Docker image release for Fabric MCP Server. [[#1241](https://github.com/microsoft/mcp/pull/1241)]
- Added new item definitions for Lakehouse, Ontology, and Snowflake Database workloads. [[#1240](https://github.com/microsoft/mcp/pull/1240)]
- Enhanced README documentation for released packages.

### Fixed

- Fixed UI for server help messages and display to show Fabric.Mcp.Server. [[#1269](https://github.com/microsoft/mcp/pull/1269)]

## 0.0.0-beta.2 (2025-11-21) (pre-release)

### Added

Initial release of the Microsoft Fabric MCP Server in **Public Preview**.

- **Complete API Context**: Full OpenAPI specifications for all supported Fabric workloads.
- **Item Definition Knowledge**: JSON schemas for every Fabric item type including Lakehouse, Warehouse, KQL Database, Eventhouse, Data Pipeline, Dataflow, Notebook, Report, Semantic Model, and many more.
- **Built-in Best Practices**: Embedded guidance for pagination, long-running operations, error handling, and authentication.
- **Local-First Security**: Runs entirely on your machine without connecting to live Fabric environments.
- **Platform APIs**: Core platform operations for workspace management and common resources.
- **Example-Driven Development**: Real API request/response examples for every workload.

**Public API Tools**:
- `publicapis bestpractices examples get` - Retrieve example API request/response files.
- `publicapis bestpractices get` - Get embedded best practice documentation.
- `publicapis bestpractices itemdefinition get` - Get JSON schema definitions for workload items.
- `publicapis get` - Get workload-specific API specifications.
- `publicapis list` - List all available Fabric workload types.
- `publicapis platform get` - Get platform-level API specifications.
