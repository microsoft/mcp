// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.Recommendation;

[CommandMetadata(
    Id = "e8f4f6d2-2a11-4e26-9f9e-0ee65e6ab7f5",
    Name = "recommendation_service_group_scope",
    Title = "Resolve Advisor Recommendation Service Group Scope",
    Description = "Resolve tenant-root Azure Service Group IDs from Azure Resource Graph. " +
        "This is the dedicated read-only scope operation for broad Service Group cost analysis. " +
        "The result contains deduplicated Service Group IDs and completeness metadata.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationServiceGroupScopeCommand(
    ILogger<RecommendationServiceGroupScopeCommand> logger,
    IAzureService azureService)
    : AuthenticatedCommand<
        RecommendationServiceGroupScopeOptions,
        RecommendationServiceGroupScopeCommand.RecommendationServiceGroupScopeResult>
{
    // Service Groups are tenant-root resources. Querying resourcecontainers rather than
    // recommendation instances returns groups even when they have no Advisor record.
    private const string ServiceGroupScopeQuery =
        """
        resourcecontainers
        | where type =~ "microsoft.management/servicegroups"
        | extend
            serviceGroupId = tostring(id),
            serviceGroupName = coalesce(
                tostring(properties.displayName),
                tostring(name)
            ),
            parentServiceGroupId = tostring(properties.parent.resourceId),
            tenantRootServiceGroupId =
                strcat(
                    "/providers/Microsoft.Management/serviceGroups/",
                    tostring(tenantId))
        | where isnotempty(serviceGroupId)
        | where parentServiceGroupId =~ tenantRootServiceGroupId
        | project id = serviceGroupId, name = serviceGroupName
        | order by name asc
        """;

    private readonly ILogger<RecommendationServiceGroupScopeCommand> _logger = logger;
    private readonly IAzureService _azureService = azureService;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        RecommendationServiceGroupScopeOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var executor = new AdvisorResourceGraphQueryExecutor(_azureService);
            var tenant = await executor.ResolveTenantResourceAsync(options.Tenant, cancellationToken);
            var result = await AdvisorResourceGraphQueryExecutor.ExecutePagedAsync(
                tenant,
                ServiceGroupScopeQuery,
                cancellationToken);

            var serviceGroups = result.Rows
                .Where(static row => row.TryGetProperty("id", out _))
                .Select(static row => row.GetProperty("id").GetString())
                .Where(static id => !string.IsNullOrWhiteSpace(id))
                .Select(static id => id![(id!.LastIndexOf('/') + 1)..])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static id => id, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            context.Response.Results = ResponseResult.Create(
                new RecommendationServiceGroupScopeResult(
                    serviceGroups,
                    serviceGroups.Length,
                    result.IsTruncated,
                    ServiceGroupScopeQuery),
                AdvisorJsonContext.Default.RecommendationServiceGroupScopeResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Tenant: {Tenant}", Name, options.Tenant);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record RecommendationServiceGroupScopeResult(
        IReadOnlyList<string> ServiceGroups,
        int TotalServiceGroupCount,
        bool ResultTruncated,
        string ExecutedQuery);
}
