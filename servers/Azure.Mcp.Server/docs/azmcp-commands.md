# Azure MCP CLI Command Reference

> [!IMPORTANT]
> The Azure MCP Server has two modes: MCP Server mode and CLI mode.  When you start the MCP Server with `azmcp server start` that will expose an endpoint for MCP Client communication. The `azmcp` CLI also exposes all of the tools via a command line interface, i.e. `azmcp subscription list`.  In this document, "command" refers to CLI commands (e.g., `azmcp storage account list`), while "tool" refers to MCP server tools that can be invoked by MCP clients.

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

#### Using azmcp locally vs in container images

The commands in this document assume you are running the **`azmcp` CLI locally**
for example as a .NET global tool. In that case the executable is called
`azmcp` and commands such as:

```bash
azmcp server start --mode namespace --transport=stdio
```

are valid.

When you run the **Azure MCP Server container image** `mcr.microsoft.com/azure-sdk/azure-mcp` (for example in Azure Container Apps),
the image already contains an entrypoint that starts the MCP server process.
The image does **not** support overriding the container command with `azmcp ...` directly, as the entrypoint is already configured to start the server.
- Do **not** override the container command / entrypoint with `azmcp ...` when
  deploying the image. Doing so will cause the container to fail to start.
- Leave the command / entrypoint blank in Azure Container Apps so the default
  image entrypoint is used.
- If you need to customize the startup command or add extra arguments, build
  your own image based on the Azure MCP Server and set the ENTRYPOINT and/or
  CMD values in your Dockerfile there. That way you control exactly how the
  server starts without replacing the upstream image entrypoint at runtime.

> [!NOTE]
> ENTRYPOINT defines the executable that always runs; CMD provides
> default arguments to that executable. Overriding the container command in
> many PaaS providers replaces the image's ENTRYPOINT/CMD behavior, which can
> break startup. The Azure MCP Server image ENTRYPOINT in the repository is:

```text
ENTRYPOINT ["./server-binary", "server", "start"]
```

Because the image sets a fixed entrypoint, passing a container command such
as `azmcp ...` will replace or conflict with that entrypoint. If you must
change startup behavior, create a small derived Dockerfile that modifies the
ENTRYPOINT/CMD as needed and deploy your custom image instead of overriding
the command in the PaaS UI.

For the exact Dockerfile used to build the image see:
https://github.com/microsoft/mcp/blob/main/Dockerfile

The remaining sections describe the different server modes that apply to both
the CLI and the container image entrypoint.

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

#### Specific Tool Filtering

Exposes only specific tools by name, providing the finest level of granularity. The `--namespace` and `--tool` options cannot be used together. Use multiple `--tool` parameters to include multiple tools. Using `--tool` automatically switches to `all` mode.

```bash
# Start MCP Server with default mode and only subscription and resource group tools
azmcp server start \
    --tool azmcp_subscription_list \
    --tool azmcp_group_list \
    [--transport <transport>] \
    [--read-only]

# Start MCP Server with all mode and essential storage management tools
azmcp server start \
    --mode all \
    --tool azmcp_storage_account_get \
    --tool azmcp_storage_account_create \
    --tool azmcp_storage_blob_get \
    [--transport <transport>] \
    [--read-only]
```

#### Consolidated Mode

Exposes carefully curated tools that group related Azure operations together based on common user workflows and tasks. This mode provides the optimal balance between discoverability and usability by organizing consolidated tools that combine multiple related operations.

Each consolidated tool groups operations that are commonly used together:
- **Resource management**: Groups operations by resource type and action (get, create, edit, delete)
- **Workflow-based**: Organizes tools around common tasks (deployment, monitoring, security)
- **Metadata-aligned**: Only groups commands with exactly the same toolMetadata values (destructive, idempotent, readOnly, etc.)

**Benefits:**
- **Better for AI agents**: Reduces decision complexity by presenting meaningful tool groupings
- **Optimized tool count**: Well under VS Code's 128-tool limit
- **Task-oriented**: Tools are named after user intents (e.g., `get_azure_databases_details`, `deploy_azure_resources_and_applications`)
- **Maintains functionality**: All individual commands are still accessible through the consolidated tools

```bash
# Start MCP Server with consolidated mode
azmcp server start \
    --mode consolidated \
    [--transport <transport>] \
    [--namespace <namespace>] \
    [--read-only]
```

**Configuration file location**: The consolidated tool definitions are maintained in `core/Azure.Mcp.Core/src/Areas/Server/Resources/consolidated-tools.json`. Each definition includes:
- Tool name and description optimized for AI agent selection
- List of mapped individual commands
- Matching toolMetadata (destructive, idempotent, readOnly, secret, etc.)

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
| `--transport` | No | `stdio` | Transport mechanism to use. Valid values: `stdio` (default, supported in all distributions) or `http` (supported only in the Docker image distribution and other builds with HTTP enabled; may not be available in local CLI builds). |
| `--mode` | No | `namespace` | Server mode: `namespace` (default), `consolidated`, `all`, or `single` |
| `--namespace` | No | All namespaces | Specific Azure service namespaces to expose (can be repeated). Works with all existing modes to filter tools. |
| `--tool` | No | All tools | Expose specific tools by name (e.g., 'azmcp_storage_account_get'). It automatically switches to `all` mode. It can't be used together with `--namespace`. |
| `--read-only` | No | `false` | Only expose read-only operations |
| `--debug` | No | `false` | Enable verbose debug logging to stderr |
| `--dangerously-disable-http-incoming-auth` | No | false | Dangerously disable HTTP incoming authentication |
| `--dangerously-disable-elicitation` | No | `false` | **⚠️ DANGEROUS**: Disable user consent prompts for sensitive operations |
| `--outgoing-auth-strategy` | No | `NotSet` | Outgoing authentication strategy for service requests. Valid values: `NotSet`, `UseHostingEnvironmentIdentity`, `UseOnBehalfOf`. |
| `--dangerously-write-support-logs-to-dir` | No | - | **⚠️ DANGEROUS**: Enables detailed debug-level logging for support and troubleshooting. Specify a folder path where log files will be created with timestamp-based filenames. May include sensitive information in logs. |
| `--cloud` | No | `AzureCloud` | Azure cloud environment for authentication. Valid values: `AzureCloud` (default), `AzureChinaCloud`, `AzureUSGovernment`, or a custom authority host URL starting with `https://`. When a custom authority host URL is used, only the authentication authority host is changed; ARM and other service endpoints continue to use the Azure public cloud. |

> **⚠️ Security Warning for `--dangerously-disable-elicitation`:**
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
> azmcp server start --dangerously-disable-elicitation
> ```

> **⚠️ Security Warning for `--dangerously-write-support-logs-to-dir`:**
>
> This option enables detailed debug-level logging that may include sensitive information such as request payloads and authentication details. When enabled:
> - Log files are created in the specified directory with timestamp-based filenames (e.g., `azmcp_20251202_143052.log`)
> - Logs may contain sensitive data that could be useful for support troubleshooting
> - Only use this option when specifically requested by support for diagnosing issues
> - Remove log files after troubleshooting is complete
>
> **Example usage:**
> ```bash
> # For support troubleshooting only
> azmcp server start --dangerously-write-support-logs-to-dir /path/to/logs
> ```

> **Note on `--outgoing-auth-strategy`:**
>
> This option controls how the server authenticates when making requests to downstream Azure services:
> - `NotSet` (default): A safe default is chosen based on other settings
> - `UseHostingEnvironmentIdentity`: Uses the hosting environment's identity (similar to `DefaultAzureCredential`). All outgoing requests use the same identity regardless of the incoming request's identity
> - `UseOnBehalfOf`: Exchanges the incoming request's access token for a new token valid for the downstream service. Only valid when the server is running with HTTP transport and incoming HTTP authentication enabled (i.e., `--transport http` without `--dangerously-disable-http-incoming-auth`)

> **Note on `--cloud`:**
>
> Use this option to target sovereign cloud environments:
> - `AzureCloud` (default): Azure public cloud
> - `AzureChinaCloud`: Azure China (operated by 21Vianet)
> - `AzureUSGovernment`: Azure US Government
> - Custom URL: A custom authority host URL starting with `https://`
>
> **Example usage:**
> ```bash
> # Connect to Azure US Government cloud
> azmcp server start --cloud AzureUSGovernment
> ```

#### Server Info

```bash
# Get information about the MCP server, which includes the server's name and version.
azmcp server info
```

### Azure Advisor Operations

```bash
# List Advisor recommendations in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp advisor recommendations list --subscription <subscription>

# Apply Advisor recommendation to create or modify IaaC files (like ARM, Bicep) for Azure resources
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp advisor recommendations apply --resource <resource>
```

### Azure AI Search Operations

```bash
# Get detailed properties of AI Search indexes
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp search index get --service <service> \
                       [--index <index>]

# Query AI Search index
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp search index query --subscription <subscription> \
                         --service <service> \
                         --index <index> \
                         --query <query>

# Get AI Search knowledge bases (all or a specific one)
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp search knowledge base get --service <service>
                                [--knowledge-base <knowledge-base>]

# Run retrieval against an AI Search knowledge base
# ❌ Destructive | ✅ Idempotent | ✅ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp search knowledge base retrieve --service <service> \
                                     --knowledge-base <knowledge-base> \
                                     [--query <query>] \
                                     [--messages <messages>]

# Get AI Search knowledge sources (all or a specific one)
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp search knowledge source get --service <service>
                                  [--knowledge-source <knowledge-source>]

# List AI Search accounts in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp search service list --subscription <subscription>
```

### Azure AI Services Speech Operations

```bash
# Recognize speech from an audio file using Azure AI Services Speech
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ✅ LocalRequired
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
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp speech stt recognize --endpoint <endpoint> --file audio.wav \
    --phrases "Azure" --phrases "cognitive services" --phrases "machine learning"
```

**Comma-Separated Values:**
```bash
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp speech stt recognize --endpoint <endpoint> --file audio.wav \
    --phrases "Azure, cognitive services, machine learning"
```

**Mixed Syntax:**
```bash
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp speech stt recognize --endpoint <endpoint> --file audio.wav \
    --phrases "Azure, cognitive services" --phrases "machine learning"
```

Use phrase hints when you expect specific terminology, technical terms, or domain-specific vocabulary in your audio content. This significantly improves recognition accuracy for specialized content.

```bash
# Synthesize speech from text and save to an audio file using Azure AI Services Speech
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp speech tts synthesize --endpoint <endpoint> \
                            --text <text-to-synthesize> \
                            --outputAudio <output-file-path> \
                            [--language <language>] \
                            [--voice <voice-name>] \
                            [--format <audio-format>] \
                            [--endpointId <custom-voice-endpoint-id>]
```

#### Text-to-Speech Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `--endpoint` | Yes | Azure AI Services endpoint URL (e.g., https://your-service.cognitiveservices.azure.com/) |
| `--text` | Yes | The text to convert to speech |
| `--outputAudio` | Yes | Path where the synthesized audio file will be saved (e.g., output.wav, speech.mp3) |
| `--language` | No | Speech synthesis language (default: en-US). Examples: es-ES, fr-FR, de-DE |
| `--voice` | No | Neural voice to use (e.g., en-US-JennyNeural, es-ES-ElviraNeural). If not specified, default voice for the language is used |
| `--format` | No | Output audio format (default: Riff24Khz16BitMonoPcm). Supported formats: Riff24Khz16BitMonoPcm, Audio16Khz32KBitRateMonoMp3, Audio24Khz96KBitRateMonoMp3, Ogg16Khz16BitMonoOpus, Raw16Khz16BitMonoPcm |
| `--endpointId` | No | Endpoint ID of a custom voice model for personalized speech synthesis |

#### Supported Audio Formats

The `--format` parameter accepts the following values:

- **WAV formats**: `Riff24Khz16BitMonoPcm` (default), `Riff16Khz16BitMonoPcm`, `Raw16Khz16BitMonoPcm`
- **MP3 formats**: `Audio16Khz32KBitRateMonoMp3`, `Audio24Khz96KBitRateMonoMp3`, `Audio48Khz192KBitRateMonoMp3`
- **OGG/Opus formats**: `Ogg16Khz16BitMonoOpus`, `Ogg24Khz16BitMonoOpus`

**Examples:**

```bash
# Basic text-to-speech synthesis
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp speech tts synthesize --endpoint https://myservice.cognitiveservices.azure.com/ \
    --text "Hello, welcome to Azure AI Services Speech" \
    --outputAudio welcome.wav

# Synthesize with specific language and voice
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp speech tts synthesize --endpoint https://myservice.cognitiveservices.azure.com/ \
    --text "Hola, bienvenido a los servicios de voz de Azure" \
    --outputAudio spanish-greeting.wav \
    --language es-ES \
    --voice es-ES-ElviraNeural

# Generate MP3 output with high quality
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp speech tts synthesize --endpoint https://myservice.cognitiveservices.azure.com/ \
    --text "This is a high quality audio output" \
    --outputAudio output.mp3 \
    --format Audio48Khz192KBitRateMonoMp3

# Use custom voice model
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp speech tts synthesize --endpoint https://myservice.cognitiveservices.azure.com/ \
    --text "This uses my custom trained voice" \
    --outputAudio custom-voice.wav \
    --voice my-custom-voice-model
    --endpointId my-custom-voice-endpoint-id
```

### Azure App Configuration Operations

