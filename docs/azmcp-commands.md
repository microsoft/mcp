# Azure MCP CLI Command Reference

> [!IMPORTANT]
> The Azure MCP Server has two modes: MCP Server mode and CLI mode.  When you start the MCP Server with `azmcp server start` that will expose an endpoint for MCP Client communication. The `azmcp` CLI also exposes all of the Tools via a command line interface, i.e. `azmcp subscription list`.  Since `azmcp` is built on a CLI infrastructure, you'll see the word "Command" be used interchangeably with "Tool".

## Global Options

The following options are available for all commands:

| Option | Required | Default | Description |
|-----------|----------|---------|-------------|
| `--subscription` | No | Environment variable `AZURE_SUBSCRIPTION_ID` | Azure subscription ID for target resources |
| `--tenant-id` | No | - | Azure tenant ID for authentication |
| `--auth-method` | No | 'credential' | Authentication method ('credential', 'key', 'connectionString') |
| `--retry-max-retries` | No | 3 | Maximum retry attempts for failed operations |
| `--retry-delay` | No | 2 | Delay between retry attempts (seconds) |
| `--retry-max-delay` | No | 10 | Maximum delay between retries (seconds) |
| `--retry-mode` | No | 'exponential' | Retry strategy ('fixed' or 'exponential') |
| `--retry-network-timeout` | No | 100 | Network operation timeout (seconds) |

## Available Commands

### Server Operations

The Azure MCP Server can be started in several different modes depending on how you want to expose the Azure tools:

#### Default Mode (Namespace)

Exposes Azure tools grouped by service namespace. Each Azure service appears as a single namespace-level tool that routes to individual operations internally. This is the default mode to reduce tool count and prevent VS Code from hitting the 128 tool limit.

```bash
# Start MCP Server with namespace-level tools (default behavior)
azmcp server start \
    [--transport <transport>] \
    [--read-only]

# Explicitly specify namespace mode
azmcp server start \
    --mode namespace \
    [--transport <transport>] \
    [--read-only]
```

#### All Tools Mode

Exposes all Azure tools individually. Each Azure service operation appears as a separate MCP tool.

```bash
# Start MCP Server with all tools exposed individually
azmcp server start \
    --mode all \
    [--transport <transport>] \
    [--read-only]
```

#### Single Tool Mode

Exposes a single "azure" tool that handles internal routing across all Azure MCP tools.

```bash
# Start MCP Server with single azure tool
azmcp server start \
    --mode single \
    [--transport <transport>] \
    [--read-only]
```

#### Namespace Filtering

Exposes only tools for specific Azure service namespaces. Use multiple `--namespace` parameters to include multiple namespaces.

```bash
# Start MCP Server with only Storage tools
azmcp server start \
    --namespace storage \
    --mode all \
    [--transport <transport>] \
    [--read-only]

# Start MCP Server with Storage and Key Vault tools
azmcp server start \
    --namespace storage \
    --namespace keyvault \
    --mode all \
    [--transport <transport>] \
    [--read-only]
```

#### Namespace Mode (Default)

Collapses all tools within each namespace into a single tool (e.g., all storage operations become one "storage" tool with internal routing). This mode is particularly useful when working with MCP clients that have tool limits - for example, VS Code only supports a maximum of 128 tools across all registered MCP servers.

```bash
# Start MCP Server with service proxy tools
azmcp server start \
    --mode namespace \
    [--transport <transport>] \
    [--read-only]
```

#### Single Tool Proxy Mode

Exposes a single "azure" tool that handles internal routing across all Azure MCP tools.

```bash
# Start MCP Server with single Azure tool proxy
azmcp server start \
    --mode single \
    [--transport <transport>] \
    [--read-only]
```

> **Note:**
>
> - For namespace mode, replace `<namespace-name>` with available top level command groups. Run `azmcp -h` to review available namespaces. Examples include `storage`, `keyvault`, `cosmos`, `monitor`, etc.
> - The `--read-only` flag applies to all modes and filters the tool list to only contain tools that provide read-only operations.
> - Multiple `--namespace` parameters can be used together to expose tools for multiple specific namespaces.
> - The `--namespace` and `--mode` parameters can also be combined to provide a unique running mode based on the desired scenario.

### Azure AI Foundry Operations

