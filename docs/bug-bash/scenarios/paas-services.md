# 🚀 PaaS Services Testing Scenario

> **⚠️ READ FIRST**: [TESTING-SCOPE.md](../TESTING-SCOPE.md) explains what MCP tools can and cannot do. **Azure MCP Server has LIMITED support for PaaS services** - primarily Function App inspection and App Service database connections. Use Azure CLI to create App Services, Container Apps, and Function Apps before testing.

Test Azure MCP Server's capabilities with Platform-as-a-Service offerings including App Service, Container Apps, and Azure Functions.

## Objectives

- Deploy and manage Azure App Service web apps
- Work with Azure Container Apps
- Create and manage Azure Functions
- Test configuration and scaling
- Verify integration with other services
- Test deployment workflows

## Prerequisites

- [ ] Azure MCP Server installed and configured
- [ ] Azure CLI installed (`az --version`)
- [ ] Authenticated to Azure (`az login`)
- [ ] Active Azure subscription
- [ ] Sample application code (optional)
- [ ] GitHub Copilot with Agent mode enabled

## Test Scenarios

### Scenario 1: Azure App Service

**Objective**: Test web app creation, configuration, and deployment

#### Phase 1: Create App Service Resources

1. **Create App Service Plan**:
   ```
   Create an Azure App Service Plan:
   - Name: 'bugbash-plan-<random>'
   - Resource group: '<your-rg>'
   - SKU: B1 (Basic)
   - Region: East US
   - OS: Linux
   ```

2. **Verify plan creation**:
   ```
   List all App Service plans in resource group '<your-rg>'
   ```

3. **Create web app**:
   ```
   Create a web app:
   - Name: 'bugbash-webapp-<random>'
   - Resource group: '<your-rg>'
   - App Service Plan: 'bugbash-plan-<random>'
   - Runtime: Node.js 20 LTS
   ```

4. **List web apps**:
   ```
   List all web apps in my subscription
   ```

5. **Get web app details**:
   ```
   Show me the details for web app 'bugbash-webapp-<random>' in 
   resource group '<your-rg>'
   ```

**Verify**:
- [ ] App Service Plan created with correct SKU
- [ ] Web app created successfully
- [ ] Runtime configured correctly
- [ ] Web app URL is accessible

#### Phase 2: Configure Web App

6. **Get configuration**:
   ```
   Get the configuration for web app 'bugbash-webapp-<random>'
   ```

7. **Add application settings**:
   ```
   Add the following application settings to web app 'bugbash-webapp-<random>':
   - NODE_ENV: production
   - API_URL: https://api.example.com
   - LOG_LEVEL: info
   ```

8. **Add connection strings**:
   ```
   Add a connection string named 'Database' with value 
   '<connection-string>' of type SQLAzure to web app 'bugbash-webapp-<random>'
   ```

9. **Enable Application Insights**:
   ```
   Enable Application Insights for web app 'bugbash-webapp-<random>'
   ```

**Verify**:
- [ ] Application settings added
- [ ] Connection strings configured
- [ ] Application Insights enabled
- [ ] Configuration changes applied

#### Phase 3: Database Integration

10. **Add database connection**:
    ```
    Add a SQL Server database connection to app service 'bugbash-webapp-<random>':
    - Database name: '<database>'
    - Server: '<server>'
    ```

11. **Configure PostgreSQL**:
    ```
    Add a PostgreSQL database to app service 'bugbash-webapp-<random>'
    ```

12. **Verify database connections**:
    ```
    Show me all database connections for web app 'bugbash-webapp-<random>'
    ```

**Verify**:
- [ ] Database connections configured
- [ ] Connection strings created
- [ ] Databases accessible from app

#### Phase 4: Deployment and Testing

13. **Get deployment information**:
    ```
    How do I deploy my Node.js application to web app 'bugbash-webapp-<random>'?
    ```

14. **Check web app status**:
    ```
    What is the status of web app 'bugbash-webapp-<random>'?
    ```

15. **Get web app URL**:
    ```
    What is the URL for web app 'bugbash-webapp-<random>'?
    ```

16. **Test accessibility**:
    - Open the web app URL in browser
    - Verify the app is running
    - Check if default page loads

**Verify**:
- [ ] Deployment instructions provided
- [ ] Web app is running
- [ ] URL is accessible
- [ ] App responds to requests

**Expected Results**:
- App Service Plan created
- Web app created and configured
- Database connections working
- Application is deployable
- Web app is accessible

---

### Scenario 2: Azure Container Apps

**Objective**: Test container deployment and management

#### Phase 1: Create Container Apps Environment

1. **Create Container Apps Environment**:
   ```
   Create an Azure Container Apps Environment:
   - Name: 'bugbash-containerenv-<random>'
   - Resource group: '<your-rg>'
   - Location: East US
   ```

2. **List Container Apps Environments**:
   ```
   List all Container Apps environments in my subscription
   ```

