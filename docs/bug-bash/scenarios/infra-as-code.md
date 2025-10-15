# Infrastructure as Code Testing Scenario

Test Azure MCP Server's ability to generate and deploy infrastructure using Bicep templates and Azure resources.

## Objectives

- Generate Bicep templates for Azure resources
- Deploy infrastructure using generated templates
- Validate deployed resources
- Test different resource types
- Verify error handling during deployment

## Prerequisites

- [ ] Azure MCP Server installed and configured
- [ ] Azure CLI installed (`az --version`)
- [ ] Authenticated to Azure (`az login`)
- [ ] Active Azure subscription
- [ ] Resource group for testing (or permission to create one)
- [ ] GitHub Copilot with Agent mode enabled

## Test Scenarios

### Scenario 1: Generate Simple Storage Account Bicep Template

**Objective**: Test basic Bicep template generation

**Steps**:

1. Open GitHub Copilot Chat in Agent mode

2. Use this prompt:
   ```
   Generate a Bicep template to create an Azure Storage account named 
   'bugbashstorage' in the 'eastus' region with Standard_LRS redundancy
   ```

3. **Verify**:
   - [ ] Bicep template is generated
   - [ ] Template includes storage account resource
   - [ ] Properties match requested configuration
   - [ ] Template syntax is valid

4. **Save the template** to `storage-account.bicep`

5. **Deploy the template**:
   ```bash
   az deployment group create \
     --resource-group <your-rg> \
     --template-file storage-account.bicep
   ```

6. **Verify deployment**:
   - Ask Copilot: `"List my storage accounts in resource group <your-rg>"`
   - Confirm the storage account appears

**Expected Results**:
- Valid Bicep template generated
- Template deploys successfully
- Resource appears in Azure

**Things to Check**:
- [ ] Template follows Bicep best practices
- [ ] Resource names are valid
- [ ] Parameters are properly defined
- [ ] Outputs are included (if applicable)


### Scenario 2: Generate Multi-Resource Infrastructure

**Objective**: Test complex infrastructure generation

**Steps**:

1. Use this prompt:
   ```
   Generate a Bicep template that creates:
   - A storage account
   - An App Service plan (B1 tier)
   - A web app connected to the storage account
   - An Application Insights resource
   All resources should be in the East US region
   ```

2. **Verify the template includes**:
   - [ ] All requested resources
   - [ ] Proper resource dependencies
   - [ ] Connection strings/references
   - [ ] Valid parameter definitions

3. **Review the template** for:
   - [ ] Naming conventions
   - [ ] Resource locations
   - [ ] SKU/tier selections
   - [ ] Resource links

4. **Deploy the template**:
   ```bash
   az deployment group create \
     --resource-group <your-rg> \
     --template-file multi-resource.bicep
   ```

5. **Verify all resources** are created:
   - Ask Copilot: `"List all resources in resource group <your-rg>"`
   - Confirm all expected resources appear

**Expected Results**:
- Complete Bicep template with all resources
- Proper dependencies between resources
- All resources deploy successfully
- Resources are correctly connected

### Scenario 3: Test Bicep Best Practices

**Objective**: Verify generated templates follow Azure best practices

**Steps**:

1. Use this prompt:
   ```
   Show me the best practices for creating a Bicep template for a
   storage account with blob containers
   ```

2. **Review the guidance** provided

3. Generate a template with best practices:
   ```
   Generate a Bicep template for a storage account following best 
   practices, including:
   - Parameters for names and locations
   - Variables for common values
   - Outputs for important values
   - Proper resource dependencies
   ```

4. **Verify the template includes**:
   - [ ] Parameterized values
   - [ ] Variables for reusable values
   - [ ] Descriptive resource descriptions
   - [ ] Outputs for key information
   - [ ] Tags for resource management

**Expected Results**:
- Template follows Azure naming conventions
- Sensitive values are parameterized
- Template is reusable across environments
- Outputs provide useful information

## Common Issues to Watch For

- **Naming Conflicts**: Resources with duplicate names
- **Region Availability**: Services not available in requested regions
- **Quota Limits**: Exceeding subscription quotas
- **Dependency Ordering**: Resources deployed in wrong order
- **Authentication Issues**: Permission problems during deployment
- **Parameter Validation**: Invalid parameter values
- **Syntax Errors**: Invalid Bicep syntax
- **Version Compatibility**: API version mismatches

## Related Resources

- [Azure Bicep Documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
- [Azure Best Practices Tool](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md#azure-mcp-best-practices)
- [Troubleshooting Deployments](troubleshooting.md)
- [Report Issues](https://github.com/microsoft/mcp/issues)

---

**Next**: [PaaS Services Testing](paas-services.md)