```bash
# List knowledge indexes in an AI Foundry project
azmcp foundry knowledge index list --endpoint <endpoint>

# Get knowledge index schema information
azmcp foundry knowledge index schema --endpoint <endpoint> \
                                     --index <index>

# Deploy an AI Foundry model
azmcp foundry models deploy --subscription <subscription> \
                            --resource-group <resource-group> \
                            --deployment <deployment> \
                            --model-name <model> \
                            --model-format <model-format> \
                            --azure-ai-services <azure-ai-services> \
                            [--model-version <model-version>] \
                            [--model-source <model-source>] \
                            [--sku <sku>] \
                            [--sku-capacity <sku-capacity>] \
                            [--scale-type <scale-type>] \
                            [--scale-capacity <scale-capacity>]

# List AI Foundry model deployments
azmcp foundry models deployments list --endpoint <endpoint>

# List AI Foundry models
azmcp foundry models list [--search-for-free-playground <search-for-free-playground>] \
                          [--publisher <publisher>] \
                          [--license <license>] \
                          [--model-name <model>]
```

### Azure AI Search Operations

```bash
# Get AI Search index
azmcp search index describe --subscription <subscription> \
                            --service <service> \
                            --index <index>

# List AI Search indexes in account
azmcp search index list --subscription <subscription> \
                        --service <service>

# Query AI Search index
azmcp search index query --subscription <subscription> \
                         --service <service> \
                         --index <index> \
                         --query <query>

# List AI Search accounts in a subscription
azmcp search list --subscription <subscription>
```

### Azure App Configuration Operations

```bash
# List App Configuration stores in a subscription
azmcp appconfig account list --subscription <subscription>

# Delete a key-value setting
azmcp appconfig kv delete --subscription <subscription> \
                          --account <account> \
                          --key <key> \
                          [--label <label>]

# List all key-value settings in an App Configuration store
azmcp appconfig kv list --subscription <subscription> \
                        --account <account> \
                        [--key <key>] \
                        [--label <label>]

# Lock a key-value setting (make it read-only)
azmcp appconfig kv lock --subscription <subscription> \
                        --account <account> \
                        --key <key> \
                        [--label <label>]

# Set a key-value setting
azmcp appconfig kv set --subscription <subscription> \
                       --account <account> \
                       --key <key> \
                       --value <value> \
                       [--label <label>]

# Show a specific key-value setting
azmcp appconfig kv show --subscription <subscription> \
                        --account <account> \
                        --key <key> \
                        [--label <label>]

# Unlock a key-value setting (make it editable)
azmcp appconfig kv unlock --subscription <subscription> \
                          --account <account> \
                          --key <key> \
                          [--label <label>]
```

### Azure CLI Operations

```bash
# Execute any Azure CLI command
azmcp extension az --command "<command>"

# Examples:
# List resource groups
azmcp extension az --command "group list"

# Get storage account details
azmcp extension az --command "storage account show --name <account> --resource-group <resource-group>"

# List virtual machines
azmcp extension az --command "vm list --resource-group <resource-group>"
```

### Azure Container Registry (ACR) Operations

```bash
# List Azure Container Registries in a subscription
azmcp acr registry list --subscription <subscription>

# List Azure Container Registries in a specific resource group
azmcp acr registry list --subscription <subscription> \
                        --resource-group <resource-group>

# List repositories across all registries in a subscription
azmcp acr registry repository list --subscription <subscription>

# List repositories across all registries in a specific resource group
azmcp acr registry repository list --subscription <subscription> \
                                   --resource-group <resource-group>

# List repositories in a specific registry
azmcp acr registry repository list --subscription <subscription> \
                                   --resource-group <resource-group> \
                                   --registry <registry>
```

### Azure Cosmos DB Operations

```bash
# List Cosmos DB accounts in a subscription
azmcp cosmos account list --subscription <subscription>

# Query items in a Cosmos DB container
azmcp cosmos database container item query --subscription <subscription> \
                                           --account <account> \
                                           --database <database> \
                                           --container <container> \
                                           [--query "SELECT * FROM c"]

# List containers in a Cosmos DB database
azmcp cosmos database container list --subscription <subscription> \
                                     --account <account> \
                                     --database <database>

# List databases in a Cosmos DB account
azmcp cosmos database list --subscription <subscription> \
                           --account <account>
```

### Azure Data Explorer Operations

