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

    public async Task<List<PolicyAssignment>> ListPolicyAssignmentsAsync(
        string subscription,
        string? scope = null,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenantId, retryPolicy, cancellationToken);
            var armClient = await CreateArmClientAsync(tenantId, retryPolicy, cancellationToken: cancellationToken);

            var assignments = new List<PolicyAssignment>();

            // Get policy assignments collection
            PolicyAssignmentCollection policyAssignments;

            if (string.IsNullOrEmpty(scope))
            {
                // Get subscription-level policy assignments
                policyAssignments = subscriptionResource.GetPolicyAssignments();
            }
            else
            {
                // Get policy assignments at the specified scope
                // This approach works for all scope types including management groups
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

}

