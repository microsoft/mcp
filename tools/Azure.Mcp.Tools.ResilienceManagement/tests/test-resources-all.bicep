targetScope = 'tenant'

// =============================================================================
// Standalone live-test provisioning template for Azure Resilience Management.
//
// This template is TENANT-SCOPED and creates the resource group itself, so it is
// NOT deployed by the standard MCP test harness (eng/scripts/Deploy-TestResources.ps1
// only runs New-AzResourceGroupDeployment). Deploy it manually, e.g.:
//
//   New-AzTenantDeployment -Location eastus `
//     -TemplateFile tools/Azure.Mcp.Tools.ResilienceManagement/tests/test-resources-all.bicep `
//     -subscriptionId <sub-id>
//
// Cross-scope resources (the resource group, storage, usage plan, enrollment) are
// created through modules, because Bicep requires modules to deploy to a scope
// other than the file's own scope. The service-group-scoped resources (goal
// template/assignment, recovery plan, drill) are created here at tenant scope.
//
// The deploying identity needs tenant-level serviceGroups/write plus contributor
// on the target subscription.
// =============================================================================

@description('Subscription where test RG, storage account, usage plan and enrollment will be created.')
param subscriptionId string

@description('Azure location for the test resources.')
param location string = 'eastus'

@description('Short prefix. Keep it lowercase and simple because some resources have strict name limits.')
param prefix string = 'rhubtest'

@description('Service Group name. Must be unique in tenant. Keep <= 90 chars.')
param serviceGroupName string = '${prefix}-sg-${uniqueString(tenant().tenantId, subscriptionId)}'

@description('Resource group name for live test resources.')
param resourceGroupName string = '${prefix}-rg-${uniqueString(tenant().tenantId, subscriptionId)}'

@description('Storage account name. Must be globally unique, lowercase, 3-24 chars.')
param storageAccountName string = toLower(take('${prefix}${uniqueString(tenant().tenantId, subscriptionId)}', 24))

@description('Usage plan name. Must match ^[a-zA-Z0-9-]{3,24}$')
param usagePlanName string = take('${prefix}-up-${uniqueString(resourceGroupName)}', 24)

@description('Enrollment name. Must match ^[a-zA-Z0-9-]{3,24}$')
param enrollmentName string = take('${prefix}-en-${uniqueString(serviceGroupName)}', 24)

@description('Goal template name. Must match ^[a-zA-Z0-9-]{3,24}$')
param goalTemplateName string = take('${prefix}-gt-${uniqueString(serviceGroupName)}', 24)

@description('Goal assignment name. Must match ^[a-zA-Z0-9-]{3,24}$')
param goalAssignmentName string = take('${prefix}-ga-${uniqueString(serviceGroupName)}', 24)

@description('Drill name. Must match ^[a-zA-Z0-9-]{3,24}$')
param drillName string = take('${prefix}-dr-${uniqueString(serviceGroupName)}', 24)

@description('Recovery plan name. Must match ^[a-zA-Z0-9-]{3,24}$')
param recoveryPlanName string = take('${prefix}-rp-${uniqueString(serviceGroupName)}', 24)

// ---------------------------------------------------------
// 1. Create Service Group at tenant scope
// ---------------------------------------------------------
resource serviceGroup 'Microsoft.Management/serviceGroups@2024-02-01-preview' = {
  name: serviceGroupName
  properties: {
    displayName: serviceGroupName
  }
}

// ---------------------------------------------------------
// 2. Resource group + storage + usage plan + enrollment
//    (created via a subscription-scoped module because they live in a
//    different scope than this tenant-scoped file).
// ---------------------------------------------------------
module subscriptionResources 'modules/subscription-resources.bicep' = {
  name: 'rhub-subscription-resources'
  scope: subscription(subscriptionId)
  params: {
    resourceGroupName: resourceGroupName
    location: location
    serviceGroupId: serviceGroup.id
    storageAccountName: storageAccountName
    usagePlanName: usagePlanName
    enrollmentName: enrollmentName
  }
}