```bash
# Get details for a Azure Data Explorer cluster
azmcp kusto cluster get --subscription <subscription> \
                        --cluster <cluster>

# List Azure Data Explorer clusters in a subscription
azmcp kusto cluster list --subscription <subscription>

# List databases in a Azure Data Explorer cluster
azmcp kusto database list [--cluster-uri <cluster-uri> | --subscription <subscription> --cluster <cluster>]

# Retrieves a sample of data from a specified Azure Data Explorer table.
azmcp kusto sample [--cluster-uri <cluster-uri> | --subscription <subscription> --cluster <cluster>]
                   --database <database> \
                   --table <table> \
                   [--limit <limit>]

# List tables in a Azure Data Explorer database
azmcp kusto table list [--cluster-uri <cluster-uri> | --subscription <subscription> --cluster <cluster>] \
                       --database <database>

# Retrieves the schema of a specified Azure Data Explorer table.
azmcp kusto table schema [--cluster-uri <cluster-uri> | --subscription <subscription> --cluster <cluster>] \
                         --database <database> \
                         --table <table>

# Query Azure Data Explorer database
azmcp kusto query [--cluster-uri <cluster-uri> | --subscription <subscription> --cluster <cluster>] \
                  --database <database> \
                  --query "<kql-query>"

```

### Azure Database for MySQL Operations

#### Database commands

```bash
# List all databases in a MySQL server
azmcp mysql database list --subscription <subscription> \
                          --resource-group <resource-group> \
                          --user <user> \
                          --server <server>

# Executes a SELECT query on a MySQL Database. The query must start with SELECT and cannot contain any destructive SQL operations for security reasons.
azmcp mysql database query --subscription <subscription> \
                           --resource-group <resource-group> \
                           --user <user> \
                           --server <server> \
                           --database <database> \
                           --query <query>
```

#### Table Commands

```bash
# List all tables in a MySQL database
azmcp mysql table list --subscription <subscription> \
                       --resource-group <resource-group> \
                       --user <user> \
                       --server <server> \
                       --database <database>

# Get the schema of a specific table in a MySQL database
azmcp mysql table schema get --subscription <subscription> \
                             --resource-group <resource-group> \
                             --user <user> \
                             --server <server> \
                             --database <database> \
                             --table <table>
```

#### Server Commands

```bash
# Retrieve the configuration of a MySQL server
azmcp mysql server config get --subscription <subscription> \
                              --resource-group <resource-group> \
                              --user <user> \
                              --server <server>

# List all MySQL servers in a subscription & resource group
azmcp mysql server list --subscription <subscription> \
                        --resource-group <resource-group> \
                        --user <user>

# Retrieve a specific parameter of a MySQL server
azmcp mysql server param get --subscription <subscription> \
                             --resource-group <resource-group> \
                             --user <user> \
                             --server <server> \
                             --param <parameter>

# Set a specific parameter of a MySQL server to a specific value
azmcp mysql server param set --subscription <subscription> \
                             --resource-group <resource-group> \
                             --user <user> \
                             --server <server> \
                             --param <parameter> \
                             --value <value>
```

### Azure Database for PostgreSQL Operations

#### Database commands

```bash
# List all databases in a PostgreSQL server
azmcp postgres database list --subscription <subscription> \
                             --resource-group <resource-group> \
                             --user <user> \
                             --server <server>

# Execute a query on a PostgreSQL database
azmcp postgres database query --subscription <subscription> \
                              --resource-group <resource-group> \
                              --user <user> \
                              --server <server> \
                              --database <database> \
                              --query <query>
```

#### Table Commands

```bash
# List all tables in a PostgreSQL database
azmcp postgres table list --subscription <subscription> \
                          --resource-group <resource-group> \
                          --user <user> \
                          --server <server> \
                          --database <database>

# Get the schema of a specific table in a PostgreSQL database
azmcp postgres table schema get --subscription <subscription> \
                                --resource-group <resource-group> \
                                --user <user> \
                                --server <server> \
                                --database <database> \
                                --table <table>
```

#### Server Commands

```bash
# Retrieve the configuration of a PostgreSQL server
azmcp postgres server config get --subscription <subscription> \
                                 --resource-group <resource-group> \
                                 --user <user> \
                                 --server <server>

# List all PostgreSQL servers in a subscription & resource group
azmcp postgres server list --subscription <subscription> \
                           --resource-group <resource-group> \
                           --user <user>

# Retrieve a specific parameter of a PostgreSQL server
azmcp postgres server param get --subscription <subscription> \
                                --resource-group <resource-group> \
                                --user <user> \
                                --server <server> \
                                --param <parameter>

# Set a specific parameter of a PostgreSQL server to a specific value
azmcp postgres server param set --subscription <subscription> \
                                --resource-group <resource-group> \
                                --user <user> \
                                --server <server> \
                                --param <parameter> \
                                --value <value>
```

### Azure Developer CLI Operations