3. **Get environment details**:
   ```
   Show me details of Container Apps environment 'bugbash-containerenv-<random>'
   ```

**Verify**:
- [ ] Environment created successfully
- [ ] Environment is in running state
- [ ] Configuration is correct

#### Phase 2: Deploy Container App

4. **Create container app**:
   ```
   Create a container app in environment 'bugbash-containerenv-<random>':
   - Name: 'bugbash-container-<random>'
   - Container image: nginx:latest
   - Ingress: enabled, external
   - Target port: 80
   ```

5. **List container apps**:
   ```
   List all container apps in resource group '<your-rg>'
   ```

6. **Get container app details**:
   ```
   Show me details of container app 'bugbash-container-<random>'
   ```

**Verify**:
- [ ] Container app created
- [ ] Container is running
- [ ] Ingress configured correctly
- [ ] App is accessible externally

#### Phase 3: Configuration and Scaling

7. **Configure environment variables**:
   ```
   Add environment variables to container app 'bugbash-container-<random>':
   - ENVIRONMENT: production
   - DEBUG: false
   ```

8. **Configure scaling**:
   ```
   Configure scaling for container app 'bugbash-container-<random>':
   - Min replicas: 1
   - Max replicas: 5
   ```

9. **Get container app URL**:
   ```
   What is the URL for container app 'bugbash-container-<random>'?
   ```

**Verify**:
- [ ] Environment variables set
- [ ] Scaling configured
- [ ] URL provided and accessible

**Expected Results**:
- Container Apps environment created
- Container app deployed
- Configuration applied
- Scaling works
- App is accessible

---

### Scenario 3: Azure Functions

**Objective**: Test Function App creation and management

#### Phase 1: Create Function App

1. **Create storage account for functions**:
   ```
   Create a storage account 'bugbashfunc<random>' for Azure Functions 
   in resource group '<your-rg>'
   ```

2. **Create Function App**:
   ```
   Create an Azure Function App:
   - Name: 'bugbash-funcapp-<random>'
   - Resource group: '<your-rg>'
   - Runtime: Node.js 20
   - Plan: Consumption (serverless)
   - Storage account: 'bugbashfunc<random>'
   - Region: East US
   ```

3. **List Function Apps**:
   ```
   List all function apps in my subscription
   ```

4. **Get Function App details**:
   ```
   Show me details for function app 'bugbash-funcapp-<random>' in 
   resource group '<your-rg>'
   ```

**Verify**:
- [ ] Storage account created
- [ ] Function App created
- [ ] Runtime configured correctly
- [ ] Consumption plan assigned

#### Phase 2: Configure Function App

5. **Get Function App configuration**:
   ```
   Get configuration for function app 'bugbash-funcapp-<random>'
   ```

6. **Add application settings**:
   ```
   Add application settings to function app 'bugbash-funcapp-<random>':
   - ENVIRONMENT: production
   - API_KEY: <api-key>
   ```

7. **Enable Application Insights**:
   ```
   Enable Application Insights for function app 'bugbash-funcapp-<random>'
   ```

8. **Get function app keys**:
   ```
   Get the host keys for function app 'bugbash-funcapp-<random>'
   ```

**Verify**:
- [ ] Configuration retrieved
- [ ] Application settings added
- [ ] Application Insights enabled
- [ ] Host keys accessible

#### Phase 3: Deployment Information

9. **Get deployment instructions**:
   ```
   How do I deploy my Node.js functions to function app 
   'bugbash-funcapp-<random>'?
   ```

10. **Get function app URL**:
    ```
    What is the URL for function app 'bugbash-funcapp-<random>'?
    ```

11. **Check function app status**:
    ```
    What is the status of function app 'bugbash-funcapp-<random>'?
    ```

**Verify**:
- [ ] Deployment guidance provided
- [ ] URL retrieved
- [ ] Status is running

**Expected Results**:
- Function App created
- Configuration applied
- Application Insights enabled
- Deployment guidance provided
- App is operational


## Common Issues to Watch For

- **Naming Conflicts**: App names must be globally unique
- **Plan Compatibility**: Some features require specific plan tiers
- **Runtime Mismatches**: Ensure runtime version compatibility
- **Storage Dependencies**: Function Apps require storage accounts
- **Port Configuration**: Container Apps need correct port mappings
- **Memory Limits**: Consumption plan has memory constraints
- **Cold Starts**: Serverless functions have startup latency
- **CORS Issues**: Cross-origin requests may need configuration

## Related Resources

- [Azure App Service Documentation](https://learn.microsoft.com/azure/app-service/)
- [Azure Container Apps Documentation](https://learn.microsoft.com/azure/container-apps/)
- [Azure Functions Documentation](https://learn.microsoft.com/azure/azure-functions/)
- [PaaS Test Prompts](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md#azure-app-service)
- [Report Issues](https://github.com/microsoft/mcp/issues)

---

**Next**: [Storage Operations Testing](storage-operations.md)
