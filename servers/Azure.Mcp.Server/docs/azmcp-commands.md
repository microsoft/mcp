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

#### Server Start Command Options

The `azmcp server start` command supports the following options:

| Option | Required | Default | Description |
|--------|----------|---------|-------------|
| `--transport` | No | `stdio` | Transport mechanism to use (currently only `stdio` is supported) |
| `--mode` | No | `namespace` | Server mode: `namespace` (default), `all`, or `single` |
| `--namespace` | No | All namespaces | Specific Azure service namespaces to expose (can be repeated) |
| `--read-only` | No | `false` | Only expose read-only operations |
| `--debug` | No | `false` | Enable verbose debug logging to stderr |
| `--insecure-disable-elicitation` | No | `false` | **⚠️ INSECURE**: Disable user consent prompts for sensitive operations |

> **⚠️ Security Warning for `--insecure-disable-elicitation`:**
>
> This option disables user confirmations (elicitations) before running tools that read sensitive data. When enabled:
> - Tools that handle secrets, credentials, or sensitive data will execute without user confirmation
> - This removes an important security layer designed to prevent unauthorized access to sensitive information
> - Only use this option in trusted, automated environments where user interaction is not possible
> - Never use this option in production environments or when handling untrusted input
>
> **Example usage (use with caution):**
> ```bash
> # For automated scenarios only - bypasses security prompts
> azmcp server start --insecure-disable-elicitation
> ```

### Azure AI Foundry Operations

```bash

# Connect to an agent in an AI Foundry project and query it
azmcp foundry agents connect --agent-id <agent-id> \
                             --query <query> \
                             --endpoint <endpoint>

# Evaluate a response from an agent by passing query and response inline
azmcp foundry agents evaluate --agent-id <agent-id> \
                              --query <query> \
                              --response <response> \
                              --evaluator <evaluator> \
                              --azure-openai-endpoint <azure-openai-endpoint> \
                              --azure-openai-deployment <azure-openai-deployment> \
                              [--tool-definitions <tool-definitions>]

# Query and evaluate an agent in one command
azmcp foundry agents query-and-evaluate --agent-id <agent-id> \
                                        --query <query> \
                                        --endpoint <endpoint> \
                                        --azure-openai-endpoint <azure-openai-endpoint> \
                                        --azure-openai-deployment <azure-openai-deployment> \
                                        [--evaluators <evaluators>]

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

# Generate text completions using deployed Azure OpenAI models in AI Foundry
azmcp foundry openai create-completion --subscription <subscription> \
                                       --resource-group <resource-group> \
                                       --resource-name <resource-name> \
                                       --deployment <deployment-name> \
                                       --prompt-text <prompt-text> \
                                       [--max-tokens <max-tokens>] \
                                       [--temperature <temperature>]

# Create interactive chat completions using Azure OpenAI chat models
azmcp foundry openai chat-completions-create --subscription <subscription> \
                                             --resource-group <resource-group> \
                                             --resource-name <resource-name> \
                                             --deployment <deployment-name> \
                                             --message-array <message-array> \
                                             [--max-tokens <max-tokens>] \
                                             [--temperature <temperature>] \
                                             [--top-p <top-p>] \
                                             [--frequency-penalty <frequency-penalty>] \
                                             [--presence-penalty <presence-penalty>] \
                                             [--stop <stop-sequences>] \
                                             [--stream <stream>] \
                                             [--seed <seed>] \
                                             [--user <user>] \
                                             [--auth-method <auth-method>]

# Generate vector embeddings for text using Azure OpenAI embedding models
azmcp foundry openai embeddings-create --subscription <subscription> \
                                       --resource-group <resource-group> \
                                       --resource-name <resource-name> \
                                       --deployment <deployment-name> \
                                       --input-text <input-text> \
                                       [--user <user>] \
                                       [--encoding-format <encoding-format>] \
                                       [--dimensions <dimensions>] \
                                       [--auth-method <auth-method>]

# List all available OpenAI models and deployments in an Azure resource
azmcp foundry openai models-list --subscription <subscription> \
                                 --resource-group <resource-group> \
                                 --resource-name <resource-name> \
                                 [--auth-method <auth-method>]
```

### Azure AI Search Operations

```bash
# Get detailed properties of AI Search indexes
azmcp search index get --service <service> \
                       [--index <index>]

# Query AI Search index
azmcp search index query --subscription <subscription> \
                         --service <service> \
                         --index <index> \
                         --query <query>

# List AI Search accounts in a subscription
azmcp search list --subscription <subscription>
```