```bash
# Execute any Azure CLI command
azmcp extension azd --command "<command>"

# Examples:
# Create a sample todo list app with NodeJS and MongoDB
azmcp extension azd --command "init --template todo-nodejs-mongo"
```

### Azure Deploy Operations

```bash
# Get the application service log for a specific azd environment
azmcp deploy app logs get --workspace-folder <workspace-folder> \
                          --azd-env-name <azd-env-name> \
                          [--limit <limit>]

# Generate a mermaid architecture diagram for the application topology follow the schema defined in [deploy-app-topology-schema.json](../areas/deploy/src/AzureMcp.Deploy/Schemas/deploy-app-topology-schema.json)
azmcp deploy architecture diagram generate --raw-mcp-tool-input <app-topology>

# Get the iac generation rules for the resource types
azmcp deploy iac rules get --deployment-tool <deployment-tool> \
                           --iac-type <iac-type> \
                           --resource-types <resource-types>

# Get the ci/cd pipeline guidance
azmcp deploy pipeline guidance get [--use-azd-pipeline-config <use-azd-pipeline-config>] \
                                   [--organization-name <organization-name>] \
                                   [--repository-name <repository-name>] \
                                   [--github-environment-name <github-environment-name>]

# Get a deployment plan for a specific project
azmcp deploy plan get --workspace-folder <workspace-folder> \
                      --project-name <project-name> \
                      --target-app-service <target-app-service> \
                      --provisioning-tool <provisioning-tool> \
                      [--azd-iac-options <azd-iac-options>]
```

### Azure Event Grid Operations

```bash
# List all Event Grid topics in a subscription or resource group
azmcp eventgrid topic list --subscription <subscription> \
                           [--resource-group <resource-group>]
```

### Azure Function App Operations

```bash
# Get details for a specific Function App
azmcp functionapp get --subscription <subscription> \
                      --resource-group <resource-group> \
                      --function-app <function-app-name>

# List function apps in a subscription
azmcp functionapp list --subscription <subscription>
```

### Azure Key Vault Operations

```bash
# Creates a certificate in a key vault with the default policy
azmcp keyvault certificate create --subscription <subscription> \
                                  --vault <vault-name> \
                                  --name <certificate-name>

# Gets a certificate in a key vault
azmcp keyvault certificate get --subscription <subscription> \
                               --vault <vault-name> \
                               --name <certificate-name>

# Imports an existing certificate (PFX or PEM) into a key vault
azmcp keyvault certificate import --subscription <subscription> \
                                  --vault <vault-name> \
                                  --certificate <certificate-name> \
                                  --certificate-data <path-or-base64-or-raw-pem> \
                                  [--password <pfx-password>]

# Lists certificates in a key vault
azmcp keyvault certificate list --subscription <subscription> \
                                --vault <vault-name>

# Creates a key in a key vault
azmcp keyvault key create --subscription <subscription> \
                          --vault <vault-name> \
                          --key <key-name> \
                          --key-type <key-type>

# Lists keys in a key vault
azmcp keyvault key list --subscription <subscription> \
                        --vault <vault-name> \
                        --include-managed <true/false>

# Creates a secret in a key vault
azmcp keyvault secret create --subscription <subscription> \
                             --vault <vault-name> \
                             --name <secret-name> \
                             --value <secret-value>

# Lists secrets in a key vault
azmcp keyvault secret list --subscription <subscription> \
                           --vault <vault-name>
```

### Azure Kubernetes Service (AKS) Operations

```bash
# Get details of a specific AKS cluster
azmcp aks cluster get --subscription <subscription> \
                      --name <cluster>

# List AKS clusters in a subscription
azmcp aks cluster list --subscription <subscription>
```

### Azure Load Testing Operations

