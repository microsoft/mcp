# Tool Description Evaluator

This is a testing and analysis tool that evaluates how well Azure MCP Server tool descriptions match user prompts using AI embeddings. It helps ensure that the right tools are selected when users ask questions or make requests.

## Overview

The application:

1. Loads tool definitions from the Azure MCP Server (dynamically or from JSON files)
1. Loads test prompts from markdown or JSON files (default: `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`)
1. Creates embeddings for tool descriptions using Azure OpenAI's `text-embedding-3-large` model
1. Tests prompt-to-tool matching using vector similarity search with cosine similarity
1. Generates confidence scores and analysis reports to identify gaps in tool selection accuracy

## Project Structure

```text
.
├── Program.cs                              # Main application logic
├── Models/                                 # Data models for tools, prompts, and results
├── Services/                               # Embedding and analysis services
├── VectorDb/                               # Vector database implementation with cosine similarity
├── ToolDescriptionEvaluator.csproj         # Project file
├── tools.json                              # Tool definitions (fallback/static)
├── prompts.json                            # Test prompts (fallback/static)
├── .env.example                            # Environment variables template
├── results.md                              # Analysis output (markdown)
└── README.md                               # This file
```

## Usage Modes

### 1. Full Analysis Mode (Default)

Runs comprehensive analysis on all tools and prompts:

```bash
dotnet run
```

### 2. Validation Mode

Tests a specific tool description against one or more prompts:

```bash
# Single prompt validation
dotnet run -- --validate \
  --tool-description "Lists all storage accounts in a subscription" \
  --prompt "show me my storage accounts"

# Multiple prompt validation
dotnet run -- --validate \
  --tool-description "Lists storage accounts" \
  --prompt "show me storage accounts" \
  --prompt "list my storage accounts" \
  --prompt "what storage accounts do I have"
```

### 3. Tool Prefix Filtering Mode

Filter prompts by tool name prefix to test specific Azure service tools. Service names are automatically prefixed with `azmcp_`:

```bash
# Filter by service name (auto-prefixed to azmcp_*)
dotnet run -- --area "keyvault"      # Filters azmcp_keyvault_* tools
dotnet run -- --area "storage"       # Filters azmcp_storage_* tools
dotnet run -- --area "functionapp"   # Filters azmcp_functionapp_* tools
dotnet run -- --area "sql"           # Filters azmcp_sql_* tools
dotnet run -- --area "cosmos"        # Filters azmcp_cosmos_* tools

# Filter multiple services at once (comma-separated)
dotnet run -- --area "keyvault,storage"       # Filters both Key Vault and Storage tools
dotnet run -- --area "sql,cosmos,storage"     # Filters SQL, Cosmos, and Storage tools

# Or use explicit prefix (same result)
dotnet run -- --area "azmcp_keyvault"
```

### 4. Custom Files Mode

Use custom tools or prompts files:

```bash
# Use custom tools file
dotnet run -- --tools-file my-tools.json

# Use custom prompts file (supports .md or .json)
dotnet run -- --prompts-file my-prompts.md

# Use both custom files with prefix filtering
dotnet run -- --tools-file my-tools.json --prompts-file my-prompts.json --area "storage"
```

## Input Data Sources

The tool can load data from multiple sources:

### Tool Definitions

- **Dynamic loading** (default): Queries Azure MCP Server directly for current tool definitions
- **Static JSON file**: Uses `tools.json` or custom file specified with `--tools-file`

### Test Prompts

- **Markdown format** (default): Uses `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`
- **JSON format**: Uses `prompts.json` or custom file specified with `--prompts-file`
- **Custom files**: Supports both `.md` and `.json` formats

## Quick Start

You can call the build script in this directory:

```bash
# Run with all areas
./Run-ToolDescriptionEvaluator.ps1

# Run with specific tool prefix filtering
./Run-ToolDescriptionEvaluator.ps1 -Area "storage"
./Run-ToolDescriptionEvaluator.ps1 -Area "keyvault"
./Run-ToolDescriptionEvaluator.ps1 -Area "functionapp"

# Run with multiple areas (comma-separated)
./Run-ToolDescriptionEvaluator.ps1 -Area "keyvault,storage"
./Run-ToolDescriptionEvaluator.ps1 -Area "sql,cosmos,functionapp"

# Build Azure MCP Server first, then run with prefix filtering
./Run-ToolDescriptionEvaluator.ps1 -BuildAzureMcp -Area "sql"
```

