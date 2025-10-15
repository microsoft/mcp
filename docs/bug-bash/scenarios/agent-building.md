# 🤖 AI Agent Building Testing Scenario

> **MCP Tool Support Notice**
> Azure MCP Server provides **AI Foundry resource inspection, model listing, and agent interaction** capabilities. Resource creation and deployment require Azure CLI or Portal. This scenario guides you through complete end-to-end workflows, clearly marking when to use MCP tools vs external tools.

## Objectives

- Test Azure AI Foundry resource discovery and inspection
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

## Scenario 1: Azure AI Foundry Resource Discovery & Model Management

**Objective**: Complete workflow for discovering AI Foundry resources and managing model deployments

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

# Create Azure AI Foundry resource (AI Services account)
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

### Step 2: Discover AI Foundry Resources with Azure MCP Server

**2.1 Get AI Foundry resource details** (uses `azmcp_foundry_resource_get`):
```
Show me details for Azure AI Foundry resources in my subscription
```

**Verify**:
- [ ] Tool invoked: `azmcp_foundry_resource_get`
- [ ] Your newly created AI Foundry resource appears
- [ ] Resource properties shown (name, location, SKU)

**2.2 Alternative phrasing**:
```
List all Azure AI Foundry resources in resource group 'bugbash-aifoundry-rg'
```

### Step 3: List Available Models with Azure MCP Server

**3.1 List all available models** (uses `azmcp_foundry_models_list`):
```
List all available AI models in Azure AI Foundry
```

**Verify**:
- [ ] Tool invoked: `azmcp_foundry_models_list`
- [ ] Model catalog displayed
- [ ] GPT-4o and other models listed
- [ ] Model details shown (publisher, license, capabilities)

**3.2 Search for specific models**:
```
Show me all GPT models available in Azure AI Foundry
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
List all model deployments in my Azure AI Foundry resource
```

**Verify**:
- [ ] Tool invoked: `azmcp_foundry_models_deployments_list`
- [ ] Your GPT-4o deployment appears
- [ ] Deployment details shown (name, model, SKU, capacity)

**4.2 Alternative phrasing**:
```
Show me all deployed models in my AI Foundry resource
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
- AI Foundry resource discovery works
- Model catalog listing successful
- Deployment inspection accurate
- OpenAI model listing functional

---

## Scenario 2: AI Agent Creation & Interaction End-to-End

**Objective**: Complete workflow for creating, querying, and evaluating AI agents

### Step 1: Setup Agent Resources (External - Not MCP)

> **External Setup Required**: Azure MCP Server cannot create resources. Use GitHub Copilot Chat to run Azure CLI commands or use Azure Portal.

**Option A: Prompt GitHub Copilot Chat** (Recommended):
```
Create an Azure resource group 'bugbash-agents-rg' in eastus, create an Azure AI Services account with SKU S0, deploy GPT-4o model with deployment name 'agent-gpt4o', then show me the endpoint URL
```

**Option B: Run Azure CLI Commands Manually**:
```bash
# Create resource group
az group create --name bugbash-agents-rg --location eastus

# Create Azure AI Foundry resource
az cognitiveservices account create \
  --name bugbash-agents-$RANDOM \
  --resource-group bugbash-agents-rg \
  --location eastus \
  --kind AIServices \
  --sku S0

# Get endpoint URL (you'll need this)
az cognitiveservices account show \
  --name <account-name-from-above> \
  --resource-group bugbash-agents-rg \
  --query "properties.endpoint" -o tsv

# Deploy GPT-4o for agent use
az cognitiveservices account deployment create \
  --name <account-name-from-above> \
  --resource-group bugbash-agents-rg \
  --deployment-name agent-gpt4o \
  --model-name gpt-4o \
  --model-version "2024-05-13" \
  --model-format OpenAI \
  --sku-capacity 10 \
  --sku-name Standard
