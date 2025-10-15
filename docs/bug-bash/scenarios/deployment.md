# 🚀 Deployment Scenarios Testing

> **⚠️ MCP Tool Support Notice**
> Azure MCP Server provides **deployment planning guidance, CI/CD pipeline recommendations, and application log retrieval** capabilities. Actual resource creation and application deployment require Azure CLI, Portal, or CI/CD tools. This scenario guides you through complete end-to-end workflows, clearly marking when to use MCP tools vs external tools.

## 🎯 Objectives

- Test deployment planning and guidance from Azure MCP Server
- Verify CI/CD pipeline recommendation accuracy
- Test application log retrieval and analysis
- Validate deployment plan generation for various project types

## ✅ Prerequisites

- [ ] Azure MCP Server installed and configured
- [ ] Azure CLI installed (`az --version`)
- [ ] Azure Developer CLI installed (`azd version`)
- [ ] Authenticated to Azure (`az login`)
- [ ] Active Azure subscription
- [ ] GitHub Copilot with Agent mode enabled

---

## 📋 Scenario 1: Application Deployment Planning End-to-End

**Objective**: Complete workflow for getting deployment guidance and executing deployment

### Step 1: Setup Sample Application (Local - Not MCP)

> **External Setup Required**: Create a sample Node.js application locally

```bash
# Create a sample Node.js application
mkdir bugbash-deploy-app && cd bugbash-deploy-app

# Initialize Node.js project
npm init -y

# Install Express
npm install express

# Create simple server (index.js)
cat > index.js << 'EOF'
const express = require('express');
const app = express();
const port = process.env.PORT || 3000;

app.get('/', (req, res) => {
  res.json({ message: 'Hello from Azure!', timestamp: new Date() });
});

app.listen(port, () => {
  console.log(`Server running on port ${port}`);
});
EOF

# Create package.json start script
npm pkg set scripts.start="node index.js"
```

### Step 2: Get Deployment Plan with Azure MCP Server

**2.1 Request deployment plan** (uses `azmcp_deploy_plan_get`):
```
I have a Node.js Express application in folder 'bugbash-deploy-app'. Create a deployment plan to deploy this to Azure App Service.
```

**Verify**:
- [ ] Tool invoked: `azmcp_deploy_plan_get`
- [ ] Deployment plan includes App Service target
- [ ] Runtime configuration identified (Node.js)
- [ ] Provisioning tool recommendation provided (Bicep, Terraform, or azd)
- [ ] Project structure analysis included

**2.2 Alternative phrasing**:
```
Show me how to deploy my Node.js app in 'bugbash-deploy-app' to Azure
```

### Step 3: Get CI/CD Pipeline Guidance with Azure MCP Server

**3.1 Get pipeline guidance** (uses `azmcp_deploy_pipeline_guidance_get`):
```
How do I set up a CI/CD pipeline to automatically deploy my application to Azure App Service?
```

**Verify**:
- [ ] Tool invoked: `azmcp_deploy_pipeline_guidance_get`
- [ ] GitHub Actions workflow guidance provided
- [ ] Azure DevOps pipeline guidance included
- [ ] Deployment credentials setup explained
- [ ] Best practices mentioned

**3.2 Request specific pipeline configuration**:
```
Generate GitHub Actions workflow guidance for deploying my Node.js app to Azure App Service with staging and production environments
```

**Verify**:
- [ ] Multi-environment deployment strategy explained
- [ ] Approval workflows mentioned
- [ ] Environment-specific configuration guidance provided

### Step 4: Deploy Application (Azure CLI/azd - Not MCP)

> **External Deployment Required**: Use Azure CLI or azd to deploy

```bash
# Create resource group
az group create --name bugbash-deploy-rg --location eastus

# Create App Service plan
az appservice plan create \
  --name bugbash-plan \
  --resource-group bugbash-deploy-rg \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --name bugbash-app-$RANDOM \
  --resource-group bugbash-deploy-rg \
  --plan bugbash-plan \
  --runtime "NODE:18-lts"

# Deploy application
cd bugbash-deploy-app
zip -r app.zip .
az webapp deployment source config-zip \
  --resource-group bugbash-deploy-rg \
  --name <webapp-name-from-above> \
  --src app.zip
```

### Step 5: View Application Logs with Azure MCP Server

**5.1 Get application logs** (uses `azmcp_deploy_app_logs_get`):
```
Show me the logs for my application in Azure environment 'bugbash-deploy-rg'
```

