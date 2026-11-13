# Release History

## 1.8.0 (2026-11-11)

### Added

- Added `core_add-workspace-role-assignment` to grant an explicit principal and workspace role with one request, safe creation-outcome guidance, and no identity lookups or automatic retries. [[#4133](https://github.com/microsoft/mcp/pull/4133)]
- Added `core_create-folder` to create one logical workspace folder at the root or under an explicit parent UUID, with validated names, typed created metadata, safe creation-outcome guidance, and no lookups, retries, or polling. [[#4121](https://github.com/microsoft/mcp/pull/4121)]
- Added `core_delete-folder` to delete one explicitly identified empty logical workspace folder with a confirmed deletion receipt, normal mutation consent, and no content changes, preflight, automatic retries, or polling. [[#4126](https://github.com/microsoft/mcp/pull/4126)]
- Added `core_delete-workspace-role-assignment` to remove one explicitly identified workspace role assignment with a single request, standard mutation confirmation, and a deletion receipt without identity lookups or automatic retries. [[#4135](https://github.com/microsoft/mcp/pull/4135)]
- Added `core_get-folder` to read one logical workspace folder by explicit workspace and folder IDs, returning safe metadata and available parent information without lookups, retries, or polling. [[#4122](https://github.com/microsoft/mcp/pull/4122)]
- Added `core_get-workspace-role-assignment` to read one workspace role assignment and available principal metadata by explicit workspace and assignment IDs, without listing assignments, directory lookups, group expansion, or role changes. [[#4132](https://github.com/microsoft/mcp/pull/4132)]
- Added `core_list-folders` to read one complete page of logical workspace folders with optional root and recursive filters, parent relationships, and explicit continuation. [[#4113](https://github.com/microsoft/mcp/pull/4113)]
- Added `core_list-workspace-role-assignments` to read one complete page of workspace role assignments and available principal metadata, with explicit continuation and no directory lookups, group expansion, or role changes. [[#4128](https://github.com/microsoft/mcp/pull/4128)]
- Added `core_move-folder` to move one logical Fabric folder within its workspace using explicit IDs, with optional root placement and safe mutation outcomes. [[#4124](https://github.com/microsoft/mcp/pull/4124)]
- Added `core_move-item` to move an existing Fabric item and its child items within one workspace, returning the complete moved-item metadata collection without lookups, polling, or automatic retries. [[#4127](https://github.com/microsoft/mcp/pull/4127)]
- Added `core_update-folder` to rename one logical workspace folder by explicit workspace and folder IDs with a required display name, typed updated metadata, and safe mutation-outcome guidance. [[#4123](https://github.com/microsoft/mcp/pull/4123)]
- Added `core_update-workspace-role-assignment` to update one explicitly identified workspace role assignment with one PATCH, preserving requested access and truthful mutation outcomes without lookups, creation fallback, retries, or polling. [[#4134](https://github.com/microsoft/mcp/pull/4134)]
- Added the `jobs` namespace with `jobs_get-item-job-instance` to read one existing Fabric item job instance with status, timing, safe failure metadata, and available retry guidance, without polling or starting jobs. [[#4076](https://github.com/microsoft/mcp/pull/4076)]
- Added `jobs_cancel-item-job-instance` to request cancellation of one known Fabric item job, returning cancellation-request acceptance and retry guidance without polling or automatic retries. [[#4101](https://github.com/microsoft/mcp/pull/4101)]
- Added `jobs_create-item-schedule` to create one Fabric schedule with explicit enabled state, UTC timing, time zone, configuration, and optional static execution data, with consent and no automatic retries or polling. [[#4106](https://github.com/microsoft/mcp/pull/4106)]
- Added `jobs_delete-item-schedule` to delete one explicitly identified Fabric item schedule with consent, a request without a body, and a confirmed deletion receipt without retries or job cancellation. [[#4112](https://github.com/microsoft/mcp/pull/4112)]
- Added `jobs_get-item-schedule` to read one explicitly identified Fabric schedule with sensitive-data consent, extensible settings, identity validation, and safe retry guidance without discovery, polling, or mutations. [[#4111](https://github.com/microsoft/mcp/pull/4111)]
- Added `jobs_list-item-job-instances` to read one complete page of Fabric item job snapshots with safe failure metadata, explicit token continuation, and read-only mode support. [[#4102](https://github.com/microsoft/mcp/pull/4102)]
- Added `jobs_list-item-schedules` to read one complete schedule page for an explicit workspace, item, and job type, with extensible settings and explicit continuation. [[#4103](https://github.com/microsoft/mcp/pull/4103)]
- Added `jobs_run-on-demand-item-job` to submit one Fabric item job with explicit context and optional execution data and parameters, returning an accepted receipt and validated status-read guidance without polling or automatic retries. [[#4092](https://github.com/microsoft/mcp/pull/4092)]
- Added `jobs_update-item-schedule` to update one Fabric item schedule with explicit enabled state, full configuration, and optional static execution data in a single PATCH, with safe update confirmation and no automatic retry or job execution. [[#4110](https://github.com/microsoft/mcp/pull/4110)]

### Changed

- Updated bundled Fabric REST API specifications. [[#4136](https://github.com/microsoft/mcp/pull/4136)]

#### Dependency Updates

- Updated the .NET SDK to 10.0.403 and .NET base library packages to 10.0.14. [[#4096](https://github.com/microsoft/mcp/pull/4096)]

### Fixed

- Failed Data Factory pipeline and dataflow operation results now report command status `422` (Unprocessable Entity) instead of generic `500` (Internal Server Error). [[#3795](https://github.com/microsoft/mcp/pull/3795)]

## 1.7.0 (2026-11-01)

### Added

- Extended `core_create-item` with inline definitions or explicitly selected local definition files, existing folder IDs, item-specific creation payloads and definition options, and sensitivity label settings. Generic safety and request-contract checks run locally; Fabric validates item-specific layouts and content. Supplied label settings require a nonempty label ID. HTTP inline creation and bounded operation waiting remain supported. [[#4051](https://github.com/microsoft/mcp/pull/4051)]
- Added `core_get-item-definition` with typed inline results, bounded LRO waiting and one-GET resumption, and explicit local-only, no-overwrite definition export. [[#4064](https://github.com/microsoft/mcp/pull/4064)]
- Added `core_update-item-definition` to replace complete item definitions from inline content or explicitly selected saved files, with generic safety validation, unchanged definition formats and bytes, optional metadata application, and bounded no-result operation polling. [[#4065](https://github.com/microsoft/mcp/pull/4065)]

### Changed

- Updated bundled Fabric REST API specifications and examples, and added item-definition documentation for deployment plans, monitoring, and warehouses. [[#4078](https://github.com/microsoft/mcp/pull/4078)]

### Fixed

- Fixed concurrent first calls to namespace tools corrupting the tool-list cache and causing subsequent calls to fail until the server restarts. [[#4057](https://github.com/microsoft/mcp/pull/4057)]

## 1.6.0 (2026-10-18)

### Changed

- Updated bundled Fabric REST API specifications and examples, including CopyJob execution, Reflex activation queries, Warehouse SQL pool configuration, and item-definition links. [[#3978](https://github.com/microsoft/mcp/pull/3978)]

## 1.5.0 (2026-10-12)

### Added

- Added the `--disable-proxy-tools` server start option to disable tools loaded via `registry.json`. [[#3287](https://github.com/microsoft/mcp/pull/3287)]
- Added `core_assign-workspace-to-capacity` to submit workspace capacity assignments and return a 202 accepted/pending receipt without polling or automatic retries. [[#3887](https://github.com/microsoft/mcp/pull/3887)]
- Added `core_create-workspace` to create a Fabric workspace with optional existing capacity and domain assignment in one request. [[#3887](https://github.com/microsoft/mcp/pull/3887)]
- Added `core_delete-workspace` to delete one explicitly identified Fabric workspace and the items under it, with required UUID validation, destructive-operation metadata, and a typed deletion acknowledgement. [[#3887](https://github.com/microsoft/mcp/pull/3887)]
- Added the read-only `core_get-capacity` tool to retrieve a Fabric capacity by UUID, returning its display name, SKU, region, and state. [[#3846](https://github.com/microsoft/mcp/pull/3846)]
- Added the read-only `core_get-workspace` tool to retrieve workspace metadata by UUID, including optional capacity, domain, identity, tags, and workspace-specific endpoint information. [[#3887](https://github.com/microsoft/mcp/pull/3887)]
- Added `core_list-capacities` to return one page of accessible Fabric capacity metadata and continuation information. [[#3852](https://github.com/microsoft/mcp/pull/3852)]
- Added `core_list-items` to list one page of Fabric item metadata within a workspace or folder, with item-type filtering, recursive listing, and continuation information. [[#3887](https://github.com/microsoft/mcp/pull/3887)]
- Added `core_list-workspaces` to list one page of accessible Fabric workspace management metadata, with role filtering, continuation information, and optional workspace-specific API endpoints. [[#3887](https://github.com/microsoft/mcp/pull/3887)]
- Added `core_update-item` to update a Fabric item display name or description, preserve omitted properties, and clear descriptions with an empty string. [[#3887](https://github.com/microsoft/mcp/pull/3887)]
- Added `core_update-workspace` to rename Fabric workspaces and update or clear descriptions without overwriting omitted properties. [[#3887](https://github.com/microsoft/mcp/pull/3887)]
- Added the `core_delete-item` tool with explicit permanent-deletion opt-in, validated workspace and item UUIDs, and minimal deletion confirmation. [[#3887](https://github.com/microsoft/mcp/pull/3887)]
- Added `core_get-operation-state` and `core_get-operation-result` for single-read operation tracking and bounded JSON or empty results, plus optional asynchronous waiting for Create Item with configurable early polling, a 120-second default budget, and a server-enforced 600-second maximum. [[#3892](https://github.com/microsoft/mcp/pull/3892)]
- Added the read-only `core_get-item` tool to retrieve generic Fabric item metadata by workspace ID and item ID, with input validation and typed output. [[#3701](https://github.com/microsoft/mcp/pull/3701)]

### Changed

- **Breaking:** Normalized result payloads for the following tools: [[#3165](https://github.com/microsoft/mcp/pull/3165)]
  - `docs best-practices` now returns the resulting best practices in a `{ bestPractices }` object.
  - `docs item-definitions` now returns the resulting definition string in a `{ definition }` object.
  - `docs platform-api-spec` and `docs item-api-spec` now return the produced public API in a `{ publicApi }` object.
  - `onelake create-or-update-data-access-role` and `onelake get-data-access-role` now return the produced role in a `{ role }` object.
  - `onelake list-data-access-roles` now returns `{ roles }` instead of `{ value }`.
  - `onelake get settings` now returns the produced settings in a `{ settings }` object.
  - `onelake create shortcut` and `onelake get shortcut` now return the produced shortcut in a `{ shortcut }` object.
  - `onelake list shortcuts` now returns `{ shortcuts }` instead of `{ value }`.
- **Breaking:** Removed the unused `target-table-name` parameter from the `onelake create-shortcut-dataverse` tool. [[#3650](https://github.com/microsoft/mcp/pull/3650)]
- Updated Fabric REST API specifications and item definition documentation. [[#3914](https://github.com/microsoft/mcp/pull/3914)]

### Fixed

- `--mode single` no longer spawns child processes for tools managed in the repository. [[#3466](https://github.com/microsoft/mcp/pull/3466)]
- Fixed the VS Code extension namespace selector to offer `core`, `docs`, `onelake`, and `datafactory` instead of the obsolete `publicapis` option. [[#3916](https://github.com/microsoft/mcp/pull/3916)]
- Create Item now returns a resumable operation receipt for HTTP 202 responses instead of treating an accepted operation as a completed item. [[#3892](https://github.com/microsoft/mcp/pull/3892)]
- Preserved HTTP error statuses and sanitized public errors for Create Item and Catalog Search, including invalid or missing options, with actionable feature-availability and throttling guidance. [[#3887](https://github.com/microsoft/mcp/pull/3887)]
- Validated Create Item workspace UUIDs while retaining the workspace alias, and corrected Catalog Search continuation handling so continuation tokens are not combined with search or filter values. [[#3887](https://github.com/microsoft/mcp/pull/3887)]
- Disabled automatic HTTP redirects for Create Item and Catalog Search. [[#3887](https://github.com/microsoft/mcp/pull/3887)]
- The npm wrapper no longer writes its platform package into the calling project's `package.json` and lockfile when it auto-installs a missing platform package. The install now targets a private directory inside the wrapper package, or a per-user cache directory when that is not writable. Later runs reuse that install, and npm output no longer goes to stdout, which carries the MCP protocol stream during server startup. [[#3184](https://github.com/microsoft/mcp/pull/3184)]

## 1.4.0 (2026-08-27) (pre-release)

### Changed

- **Breaking:** Tool calls with unknown parameters are now explicitly rejected. [[#3282](https://github.com/microsoft/mcp/pull/3282)]
- **Breaking:** Renamed the `docs` toolset's `--workload-type` parameter to `--item-type`, aligning it with `core_create-item`'s existing `--item-type` and with official Fabric terminology. The accepted values are almost all Fabric item types (`notebook`, `lakehouse`, `dataPipeline`, `report`, ...); the description now also enumerates the four non-item API areas (`platform`, `admin`, `spark`, `realTimeIntelligence`). [[#3327](https://github.com/microsoft/mcp/pull/3327)]
- **Breaking:** Renamed the `docs_workloads` tool to `docs_list-item-types` and the `docs_workload-api-spec` tool to `docs_item-api-spec`. [[#3327](https://github.com/microsoft/mcp/pull/3327)]
- **Breaking:** The `docs_list-item-types` response property changed from `workloads` to `itemTypes`. [[#3327](https://github.com/microsoft/mcp/pull/3327)]
- **Breaking:** Renamed all `onelake` tools to use kebab-case, matching the `<namespace>_<tool-name>` convention already used by the `core`, `docs`, and `datafactory` namespaces. For example, `onelake_list_workspaces` is now `onelake_list-workspaces` and `onelake_create_shortcut_adls_gen2` is now `onelake_create-shortcut-adls-gen2`. The `onelake_confirm_delete` prompt is now `onelake_confirm-delete`. [[#3325](https://github.com/microsoft/mcp/pull/3325)]
- Updated Fabric REST API specifications and item definition documentation. [[#3397](https://github.com/microsoft/mcp/pull/3397)]

### Fixed

- Fixed `docs_item-api-spec` error messages referencing a non-existent `list_workloads` command; they now point to the `list-item-types` tool. [[#3327](https://github.com/microsoft/mcp/pull/3327)]

## 1.3.0 (2026-08-10) (pre-release)

### Changed

- **Breaking:** Removed unused parameters from Fabric tools. [[#3097](https://github.com/microsoft/mcp/pull/3097)]
- **Breaking:** Removed legacy tool design creation. [[#3137](https://github.com/microsoft/mcp/pull/3137)]
- Updated Fabric REST API specifications and item definition documentation. [[#3234](https://github.com/microsoft/mcp/pull/3234)]
- Updated `@microsoft/vscode-azext-utils` from ~2 to ~4 (4.1.1) and `@vscode/vsce` from 3.7.1 to 3.9.2. [[#3001](https://github.com/microsoft/mcp/pull/3001)]

## 1.2.0 (2026-07-08)

### Added

- Added 12 new OneLake tools: Data Access Security (list, get, create-or-update, delete roles), Shortcuts (list, get, create-or-update, delete, reset-cache), and Settings (get, modify-diagnostics, modify-immutability-policy) [[#2625](https://github.com/microsoft/mcp/pull/2625)]
- Added 9 per-target shortcut creation tools (OneLake, ADLS Gen2, Amazon S3, Azure Blob, GCS, S3-compatible, Dataverse, OneDrive/SharePoint, External Data Share) with flat typed options for better LLM ergonomics. [[#2625](https://github.com/microsoft/mcp/pull/2625)]
- Flattened JSON-string options into discrete typed parameters for diagnostics, immutability policy, and data access role commands. [[#2625](https://github.com/microsoft/mcp/pull/2625)]
- Added the `core_search-catalog` tool to search the Microsoft Fabric OneLake catalog for items across workspaces by display name, description, or workspace name, with optional filtering by item type. Calls the Catalog Search API (POST /v1/catalog/search). [[#2963](https://github.com/microsoft/mcp/pull/2963)]

### Changed

- Updated Fabric REST API specifications and item definition documentation. [[#3006](https://github.com/microsoft/mcp/pull/3006)]

### Fixed

- Refactored OneLake DFS ListPath methods to follow ADLS Gen2 Path List API specification (directory as query parameter) [[#2625](https://github.com/microsoft/mcp/pull/2625)]
- Fixed OneLake diagnostics and immutability settings models to match the Fabric REST API contract. [[#2625](https://github.com/microsoft/mcp/pull/2625)]

## 1.1.0 (2026-06-15)

### Added

- **Data Factory Tools**: Added 7 new commands for managing Microsoft Fabric Data Factory resources through MCP. Includes pipeline operations (list, create, get, run) and Dataflow Gen2 operations (list, create, execute query). Powered by the [Microsoft.DataFactory.MCP.Core](https://www.nuget.org/packages/Microsoft.DataFactory.MCP.Core) NuGet package.

### Changed

- Add better handling for MsalClientException and MsalServiceException. [[#2587](https://github.com/microsoft/mcp/pull/2587)]
- Updated Fabric REST API specifications and item definition documentation. [[#2797](https://github.com/microsoft/mcp/pull/2797)]

### Fixed

- Fixed console logging polluting stdout which caused smoke test failures on macOS. Console logs are now redirected to stderr via `LogToStandardErrorThreshold`.

## 1.0.0 (2026-04-14)

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
