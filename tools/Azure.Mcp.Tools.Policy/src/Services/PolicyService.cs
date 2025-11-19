// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Policy.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Policy.Services;

public class PolicyService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<PolicyService> logger)
    : BaseAzureService(tenantService), IPolicyService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ILogger<PolicyService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<PolicyAssignment?> GetPolicyAssignmentAsync(
        string assignmentName,
        string scope,
        string subscription,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(assignmentName), assignmentName), (nameof(scope), scope));

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenantId, retryPolicy);
            var armClient = await CreateArmClientAsync(tenantId, retryPolicy);

            // Get the generic resource collection from the scope
            var scopeId = new ResourceIdentifier(scope);

            // Get policy assignments using the ArmClient
            var policyAssignmentResourceId = new ResourceIdentifier($"{scope}/providers/Microsoft.Authorization/policyAssignments/{assignmentName}");
            var policyAssignment = armClient.GetPolicyAssignmentResource(policyAssignmentResourceId);

            // Get the policy assignment
            var response = await policyAssignment.GetAsync(cancellationToken);
            var assignment = response.Value;

            if (assignment == null)
            {
                return null;
            }

            // Convert to our model
            var result = new PolicyAssignment
            {
                Id = assignment.Id.ToString(),
                Name = assignment.Data.Name,
                Type = assignment.Data.ResourceType.ToString(),
                DisplayName = assignment.Data.DisplayName,
                PolicyDefinitionId = assignment.Data.PolicyDefinitionId,
                Scope = assignment.Data.Scope,
                EnforcementMode = assignment.Data.EnforcementMode?.ToString(),
                Description = assignment.Data.Description,
                Metadata = assignment.Data.Metadata?.ToString(),
                Parameters = assignment.Data.Parameters?.ToString(),
                Identity = assignment.Data.ManagedIdentity?.ToString(),
                Location = assignment.Data.Location?.ToString()
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving policy assignment '{AssignmentName}' in scope '{Scope}'",
                assignmentName, scope);
            throw;
        }
    }

    public async Task<List<PolicyAssignment>> ListPolicyAssignmentsAsync(
        string subscription,
        string? scope = null,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenantId, retryPolicy);
            var armClient = await CreateArmClientAsync(tenantId, retryPolicy);

            var assignments = new List<PolicyAssignment>();

            // Get policy assignments collection
            PolicyAssignmentCollection policyAssignments;

            if (string.IsNullOrEmpty(scope))
            {
                // Get subscription-level policy assignments
                // subscriptionResource is already a SubscriptionResource with the correct Id
                policyAssignments = subscriptionResource.GetPolicyAssignments();
            }
            else
            {
                // Get policy assignments at the specified scope
                var scopeId = new ResourceIdentifier(scope);
                var genericResource = armClient.GetGenericResource(scopeId);
                var genericResourceData = await genericResource.GetAsync(cancellationToken);
                policyAssignments = genericResourceData.Value.GetPolicyAssignments();
            }

            // Iterate through all policy assignments
            await foreach (var assignment in policyAssignments.GetAllAsync(cancellationToken: cancellationToken))
            {
                var result = new PolicyAssignment
                {
                    Id = assignment.Id.ToString(),
                    Name = assignment.Data.Name,
                    Type = assignment.Data.ResourceType.ToString(),
                    DisplayName = assignment.Data.DisplayName,
                    PolicyDefinitionId = assignment.Data.PolicyDefinitionId,
                    Scope = assignment.Data.Scope,
                    EnforcementMode = assignment.Data.EnforcementMode?.ToString(),
                    Description = assignment.Data.Description,
                    Metadata = assignment.Data.Metadata?.ToString(),
                    Parameters = assignment.Data.Parameters?.ToString(),
                    Identity = assignment.Data.ManagedIdentity?.ToString(),
                    Location = assignment.Data.Location?.ToString()
                };

                assignments.Add(result);
            }

            return assignments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing policy assignments in subscription '{Subscription}' with scope '{Scope}'",
                subscription, scope ?? "all");
            throw;
        }
    }

    public async Task<PolicyDefinition?> GetPolicyDefinitionAsync(
        string definitionName,
        string? subscription = null,
        string? managementGroup = null,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(definitionName), definitionName));

        // Validate that either subscription or managementGroup is provided
        if (string.IsNullOrEmpty(subscription) && string.IsNullOrEmpty(managementGroup))
        {
            throw new ArgumentException("Either subscription or managementGroup must be specified.");
        }

        try
        {
            var armClient = await CreateArmClientAsync(tenantId, retryPolicy);
            SubscriptionPolicyDefinitionResource? policyDefinition = null;

            if (!string.IsNullOrEmpty(managementGroup))
            {
                // Get management group policy definition
                var managementGroupPolicyDefinitionResourceId = ManagementGroupPolicyDefinitionResource.CreateResourceIdentifier(
                    managementGroup,
                    definitionName);

                var mgPolicyDefinition = armClient.GetManagementGroupPolicyDefinitionResource(managementGroupPolicyDefinitionResourceId);
                var mgResponse = await mgPolicyDefinition.GetAsync(cancellationToken);

                if (mgResponse?.Value == null)
                {
                    return null;
                }

                // Convert management group policy definition to our model
                var mgDefinition = mgResponse.Value;
                return new PolicyDefinition
                {
                    Id = mgDefinition.Id.ToString(),
                    Name = mgDefinition.Data.Name,
                    Type = mgDefinition.Data.ResourceType.ToString(),
                    DisplayName = mgDefinition.Data.DisplayName,
                    Description = mgDefinition.Data.Description,
                    PolicyType = mgDefinition.Data.PolicyType?.ToString(),
                    Mode = mgDefinition.Data.Mode,
                    PolicyRule = mgDefinition.Data.PolicyRule?.ToString(),
                    Parameters = mgDefinition.Data.Parameters?.ToString(),
                    Metadata = mgDefinition.Data.Metadata?.ToString()
                };
            }
            else
            {
                // Get subscription policy definition
                var subscriptionResource = await _subscriptionService.GetSubscription(subscription!, tenantId, retryPolicy);

                var policyDefinitionResourceId = SubscriptionPolicyDefinitionResource.CreateResourceIdentifier(
                    subscriptionResource.Id.SubscriptionId,
                    definitionName);

                policyDefinition = armClient.GetSubscriptionPolicyDefinitionResource(policyDefinitionResourceId);

                // Get the policy definition
                var response = await policyDefinition.GetAsync(cancellationToken);
                var definition = response.Value;

                if (definition == null)
                {
                    return null;
                }

                // Convert to our model
                return new PolicyDefinition
                {
                    Id = definition.Id.ToString(),
                    Name = definition.Data.Name,
                    Type = definition.Data.ResourceType.ToString(),
                    DisplayName = definition.Data.DisplayName,
                    Description = definition.Data.Description,
                    PolicyType = definition.Data.PolicyType?.ToString(),
                    Mode = definition.Data.Mode,
                    PolicyRule = definition.Data.PolicyRule?.ToString(),
                    Parameters = definition.Data.Parameters?.ToString(),
                    Metadata = definition.Data.Metadata?.ToString()
                };
            }
        }
        catch (Exception ex)
        {
            var scope = !string.IsNullOrEmpty(managementGroup)
                ? $"management group '{managementGroup}'"
                : $"subscription '{subscription}'";

            _logger.LogError(ex,
                "Error retrieving policy definition '{DefinitionName}' in {Scope}",
                definitionName, scope);
            throw;
        }
    }
}