or run the following commands directly:

```bash
dotnet build
dotnet run

# With tool prefix filtering
dotnet run -- --area "storage"
```

## Setup

### Environment Configuration

This application requires two environment variables to be configured:

#### Required Environment Variables

1. **`AOAI_ENDPOINT`** - Your Azure OpenAI endpoint base URL (without deployment path)
2. **`TEXT_EMBEDDING_API_KEY`** - Your Azure OpenAI API key

#### Option 1: Environment Variables (Recommended for Production)

Set both required environment variables:

```bash
export AOAI_ENDPOINT="https://<your-resource>.openai.azure.com/openai/deployments/<embeddings-deployment-name>/embeddings?api-version=<api-version>"
export TEXT_EMBEDDING_API_KEY="your_api_key_here"
```

#### Option 2: .env File (Recommended for Local Development)

1. Copy the example environment file:

   ```bash
   cp .env.example .env
   ```

2. Edit `.env` and add both required variables:

   ```env
   AOAI_ENDPOINT="https://<your-resource>.openai.azure.com/openai/deployments/<embeddings-deployment-name>/embeddings?api-version=<api-version>"
   TEXT_EMBEDDING_API_KEY=your_actual_api_key_here
   ```

## Output Formats and Analysis

The tool generates detailed analysis reports in two formats:

### Markdown Output (Default)

Generate structured markdown reports:

```bash
dotnet run
```

Results are written to `results.md` with:

- 📊 **Structured layout** with headers and navigation
- 📋 **Table of Contents** with clickable links  
- 📈 **Results tables** with visual indicators (✅/❌)
- 📊 **Success rate analysis** with performance ratings
- 🕐 **Execution timing** and statistics

### Plain Text Output

Results are written to `results.txt` when using the following option:

```bash
dotnet run -- --text
```

- Compact, simple format for quick review
- Includes confidence scores and success rates
- Shows top matching tools for each prompt

### Command Line Options

The tool supports several command line options for customization:

```bash
# Tool prefix filtering
dotnet run -- --area "storage"                    # Filter to storage tools only (auto-prefixed to azmcp_storage)
dotnet run -- --area "keyvault"                   # Filter to Key Vault tools only (auto-prefixed to azmcp_keyvault)
dotnet run -- --area "functionapp"                # Filter to Function App tools only (auto-prefixed to azmcp_functionapp)
dotnet run -- --area "keyvault,storage"           # Filter to multiple areas (comma-separated)
dotnet run -- --area "sql,cosmos,functionapp"     # Filter to SQL, Cosmos, and Function App tools

# File options
dotnet run -- --tools-file my-tools.json          # Use custom tools file
dotnet run -- --prompts-file my-prompts.md        # Use custom prompts file
dotnet run -- --output-file-name my-tests         # Custom output filename

# Output format
dotnet run -- --text-results                      # Output in plain text format

# Result limits
dotnet run -- --top 10                           # Show top 10 results per test

# CI mode
dotnet run -- --ci                               # Run in CI mode (graceful failures)

# Combined options
dotnet run -- --area "keyvault" --text-results --top 3
```

### Analysis Metrics

The tool provides several key metrics:

- **Confidence scores:** Cosine similarity (0.0–1.0) between prompts and tool descriptions
- **Top choice success rate:** Percentage of prompts where the expected tool ranked #1
- **Confidence level distribution:** Share of prompts by confidence band
  - Very High (≥0.8)
  - High (≥0.7)
  - Good (≥0.6)
  - Fair (≥0.5)
  - Acceptable (≥0.4)
  - Low (<0.4)