```bash
# Create load test
azmcp loadtesting test create --subscription <subscription> \
                              --resource-group <resource-group> \
                              --test-resource-name <test-resource-name> \
                              --test-id <test-id> \
                              --display-name <display-name> \
                              --description <description> \
                              --endpoint <endpoint> \
                              --virtual-users <virtual-users> \
                              --duration <duration> \
                              --ramp-up-time <ramp-up-time>

# Get load test
azmcp loadtesting test get --subscription <subscription> \
                           --resource-group <resource-group> \
                           --test-resource-name <test-resource-name> \
                           --test-id <test-id>

# Create load test resources
azmcp loadtesting testresource create --subscription <subscription> \
                                      --resource-group <resource-group> \
                                      --test-resource-name <test-resource-name>

# List load test resources
azmcp loadtesting testresource list --subscription <subscription> \
                                    --resource-group <resource-group> \
                                    --test-resource-name <test-resource-name>

# Create load test run
azmcp loadtesting testrun create --subscription <subscription> \
                                 --resource-group <resource-group> \
                                 --test-resource-name <test-resource-name> \
                                 --test-id <test-id> \
                                 --testrun-id <testrun-id> \
                                 --display-name <display-name> \
                                 --description <description> \
                                 --old-testrun-id <old-testrun-id>

# Get load test run
azmcp loadtesting testrun get --subscription <subscription> \
                              --resource-group <resource-group> \
                              --test-resource-name <test-resource-name> \
                              --testrun-id <testrun-id>

# List load test run
azmcp loadtesting testrun list --subscription <subscription> \
                               --resource-group <resource-group> \
                               --test-resource-name <test-resource-name> \
                               --test-id <test-id>

# Update load test run
azmcp loadtesting testrun update --subscription <subscription> \
                                 --resource-group <resource-group> \
                                 --test-resource-name <test-resource-name> \
                                 --test-id <test-id> \
                                 --testrun-id <testrun-id> \
                                 --display-name <display-name> \
                                 --description <description>
```

### Azure Managed Grafana Operations

```bash
# List Azure Managed Grafana
azmcp grafana list --subscription <subscription>
```

### Azure Marketplace Operations

```bash
# List marketplace products available to a subscription
azmcp marketplace product list --subscription <subscription> \
                               [--language <language-code>] \
                               [--search <terms>] \
                               [--filter <odata-filter>] \
                               [--orderby <odata-orderby>] \
                               [--select <odata-select>] \
                               [--expand <odata-expand>] \
                               [--next-cursor <pagination-cursor>]

# Get details about an Azure Marketplace product
azmcp marketplace product get --subscription <subscription> \
                              --product-id <product-id> \
                              [--include-stop-sold-plans <true/false>] \
                              [--language <language-code>] \
                              [--market <market-code>] \
                              [--lookup-offer-in-tenant-level <true/false>] \
                              [--plan-id <plan-id>] \
                              [--sku-id <sku-id>] \
                              [--include-service-instruction-templates <true/false>] \
                              [--pricing-audience <pricing-audience>]
```

### Azure MCP Best Practices

```bash
# Get best practices for secure, production-grade Azure usage
azmcp bestpractices get --resource <resource> --action <action>

# Resource options:
#   general        - General Azure best practices
#   azurefunctions - Azure Functions specific best practices
#   static-web-app - Azure Static Web Apps specific best practices
#
# Action options:
#   all             - Best practices for both code generation and deployment (only for static-web-app)
#   code-generation - Best practices for code generation (for general and azurefunctions)
#   deployment      - Best practices for deployment (for general and azurefunctions)

```

### Azure MCP Tools

```bash
# List all available tools in the Azure MCP server
azmcp tools list
```

### Azure Monitor Operations

#### Log Analytics

```bash
# List tables in a Log Analytics workspace
azmcp monitor table list --subscription <subscription> \
                         --workspace <workspace> \
                         --resource-group <resource-group>

# List Log Analytics workspaces in a subscription
azmcp monitor workspace list --subscription <subscription>

# Query logs from Azure Monitor using KQL
azmcp monitor resource log query --subscription <subscription> \
                                 --resource-id <resource-id> \
                                 --table <table> \
                                 --query "<kql-query>" \
                                 [--hours <hours>] \
                                 [--limit <limit>]

azmcp monitor workspace log query --subscription <subscription> \
                                  --workspace <workspace> \
                                  --table <table> \
                                  --query "<kql-query>" \
                                  [--hours <hours>] \
                                  [--limit <limit>]

# Examples:
# Query logs from a specific table
azmcp monitor workspace log query --subscription <subscription> \
                                  --workspace <workspace> \
                                  --table "AppEvents_CL" \
                                  --query "| order by TimeGenerated desc"
```

#### Health Models

```bash
# Get the health of an entity
azmcp monitor healthmodels entity gethealth --subscription <subscription> \
                                            --resource-group <resource-group> \
                                            --health-model <health-model-name> \
                                            --entity <entity-id>
```

#### Metrics