// ---------------------------------------------------------
// 3. Goal Template on the Service Group
// ---------------------------------------------------------
resource goalTemplate 'Microsoft.AzureResilienceManagement/goalTemplates@2026-04-01-preview' = {
  scope: serviceGroup
  name: goalTemplateName
  properties: {
    goalType: 'Resiliency'
    requireHighAvailability: 'Required'
    requireDisasterRecovery: 'NotRequired'
    regionalRecoveryPointObjective: 'PT15M'
    regionalRecoveryTimeObjective: 'PT30M'
  }
  dependsOn: [
    subscriptionResources
  ]
}

// ---------------------------------------------------------
// 4. Assign Goal Template to the Service Group
// ---------------------------------------------------------
resource goalAssignment 'Microsoft.AzureResilienceManagement/goalAssignments@2026-04-01-preview' = {
  scope: serviceGroup
  name: goalAssignmentName
  properties: {
    goalAssignmentType: 'Resiliency'
    goalTemplateId: goalTemplate.id
  }
}

// ---------------------------------------------------------
// 5. Recovery Plan on the Service Group
// ---------------------------------------------------------
resource recoveryPlan 'Microsoft.AzureResilienceManagement/recoveryPlans@2026-04-01-preview' = {
  scope: serviceGroup
  name: recoveryPlanName
  identity: {
    type: 'None'
  }
  properties: {
    planDescription: 'Recovery plan for live testing.'
    planType: 'Regional'
    recoveryGroupsSetting: {
      defaultGroup: {
        properties: {
          description: 'Default recovery group'
          groupUniqueId: guid(serviceGroup.id, recoveryPlanName, 'default-group')
          orderId: 0
          preActions: []
          postActions: []
        }
      }
      additionalGroups: []
    }
  }
  dependsOn: [
    goalAssignment
  ]
}

// ---------------------------------------------------------
// 6. Drill on the Service Group
// ---------------------------------------------------------
resource drill 'Microsoft.AzureResilienceManagement/drills@2026-04-01-preview' = {
  scope: serviceGroup
  name: drillName
  identity: {
    type: 'None'
  }
  properties: {
    drillType: 'Regional'
    drillAssetProperties: {
      subscription: subscriptionId
      resourceGroup: resourceGroupName
      region: location
    }
    rbacSetupMode: 'AutomatedCustomRole'
    chaosResourceProperties: {
      identity: {
        type: 'None'
      }
      chaosResourceIdentityForFaults: {
        type: 'None'
      }
    }
    monitoringProperties: {
      identity: {
        type: 'None'
      }
    }
    recoveryPlanProperties: {
      identity: {
        type: 'None'
      }
    }
  }
  dependsOn: [
    recoveryPlan
  ]
}

// ---------------------------------------------------------
// Outputs (resource IDs)
// ---------------------------------------------------------
output serviceGroupId string = serviceGroup.id
output resourceGroupId string = subscriptionResources.outputs.resourceGroupId
output storageAccountId string = subscriptionResources.outputs.storageAccountId
output usagePlanId string = subscriptionResources.outputs.usagePlanId
output enrollmentId string = subscriptionResources.outputs.enrollmentId
output goalTemplateId string = goalTemplate.id
output goalAssignmentId string = goalAssignment.id
output recoveryPlanId string = recoveryPlan.id
output drillId string = drill.id

// ---------------------------------------------------------
// Outputs (resource names) — consumed by recorded tests via
// RegisterOrRetrieveDeploymentOutputVariable (deployment output keys are UPPERCASE).
// ---------------------------------------------------------
output serviceGroupName string = serviceGroupName
output resourceGroupName string = resourceGroupName
output storageAccountName string = storageAccountName
output usagePlanName string = usagePlanName
output enrollmentName string = enrollmentName
output goalTemplateName string = goalTemplateName
output goalAssignmentName string = goalAssignmentName
output recoveryPlanName string = recoveryPlanName
output drillName string = drillName