```bash
# List App Configuration stores in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appconfig account list --subscription <subscription>

# Delete a key-value setting
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appconfig kv delete --subscription <subscription> \
                          --account <account> \
                          --key <key> \
                          [--label <label>]

# Get key-value settings in an App Configuration store
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appconfig kv get --subscription <subscription> \
                       --account <account> \
                       [--key <key>] \
                       [--label <label>] \
                       [--key-filter <key-filter>] \
                       [--label-filter <label-filter>]

# Lock (make it read-only) or unlock (remove read-only) a key-value setting
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appconfig kv lock set --subscription <subscription> \
                            --account <account> \
                            --key <key> \
                            [--label <label>] \
                            [--lock]

# Set a key-value setting
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appconfig kv set --subscription <subscription> \
                       --account <account> \
                       --key <key> \
                       --value <value> \
                       [--label <label>] \
                       [--content-type <content-type>] \
                       [--tags <tags>]
```

### Azure App Lens Operations

```bash
# Diagnose resource using Azure App Lens
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
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
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp applicationinsights recommendation list --subscription <subscription>

# Scope to a specific resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp applicationinsights recommendation list --subscription <subscription> \
                                              --resource-group <resource-group>
```

### Azure App Service Operations

#### Databases

```bash
# Add a database connection to an App Service
# ❌ Destructive | ❌ Idempotent | ✅ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
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
# ❌ Destructive | ❌ Idempotent | ✅ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice database add --subscription "my-subscription" \
                              --resource-group "my-rg" \
                              --app "my-webapp" \
                              --database-type "SqlServer" \
                              --database-server "myserver.database.windows.net" \
                              --database "mydb"

# Add a MySQL database connection with custom connection string
# ❌ Destructive | ❌ Idempotent | ✅ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice database add --subscription "my-subscription" \
                              --resource-group "my-rg" \
                              --app "my-webapp" \
                              --database-type "MySQL" \
                              --database-server "myserver.mysql.database.azure.com" \
                              --database "mydb" \
                              --connection-string "Server=myserver.mysql.database.azure.com;Database=mydb;Uid=myuser;Pwd=mypass;"

# Add a PostgreSQL database connection
# ❌ Destructive | ❌ Idempotent | ✅ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice database add --subscription "my-subscription" \
                              --resource-group "my-rg" \
                              --app "my-webapp" \
                              --database-type "PostgreSQL" \
                              --database-server "myserver.postgres.database.azure.com" \
                              --database "mydb"

# Add a Cosmos DB connection
# ❌ Destructive | ❌ Idempotent | ✅ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
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

#### Web Apps

```bash
# Get App Service Web App details
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp get --subscription <subscription> \
                            [--resource-group <resource-group>] \
                            [--app <app>]

# Examples:
# List the App Service Web Apps details in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp get --subscription "my-subscription"

# List the App Service Web Apps details in a resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp get --subscription "my-subscription" \
                            --resource-group "my-resource-group"

# Get the details for a specific App Service Web App
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp get --subscription "my-subscription" \
                            --resource-group "my-resource-group" \
                            --app "my-app"
```

#### Web App Application Settings

```bash
# Get application settings for an App Service Web App
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp appservice webapp settings get-appsettings --subscription <subscription> \
                                                 --resource-group <resource-group> \
                                                 --app <app>

# Examples:
# Get the application settings for an App Service Web App
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp appservice webapp settings get-appsettings --subscription "my-subscription" \
                                                 --resource-group "my-resource-group" \
                                                 --app "my-app"
```

```bash
# Update application settings for an App Service Web App
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp settings update-appsettings --subscription <subscription> \
                                                    --resource-group <resource-group> \
                                                    --app <app> \
                                                    --setting-name <setting-name> \
                                                    --setting-update-type <add/set/delete> \
                                                    [--setting-value <setting-value>]

# Examples:
# Add the application setting 'foo' with value 'bar' to an App Service Web App
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp settings update-appsettings --subscription "my-subscription" \
                                                    --resource-group "my-resource-group" \
                                                    --app "my-app" \
                                                    --setting-name "foo" \
                                                    --setting-update-type "add" \
                                                    --setting-value "bar"

# Set the application setting 'fizz' with value 'buzz' to an App Service Web App
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp settings update-appsettings --subscription "my-subscription" \
                                                    --resource-group "my-resource-group" \
                                                    --app "my-app" \
                                                    --setting-name "fizz" \
                                                    --setting-update-type "set" \
                                                    --setting-value "buzz"

# Delete the application setting 'baz' from an App Service Web App
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp settings update-appsettings --subscription "my-subscription" \
                                                    --resource-group "my-resource-group" \
                                                    --app "my-app" \
                                                    --setting-name "baz" \
                                                    --setting-update-type "delete"
```

#### Web App Deployments

```bash
# Get the deployments for an App Service web app
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp deployment get --subscription <subscription> \
                                       --resource-group <resource-group> \
                                       --app <app> \
                                       [--deployment-id <deployment-id>]

# Examples:
# List the deployments for an App Service web app
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp deployment get --subscription "my-subscription" \
                                       --resource-group "my-resource-group" \
                                       --app "my-app"

# Get the deployment for an App Service web app
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp deployment get --subscription "my-subscription" \
                                       --resource-group "my-resource-group" \
                                       --app "my-app" \
                                       --deployment-id "deployment-id"
```

#### Web App Diagnostics

```bash
# List detectors for an App Service Web App
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp diagnostic list --subscription <subscription> \
                                        --resource-group <resource-group> \
                                        --app <app>

# Examples:
# List diagnostic detectors for an App Service Web App
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp diagnostic list --subscription "my-subscription" \
                                        --resource-group "my-resource-group" \
                                        --app "my-web-app"
```

```bash
# Diagnose an App Service Web App with detector
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp diagnostic diagnose --subscription <subscription> \
                                            --resource-group <resource-group> \
                                            --app <app> \
                                            --detector-name <detector-name> \
                                            [--start-time <start-time>] \
                                            [--end-time <end-time>] \
                                            [--interval <interval>]

# Examples:
# Diagnose the Web App with detector
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp diagnostic diagnose --subscription "my-subscription" \
                                            --resource-group "my-resource-group" \
                                            --app "my-web-app" \
                                            --detector-name "detector"

# Diagnose the Web App with detector between start and end time with interval
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp appservice webapp diagnostic diagnose --subscription "my-subscription" \
                                            --resource-group "my-resource-group" \
                                            --app "my-web-app" \
                                            --detector-name "detector"
                                            --start-time "2026-01-01T00:00:00Z" \
                                            --end-time "2026-01-01T23:59:59Z" \
                                            --interval "PT1H"
```

### Azure CLI Operations

#### Generate

```bash
# Generate an Azure CLI command based on user intent
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp extension cli generate --cli-type <cli-type>
                             --intent <intent>
```

#### Install

```bash
# Get installation instructions for Azure CLI, Azure Developer CLI or Azure Functions Core Tools CLI
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp extension cli install --cli-type <cli-type>
```

### Azure Communication Services Operations

#### Email

```bash
# Send email using Azure Communication Services
# ❌ Destructive | ❌ Idempotent | ✅ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp communication email send --endpoint <endpoint> \
                               --from <sender-email> \
                               --to <recipient-email> \
                               --subject <email-subject> \
                               --message <email-content> \
                               [--is-html] \
                               [--sender-name <sender-display-name>] \
                               [--cc <cc-recipient-email>] \
                               [--bcc <bcc-recipient-email>] \
                               [--reply-to <reply-to-email>]

# Examples:
# Send plain text email
# ❌ Destructive | ❌ Idempotent | ✅ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp communication email send --endpoint "https://mycomms.communication.azure.com" \
                               --from "sender@verified-domain.com" \
                               --to "recipient@example.com" \
                               --subject "Important message" \
                               --message "Hello from Azure Communication Services!"

# Send HTML-formatted email with CC and sender name
# ❌ Destructive | ❌ Idempotent | ✅ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp communication email send --endpoint "https://mycomms.communication.azure.com" \
                               --from "sender@verified-domain.com" \
                               --sender-name "Support Team" \
                               --to "recipient@example.com" \
                               --cc "manager@example.com" \
                               --subject "Monthly Report" \
                               --message "<h1>Monthly Report</h1><p>Please find attached your monthly report.</p>" \
                               --is-html

# Send to multiple recipients with BCC and reply-to
# ❌ Destructive | ❌ Idempotent | ✅ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp communication email send --endpoint "https://mycomms.communication.azure.com" \
                               --from "notifications@verified-domain.com" \
                               --to "recipient1@example.com,recipient2@example.com" \
                               --bcc "archive@example.com" \
                               --reply-to "support@example.com" \
                               --subject "System Notification" \
                               --message "This is an automated notification."
```

**Options:**
-   `--endpoint`: Azure Communication Services endpoint URL (required)
-   `--sender`: Email address to send from, must be from a verified domain (required)
-   `--to`: Recipient email address(es), comma-separated for multiple recipients (required)
-   `--subject`: Email subject line (required)
-   `--message`: Email content body (required)
-   `--is-html`: Flag indicating the message content is HTML format (optional)
-   `--sender-name`: Display name of the sender (optional)
-   `--cc`: Carbon copy recipient email address(es), comma-separated for multiple recipients (optional)
-   `--bcc`: Blind carbon copy recipient email address(es), comma-separated for multiple recipients (optional)
-   `--reply-to`: Reply-to email address(es), comma-separated for multiple addresses (optional)

#### SMS

```bash
# SMS message using Azure Communication Services
# ❌ Destructive | ❌ Idempotent | ✅ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp communication sms send --endpoint <endpoint> \
                             --from <sender-phone-number> \
                             --to <recipient-phone-number> \
                             --message <message-text> \
                             [--enable-delivery-report] \
                             [--tag <custom-tag>]

# Examples:
# Send SMS to single recipient
# ❌ Destructive | ❌ Idempotent | ✅ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp communication sms send --endpoint "https://mycomms.communication.azure.com" \
                             --from "+1234567890" \
                             --to "+1234567891" \
                             --message "Hello from Azure Communication Services!"

# Send SMS to multiple recipients with delivery reporting
# ❌ Destructive | ❌ Idempotent | ✅ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp communication sms send --endpoint "https://mycomms.communication.azure.com"
                             --from "+1234567890" \
                             --to "+1234567891,+1234567892" \
                             --message "Broadcast message" \
                             --enable-delivery-report \
                             --tag "marketing-campaign"
```

**Options:**
-   `--endpoint`: Azure Communication Services endpoint URL (required)
-   `--from`: SMS-enabled phone number in E.164 format (required)
-   `--to`: Recipient phone number(s) in E.164 format, comma-separated for multiple recipients (required)
-   `--message`: SMS message content (required)
-   `--enable-delivery-report`: Enable delivery reporting for the SMS message (optional)
-   `--tag`: Custom tag for message tracking (optional)


### Azure Compute Operations

#### Virtual Machines

```bash
# Get Virtual Machine(s) - behavior depends on provided parameters
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vm get --subscription <subscription> \
                     [--resource-group <resource-group>] \
                     [--vm-name <vm-name>] \
                     [--instance-view]

# Examples:

# List all VMs in subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vm get --subscription "my-subscription"

# List all VMs in a resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vm get --subscription "my-subscription" \
                     --resource-group "my-rg"

# Get specific VM details
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vm get --subscription "my-subscription" \
                     --resource-group "my-rg" \
                     --vm-name "my-vm"

# Get specific VM with instance view (runtime status)
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vm get --subscription "my-subscription" \
                     --resource-group "my-rg" \
                     --vm-name "my-vm" \
                     --instance-view
```

**Command Behavior:**
- **With `--vm-name`**: Gets detailed information about a specific VM (requires `--resource-group`). Optionally include `--instance-view` for runtime status.
- **With `--resource-group` only**: Lists all VMs in the specified resource group.
- **With neither**: Lists all VMs in the subscription.

**Returns:**
- VM information including name, location, VM size, provisioning state, OS type, license type, zones, and tags.
- When `--instance-view` is specified: Also includes power state, provisioning state, VM agent status, disk status, and extension status.

**Parameters:**
| Parameter | Required | Description |
|-----------|----------|-------------|
| `--subscription` | Yes | Azure subscription ID |
| `--resource-group`, `-g` | Conditional | Resource group name (required when using `--vm-name`) |
| `--vm-name`, `--name` | No | Name of the virtual machine |
| `--instance-view` | No | Include instance view details (only available with `--vm-name`) |

```bash
# Create Virtual Machine (automatically creates networking resources when missing)
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp compute vm create --subscription <subscription> \
                        --resource-group <resource-group> \
                        --vm-name <vm-name> \
                        --location <location> \
                        --admin-username <admin-username> \
                        [--admin-password <admin-password>] \
                        [--ssh-public-key <ssh-public-key>] \
                        [--vm-size <vm-size>] \
                        [--image <image>] \
                        [--os-type <os-type>] \
                        [--virtual-network <virtual-network>] \
                        [--subnet <subnet>] \
                        [--public-ip-address <public-ip-address>] \
                        [--network-security-group <network-security-group>] \
                        [--source-address-prefix <source-address-prefix>] \
                        [--no-public-ip] \
                        [--zone <zone>] \
                        [--os-disk-size-gb <os-disk-size-gb>] \
                        [--os-disk-type <os-disk-type>]

Defaults to the Azure CLI baseline of Standard_DS1_v2 size and the Ubuntu2404 image when not specified. When new NSG rules are created, SSH/RDP access is allowed from any source unless `--source-address-prefix` is provided.

# Examples:

# Create Linux VM with SSH key
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp compute vm create --subscription "my-subscription" \
                        --resource-group "my-rg" \
                        --vm-name "my-linux-vm" \
                        --location "eastus" \
                        --admin-username "azureuser" \
                        --ssh-public-key "ssh-ed25519 AAAAC3..."

# Create Windows VM with password
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp compute vm create --subscription "my-subscription" \
                        --resource-group "my-rg" \
                        --vm-name "my-win-vm" \
                        --location "eastus" \
                        --admin-username "adminuser" \
                        --admin-password "ComplexPassword123!" \
                        --image "Win2022Datacenter"