**Verify**:
- [ ] Tool invoked: `azmcp_deploy_app_logs_get`
- [ ] Application logs retrieved
- [ ] Log timestamps shown
- [ ] Log content is readable

**5.2 Request recent logs**:
```
Show me the last 50 log entries for my deployed application
```

**Verify**:
- [ ] Limit parameter works
- [ ] Most recent logs displayed first
- [ ] Startup messages visible

### Step 6: Get Architecture Diagram with Azure MCP Server

**6.1 Generate architecture diagram** (uses `azmcp_deploy_architecture_diagram_generate`):
```
Generate a Mermaid architecture diagram for my deployment showing the App Service, application topology, and resource dependencies
```

**Verify**:
- [ ] Tool invoked: `azmcp_deploy_architecture_diagram_generate`
- [ ] Mermaid diagram generated
- [ ] Components clearly shown
- [ ] Relationships between resources displayed

### Step 7: Cleanup (External - Not MCP)

```bash
# Delete resource group (removes all resources)
az group delete --name bugbash-deploy-rg --yes --no-wait
```

**Expected Results**:
- ✅ Deployment plan guidance accurate and actionable
- ✅ CI/CD pipeline recommendations comprehensive
- ✅ Application logs successfully retrieved
- ✅ Architecture diagram generated correctly

---

## 📋 Scenario 2: Infrastructure-as-Code Deployment Guidance End-to-End

**Objective**: Complete workflow for getting IaC recommendations and deployment guidance

### Step 1: Setup Project Structure (Local - Not MCP)

> **External Setup Required**: Create a sample project with multiple services

```bash
# Create multi-service project
mkdir bugbash-fullstack && cd bugbash-fullstack
mkdir frontend backend

# Frontend (React app - simplified)
cat > frontend/package.json << 'EOF'
{
  "name": "frontend",
  "version": "1.0.0",
  "scripts": {
    "build": "echo 'Building React app'"
  }
}
EOF

# Backend (Node.js API)
cat > backend/package.json << 'EOF'
{
  "name": "backend",
  "version": "1.0.0",
  "main": "server.js",
  "scripts": {
    "start": "node server.js"
  },
  "dependencies": {
    "express": "^4.18.0"
  }
}
EOF

cat > backend/server.js << 'EOF'
const express = require('express');
const app = express();
app.get('/api/health', (req, res) => res.json({ status: 'healthy' }));
app.listen(process.env.PORT || 8080);
EOF
```

### Step 2: Get IaC Generation Rules with Azure MCP Server

**2.1 Request IaC rules for resources** (uses `azmcp_deploy_iac_rules_get`):
```
Show me the infrastructure-as-code generation rules for deploying Azure App Service and Azure SQL Database using Bicep
```

**Verify**:
- [ ] Tool invoked: `azmcp_deploy_iac_rules_get`
- [ ] Bicep rules provided for App Service
- [ ] Bicep rules provided for Azure SQL Database
- [ ] Best practices included
- [ ] Parameter recommendations shown

**2.2 Get rules for different deployment tools**:
```
Show me the IaC rules for Azure Storage Account and Cosmos DB using Terraform
```

**Verify**:
- [ ] Terraform-specific rules returned
- [ ] Resource type rules accurate
- [ ] Configuration recommendations included

### Step 3: Get Deployment Plan for Multi-Service App with Azure MCP Server

**3.1 Request deployment plan** (uses `azmcp_deploy_plan_get`):
```
I have a full-stack application with a React frontend in 'frontend' folder and Node.js API in 'backend' folder. Create a deployment plan for Azure with Static Web App for frontend and App Service for backend.
```

**Verify**:
- [ ] Tool invoked: `azmcp_deploy_plan_get`
- [ ] Frontend deployment to Static Web App recommended
- [ ] Backend deployment to App Service recommended
- [ ] Service communication strategy included
- [ ] Infrastructure dependencies identified

### Step 4: Get Pipeline Guidance for Full-Stack App with Azure MCP Server

**4.1 Request CI/CD guidance** (uses `azmcp_deploy_pipeline_guidance_get`):
```
How do I set up a CI/CD pipeline for my full-stack application with separate build and deployment for frontend and backend?
```

**Verify**:
- [ ] Tool invoked: `azmcp_deploy_pipeline_guidance_get`
- [ ] Multi-stage pipeline guidance provided
- [ ] Frontend build process explained
- [ ] Backend build process explained
- [ ] Deployment orchestration guidance included

