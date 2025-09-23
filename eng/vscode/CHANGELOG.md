
# Release History

## 0.8.1 (Unreleased)

### Added

### Changed

### Fixed

## 0.8.0 (2025-09-18)

### Added

- Added the `--insecure-disable-elicitation` server startup switch. When enabled, the server will bypass user confirmation (elicitation) for tools marked as handling secrets and execute them immediately. This is **INSECURE** and meant only for controlled automation scenarios (e.g., CI or disposable test environments) because it removes a safety barrier that helps prevent accidental disclosure of sensitive data. [[#486](https://github.com/microsoft/mcp/pull/486)]
- Enhanced Azure authentication with targeted credential selection via the `AZURE_TOKEN_CREDENTIALS` environment variable: [[#56](https://github.com/microsoft/mcp/pull/56)]
  - `"dev"`: Development credentials (Visual Studio → Visual Studio Code → Azure CLI → Azure PowerShell → Azure Developer CLI)
  - `"prod"`: Production credentials (Environment → Workload Identity → Managed Identity)
  - Specific credential names (e.g., `"AzureCliCredential"`): Target only that credential
  - Improved Visual Studio Code credential error handling with proper exception wrapping for credential chaining
  - Replaced custom `DefaultAzureCredential` implementation with explicit credential chain for better control and transparency
  - For more details, see [Controlling Authentication Methods with AZURE_TOKEN_CREDENTIALS](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/TROUBLESHOOTING.md#controlling-authentication-methods-with-azure_token_credentials)
- Added support for updating Azure SQL databases via the command `azmcp_sql_db_update`. [[#488](https://github.com/microsoft/mcp/pull/488)]
- Added support for listing Event Grid subscriptions via the command `azmcp_eventgrid_subscription_list`. [[#364](https://github.com/microsoft/mcp/pull/364)]
- Added support for listing Application Insights code optimization recommendations across components via the command `azmcp_applicationinsights_recommendation_list`. [#387](https://github.com/microsoft/mcp/pull/387)
- **Errata**: The following was announced as part of release `0.7.0, but was not actually included then.
  - Added support for creating and deleting SQL databases via the commands `azmcp_sql_db_create` and `azmcp_sql_db_delete`. [[#434](https://github.com/microsoft/mcp/pull/434)]
- Restored support for the following Key Vault commands: [[#506](https://github.com/microsoft/mcp/pull/506)]
  - `azmcp_keyvault_key_get`
  - `azmcp_keyvault_secret_get`

### Changed

- **Breaking:** Redesigned how conditionally required options are handled. Commands now use explicit option registration via extension methods (`.AsRequired()`, `.AsOptional()`) instead of legacy patterns (`UseResourceGroup()`, `RequireResourceGroup()`). [[#452](https://github.com/microsoft/mcp/pull/452)]
- **Breaking:** Removed support for the `AZURE_MCP_INCLUDE_PRODUCTION_CREDENTIALS` environment variable. Use `AZURE_TOKEN_CREDENTIALS` instead for more flexible credential selection. For migration details, see [Controlling Authentication Methods with AZURE_TOKEN_CREDENTIALS](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/TROUBLESHOOTING.md#controlling-authentication-methods-with-azure_token_credentials). [[#56](https://github.com/microsoft/mcp/pull/56)]
- Enhanced AKS nodepool information with comprehensive properties. [[#454](https://github.com/microsoft/mcp/pull/454)]
- Merged `azmcp_appconfig_kv_lock` and `azmcp_appconfig_kv_unlock` into `azmcp_appconfig_kv_lock_set` which can handle locking or unlocking a key-value based on the `--lock` parameter. [[#485](https://github.com/microsoft/mcp/pull/485)]
- Update `azmcp_foundry_models_deploy` to use "GenericResource" for deploying models to Azure AI Services. [[#456](https://github.com/microsoft/mcp/pull/456)]

## 0.7.0 (2025-09-16)

### Added

- Added support for diagnosing Azure Resources using the App Lens API via the command `azmcp_applens_resource_diagnose`. [[#356](https://github.com/microsoft/mcp/pull/356)]
- Added support for getting a node pool in an AKS managed cluster via the command `azmcp_aks_nodepool_get`. [[#394](https://github.com/microsoft/mcp/pull/394)]
- Added elicitation support. An elicitation request is sent if the tool annotation `secret` hint is true. [[#404](https://github.com/microsoft/mcp/pull/404)]
- Added `azmcp_sql_server_create`, `azmcp_sql_server_delete`, `azmcp_sql_server_show` to support SQL server create, delete, and show commands. [[#312](https://github.com/microsoft/mcp/pull/312)]
- Added the support for getting information about Azure Managed Lustre SKUs via the following command `azmcp_azuremanagedlustre_filesystem_get_sku_info`. [[#100](https://github.com/microsoft/mcp/issues/100)]
- `azmcp_functionapp_get` can now list Function Apps on a resource group level. [[#427](https://github.com/microsoft/mcp/pull/427)]

### Changed

- **Breaking:** Merged `azmcp_functionapp_list` into `azmcp_functionapp_get`, which can perform both operations based on whether `--function-app` is passed. [[#427](https://github.com/microsoft/mcp/pull/427)]
- **Breaking:** Removed Azure CLI (`az`) and Azure Developer CLI (`azd`) extension tools to reduce complexity and focus on native Azure service operations. [[#404](https://github.com/microsoft/mcp/pull/404)].

### Fixed

- Marked the `secret` hint of `azmcp_keyvault_secret_create` tool to "true". [[#430](https://github.com/microsoft/mcp/pull/430)]

## 0.6.0 (2025-09-11)

### Added

- **The Azure MCP Server is now also available on NuGet.org** [[#368](https://github.com/microsoft/mcp/pull/368)]
- Added support for listing node pools in an AKS managed cluster. [[#360](https://github.com/microsoft/mcp/pull/360)]

### Changed

- To improve performance, packages now ship with trimmed binaries that have unused code and dependencies removed, resulting in significantly smaller file sizes, faster startup times, and reduced memory footprint. [Learn more](https://learn.microsoft.com/dotnet/core/deploying/trimming/trim-self-contained). [[#405](https://github.com/microsoft/mcp/pull/405)]
- Merged `azmcp_search_index_describe` and `azmcp_search_index_list` into `azmcp_search_index_get`, which can perform both operations based on whether `--index` is passed. [[#378](https://github.com/microsoft/mcp/pull/378)]
- Merged the following Storage tools: [[#376](https://github.com/microsoft/mcp/pull/376)]
  - `azmcp_storage_account_details` and `azmcp_storage_account_list` into `azmcp_storage_account_get`, which supports the behaviors of both tools based on whether `--account` is passed.
  - `azmcp_storage_blob_details` and `azmcp_storage_blob_list` into `azmcp_storage_blob_get`, which supports the behaviors of both tools based on whether `--blob` is passed.
  - `azmcp_storage_blob_container_details` and `azmcp_storage_blob_container_list` into `azmcp_storage_blob_container_get`, which supports the behaviors of both tools based on whether `--container` is passed.
- Updated the descriptions of all Storage tools. [[#376](https://github.com/microsoft/mcp/pull/376)]

## 0.5.13 - 2025-09-10

### Added

- Added support for listing all Event Grid topics in a subscription via the command `azmcp_eventgrid_topic_list`. [[#43](https://github.com/microsoft/mcp/pull/43)]
- Added support for retrieving knowledge index schema information in Azure AI Foundry projects via the command `azmcp_foundry_knowledge_index_schema`. [[#41](https://github.com/microsoft/mcp/pull/41)]
- Added support for listing service health events in a subscription via the command `azmcp_resourcehealth_service-health-events_list`. [[#367](https://github.com/microsoft/mcp/pull/367)]

### Changed

- **Breaking:** Updated/removed options for the following commands: [[#108](https://github.com/microsoft/mcp/pull/108)]
  - `azmcp_storage_account_create`: Removed the ability to configure `enable-https-traffic-only` (always `true` now), `allow-blob-public-access` (always `false` now), and `kind` (always `StorageV2` now).
  - `azmcp_storage_blob_container_create`: Removed the ability to configure `blob-container-public-access` (always `false` now).
  - `azmcp_storage_blob_upload`: Removed the ability to configure `overwrite` (always `false` now).
- Added telemetry to log parameter values for the `azmcp_bestpractices_get` tool. [[#375](https://github.com/microsoft/mcp/pull/375)]
- Updated tool annotations. [[#377](https://github.com/microsoft/mcp/pull/377)]

### Fixed

- Fixed telemetry bug where "ToolArea" was incorrectly populated with "ToolName". [[#346](https://github.com/microsoft/mcp/pull/346)]

## 0.5.12 - 2025-09-04

### Added

- Added `azmcp_sql_server_firewall-rule_create` and `azmcp_sql_server_firewall-rule_delete` commands. [[#121](https://github.com/microsoft/mcp/pull/121)]
- Added a verb to the namespace name for bestpractices. [[#109](https://github.com/microsoft/mcp/pull/109)]
- Added instructions about consumption plan for azure functions deployment best practices. [[#218](https://github.com/microsoft/mcp/pull/218)]

### Fixed

- Fixed a bug in MySQL query validation logic. [[#81](https://github.com/microsoft/mcp/pull/81)]

## 0.5.11 - 2025-09-02

### Fixed

- Fixed VSIX signing [[#91](https://github.com/microsoft/mcp/pull/91)]
- Included native packages in build artifacts and pack/release scripts. [[#51](https://github.com/microsoft/mcp/pull/51)]

## 0.5.10 - 2025-08-28

### Fixed

- Fixed a bug with telemetry collection related to AppConfig tools. [[#44](https://github.com/microsoft/mcp/pull/44)]

## 0.5.9 - 2025-08-26

### Changed

- Updated dependencies to improve .NET Ahead-of-Time (AOT) compilation support:
  - `Microsoft.Azure.Cosmos` `3.51.0` → `Microsoft.Azure.Cosmos.Aot` `0.1.1-preview.1`. [[#37](https://github.com/microsoft/mcp/pull/37)]

## 0.5.8 - 2025-08-21

### Added

- Added support for listing knowledge indexes in Azure AI Foundry projects via the command `azmcp_foundry_knowledge_index_list`. [[#1004](https://github.com/Azure/azure-mcp/pull/1004)]
- Added support for getting details of an Azure Function App via the `azmcp_functionapp_get` command. [[#970](https://github.com/Azure/azure-mcp/pull/970)]
- Added the following Azure Managed Lustre commands: [[#1003](https://github.com/Azure/azure-mcp/issues/1003)]
  - `azmcp_azuremanagedlustre_filesystem_list`: List available Azure Managed Lustre filesystems.
  - `azmcp_azuremanagedlustre_filesystem_required-subnet-size`: Returns the number of IP addresses required for a specific SKU and size of Azure Managed Lustre filesystem.
- Added support for designing Azure Cloud Architecture through guided questions via the `azmcp_cloudarchitect_design` command. [[#890](https://github.com/Azure/azure-mcp/pull/890)]
- Added support for the following Azure MySQL operations: [[#855](https://github.com/Azure/azure-mcp/issues/855)]
  - `azmcp_mysql_database_list` - List all databases in a MySQL server.
  - `azmcp_mysql_database_query` - Execute a SELECT query on a MySQL database (non-destructive only).
  - `azmcp_mysql_table_list` - List all tables in a MySQL database.
  - `azmcp_mysql_table_schema_get` - Get the schema of a specific table in a MySQL database.
  - `azmcp_mysql_server_config_get` - Retrieve the configuration of a MySQL server.
  - `azmcp_mysql_server_list` - List all MySQL servers in a subscription and resource group.
  - `azmcp_mysql_server_param_get` - Retrieve a specific parameter of a MySQL server.
  - `azmcp_mysql_server_param_set` - Set a specific parameter of a MySQL server to a specific value.
- Added telemetry for tracking service area when calling tools. [[#1024](https://github.com/Azure/azure-mcp/pull/1024)]

### Changed

- Standardized Azure Storage command descriptions, option names, and parameter names; cleaned up JSON serialization context. [[#1015](https://github.com/Azure/azure-mcp/pull/1015)]
  - **Breaking:** Renamed the following Storage tool option names for consistency:
    - `azmcp_storage_account_create`: `account-name` → `account`.
    - `azmcp_storage_blob_batch_set-tier`: `blob-names` → `blobs`.
- Introduced `BaseAzureResourceService` to enable Azure Resource read operations using Azure Resource Graph queries. [[#938](https://github.com/Azure/azure-mcp/pull/938)]
- Refactored SQL service to use Azure Resource Graph instead of direct ARM API calls, removing dependency on `Azure.ResourceManager.Sql` and improving startup performance. [[#938](https://github.com/Azure/azure-mcp/pull/938)]
- Enhanced `BaseAzureService` with `EscapeKqlString` for safe KQL query construction across all Azure services; fixed KQL string escaping in Workbooks queries. [[#938](https://github.com/Azure/azure-mcp/pull/938)]
- Updated to .NET 10 SDK to prepare for .NET tool packing.
- Improved `bestpractices` and `azureterraformbestpractices` tool descriptions to work better with VS Code Copilot tool grouping. [[#1029](https://github.com/Azure/azure-mcp/pull/1029)]

### Fixed

- SQL service tests now use case-insensitive string comparisons for resource type validation. [[#938](https://github.com/Azure/azure-mcp/pull/938)]
- HttpClient service tests now validate NoProxy collection handling correctly (instead of assuming a single string). [[#938](https://github.com/Azure/azure-mcp/pull/938)]

## 0.5.7 - 2025-08-19

### Added

- Added support for the following Azure Deploy and Azure Quota operations: [[#626](https://github.com/Azure/azure-mcp/pull/626)]
  - `azmcp_deploy_app_logs_get` - Get logs from Azure applications deployed using azd.
  - `azmcp_deploy_iac_rules_get` - Get Infrastructure as Code rules.
  - `azmcp_deploy_pipeline_guidance-get` - Get guidance for creating CI/CD pipelines to provision Azure resources and deploy applications.
  - `azmcp_deploy_plan_get` - Generate deployment plans to construct infrastructure and deploy applications on Azure.
  - `azmcp_deploy_architecture_diagram-generate` - Generate Azure service architecture diagrams based on application topology.
  - `azmcp_quota_region_availability-list` - List available Azure regions for specific resource types.
  - `azmcp_quota_usage_check` - Check Azure resource usage and quota information for specific resource types and regions.
- Added support for listing Azure Function Apps via the `azmcp-functionapp-list` command. [[#863](https://github.com/Azure/azure-mcp/pull/863)]
- Added support for importing existing certificates into Azure Key Vault via the `azmcp-keyvault-certificate-import` command. [[#968](https://github.com/Azure/azure-mcp/issues/968)]
- Added support for uploading a local file to an Azure Storage blob via the `azmcp-storage-blob-upload` command. [[#960](https://github.com/Azure/azure-mcp/pull/960)]
- Added support for the following Azure Service Health operations: [[#998](https://github.com/Azure/azure-mcp/pull/998)]
  - `azmcp-resourcehealth-availability-status-get` - Get the availability status for a specific resource.
  - `azmcp-resourcehealth-availability-status-list` - List availability statuses for all resources in a subscription or resource group.
- Added support for listing repositories in Azure Container Registries via the `azmcp-acr-registry-repository-list` command. [[#983](https://github.com/Azure/azure-mcp/pull/983)]

### Changed

- Improved guidance for LLM interactions with Azure MCP server by adding rules around bestpractices tool calling to server instructions. [[#1007](https://github.com/Azure/azure-mcp/pull/1007)]

## 0.5.6 - 2025-08-14

### Added

- New VS Code settings to control Azure MCP server startup behavior: [[#971](https://github.com/Azure/azure-mcp/issues/971)]
  - `azureMcp.serverMode`: choose tool exposure mode — `single` | `namespace` (default) | `all`.
  - `azureMcp.readOnly`: start the server in read-only mode.
  - `azureMcp.enabledServices`: added drop down list to select and configure the enabled services.
- Added support for listing Azure Function Apps via the `azmcp-functionapp-list` command. [[#863](https://github.com/Azure/azure-mcp/pull/863)]
- Added support for getting details about an Azure Storage Account via the `azmcp-storage-account-details` command. [[#934](https://github.com/Azure/azure-mcp/issues/934)]

### Changed

- Centralized handling and validation of the `--resource-group` option across all commands. [[#961](https://github.com/Azure/azure-mcp/issues/961)]

## 0.5.5 - 2025-08-12

### Added

- Added support for listing Azure Container Registry (ACR) registries in a subscription via the `azmcp-acr-registry-list` command. [[#915](https://github.com/Azure/azure-mcp/issues/915)]
- Added new Azure Storage commands:
  - `azmcp-storage-account-create`: Create a new Storage account. [[#927](https://github.com/Azure/azure-mcp/issues/927)]
  - `azmcp-storage-queue-message-send`: Send a message to a Storage queue. [[#794](https://github.com/Azure/azure-mcp/pull/794)]
  - `azmcp-storage-blob-details`: Get details about a Storage blob. [[#930](https://github.com/Azure/azure-mcp/issues/930)]
  - `azmcp-storage-blob-container-create`: Create a new Storage blob container. [[#937](https://github.com/Azure/azure-mcp/issues/937)]
- Bundled the **GitHub Copilot for Azure** extension as part of the Azure MCP Server extension pack.

### Changed

- The `azmcp-storage-account-list` command now returns account metadata objects instead of plain strings. Each item includes: `name`, `location`, `kind`, `skuName`, `skuTier`, `hnsEnabled`, `allowBlobPublicAccess`, `enableHttpsTrafficOnly`. Update scripts to read the `name` property. The underlying `IStorageService.GetStorageAccounts()` signature changed from `Task<List<string>>` to `Task<List<StorageAccountInfo>>`. [[#904](https://github.com/Azure/azure-mcp/issues/904)]
- Consolidated "AzSubscriptionGuid" telemetry logic into `McpRuntime`. [[#935](https://github.com/Azure/azure-mcp/pull/935)]

### Fixed

- Fixed best practices tool invocation failure when passing "all" action with "general" or "azurefunctions" resources. [[#757](https://github.com/Azure/azure-mcp/issues/757)]
- Updated metadata for CREATE and SET tools to `destructive = true`. [[#773](https://github.com/Azure/azure-mcp/pull/773)]

## 0.5.4 - 2025-08-07

### Changed

- Improved Azure MCP display name in VS Code from 'azure-mcp-server-ext' to 'Azure MCP' for better user experience in the Configure Tools interface. [[#871](https://github.com/Azure/azure-mcp/issues/871), [#876](https://github.com/Azure/azure-mcp/pull/876)]
- Updated the description of the following `CommandGroup`s to improve their tool usage by Agents:
  - Azure AI Search [[#874](https://github.com/Azure/azure-mcp/pull/874)]
  - Storage [#879](https://github.com/Azure/azure-mcp/pull/879)

### Fixed

- Fixed subscription parameter handling across all Azure MCP service methods to consistently use `subscription` instead of `subscriptionId`, enabling proper support for both subscription IDs and subscription names. [[#877](https://github.com/Azure/azure-mcp/issues/877)]
- Fixed `ToolExecuted` telemetry activity being created twice. [[#741](https://github.com/Azure/azure-mcp/pull/741)]

## 0.5.3 - 2025-08-05

### Added

- Added support for providing the `--content-type` and `--tags` properties to the `azmcp-appconfig-kv-set` command. [[#459](https://github.com/Azure/azure-mcp/pull/459)]
- Added `filter-path` and `recursive` capabilities to `azmcp-storage-datalake-file-system-list-paths`. [[#770](https://github.com/Azure/azure-mcp/issues/770)]
- Added support for listing files and directories in Azure File Shares via the `azmcp-storage-share-file-list` command. This command recursively lists all items in a specified file share directory with metadata including size, last modified date, and content type. [[#793](https://github.com/Azure/azure-mcp/pull/793)]
- Added support for Azure Virtual Desktop with new commands: [[#653](https://github.com/Azure/azure-mcp/pull/653)]
  - `azmcp-virtualdesktop-hostpool-list` - List all host pools in a subscription
  - `azmcp-virtualdesktop-sessionhost-list` - List all session hosts in a host pool
  - `azmcp-virtualdesktop-sessionhost-usersession-list` - List all user sessions on a specific session host
- Added support for creating and publishing DevDeviceId in telemetry. [[#810](https://github.com/Azure/azure-mcp/pull/810/)]
- Added caching for Cosmos DB databases and containers. [[813](https://github.com/Azure/azure-mcp/pull/813)]

### Changed

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

### Fixed

- Fixed an issue where the `azmcp-storage-blob-batch-set-tier` command did not correctly handle the `--tier` parameter when setting the access tier for multiple blobs. [[#808](https://github.com/Azure/azure-mcp/pull/808)]

## 0.5.2 - 2025-07-31

### Added

- Added support for batch setting access tier for multiple Azure Storage blobs via the `azmcp-storage-blob-batch-set-tier` command. This command efficiently changes the storage tier (Hot, Cool, Archive, etc) for multiple blobs simultaneously in a single operation. [[#735](https://github.com/Azure/azure-mcp/issues/735)]
- Added descriptions to all Azure MCP command groups to improve discoverability and usability when running the server with `--mode single` or `--mode namespace`. [[#791](https://github.com/Azure/azure-mcp/pull/791)]

### Changed

- Removed toast notifications related to Azure MCP server registration and startup instructions.[[#785](https://github.com/Azure/azure-mcp/pull/785)]
- Removed `--partner-tenant-id` option from `azmcp-marketplace-product-get` command. [[#656](https://github.com/Azure/azure-mcp/pull/656)]

## 0.5.1 - 2025-07-29

### Added

- Added support for listing SQL databases via the command: `azmcp-sql-db-list`. [[#746](https://github.com/Azure/azure-mcp/pull/746)]
- Added support for reading `AZURE_SUBSCRIPTION_ID` from the environment variables if a subscription is not provided. [[#533](https://github.com/Azure/azure-mcp/pull/533)]

## 0.5.0 - 2025-07-24

### Added
- Initial Release