```

**Option C: Create Agent via Azure AI Foundry Portal**:
1. Go to [Azure AI Foundry Portal](https://ai.azure.com)
2. Create a new project
3. Create an agent with your deployed model
4. Note the agent ID for testing

### Step 2: List Available Agents with Azure MCP Server

**2.1 List all agents** (uses `azmcp_foundry_agents_list`):
```
List all AI agents in my Azure AI Foundry project with endpoint '<endpoint-url>'
```

**Verify**:
- [ ] Tool invoked: `azmcp_foundry_agents_list`
- [ ] Your created agent appears
- [ ] Agent properties shown (ID, name, model)

**2.2 Alternative phrasing**:
```
Show me all agents in my AI Foundry project
```

### Step 3: Query an Agent with Azure MCP Server

**3.1 Connect and query agent** (uses `azmcp_foundry_agents_connect`):
```
Query agent '<agent-id>' with: "What Azure services can you help me manage?" using endpoint '<endpoint-url>'
```

**Verify**:
- [ ] Tool invoked: `azmcp_foundry_agents_connect`
- [ ] Agent response received
- [ ] Response is coherent and relevant

**3.2 Test agent capabilities**:
```
Ask agent '<agent-id>' to: "List the most common Azure resources used in web applications" using endpoint '<endpoint-url>'
```

**Verify**:
- [ ] Agent understands the query
- [ ] Response includes relevant Azure services
- [ ] Answer is accurate

**3.3 Test agent with specific query**:
```
Query agent '<agent-id>': "Explain the difference between Azure SQL Database and Cosmos DB" using endpoint '<endpoint-url>'
```

**Verify**:
- [ ] Detailed explanation provided
- [ ] Technical accuracy maintained
- [ ] Response format is clear

### Step 4: Evaluate Agent Responses with Azure MCP Server

**4.1 Evaluate a response** (uses `azmcp_foundry_agents_evaluate`):
```
Evaluate the response from agent '<agent-id>' for query "What is Azure Storage?" with response "Azure Storage is a cloud storage solution" using evaluator 'groundedness' with Azure OpenAI endpoint '<openai-endpoint>' and deployment '<deployment-name>'
```

**Verify**:
- [ ] Tool invoked: `azmcp_foundry_agents_evaluate`
- [ ] Evaluation score returned
- [ ] Groundedness metrics shown

**4.2 Query and evaluate in one step** (uses `azmcp_foundry_agents_query-and-evaluate`):
```
Query and evaluate agent '<agent-id>' with: "What are the main features of Azure Functions?" using endpoint '<endpoint-url>' and evaluate with Azure OpenAI endpoint '<openai-endpoint>' and deployment '<deployment-name>'
```

**Verify**:
- [ ] Tool invoked: `azmcp_foundry_agents_query-and-evaluate`
- [ ] Agent response received
- [ ] Evaluation metrics provided
- [ ] Combined workflow successful

### Step 5: Explore Knowledge Indexes with Azure MCP Server

**5.1 List knowledge indexes** (uses `azmcp_foundry_knowledge_index_list`):
```
List all knowledge indexes in my AI Foundry project with endpoint '<endpoint-url>'
```

**Verify**:
- [ ] Tool invoked: `azmcp_foundry_knowledge_index_list`
- [ ] Knowledge indexes listed (if any exist)
- [ ] Index properties shown

**5.2 Get index schema** (uses `azmcp_foundry_knowledge_index_schema`):
```
Show me the schema for knowledge index '<index-name>' using endpoint '<endpoint-url>'
```

**Verify**:
- [ ] Tool invoked: `azmcp_foundry_knowledge_index_schema`
- [ ] Schema information returned
- [ ] Field types and structure shown

### Step 6: Cleanup (External - Not MCP)

**Option A: Prompt GitHub Copilot Chat**:
```
Delete the Azure resource group 'bugbash-agents-rg' and all its resources
```

**Option B: Run Azure CLI Command Manually**:
```bash
# Delete resource group (removes all resources)
az group delete --name bugbash-agents-rg --yes --no-wait
```

**Expected Results**:
- Agent listing works correctly
- Agent querying returns responses
- Response evaluation provides metrics
- Combined query-and-evaluate workflow functional
- Knowledge index inspection successful

---

## Common Issues to Watch For

| Issue | Description | Resolution |
|-------|-------------|------------|
| **Authentication Failures** | Can't connect to AI Foundry endpoint | Verify `az login` and endpoint URL is correct |
| **Agent Not Found** | Agent ID doesn't exist | List agents first to get valid agent IDs |
| **Token Limits** | Response truncated or incomplete | Model context window exceeded; use shorter prompts |
| **Rate Limiting** | API throttling errors | Reduce request frequency or upgrade SKU |
| **Endpoint Mismatch** | Wrong endpoint URL | Verify endpoint matches your AI Foundry resource |
| **Model Not Deployed** | Deployment not found | Check model deployments are active and provisioned |
| **Evaluation Failures** | Evaluator returns errors | Ensure Azure OpenAI deployment exists for evaluation |
| **Empty Agent List** | No agents returned | Create agents via AI Foundry Portal first |

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

- [Azure AI Foundry Documentation](https://learn.microsoft.com/azure/ai-studio/)
- [Azure OpenAI Service](https://learn.microsoft.com/azure/ai-services/openai/)
- [Azure AI Agents](https://learn.microsoft.com/azure/ai-services/agents/)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [MCP Command Reference](../../servers/Azure.Mcp.Server/docs/azmcp-commands.md)
- [E2E Test Prompts](../../servers/Azure.Mcp.Server/docs/e2eTestPrompts.md)
- [Report Issues](https://github.com/microsoft/mcp/issues)

## 💡 Quick Reference: Supported MCP Tools

### AI Foundry Resources
- `azmcp_foundry_resource_get` - Get AI Foundry resource details
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

**Next**: [Database Operations Testing](database-operations.md)