```bash
# List available metric definitions for a resource
azmcp monitor metrics definitions --subscription <subscription> \
                                  --resource <resource> \
                                  [--resource-group <resource-group>] \
                                  [--resource-type <resource-type>] \
                                  [--metric-namespace <metric-namespace>] \
                                  [--search-string <search-string>] \
                                  [--limit <limit>]

# Query Azure Monitor metrics for a resource
azmcp monitor metrics query --subscription <subscription> \
                            --resource <resource> \
                            --metric-namespace <metric-namespace> \
                            --metric-names <metric-names> \
                            [--resource-group <resource-group>] \
                            [--resource-type <resource-type>] \
                            [--start-time <start-time>] \
                            [--end-time <end-time>] \
                            [--interval <interval>] \
                            [--aggregation <aggregation>] \
                            [--filter <filter>] \
                            [--max-buckets <max-buckets>]

# Examples:
# List all available metrics for a storage account
azmcp monitor metrics definitions --subscription <subscription> \
                                  --resource <resource> \
                                  --resource-type "Microsoft.Storage/storageAccounts"

# Find metrics related to transactions
azmcp monitor metrics definitions --subscription <subscription> \
                                  --resource <resource> \
                                  --search-string "transaction"

# Query CPU and memory metrics for a virtual machine
azmcp monitor metrics query --subscription <subscription> \
                            --resource <resource> \
                            --resource-group <resource-group> \
                            --metric-namespace "microsoft.compute/virtualmachines" \
                            --resource-type "Microsoft.Compute/virtualMachines" \
                            --metric-names "Percentage CPU,Available Memory Bytes" \
                            --start-time "2024-01-01T00:00:00Z" \
                            --end-time "2024-01-01T23:59:59Z" \
                            --interval "PT1H" \
                            --aggregation "Average"
```

### Azure Managed Lustre

```bash
# List Azure Managed Lustre Filesystems available in a subscription or resource group
azmcp azuremanagedlustre filesystem list --subscription <subscription> \
                                      --resource-group <resource-group>

# Returns the required number of IP addresses for a specific Azure Managed Lustre SKU and filesystem size
azmcp azuremanagedlustre filesystem required-subnet-size --subscription <subscription> \
                                      --sku <azure-managed-lustre-sku> \
                                      --size <filesystem-size-in-tib>
```



### Azure Native ISV Operations

```bash
# List monitored resources in Datadog
azmcp datadog monitoredresources list --subscription <subscription> \
                                      --resource-group <resource-group> \
                                      --datadog-resource <datadog-resource>
```

### Azure Quick Review CLI Operations

```bash
# Scan a subscription for recommendations
azmcp extension azqr --subscription <subscription>

# Scan a subscription and scope to a specific resource group
azmcp extension azqr --subscription <subscription> \
                     --resource-group <resource-group-name>
```

### Azure Quota Operations

```bash
# Get the available regions for the resources types
azmcp quota region availability list --subscription <subscription> \
                                     --resource-types <resource-types> \
                                     [--cognitive-service-model-name <cognitive-service-model-name>] \
                                     [--cognitive-service-model-version <cognitive-service-model-version>] \
                                     [--cognitive-service-deployment-sku-name <cognitive-service-deployment-sku-name>]

# Check the usage for Azure resources type
azmcp quota usage check --subscription <subscription> \
                        --region <region> \
                        --resource-types <resource-types>
```

### Azure RBAC Operations

```bash
# List Azure RBAC role assignments
azmcp role assignment list --subscription <subscription> \
                           --scope <scope>
```

### Azure Redis Operations

```bash

# Lists Databases in an Azure Redis Cluster
azmcp redis cluster database list --subscription <subscription> \
                                  --resource-group <resource-group> \
                                  --cluster <cluster>

# Lists Redis Clusters in the Azure Managed Redis or Azure Redis Enterprise services
azmcp redis cluster list --subscription <subscription>

# Lists Redis Caches in the Azure Cache for Redis service
azmcp redis cache list --subscription <subscription>

# Lists Access Policy Assignments in an Azure Redis Cache
azmcp redis cache list accesspolicy --subscription <subscription> \
                                    --resource-group <resource-group> \
                                    --cache <cache-name>
```

### Azure Resource Group Operations

```bash
# List resource groups in a subscription
azmcp group list --subscription <subscription>
```

### Azure Resource Health Operations

```bash
# Get availability status for a specific resource
azmcp resourcehealth availability-status get --resourceId <resource-id>

# List availability statuses for all resources in a subscription
azmcp resourcehealth availability-status list --subscription <subscription> \
                                              [--resource-group <resource-group>]

# List service health events in a subscription
azmcp resourcehealth service-health-events list --subscription <subscription> \
                                                [--event-type <event-type>] \
                                                [--status <status>] \
                                                [--query-start-time <start-time>] \
                                                [--query-end-time <end-time>]
```

### Azure Service Bus Operations