### Azure AI Services Speech Operations

```bash
# Recognize speech from an audio file using Azure AI Services Speech
azmcp speech stt recognize --endpoint <endpoint> \
                           --file <file-path> \
                           [--language <language>] \
                           [--phrases <phrase-hints>] \
                           [--format <simple|detailed>] \
                           [--profanity <masked|removed|raw>]
```

#### Phrase Hints for Improved Accuracy

The `--phrases` parameter supports multiple ways to specify phrase hints that improve speech recognition accuracy:

**Multiple Arguments:**
```bash
azmcp speech stt recognize --endpoint <endpoint> --file audio.wav \
    --phrases "Azure" --phrases "cognitive services" --phrases "machine learning"
```

**Comma-Separated Values:**
```bash
azmcp speech stt recognize --endpoint <endpoint> --file audio.wav \
    --phrases "Azure, cognitive services, machine learning"
```

**Mixed Syntax:**
```bash
azmcp speech stt recognize --endpoint <endpoint> --file audio.wav \
    --phrases "Azure, cognitive services" --phrases "machine learning"
```

Use phrase hints when you expect specific terminology, technical terms, or domain-specific vocabulary in your audio content. This significantly improves recognition accuracy for specialized content.

### Azure App Configuration Operations

```bash
# List App Configuration stores in a subscription
azmcp appconfig account list --subscription <subscription>

# Delete a key-value setting
azmcp appconfig kv delete --subscription <subscription> \
                          --account <account> \
                          --key <key> \
                          [--label <label>]

# Get key-value settings in an App Configuration store
azmcp appconfig kv get --subscription <subscription> \
                       --account <account> \
                       [--key <key>] \
                       [--label <label>] \
                       [--key-filter <key-filter>] \
                       [--label-filter <label-filter>]

# Lock (make it read-only) or unlock (remove read-only) a key-value setting 
azmcp appconfig kv lock set --subscription <subscription> \
                            --account <account> \
                            --key <key> \
                            [--label <label>] \
                            [--lock]

# Set a key-value setting
azmcp appconfig kv set --subscription <subscription> \
                       --account <account> \
                       --key <key> \
                       --value <value> \
                       [--label <label>]
```

### Azure App Lens Operations

```bash
# Diagnose resource using Azure App Lens
azmcp applens resource diagnose --subscription <subscription> \
                                --resource-group <resource-group> \
                                --question <question> \
                                --resource-type <resource-type> \
                                --resource <resource>
```

### Azure Application Insights Operations

#### Code Optimization Recommendations

```bash
# List code optimization recommendations across all Application Insights components in a subscription
azmcp applicationinsights recommendation list --subscription <subscription>

# Scope to a specific resource group
azmcp applicationinsights recommendation list --subscription <subscription> \
                                              --resource-group <resource-group>
### Azure App Service Operations

```bash
# Add a database connection to an App Service
azmcp appservice database add --subscription <subscription> \
                              --resource-group <resource-group> \
                              --app <app> \
                              --database-type <database-type> \
                              --database-server <database-server> \
                              --database <database> \
                              [--connection-string <connection-string>] \
                              [--tenant <tenant-id>]

# Examples:
# Add a SQL Server database connection
azmcp appservice database add --subscription "my-subscription" \
                              --resource-group "my-rg" \
                              --app "my-webapp" \
                              --database-type "SqlServer" \
                              --database-server "myserver.database.windows.net" \
                              --database "mydb"

# Add a MySQL database connection with custom connection string
azmcp appservice database add --subscription "my-subscription" \
                              --resource-group "my-rg" \
                              --app "my-webapp" \
                              --database-type "MySQL" \
                              --database-server "myserver.mysql.database.azure.com" \
                              --database "mydb" \
                              --connection-string "Server=myserver.mysql.database.azure.com;Database=mydb;Uid=myuser;Pwd=mypass;"

# Add a PostgreSQL database connection
azmcp appservice database add --subscription "my-subscription" \
                              --resource-group "my-rg" \
                              --app "my-webapp" \
                              --database-type "PostgreSQL" \
                              --database-server "myserver.postgres.database.azure.com" \
                              --database "mydb"

# Add a Cosmos DB connection
azmcp appservice database add --subscription "my-subscription" \
                              --resource-group "my-rg" \
                              --app "my-webapp" \
                              --database-type "CosmosDB" \
                              --database-server "myaccount" \
                              --database "mydb"
