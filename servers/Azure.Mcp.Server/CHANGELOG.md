# CHANGELOG 📝

The Azure MCP Server updates automatically by default whenever a new release comes out 🚀. We ship updates twice a week on Tuesdays and Thursdays 😊

## 0.7.1 (Unreleased)

### Features Added

### Breaking Changes

### Bugs Fixed

### Other Changes

## 0.7.0 (2025-09-16)

### Features Added

- Added support for getting a node pool in an AKS managed cluster via the command `azmcp_aks_nodepool_get`. [[#394](https://github.com/microsoft/mcp/pull/394)]
- Added support for diagnosing Azure Resources using the App Lens API via the command `azmcp_applens_resource_diagnose`. [[#356](https://github.com/microsoft/mcp/pull/356)]
- Added elicitation support. An elicitation request is sent if the tool annotation `secret` hint is true. [[#404](https://github.com/microsoft/mcp/pull/404)]
- Added `azmcp_sql_server_create`, `azmcp_sql_server_delete`, `azmcp_sql_server_show` to support SQL server create, delete, and show commands. [[#312](https://github.com/microsoft/mcp/pull/312)]
- Added the support for getting information about Azure Managed Lustre SKUs via the following command `azmcp_azuremanagedlustre_filesystem_get_sku_info`. [[#100](https://github.com/microsoft/mcp/issues/100)]
- `azmcp_functionapp_get` can now list Function Apps on a resource group level. [[#427](https://github.com/microsoft/mcp/pull/427)]

### Breaking Changes

- Merged `azmcp_functionapp_list` into `azmcp_functionapp_get`, which can perform both operations based on whether `--function-app` is passed. [[#427](https://github.com/microsoft/mcp/pull/427)]
- Removed Azure CLI (`az`) and Azure Developer CLI (`azd`) extension tools to reduce complexity and focus on native Azure service operations. [[#404](https://github.com/microsoft/mcp/pull/404)].

### Bugs Fixed

- Marked the `secret` hint of `azmcp_keyvault_secret_create` tool to "true". [[#430](https://github.com/microsoft/mcp/pull/430)]

### Other Changes

## 0.6.0 (2025-09-11)

### Features Added

- **The Azure MCP Server is now also available on NuGet.org** [[#368](https://github.com/microsoft/mcp/pull/368)]
- Added support for listing node pools in an AKS managed cluster via the command `azmcp_aks_nodepool_list`. [[#360](https://github.com/microsoft/mcp/pull/360)]

### Breaking Changes

- To improve performance, packages now ship with trimmed binaries that have unused code and dependencies removed, resulting in significantly smaller file sizes, faster startup times, and reduced memory footprint. [Learn more](https://learn.microsoft.com/dotnet/core/deploying/trimming/trim-self-contained). [[#405](https://github.com/microsoft/mcp/pull/405)]
- Merged `azmcp_search_index_describe` and `azmcp_search_index_list` into `azmcp_search_index_get`, which can perform both operations based on whether `--index` is passed. [[#378](https://github.com/microsoft/mcp/pull/378)]
- Merged the following Storage tools: [[#376](https://github.com/microsoft/mcp/pull/376)]
  - `azmcp_storage_account_details` and `azmcp_storage_account_list` into `azmcp_storage_account_get`, which supports the behaviors of both tools based on whether `--account` is passed.
  - `azmcp_storage_blob_details` and `azmcp_storage_blob_list` into `azmcp_storage_blob_get`, which supports the behaviors of both tools based on whether `--blob` is passed.
  - `azmcp_storage_blob_container_details` and `azmcp_storage_blob_container_list` into `azmcp_storage_blob_container_get`, which supports the behaviors of both tools based on whether `--container` is passed.
- Updated the descriptions of all Storage tools. [[#376](https://github.com/microsoft/mcp/pull/376)]

### Other Changes

#### Dependency updates

- Updated the following dependencies: [[#380](https://github.com/microsoft/mcp/pull/380)]
  - Azure.Core: `1.47.1` → `1.48.0`
  - Azure.Identity: `1.15.0` → `1.16.0`

## 0.5.13 (2025-09-10)

### Features Added

- Added support for listing all Event Grid topics in a subscription via the command `azmcp_eventgrid_topic_list`. [[#43](https://github.com/microsoft/mcp/pull/43)]
- Added support for retrieving knowledge index schema information in Azure AI Foundry projects via the command `azmcp_foundry_knowledge_index_schema`. [[#41](https://github.com/microsoft/mcp/pull/41)]
- Added support for listing service health events in a subscription via the command `azmcp_resourcehealth_service-health-events_list`. [[#367](https://github.com/microsoft/mcp/pull/367)]

### Breaking Changes

- Updated/removed options for the following commands: [[#108](https://github.com/microsoft/mcp/pull/108)]
  - `azmcp_storage_account_create`: Removed the ability to configure `enable-https-traffic-only` (always `true` now), `allow-blob-public-access` (always `false` now), and `kind` (always `StorageV2` now).
  - `azmcp_storage_blob_container_create`: Removed the ability to configure `blob-container-public-access` (always `false` now).
  - `azmcp_storage_blob_upload`: Removed the ability to configure `overwrite` (always `false` now).

### Bugs Fixed

- Fixed telemetry bug where "ToolArea" was incorrectly populated in with "ToolName". [[#346](https://github.com/microsoft/mcp/pull/346)]

### Other Changes

- Added telemetry to log parameter values for the `azmcp_bestpractices_get` tool. [[#375](https://github.com/microsoft/mcp/pull/375)]
- Updated tool annotations. [[#377](https://github.com/microsoft/mcp/pull/377)]

#### Dependency updates

- Updated the following dependencies:
  - Azure.Identity: `1.14.0` → `1.15.0` [[#352](https://github.com/microsoft/mcp/pull/352)]
  - Azure.Identity.Broker: `1.2.0` → `1.3.0` [[#352](https://github.com/microsoft/mcp/pull/352)]
  - Microsoft.Azure.Cosmos.Aot: `0.1.1-preview.1` → `0.1.2-preview.1` [[#383](https://github.com/microsoft/mcp/pull/383)]
- Updated the following dependency to improve .NET Ahead-of-Time (AOT) compilation support: [[#363](https://github.com/microsoft/mcp/pull/363)]
  - Azure.ResourceManager.StorageCache: `1.3.1` → `1.3.2`

## 0.5.12 (2025-09-04)

### Features Added

- Added `azmcp_sql_server_firewall-rule_create` and `azmcp_sql_server_firewall-rule_delete` commands. [[#121](https://github.com/microsoft/mcp/pull/121)]

### Bugs Fixed

- Fixed a bug in MySQL query validation logic. [[#81](https://github.com/microsoft/mcp/pull/81)]

### Other Changes

AOT- Added a verb to the namespace name for bestpractices [[#109](https://github.com/microsoft/mcp/pull/109)]
- Added instructions about consumption plan for azure functions deployment best practices [[#218](https://github.com/microsoft/mcp/pull/218)]

## 0.5.11 (2025-09-02)

### Other Changes

- Fixed VSIX signing [[#91](https://github.com/microsoft/mcp/pull/91)]
- Included native packages in build artifacts and pack/release scripts. [[#51](https://github.com/microsoft/mcp/pull/51)]

## 0.5.10 (2025-08-28)

### Bugs fixed

- Fixed a bug with telemetry collection related to AppConfig tools. [[#44](https://github.com/microsoft/mcp/pull/44)]

## 0.5.9 (2025-08-26)

### Other Changes

#### Dependency Updates

- Updated the following dependencies to improve .NET Ahead-of-Time (AOT) compilation support:
  - Microsoft.Azure.Cosmos `3.51.0` → Microsoft.Azure.Cosmos.Aot `0.1.1-preview.1`. [[#37](https://github.com/microsoft/mcp/pull/37)]

## 0.5.8 (2025-08-21)

### Features Added

- Added support for listing knowledge indexes in Azure AI Foundry projects via the command `azmcp_foundry_knowledge_index_list`. [[#1004](https://github.com/Azure/azure-mcp/pull/1004)]
- Added support for getting details of an Azure Function App via the command `azmcp_functionapp_get`. [[#970](https://github.com/Azure/azure-mcp/pull/970)]
- Added the following Azure Managed Lustre commands: [[#1003](https://github.com/Azure/azure-mcp/issues/1003)]
  - `azmcp_azuremanagedlustre_filesystem_list`: List available Azure Managed Lustre filesystems.
  - `azmcp_azuremanagedlustre_filesystem_required-subnet-size`: Returns the number of IP addresses required for a specific SKU and size of Azure Managed Lustre filesystem.
- Added support for designing Azure Cloud Architecture through guided questions via the command `azmcp_cloudarchitect_design`. [[#890](https://github.com/Azure/azure-mcp/pull/890)]
- Added support for the following Azure MySQL operations: [[#855](https://github.com/Azure/azure-mcp/issues/855)]
  - `azmcp_mysql_database_list` - List all databases in a MySQL server.
  - `azmcp_mysql_database_query` - Executes a SELECT query on a MySQL Database. The query must start with SELECT and cannot contain any destructive SQL operations for security reasons.
  - `azmcp_mysql_table_list` - List all tables in a MySQL database.
  - `azmcp_mysql_table_schema_get` - Get the schema of a specific table in a MySQL database.
  - `azmcp_mysql_server_config_get` - Retrieve the configuration of a MySQL server.
  - `azmcp_mysql_server_list` - List all MySQL servers in a subscription & resource group.
  - `azmcp_mysql_server_param_get` - Retrieve a specific parameter of a MySQL server.
  - `azmcp_mysql_server_param_set` - Set a specific parameter of a MySQL server to a specific value.
- Added telemetry for tracking service area when calling tools. [[#1024](https://github.com/Azure/azure-mcp/pull/1024)]

### Breaking Changes

- Renamed the following Storage tool option names: [[#1015](https://github.com/Azure/azure-mcp/pull/1015)]
  - `azmcp_storage_account_create`: `account-name` → `account`.
  - `azmcp_storage_blob_batch_set-tier`: `blob-names` → `blobs`.

### Bugs Fixed

- Fixed SQL service test assertions to use case-insensitive string comparisons for resource type validation. [[#938](https://github.com/Azure/azure-mcp/pull/938)]
- Fixed HttpClient service test assertions to properly validate NoProxy collection handling instead of expecting a single string value. [[#938](https://github.com/Azure/azure-mcp/pull/938)]

### Other Changes

- Introduced the `BaseAzureResourceService` class to allow performing Azure Resource read operations using Azure Resource Graph queries. [[#938](https://github.com/Azure/azure-mcp/pull/938)]
- Refactored SQL service implementation to use Azure Resource Graph queries instead of direct ARM API calls. [[#938](https://github.com/Azure/azure-mcp/pull/938)]
  - Removed dependency on `Azure.ResourceManager.Sql` package by migrating to Azure Resource Graph queries, reducing package size and improving startup performance.
- Enhanced `BaseAzureService` with `EscapeKqlString` method for safe KQL query construction across all Azure services. [[#938](https://github.com/Azure/azure-mcp/pull/938)]
  - Fixed KQL string escaping in Workbooks service queries.
- Standardized Azure Storage command descriptions, option names, and parameter names for consistency across all storage commands. Updated JSON serialization context to remove unused model types and improve organization. [[#1015](https://github.com/Azure/azure-mcp/pull/1015)]
- Updated to .NET 10 SDK to prepare for .NET tool packing. [[#1023](https://github.com/Azure/azure-mcp/pull/1023)]
- Enhanced `bestpractices` and `azureterraformbestpractices` tool descriptions to better work with the vscode copilot tool grouping feature. [[#1029](https://github.com/Azure/azure-mcp/pull/1029)]
- The Azure MCP Server can now be packaged as a .NET SDK Tool for easier use by users with the .NET 10 SDK installed. [[#422](https://github.com/Azure/azure-mcp/issues/422)]

#### Dependency Updates

- Updated the following dependencies to improve .NET Ahead-of-Time (AOT) compilation support: [[#1031](https://github.com/Azure/azure-mcp/pull/1031)]
  - Azure.ResourceManager.ResourceHealth: `1.0.0` → `1.1.0-beta.5`

## 0.5.7 (2025-08-19)

### Features Added
- Added support for the following Azure Deploy and Azure Quota operations: [[#626](https://github.com/Azure/azure-mcp/pull/626)]
  - `azmcp_deploy_app_logs_get` - Get logs from Azure applications deployed using azd.
  - `azmcp_deploy_iac_rules_get` - Get Infrastructure as Code rules.
  - `azmcp_deploy_pipeline_guidance-get` - Get guidance for creating CI/CD pipelines to provision Azure resources and deploy applications.
  - `azmcp_deploy_plan_get` - Generate deployment plans to construct infrastructure and deploy applications on Azure.
  - `azmcp_deploy_architecture_diagram-generate` - Generate Azure service architecture diagrams based on application topology.
  - `azmcp_quota_region_availability-list` - List available Azure regions for specific resource types.
  - `azmcp_quota_usage_check` - Check Azure resource usage and quota information for specific resource types and regions.
- Added support for listing Azure Function Apps via the command `azmcp-functionapp-list`. [[#863](https://github.com/Azure/azure-mcp/pull/863)]
- Added support for importing existing certificates into Azure Key Vault via the command `azmcp-keyvault-certificate-import`. [[#968](https://github.com/Azure/azure-mcp/issues/968)]
- Added support for uploading a local file to an Azure Storage blob via the command `azmcp-storage-blob-upload`. [[#960](https://github.com/Azure/azure-mcp/pull/960)]
- Added support for the following Azure Service Health operations: [[#998](https://github.com/Azure/azure-mcp/pull/998)]
  - `azmcp-resourcehealth-availability-status-get` - Get the availability status for a specific resource.
  - `azmcp-resourcehealth-availability-status-list` - List availability statuses for all resources in a subscription or resource group.
- Added support for listing repositories in Azure Container Registries via the command `azmcp-acr-registry-repository-list`. [[#983](https://github.com/Azure/azure-mcp/pull/983)]

### Other Changes

- Improved guidance for LLM interactions with Azure MCP server by adding rules around bestpractices tool calling to server instructions. [[#1007](https://github.com/Azure/azure-mcp/pull/1007)]

#### Dependency Updates

- Updated the following dependencies to improve .NET Ahead-of-Time (AOT) compilation support: [[#893](https://github.com/Azure/azure-mcp/pull/893)]
  - Azure.Bicep.Types: `0.5.110` → `0.6.1`
  - Azure.Bicep.Types.Az: `0.2.771` → `0.2.792`
- Added the following dependencies to support Azure Managed Lustre
  - Azure.ResourceManager.StorageCache:  `1.3.1`

## 0.5.6 (2025-08-14)

### Features Added

- Added support for listing Azure Function Apps via the command `azmcp-functionapp-list`. [[#863](https://github.com/Azure/azure-mcp/pull/863)]
- Added support for getting details about an Azure Storage Account via the command `azmcp-storage-account-details`. [[#934](https://github.com/Azure/azure-mcp/issues/934)]

### Other Changes

- Refactored resource group option (`--resource-group`) handling and validation for all commands to a centralized location. [[#961](https://github.com/Azure/azure-mcp/issues/961)]

#### Dependency Updates

- Updated the following dependencies to improve .NET Ahead-of-Time (AOT) compilation support: [[#967](https://github.com/Azure/azure-mcp/issues/967)] [[#969](https://github.com/Azure/azure-mcp/issues/969)]
  - Azure.Monitor.Query: `1.6.0` → `1.7.1`
  - Azure.Monitor.Ingestion: `1.1.2` → `1.2.0`
  - Azure.Search.Documents: `11.7.0-beta.4` → `11.7.0-beta.6`
  - Azure.ResourceManager.ContainerRegistry: `1.3.0` → `1.3.1`
  - Azure.ResourceManager.DesktopVirtualization: `1.3.1` → `1.3.2`
  - Azure.ResourceManager.PostgreSql: `1.3.0` → `1.3.1`

## 0.5.5 (2025-08-12)

### Features Added

- Added support for listing ACR (Azure Container Registry) registries in a subscription via the command `azmcp-acr-registry-list`. [[#915](https://github.com/Azure/azure-mcp/issues/915)]
- Added the following Azure Storage commands:
  - `azmcp-storage-account-create`: Create a new Azure Storage account. [[#927](https://github.com/Azure/azure-mcp/issues/927)]
  - `azmcp-storage-queue-message-send`: Send a message to an Azure Storage queue. [[#794](https://github.com/Azure/azure-mcp/pull/794)]
  - `azmcp-storage-blob-details`: Get details about an Azure Storage blob. [[#930](https://github.com/Azure/azure-mcp/issues/930)]
  - `azmcp-storage-blob-container-create`: Create a new Azure Storage blob container. [[#937](https://github.com/Azure/azure-mcp/issues/937)]

### Breaking Changes

- The `azmcp-storage-account-list` command now returns account metadata objects instead of plain strings. Each item includes: `name`, `location`, `kind`, `skuName`, `skuTier`, `hnsEnabled`, `allowBlobPublicAccess`, `enableHttpsTrafficOnly`. Update scripts to read the `name` property. The underlying `IStorageService.GetStorageAccounts()` signature changed from `Task<List<string>>` to `Task<List<StorageAccountInfo>>`. [[#904](https://github.com/Azure/azure-mcp/issues/904)]

### Bugs Fixed

- Fixed best practices tool invocation failure when passing "all" action with "general" or "azurefunctions" resources. [[#757](https://github.com/Azure/azure-mcp/issues/757)]
- Updated metadata for CREATE and SET tools to `destructive = true`. [[#773](https://github.com/Azure/azure-mcp/pull/773)]

### Other Changes
- Consolidate "AzSubscriptionGuid" telemetry logic into `McpRuntime`. [[#935](https://github.com/Azure/azure-mcp/pull/935)]

## 0.5.4 (2025-08-07)

### Bugs Fixed

- Fixed subscription parameter handling across all Azure MCP service methods to consistently use `subscription` instead of `subscriptionId`, enabling proper support for both subscription IDs and subscription names. [[#877](https://github.com/Azure/azure-mcp/issues/877)]
- Fixed `ToolExecuted` telemetry activity being created twice. [[#741](https://github.com/Azure/azure-mcp/pull/741)]

### Other Changes

- Improved Azure MCP display name in VS Code from 'azure-mcp-server-ext' to 'Azure MCP' for better user experience in the Configure Tools interface. [[#871](https://github.com/Azure/azure-mcp/issues/871), [#876](https://github.com/Azure/azure-mcp/pull/876)]
- Updated the  following `CommandGroup` descriptions to improve their tool usage by Agents:
  - Azure AI Search [[#874](https://github.com/Azure/azure-mcp/pull/874)]
  - Storage [[#879](https://github.com/Azure/azure-mcp/pull/879)]

## 0.5.3 (2025-08-05)

### Features Added

- Added support for providing the `--content-type` and `--tags` properties to the `azmcp-appconfig-kv-set` command. [[#459](https://github.com/Azure/azure-mcp/pull/459)]
- Added `filter-path` and `recursive` capabilities to `azmcp-storage-datalake-file-system-list-paths`. [[#770](https://github.com/Azure/azure-mcp/issues/770)]
- Added support for listing files and directories in Azure File Shares via the `azmcp-storage-share-file-list` command. This command recursively lists all items in a specified file share directory with metadata including size, last modified date, and content type. [[#793](https://github.com/Azure/azure-mcp/pull/793)]
- Added support for Azure Virtual Desktop with new commands: [[#653](https://github.com/Azure/azure-mcp/pull/653)]
  - `azmcp-virtualdesktop-hostpool-list` - List all host pools in a subscription
  - `azmcp-virtualdesktop-sessionhost-list` - List all session hosts in a host pool
  - `azmcp-virtualdesktop-sessionhost-usersession-list` - List all user sessions on a specific session host
- Added support for creating and publishing DevDeviceId in telemetry. [[#810](https://github.com/Azure/azure-mcp/pull/810/)]

### Breaking Changes

- **Parameter Name Changes**: Removed unnecessary "-name" suffixes from command parameters across 25+ parameters in 12+ Azure service areas to improve consistency and usability. Users will need to update their command-line usage and scripts. [[#853](https://github.com/Azure/azure-mcp/pull/853)]
  - **AppConfig**: `--account-name` → `--account`
  - **Search**: `--service-name` → `--service`, `--index-name` → `--index`
  - **Cosmos**: `--account-name` → `--account`, `--database-name` → `--database`, `--container-name` → `--container`
  - **Kusto**: `--cluster-name` → `--cluster`, `--database-name` → `--database`, `--table-name` → `--table`
  - **AKS**: `--cluster-name` → `--cluster`
  - **Postgres**: `--user-name` → `--user`
  - **ServiceBus**: `--queue-name` → `--queue`, `--topic-name` → `--topic`
  - **Storage**: `--account-name` → `--account`, `--container-name` → `--container`, `--table-name` → `--table`, `--file-system-name` → `--file-system`, `--tier-name` → `--tier`
  - **Monitor**: `--table-name` → `--table`, `--model` → `--health-model`, `--resource-name` → `--resource`
  - **Foundry**: `--deployment-name` → `--deployment`, `--publisher-name` → `--publisher`, `--license-name` → `--license`, `--sku-name` → `--sku`, `--azure-ai-services-name` → `--azure-ai-services`

### Bugs Fixed

- Fixed an issue where the `azmcp-storage-blob-batch-set-tier` command did not correctly handle the `--tier` parameter when setting the access tier for multiple blobs. [[#808](https://github.com/Azure/azure-mcp/pull/808)]

### Other Changes

- Implemented centralized HttpClient service with proxy support for better resource management and enterprise compatibility. [[#857](https://github.com/Azure/azure-mcp/pull/857)]
- Added caching for Cosmos DB databases and containers. [[#813](https://github.com/Azure/azure-mcp/pull/813)]
- Refactored PostgreSQL commands to follow ObjectVerb naming pattern, fix command hierarchy, and ensure all commands end with verbs. This improves consistency and discoverability across all postgres commands. [[#865](https://github.com/Azure/azure-mcp/issues/865)] [[#866](https://github.com/Azure/azure-mcp/pull/866)]

#### Dependency Updates

- Updated the following dependencies to improve .NET Ahead-of-Time (AOT) compilation support. AOT will enable shipping Azure MCP Server as self-contained native executable.
  - Azure.Core: `1.46.2` → `1.47.1`
  - Azure.ResourceManager: `1.13.1` → `1.13.2`
  - Azure.ResourceManager.ApplicationInsights: `1.0.1` → `1.1.0-beta.1`
  - Azure.ResourceManager.AppConfiguration: `1.4.0` → `1.4.1`
  - Azure.ResourceManager.Authorization: `1.1.4` → `1.1.5`
  - Azure.ResourceManager.ContainerService: `1.2.3` → `1.2.5`
  - Azure.ResourceManager.Kusto: `1.6.0` → `1.6.1`
  - Azure.ResourceManager.CognitiveServices: `1.4.0` → `1.5.1`
  - Azure.ResourceManager.Redis: `1.5.0` → `1.5.1`
  - Azure.ResourceManager.RedisEnterprise: `1.1.0` → `1.2.1`
  - Azure.ResourceManager.LoadTesting: `1.1.1` → `1.1.2`
  - Azure.ResourceManager.Sql: `1.3.0` → `1.4.0-beta.3`
  - Azure.ResourceManager.Datadog: `1.0.0-beta.5` → `1.0.0-beta.6`
  - Azure.ResourceManager.CosmosDB: `1.3.2` → `1.4.0-beta.13`
  - Azure.ResourceManager.OperationalInsights: `1.3.0` → `1.3.1`
  - Azure.ResourceManager.Search: `1.2.3` → `1.3.0`
  - Azure.ResourceManager.Storage: `1.4.2` → `1.4.4`
  - Azure.ResourceManager.Grafana: `1.1.1` → `1.2.0-beta.2`
  - Azure.ResourceManager.ResourceGraph: `1.1.0-beta.3` → `1.1.0-beta.4`

## 0.5.2 (2025-07-31)

### Features Added

- Added support for batch setting access tier for multiple Azure Storage blobs via the `azmcp-storage-blob-batch-set-tier` command. This command efficiently changes the storage tier (Hot, Cool, Archive, etc) for multiple blobs simultaneously in a single operation. [[#735](https://github.com/Azure/azure-mcp/issues/735)]
- Added descriptions to all Azure MCP command groups to improve discoverability and usability when running the server with `--mode single` or `--mode namespace`. [[#791](https://github.com/Azure/azure-mcp/pull/791)]

### Breaking Changes

- Removed `--partner-tenant-id` option from `azmcp-marketplace-product-get` command. [[#656](https://github.com/Azure/azure-mcp/pull/656)]

## 0.5.1 (2025-07-29)

### Features Added

- Added support for listing SQL databases via the command: `azmcp-sql-db-list`. [[#746](https://github.com/Azure/azure-mcp/pull/746)]
- Added support for reading `AZURE_SUBSCRIPTION_ID` from the environment variables if a subscription is not provided. [[#533](https://github.com/Azure/azure-mcp/pull/533)]

### Breaking Changes

- Removed the following Key Vault operations: [[#768](https://github.com/Azure/azure-mcp/pull/768)]
  - `azmcp-keyvault-secret-get`
  - `azmcp-keyvault-key-get`

### Other Changes

- Improved the MAC address search logic for telemetry by making it more robust in finding a valid network interface. [[#759](https://github.com/Azure/azure-mcp/pull/759)]
- Major repository structure change:
  - Service areas moved from `/src/areas/{Area}` and `/tests/areas/{Area}` into `/areas/{area}/src` and `/areas/{area}/tests`
  - Common code moved into `/core/src` and `/core/tests`

## 0.5.0 (2025-07-24)

### Features Added

- Added a new VS Code extension (VSIX installer) for the VS Code Marketplace. [[#661](https://github.com/Azure/azure-mcp/pull/661)]
- Added `--mode all` startup option to expose all Azure MCP tools individually. [[#689](https://github.com/Azure/azure-mcp/issues/689)]
- Added more tools for Azure Key Vault: [[#517](https://github.com/Azure/azure-mcp/pull/517)]
  - `azmcp-keyvault-certificate-list` - List certificates in a key vault
  - `azmcp-keyvault-certificate-get` - Get details of a specific certificate
  - `azmcp-keyvault-certificate-create` - Create a new certificate
  - `azmcp-keyvault-secret-list` - List secrets in a key vault
  - `azmcp-keyvault-secret-create` - Create a new secret
- Added support for Azure Workbooks management operations: [[#629](https://github.com/Azure/azure-mcp/pull/629)]
  - `azmcp-workbooks-list` - List workbooks in a resource group with optional filtering
  - `azmcp-workbooks-show` - Get detailed information about a specific workbook
  - `azmcp-workbooks-create` - Create new workbooks with custom visualizations and content
  - `azmcp-workbooks-update` - Update existing workbook configurations and metadata
  - `azmcp-workbooks-delete` - Delete workbooks when no longer needed
- Added support for creating a directory in Azure Storage DataLake via the `azmcp-storage-datalake-directory-create` command. [[#647](https://github.com/Azure/azure-mcp/pull/647)]
- Added support for getting the details of an Azure Kubernetes Service (AKS) cluster via the `azmcp-aks-cluster-get` command. [[#700](https://github.com/Azure/azure-mcp/pull/700)]

### Breaking Changes

- Changed the default startup mode to list tools at the namespace level instead of at an individual level, reducing total tool count from around 128 tools to 25. Use `--mode all` to restore the previous behavior of exposing all tools individually. [[#689](https://github.com/Azure/azure-mcp/issues/689)]
- Consolidated Azure best practices commands into the command `azmcp-bestpractices-get` with `--resource` and `--action` parameters: [[#677](https://github.com/Azure/azure-mcp/pull/677)]
  - Removed `azmcp-bestpractices-general-get`, `azmcp-bestpractices-azurefunctions-get-code-generation` and `azmcp-bestpractices-azurefunctions-get-deployment`
  - Use `--resource general --action code-generation` for general Azure code generation best practices
  - Use `--resource general --action deployment` for general Azure deployment best practices
  - Use `--resource azurefunctions --action code-generation` instead of the old azurefunctions code-generation command
  - Use `--resource azurefunctions --action deployment` instead of the old azurefunctions deployment command
  - Use `--resource static-web-app --action all` to get Static Web Apps development and deployment best practices

### Bugs Fixed

- Fixes tool discovery race condition causing "tool not found" errors in MCP clients that use different processes to start and use the server, like LangGraph. [[#556](https://github.com/Azure/azure-mcp/issues/556)]

## 0.4.1 (2025-07-17)

### Features Added

- Added support for the following Azure Load Testing operations: [[#315](https://github.com/Azure/azure-mcp/pull/315)]
  - `azmcp-loadtesting-testresource-list` - List Azure Load testing resources.
  - `azmcp-loadtesting-testresource-create` - Create a new Azure Load testing resource.
  - `azmcp-loadtesting-test-get` - Get details of a specific load test configuration.
  - `azmcp-loadtesting-test-create` - Create a new load test configuration.
  - `azmcp-loadtesting-testrun-get` - Get details of a specific load test run.
  - `azmcp-loadtesting-testrun-list` - List all load test runs for a specific test.
  - `azmcp-loadtesting-testrun-create` - Create a new load test run.
  - `azmcp-loadtesting-testrun-delete` - Delete a specific load test run.
- Added support for scanning Azure resources for compliance recommendations using the Azure Quick Review CLI via the command: `azmcp-extension-azqr`. [[#510](https://github.com/Azure/azure-mcp/pull/510)]
- Added support for listing paths in Data Lake file systems via the command: `azmcp-storage-datalake-file-system-list-paths`. [[#608](https://github.com/Azure/azure-mcp/pull/608)]
- Added support for listing SQL elastic pools via the command: `azmcp-sql-elastic-pool-list`. [[#606](https://github.com/Azure/azure-mcp/pull/606)]
- Added support for listing SQL server firewall rules via the command: `azmcp-sql-firewall-rule-list`. [[#610](https://github.com/Azure/azure-mcp/pull/610)]
- Added new commands for obtaining Azure Functions best practices via the following commands: [[#630](https://github.com/Azure/azure-mcp/pull/630)]
  - `azmcp-bestpractices-azurefunctions-get-code-generation` - Get code generation best practices for Azure Functions.
  - `azmcp-bestpractices-azurefunctions-get-deployment` - Get deployment best practices for Azure Functions.
- Added support for get details about a product in the Azure Marketplace via the command: `azmcp-marketplace-product-get`. [[#442](https://github.com/Azure/azure-mcp/pull/442)]

### Breaking Changes

- Renamed the command `azmcp-bestpractices-get` to `azmcp-bestpractices-general-get`. [[#630](https://github.com/Azure/azure-mcp/pull/630)]

### Bugs Fixed

- Fixed an issue with Azure CLI executable path resolution on Windows. [[#611](https://github.com/Azure/azure-mcp/issues/611)]
- Fixed a tool discovery timing issue when calling tools on fresh server instances. [[#604](https://github.com/Azure/azure-mcp/issues/604)]
- Fixed issue where unrecognizable json would be sent to MCP clients in STDIO mode at startup. [[#644](https://github.com/Azure/azure-mcp/issues/644)]

### Other Changes

- Changed `engines.node` in `package.json` to require Node.js version `>=20.0.0`. [[#628](https://github.com/Azure/azure-mcp/pull/628)]

## 0.4.0 (2025-07-15)

### Features Added

- Added support for listing Azure Kubernetes Service (AKS) clusters via the command `azmcp-aks-cluster-list`. [[#560](https://github.com/Azure/azure-mcp/pull/560)]
- Made the following Ahead of Time (AOT) compilation improvements saving `6.96 MB` in size total:
  - Switched to the trimmer-friendly `CreateSlimBuilder` API from `CreateBuilder`, saving `0.63 MB` in size for the native executable. [[#564](https://github.com/Azure/azure-mcp/pull/564)]
  - Switched to the trimmer-friendly `npgsql` API, saving `2.69 MB` in size for the native executable. [[#592](https://github.com/Azure/azure-mcp/pull/592)]
  - Enabled `IlcFoldIdenticalMethodBodies` to fold identical method bodies, saving `3.64 MB` in size for the native executable. [[#598](https://github.com/Azure/azure-mcp/pull/598)]
- Added support for using the hyphen/dash ("-") character in command names. [[#531](https://github.com/Azure/azure-mcp/pull/531)]
- Added support for authenticating with the Azure account used to log into VS Code. Authentication now prioritizes the VS Code broker credential when in the context of VS Code. [[#452](https://github.com/Azure/azure-mcp/pull/452)]

### Breaking Changes

- Removed SSE (Server-Sent Events) transport support. Now, only stdio transport is supported as SSE is no longer part of the MCP specification. [[#593](https://github.com/Azure/azure-mcp/issues/593)]
- Renamed `azmcp-sql-server-entraadmin-list` to `azmcp-sql-server-entra-admin-list` for better readability. [[#602](https://github.com/Azure/azure-mcp/pull/602)]

### Bugs Fixed

- Added a post-install script to ensure platform-specific versions like `@azure/mcp-${platform}-${arch}` can be resolved. Otherwise, fail install to prevent npx caching of `@azure/mcp`. [[#597](https://github.com/Azure/azure-mcp/pull/597)]
- Improved install reliability and error handling when missing platform packages on Ubuntu. [[#394](https://github.com/Azure/azure-mcp/pull/394)]

### Other Changes
- Updated `engines.node` in `package.json` to require Node.js version `>=22.0.0`.

#### Dependency Updates

- Updated the `ModelContextProtocol.AspNetCore` version from `0.3.0-preview.1` to `0.3.0-preview.2`. [[#519](https://github.com/Azure/azure-mcp/pull/519)]

## 0.3.2 (2025-07-10)

### Features Added

- Added support for listing Azure Managed Grafana details via the command: `azmcp-grafana-list`. [[#532](https://github.com/Azure/azure-mcp/pull/532)]
- Added agent best practices for Azure Terraform commands. [[#420](https://github.com/Azure/azure-mcp/pull/420)]

### Bugs Fixed

- Fixed issue where trace logs could be collected as telemetry. [[#540](https://github.com/Azure/azure-mcp/pull/540/)]
- Fixed an issue that prevented the Azure MCP from finding the Azure CLI if it was installed on a path other than the default global one. [[#351](https://github.com/Azure/azure-mcp/issues/351)]

## 0.3.1 (2025-07-08)

### Features Added

- Added support for the following SQL operations:
  - `azmcp-sql-db-show` - Show details of a SQL Database [[#516](https://github.com/Azure/azure-mcp/pull/516)]
  - `azmcp-sql-server-entra-admin-list` - List Microsoft Entra ID administrators for a SQL server [[#529](https://github.com/Azure/azure-mcp/pull/529)]
- Updates Azure MCP tool loading configurations at launch time. [[#513](https://github.com/Azure/azure-mcp/pull/513)]

### Breaking Changes

- Deprecated the `--service` flag. Use `--namespace` and `--mode` options to specify the service and mode the server will run in. [[#513](https://github.com/Azure/azure-mcp/pull/513)]

## 0.3.0 (2025-07-03)

### Features Added

- Added support for Azure AI Foundry [[#274](https://github.com/Azure/azure-mcp/pull/274)]. The following tools are now available:
  - `azmcp-foundry-models-list`
  - `azmcp-foundry-models-deploy`
  - `azmcp-foundry-models-deployments-list`
- Added support for telemetry [[#386](https://github.com/Azure/azure-mcp/pull/386)]. Telemetry is enabled by default but can be disabled by setting `AZURE_MCP_COLLECT_TELEMETRY` to `false`.

### Bugs Fixed

- Fixed a bug where `CallToolResult` was always successful. [[#511](https://github.com/Azure/azure-mcp/pull/511)]

## 0.2.6 (2025-07-01)

### Other Changes

- Updated the descriptions of the following tools to improve their usage by Agents: [[#492](https://github.com/Azure/azure-mcp/pull/492)]
  - `azmcp-datadog-monitoredresources-list`
  - `azmcp-kusto-cluster-list`
  - `azmcp-kusto-database-list`
  - `azmcp-kusto-sample`
  - `azmcp-kusto-table-list`
  - `azmcp-kusto-table-schema`

## 0.2.5 (2025-06-26)

### Bugs Fixed

- Fixed issue where tool listing incorrectly returned resources instead of text. [#465](https://github.com/Azure/azure-mcp/issues/465)
- Fixed invalid modification to HttpClient in KustoClient. [#433](https://github.com/Azure/azure-mcp/issues/433)

## 0.2.4 (2025-06-24)

### Features Added

- Added new command for resource-centric logs query in Azure Monitor with command path `azmcp-monitor-resource-logs-query` - https://github.com/Azure/azure-mcp/pull/413
- Added support for starting the server with a subset of services using the `--service` flag - https://github.com/Azure/azure-mcp/pull/424
- Improved index schema handling in Azure AI Search (index descriptions, facetable fields, etc.) - https://github.com/Azure/azure-mcp/pull/440
- Added new commands for querying metrics with Azure Monitor with command paths `azmcp-monitor-metrics-query` and `azmcp-monitor-metrics-definitions`. - https://github.com/Azure/azure-mcp/pull/428

### Breaking Changes

- Changed the command for workspace-based logs query in Azure Monitor from `azmcp-monitor-log-query` to `azmcp-monitor-workspace-logs-query`

### Bugs Fixed

- Fixed handling of non-retrievable fields in Azure AI Search. [#416](https://github.com/Azure/azure-mcp/issues/416)

### Other Changes

- Repository structure changed to organize all of an Azure service's code into a single "area" folder. ([426](https://github.com/Azure/azure-mcp/pull/426))
- Upgraded Azure.Messaging.ServiceBus to 7.20.1 and Azure.Core to 1.46.2. ([441](https://github.com/Azure/azure-mcp/pull/441/))
- Updated to ModelContextProtocol 0.3.0-preview1, which brings support for the 06-18-2025 MCP specification. ([431](https://github.com/Azure/azure-mcp/pull/431))

## 0.2.3 (2025-06-19)

### Features Added

- Adds support to launch MCP server in readonly mode - https://github.com/Azure/azure-mcp/pull/410

### Bugs Fixed

- MCP tools now expose annotations to clients https://github.com/Azure/azure-mcp/pull/388

## 0.2.2 (2025-06-17)

### Features Added

- Support for Azure ISV Services https://github.com/Azure/azure-mcp/pull/199/
- Support for Azure RBAC https://github.com/Azure/azure-mcp/pull/266
- Support for Key Vault Secrets https://github.com/Azure/azure-mcp/pull/173


## 0.2.1 (2025-06-12)

### Bugs Fixed

- Fixed the issue where queries containing double quotes failed to execute. https://github.com/Azure/azure-mcp/pull/338
- Enables dynamic proxy mode within single "azure" tool. https://github.com/Azure/azure-mcp/pull/325

## 0.2.0 (2025-06-09)

### Features Added

- Support for launching smaller service level MCP servers. https://github.com/Azure/azure-mcp/pull/324

### Bugs Fixed

- Fixed failure starting Docker image. https://github.com/Azure/azure-mcp/pull/301

## 0.1.2 (2025-06-03)

### Bugs Fixed

- Monitor Query Logs Failing.  Fixed with https://github.com/Azure/azure-mcp/pull/280

## 0.1.1 (2025-05-30)

### Bugs Fixed

- Fixed return value of `tools/list` to use JSON object names. https://github.com/Azure/azure-mcp/pull/275

### Other Changes

- Update .NET SDK version to 9.0.300 https://github.com/Azure/azure-mcp/pull/278

## 0.1.0 (2025-05-28)

### Breaking Changes

- `azmcp tool list` "args" changes to "options"

### Other Changes

- Removed "Arguments" from code base in favor of "Options" to align with System. CommandLine semantics. https://github.com/Azure/azure-mcp/pull/232

## 0.0.21 (2025-05-22)

### Features Added

- Support for Azure Redis Caches and Clusters https://github.com/Azure/azure-mcp/pull/198
- Support for Azure Monitor Health Models https://github.com/Azure/azure-mcp/pull/208

### Bugs Fixed

- Updates the usage patterns of Azure Developer CLI (azd) when invoked from MCP. https://github.com/Azure/azure-mcp/pull/203
- Fixes server binding issue when using SSE transport in Docker by replacing `ListenLocalhost` with `ListenAnyIP`, allowing external access via port mapping. https://github.com/Azure/azure-mcp/pull/233

### Other Changes

- Updated to the latest ModelContextProtocol library. https://github.com/Azure/azure-mcp/pull/220

## 0.0.20 (2025-05-17)

### Bugs Fixed

- Improve the formatting in the ParseJsonOutput method and refactor it to utilize a ParseError record. https://github.com/Azure/azure-mcp/pull/218
- Added dummy argument for best practices tool, so the schema is properly generated for Python Open API use cases. https://github.com/Azure/azure-mcp/pull/219

## 0.0.19 (2025-05-15)

### Bugs Fixed

- Fixes Service Bus host name parameter description. https://github.com/Azure/azure-mcp/pull/209/

## 0.0.18 (2025-05-14)

### Bugs Fixed

- Include option to exclude managed keys. https://github.com/Azure/azure-mcp/pull/202

## 0.0.17 (2025-05-13)

### Bugs Fixed

- Added an opt-in timeout for browser-based authentication to handle cases where the process waits indefinitely if the user closes the browser. https://github.com/Azure/azure-mcp/pull/189

## 0.0.16 (2025-05-13)

### Bugs Fixed

- Fixed being able to pass args containing spaces through an npx call to the cli

### Other Changes

- Updated to the latest ModelContextProtocol library. https://github.com/Azure/azure-mcp/pull/161

## 0.0.15 (2025-05-09)

### Features Added

- Support for getting properties and runtime information for Azure Service Bus queues, topics, and subscriptions. https://github.com/Azure/azure-mcp/pull/150/
- Support for peeking at Azure Service Bus messages from queues or subscriptions. https://github.com/Azure/azure-mcp/pull/144
- Adds Best Practices tool that provides guidance to LLMs for effective code generation. https://github.com/Azure/azure-mcp/pull/153 https://github.com/Azure/azure-mcp/pull/156

### Other Changes

- Disabled Parallel testing in the ADO pipeline for Live Tests https://github.com/Azure/azure-mcp/pull/151

## 0.0.14 (2025-05-07)

### Features Added

- Support for Azure Key Vault keys https://github.com/Azure/azure-mcp/pull/119
- Support for Azure Data Explorer  https://github.com/Azure/azure-mcp/pull/21

## 0.0.13 (2025-05-06)

### Features Added

- Support for Azure PostgreSQL. https://github.com/Azure/azure-mcp/pull/81

## 0.0.12 (2025-05-05)

### Features Added

- Azure Search Tools https://github.com/Azure/azure-mcp/pull/83

### Other Changes

- Arguments no longer echoed in response: https://github.com/Azure/azure-mcp/pull/79
- Editorconfig and gitattributes updated: https://github.com/Azure/azure-mcp/pull/91

## 0.0.11 (2025-04-29)

### Features Added

### Breaking Changes

### Bugs Fixed
- Bug fixes to existing MCP commands
- See https://github.com/Azure/azure-mcp/releases/tag/0.0.11

### Other Changes

## 0.0.10 (2025-04-17)

### Features Added
- Support for Azure Cosmos DB (NoSQL databases).
- Support for Azure Storage.
- Support for Azure Monitor (Log Analytics).
- Support for Azure App Configuration.
- Support for Azure Resource Groups.
- Support for Azure CLI.
- Support for Azure Developer CLI (azd).

### Breaking Changes

### Bugs Fixed
- See https://github.com/Azure/azure-mcp/releases/tag/0.0.10

### Other Changes
- See Blog post for details https://devblogs.microsoft.com/azure-sdk/introducing-the-azure-mcp-server/
