// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Authorization.Models;
using Azure.Mcp.Tools.Authorization.Services.Models;

namespace Azure.Mcp.Tools.Authorization.Services;

public class AuthorizationService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IAuthorizationService
{
    private const string ApproveReviewResult = "Approve";
    private const string InProgressStatus = "InProgress";
    private const string NotStartedStatus = "NotStarted";
    private const string PendingStatus = "Pending";
    private const string RoleAssignmentApprovalsApiVersion = "2021-01-01-preview";
    private const string RoleAssignmentsTable = "authorizationresources";
    private const string RoleAssignmentResourceType = "Microsoft.Authorization/roleAssignments";

    public async Task<ResourceQueryResults<RoleAssignment>> ListRoleAssignmentsAsync(
        string? subscription,
        string scope,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameter(nameof(scope), scope);

        // Match the scope itself, plus anything nested beneath it. The trailing separator keeps a scope
        // ending in "rg1" from also matching "rg10".
        var escapedScope = EscapeKqlString(scope.TrimEnd('/'));
        var scopeFilter = $"(properties.scope =~ '{escapedScope}' or properties.scope startswith '{escapedScope}/')";

        if (ManagementGroupScope.TryParse(scope, out var managementGroup))
        {
            if (!string.IsNullOrEmpty(subscription))
            {
                throw new ArgumentException(
                    "Subscription must not be specified for a management group scope.",
                    nameof(subscription));
            }

            // Role assignments on a management group are not part of any subscription, so a
            // subscription-scoped Resource Graph query can never return them.
            return await ExecuteManagementGroupResourceQueryAsync(
                RoleAssignmentResourceType,
                managementGroup,
                ConvertToRoleAssignmentModel,
                RoleAssignmentsTable,
                additionalFilter: scopeFilter,
                tenant: tenantId,
                cancellationToken: cancellationToken);
        }

        ValidateRequiredParameter(nameof(subscription), subscription);

        return await ExecuteResourceQueryAsync(
            RoleAssignmentResourceType,
            null, // all resource groups
            subscription,
            ConvertToRoleAssignmentModel,
            RoleAssignmentsTable,
            additionalFilter: scopeFilter,
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
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(scope), scope));
        var resolvedTenantId = await GetSubscriptionTenantIdAsync(subscription, tenantId, cancellationToken);

        var approvalsPath = $"{NormalizeScope(scope)}/providers/Microsoft.Authorization/roleAssignmentApprovals";
        var requestUri = CreateArmUri($"{approvalsPath}?api-version={RoleAssignmentApprovalsApiVersion}&%24filter=asApprover()");

        using var responseDocument = await SendArmRequestAsync(
            HttpMethod.Get,
            requestUri,
            resolvedTenantId,
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
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(scope), scope),
            (nameof(approval), approval),
            (nameof(stage), stage),
            (nameof(justification), justification));
        var resolvedTenantId = await GetSubscriptionTenantIdAsync(subscription, tenantId, cancellationToken);

        var stagePath = GetApprovalStagePath(scope, approval, stage);
        var requestUri = CreateArmUri($"{stagePath}?api-version={RoleAssignmentApprovalsApiVersion}");

        using var content = CreateApprovalContent(justification);
        using var responseDocument = await SendArmRequestAsync(
            HttpMethod.Patch,
            requestUri,
            resolvedTenantId,
            content,
            cancellationToken);

        return ConvertToRoleAssignmentApprovalStage(responseDocument.RootElement);
    }

    private async Task<JsonDocument> SendArmRequestAsync(
        HttpMethod method,
        Uri requestUri,
        string? tenantId,
        HttpContent? content = null,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = AzureService.GetClient();
        using var request = new HttpRequestMessage(method, requestUri)
        {
            Content = content
        };

        var token = await GetArmAccessTokenAsync(tenantId, cancellationToken);
        request.Headers.Authorization = new("Bearer", token.Token);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Azure RBAC PIM approval request failed with status {(int)response.StatusCode}.",
                null,
                response.StatusCode);
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
    }

    private Uri CreateArmUri(string pathAndQuery)
    {
        var relativePathAndQuery = pathAndQuery.TrimStart('/');
        if (Uri.TryCreate(relativePathAndQuery, UriKind.Absolute, out _))
        {
            throw new ArgumentException("ARM request path must be relative.", nameof(pathAndQuery));
        }

        return new Uri(AzureService.CloudConfiguration.ArmEnvironment.Endpoint, relativePathAndQuery);
    }

    private static ByteArrayContent CreateApprovalContent(string justification)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("properties");
            writer.WriteStartObject();
            writer.WriteString("reviewResult", ApproveReviewResult);
            writer.WriteString("justification", justification);
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.Flush();
        }

        var contentBytes = buffer.WrittenMemory.ToArray();
        return new ByteArrayContent(contentBytes)
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
        return new RoleAssignmentApprovalStage
        {
            Id = GetStringProperty(item, "id"),
            DisplayName = GetStringProperty(item, properties, "displayName"),
            AssignedToMe = GetBoolProperty(item, properties, "assignedToMe"),
            Status = GetStringProperty(item, properties, "status"),
            ReviewResult = GetStringProperty(item, properties, "reviewResult"),
            ReviewedBy = GetStringProperty(item, properties, "reviewedBy"),
            ReviewedDateTime = GetStringProperty(item, properties, "reviewedDateTime"),
            Justification = GetStringProperty(item, properties, "justification")
        };
    }

    private async Task<string?> GetSubscriptionTenantIdAsync(
        string subscription,
        string? tenantId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(tenantId))
        {
            return tenantId;
        }

        var subscriptionResource = await AzureService.GetSubscription(subscription, tenantId, cancellationToken);
        return subscriptionResource.Data.TenantId?.ToString();
    }

    private static bool IsPendingApprovalStage(RoleAssignmentApprovalStage stage)
    {
        return IsPendingStatus(stage.Status) && string.IsNullOrEmpty(stage.ReviewResult);
    }

    private static bool IsPendingStatus(string? status)
    {
        return string.IsNullOrEmpty(status)
            || status.Equals(PendingStatus, StringComparison.OrdinalIgnoreCase)
            || status.Equals(InProgressStatus, StringComparison.OrdinalIgnoreCase)
            || status.Equals(NotStartedStatus, StringComparison.OrdinalIgnoreCase);
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

    private static string? GetStringProperty(JsonElement item, JsonElement properties, string propertyName)
    {
        return GetStringProperty(item, propertyName) ?? GetStringProperty(properties, propertyName);
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

    private static bool? GetBoolProperty(JsonElement item, JsonElement properties, string propertyName)
    {
        return GetBoolProperty(item, propertyName) ?? GetBoolProperty(properties, propertyName);
    }

    private static bool TryGetProperty(JsonElement item, string propertyName, out JsonElement property)
    {
        if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty(propertyName, out property))
        {
            return true;
        }

        property = default;
        return false;
    }
}
