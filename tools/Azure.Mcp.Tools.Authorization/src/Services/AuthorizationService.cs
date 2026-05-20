// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Authorization.Models;
using Azure.Mcp.Tools.Authorization.Services.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Authorization.Services;

public class AuthorizationService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<AuthorizationService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IAuthorizationService
{
    private const string RoleAssignmentApprovalsApiVersion = "2021-01-01-preview";
    private readonly ILogger<AuthorizationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ResourceQueryResults<RoleAssignment>> ListRoleAssignmentsAsync(
        string subscription,
        string scope,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(scope), scope));

        var scopeId = new ResourceIdentifier(scope!);
        return await ExecuteResourceQueryAsync(
            "Microsoft.Authorization/roleAssignments",
            null, // all resource groups
            subscription,
            retryPolicy,
            ConvertToRoleAssignmentModel,
            "authorizationresources",
            additionalFilter: $"id contains '{EscapeKqlString(scope)}'",
            tenant: tenantId,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Converts a JsonElement from Azure Resource Graph query to a role assignment model.
    /// </summary>
    /// <param name="item">The JsonElement containing role assignment data</param>
    /// <returns>The role assignment model</returns>
    private static RoleAssignment ConvertToRoleAssignmentModel(JsonElement item)
    {
        RoleAssignmentData? roleAssignmentData = RoleAssignmentData.FromJson(item)
            ?? throw new InvalidOperationException("Failed to parse role assignment data");

        return new()
        {
            Id = roleAssignmentData.ResourceId,
            Name = roleAssignmentData.ResourceName,
            PrincipalId = roleAssignmentData.Properties?.PrincipalId,
            PrincipalType = roleAssignmentData.Properties?.PrincipalType,
            RoleDefinitionId = roleAssignmentData.Properties?.RoleDefinitionId,
            Scope = roleAssignmentData.Properties?.Scope,
            Description = roleAssignmentData.Properties?.Description,
            DelegatedManagedIdentityResourceId = roleAssignmentData.Properties?.DelegatedManagedIdentityResourceId,
            Condition = roleAssignmentData.Properties?.Condition
        };
    }

    public async Task<List<RoleAssignmentApproval>> ListPendingRoleAssignmentApprovalsAsync(
        string subscription,
        string scope,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(scope), scope));

        var approvalsPath = $"{NormalizeScope(scope)}/providers/Microsoft.Authorization/roleAssignmentApprovals";
        var requestUri = CreateArmUri($"{approvalsPath}?api-version={RoleAssignmentApprovalsApiVersion}&%24filter=asApprover()");

        using var responseDocument = await SendArmRequestAsync(
            HttpMethod.Get,
            requestUri,
            tenantId,
            retryPolicy,
            cancellationToken: cancellationToken);

        var approvals = new List<RoleAssignmentApproval>();
        if (responseDocument.RootElement.TryGetProperty("value", out var values) && values.ValueKind == JsonValueKind.Array)
        {
            foreach (var value in values.EnumerateArray())
            {
                var approval = ConvertToRoleAssignmentApproval(value);
                if (approval.Stages.Any(IsPendingApprovalStage))
                {
                    approvals.Add(approval);
                }
            }
        }

        return approvals;
    }

    public async Task<RoleAssignmentApprovalStage> ApproveRoleAssignmentApprovalAsync(
        string subscription,
        string scope,
        string approval,
        string stage,
        string justification,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(scope), scope),
            (nameof(approval), approval),
            (nameof(stage), stage),
            (nameof(justification), justification));

        var stagePath = GetApprovalStagePath(scope, approval, stage);
        var requestUri = CreateArmUri($"{stagePath}?api-version={RoleAssignmentApprovalsApiVersion}");

        using var content = CreateApprovalContent(justification);
        using var responseDocument = await SendArmRequestAsync(
            HttpMethod.Patch,
            requestUri,
            tenantId,
            retryPolicy,
            content,
            cancellationToken);

        return ConvertToRoleAssignmentApprovalStage(responseDocument.RootElement);
    }

    private async Task<JsonDocument> SendArmRequestAsync(
        HttpMethod method,
        Uri requestUri,
        string? tenantId,
        RetryPolicyOptions? retryPolicy,
        HttpContent? content = null,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = TenantService.GetClient();
        if (retryPolicy?.NetworkTimeoutSeconds is { } networkTimeoutSeconds)
        {
            httpClient.Timeout = TimeSpan.FromSeconds(networkTimeoutSeconds);
        }

        using var request = new HttpRequestMessage(method, requestUri)
        {
            Content = content
        };

        var token = await GetArmAccessTokenAsync(tenantId, cancellationToken);
        request.Headers.Authorization = new("Bearer", token.Token);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Azure RBAC PIM approval request failed with status {(int)response.StatusCode} ({response.StatusCode}): {responseContent}",
                null,
                response.StatusCode);
        }

        return JsonDocument.Parse(responseContent);
    }

    private Uri CreateArmUri(string pathAndQuery)
    {
        var relativePathAndQuery = pathAndQuery.TrimStart('/');
        return new Uri(TenantService.CloudConfiguration.ArmEnvironment.Endpoint, relativePathAndQuery);
    }

    private static ByteArrayContent CreateApprovalContent(string justification)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("properties");
            writer.WriteStartObject();
            writer.WriteString("reviewResult", "Approve");
            writer.WriteString("justification", justification);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return new ByteArrayContent(stream.ToArray())
        {
            Headers = { ContentType = new("application/json") }
        };
    }

    private static string GetApprovalStagePath(string scope, string approval, string stage)
    {
        if (stage.StartsWith("/", StringComparison.Ordinal))
        {
            return stage;
        }

        var approvalPath = approval.StartsWith("/", StringComparison.Ordinal)
            ? approval
            : $"{NormalizeScope(scope)}/providers/Microsoft.Authorization/roleAssignmentApprovals/{Uri.EscapeDataString(approval)}";

        return $"{approvalPath.TrimEnd('/')}/stages/{Uri.EscapeDataString(stage)}";
    }

    private static string NormalizeScope(string scope)
    {
        var normalizedScope = scope.Trim();
        return normalizedScope.StartsWith("/", StringComparison.Ordinal)
            ? normalizedScope
            : $"/{normalizedScope}";
    }

    private static RoleAssignmentApproval ConvertToRoleAssignmentApproval(JsonElement item)
    {
        var properties = GetObjectProperty(item, "properties");
        var approval = new RoleAssignmentApproval
        {
            Id = GetStringProperty(item, "id"),
            Name = GetStringProperty(item, "name"),
            Type = GetStringProperty(item, "type"),
            PrincipalId = GetStringProperty(properties, "principalId"),
            RoleDefinitionId = GetStringProperty(properties, "roleDefinitionId"),
            RequestorId = GetStringProperty(properties, "requestorId"),
            Scope = GetStringProperty(properties, "scope"),
            Status = GetStringProperty(properties, "status"),
            CreatedOn = GetStringProperty(properties, "createdOn")
        };

        var stages = GetArrayProperty(properties, "stages");
        if (stages.HasValue)
        {
            foreach (var stage in stages.Value.EnumerateArray())
            {
                approval.Stages.Add(ConvertToRoleAssignmentApprovalStage(stage));
            }
        }

        return approval;
    }

    private static RoleAssignmentApprovalStage ConvertToRoleAssignmentApprovalStage(JsonElement item)
    {
        var properties = GetObjectProperty(item, "properties");
        return new()
        {
            Id = GetStringProperty(item, "id"),
            DisplayName = GetStringProperty(item, "displayName") ?? GetStringProperty(properties, "displayName"),
            AssignedToMe = GetBoolProperty(item, "assignedToMe") ?? GetBoolProperty(properties, "assignedToMe"),
            Status = GetStringProperty(item, "status") ?? GetStringProperty(properties, "status"),
            ReviewResult = GetStringProperty(item, "reviewResult") ?? GetStringProperty(properties, "reviewResult"),
            ReviewedBy = GetStringProperty(item, "reviewedBy") ?? GetStringProperty(properties, "reviewedBy"),
            ReviewedDateTime = GetStringProperty(item, "reviewedDateTime") ?? GetStringProperty(properties, "reviewedDateTime"),
            Justification = GetStringProperty(item, "justification") ?? GetStringProperty(properties, "justification")
        };
    }

    private static bool IsPendingApprovalStage(RoleAssignmentApprovalStage stage)
    {
        return IsPendingStatus(stage.Status) && string.IsNullOrEmpty(stage.ReviewResult);
    }

    private static bool IsPendingStatus(string? status)
    {
        return string.IsNullOrEmpty(status)
            || status.Equals("Pending", StringComparison.OrdinalIgnoreCase)
            || status.Equals("InProgress", StringComparison.OrdinalIgnoreCase)
            || status.Equals("NotStarted", StringComparison.OrdinalIgnoreCase);
    }

    private static JsonElement GetObjectProperty(JsonElement item, string propertyName)
    {
        return TryGetProperty(item, propertyName, out var property) && property.ValueKind == JsonValueKind.Object
            ? property
            : default;
    }

    private static JsonElement? GetArrayProperty(JsonElement item, string propertyName)
    {
        return TryGetProperty(item, propertyName, out var property) && property.ValueKind == JsonValueKind.Array
            ? property
            : null;
    }

    private static string? GetStringProperty(JsonElement item, string propertyName)
    {
        return TryGetProperty(item, propertyName, out var property) && property.ValueKind != JsonValueKind.Null
            ? property.ToString()
            : null;
    }

    private static bool? GetBoolProperty(JsonElement item, string propertyName)
    {
        if (!TryGetProperty(item, propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    private static bool TryGetProperty(JsonElement item, string propertyName, out JsonElement property)
    {
        if (item.ValueKind == JsonValueKind.Object)
        {
            foreach (var candidate in item.EnumerateObject())
            {
                if (candidate.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    property = candidate.Value;
                    return true;
                }
            }
        }

        property = default;
        return false;
    }
}
