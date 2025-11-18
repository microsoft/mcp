# AI Agent Building Testing Scenario

> **MCP Tool Support Notice**
> Azure MCP Server provides **Microsoft Foundry resource inspection, model listing, and agent interaction** capabilities. Resource creation and deployment require Azure CLI or Portal. This scenario guides you through complete end-to-end workflows, clearly marking when to use MCP tools vs external tools.

## Objectives

- Test Microsoft Foundry resource discovery and inspection
- Test AI model listing and deployment management
- Test agent creation and interaction workflows
- Validate agent querying and evaluation capabilities

## Prerequisites

- [ ] Azure MCP Server installed and configured
- [ ] Azure CLI installed (`az --version`)
- [ ] Authenticated to Azure (`az login`)
- [ ] Active Azure subscription
- [ ] GitHub Copilot with Agent mode enabled

---

## Scenario 1: Microsoft Foundry Resource Discovery & Model Management

**Objective**: Complete workflow for discovering Microsoft Foundry resources and managing model deployments

### Step 1: Setup Resources (External - Not MCP)

> **External Setup Required**: Azure MCP Server cannot create resources. Use GitHub Copilot Chat to run Azure CLI commands or use Azure Portal.

**Option A: Prompt GitHub Copilot Chat** (Recommended):
```
Create an Azure resource group 'bugbash-aifoundry-rg' in eastus, then create an Azure AI Services account with SKU S0, and deploy GPT-4o model with deployment name 'gpt-4o-deployment'
```

**Option B: Run Azure CLI Commands Manually**:
```bash
# Create resource group
az group create --name bugbash-aifoundry-rg --location eastus

# Create Microsoft Foundry resource (AI Services account)
az cognitiveservices account create \
  --name bugbash-ai-foundry-$RANDOM \
  --resource-group bugbash-aifoundry-rg \
  --location eastus \
  --kind AIServices \
  --sku S0

# Deploy a model (GPT-4o)
az cognitiveservices account deployment create \
  --name <account-name-from-above> \
  --resource-group bugbash-aifoundry-rg \
  --deployment-name gpt-4o-deployment \
  --model-name gpt-4o \
  --model-version "2024-05-13" \
  --model-format OpenAI \
  --sku-capacity 10 \
  --sku-name Standard
```

### Step 2: Discover Microsoft Foundry Resources with Azure MCP Server

**2.1 Get Microsoft Foundry resource details** (uses `azmcp_foundry_resource_get`):
```
Show me details for Microsoft Foundry resources in my subscription
```

**Verify**:
- [ ] Tool invoked: `azmcp_foundry_resource_get`
- [ ] Your newly created Microsoft Foundry resource appears
- [ ] Resource properties shown (name, location, SKU)

**2.2 Alternative phrasing**:
```
List all Microsoft Foundry resources in resource group 'bugbash-aifoundry-rg'
```

### Step 3: List Available Models with Azure MCP Server

**3.1 List all available models** (uses `azmcp_foundry_models_list`):
```
List all available AI models in Microsoft Foundry
```

**Verify**:
- [ ] Tool invoked: `azmcp_foundry_models_list`
- [ ] Model catalog displayed
- [ ] GPT-4o and other models listed
- [ ] Model details shown (publisher, license, capabilities)

**3.2 Search for specific models**:
```
Show me all GPT models available in Microsoft Foundry
```

**Verify**:
- [ ] Filtered list returned
- [ ] GPT models displayed
- [ ] Search functionality works

**3.3 Check playground-compatible models**:
```
Which models can I use in the free playground?
```

**Verify**:
- [ ] Playground-compatible models listed
- [ ] Filter applied correctly

### Step 4: Inspect Model Deployments with Azure MCP Server

**4.1 List model deployments** (uses `azmcp_foundry_models_deployments_list`):
```
List all model deployments in my Microsoft Foundry resource
```

**Verify**:
- [ ] Tool invoked: `azmcp_foundry_models_deployments_list`
- [ ] Your GPT-4o deployment appears
- [ ] Deployment details shown (name, model, SKU, capacity)

**4.2 Alternative phrasing**:
```
Show me all deployed models in my Microsoft Foundry resource
```