# Create VM with specific size and no public IP
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp compute vm create --subscription "my-subscription" \
                        --resource-group "my-rg" \
                        --vm-name "my-private-vm" \
                        --location "eastus" \
                        --admin-username "azureuser" \
                        --ssh-public-key "ssh-ed25519 AAAAC3..." \
                        --vm-size "Standard_D4s_v3" \
                        --no-public-ip
```

**Image Aliases:**
- Linux: `Ubuntu2404`, `Ubuntu2204`, `Ubuntu2004`, `Debian11`, `Debian12`, `RHEL9`, `CentOS8`
- Windows: `Win2022Datacenter`, `Win2019Datacenter`, `Win11Pro`, `Win10Pro`

**Parameters:**
| Parameter | Required | Description |
|-----------|----------|-------------|
| `--subscription` | Yes | Azure subscription ID |
| `--resource-group`, `-g` | Yes | Resource group name |
| `--vm-name` | Yes | Name of the virtual machine |
| `--location` | Yes | Azure region |
| `--admin-username` | Yes | Admin username |
| `--admin-password` | Conditional | Admin password (required for Windows, optional for Linux) |
| `--ssh-public-key` | Conditional | SSH public key (for Linux VMs) |
| `--vm-size` | No | VM size (default: Standard_DS1_v2) |
| `--image` | No | Image alias or URN (default: Ubuntu2404) |
| `--os-type` | No | OS type: 'linux' or 'windows' (auto-detected from image) |
| `--virtual-network` | No | Virtual network name |
| `--subnet` | No | Subnet name |
| `--public-ip-address` | No | Public IP address name |
| `--network-security-group` | No | Network security group name |
| `--source-address-prefix` | No | Source IP/CIDR for created NSG inbound rules (default: `*`) |
| `--no-public-ip` | No | Do not create a public IP address |
| `--zone` | No | Availability zone |
| `--os-disk-size-gb` | No | OS disk size in GB |
| `--os-disk-type` | No | OS disk type: 'Premium_LRS', 'StandardSSD_LRS', 'Standard_LRS' |

```bash
# Update Virtual Machine configuration
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vm update --subscription <subscription> \
                        --resource-group <resource-group> \
                        --vm-name <vm-name> \
                        [--vm-size <vm-size>] \
                        [--tags <tags>] \
                        [--license-type <license-type>] \
                        [--boot-diagnostics <boot-diagnostics>] \
                        [--user-data <user-data>]

# Examples:

# Add tags to a VM
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vm update --subscription "my-subscription" \
                        --resource-group "my-rg" \
                        --vm-name "my-vm" \
                        --tags "environment=prod,team=compute"

# Enable Azure Hybrid Benefit
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vm update --subscription "my-subscription" \
                        --resource-group "my-rg" \
                        --vm-name "my-vm" \
                        --license-type "Windows_Server"

# Enable boot diagnostics
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vm update --subscription "my-subscription" \
                        --resource-group "my-rg" \
                        --vm-name "my-vm" \
                        --boot-diagnostics "true"
```

**Parameters:**
| Parameter | Required | Description |
|-----------|----------|-------------|
| `--subscription` | Yes | Azure subscription ID |
| `--resource-group`, `-g` | Yes | Resource group name |
| `--vm-name` | Yes | Name of the virtual machine |
| `--vm-size` | No | New VM size (may require VM to be deallocated) |
| `--tags` | No | Tags in key=value,key2=value2 format |
| `--license-type` | No | License type: 'Windows_Server', 'RHEL_BYOS', 'SLES_BYOS', 'None' |
| `--boot-diagnostics` | No | Enable or disable boot diagnostics: 'true' or 'false' |
| `--user-data` | No | Base64-encoded user data |

```bash
# Delete a Virtual Machine
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp compute vm delete --subscription <subscription> \
                        --resource-group <resource-group> \
                        --vm-name <vm-name> \
                        [--force-deletion]

# Examples:

# Delete a VM
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp compute vm delete --subscription "my-subscription" \
                        --resource-group "my-rg" \
                        --vm-name "my-vm"

# Force delete a VM even if it is in a running or failed state
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp compute vm delete --subscription "my-subscription" \
                        --resource-group "my-rg" \
                        --vm-name "my-vm" \
                        --force-deletion
```

**Command Behavior:**
- Deletes the VM. Associated resources (disks, NICs, public IPs) are NOT automatically deleted.
- **With `--force-deletion`**: Passes `forceDeletion=true` to the Azure API, which force-deletes the VM even if it is in a running or failed state.

**Parameters:**
| Parameter | Required | Description |
|-----------|----------|-------------|
| `--subscription` | Yes | Azure subscription ID |
| `--resource-group`, `-g` | Yes | Resource group name |
| `--vm-name` | Yes | Name of the virtual machine to delete |
| `--force-deletion` | No | Force delete the VM even if running or failed (Azure API forceDeletion) |

#### Virtual Machine Scale Sets

```bash
# Get Virtual Machine Scale Set(s) - behavior depends on provided parameters
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vmss get --subscription <subscription> \
                       [--resource-group <resource-group>] \
                       [--vmss-name <vmss-name>] \
                       [--instance-id <instance-id>]

# Examples:

# List all VMSS in subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vmss get --subscription "my-subscription"

# List all VMSS in a resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vmss get --subscription "my-subscription" \
                       --resource-group "my-rg"

# Get specific VMSS details
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vmss get --subscription "my-subscription" \
                       --resource-group "my-rg" \
                       --vmss-name "my-vmss"

# Get specific VM instance in a VMSS
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vmss get --subscription "my-subscription" \
                       --resource-group "my-rg" \
                       --vmss-name "my-vmss" \
                       --instance-id "0"
```

**Command Behavior:**
- **With `--instance-id`**: Gets detailed information about a specific VM instance in the scale set (requires `--vmss-name` and `--resource-group`).
- **With `--vmss-name`**: Gets detailed information about a specific VMSS (requires `--resource-group`).
- **With `--resource-group` only**: Lists all VMSS in the specified resource group.
- **With neither**: Lists all VMSS in the subscription.

**Returns:**
- VMSS information including name, location, SKU, capacity, provisioning state, upgrade policy, overprovision setting, zones, and tags.
- When `--instance-id` is specified: Returns VM instance information including instance ID, name, location, VM size, provisioning state, OS type, zones, and tags.

**Parameters:**
| Parameter | Required | Description |
|-----------|----------|-------------|
| `--subscription` | Yes | Azure subscription ID |
| `--resource-group`, `-g` | Conditional | Resource group name (required when using `--vmss-name`) |
| `--vmss-name` | No | Name of the virtual machine scale set |
| `--instance-id` | No | Instance ID of the VM in the scale set (requires `--vmss-name`) |

```bash
# Create Virtual Machine Scale Set (automatically creates networking resources when missing)
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp compute vmss create --subscription <subscription> \
                          --resource-group <resource-group> \
                          --vmss-name <vmss-name> \
                          --location <location> \
                          --admin-username <admin-username> \
                          [--admin-password <admin-password>] \
                          [--ssh-public-key <ssh-public-key>] \
                          [--vm-size <vm-size>] \
                          [--image <image>] \
                          [--os-type <os-type>] \
                          [--virtual-network <virtual-network>] \
                          [--subnet <subnet>] \
                          [--instance-count <instance-count>] \
                          [--upgrade-policy <upgrade-policy>] \
                          [--zone <zone>] \
                          [--os-disk-size-gb <os-disk-size-gb>] \
                          [--os-disk-type <os-disk-type>]

Defaults to two Standard_DS1_v2 instances running Ubuntu2404 when size or image are not provided.

# Examples:

# Create Linux VMSS with SSH key
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp compute vmss create --subscription "my-subscription" \
                          --resource-group "my-rg" \
                          --vmss-name "my-vmss" \
                          --location "eastus" \
                          --admin-username "azureuser" \
                          --ssh-public-key "ssh-ed25519 AAAAC3..." \
                          --instance-count 3

# Create Windows VMSS with automatic upgrade policy
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp compute vmss create --subscription "my-subscription" \
                          --resource-group "my-rg" \
                          --vmss-name "my-win-vmss" \
                          --location "eastus" \
                          --admin-username "adminuser" \
                          --admin-password "ComplexPassword123!" \
                          --image "Win2022Datacenter" \
                          --upgrade-policy "Automatic"
```

**Parameters:**
| Parameter | Required | Description |
|-----------|----------|-------------|
| `--subscription` | Yes | Azure subscription ID |
| `--resource-group`, `-g` | Yes | Resource group name |
| `--vmss-name` | Yes | Name of the VMSS (max 9 chars for Windows) |
| `--location` | Yes | Azure region |
| `--admin-username` | Yes | Admin username |
| `--admin-password` | Conditional | Admin password (required for Windows) |
| `--ssh-public-key` | Conditional | SSH public key (for Linux VMSS) |
| `--vm-size` | No | VM size (default: Standard_DS1_v2) |
| `--image` | No | Image alias or URN (default: Ubuntu2404) |
| `--os-type` | No | OS type: 'linux' or 'windows' |
| `--virtual-network` | No | Virtual network name |
| `--subnet` | No | Subnet name |
| `--instance-count` | No | Number of VM instances (default: 2) |
| `--upgrade-policy` | No | Upgrade policy: 'Automatic', 'Manual', 'Rolling' (default: 'Manual') |
| `--zone` | No | Availability zone |
| `--os-disk-size-gb` | No | OS disk size in GB |
| `--os-disk-type` | No | OS disk type |

```bash
# Update Virtual Machine Scale Set configuration
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vmss update --subscription <subscription> \
                          --resource-group <resource-group> \
                          --vmss-name <vmss-name> \
                          [--capacity <capacity>] \
                          [--vm-size <vm-size>] \
                          [--upgrade-policy <upgrade-policy>] \
                          [--overprovision] \
                          [--enable-auto-os-upgrade] \
                          [--scale-in-policy <scale-in-policy>] \
                          [--tags <tags>]

# Examples:

# Scale VMSS to 5 instances
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vmss update --subscription "my-subscription" \
                          --resource-group "my-rg" \
                          --vmss-name "my-vmss" \
                          --capacity 5

# Enable automatic OS upgrades
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vmss update --subscription "my-subscription" \
                          --resource-group "my-rg" \
                          --vmss-name "my-vmss" \
                          --enable-auto-os-upgrade true

# Set scale-in policy to remove oldest VMs first
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute vmss update --subscription "my-subscription" \
                          --resource-group "my-rg" \
                          --vmss-name "my-vmss" \
                          --scale-in-policy "OldestVM"
```

**Parameters:**
| Parameter | Required | Description |
|-----------|----------|-------------|
| `--subscription` | Yes | Azure subscription ID |
| `--resource-group`, `-g` | Yes | Resource group name |
| `--vmss-name` | Yes | Name of the VMSS |
| `--capacity` | No | Number of VM instances |
| `--vm-size` | No | VM size |
| `--upgrade-policy` | No | Upgrade policy: 'Automatic', 'Manual', 'Rolling' |
| `--overprovision` | No | Enable or disable overprovisioning |
| `--enable-auto-os-upgrade` | No | Enable automatic OS image upgrades |
| `--scale-in-policy` | No | Scale-in policy: 'Default', 'OldestVM', 'NewestVM' |
| `--tags` | No | Tags in key=value,key2=value2 format |

```bash
# Delete a Virtual Machine Scale Set
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp compute vmss delete --subscription <subscription> \
                          --resource-group <resource-group> \
                          --vmss-name <vmss-name> \
                          [--force-deletion]

# Examples:

# Delete a VMSS
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp compute vmss delete --subscription "my-subscription" \
                          --resource-group "my-rg" \
                          --vmss-name "my-vmss"

# Force delete a VMSS even if it is in a running or failed state
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp compute vmss delete --subscription "my-subscription" \
                          --resource-group "my-rg" \
                          --vmss-name "my-vmss" \
                          --force-deletion
```

**Command Behavior:**
- Deletes the VMSS and all its VM instances. This operation is irreversible.
- **With `--force-deletion`**: Passes `forceDeletion=true` to the Azure API, which force-deletes the VMSS even if it is in a running or failed state.

**Parameters:**
| Parameter | Required | Description |
|-----------|----------|-------------|
| `--subscription` | Yes | Azure subscription ID |
| `--resource-group`, `-g` | Yes | Resource group name |
| `--vmss-name` | Yes | Name of the VMSS to delete |
| `--force-deletion` | No | Force delete the VMSS even if running or failed (Azure API forceDeletion) |

#### Disks

```bash
# List all managed disks in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute disk get --subscription <subscription>

# List all managed disks in a specific resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute disk get --subscription <subscription> \
                       --resource-group <resource-group>

# Get details of a specific managed disk
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute disk get --subscription <subscription> \
                       --resource-group <resource-group> \
                       --disk-name <disk-name>
```

**Options:**
-   `--disk-name`: The name of the managed disk (optional - if not provided, lists all disks)
-   `--resource-group`: The resource group to filter by (optional - if not provided, lists disks across all resource groups; required when specifying a disk name)
-   `--subscription`: Azure subscription ID or name (optional - defaults to AZURE_SUBSCRIPTION_ID environment variable)

```bash
# Create an empty managed disk (location defaults to resource group's location)
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute disk create --subscription <subscription> \
                          --resource-group <resource-group> \
                          --disk-name <disk-name> \
                          --size-gb <size>

# Create a managed disk from a snapshot or another disk (by resource ID)
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute disk create --subscription <subscription> \
                          --resource-group <resource-group> \
                          --disk-name <disk-name> \
                          --source <snapshot-or-disk-resource-id>