```

**Database Types Supported:**

-   `SqlServer` - Azure SQL Database
-   `MySQL` - Azure Database for MySQL
-   `PostgreSQL` - Azure Database for PostgreSQL
-   `CosmosDB` - Azure Cosmos DB

**Parameters:**

-   `--subscription`: Azure subscription ID (required)
-   `--resource-group`: Resource group containing the App Service (required)
-   `--app`: Name of the App Service web app (required)
-   `--database-type`: Type of database - SqlServer, MySQL, PostgreSQL, or CosmosDB (required)
-   `--database-server`: Database server name or endpoint (required)
-   `--database`: Name of the database (required)
-   `--connection-string`: Custom connection string (optional - auto-generated if not provided)
-   `--tenant`: Azure tenant ID for authentication (optional)

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

### Azure Communication Services Operations

```bash
# Send SMS message using Azure Communication Services
azmcp communication sms send \
    --connection-string "<connection-string>" \
    --from "<sender-phone-number>" \
    --to "<recipient-phone-number>" \
    --message "<message-text>" \
    [--enable-delivery-report] \
    [--tag "<custom-tag>"]

# Examples:
# Send SMS to single recipient
azmcp communication sms send \
    --connection-string "endpoint=https://mycomms.communication.azure.com/;accesskey=..." \
    --from "+14255550123" \
    --to "+14255550124" \
    --message "Hello from Azure Communication Services!"

# Send SMS to multiple recipients with delivery reporting
azmcp communication sms send \
    --connection-string "endpoint=https://mycomms.communication.azure.com/;accesskey=..." \
    --from "+14255550123" \
    --to "+14255550124,+14255550125" \
    --message "Broadcast message" \
    --enable-delivery-report \
    --tag "marketing-campaign"
```

**Options:**
-   `--connection-string`: Azure Communication Services connection string (required)
-   `--from`: SMS-enabled phone number in E.164 format (required)
-   `--to`: Recipient phone number(s) in E.164 format, comma-separated for multiple recipients (required)
-   `--message`: SMS message content (required)
-   `--enable-delivery-report`: Enable delivery reporting for the SMS message (optional)
-   `--tag`: Custom tag for message tracking (optional)


### Azure Confidential Ledger Operations

```bash
# Append a tamper-proof entry to a Confidential Ledger
azmcp confidentialledger entries append --ledger <ledger-name> \
                                        --content <json-or-text-data> \
                                        [--collection-id <collection-id>]
```

**Options:**
-   `--ledger`: Confidential Ledger name (required)
-   `--content`: JSON or text data to insert into the ledger (required)
-   `--collection-id`: Collection ID to store the data with (optional)

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

#### Database

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

#### Table

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

#### Server

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

#### Database

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

#### Table

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

#### Server

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


# List all Event Grid subscriptions in a subscription, resource group, or topic
azmcp eventgrid subscription list --subscription <subscription> \
                                  [--resource-group <resource-group>] \
                                  [--topic <topic>]
                                  [--location <location>]

# Publish custom events to Event Grid topics
azmcp eventgrid events publish --subscription <subscription> \
                               --topic <topic> \
                               --data <json-event-data> \
                               [--resource-group <resource-group>] \
                               [--schema <schema-type>]
```

### Azure Event Hubs

```bash
# Get detailed properties of an Event Hubs namespace
azmcp eventhubs namespace get --subscription <subscription> \
                              --namespace <namespace> \
                              --resource-group <resource-group>
```

### Azure Function App Operations

```bash
# Get detailed properties of function apps
azmcp functionapp get --subscription <subscription> \
                      [--resource-group <resource-group>] \
                      [--function-app <function-app-name>]
```

### Azure Key Vault Operations

#### Administration

```bash
# Gets Key Vault Managed HSM account settings
azmcp keyvault admin settings get --subscription <subscription> \
                                  --vault <vault-name>
```

#### Certificates

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
```

#### Keys

```bash
# Creates a key in a key vault
azmcp keyvault key create --subscription <subscription> \
                          --vault <vault-name> \
                          --key <key-name> \
                          --key-type <key-type>

# Get a key in a key vault
azmcp keyvault key get --subscription <subscription> \
                       --vault <vault-name> \
                       --key <key-name>

# Lists keys in a key vault
azmcp keyvault key list --subscription <subscription> \
                        --vault <vault-name> \
                        --include-managed <true/false>
