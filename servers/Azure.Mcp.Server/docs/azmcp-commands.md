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
| `--transport` | No | `stdio` | Transport mechanism to use (currently only `stdio` is supported) |
| `--mode` | No | `namespace` | Server mode: `namespace` (default), `consolidated`, `all`, or `single` |
| `--namespace` | No | All namespaces | Specific Azure service namespaces to expose (can be repeated). Works with all exiting modes to filter tools. |
| `--tool` | No | All tools | Expose specific tools by name (e.g., 'azmcp_storage_account_get'). It automatically switches to `all` mode. It can't be used together with `--namespace`. |
| `--read-only` | No | `false` | Only expose read-only operations |
| `--debug` | No | `false` | Enable verbose debug logging to stderr |
| `--dangerously-disable-http-incoming-auth` | No | false | Dangerously disable HTTP incoming authentication |
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

#### Server Info

```bash
# Get information about the MCP server, which includes the server's name and version.
azmcp server info
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
# List Cosmos DB accounts in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp cosmos account list --subscription <subscription>

# Query items in a Cosmos DB container
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp cosmos database container item query --subscription <subscription> \
                                           --account <account> \
                                           --database <database> \
                                           --container <container> \
                                           [--query "SELECT * FROM c"]

# List containers in a Cosmos DB database
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp cosmos database container list --subscription <subscription> \
                                     --account <account> \
                                     --database <database>

# List databases in a Cosmos DB account
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp cosmos database list --subscription <subscription> \
                           --account <account>
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

#### Database

```bash
# List all databases in a MySQL server
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp mysql database list --subscription <subscription> \
                          --resource-group <resource-group> \
                          --user <user> \
                          --server <server>

# Executes a SELECT query on a MySQL Database. The query must start with SELECT and cannot contain any destructive SQL operations for security reasons.
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
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
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp mysql table list --subscription <subscription> \
                       --resource-group <resource-group> \
                       --user <user> \
                       --server <server> \
                       --database <database>

# Get the schema of a specific table in a MySQL database
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
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
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp mysql server config get --subscription <subscription> \
                              --resource-group <resource-group> \
                              --user <user> \
                              --server <server>

# List all MySQL servers in a subscription & resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp mysql server list --subscription <subscription> \
                        --resource-group <resource-group> \
                        --user <user>

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

#### Database

```bash
# List all databases in a PostgreSQL server
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp postgres database list --subscription <subscription> \
                             --resource-group <resource-group> \
                             --user <user> \
                             --server <server>

# Execute a query on a PostgreSQL database
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
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
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp postgres table list --subscription <subscription> \
                          --resource-group <resource-group> \
                          --user <user> \
                          --server <server> \
                          --database <database>

# Get the schema of a specific table in a PostgreSQL database
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
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
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp postgres server config get --subscription <subscription> \
                                 --resource-group <resource-group> \
                                 --user <user> \
                                 --server <server>

# List all PostgreSQL servers in a subscription & resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp postgres server list --subscription <subscription> \
                           --resource-group <resource-group> \
                           --user <user>

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
azmcp deploy pipeline guidance get [--use-azd-pipeline-config <use-azd-pipeline-config>] \
                                   [--organization-name <organization-name>] \
                                   [--repository-name <repository-name>] \
                                   [--github-environment-name <github-environment-name>]

# Get a deployment plan for a specific project
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp deploy plan get --workspace-folder <workspace-folder> \
                      --project-name <project-name> \
                      --target-app-service <target-app-service> \
                      --provisioning-tool <provisioning-tool> \
                      [--azd-iac-options <azd-iac-options>]
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

### Azure Function App Operations

```bash
# Get detailed properties of function apps
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp functionapp get --subscription <subscription> \
                      [--resource-group <resource-group>] \
                      [--function-app <function-app-name>]
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

# Gets a certificate in a key vault
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp keyvault certificate get --subscription <subscription> \
                               --vault <vault-name> \
                               --name <certificate-name>

# Imports an existing certificate (PFX or PEM) into a key vault
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ✅ LocalRequired
azmcp keyvault certificate import --subscription <subscription> \
                                  --vault <vault-name> \
                                  --certificate <certificate-name> \
                                  --certificate-data <path-or-base64-or-raw-pem> \
                                  [--password <pfx-password>]

# Lists certificates in a key vault
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp keyvault certificate list --subscription <subscription> \
                                --vault <vault-name>
