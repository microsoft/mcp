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
    Description = "Resolve observed Azure Advisor Service Group IDs from Advisor recommendation records. " +
        "This is the dedicated read-only scope operation for broad Service Group cost analysis; it does not enumerate generic Azure resources. " +
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
    private const string ServiceGroupScopeQuery =
        """
        advisorresources
        | where type =~ "microsoft.advisor/recommendations"
        | where tolower(id) startswith "/providers/microsoft.management/servicegroups/"
        | project serviceGroupId = tolower(extract(@"/providers/microsoft.management/servicegroups/([^/]+)", 1, tostring(id)))
        | where isnotempty(serviceGroupId)
        | distinct serviceGroupId
        | order by serviceGroupId asc
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
                .Where(static row => row.TryGetProperty("serviceGroupId", out _))
                .Select(static row => row.GetProperty("serviceGroupId").GetString())
                .Where(static id => !string.IsNullOrWhiteSpace(id))
                .Select(static id => id!)
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
        int TotalGroups,
        bool ResultTruncated,
        string ExecutedQuery);
}
