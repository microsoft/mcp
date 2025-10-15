# Deployment Scenarios Testing

Test Azure MCP Server's deployment capabilities, CI/CD integration, and deployment management features.

## Objectives

- Test application deployment workflows
- Verify deployment guidance and automation
- Test multi-environment deployments
- Validate deployment monitoring
- Test rollback and recovery procedures
- Verify CI/CD integration guidance

## Prerequisites

- [ ] Azure MCP Server installed and configured
- [ ] Azure CLI installed (`az --version`)
- [ ] Authenticated to Azure (`az login`)
- [ ] Active Azure subscription
- [ ] Sample application ready for deployment
- [ ] GitHub Copilot with Agent mode enabled

## Test Scenarios

### Scenario 1: Simple Application Deployment

**Objective**: Test basic deployment workflow for a simple application

#### Phase 1: Get Deployment Guidance

1. **Ask for deployment plan**:
   ```
   I have a Node.js Express application. Create a plan to deploy this 
   application to Azure.
   ```

2. **Verify deployment plan includes**:
   - [ ] Resource requirements (App Service, Storage, etc.)
   - [ ] Configuration steps
   - [ ] Deployment method recommendations
   - [ ] Post-deployment verification steps

3. **Get Azure CLI deployment commands**:
   ```
   Generate Azure CLI commands to deploy my Node.js app to App Service
   ```

4. **Get infrastructure as code**:
   ```
   Generate a Bicep template to deploy my application infrastructure
   ```

**Verify**:
- [ ] Deployment plan is comprehensive
- [ ] CLI commands are correct
- [ ] Infrastructure code is valid
- [ ] Steps are in logical order

#### Phase 2: Execute Deployment

5. **Deploy application**:
   ```
   Deploy my Node.js application to Azure App Service 'bugbash-app-<random>' 
   in resource group '<your-rg>'
   ```

6. **Monitor deployment**:
   ```
   Show me the deployment status for app service 'bugbash-app-<random>'
   ```

7. **Verify deployment**:
   ```
   Check if my app is running on 'bugbash-app-<random>'
   ```

8. **Get application URL**:
   ```
   What is the URL for my deployed application?
   ```

**Verify**:
- [ ] Deployment completes successfully
- [ ] Application is accessible
- [ ] URL is correct
- [ ] App responds to requests

**Expected Results**:
- Deployment plan provided
- Infrastructure created
- Application deployed
- App is accessible and functional

---

### Scenario 2: Multi-Environment Deployment

**Objective**: Test deployment across development, staging, and production environments

#### Phase 1: Create Environment Infrastructure

1. **Create development environment**:
   ```
   Create Azure resources for a development environment:
   - Resource group: 'bugbash-dev-rg'
   - App Service: 'bugbash-app-dev'
   - SQL Database: 'bugbash-db-dev' (Basic tier)
   - Storage account: 'bugbashdevstore'
   - Region: East US
   ```

2. **Create staging environment**:
   ```
   Create Azure resources for a staging environment:
   - Resource group: 'bugbash-staging-rg'
   - App Service: 'bugbash-app-staging'
   - SQL Database: 'bugbash-db-staging' (Standard tier)
   - Storage account: 'bugbashstagingstore'
   - Region: East US
   ```

3. **Create production environment**:
   ```
   Create Azure resources for a production environment:
   - Resource group: 'bugbash-prod-rg'
   - App Service: 'bugbash-app-prod' (Premium tier)
   - SQL Database: 'bugbash-db-prod' (Standard tier with geo-replication)
   - Storage account: 'bugbashprodstore' with redundancy
   - Region: East US
   ```

**Verify**:
- [ ] All three environments created
- [ ] Appropriate tiers for each environment
- [ ] Naming conventions consistent
- [ ] Resources properly tagged

#### Phase 2: Deploy to Environments

4. **Deploy to development**:
   ```
   Deploy my application to the development environment 
   (bugbash-app-dev) with debug settings enabled
   ```

5. **Deploy to staging**:
   ```
   Deploy my application to the staging environment 
   (bugbash-app-staging) with production-like settings
   ```

6. **Verify staging deployment**:
   ```
   Test my application in the staging environment and verify all 
   features work correctly
   ```

7. **Deploy to production**:
   ```
   After successful staging tests, deploy my application to production 
   environment (bugbash-app-prod)
   ```