```

#### Keys

```bash
# Creates a key in a key vault
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp keyvault key create --subscription <subscription> \
                          --vault <vault-name> \
                          --key <key-name> \
                          --key-type <key-type>

# Get a key in a key vault
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp keyvault key get --subscription <subscription> \
                       --vault <vault-name> \
                       --key <key-name>

# Lists keys in a key vault
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
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
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp keyvault secret create --subscription <subscription> \
                             --vault <vault-name> \
                             --name <secret-name> \
                             --value <secret-value>

# Get a secret in a key vault (will prompt for user consent)
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ✅ Secret | ❌ LocalRequired
azmcp keyvault secret get --subscription <subscription> \
                          --vault <vault-name> \
                          --secret <secret-name>

# Lists secrets in a key vault
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp keyvault secret list --subscription <subscription> \
                           --vault <vault-name>
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
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp loadtesting test get --subscription <subscription> \
                           --resource-group <resource-group> \
                           --test-resource-name <test-resource-name> \
                           --test-id <test-id>

# Create load test resources
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp loadtesting testresource create --subscription <subscription> \
                                      --resource-group <resource-group> \
                                      --test-resource-name <test-resource-name>

# List load test resources
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp loadtesting testresource list --subscription <subscription> \
                                    --resource-group <resource-group> \
                                    --test-resource-name <test-resource-name>

# Create load test run
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp loadtesting testrun create --subscription <subscription> \
                                 --resource-group <resource-group> \
                                 --test-resource-name <test-resource-name> \
                                 --test-id <test-id> \
                                 --testrun-id <testrun-id> \
                                 --display-name <display-name> \
                                 --description <description> \
                                 --old-testrun-id <old-testrun-id>

# Get load test run
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp loadtesting testrun get --subscription <subscription> \
                              --resource-group <resource-group> \
                              --test-resource-name <test-resource-name> \
                              --testrun-id <testrun-id>

# List load test run
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp loadtesting testrun list --subscription <subscription> \
                               --resource-group <resource-group> \
                               --test-resource-name <test-resource-name> \
                               --test-id <test-id>

# Update load test run
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
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
azmcp get bestpractices get --resource <resource> --action <action>

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
# Call this before generating code for any AI application, building with Microsoft Foundry models,
# working with Microsoft Agent Framework, or implementing AI solutions in Azure.
azmcp get bestpractices ai_app

# AI App Development:
#   ai_app - Comprehensive guidance for AI applications including:
#     • Microsoft Agent Framework usage and patterns
#     • Microsoft Foundry model selection
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
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
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
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp monitor webtests list --subscription <subscription> [--resource-group <resource-group>]

# Update an existing web test in Azure Monitor
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
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

### Azure Managed Lustre

```bash
# List Azure Managed Lustre Filesystems available in a subscription or resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs list --subscription <subscription> \
                            --resource-group <resource-group>

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
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs autoexport-job create --subscription <subscription> \
                                             --resource-group <resource-group> \
                                             --filesystem-name <filesystem-name>

# Cancel an autoexport job for an Azure Managed Lustre filesystem
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs autoexport-job cancel --subscription <subscription> \
                                             --resource-group <resource-group> \
                                             --filesystem-name <filesystem-name> \
                                             --job-name <job-name>

# Get details of an autoexport job for an Azure Managed Lustre filesystem
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs autoexport-job get --subscription <subscription> \
                                          --resource-group <resource-group> \
                                          --filesystem-name <filesystem-name> \
                                          --job-name <job-name>

# List all autoexport jobs for an Azure Managed Lustre filesystem
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs autoexport-job list --subscription <subscription> \
                                           --resource-group <resource-group> \
                                           --filesystem-name <filesystem-name>

# Delete an autoexport job for an Azure Managed Lustre filesystem
# ✅ Destructive | ✅ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp managedlustre fs autoexport-job delete --subscription <subscription> \
                                             --resource-group <resource-group> \
                                             --filesystem-name <filesystem-name> \
                                             --job-name <job-name>
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
```

### Azure Resource Health Operations