# Create a managed disk from a VHD blob URI
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute disk create --subscription <subscription> \
                          --resource-group <resource-group> \
                          --disk-name <disk-name> \
                          --source <blob-uri>

# Create a managed disk from a Shared Image Gallery image version (OS disk)
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute disk create --subscription <subscription> \
                          --resource-group <resource-group> \
                          --disk-name <disk-name> \
                          --gallery-image-reference <image-version-resource-id>

# Create a managed disk from a specific data disk LUN in a gallery image version
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute disk create --subscription <subscription> \
                          --resource-group <resource-group> \
                          --disk-name <disk-name> \
                          --gallery-image-reference <image-version-resource-id> \
                          --gallery-image-reference-lun <lun>

# Create a managed disk ready for upload
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute disk create --subscription <subscription> \
                          --resource-group <resource-group> \
                          --disk-name <disk-name> \
                          --upload-type Upload \
                          --upload-size-bytes <size-in-bytes>

# Create a managed disk ready for upload with security data (Trusted Launch)
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute disk create --subscription <subscription> \
                          --resource-group <resource-group> \
                          --disk-name <disk-name> \
                          --upload-type UploadWithSecurityData \
                          --upload-size-bytes <size-in-bytes> \
                          --security-type TrustedLaunch \
                          --hyper-v-generation V2

# Create a managed disk with a specific location, SKU, and all options
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute disk create --subscription <subscription> \
                          --resource-group <resource-group> \
                          --disk-name <disk-name> \
                          --size-gb <size> \
                          --location <location> \
                          --sku <sku> \
                          --os-type <os-type> \
                          --zone <zone> \
                          --hyper-v-generation <generation> \
                          --tags <space-separated-tags> \
                          --disk-encryption-set <encryption-set-resource-id> \
                          --encryption-type <encryption-type> \
                          --disk-access <disk-access-resource-id> \
                          --tier <performance-tier> \
                          --max-shares <count> \
                          --network-access-policy <policy> \
                          --enable-bursting <true|false> \
                          --disk-iops-read-write <iops> \
                          --disk-mbps-read-write <mbps> \
                          --upload-type <upload-type> \
                          --upload-size-bytes <size-in-bytes> \
                          --security-type <security-type> \
                          --gallery-image-reference <image-version-resource-id> \
                          --gallery-image-reference-lun <lun>
```

**Command Behavior:**
- Creates a new Azure managed disk in the specified resource group.
- Either `--size-gb`, `--source`, `--gallery-image-reference`, or `--upload-type` must be specified.
- When `--source` is a resource ID (snapshot or managed disk), the disk is created as a copy. When `--source` is a blob URI, the disk is imported from the VHD.
- When `--gallery-image-reference` is specified, the disk is created from a Shared Image Gallery image version. Use `--gallery-image-reference-lun` to select a specific data disk from the image; if omitted, the OS disk is used.
- When `--upload-type` is specified with `--upload-size-bytes`, creates a disk in a ready-to-upload state. Use `UploadWithSecurityData` with `--security-type` and `--hyper-v-generation V2` for Trusted Launch or Confidential VM scenarios.
- If `--location` is not specified, defaults to the resource group's location.
- Supports configuring disk size, storage SKU, OS type, availability zone, hypervisor generation, tags, encryption settings, performance tier, shared disk, network access, on-demand bursting, IOPS and throughput limits (UltraSSD only), upload type, and security type.

**Returns:**
- Disk information including name, location, resource group, disk size, SKU, provisioning state, OS type, zones, and tags.

**Parameters:**
| Parameter | Required | Description |
|-----------|----------|-------------|
| `--subscription` | Yes | Azure subscription ID or name |
| `--resource-group`, `-g` | Yes | Resource group name |
| `--disk-name` | Yes | Name of the managed disk to create |
| `--source` | Conditional | Source to create the disk from: a resource ID of a snapshot or managed disk, or a blob URI of a VHD. Required if `--size-gb`, `--gallery-image-reference`, and `--upload-type` are not specified. |
| `--size-gb` | Conditional | Size of the disk in GB. Required if `--source`, `--gallery-image-reference`, and `--upload-type` are not specified. When used with `--source`, overrides the source size. |
| `--location` | No | Azure region (defaults to the resource group's location if not specified) |
| `--sku` | No | Storage SKU (e.g., Premium_LRS, Standard_LRS, StandardSSD_LRS, UltraSSD_LRS) |
| `--os-type` | No | OS type for the disk (Windows or Linux) |
| `--zone` | No | Availability zone for the disk |
| `--hyper-v-generation` | No | Hypervisor generation (V1 or V2) |
| `--tags` | No | Space-separated tags in key=value format (e.g., env=prod team=infra) |
| `--disk-encryption-set` | No | Resource ID of the disk encryption set for customer-managed key encryption |
| `--encryption-type` | No | Encryption type (e.g., EncryptionAtRestWithCustomerKey, EncryptionAtRestWithPlatformAndCustomerKeys) |
| `--disk-access` | No | Resource ID of the disk access resource for private endpoint connections |
| `--tier` | No | Performance tier for the disk (e.g., P30, P40, P50) |
| `--max-shares` | No | Maximum number of VMs that can attach the disk simultaneously |
| `--network-access-policy` | No | Network access policy (AllowAll, AllowPrivate, DenyAll) |
| `--enable-bursting` | No | Enable on-demand bursting (true or false) |
| `--disk-iops-read-write` | No | IOPS limit for the disk (UltraSSD only) |
| `--disk-mbps-read-write` | No | Throughput limit in MBps for the disk (UltraSSD only) |
| `--gallery-image-reference` | Conditional | Resource ID of a Shared Image Gallery image version to create the disk from. Required if `--size-gb`, `--source`, and `--upload-type` are not specified. |
| `--gallery-image-reference-lun` | No | LUN of the data disk in the gallery image version (if omitted, creates from the OS disk) |
| `--upload-type` | Conditional | Upload type for the disk (Upload or UploadWithSecurityData). Required if `--size-gb`, `--source`, and `--gallery-image-reference` are not specified. When specified, `--upload-size-bytes` is required. |
| `--upload-size-bytes` | Conditional | Size in bytes for upload disks, including the VHD footer (required when `--upload-type` is specified) |
| `--security-type` | Conditional | Security type for the disk. Accepted values: ConfidentialVM_DiskEncryptedWithCustomerKey, ConfidentialVM_DiskEncryptedWithPlatformKey, ConfidentialVM_VMGuestStateOnlyEncryptedWithPlatformKey, Standard, TrustedLaunch. Required when `--upload-type` is UploadWithSecurityData. |

```bash
# Delete a managed disk
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp compute disk delete --subscription <subscription> \
                          --resource-group <resource-group> \
                          --disk-name <disk-name>
```

**Command Behavior:**
- Deletes an Azure managed disk from the specified resource group.
- This is an idempotent operation: returns `Deleted = true` if the disk was successfully removed, or `Deleted = false` if the disk was not found.
- The disk must not be attached to a virtual machine. Detach it first before deleting.

**Returns:**
- `Deleted`: Boolean indicating whether the disk was deleted.
- `DiskName`: Name of the disk that was targeted for deletion.

**Parameters:**
| Parameter | Required | Description |
|-----------|----------|-------------|
| `--subscription` | Yes | Azure subscription ID or name |
| `--resource-group`, `-g` | Yes | Resource group name |
| `--disk-name` | Yes | Name of the managed disk to delete |

```bash
# Update a managed disk's size
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute disk update --subscription <subscription> \
                          --resource-group <resource-group> \
                          --disk-name <disk-name> \
                          --size-gb <size>

# Update a managed disk without specifying resource group (resolved by searching subscription)
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute disk update --subscription <subscription> \
                          --disk-name <disk-name> \
                          --sku <sku>

# Update multiple properties of a managed disk
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp compute disk update --subscription <subscription> \
                          --resource-group <resource-group> \
                          --disk-name <disk-name> \
                          --size-gb <size> \
                          --sku <sku> \
                          --disk-iops-read-write <iops> \
                          --disk-mbps-read-write <mbps> \
                          --max-shares <count> \
                          --network-access-policy <policy> \
                          --enable-bursting <true|false> \
                          --tags <space-separated-tags> \
                          --disk-encryption-set <encryption-set-resource-id> \
                          --encryption-type <encryption-type> \
                          --disk-access <disk-access-resource-id> \
                          --tier <performance-tier>
```

**Command Behavior:**
- Updates properties of an existing Azure managed disk. Only specified properties are modified; unspecified properties remain unchanged.
- If `--resource-group` is not specified, the disk is located by name within the subscription.
- Disk size can only be increased, not decreased.
- IOPS and throughput limits (`--disk-iops-read-write`, `--disk-mbps-read-write`) apply to UltraSSD disks only.

**Returns:**
- Updated disk information including name, location, resource group, disk size, SKU, provisioning state, OS type, zones, and tags.

**Parameters:**
| Parameter | Required | Description |
|-----------|----------|-------------|
| `--subscription` | Yes | Azure subscription ID or name |
| `--disk-name` | Yes | Name of the managed disk to update |
| `--resource-group`, `-g` | No | Resource group name (if not provided, disk is located by searching the subscription) |
| `--size-gb` | No | New size of the disk in GB (can only increase) |
| `--sku` | No | New storage SKU (e.g., Premium_LRS, Standard_LRS, StandardSSD_LRS, UltraSSD_LRS) |
| `--disk-iops-read-write` | No | IOPS limit for the disk (UltraSSD only) |
| `--disk-mbps-read-write` | No | Throughput limit in MBps (UltraSSD only) |
| `--max-shares` | No | Maximum number of VMs that can attach the disk simultaneously |
| `--network-access-policy` | No | Network access policy (AllowAll, AllowPrivate, DenyAll) |
| `--enable-bursting` | No | Enable on-demand bursting (true or false) |
| `--tags` | No | Space-separated tags in key=value format (e.g., env=prod team=infra) |
| `--disk-encryption-set` | No | Resource ID of the disk encryption set for customer-managed key encryption |
| `--encryption-type` | No | Encryption type (e.g., EncryptionAtRestWithCustomerKey, EncryptionAtRestWithPlatformAndCustomerKeys) |
| `--disk-access` | No | Resource ID of the disk access resource for private endpoint connections |
| `--tier` | No | Performance tier for the disk (e.g., P30, P40, P50) |

### Azure Confidential Ledger Operations

```bash
# Append a tamper-proof entry to a Confidential Ledger
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp confidentialledger entries append --ledger <ledger-name> \
                                        --content <json-or-text-data> \
                                        [--collection-id <collection-id>]

# Retrieve a Confidential Ledger entry with verification proof
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp confidentialledger entries get --ledger <ledger-name> \
                                     --transaction-id <transaction-id> \
                                     [--collection-id <collection-id>]
```

**Options:**
-   `--ledger`: Confidential Ledger name (required)
-   `--content`: JSON or text data to insert into the ledger (required for the append command)
-   `--collection-id`: Collection ID to store the data with (optional)
-   `--transaction-id`: Ledger transaction identifier to retrieve (required for the get command)

### Azure Container Apps Operations

```bash
# List Azure Container Apps in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp containerapps list --subscription <subscription>

# List Azure Container Apps in a specific resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp containerapps list --subscription <subscription> \
                         [--resource-group <resource-group>]
```

### Azure Container Registry (ACR) Operations

```bash
# List Azure Container Registries in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp acr registry list --subscription <subscription>

# List Azure Container Registries in a specific resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp acr registry list --subscription <subscription> \
                        --resource-group <resource-group>

# List repositories across all registries in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp acr registry repository list --subscription <subscription>

# List repositories across all registries in a specific resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp acr registry repository list --subscription <subscription> \
                                   --resource-group <resource-group>

# List repositories in a specific registry
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp acr registry repository list --subscription <subscription> \
                                   --resource-group <resource-group> \
                                   --registry <registry>
```

### Azure Cosmos DB Operations

```bash
# List Cosmos DB resources (accounts, databases, or containers) in a subscription.
# Omit --account to list accounts. Provide --account to list databases.
# Provide --account and --database to list containers.
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp cosmos list --subscription <subscription> \
                  [--account <account>] \
                  [--database <database>]

# Query items in a Cosmos DB container
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp cosmos database container item query --subscription <subscription> \
                                           --account <account> \
                                           --database <database> \
                                           --container <container> \
                                           [--query "SELECT * FROM c"]
```

### Azure Data Explorer Operations

```bash
# Get details for a Azure Data Explorer cluster
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp kusto cluster get --subscription <subscription> \
                        --cluster <cluster>

# List Azure Data Explorer clusters in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp kusto cluster list --subscription <subscription>

# List databases in a Azure Data Explorer cluster
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp kusto database list [--cluster-uri <cluster-uri> | --subscription <subscription> --cluster <cluster>]

# Retrieves a sample of data from a specified Azure Data Explorer table.
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp kusto sample [--cluster-uri <cluster-uri> | --subscription <subscription> --cluster <cluster>]
                   --database <database> \
                   --table <table> \
                   [--limit <limit>]

# List tables in a Azure Data Explorer database
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp kusto table list [--cluster-uri <cluster-uri> | --subscription <subscription> --cluster <cluster>] \
                       --database <database>

# Retrieves the schema of a specified Azure Data Explorer table.
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp kusto table schema [--cluster-uri <cluster-uri> | --subscription <subscription> --cluster <cluster>] \
                         --database <database> \
                         --table <table>

# Query Azure Data Explorer database
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp kusto query [--cluster-uri <cluster-uri> | --subscription <subscription> --cluster <cluster>] \
                  --database <database> \
                  --query <kql-query>