**Verify**:
- [ ] Each environment has correct configuration
- [ ] Deployments succeed in order
- [ ] Environment-specific settings applied
- [ ] Production deployment after verification

**Expected Results**:
- All environments created with appropriate configurations
- Deployments succeed to each environment
- Progressive deployment strategy followed
- Each environment is accessible and functional

---

### Scenario 3: CI/CD Pipeline Guidance

**Objective**: Test guidance for setting up continuous integration and deployment

#### Phase 1: Pipeline Recommendations

1. **Get CI/CD guidance**:
   ```
   How can I create a CI/CD pipeline to deploy this application to Azure?
   ```

2. **Get GitHub Actions workflow**:
   ```
   Generate a GitHub Actions workflow to:
   - Build my Node.js application
   - Run tests
   - Deploy to Azure App Service
   - Deploy to staging first, then production after approval
   ```

3. **Get Azure DevOps pipeline**:
   ```
   Generate an Azure DevOps pipeline YAML for deploying my application
   ```

**Verify**:
- [ ] CI/CD recommendations provided
- [ ] GitHub Actions workflow is valid
- [ ] Azure DevOps pipeline is complete
- [ ] Best practices included

#### Phase 2: Pipeline Configuration

4. **Configure deployment credentials**:
   ```
   How do I configure deployment credentials for GitHub Actions to 
   deploy to Azure?
   ```

5. **Set up secrets**:
   ```
   What secrets do I need to configure for automated deployment to Azure?
   ```

6. **Configure service principal**:
   ```
   Help me create a service principal for automated deployments with 
   minimal required permissions
   ```

**Verify**:
- [ ] Credential setup instructions clear
- [ ] Secret requirements documented
- [ ] Service principal guidance provided
- [ ] Security best practices mentioned

**Expected Results**:
- Complete CI/CD guidance provided
- Pipeline templates generated
- Security configuration documented
- Ready to implement automated deployments

### Scenario 4: Deployment Monitoring and Logging

**Objective**: Test deployment monitoring and log analysis capabilities

#### Phase 1: Monitor Deployment

1. **Check deployment history**:
   ```
   Show me the deployment history for app service 'bugbash-app-prod'
   ```

2. **Get deployment logs**:
   ```
   Show me the deployment logs for the latest deployment to 
   'bugbash-app-prod'
   ```

3. **Check application logs**:
   ```
   Show me the application logs for 'bugbash-app-prod' from the last 
   30 minutes
   ```

**Verify**:
- [ ] Deployment history retrieved
- [ ] Logs are accessible
- [ ] Log content is useful
- [ ] Timestamps are accurate

#### Phase 2: Troubleshoot Failed Deployment

4. **Simulate deployment failure**:
   - Intentionally cause a deployment to fail
   - Could be wrong runtime, missing dependency, etc.

5. **Diagnose failure**:
   ```
   My deployment to 'bugbash-app-staging' failed. Help me diagnose 
   what went wrong.
   ```

6. **Get logs for failed deployment**:
   ```
   Show me the error logs for the failed deployment
   ```

7. **Get fix recommendations**:
   ```
   Based on the error logs, what should I do to fix the deployment?
   ```

**Verify**:
- [ ] Failure is detected
- [ ] Error logs retrieved
- [ ] Root cause identified
- [ ] Fix suggestions provided

**Expected Results**:
- Deployment monitoring works
- Logs are accessible
- Failures are diagnosed
- Actionable recommendations provided

## 🐛 Common Issues to Watch For

- **Deployment Timeouts**: Long-running deployments
- **Configuration Errors**: Wrong settings causing failures
- **Resource Dependencies**: Resources deployed in wrong order
- **Authentication Issues**: Credential problems during deployment
- **Network Connectivity**: Firewall or network issues
- **Resource Quotas**: Hitting subscription limits
- **Naming Conflicts**: Duplicate resource names
- **Version Mismatches**: Incompatible runtime versions

## Related Resources

- [Azure Deployment Center](https://learn.microsoft.com/azure/app-service/deploy-continuous-deployment)
- [GitHub Actions for Azure](https://learn.microsoft.com/azure/developer/github/github-actions)
- [Azure DevOps Pipelines](https://learn.microsoft.com/azure/devops/pipelines/)
- [Deploy Test Prompts](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md#azure-deploy)
- [Report Issues](https://github.com/microsoft/mcp/issues)

---

**Next**: [Full Stack Applications Testing](full-stack-apps.md)