**4.2 Request GitHub-specific guidance**:
```
Generate GitHub Actions pipeline guidance for my full-stack app with automated testing and deployment to Azure
```

**Verify**:
- [ ] GitHub Actions workflow structure explained
- [ ] Testing stage recommendations included
- [ ] Deployment stages for both services covered
- [ ] Environment variables and secrets guidance provided

### Step 5: Deploy Infrastructure (Azure CLI/Bicep - Not MCP)

> **External Deployment Required**: Use Azure CLI with Bicep

```bash
# Create resource group
az group create --name bugbash-fullstack-rg --location eastus

# Create Static Web App for frontend
az staticwebapp create \
  --name bugbash-frontend-$RANDOM \
  --resource-group bugbash-fullstack-rg \
  --location eastus

# Create App Service for backend
az appservice plan create \
  --name bugbash-backend-plan \
  --resource-group bugbash-fullstack-rg \
  --sku B1 \
  --is-linux

az webapp create \
  --name bugbash-backend-$RANDOM \
  --resource-group bugbash-fullstack-rg \
  --plan bugbash-backend-plan \
  --runtime "NODE:18-lts"
```

### Step 6: Verify Deployment Guidance with Azure MCP Server

**6.1 Get architecture diagram** (uses `azmcp_deploy_architecture_diagram_generate`):
```
Generate a Mermaid architecture diagram showing my full-stack application with Static Web App frontend, App Service backend, and their connectivity
```

**Verify**:
- [ ] Tool invoked: `azmcp_deploy_architecture_diagram_generate`
- [ ] Both services shown in diagram
- [ ] Communication flow displayed
- [ ] Azure resources clearly labeled

### Step 7: Cleanup (External - Not MCP)

```bash
# Delete resource group (removes all resources)
az group delete --name bugbash-fullstack-rg --yes --no-wait
```

**Expected Results**:
- ✅ IaC rules provided for requested resource types
- ✅ Deployment plan includes multi-service architecture
- ✅ CI/CD guidance covers full-stack complexity
- ✅ Architecture diagrams accurately represent topology

---

## � Common Issues to Watch For

| Issue | Description | Resolution |
|-------|-------------|------------|
| **Invalid Workspace Path** | Workspace folder not found | Verify the workspace folder path exists and is accessible |
| **Project Not Recognized** | MCP can't identify project type | Ensure package.json or project files exist in the folder |
| **Missing Dependencies** | Required project files missing | Check project structure has all necessary configuration files |
| **IaC Tool Mismatch** | Wrong provisioning tool recommended | Specify preferred tool (Bicep/Terraform) in the prompt |
| **Log Retrieval Fails** | Can't access application logs | Verify azd environment exists and is properly configured |
| **Architecture Diagram Errors** | Diagram generation fails | Ensure application topology is properly defined |
| **Pipeline Guidance Generic** | Recommendations too generic | Provide more context about your project structure and requirements |

## 📝 What to Report

When logging issues, include:
- [ ] Exact prompt used
- [ ] Tool invoked (from MCP tool output)
- [ ] Expected vs actual results
- [ ] Error messages (if any)
- [ ] Project type and structure
- [ ] Workspace folder path
- [ ] Screenshots of unexpected behavior

## 📚 Related Resources

- [Azure Deployment Center](https://learn.microsoft.com/azure/app-service/deploy-continuous-deployment)
- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/)
- [GitHub Actions for Azure](https://learn.microsoft.com/azure/developer/github/github-actions)
- [Azure DevOps Pipelines](https://learn.microsoft.com/azure/devops/pipelines/)
- [MCP Command Reference](../../servers/Azure.Mcp.Server/docs/azmcp-commands.md)
- [E2E Test Prompts](../../servers/Azure.Mcp.Server/docs/e2eTestPrompts.md)
- [Report Issues](https://github.com/microsoft/mcp/issues)

## 💡 Quick Reference: Supported MCP Tools

### Deployment Planning
- `azmcp_deploy_plan_get` - Get deployment plan for a project
- `azmcp_deploy_iac_rules_get` - Get IaC generation rules for resource types
- `azmcp_deploy_architecture_diagram_generate` - Generate Mermaid architecture diagram

### CI/CD Guidance
- `azmcp_deploy_pipeline_guidance_get` - Get CI/CD pipeline setup guidance

### Application Logs
- `azmcp_deploy_app_logs_get` - Get application service logs for azd environment

---

**Next**: [Full Stack Applications Testing](full-stack-apps.md)