```

### Azure Database for MySQL Operations

```bash
# Hierarchical list command for MySQL resources
# Without parameters: lists all MySQL servers in the resource group
# With --server: lists all databases on that server
# With --server and --database: lists all tables in that database
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp mysql list --subscription <subscription> \
                 --resource-group <resource-group> \
                 --user <user> \
                 [--server <server>] \
                 [--database <database>]

# Executes a SELECT query on a MySQL Database. The query must start with SELECT and cannot contain any destructive SQL operations for security reasons.
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp mysql database query --subscription <subscription> \
                           --resource-group <resource-group> \
                           --user <user> \
                           --server <server> \
                           --database <database> \
                           --query <query>

# Get the schema of a specific table in a MySQL database
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp mysql table schema get --subscription <subscription> \
                             --resource-group <resource-group> \
                             --user <user> \
                             --server <server> \
                             --database <database> \
                             --table <table>

# Retrieve the configuration of a MySQL server
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp mysql server config get --subscription <subscription> \
                              --resource-group <resource-group> \
                              --user <user> \
                              --server <server>

# Retrieve a specific parameter of a MySQL server
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp mysql server param get --subscription <subscription> \
                             --resource-group <resource-group> \
                             --user <user> \
                             --server <server> \
                             --param <parameter>

# Set a specific parameter of a MySQL server to a specific value
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp mysql server param set --subscription <subscription> \
                             --resource-group <resource-group> \
                             --user <user> \
                             --server <server> \
                             --param <parameter> \
                             --value <value>
```

### Azure Database for PostgreSQL Operations

```bash
# Hierarchical list command for PostgreSQL resources
# Without parameters: lists all PostgreSQL servers in the resource group
# With --server: lists all databases on that server
# With --server and --database: lists all tables in that database
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp postgres list --subscription <subscription> \
                    --resource-group <resource-group> \
                    --user <user> \
                    [--server <server>] \
                    [--database <database>]

# Execute a query on a PostgreSQL database
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp postgres database query --subscription <subscription> \
                              --resource-group <resource-group> \
                              --user <user> \
                              --server <server> \
                              --database <database> \
                              --query <query>

# Get the schema of a specific table in a PostgreSQL database
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp postgres table schema get --subscription <subscription> \
                                --resource-group <resource-group> \
                                --user <user> \
                                --server <server> \
                                --database <database> \
                                --table <table>

# Retrieve the configuration of a PostgreSQL server
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp postgres server config get --subscription <subscription> \
                                 --resource-group <resource-group> \
                                 --user <user> \
                                 --server <server>

# Retrieve a specific parameter of a PostgreSQL server
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp postgres server param get --subscription <subscription> \
                                --resource-group <resource-group> \
                                --user <user> \
                                --server <server> \
                                --param <parameter>

# Set a specific parameter of a PostgreSQL server to a specific value
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
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
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp deploy app logs get --workspace-folder <workspace-folder> \
                          --azd-env-name <azd-env-name> \
                          [--limit <limit>]

# Generate a mermaid architecture diagram for the application topology follow the schema defined in [deploy-app-topology-schema.json](../areas/deploy/src/AzureMcp.Deploy/Schemas/deploy-app-topology-schema.json)
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp deploy architecture diagram generate --raw-mcp-tool-input <app-topology>

# Get the iac generation rules for the resource types
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp deploy iac rules get --deployment-tool <deployment-tool> \
                           --iac-type <iac-type> \
                           --resource-types <resource-types>

# Get the ci/cd pipeline guidance
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp deploy pipeline guidance get [--is-azd-project <is-azd-project>] \
                                   [--pipeline-platform <pipeline-platform>] \
                                   [--deploy-option <deploy-option>]

# Get a deployment plan for a specific project
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp deploy plan get --workspace-folder <workspace-folder> \
                      --project-name <project-name> \
                      --target-app-service <target-app-service> \
                      --provisioning-tool <provisioning-tool> \
                      --source-type <source-type> \
                      [--iac-options <iac-options>] \
                      [--deploy-option <deploy-option>] \
                      [--resource-group <resource-group>] \
                      [--subscription <subscription>]
```

### Azure Device Registry Operations

```bash
# List Azure Device Registry namespaces in a subscription or resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp deviceregistry namespace list --subscription <subscription> \
                                    [--resource-group <resource-group>]
```

### Azure Event Grid Operations

```bash
# List all Event Grid topics in a subscription or resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp eventgrid topic list --subscription <subscription> \
                           [--resource-group <resource-group>]


# List all Event Grid subscriptions in a subscription, resource group, or topic
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp eventgrid subscription list --subscription <subscription> \
                                  [--resource-group <resource-group>] \
                                  [--topic <topic>]
                                  [--location <location>]

# Publish custom events to Event Grid topics
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp eventgrid events publish --subscription <subscription> \
                               --topic <topic> \
                               --data <json-event-data> \
                               [--resource-group <resource-group>] \
                               [--schema <schema-type>]
```

### Azure Event Hubs

```bash
# Delete a Consumer Group
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp eventhubs eventhub consumergroup delete --subscription <subscription> \
                                              --resource-group <resource-group> \
                                              --namespace <namespace> \
                                              --eventhub <eventhub-name> \
                                              --consumer-group <consumer-group-name>

# Get Consumer Groups (list all in event hub or get specific consumer group)
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp eventhubs eventhub consumergroup get --subscription <subscription> \
                                           --resource-group <resource-group> \
                                           --namespace <namespace> \
                                           --eventhub <eventhub-name> \
                                           [--consumer-group <consumer-group-name>]

# Create or update a Consumer Group
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp eventhubs eventhub consumergroup update --subscription <subscription> \
                                              --resource-group <resource-group> \
                                              --namespace <namespace> \
                                              --eventhub <eventhub-name> \
                                              --consumer-group <consumer-group-name> \
                                              [--user-metadata <user-metadata>]
```

```bash
# Delete an Event Hub
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp eventhubs eventhub delete --subscription <subscription> \
                                --resource-group <resource-group> \
                                --namespace <namespace> \
                                --eventhub <eventhub-name>

# Get Event Hubs (list all in namespace or get specific event hub)
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp eventhubs eventhub get --subscription <subscription> \
                             --resource-group <resource-group> \
                             --namespace <namespace> \
                             [--eventhub <eventhub-name>]

# Create or update an Event Hub
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp eventhubs eventhub update --subscription <subscription> \
                                --resource-group <resource-group> \
                                --namespace <namespace> \
                                --eventhub <eventhub-name> \
                                [--partition-count <count>] \
                                [--message-retention-in-hours <hours>] \
                                [--status <status>]
```

```bash
# Delete an Event Hubs Namespace
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp eventhubs namespace delete --subscription <subscription> \
                                 --resource-group <resource-group> \
                                 --namespace <namespace>

# Get Event Hubs Namespaces (list all in subscription/resource group or get specific namespace)
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp eventhubs namespace get --subscription <subscription> \
                              [--resource-group <resource-group>] \
                              [--namespace <namespace>]

# Create or update an Event Hubs Namespace
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp eventhubs namespace update --subscription <subscription> \
                                 --resource-group <resource-group> \
                                 --namespace <namespace> \
                                 [--location <location>] \
                                 [--sku-name <sku-name>] \
                                 [--sku-tier <sku-tier>] \
                                 [--sku-capacity <sku-capacity>] \
                                 [--is-auto-inflate-enabled <true/false>] \
                                 [--maximum-throughput-units <units>] \
                                 [--kafka-enabled <true/false>] \
                                 [--zone-redundant <true/false>] \
                                 [--tags <json-tags>]
```

### Azure File Shares Operations

```bash
# Get a specific File Share or list all File Shares
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp fileshares fileshare get --subscription <subscription> \
                               --resource-group <resource-group> \
                               --name <file-share-name>

# Create a new File Share
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp fileshares fileshare create --subscription <subscription> \
                                  --resource-group <resource-group> \
                                  --name <file-share-name> \
                                  --location <azure-region> \
                                  [--mount-name <mount-name>] \
                                  [--media-tier <SSD|HDD>] \
                                  [--redundancy <Local|Zone>] \
                                  [--protocol <NFS>] \
                                  [--provisioned-storage-in-gib <size>] \
                                  [--provisioned-io-per-sec <iops>] \
                                  [--provisioned-throughput-mib-per-sec <throughput>] \
                                  [--public-network-access <Enabled|Disabled>] \
                                  [--nfs-root-squash <NoRootSquash|RootSquash|AllSquash>] \
                                  [--allowed-subnets <comma-separated-subnet-ids>] \
                                  [--tags <json-tags>]

# Update an existing File Share
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp fileshares fileshare update --subscription <subscription> \
                                  --resource-group <resource-group> \
                                  --name <file-share-name> \
                                  [--provisioned-storage-in-gib <size>] \
                                  [--provisioned-io-per-sec <iops>] \
                                  [--provisioned-throughput-mib-per-sec <throughput>] \
                                  [--public-network-access <Enabled|Disabled>] \
                                  [--nfs-root-squash <NoRootSquash|RootSquash|AllSquash>] \
                                  [--allowed-subnets <comma-separated-subnet-ids>] \
                                  [--tags <json-tags>]

# Delete a File Share
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp fileshares fileshare delete --subscription <subscription> \
                                  --resource-group <resource-group> \
                                  --name <file-share-name>

# Check File Share name availability
azmcp fileshares fileshare checkname --subscription <subscription> \
                                     --name <file-share-name>
```

```bash
# Get a specific File Share snapshot
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp fileshares fileshare snapshot get --subscription <subscription> \
                                        --resource-group <resource-group> \
                                        --file-share-name <file-share-name> \
                                        --snapshot-name <snapshot-name>

# Create a File Share snapshot
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp fileshares fileshare snapshot create --subscription <subscription> \
                                           --resource-group <resource-group> \
                                           --file-share-name <file-share-name>

# Update a File Share snapshot
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp fileshares fileshare snapshot update --subscription <subscription> \
                                           --resource-group <resource-group> \
                                           --file-share-name <file-share-name> \
                                           --snapshot-name <snapshot-name> \
                                           [--tags <json-tags>]

# Delete a File Share snapshot
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp fileshares fileshare snapshot delete --subscription <subscription> \
                                           --resource-group <resource-group> \
                                           --file-share-name <file-share-name> \
                                           --snapshot-name <snapshot-name>
```

```bash
# Get a specific private endpoint connection or list all private endpoint connections for a file share
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp fileshares fileshare peconnection get --subscription <subscription> \
                                            --resource-group <resource-group> \
                                            --file-share-name <file-share-name> \
                                            [--connection-name <connection-name>]

# Update the state of a private endpoint connection for a file share
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp fileshares fileshare peconnection update --subscription <subscription> \
                                               --resource-group <resource-group> \
                                               --file-share-name <file-share-name> \
                                               --connection-name <connection-name> \
                                               --status <Approved|Rejected> \
                                               [--description <description>]
```

```bash
# Get File Shares limits and quotas for a region
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp fileshares limits --subscription <subscription> \
                        --location <azure-region>

# Get provisioning recommendations for File Shares
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp fileshares rec --subscription <subscription> \
                     --location <azure-region> \
                     --provisioned-storage-in-gib <size>

# Get usage data and metrics for File Shares
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp fileshares usage --subscription <subscription> \
                       --location <azure-region>
```

### Microsoft Foundry Extensions Operations

```bash
# List knowledge indexes in a Microsoft Foundry project
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundryextensions knowledge index list \
    --endpoint <project-endpoint>

# Get the schema of a knowledge index in Microsoft Foundry
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundryextensions knowledge index schema \
    --endpoint <project-endpoint> \
    --index-name <index-name>

# Create chat completions using Azure OpenAI in Microsoft Foundry
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundryextensions openai chat-completions-create \
    --subscription <subscription> \
    --resource-group <resource-group> \
    --resource-name <resource-name> \
    --deployment-name <deployment-name> \
    --message-array <json-message-array>

# Create text completions using Azure OpenAI in Microsoft Foundry
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundryextensions openai create-completion \
    --subscription <subscription> \
    --resource-group <resource-group> \
    --resource-name <resource-name> \
    --deployment-name <deployment-name> \
    --prompt-text <prompt>

# Create embeddings using Azure OpenAI in Microsoft Foundry
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundryextensions openai embeddings-create \
    --subscription <subscription> \
    --resource-group <resource-group> \
    --resource-name <resource-name> \
    --deployment-name <deployment-name> \
    --input-text <text>

# List available Azure OpenAI model deployments in a Microsoft Foundry resource
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundryextensions openai models-list \
    --subscription <subscription> \
    --resource-group <resource-group> \
    --resource-name <resource-name>

# Get details of Microsoft Foundry (AI) resources in a subscription or resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundryextensions resource get \
    --subscription <subscription> \
    [--resource-group <resource-group>] \
    [--resource-name <resource-name>]
```

### Azure Function App Operations

```bash
# Get detailed properties of function apps
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp functionapp get --subscription <subscription> \
                      [--resource-group <resource-group>] \
                      [--function-app <function-app-name>]
```

### Azure Functions Operations

```bash
# List supported programming languages for Azure Functions with runtime versions, prerequisites, and development tools
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp functions language list

# Get project initialization files for a new Azure Functions app including host.json, local.settings.json, and language-specific files
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp functions project get --language <language>

# List all available function templates for a language or get the complete source code for a specific template
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp functions template get --language <language> \
                             [--template <template-name>] \
                             [--runtime-version <runtime-version>]
```

### Azure Key Vault Operations

#### Administration

```bash
# Gets Key Vault Managed HSM account settings
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp keyvault admin settings get --subscription <subscription> \
                                  --vault <vault-name>
```

#### Certificates

```bash
# Creates a certificate in a key vault with the default policy
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp keyvault certificate create --subscription <subscription> \
                                  --vault <vault-name> \
                                  --name <certificate-name>