### Step 5: List OpenAI Models with Azure MCP Server

**5.1 List OpenAI models** (uses `azmcp_foundry_openai_models-list`):
```
List all OpenAI models and deployments in my Azure AI resource '<resource-name>' in resource group 'bugbash-aifoundry-rg'
```

**Verify**:
- [ ] Tool invoked: `azmcp_foundry_openai_models-list`
- [ ] OpenAI models listed
- [ ] Deployment information shown

### Step 6: Cleanup (External - Not MCP)

**Option A: Prompt GitHub Copilot Chat**:
```
Delete the Azure resource group 'bugbash-aifoundry-rg' and all its resources
```

**Option B: Run Azure CLI Command Manually**:
```bash
# Delete resource group (removes all resources)
az group delete --name bugbash-aifoundry-rg --yes --no-wait
```

**Expected Results**:
- Microsoft Foundry resource discovery works
- Model catalog listing successful
- Deployment inspection accurate
- OpenAI model listing functional


## Common Issues to Watch For

| Issue | Description | Resolution |
|-------|-------------|------------|
| **Authentication Failures** | Can't connect to Microsoft Foundry endpoint | Verify `az login` and endpoint URL is correct |
| **Agent Not Found** | Agent ID doesn't exist | List agents first to get valid agent IDs |
| **Token Limits** | Response truncated or incomplete | Model context window exceeded; use shorter prompts |
| **Rate Limiting** | API throttling errors | Reduce request frequency or upgrade SKU |
| **Endpoint Mismatch** | Wrong endpoint URL | Verify endpoint matches your Microsoft Foundry resource |
| **Model Not Deployed** | Deployment not found | Check model deployments are active and provisioned |
| **Evaluation Failures** | Evaluator returns errors | Ensure Azure OpenAI deployment exists for evaluation |
| **Empty Agent List** | No agents returned | Create agents via Microsoft Foundry Portal first |

## What to Report

When logging issues, include:
- [ ] Exact prompt used
- [ ] Tool invoked (from MCP tool output)
- [ ] Expected vs actual results
- [ ] Error messages (if any)
- [ ] Agent ID and endpoint URL (redact sensitive info)
- [ ] Model name and deployment name
- [ ] Screenshots of unexpected behavior

## Related Resources

- [Microsoft Foundry Documentation](https://learn.microsoft.com/azure/ai-studio/)
- [Azure OpenAI Service](https://learn.microsoft.com/azure/ai-services/openai/)
- [Azure AI Agents](https://learn.microsoft.com/azure/ai-services/agents/)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [MCP Command Reference](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md)
- [E2E Test Prompts](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md)
- [Report Issues](https://github.com/microsoft/mcp/issues)

## 💡 Quick Reference: Supported MCP Tools

### Microsoft Foundry Resources
- `azmcp_foundry_resource_get` - Get Microsoft Foundry resource details
- `azmcp_foundry_models_list` - List available AI models
- `azmcp_foundry_models_deployments_list` - List model deployments

### OpenAI Integration
- `azmcp_foundry_openai_models-list` - List OpenAI models and deployments
- `azmcp_foundry_openai_chat-completions-create` - Create chat completions
- `azmcp_foundry_openai_create-completion` - Generate text completions
- `azmcp_foundry_openai_embeddings-create` - Generate embeddings

### AI Agents
- `azmcp_foundry_agents_list` - List AI agents
- `azmcp_foundry_agents_connect` - Query an agent
- `azmcp_foundry_agents_evaluate` - Evaluate agent response
- `azmcp_foundry_agents_query-and-evaluate` - Query and evaluate in one step

### Knowledge Management
- `azmcp_foundry_knowledge_index_list` - List knowledge indexes
- `azmcp_foundry_knowledge_index_schema` - Get index schema
- `azmcp_foundry_knowledge_source_get` - Get knowledge sources

### Model Deployment
- `azmcp_foundry_models_deploy` - Deploy AI model (write operation)

---

**Next**: [Database Operations Testing](https://github.com/microsoft/mcp/tree/main/docs/bug-bash/scenarios/database-operations.md)