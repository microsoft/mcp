# Agent Building Testing Scenario

Test Azure MCP Server's capabilities for building, deploying, and managing AI agents using Azure AI Foundry and related services.

## Objectives

- Create and configure Azure AI Foundry resources
- Build custom AI agents with Azure MCP integration
- Deploy agents to Azure
- Test agent interactions and workflows
- Validate agent monitoring and management
- Test multi-agent scenarios

## Prerequisites

- [ ] Azure MCP Server installed and configured
- [ ] Azure CLI installed (`az --version`)
- [ ] Authenticated to Azure (`az login`)
- [ ] Active Azure subscription with AI services enabled
- [ ] GitHub Copilot with Agent mode enabled
- [ ] Basic understanding of AI agent concepts

## Test Scenarios

### Scenario 1: Azure AI Foundry Setup

**Objective**: Set up Azure AI Foundry resources for agent development

#### Phase 1: Create AI Foundry Resources

1. **Create AI Hub**:
   ```
   Create an Azure AI Foundry hub:
   - Name: 'bugbash-ai-hub-<random>'
   - Resource group: '<your-rg>'
   - Location: East US
   - SKU: Standard
   ```

2. **Verify hub creation**:
   ```
   List all Azure AI Foundry hubs in my subscription
   ```

3. **Get hub details**:
   ```
   Show me the details of AI Foundry hub 'bugbash-ai-hub-<random>'
   ```

4. **Create AI project**:
   ```
   Create an Azure AI Foundry project:
   - Name: 'bugbash-ai-project'
   - Hub: 'bugbash-ai-hub-<random>'
   - Description: 'Bug bash agent testing project'
   ```

**Verify**:
- [ ] AI Hub created successfully
- [ ] Hub is in running state
- [ ] AI Project created
- [ ] Resources are properly linked

#### Phase 2: Configure AI Services

5. **Check available AI models**:
   ```
   What AI models are available in my Azure AI Foundry hub 
   'bugbash-ai-hub-<random>'?
   ```

6. **Deploy OpenAI model**:
   ```
   Deploy GPT-4o model to my AI Foundry project 'bugbash-ai-project'
   ```

7. **List deployments**:
   ```
   List all model deployments in AI Foundry project 'bugbash-ai-project'
   ```

8. **Get endpoint information**:
   ```
   Get the endpoint URL and keys for my deployed model
   ```

**Verify**:
- [ ] Available models listed
- [ ] Model deployment successful
- [ ] Endpoint accessible
- [ ] Authentication configured

**Expected Results**:
- AI Foundry hub created
- AI project configured
- AI model deployed
- Endpoints are accessible

### Scenario 2: Deploy and Test Agent

**Objective**: Deploy the agent and test its functionality

#### Phase 1: Deploy Agent Code

1. **Get deployment guidance**:
   ```
   How do I deploy my Python agent code to Function App 
   'bugbash-agent-func-<random>'?
   ```

2. **Generate sample agent code**:
   ```
   Generate sample Python code for an Azure management agent that:
   - Uses Azure OpenAI for natural language understanding
   - Uses Azure MCP Server to manage Azure resources
   - Stores conversation history in Cosmos DB
   ```

3. **Deploy agent**:
   ```
   Deploy my agent code to Function App 'bugbash-agent-func-<random>'
   ```

**Verify**:
- [ ] Deployment instructions clear
- [ ] Sample code provided
- [ ] Deployment successful
- [ ] Function App running

#### Phase 2: Test Agent Interactions

4. **Test basic agent query**:
   ```
   Send a test request to my agent: "List all my storage accounts"
   ```

5. **Test resource creation**:
   ```
   Ask my agent to: "Create a new storage account named 
   'agenttest<random>' in East US"
   ```

6. **Test monitoring capability**:
   ```
   Ask my agent: "Show me the health status of all my resources in 
   resource group '<your-rg>'"
   ```

7. **Test complex query**:
   ```
   Ask my agent: "Which of my resources are costing the most this month?"
   ```

**Verify**:
- [ ] Agent responds correctly
- [ ] Resource operations work
- [ ] Monitoring data retrieved
- [ ] Complex queries handled

#### Phase 3: Monitor Agent Performance

8. **Check agent logs**:
   ```
   Show me the logs for Function App 'bugbash-agent-func-<random>' 
   from the last hour
   ```

9. **Monitor agent metrics**:
   ```
   Show me performance metrics for my agent Function App
   ```

10. **Check Cosmos DB usage**:
    ```
    Show me the Cosmos DB usage statistics for my agent's state database
    ```

**Verify**:
- [ ] Logs accessible
- [ ] Metrics available
- [ ] Performance acceptable
- [ ] State persistence working

**Expected Results**:
- Agent deployed successfully
- Agent responds to queries
- Resource operations work
- Monitoring and logging functional

## Common Issues to Watch For

- **Authentication Failures**: Managed identity misconfiguration
- **Token Limits**: Exceeding model context windows
- **Rate Limiting**: API throttling from AI services
- **State Persistence**: Session state not saved correctly
- **Cold Starts**: Function App cold start latency
- **Network Timeouts**: Long-running operations timing out
- **Cost Overruns**: Unexpected charges from AI API calls
- **Model Hallucinations**: Incorrect or inconsistent agent responses
- **Integration Issues**: MCP Server connection problems
- **Event Ordering**: Messages processed out of sequence

## Related Resources

- [Azure AI Foundry Documentation](https://learn.microsoft.com/azure/ai-studio/)
- [Azure OpenAI Service](https://learn.microsoft.com/azure/ai-services/openai/)
- [Azure Functions for AI Agents](https://learn.microsoft.com/azure/azure-functions/)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [Azure MCP Server GitHub](https://github.com/microsoft/mcp)
- [Agent Test Prompts](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md)
- [Report Issues](https://github.com/microsoft/mcp/issues)

**Next**: [Database Operations Testing](database-operations.md)