```

#### Secrets

Tools that handle sensitive data such as secrets require user consent before execution through a security mechanism called **elicitation**. When you run commands that access sensitive information, the MCP client will prompt you to confirm the operation before proceeding.

> **🛡️ Elicitation (user confirmation) Security Feature:**
> 
> Elicitation prompts appear when tools may expose sensitive information like:
> - Key Vault secrets
> - Connection strings and passwords
> - Certificate private keys
> - Other confidential data
>
> These prompts protect against unauthorized access to sensitive information. You can bypass elicitation in automated scenarios using the `--insecure-disable-elicitation` server start option, but this should only be used in trusted environments.

```bash
# Creates a secret in a key vault (will prompt for user consent)
azmcp keyvault secret create --subscription <subscription> \
                             --vault <vault-name> \
                             --name <secret-name> \
                             --value <secret-value>

# Get a secret in a key vault (will prompt for user consent)
azmcp keyvault secret get --subscription <subscription> \
                          --vault <vault-name> \
                          --secret <secret-name>

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

# Get details of a specific AKS nodepool
azmcp aks nodepool get --subscription <subscription> \
                       --resource-group <resource-group> \
                       --cluster <cluster> \
                       --nodepool <nodepool>

# List AKS cluster's nodepools
azmcp aks nodepool list --subscription <subscription> \
                        --resource-group <resource-group> \
                        --cluster <cluster>
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

# List only the available top-level service namespaces
azmcp tools list --namespaces
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

# Create an Azure Managed Lustre filesystem
azmcp azuremanagedlustre filesystem create --subscription <subscription> \
                                           --sku <sku> \
                                           --size <filesystem-size-in-tib> \
                                           --subnet-id <subnet-id> \
                                           --zone <zone> \
                                           --maintenance-day <maintenance-day> \
                                           --maintenance-time <maintenance-time> \
                                           [--hsm-container <hsm-container>] \
                                           [--hsm-log-container <hsm-log-container>] \
                                           [--import-prefix <import-prefix>] \
                                           [--root-squash-mode <root-squash-mode>] \
                                           [--no-squash-nid-list <no-squash-nid-list>] \
                                           [--squash-uid <squash-uid>] \
                                           [--squash-gid <squash-gid>] \
                                           [--custom-encryption] \
                                           [--key-url <key-url>] \
                                           [--source-vault <source-vault>] \
                                           [--user-assigned-identity-id <user-assigned-identity-id>]

# Update an existing Azure Managed Lustre filesystem
azmcp azuremanagedlustre filesystem update --subscription <subscription> \
                                           --resource-group <resource-group> \
                                           --name <filesystem-name> \
                                           [--maintenance-day <maintenance-day>] \
                                           [--maintenance-time <HH:mm>] \
                                           [--root-squash-mode <mode>] \
                                           [--no-squash-nid-list <nid1,nid2,...>] \
                                           [--squash-uid <uid>] \
                                           [--squash-gid <gid>]

# Returns the required number of IP addresses for a specific Azure Managed Lustre SKU and filesystem size
azmcp azuremanagedlustre filesystem subnetsize ask --subscription <subscription> \
                                                   --sku <azure-managed-lustre-sku> \
                                                   --size <filesystem-size-in-tib>

# Checks if a subnet has enough available IP addresses for the specified Azure Managed Lustre SKU and filesystem size
azmcp azuremanagedlustre filesystem subnetsize validate --subscription <subscription> \
                                                        --subnet-id <subnet-resource-id> \
                                                        --sku <azure-managed-lustre-sku> \
                                                        --size <filesystem-size-in-tib> \
                                                        --location <filesystem-location>

# Lists the available Azure Managed Lustre SKUs in a specific location
azmcp azuremanagedlustre filesystem sku get --subscription <subscription> \
                                            --location <location>
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

### Azure SQL Operations

#### Database

