# Microsoft Foundry Extensions Testing Scenario

> **MCP Tool Support Notice**
> Azure MCP Server provides **Microsoft Foundry resource inspection, Azure OpenAI model/deployment listing, completions, embeddings, and knowledge-index inspection** through the Foundry Extensions toolset. Resource creation, deployment, and agent lifecycle management require other tools such as Azure CLI or the Azure portal.

## Objectives

- Test Microsoft Foundry resource discovery and inspection
- Test Azure OpenAI model and deployment listing
- Validate completion and embedding tool discovery
- Test knowledge-index listing and schema inspection when indexes are available

## Prerequisites

- [ ] Azure MCP Server installed and configured
- [ ] Azure CLI installed (`az --version`)
- [ ] Authenticated to Azure (`az login`)
- [ ] Active Azure subscription
- [ ] GitHub Copilot with Agent mode enabled

---

## Scenario 1: Microsoft Foundry Resource Discovery and OpenAI Models

**Objective**: Discover Microsoft Foundry resources and inspect Azure OpenAI models and deployments

### Step 1: Setup Resources (External - Not MCP)

> **External Setup Required**: The Foundry Extensions toolset does not create Foundry resources or model deployments. Use Azure CLI or the Azure portal for setup.

**Option A: Prompt GitHub Copilot Chat** (Recommended):
```
Create an Azure resource group 'bugbash-foundry-rg' in eastus, then create an Azure AI Services account with SKU S0, and deploy GPT-4o model with deployment name 'gpt-4o-deployment'
```

**Option B: Run Azure CLI Commands Manually**:
```bash
# Create resource group
az group create --name bugbash-foundry-rg --location eastus

# Create Microsoft Foundry resource (AI Services account)
az cognitiveservices account create \
  --name bugbash-ai-foundry-$RANDOM \
  --resource-group bugbash-foundry-rg \
  --location eastus \
  --kind AIServices \
  --sku S0

# Deploy a model (GPT-4o)
az cognitiveservices account deployment create \
  --name <account-name-from-above> \
  --resource-group bugbash-foundry-rg \
  --deployment-name gpt-4o-deployment \
  --model-name gpt-4o \
  --model-version "2024-05-13" \
  --model-format OpenAI \
  --sku-capacity 10 \
  --sku-name Standard
```

### Step 2: Discover Microsoft Foundry Resources with Azure MCP Server

**2.1 Get Microsoft Foundry resource details** (uses `foundryextensions_resource_get`):
```
Show me details for Microsoft Foundry resources in my subscription
```

**Verify**:
- [ ] Tool invoked: `foundryextensions_resource_get`
- [ ] Your newly created Microsoft Foundry resource appears
- [ ] Resource properties shown (name, location, SKU)

**2.2 Alternative phrasing**:
```
List all Microsoft Foundry resources in resource group 'bugbash-foundry-rg'
```

### Step 3: List OpenAI Models with Azure MCP Server

**3.1 List OpenAI models and deployments** (uses `foundryextensions_openai_models-list`):
```
List all OpenAI models and deployments in my Azure AI resource '<resource-name>' in resource group 'bugbash-foundry-rg'
```

**Verify**:
- [ ] Tool invoked: `foundryextensions_openai_models-list`
- [ ] OpenAI models listed
- [ ] Deployment information shown

### Step 4: Optional Foundry Extension Operations

When the resource has suitable deployments or knowledge indexes, also verify these current tools:

- `foundryextensions_openai_chat-completions-create`
- `foundryextensions_openai_create-completion`
- `foundryextensions_openai_embeddings-create`
- `foundryextensions_knowledge_index_list`
- `foundryextensions_knowledge_index_schema`

Use `--learn` or the Azure MCP command reference to discover each tool's current required parameters before invoking it.

### Step 5: Cleanup (External - Not MCP)

**Option A: Prompt GitHub Copilot Chat**:
```
Delete the Azure resource group 'bugbash-foundry-rg' and all its resources
```

**Option B: Run Azure CLI Command Manually**:
```bash
# Delete resource group (removes all resources)
az group delete --name bugbash-foundry-rg --yes --no-wait
```

**Expected Results**:
- Microsoft Foundry resource discovery works
- OpenAI model and deployment listing is accurate
- Applicable completion, embedding, or knowledge-index operations return structured results


## Common Issues to Watch For

| Issue | Description | Resolution |
|-------|-------------|------------|
| **Authentication Failures** | Can't connect to Microsoft Foundry endpoint | Verify `az login` and endpoint URL is correct |
| **Token Limits** | Response truncated or incomplete | Model context window exceeded; use shorter prompts |
| **Rate Limiting** | API throttling errors | Reduce request frequency or upgrade SKU |
| **Endpoint Mismatch** | Wrong endpoint URL | Verify endpoint matches your Microsoft Foundry resource |
| **Model Not Deployed** | Deployment not found | Check model deployments are active and provisioned |

## What to Report

When logging issues, include:
- [ ] Exact prompt used
- [ ] Tool invoked (from MCP tool output)
- [ ] Expected vs actual results
- [ ] Error messages (if any)
- [ ] Resource endpoint URL (redact resource-specific details when needed)
- [ ] Model name and deployment name
- [ ] Screenshots of unexpected behavior

## Related Resources

- [Microsoft Foundry Documentation](https://learn.microsoft.com/azure/ai-foundry/)
- [Azure OpenAI Service](https://learn.microsoft.com/azure/ai-services/openai/)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [MCP Command Reference](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md)
- [E2E Test Prompts](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md)
- [Report Issues](https://github.com/microsoft/mcp/issues)

## 💡 Quick Reference: Supported MCP Tools

### Microsoft Foundry Resources
- `foundryextensions_resource_get` - Get Microsoft Foundry resource details

### OpenAI Integration
- `foundryextensions_openai_models-list` - List OpenAI models and deployments
- `foundryextensions_openai_chat-completions-create` - Create chat completions
- `foundryextensions_openai_create-completion` - Generate text completions
- `foundryextensions_openai_embeddings-create` - Generate embeddings

### Knowledge Management
- `foundryextensions_knowledge_index_list` - List knowledge indexes
- `foundryextensions_knowledge_index_schema` - Get index schema

---

**Next**: [Database Operations Testing](https://github.com/microsoft/mcp/tree/main/docs/bug-bash/scenarios/database-operations.md)