```bash
# Returns runtime and details about the Service Bus queue
azmcp servicebus queue details --subscription <subscription> \
                               --namespace <service-bus-namespace> \
                               --queue <queue>

# Gets runtime details a Service Bus topic
azmcp servicebus topic details --subscription <subscription> \
                               --namespace <service-bus-namespace> \
                               --topic <topic>

# Gets runtime details and message counts for a Service Bus subscription
azmcp servicebus topic subscription details --subscription <subscription> \
                                            --namespace <service-bus-namespace> \
                                            --topic <topic> \
                                            --subscription-name <subscription-name>
```

### Azure SQL Database Operations

```bash
# Gets a list of all databases in a SQL server
azmcp sql db list --subscription <subscription> \
                  --resource-group <resource-group> \
                  --server <server-name>

# Show details of a specific SQL database
azmcp sql db show --subscription <subscription> \
                  --resource-group <resource-group> \
                  --server <server-name> \
                  --database <database>

# Create a firewall rule for a SQL server
azmcp sql server firewall-rule create --subscription <subscription> \
                                      --resource-group <resource-group> \
                                      --server <server-name> \
                                      --firewall-rule-name <rule-name> \
                                      --start-ip-address <start-ip> \
                                      --end-ip-address <end-ip>

# Delete a firewall rule from a SQL server
azmcp sql server firewall-rule delete --subscription <subscription> \
                                      --resource-group <resource-group> \
                                      --server <server-name> \
                                      --firewall-rule-name <rule-name>

# Gets a list of firewall rules for a SQL server
azmcp sql server firewall-rule list --subscription <subscription> \
                                  --resource-group <resource-group> \
                                  --server <server-name>
```

### Azure SQL Elastic Pool Operations

```bash
# List all elastic pools in a SQL server
azmcp sql elastic-pool list --subscription <subscription> \
                            --resource-group <resource-group> \
                            --server <server-name>
```

### Azure SQL Server Operations

```bash
# List Microsoft Entra ID administrators for a SQL server
azmcp sql server entra-admin list --subscription <subscription> \
                                  --resource-group <resource-group> \
                                  --server <server-name>
```

### Azure Storage Operations

```bash
# Create a new Storage account with custom configuration
azmcp storage account create --subscription <subscription> \
                             --account <unique-account-name> \
                             --resource-group <resource-group> \
                             --location <location> \
                             --sku <sku> \
                             --access-tier <access-tier> \
                             --enable-hierarchical-namespace false

# Get detailed information about a specific Storage account
azmcp storage account details --subscription <subscription> \
                              --account <account> \
                              [--tenant <tenant>]

# List Storage accounts in a subscription
azmcp storage account list --subscription <subscription>

# Set access tier for multiple blobs in a batch operation
azmcp storage blob batch set-tier --subscription <subscription> \
                                  --account <account> \
                                  --container <container> \
                                  --tier <tier> \
                                  --blobs <blob-name1> <blob-name2> ... <blob-nameN>

# Create a blob container with optional public access
azmcp storage blob container create --subscription <subscription> \
                                    --account <account> \
                                    --container <container>

# Get detailed properties of a storage container
azmcp storage blob container details --subscription <subscription> \
                                     --account <account> \
                                     --container <container>

# List containers in a Storage blob service
azmcp storage blob container list --subscription <subscription> \
                                  --account <account>

# Get detailed properties of a blob
azmcp storage blob details --subscription <subscription> \
                           --account <account> \
                           --container <container> \
                           --blob <blob>

# List blobs in a Storage container
azmcp storage blob list --subscription <subscription> \
                        --account <account> \
                        --container <container>

# Upload a file to a Storage blob container
azmcp storage blob upload --subscription <subscription> \
                          --account <account> \
                          --container <container> \
                          --blob <blob> \
                          --local-file-path <path-to-local-file>

# Create a directory in DataLake using a specific path
azmcp storage datalake directory create --subscription <subscription> \
                                        --account <account> \
                                        --directory-path <directory-path>

# List paths in a Data Lake file system
azmcp storage datalake file-system list-paths --subscription <subscription> \
                                              --account <account> \
                                              --file-system <file-system> \
                                              [--filter-path <filter-path>] \
                                              [--recursive]

# Send a message to a Storage queue
azmcp storage queue message send --subscription <subscription> \
                                 --account <account> \
                                 --queue <queue> \
                                 --message "<message>" \
                                 [--time-to-live-in-seconds <seconds>] \
                                 [--visibility-timeout-in-seconds <seconds>]

# List files and directories in a File Share directory
azmcp storage share file list --subscription <subscription> \
                              --account <account> \
                              --share <share> \
                              --directory-path <directory-path> \
                              [--prefix <prefix>]

# List tables in a Storage account
azmcp storage table list --subscription <subscription> \
                         --account <account>
```