- **Top choice + confidence combinations:** How often the expected top match also meets each band (≥0.8, ≥0.7, ≥0.6, ≥0.5, ≥0.4)
- **Performance ratings** (based on Top choice + Acceptable confidence ≥0.4):
  - 🟢 Excellent (≥90%)
  - 🟡 Good (≥75% and <90%)
  - 🟠 Fair (≥50% and <75%)
  - 🔴 Poor (<50%)

## Configuration Files

### Test Prompts Format

#### Markdown Format (Default)

The tool reads from `../../../servers/Azure.Mcp.Server/docs/e2eTestPrompts.md` which contains tables organized by service:

```markdown
## Azure Storage

| Tool Name | Test Prompt |
|:----------|:------------|
| azmcp_storage_account_get | List all storage accounts in my subscription |
| azmcp_storage_account_get | Show me my storage accounts |
| azmcp_storage_container_get | List containers in storage account <account-name> |

## Azure Key Vault

| Tool Name | Test Prompt |
|:----------|:------------|
| azmcp_keyvault_secret_get | Get my secret from Key Vault |
| azmcp_keyvault_key_list | List all keys in my Key Vault |
```

#### Tool Prefix Filtering

The tool supports filtering by tool name prefixes using the `--area` parameter. This allows you to test all tools for a specific Azure service by matching the tool name prefix.

For example, `--area "keyvault"` (auto-prefixed to `azmcp_keyvault`) will match all tools starting with `azmcp_keyvault` including:
- `azmcp_keyvault_certificate_create`
- `azmcp_keyvault_certificate_get`  
- `azmcp_keyvault_secret_get`
- `azmcp_keyvault_key_list`
- And all other Key Vault tools

```bash
# Filter by service name (automatically prefixed with azmcp_)
dotnet run -- --area "keyvault"       # Matches all azmcp_keyvault_* tools
dotnet run -- --area "storage"        # Matches all azmcp_storage_* tools
dotnet run -- --area "functionapp"    # Matches all azmcp_functionapp_* tools

# Filter multiple services at once (comma-separated)
dotnet run -- --area "keyvault,storage"        # Matches Key Vault and Storage tools
dotnet run -- --area "sql,cosmos,functionapp"  # Matches SQL, Cosmos, and Function App tools

# Or use explicit prefix (same result)
dotnet run -- --area "azmcp_keyvault"
```

Common service names (automatically prefixed with `azmcp_`) include: `foundry`, `search`, `appconfig`, `applens`, `appservice`, `applicationinsights`, `acr`, `cosmos`, `kusto`, `mysql`, `postgres`, `eventgrid`, `functionapp`, `keyvault`, `aks`, `loadtesting`, `monitor`, `quota`, `redis`, `storage`, `servicebus`, `sql`, `virtualdesktop`, `workbooks`, and more.

#### JSON Format (Alternative)

Prompts can be organized in JSON format:

```json
{
  "azmcp-storage-account-get": [
    "List all storage accounts in my subscription",
    "Show me my storage accounts"
  ],
  "azmcp-storage-container-get": [
    "List containers in storage account <account-name>"
  ]
}
```

### Tool Definitions File

Contains complete tool metadata including:

- Tool names and descriptions
- Input parameter schemas  
- Usage examples and annotations

## Use Cases

This tool is valuable for:

- **Quality Assurance**: Ensuring tool descriptions accurately match user intents
- **Tool Development**: Testing new tool descriptions before deployment  
- **Performance Monitoring**: Tracking how well the MCP server matches user prompts
- **Documentation**: Identifying gaps in tool coverage or unclear descriptions
- **Regression Testing**: Verifying that changes don't break existing prompt-to-tool matching

## Security and Best Practices

- **Never commit API keys to version control**
- Use environment variables in production environments
- Use `.env` files for local development (they're automatically gitignored)
- Rotate your Azure OpenAI API keys regularly
- Use least-privilege access principles for Azure OpenAI resources
- Consider using Azure Key Vault for production deployments

## CI/CD Integration

The tool supports CI mode with graceful handling of missing credentials:

```bash
dotnet run -- --ci
```

In CI mode, the tool will:

- Skip analysis if Azure OpenAI credentials are not available
- Exit with code 0 (success) instead of failing
- Log helpful messages about what was skipped