# Get a specific certificate or list all certificates. If --name is provided, returns a specific certificate; otherwise, lists all certificates in the key vault.
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp keyvault certificate get --subscription <subscription> \
                               --vault <vault-name> \
                               [--name <certificate-name>]

# Imports an existing certificate (PFX or PEM) into a key vault
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp keyvault certificate import --subscription <subscription> \
                                  --vault <vault-name> \
                                  --certificate <certificate-name> \
                                  --certificate-data <path-or-base64-or-raw-pem> \
                                  [--password <pfx-password>]
```

#### Keys

```bash
# Creates a key in a key vault
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp keyvault key create --subscription <subscription> \
                          --vault <vault-name> \
                          --key <key-name> \
                          --key-type <key-type>

# Get a specific key or list all keys. If --key is provided, returns a specific key; otherwise, lists all keys in the key vault.
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp keyvault key get --subscription <subscription> \
                       --vault <vault-name> \
                       [--key <key-name>] \
                       [--include-managed]
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
> These prompts protect against unauthorized access to sensitive information. You can bypass elicitation in automated scenarios using the `--dangerously-disable-elicitation` server start option, but this should only be used in trusted environments.

```bash
# Creates a secret in a key vault (will prompt for user consent)
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp keyvault secret create --subscription <subscription> \
                             --vault <vault-name> \
                             --name <secret-name> \
                             --value <secret-value>

# Get a specific secret or list all secrets. If --secret is provided, returns a specific secret with its value (requires user consent); otherwise, lists all secrets in the key vault (returns secret names and properties, not values).
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp keyvault secret get --subscription <subscription> \
                          --vault <vault-name> \
                          [--secret <secret-name>]
```

### Azure Kubernetes Service (AKS) Operations

```bash
# Gets Azure Kubernetes Service (AKS) cluster details
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp aks cluster get --subscription <subscription> \
                      --resource-group <resource-group> \
                      [--cluster <cluster>]

# Gets Azure Kubernetes Service (AKS) nodepool details
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp aks nodepool get --subscription <subscription> \
                       --resource-group <resource-group> \
                       --cluster <cluster> \
                       [--nodepool <nodepool>]
```

### Azure Load Testing Operations

```bash
# Create load test
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp loadtesting test create --subscription <subscription> \
                              --test-id <test-id> \
                              --test-resource-name <test-resource-name> \
                              [--resource-group <resource-group>] \
                              [--display-name <display-name>] \
                              [--description <description>] \
                              [--endpoint <endpoint>] \
                              [--virtual-users <virtual-users>] \
                              [--duration <duration>] \
                              [--ramp-up-time <ramp-up-time>]

# Get load test
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp loadtesting test get --subscription <subscription> \
                           --test-id <test-id> \
                           --test-resource-name <test-resource-name> \
                           [--resource-group <resource-group>]

# Create load test resources
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp loadtesting testresource create --subscription <subscription> \
                                      --resource-group <resource-group> \
                                      [--test-resource-name <test-resource-name>]

# List load test resources
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp loadtesting testresource list --subscription <subscription> \
                                    [--resource-group <resource-group>] \
                                    [--test-resource-name <test-resource-name>]

# Get load test run (single run or list all runs for a test)
# Get a single test run by ID:
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp loadtesting testrun get --subscription <subscription> \
                              --test-resource-name <test-resource-name> \
                              [--resource-group <resource-group>] \
                              [--testrun-id <testrun-id>] \
                              [--test-id <test-id>]
# Note: Either --testrun-id or --test-id must be provided, but not both.

# Create or update load test run
# Note: Create operations are NOT idempotent (each creates new execution with unique timestamps).
#       Update operations ARE idempotent (repeated calls with same values produce same result).
# Create a new test run:
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp loadtesting testrun createorupdate --subscription <subscription> \
                                         --test-id <test-id> \
                                         --test-resource-name <test-resource-name> \
                                         --testrun-id <testrun-id> \
                                         [--resource-group <resource-group>] \
                                         [--display-name <display-name>] \
                                         [--description <description>] \
                                         [--old-testrun-id <old-testrun-id>]

# Rerun an existing test run:
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp loadtesting testrun createorupdate --subscription <subscription> \
                                         --test-id <test-id> \
                                         --test-resource-name <test-resource-name> \
                                         --testrun-id <new-testrun-id> \
                                         [--resource-group <resource-group>] \
                                         [--old-testrun-id <existing-testrun-id>]

# Update test run metadata (idempotent):
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp loadtesting testrun createorupdate --subscription <subscription> \
                                         --test-id <test-id> \
                                         --test-resource-name <test-resource-name> \
                                         --testrun-id <testrun-id> \
                                         [--resource-group <resource-group>] \
                                         [--display-name <updated-display-name>] \
                                         [--description <updated-description>]
```

### Azure Managed Grafana Operations

```bash
# List Azure Managed Grafana
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp grafana list --subscription <subscription>
```

### Azure Marketplace Operations

```bash
# List marketplace products available to a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp marketplace product list --subscription <subscription> \
                               [--language <language-code>] \
                               [--search <terms>] \
                               [--filter <odata-filter>] \
                               [--orderby <odata-orderby>] \
                               [--select <odata-select>] \
                               [--expand <odata-expand>] \
                               [--next-cursor <pagination-cursor>]

# Get details about an Azure Marketplace product
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
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
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp get azure bestpractices get --resource <resource> --action <action>

# Resource options:
#   general        - General Azure best practices
#   azurefunctions - Azure Functions specific best practices
#   static-web-app - Azure Static Web Apps specific best practices
#
# Action options:
#   all             - Best practices for both code generation and deployment (only for static-web-app)
#   code-generation - Best practices for code generation (for general and azurefunctions)
#   deployment      - Best practices for deployment (for general and azurefunctions)

# Get best practices for building AI applications, workflows and agents in Azure
# Call this before generating code for any AI application, working with Microsoft Agent Framework,
# or implementing AI solutions in Azure.
azmcp get azure bestpractices ai_app

# AI App Development:
#   ai_app - Comprehensive guidance for AI applications including:
#     • Microsoft Agent Framework usage and patterns
#     • Best practices for AI app/agent development in Azure
```

### Azure MCP Tools

The `azmcp tools list` command provides flexible ways to explore and discover available tools in the Azure MCP server. It supports multiple modes and filtering options that can be combined for precise control over the output format and content.

**Available Options:**
- `--namespace-mode`: List only top-level service namespaces instead of individual tools
- `--name-only`: Return only tool/namespace names without descriptions, options, or metadata
- `--namespace <namespace>`: Filter results to specific namespace(s). Can be used multiple times to include multiple namespaces

**Option Combinations:**
- Use `--name-only` alone to get a simple list of all tool names
- Use `--namespace-mode` alone to see available service namespaces with full details
- Combine `--namespace-mode` and `--name-only` to get just the namespace names
- Use `--namespace` with any other option to filter results to specific services
- All options can be combined for maximum flexibility

```bash
# List all available tools in the Azure MCP server
azmcp tools list

# List only the available top-level service namespaces
azmcp tools list --namespace-mode

# List only tool names without descriptions or metadata
azmcp tools list --name-only

# Filter tools by specific namespace(s)
azmcp tools list --namespace storage
azmcp tools list --namespace storage --namespace keyvault

# Combine options: get namespace names only for specific namespaces
azmcp tools list --namespace-mode --name-only
azmcp tools list --namespace-mode --name-only --namespace storage

# Combine options: get tool names only for specific namespace(s)
azmcp tools list --name-only --namespace storage
azmcp tools list --name-only --namespace storage --namespace keyvault
```

### Azure Monitor Operations

#### Activity Logs

```bash
# List the activity logs of an Azure Resource
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp monitor activitylog list --subscription <subscription> \
                               --resource-group <resource-group> \
                               --resource-type <resource-type> \
                               --resource-name <resource-name> \
                               --hours: <hours> \
                               --event-level: <event-level> \
                               --top: <top>
```

#### Log Analytics

```bash
# List tables in a Log Analytics workspace
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp monitor table list --subscription <subscription> \
                         --workspace <workspace> \
                         --resource-group <resource-group>

# List Log Analytics workspaces in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp monitor workspace list --subscription <subscription>

# Query logs from Azure Monitor using KQL
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp monitor resource log query --subscription <subscription> \
                                 --resource-id <resource-id> \
                                 --table <table> \
                                 --query <kql-query> \
                                 [--hours <hours>] \
                                 [--limit <limit>]

# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp monitor workspace log query --subscription <subscription> \
                                  --workspace <workspace> \
                                  --table <table> \
                                  --query <kql-query> \
                                  [--hours <hours>] \
                                  [--limit <limit>]

# Examples:
# Query logs from a specific table
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp monitor workspace log query --subscription <subscription> \
                                  --workspace <workspace> \
                                  --table "AppEvents_CL" \
                                  --query "| order by TimeGenerated desc"
```

#### Health Models

```bash
# Get the health of an entity
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp monitor healthmodels entity get --subscription <subscription> \
                                      --resource-group <resource-group> \
                                      --health-model <health-model-name> \
                                      --entity <entity-id>
```

#### Metrics

```bash
# List available metric definitions for a resource
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp monitor metrics definitions --subscription <subscription> \
                                  --resource <resource> \
                                  [--resource-group <resource-group>] \
                                  [--resource-type <resource-type>] \
                                  [--metric-namespace <metric-namespace>] \
                                  [--search-string <search-string>] \
                                  [--limit <limit>]

# Query Azure Monitor metrics for a resource
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
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
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp monitor metrics definitions --subscription <subscription> \
                                  --resource <resource> \
                                  --resource-type "Microsoft.Storage/storageAccounts"

# Find metrics related to transactions
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp monitor metrics definitions --subscription <subscription> \
                                  --resource <resource> \
                                  --search-string "transaction"

# Query CPU and memory metrics for a virtual machine
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
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

#### Web Tests (Availability Tests)

```bash
# Create a new web test in Azure Monitor
azmcp monitor webtests create --subscription <subscription> \
                              --resource-group <resource-group> \
                              --webtest-resource <webtest-resource-name> \
                              --appinsights-component <component-name> \
                              --location <location> \
                              --webtest-locations <locations> \
                              --request-url <url> \
                              [--webtest <display-name>] \
                              [--description <description>] \
                              [--enabled <true|false>] \
                              [--expected-status-code <code>] \
                              [--follow-redirects <true|false>] \
                              [--frequency <seconds>] \
                              [--headers <key=value,key2=value2>] \
                              [--http-verb <get|post|..>] \
                              [--ignore-status-code <true|false>] \
                              [--parse-requests <true|false>] \
                              [--request-body <body>] \
                              [--retry-enabled <true|false>] \
                              [--ssl-check <true|false>] \
                              [--ssl-lifetime-check <days>] \
                              [--timeout <seconds>]

# Get details for a specific web test
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp monitor webtests get --subscription <subscription> \
                          --resource-group <resource-group> \
                          --webtest-resource <webtest-resource-name>

# List all web tests in a subscription or optionally, within a resource group
azmcp monitor webtests list --subscription <subscription> [--resource-group <resource-group>]

# Update an existing web test in Azure Monitor
azmcp monitor webtests update --subscription <subscription> \
                              --resource-group <resource-group> \
                              --webtest-resource <webtest-resource-name> \
                              [--appinsights-component <component-name>] \
                              [--location <location>] \
                              [--webtest-locations <locations>] \
                              [--request-url <url>] \
                              [--webtest <display-name>] \
                              [--description <description>] \
                              [--enabled <true|false>] \
                              [--expected-status-code <code>] \
                              [--follow-redirects <true|false>] \
                              [--frequency <seconds>] \
                              [--headers <key=value,key2=value2>] \
                              [--http-verb <get|post|..>] \
                              [--ignore-status-code <true|false>] \
                              [--parse-requests <true|false>] \
                              [--request-body <body>] \
                              [--retry-enabled <true|false>] \
                              [--ssl-check <true|false>] \
                              [--ssl-lifetime-check <days>] \
                              [--timeout <seconds>]

```

### Azure Monitor Instrumentation Operations

```bash

# Get a specific learning resource by path or list all available Azure Monitor onboarding learning resources
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp monitor instrumentation get-learning-resource [--path <resource-path>]

# Start deterministic instrumentation orchestration for a local workspace
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp monitor instrumentation orchestrator-start --workspace-path <absolute-workspace-path>

# Continue orchestration after completing the previous action
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp monitor instrumentation orchestrator-next --session-id <session-id> \
                                                --completion-note <what-was-completed>

# Send brownfield analysis findings JSON to continue migration flow
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp monitor instrumentation send-brownfield-analysis --session-id <session-id> \
                                                       --findings-json <json>

# Submit enhancement selection when orchestrator-start returns enhancement_available
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp monitor instrumentation send-enhancement-select --session-id <session-id> \
                                                      --enhancement-keys <comma-separated-keys>
```

**Notes:**
- `orchestrator-start` and `orchestrator-next` mirror the orchestration flow used by Azure Monitor onboarding.
- `send-brownfield-analysis` expects a JSON payload matching the `analysisTemplate` returned by `orchestrator-start` when status is `analysis_needed`.
- `send-enhancement-select` expects one or more enhancement keys from `enhancementOptions` returned by `orchestrator-start` when status is `enhancement_available`.

### Azure Managed Lustre Operations

```bash
# List Azure Managed Lustre file systems in a subscription or resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs list --subscription <subscription> \
                            [--resource-group <resource-group>]