### Azure Subscription Management

```bash
# List available Azure subscriptions
azmcp subscription list [--tenant-id <tenant-id>]
```

### Azure Terraform Best Practices

```bash
# Get secure, production-grade Azure Terraform best practices for effective code generation and command execution.
azmcp azureterraformbestpractices get
```

### Azure Virtual Desktop Operations

```bash
# List Azure Virtual Desktop host pools in a subscription
azmcp virtualdesktop hostpool list --subscription <subscription> \
                                   [--resource-group <resource-group>]

# List session hosts in a host pool
azmcp virtualdesktop hostpool sessionhost list --subscription <subscription> \
                                               [--hostpool <hostpool-name> | --hostpool-resource-id <hostpool-resource-id>] \
                                               [--resource-group <resource-group>]

# List user sessions on a session host
azmcp virtualdesktop hostpool sessionhost usersession-list --subscription <subscription> \
                                                           [--hostpool <hostpool-name> | --hostpool-resource-id <hostpool-resource-id>] \
                                                           --sessionhost <sessionhost-name> \
                                                           [--resource-group <resource-group>]
```

#### Resource Group Optimization

The Virtual Desktop commands support an optional `--resource-group` parameter that provides significant performance improvements when specified:

- **Without `--resource-group`**: Commands enumerate through all resources in the subscription
- **With `--resource-group`**: Commands directly access resources within the specified resource group, avoiding subscription-wide enumeration

**Host Pool List Usage:**

```bash
# Standard usage - enumerates all host pools in subscription
azmcp virtualdesktop hostpool list --subscription <subscription>

# Optimized usage - lists host pools in specific resource group only
azmcp virtualdesktop hostpool list --subscription <subscription> \
                                   --resource-group <resource-group>
```

**Session Host Usage patterns:**

```bash
# Standard usage - enumerates all host pools in subscription
azmcp virtualdesktop hostpool sessionhost list --subscription <subscription> \
                                                --hostpool <hostpool-name>

# Optimized usage - direct resource group access
azmcp virtualdesktop hostpool sessionhost list --subscription <subscription> \
                                                --hostpool <hostpool-name> \
                                                --resource-group <resource-group>

# Alternative with resource ID (no resource group needed)
azmcp virtualdesktop hostpool sessionhost list --subscription <subscription> \
                                                --hostpool-resource-id /subscriptions/<sub>/resourceGroups/<rg>/providers/Microsoft.DesktopVirtualization/hostPools/<pool>
```

### Azure Workbooks Operations

```bash
# Create a new workbook
azmcp workbooks create --subscription <subscription> \
                       --resource-group <resource-group> \
                       --display-name <display-name> \
                       --serialized-content <json-content> \
                       [--source-id <source-id>]

# Delete a workbook
azmcp workbooks delete --workbook-id <workbook-resource-id>

# List Azure Monitor workbooks in a resource group
azmcp workbooks list --subscription <subscription> \
                     --resource-group <resource-group> \
                     [--category <category>] \
                     [--kind <kind>] \
                     [--source-id <source-id>]

# Show details of a specific workbook by resource ID
azmcp workbooks show --workbook-id <workbook-resource-id>

# Update an existing workbook
azmcp workbooks update --workbook-id <workbook-resource-id> \
                       [--display-name <display-name>] \
                       [--serialized-content <json-content>]
```

### Bicep

```bash
# Get Bicep schema for a specific Azure resource type
azmcp bicepschema get --resource-type <resource-type> \
```

### Cloud Architect

```bash
# Design Azure cloud architectures through guided questions
azmcp cloudarchitect design [--question <question>] \
                            [--question-number <question-number>] \
                            [--total-questions <total-questions>] \
                            [--answer <answer>] \
                            [--next-question-needed <true/false>] \
                            [--confidence-score <confidence-score>] \
                            [--architecture-component <architecture-component>]

# Example:
# Start an interactive architecture design session
azmcp cloudarchitect design --question "What type of application are you building?" \
                            --question-number 1 \
                            --total-questions 5 \
                            --confidence-score 0.1
```

## Response Format

All responses follow a consistent JSON format:

```json
{
  "status": "200|403|500, etc",
  "message": "",
  "options": [],
  "results": [],
  "duration": 123
}
```

## Error Handling

The CLI returns structured JSON responses for errors, including:

- Service availability issues
- Authentication errors