```bash
# Get availability status for a specific resource
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp resourcehealth availability-status get --resourceId <resource-id>

# List availability statuses for all resources in a subscription
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp resourcehealth availability-status list --subscription <subscription> \
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

# Gets a list of all databases in a SQL server
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql db list --subscription <subscription> \
                  --resource-group <resource-group> \
                  --server <server-name>

# Rename an existing SQL database to a new name within the same server
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql db rename --subscription <subscription> \
                    --resource-group <resource-group> \
                    --server <server-name> \
                    --database <current-database-name> \
                    --new-database-name <new-database-name>

# Show details of a specific SQL database
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql db show --subscription <subscription> \
                  --resource-group <resource-group> \
                  --server <server-name> \
                  --database <database>

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
                        --admin-user <admin-username> \
                        --admin-password <admin-password> \
                        [--version <server-version>] \
                        [--public-network-access <enabled|disabled>]

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

# List SQL servers in a resource group
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql server list --subscription <subscription> \
                      --resource-group <resource-group>

# Show details of a specific SQL server
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp sql server show --subscription <subscription> \
                      --resource-group <resource-group> \
                      --server <server-name>
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

### Azure Subscription Management

```bash
# List available Azure subscriptions
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

### Microsoft Foundry Operations

```bash
# Create an agent in a Microsoft Foundry project
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry agents create --endpoint <endpoint> \
                            --model-deployment <model-deployment> \
                            --agent-name <agent-name> \
                            --systemInstruction <system-instruction>

# Connect to an agent in a Microsoft Foundry project and query it
# ❌ Destructive | ❌ Idempotent | ✅ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry agents connect --agent-id <agent-id> \
                             --query <query> \
                             --endpoint <endpoint>

# Evaluate a response from an agent by passing query and response inline
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry agents evaluate --agent-id <agent-id> \
                              --query <query> \
                              --response <response> \
                              --evaluator <evaluator> \
                              --azure-openai-endpoint <azure-openai-endpoint> \
                              --azure-openai-deployment <azure-openai-deployment> \
                              [--tool-definitions <tool-definitions>]

# Get SDK samples for interacting with a Microsoft Foundry agent
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry agents get-sdk-sample --programming-language <python|typescript|csharp>

# List all Azure AI Agents available in the configured project
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry agents list --endpoint <endpoint>

# Query and evaluate an agent in one command
# ❌ Destructive | ❌ Idempotent | ✅ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry agents query-and-evaluate --agent-id <agent-id> \
                                        --query <query> \
                                        --endpoint <endpoint> \
                                        --azure-openai-endpoint <azure-openai-endpoint> \
                                        --azure-openai-deployment <azure-openai-deployment> \
                                        [--evaluators <evaluators>]

# List knowledge indexes in a Microsoft Foundry project
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry knowledge index list --endpoint <endpoint>

# Get knowledge index schema information
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry knowledge index schema --endpoint <endpoint> \
                                     --index <index>

# Deploy a Microsoft Foundry model
# ✅ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
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

# List Microsoft Foundry model deployments
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry models deployments list --endpoint <endpoint>

# List Microsoft Foundry models
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry models list [--search-for-free-playground <search-for-free-playground>] \
                          [--publisher <publisher>] \
                          [--license <license>] \
                          [--model-name <model>]

# Create interactive chat completions using Azure OpenAI chat models
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
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

# Generate text completions using deployed Azure OpenAI models in Microsoft Foundry
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry openai create-completion --subscription <subscription> \
                                       --resource-group <resource-group> \
                                       --resource-name <resource-name> \
                                       --deployment <deployment-name> \
                                       --prompt-text <prompt-text> \
                                       [--max-tokens <max-tokens>] \
                                       [--temperature <temperature>]

# Generate vector embeddings for text using Azure OpenAI embedding models
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
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
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry openai models-list --subscription <subscription> \
                                 --resource-group <resource-group> \
                                 --resource-name <resource-name> \
                                 [--auth-method <auth-method>]

# Get Microsoft Foundry (Cognitive Services) resource details
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry resource get --subscription <subscription> \
                           [--resource-group <resource-group>] \
                           [--resource-name <resource-name>]

# Create a Microsoft Foundry agent thread
# ❌ Destructive | ❌ Idempotent | ❌ OpenWorld | ❌ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry threads create --endpoint <endpoint>
                             --user-message <user-message>

# Get messages of a Microsoft Foundry agent thread
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry threads get-messages --endpoint <endpoint>
                                   --thread-id <thread-id>

# List Microsoft Foundry agent threads in a Foundry project
# ❌ Destructive | ✅ Idempotent | ❌ OpenWorld | ✅ ReadOnly | ❌ Secret | ❌ LocalRequired
azmcp foundry threads list --endpoint <endpoint>
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