```bash
# Create a SQL database (supports optional performance and configuration parameters)
azmcp sql db create --subscription <subscription> \
                    --resource-group <resource-group> \
                    --server <server-name> \
                    --database <database-name> \
                    [--sku-name <sku-name>] \
                    [--sku-tier <sku-tier>] \
                    [--sku-capacity <capacity>] \
                    [--collation <collation>] \
                    [--max-size-bytes <bytes>] \
                    [--elastic-pool-name <elastic-pool-name>] \
                    [--zone-redundant <true/false>] \
                    [--read-scale <Enabled|Disabled>]

# Delete a SQL database (idempotent – succeeds even if the database does not exist)
azmcp sql db delete --subscription <subscription> \
                    --resource-group <resource-group> \
                    --server <server-name> \
                    --database <database-name>

# Gets a list of all databases in a SQL server
azmcp sql db list --subscription <subscription> \
                  --resource-group <resource-group> \
                  --server <server-name>
                  
# Rename an existing SQL database to a new name within the same server
azmcp sql db rename --subscription <subscription> \
                    --resource-group <resource-group> \
                    --server <server-name> \
                    --database <current-database-name> \
                    --new-database-name <new-database-name>

# Show details of a specific SQL database
azmcp sql db show --subscription <subscription> \
                  --resource-group <resource-group> \
                  --server <server-name> \
                  --database <database>

# Update an existing SQL database (applies only the provided configuration changes)
azmcp sql db update --subscription <subscription> \
                    --resource-group <resource-group> \
                    --server <server-name> \
                    --database <database-name> \
                    [--sku-name <sku-name>] \
                    [--sku-tier <sku-tier>] \
                    [--sku-capacity <capacity>] \
                    [--collation <collation>] \
                    [--max-size-bytes <bytes>] \
                    [--elastic-pool-name <elastic-pool-name>] \
                    [--zone-redundant <true/false>] \
                    [--read-scale <Enabled|Disabled>]
```

#### Elastic Pool

```bash
# List all elastic pools in a SQL server
azmcp sql elastic-pool list --subscription <subscription> \
                            --resource-group <resource-group> \
                            --server <server-name>
```

#### Server

```bash
# Create a new SQL server
azmcp sql server create --subscription <subscription> \
                        --resource-group <resource-group> \
                        --server <server-name> \
                        --location <location> \
                        --admin-user <admin-username> \
                        --admin-password <admin-password> \
                        [--version <server-version>] \
                        [--public-network-access <enabled|disabled>]

# List Microsoft Entra ID administrators for a SQL server
azmcp sql server entra-admin list --subscription <subscription> \
                                  --resource-group <resource-group> \
                                  --server <server-name>

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

# Delete a SQL server
azmcp sql server delete --subscription <subscription> \
                        --resource-group <resource-group> \
                        --server <server-name>

# List SQL servers in a resource group
azmcp sql server list --subscription <subscription> \
                      --resource-group <resource-group>

# Show details of a specific SQL server
azmcp sql server show --subscription <subscription> \
                      --resource-group <resource-group> \
                      --server <server-name>
```

### Azure Storage Operations

#### Account

```bash
# Create a new Storage account with custom configuration
azmcp storage account create --subscription <subscription> \
                             --account <unique-account-name> \
                             --resource-group <resource-group> \
                             --location <location> \
                             --sku <sku> \
                             --access-tier <access-tier> \
                             --enable-hierarchical-namespace false

# Get detailed properties of Storage accounts
azmcp storage account get --subscription <subscription> \
                              [--account <account>] \
                              [--tenant <tenant>]
```

#### Blob Storage

```bash
# Create a blob container with optional public access
azmcp storage blob container create --subscription <subscription> \
                                    --account <account> \
                                    --container <container>

# Get detailed properties of Storage containers
azmcp storage blob container get --subscription <subscription> \
                                     --account <account> \
                                     [--container <container>]

# Get detailed properties of Storage blobs
azmcp storage blob get --subscription <subscription> \
                           --account <account> \
                           --container <container> \
                           [--blob <blob>]

# Upload a file to a Storage blob
azmcp storage blob upload --subscription <subscription> \
                          --account <account> \
                          --container <container> \
                          --blob <blob> \
                          --local-file-path <path-to-local-file>
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

### Tool and Namespace Result Objects

When invoking `azmcp tools list` (with or without `--namespaces`), each returned object now includes a `count` field:

| Field | Description |
|-------|-------------|
| `name` | Command or namespace name |
| `description` | Human-readable description |
| `command` | Fully qualified CLI invocation path |
| `subcommands` | (Namespaces only) Array of leaf command objects |
| `option` | (Leaf commands only) Array of options supported by the command |
| `count` | Namespaces: number of subcommands; Leaf commands: always 0 (options not counted) |

This quantitative field enables quick sizing of a namespace without traversing nested arrays. Leaf command complexity should be inferred from its option list, not the `count` field.

## Error Handling

The CLI returns structured JSON responses for errors, including:

- Service availability issues
- Authentication errors
