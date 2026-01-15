// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Policy.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Models;
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
                    Identity = SerializeManagedIdentity(assignment.Data.ManagedIdentity),
                    Location = assignment.Data.Location?.ToString()
                };

                // Fetch the policy definition details
                if (!string.IsNullOrEmpty(assignment.Data.PolicyDefinitionId))
                {
                    try
                    {
                        result.PolicyDefinition = await GetPolicyDefinitionAsync(
                            assignment.Data.PolicyDefinitionId,
                            tenantId,
                            retryPolicy,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Failed to fetch policy definition '{PolicyDefinitionId}' for assignment '{AssignmentId}'",
                            assignment.Data.PolicyDefinitionId, assignment.Id);
                        // Continue processing other assignments even if one definition fetch fails
                    }
                }

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
        string policyDefinitionId,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resourceId = new ResourceIdentifier(policyDefinitionId);

            // Extract the policy definition name from the resource ID
            // Format: /providers/Microsoft.Authorization/policyDefinitions/{name}
            // or /subscriptions/{sub}/providers/Microsoft.Authorization/policyDefinitions/{name}
            var policyDefinitionName = resourceId.Name;

            // Determine if this is a built-in (tenant-level) or subscription-level policy
            SubscriptionPolicyDefinitionResource? policyDefinitionResource = null;

            if (policyDefinitionId.Contains("/subscriptions/"))
            {
                // Subscription-level policy definition
                var subscriptionId = resourceId.SubscriptionId;
                if (!string.IsNullOrEmpty(subscriptionId))
                {
                    var subscriptionResource = await _subscriptionService.GetSubscription(subscriptionId, tenantId, retryPolicy, cancellationToken);
                    policyDefinitionResource = await subscriptionResource.GetSubscriptionPolicyDefinitionAsync(policyDefinitionName, cancellationToken);
                }
            }
            else
            {
                // Built-in (tenant-level) policy definition - try to get from any subscription's built-in definitions
                // Built-in policies are accessible from any subscription
                var subscriptions = await _subscriptionService.GetSubscriptions(tenantId, retryPolicy, cancellationToken);
                if (subscriptions.Count > 0)
                {
                    var firstSubscriptionId = subscriptions[0].SubscriptionId;
                    var subscriptionResource = await _subscriptionService.GetSubscription(firstSubscriptionId, tenantId, retryPolicy, cancellationToken);
                    policyDefinitionResource = await subscriptionResource.GetSubscriptionPolicyDefinitionAsync(policyDefinitionName, cancellationToken);
                }
            }

            if (policyDefinitionResource == null)
            {
                _logger.LogWarning("Could not retrieve policy definition '{PolicyDefinitionId}'", policyDefinitionId);
                return null;
            }

            return new PolicyDefinition
            {
                Id = policyDefinitionResource.Id.ToString(),
                Name = policyDefinitionResource.Data.Name,
                Type = policyDefinitionResource.Data.ResourceType.ToString(),
                DisplayName = policyDefinitionResource.Data.DisplayName,
                Description = policyDefinitionResource.Data.Description,
                PolicyType = policyDefinitionResource.Data.PolicyType?.ToString(),
                Mode = policyDefinitionResource.Data.Mode,
                PolicyRule = policyDefinitionResource.Data.PolicyRule?.ToString(),
                Parameters = policyDefinitionResource.Data.Parameters?.ToString(),
                Metadata = policyDefinitionResource.Data.Metadata?.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting policy definition '{PolicyDefinitionId}'",
                policyDefinitionId);
            throw;
        }
    }

    private static string? SerializeManagedIdentity(ManagedServiceIdentity? identity)
    {
        if (identity == null)
        {
            return null;
        }

        var identityData = new ManagedIdentityInfo
        {
            Type = identity.ManagedServiceIdentityType.ToString(),
            PrincipalId = identity.PrincipalId?.ToString(),
            TenantId = identity.TenantId?.ToString(),
            UserAssignedIdentities = identity.UserAssignedIdentities?.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => new UserAssignedIdentityDetails
                {
                    PrincipalId = kvp.Value.PrincipalId?.ToString(),
                    ClientId = kvp.Value.ClientId?.ToString()
                })
        };

        return JsonSerializer.Serialize(identityData, Commands.PolicyJsonContext.Default.ManagedIdentityInfo);
    }

}