# Create an Azure Managed Lustre filesystem
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs create --subscription <subscription> \
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
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs update --subscription <subscription> \
                              --resource-group <resource-group> \
                              --name <filesystem-name> \
                              [--maintenance-day <maintenance-day>] \
                              [--maintenance-time <HH:mm>] \
                              [--root-squash-mode <mode>] \
                              [--no-squash-nid-list <nid1,nid2,...>] \
                              [--squash-uid <uid>] \
                              [--squash-gid <gid>]

# Returns the required number of IP addresses for a specific Azure Managed Lustre SKU and filesystem size
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs subnetsize ask --subscription <subscription> \
                                      --sku <azure-managed-lustre-sku> \
                                      --size <filesystem-size-in-tib>

# Checks if a subnet has enough available IP addresses for the specified Azure Managed Lustre SKU and filesystem size
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs subnetsize validate --subscription <subscription> \
                                           --subnet-id <subnet-resource-id> \
                                           --sku <azure-managed-lustre-sku> \
                                           --size <filesystem-size-in-tib> \
                                           --location <filesystem-location>

# Lists the available Azure Managed Lustre SKUs in a specific location
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs sku get --subscription <subscription> \
                                            --location <location>

# Create an autoexport job for an Azure Managed Lustre filesystem
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs blob autoexport create --subscription <subscription> \
                                             --resource-group <resource-group> \
                                             --filesystem-name <filesystem-name> \
                                             [--job-name <job-name>] \
                                             [--autoexport-prefix <prefix>] \
                                             [--admin-status <Enable|Disable>]

# Cancel an autoexport job for an Azure Managed Lustre filesystem
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs blob autoexport cancel --subscription <subscription> \
                                             --resource-group <resource-group> \
                                             --filesystem-name <filesystem-name> \
                                             --job-name <job-name>

# Get details of autoexport jobs for an Azure Managed Lustre filesystem
# Returns a specific job if job-name is provided, or lists all jobs if omitted
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs blob autoexport get --subscription <subscription> \
                                          --resource-group <resource-group> \
                                          --filesystem-name <filesystem-name> \
                                          [--job-name <job-name>]

# Delete an autoexport job for an Azure Managed Lustre filesystem
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs blob autoexport delete --subscription <subscription> \
                                             --resource-group <resource-group> \
                                             --filesystem-name <filesystem-name> \
                                             --job-name <job-name>

# Get details of autoimport jobs for an Azure Managed Lustre filesystem
# Returns a specific job if job-name is provided, or lists all jobs if omitted
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs blob autoimport get --subscription <subscription> \
                                           --resource-group <resource-group> \
                                           --filesystem-name <filesystem-name> \
                                           [--job-name <job-name>]

# Create an autoimport job for an Azure Managed Lustre filesystem
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs blob autoimport create --subscription <subscription> \
                                             --resource-group <resource-group> \
                                             --filesystem-name <filesystem-name> \
                                             [--job-name <job-name>] \
                                             [--conflict-resolution-mode <Fail|Skip|OverwriteIfDirty|OverwriteAlways>] \
                                             [--autoimport-prefixes <prefix1> --autoimport-prefixes <prefix2> ...] \
                                             [--admin-status <Enable|Disable>] \
                                             [--enable-deletions <true|false>] \
                                             [--maximum-errors <number>]

# Cancel an autoimport job for an Azure Managed Lustre filesystem
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs blob autoimport cancel --subscription <subscription> \
                                              --resource-group <resource-group> \
                                              --filesystem-name <filesystem-name> \
                                              --job-name <job-name>

# Delete an autoimport job for an Azure Managed Lustre filesystem
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs blob autoimport delete --subscription <subscription> \
                                              --resource-group <resource-group> \
                                              --filesystem-name <filesystem-name> \
                                              --job-name <job-name>

# Create a one-time import job for an Azure Managed Lustre filesystem
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs blob import create --subscription <subscription> \
                                         --resource-group <resource-group> \
                                         --filesystem-name <filesystem-name> \
                                         [--job-name <job-name>] \
                                         [--conflict-resolution-mode <Fail|Skip|OverwriteIfDirty|OverwriteAlways>] \
                                         [--import-prefixes <prefix1> --import-prefixes <prefix2> ...] \
                                         [--maximum-errors <number>]

# Get details of one-time import jobs for an Azure Managed Lustre filesystem
# Returns a specific job if job-name is provided, or lists all jobs if omitted
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs blob import get --subscription <subscription> \
                                      --resource-group <resource-group> \
                                      --filesystem-name <filesystem-name> \
                                      [--job-name <job-name>]

# Cancel a running one-time import job for an Azure Managed Lustre filesystem
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs blob import cancel --subscription <subscription> \
                                         --resource-group <resource-group> \
                                         --filesystem-name <filesystem-name> \
                                         --job-name <job-name>

# Delete a one-time import job for an Azure Managed Lustre filesystem
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs blob import delete --subscription <subscription> \
                                         --resource-group <resource-group> \
                                         --filesystem-name <filesystem-name> \
                                         --job-name <job-name>
```

### Azure Migrate Operations

#### Platform Landing Zone Modification Guidance

```bash
# Fetch official Azure Landing Zone modification guidance for a specific scenario
# ✅ Destructive | ✅ Idempotent | ✅ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp azuremigrate platformlandingzone getguidance --scenario <scenario> \
                                                   [--policy-name <policy-name>] \
                                                   [--list-policies <true|false>]
```

**Available Scenarios:**

| Scenario | Description |
|----------|-------------|
| `resource-names` | Update resource naming prefixes and suffixes |
| `management-groups` | Customize management group names and IDs |
| `ddos` | Enable or disable DDoS protection plan |
| `bastion` | Turn off Bastion host |
| `dns` | Turn off Private DNS zones and resolvers |
| `gateways` | Turn off Virtual Network Gateways (VPN/ExpressRoute) |
| `regions` | Add or remove secondary regions |
| `ip-addresses` | Adjust CIDR ranges and IP address space |
| `policy-enforcement` | Change policy enforcement mode to DoNotEnforce |
| `policy-assignment` | Remove or disable a policy assignment |
| `ama` | Turn off Azure Monitoring Agent |
| `amba` | Deploy Azure Monitoring Baseline Alerts |
| `defender` | Turn off Defender Plans |
| `zero-trust` | Implement Zero Trust Networking |
| `slz` | Implement Sovereign Landing Zone controls |

**Policy-related Options:**
- `--policy-name`: Search for a specific policy by partial or full name
- `--list-policies`: Set to `true` to list ALL policies organized by archetype

**Examples:**
```bash
# Get guidance for enabling DDoS protection
# ✅ Destructive | ✅ Idempotent | ✅ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp azuremigrate platformlandingzone getguidance --scenario ddos

# Search for policies related to DDoS
# ✅ Destructive | ✅ Idempotent | ✅ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp azuremigrate platformlandingzone getguidance --scenario policy-enforcement \
                                                   --policy-name ddos

# List all available policies by archetype
# ✅ Destructive | ✅ Idempotent | ✅ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp azuremigrate platformlandingzone getguidance --scenario policy-assignment \
                                                   --list-policies true
```

#### Platform Landing Zone Management

```bash
# Generate, download, and manage Azure Platform Landing Zones
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp azuremigrate platformlandingzone request --subscription <subscription> \
                                               --resource-group <resource-group> \
                                               --migrate-project-name <migrate-project-name> \
                                               --action <action> \
                                               [action-specific-parameters]
```

**Actions:**

1. **Check Existing** (`--action check`)
   ```bash
   # Check if a platform landing zone already exists
   azmcp azuremigrate platformlandingzone request --subscription <subscription> \
                                                  --resource-group <resource-group> \
                                                  --migrate-project-name <migrate-project-name> \
                                                  --action check
   ```

2. **Update Parameters** (`--action update`)
   ```bash
   # Cache all parameters for generation of the platform landing zone
   # Defaults are applied automatically if not specified
   azmcp azuremigrate platformlandingzone request --subscription <subscription> \
                                                  --resource-group <resource-group> \
                                                  --migrate-project-name <migrate-project-name> \
                                                  --action update \
                                                  [--region-type <single|multi>] \
                                                  [--firewall-type <azurefirewall|nva>] \
                                                  [--network-architecture <hubspoke|vwan>] \
                                                  [--version-control-system <local|github|azuredevops>] \
                                                  [--regions <comma-separated-regions>] \
                                                  [--environment-name <environment-name>] \
                                                  [--organization-name <organization-name>] \
                                                  [--identity-subscription-id <subscription-id>] \
                                                  [--management-subscription-id <subscription-id>] \
                                                  [--connectivity-subscription-id <subscription-id>]
   ```

3. **Generate Landing Zone** (`--action generate`)
   ```bash
   # Generate the platform landing zone
   azmcp azuremigrate platformlandingzone request --subscription <subscription> \
                                                  --resource-group <resource-group> \
                                                  --migrate-project-name <migrate-project-name> \
                                                  --action generate
   ```

4. **Download Landing Zone** (`--action download`)
   ```bash
   # Download generated landing zone files to local workspace
   azmcp azuremigrate platformlandingzone request --subscription <subscription> \
                                                  --resource-group <resource-group> \
                                                  --migrate-project-name <migrate-project-name> \
                                                  --action download
   ```

5. **View Status** (`--action status`)
   ```bash
   # View cached parameters
   azmcp azuremigrate platformlandingzone request --subscription <subscription> \
                                                  --resource-group <resource-group> \
                                                  --migrate-project-name <migrate-project-name> \
                                                  --action status
   ```

6. **Create Azure Migrate Project** (`--action createmigrateproject`)
   ```bash
   # Create a new Azure Migrate project if one doesn't exist (requires location parameter)
   azmcp azuremigrate platformlandingzone request --subscription <subscription> \
                                                  --resource-group <resource-group> \
                                                  --migrate-project-name <migrate-project-name> \
                                                  --action createmigrateproject \
                                                  --location <azure-region>
   ```

### Azure Native ISV Operations

```bash
# List monitored resources in Datadog
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp datadog monitoredresources list --subscription <subscription> \
                                      --resource-group <resource-group> \
                                      --datadog-resource <datadog-resource>
```

### Azure Quick Review CLI Operations

```bash
# Scan a subscription for recommendations
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp extension azqr --subscription <subscription>

# Scan a subscription and scope to a specific resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp extension azqr --subscription <subscription> \
                     --resource-group <resource-group-name>
```

### Azure Quota Operations

```bash
# Get the available regions for the resources types
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp quota region availability list --subscription <subscription> \
                                     --resource-types <resource-types> \
                                     [--cognitive-service-model-name <cognitive-service-model-name>] \
                                     [--cognitive-service-model-version <cognitive-service-model-version>] \
                                     [--cognitive-service-deployment-sku-name <cognitive-service-deployment-sku-name>]

# Check the usage for Azure resources type
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp quota usage check --subscription <subscription> \
                        --region <region> \
                        --resource-types <resource-types>
```

### Azure Policy Operations
```bash
# List Azure Policy Assignments
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp policy assignment list --subscription <subscription> \
                             --scope <scope>
```

### Azure Pricing Operations

```bash
# Get Azure retail pricing information
# Requires at least one filter: --sku, --service, --region, --service-family, --price-type, or --filter
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp pricing get [--sku <sku>] \
                  [--service <service>] \
                  [--region <region>] \
                  [--service-family <service-family>] \
                  [--price-type <price-type>] \
                  [--currency <currency>] \
                  [--include-savings-plan] \
                  [--filter <odata-filter>]
```

| Option | Required | Default | Description |
|--------|----------|---------|-------------|
| `--sku` | No* | - | ARM SKU name (e.g., Standard_D4s_v5) |
| `--service` | No* | - | Azure service name (e.g., Virtual Machines, Storage) |
| `--region` | No* | - | Azure region (e.g., eastus, westeurope) |
| `--service-family` | No* | - | Service family (e.g., Compute, Storage, Databases) |
| `--price-type` | No* | - | Price type (Consumption, Reservation, DevTestConsumption) |
| `--currency` | No | USD | Currency code (e.g., USD, EUR) |
| `--include-savings-plan` | No | false | Include savings plan pricing (uses preview API) |
| `--filter` | No* | - | Raw OData filter for advanced queries |

\* At least one filter option is required.

### Azure RBAC Operations

```bash
# List Azure RBAC role assignments
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp role assignment list --subscription <subscription> \
                           --scope <scope>
```

### Azure Redis Operations

```bash
# Creates a new Azure Managed Redis resource
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp redis create --subscription <subscription> \
                   --resource-group <resource-group> \
                   --name <name> \
                   --sku <sku> \
                   --location <location> \
                   [--modules <modules>]
```

```bash
# Lists all Redis resources
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp redis list --subscription <subscription>
```

### Azure Resource Group Operations

```bash
# List resource groups in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp group list --subscription <subscription>

# List all resources in a resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp group resource list --subscription <subscription> --resource-group <resource-group>
```

### Azure Resource Health Operations

```bash
# Get availability status for a specific resource or list all resources (dual-mode)
# With --resourceId: Get availability status for a specific resource
# Without --resourceId: List availability statuses for all resources in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp resourcehealth availability-status get --subscription <subscription> \
                                              [--resourceId <resource-id>] \
                                              [--resource-group <resource-group>]

# List service health events in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp resourcehealth health-events list --subscription <subscription> \
                                                [--event-type <event-type>] \
                                                [--status <status>] \
                                                [--query-start-time <start-time>] \
                                                [--query-end-time <end-time>]
```

### Azure Service Bus Operations

```bash
# Returns runtime and details about the Service Bus queue
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp servicebus queue details --subscription <subscription> \
                               --namespace <service-bus-namespace> \
                               --queue <queue>

# Gets runtime details a Service Bus topic
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp servicebus topic details --subscription <subscription> \
                               --namespace <service-bus-namespace> \
                               --topic <topic>

