// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Options.ServiceGroupIntelligence;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.ServiceGroupIntelligence;

[CommandMetadata(
    Id = "6c3b6f0e-5a62-4d7e-9a53-1f0b5b0d7c41",
    Name = "get",
    Title = "Get Advisor Service Group Status",
    Description = "Get the current Azure Advisor status and criticality of Service Groups, showing which Service Groups are at risk, need attention, are critical or on track. " +
                  "Use for questions about the status of one Service Group or all Service Groups, and which mission critical (Tier 0) or other tier Service Groups need attention. " +
                  "Provide --service-group (name only, not an ARM ID) for one Service Group; when omitted, all accessible Service Groups are returned. " +
                  "Filter by --criticality (MissionCritical = Tier 0, BusinessCritical = Tier 1, BusinessOperational = Tier 2, Administrative = Tier 3) and by --status (On Track, Attention Needed, At Risk, Critical, Unavailable); map any status or tier named in the question to these filters. " +
                  "Cap results with --top (default 50, max 500). " +
                  "Each result returns the Service Group name and ID, criticality, status, status description, a description of where to focus, and last updated time. " +
                  "Present the status description and focus description exactly as returned, without rewording. " +
                  "If asked how status is calculated, answer: Azure Advisor considers the proportion of applicable resources affected by active recommendations across Zonal Resiliency, Cost Optimization, and Service Retirement, not just the number of recommendations. " +
                  "Higher-priority recommendation types have greater influence, and the results are combined into the overall Status. " +
                  "Only applicable areas with available data contribute. " +
                  "The Description, when available, identifies where to focus. " +
                  "Supporting Insight counts do not by themselves determine Status or predict how much it will improve. " +
                  "For aggregated KPIs such as idle resources or zonal resiliency use advisor insight list instead.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ServiceGroupIntelligenceGetCommand(
    ILogger<ServiceGroupIntelligenceGetCommand> logger,
    IAdvisorServiceGroupIntelligenceService service)
    : AuthenticatedCommand<ServiceGroupIntelligenceGetOptions, ServiceGroupIntelligenceGetCommand.ServiceGroupIntelligenceGetResult>()
{
    private const int MinTop = 1;
    private const int MaxTop = 500;
    private const int DefaultTop = 50;

    private readonly ILogger<ServiceGroupIntelligenceGetCommand> _logger = logger;
    private readonly IAdvisorServiceGroupIntelligenceService _service = service;

    public override void PostBindOptions(ServiceGroupIntelligenceGetOptions options)
    {
        if (options.ServiceGroup is not null)
        {
            options.ServiceGroup = options.ServiceGroup.Trim().Trim('"', '\'').Trim();
        }
    }

    public override void ValidateOptions(ServiceGroupIntelligenceGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.ServiceGroup is not null)
        {
            if (string.IsNullOrWhiteSpace(options.ServiceGroup))
            {
                validationResult.AddError("--service-group must not be empty.", "Invalid Service Group.");
            }
            else if (options.ServiceGroup.Contains('/') || options.ServiceGroup.Length > 250)
            {
                validationResult.AddError(
                    "--service-group must be the Service Group name (for example 'commerce'), not an ARM resource ID.",
                    "Invalid Service Group.");
            }
        }

        if (options.Criticality is not null
            && !AdvisorServiceGroupIntelligenceService.TryNormalizeCriticality(options.Criticality, out _))
        {
            validationResult.AddError(
                "--criticality must be one of: MissionCritical, BusinessCritical, BusinessOperational, Administrative.",
                "Invalid criticality.");
        }

        if (options.Status is not null
            && !AdvisorServiceGroupIntelligenceService.TryNormalizeStatus(options.Status, out _))
        {
            validationResult.AddError(
                "--status must be one of: On Track, Attention Needed, At Risk, Critical, Unavailable.",
                "Invalid status.");
        }

        if (options.Top is { } top && (top < MinTop || top > MaxTop))
        {
            validationResult.AddError(
                $"--top must be between {MinTop} and {MaxTop}.",
                "Invalid --top value.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ServiceGroupIntelligenceGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var results = await _service.GetStatusAsync(
                options.ServiceGroup,
                options.Criticality,
                options.Status,
                options.Top ?? DefaultTop,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(results.Results, results.AreResultsTruncated),
                AdvisorJsonContext.Default.ServiceGroupIntelligenceGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting Advisor Service Group status. HasServiceGroup: {HasServiceGroup}, Criticality: {Criticality}, Status: {Status}, Top: {Top}.",
                options.ServiceGroup is not null,
                options.Criticality,
                options.Status,
                options.Top);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx when argEx.Message.StartsWith("Multiple tenants", StringComparison.Ordinal) =>
            argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed querying Advisor Service Group status in Azure Resource Graph. Verify the signed-in identity has Reader access to the Service Group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "The requested Service Group or its Advisor status was not found.",
        RequestFailedException =>
            "Failed to query Advisor Service Group status in Azure Resource Graph.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public sealed record ServiceGroupIntelligenceGetResult(List<AdvisorServiceGroupStatus> ServiceGroups, bool AreResultsTruncated);
}
