# Tool Description Evaluator – Quickstart Guide

This tool helps you test and validate the descriptions of new Azure MCP Server tools. It checks how well your tool descriptions match real user prompts, ensuring users get the right tool when they ask for something.

The tool returns a confidence score between `0.00` and `1.00` for each tool-prompt combination. The higher the score, the better chance the tool will be selected given a specific prompt. Scores below `0.40` generally indicate low tool selection chances and tool descriptions should be improved.

## What It Does

- Loads your tool definitions
- Loads test prompts
- Uses Azure OpenAI embeddings to compare prompts and tool descriptions
- Scores how well each description matches each prompt
- Reports which tools are most likely to be selected for each prompt

## How To Use

### Requirements

An Azure OpenAI deployment of the text embedding model `text-embedding-3-large`.

> For internal contributors, refer to the **Before creating a pull request** section of [this document](https://eng.ms/docs/products/azure-developer-experience/mcp/mcp-getting-started) to use our team's deployment.

### Minimal Setup

Set your Azure OpenAI endpoint and API key as environment variables:

```bash
export AOAI_ENDPOINT="https://<your-resource>.openai.azure.com/openai/deployments/<embeddings-deployment-name>/embeddings?api-version=<api-version>"
export TEXT_EMBEDDING_API_KEY="your_api_key_here"
```

Or copy `.env.example` to `.env` and fill in your credentials.

### Typical Workflow

1. Add or update a tool description in the project
2. Add test prompts for your tool to the appropriate server's test prompts file:
   - For Azure tools: `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`
   - For Fabric tools: `servers/Fabric.Mcp.Server/docs/e2eTestPrompts.md`
3. Run the analyzer using PowerShell

    ```pwsh
    # For Azure MCP Server (default)
    ./Run-ToolDescriptionEvaluator.ps1

    # For Fabric MCP Server
    ./Run-ToolDescriptionEvaluator.ps1 -Server "Fabric"

    # For a specific server executable
    ./Run-ToolDescriptionEvaluator.ps1 -ServerExe "./path/to/azmcp.exe"
    ```

4. Check if your tool ranks in the top 3 for the prompts (ideally #1) and with a score of at least `0.4`
5. Refine the description if needed and try again

### Testing a Single Tool Description

When developing a new tool, you can test its description directly without adding it to the system first:

```bash
# Test a single tool description against one prompt
dotnet run -- --test-single-tool \
  --tool-description "Lists all storage accounts in a subscription" \
  --prompt "show me my storage accounts"

# Test against multiple prompts
dotnet run -- --test-single-tool \
  --tool-description "Retrieves secrets from Azure Key Vault" \
  --prompt "get my secret from Key Vault" \
  --prompt "show me secrets in my vault" \
  --prompt "what secrets do I have"
```

**Note:** In `--test-single-tool` mode, the following arguments are ignored: `--area`, `--server`, `--server-exe`, `--prompts-file`

## Additional Usage Options

### Testing Different Servers

By default, the tool tests Azure MCP Server tools. To test other servers:

```bash
# Test Fabric MCP Server tools
dotnet run -- --server "Fabric"

# Use a specific executable path (useful for custom builds)
dotnet run -- --server-exe "./path/to/fabmcp.dll"
```

### Filtering by Service Area

Test specific service tools by using the `--area` parameter:

```bash
# Test only Azure Storage tools
dotnet run -- --area "storage"

# Test only Azure Key Vault tools
dotnet run -- --area "keyvault"

# Test multiple services
dotnet run -- --area "storage,keyvault,sql"

# Combine server selection with area filtering
dotnet run -- --server "Fabric" --area "workspace"
```

## Why Use This Tool?

- Quickly validate new tool descriptions
- Ensure users get the right tool for their requests
- No need to learn all options—just run and review results

For more details and usage options, see the full [README](https://github.com/microsoft/mcp/blob/main/eng/tools/ToolDescriptionEvaluator/README.md).