# Gets runtime details and message counts for a Service Bus subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp servicebus topic subscription details --subscription <subscription> \
                                            --namespace <service-bus-namespace> \
                                            --topic <topic> \
                                            --subscription-name <subscription-name>
```

### Azure Service Fabric Operations

#### Managed Cluster Node

```bash
# Get nodes for a Service Fabric managed cluster (all nodes, or a single node by name)
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp servicefabric managedcluster node get --subscription <subscription> \
                                            --resource-group <resource-group> \
                                            --cluster <cluster> \
                                            [--node <node>]
```

#### Managed Cluster Node Type

```bash
# Restart nodes of a specific node type in a Service Fabric managed cluster
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp servicefabric managedcluster nodetype restart --subscription <subscription> \
                                                    --resource-group <resource-group> \
                                                    --cluster <cluster> \
                                                    --node-type <node-type> \
                                                    --nodes <node1> [--nodes <node2> ...] \
                                                    [--update-type <Default|ByUpgradeDomain>]
```

### Azure SignalR Service Operations

```bash
# Get detailed properties of SignalR Service runtimes
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp signalr runtime get --subscription <subscription> \
                           [--resource-group <resource-group>] \
                           [--signalr <signalr-name>]
```

### Azure SQL Operations

#### Database

```bash
# Create a SQL database (supports optional performance and configuration parameters)
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
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
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql db delete --subscription <subscription> \
                    --resource-group <resource-group> \
                    --server <server-name> \
                    --database <database-name>

# Gets a list of all databases in a SQL server, or shows details of a specific database
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql db get --subscription <subscription> \
                 --resource-group <resource-group> \
                 --server <server-name> \
                 [--database <database-name>]

# Rename an existing SQL database to a new name within the same server
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql db rename --subscription <subscription> \
                    --resource-group <resource-group> \
                    --server <server-name> \
                    --database <current-database-name> \
                    --new-database-name <new-database-name>

# Update an existing SQL database (applies only the provided configuration changes)
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
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
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql elastic-pool list --subscription <subscription> \
                            --resource-group <resource-group> \
                            --server <server-name>
```

#### Server

```bash
# Create a new SQL server
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql server create --subscription <subscription> \
                        --resource-group <resource-group> \
                        --server <server-name> \
                        --location <location> \
                        --administrator-login <admin-username> \
                        --administrator-password <admin-password> \
                        [--version <server-version>] \
                        [--public-network-access <Enabled|Disabled>]  # Defaults to 'Disabled'

# List Microsoft Entra ID administrators for a SQL server
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql server entra-admin list --subscription <subscription> \
                                  --resource-group <resource-group> \
                                  --server <server-name>

# Create a firewall rule for a SQL server
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql server firewall-rule create --subscription <subscription> \
                                      --resource-group <resource-group> \
                                      --server <server-name> \
                                      --firewall-rule-name <rule-name> \
                                      --start-ip-address <start-ip> \
                                      --end-ip-address <end-ip>

# Delete a firewall rule from a SQL server
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql server firewall-rule delete --subscription <subscription> \
                                      --resource-group <resource-group> \
                                      --server <server-name> \
                                      --firewall-rule-name <rule-name>

# Gets a list of firewall rules for a SQL server
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql server firewall-rule list --subscription <subscription> \
                                  --resource-group <resource-group> \
                                  --server <server-name>

# Delete a SQL server
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql server delete --subscription <subscription> \
                        --resource-group <resource-group> \
                        --server <server-name>

# List SQL servers in a resource group, or show details of a specific SQL server
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql server get --subscription <subscription> \
                     --resource-group <resource-group> \
                     [--server <server-name>]
```

### Azure Storage Operations

#### Account

```bash
# Create a new Storage account with custom configuration
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storage account create --subscription <subscription> \
                             --account <unique-account-name> \
                             --resource-group <resource-group> \
                             --location <location> \
                             --sku <sku> \
                             --access-tier <access-tier> \
                             --enable-hierarchical-namespace false

# Get detailed properties of Storage accounts
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storage account get --subscription <subscription> \
                              [--account <account>] \
                              [--tenant <tenant>]
```

#### Blob Storage

```bash
# Create a blob container with optional public access
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storage blob container create --subscription <subscription> \
                                    --account <account> \
                                    --container <container>

# Get detailed properties of Storage containers
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storage blob container get --subscription <subscription> \
                                     --account <account> \
                                     [--container <container>]

# Get detailed properties of Storage blobs
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storage blob get --subscription <subscription> \
                           --account <account> \
                           --container <container> \
                           [--blob <blob>]

# Upload a file to a Storage blob
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp storage blob upload --subscription <subscription> \
                          --account <account> \
                          --container <container> \
                          --blob <blob> \
                          --local-file-path <path-to-local-file>
```

#### Table Storage

```bash
# List tables in an Azure Storage account
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storage table list --subscription <subscription> \
                         --account <account>
```

### Azure Storage Sync Operations

#### Storage Sync Service

```bash
# Create a new Storage Sync Service for cloud file share synchronization
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync service create --subscription <subscription> \
                                 --resource-group <resource-group> \
                                 --name <service-name> \
                                 --location <location>

# Delete a Storage Sync Service (idempotent – succeeds even if the service does not exist)
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync service delete --subscription <subscription> \
                                 --resource-group <resource-group> \
                                 --name <service-name>

# Get a specific Storage Sync Service or list all services. If --name is provided, returns a specific service; otherwise, lists all services.
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync service get --subscription <subscription> \
                              [--resource-group <resource-group>] \
                              [--name <service-name>]

# Update an existing Storage Sync Service configuration
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync service update --subscription <subscription> \
                                 --resource-group <resource-group> \
                                 --name <service-name> \
                                 [--tags <tag-key=tag-value>]
```

#### Sync Group

```bash
# Create a new Sync Group within a Storage Sync Service
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync syncgroup create --subscription <subscription> \
                                   --resource-group <resource-group> \
                                   --service <service-name> \
                                   --name <syncgroup-name>

# Delete a Sync Group (idempotent – succeeds even if the group does not exist)
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync syncgroup delete --subscription <subscription> \
                                   --resource-group <resource-group> \
                                   --service <service-name> \
                                   --name <syncgroup-name>

# Get a specific Sync Group or list all sync groups. If --name is provided, returns a specific sync group; otherwise, lists all sync groups in the Storage Sync Service.
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync syncgroup get --subscription <subscription> \
                                --resource-group <resource-group> \
                                --service <service-name> \
                                [--name <syncgroup-name>]
```

#### Cloud Endpoint

```bash
# Create a new Cloud Endpoint within a Sync Group
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync cloudendpoint create --subscription <subscription> \
                                       --resource-group <resource-group> \
                                       --service <service-name> \
                                       --syncgroup <syncgroup-name> \
                                       --name <endpoint-name> \
                                       --storage-account <storage-account-name> \
                                       --share <share-name>

# Delete a Cloud Endpoint (idempotent – succeeds even if the endpoint does not exist)
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync cloudendpoint delete --subscription <subscription> \
                                       --resource-group <resource-group> \
                                       --service <service-name> \
                                       --syncgroup <syncgroup-name> \
                                       --name <endpoint-name>

# Get a specific Cloud Endpoint or list all cloud endpoints. If --name is provided, returns a specific cloud endpoint; otherwise, lists all cloud endpoints in the Sync Group.
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync cloudendpoint get --subscription <subscription> \
                                    --resource-group <resource-group> \
                                    --service <service-name> \
                                    --syncgroup <syncgroup-name> \
                                    [--name <endpoint-name>]

# Trigger change detection on a Cloud Endpoint
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync cloudendpoint changedetection --subscription <subscription> \
                                                --resource-group <resource-group> \
                                                --service <service-name> \
                                                --syncgroup <syncgroup-name> \
                                                --name <endpoint-name> \
                                                --directory-path <path> \
                                                [--change-detection-mode <mode>] \
                                                [--paths <path1> <path2> ...]
```

#### Registered Server

```bash
# Get a specific Registered Server or list all registered servers. If --server is provided, returns a specific registered server; otherwise, lists all registered servers in the Storage Sync Service.
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync registeredserver get --subscription <subscription> \
                                       --resource-group <resource-group> \
                                       --service <service-name> \
                                       [--server <server-name>]

# Unregister a server from a Storage Sync Service
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync registeredserver unregister --subscription <subscription> \
                                              --resource-group <resource-group> \
                                              --service <service-name> \
                                              --server <server-name>

# Update a Registered Server configuration
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync registeredserver update --subscription <subscription> \
                                          --resource-group <resource-group> \
                                          --service <service-name> \
                                          --server <server-name> \
                                          [--certificate <certificate-path>]
```

#### Server Endpoint

```bash
# Create a new Server Endpoint within a Sync Group
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync serverendpoint create --subscription <subscription> \
                                        --resource-group <resource-group> \
                                        --service <service-name> \
                                        --syncgroup <syncgroup-name> \
                                        --server <server-name> \
                                        --name <endpoint-name> \
                                        --server-local-path <local-path>

# Delete a Server Endpoint (idempotent – succeeds even if the endpoint does not exist)
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync serverendpoint delete --subscription <subscription> \
                                        --resource-group <resource-group> \
                                        --service <service-name> \
                                        --syncgroup <syncgroup-name> \
                                        --name <endpoint-name>

# Get a specific Server Endpoint or list all server endpoints. If --name is provided, returns a specific server endpoint; otherwise, lists all server endpoints in the Sync Group.
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync serverendpoint get --subscription <subscription> \
                                     --resource-group <resource-group> \
                                     --service <service-name> \
                                     --syncgroup <syncgroup-name> \
                                     [--name <endpoint-name>]

# Update a Server Endpoint configuration
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp storagesync serverendpoint update --subscription <subscription> \
                                        --resource-group <resource-group> \
                                        --service <service-name> \
                                        --syncgroup <syncgroup-name> \
                                        --name <endpoint-name> \
                                        [--cloud-tiering <Enabled|Disabled>] \
                                        [--tiering-policy-days <days>] \
                                        [--tiering-policy-volume-free-percent <percent>]
```

### Azure Subscription Management

```bash
# List available Azure subscriptions with default subscription indicator
# Returns subscriptionId, displayName, state, tenantId, and isDefault for each subscription
# The isDefault field is true for the default subscription set via 'az account set' or AZURE_SUBSCRIPTION_ID env var
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp subscription list [--tenant-id <tenant-id>]
```

### Azure Terraform Best Practices

```bash
# Get secure, production-grade Azure Terraform best practices for effective code generation and command execution.
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp azureterraformbestpractices get
```

### Azure Virtual Desktop Operations

```bash
# List Azure Virtual Desktop host pools in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp virtualdesktop hostpool list --subscription <subscription> \
                                   [--resource-group <resource-group>]

# List session hosts in a host pool
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp virtualdesktop hostpool host list --subscription <subscription> \
                                               [--hostpool <hostpool-name> | --hostpool-resource-id <hostpool-resource-id>] \
                                               [--resource-group <resource-group>]

# List user sessions on a session host
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp virtualdesktop hostpool host user-list --subscription <subscription> \
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
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp virtualdesktop hostpool list --subscription <subscription>

# Optimized usage - lists host pools in specific resource group only
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp virtualdesktop hostpool list --subscription <subscription> \
                                   --resource-group <resource-group>
```

**Session Host Usage patterns:**

```bash
# Standard usage - enumerates all host pools in subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp virtualdesktop hostpool host list --subscription <subscription> \
                                                --hostpool <hostpool-name>

# Optimized usage - direct resource group access
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp virtualdesktop hostpool host list --subscription <subscription> \
                                                --hostpool <hostpool-name> \
                                                --resource-group <resource-group>

# Alternative with resource ID (no resource group needed)
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp virtualdesktop hostpool host list --subscription <subscription> \
                                                --hostpool-resource-id /subscriptions/<sub>/resourceGroups/<rg>/providers/Microsoft.DesktopVirtualization/hostPools/<pool>
```

### Azure Well-Architected Framework Operations

```bash
# Get Azure Well-Architected Framework guidance for a specific Azure service or list all supported services.
# If --service is provided, returns guidance for that specific service; otherwise, lists all supported services.
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp wellarchitectedframework serviceguide get [--service <service-name>]
```

### Azure Workbooks Operations

```bash
# Create a new workbook
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp workbooks create --subscription <subscription> \
                       --resource-group <resource-group> \
                       --display-name <display-name> \
                       --serialized-content <json-content> \
                       [--source-id <source-id>]

# Delete a workbook
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp workbooks delete --workbook-id <workbook-resource-id>

# List Azure Monitor workbooks in a resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp workbooks list --subscription <subscription> \
                     --resource-group <resource-group> \
                     [--category <category>] \
                     [--kind <kind>] \
                     [--source-id <source-id>]

# Show details of a specific workbook by resource ID
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp workbooks show --workbook-id <workbook-resource-id>

# Update an existing workbook
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp workbooks update --workbook-id <workbook-resource-id> \
                       [--display-name <display-name>] \
                       [--serialized-content <json-content>]
```

### Bicep

```bash
# Get Bicep schema for a specific Azure resource type
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp bicepschema get --resource-type <resource-type> \
```

### Cloud Architect

```bash
# Design Azure cloud architectures through guided questions
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp cloudarchitect design [--question <question>] \
                            [--question-number <question-number>] \
                            [--total-questions <total-questions>] \
                            [--answer <answer>] \
                            [--next-question-needed <true/false>] \
                            [--confidence-score <confidence-score>] \
                            [--architecture-component <architecture-component>]

# Example:
# Start an interactive architecture design session
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
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

When invoking `azmcp tools list` (with or without `--namespace-mode`), each returned object now includes a `count` field:

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
