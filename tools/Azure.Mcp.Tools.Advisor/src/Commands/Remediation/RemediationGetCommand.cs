// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Advisor.Options.Remediation;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.Remediation;

[CommandMetadata(
    Id = "cdef7740-73b6-4492-8670-53b1cb98ff5a",
    Name = "get",
    Title = "Get Advisor Recommendation Remediation",
    Description = "Get the Azure Advisor remediation package for a specific recommendation type id. " +
        "Depending on the recommendation, the package returns one of three output types: remediation guidance " +
        "(manual, human-readable steps), a hybrid of manual steps plus executable artifacts, or executable " +
        "artifacts (Azure CLI, PowerShell, Bicep, and ARM template). Also includes remediation metadata, " +
        "safety flags (destructive, reversible, grounded, confidence), methods with parameters, ordered steps, " +
        "and verification. Use when an agent needs step-by-step guidance and/or an executable script to fix a recommendation.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RemediationGetCommand(ILogger<RemediationGetCommand> logger, IRemediationService remediationService)
    : AuthenticatedCommand<RemediationGetOptions, RemediationGetCommand.RemediationGetResult>()
{
    private readonly IRemediationService _remediationService = remediationService;
    private readonly ILogger<RemediationGetCommand> _logger = logger;

    public override void ValidateOptions(RemediationGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrEmpty(options.RecommendationTypeId) ||
            options.RecommendationTypeId.Length != 36 ||
            !Guid.TryParseExact(options.RecommendationTypeId, "D", out _))
        {
            validationResult.Errors.Add("--recommendation-type-id must be a 36-character GUID in the form xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RemediationGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var remediation = await _remediationService.GetRemediationAsync(
                options.RecommendationTypeId,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new RemediationGetResult(remediation),
                AdvisorJsonContext.Default.RemediationGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting Advisor remediation. RecommendationTypeId: {RecommendationTypeId}.",
                options.RecommendationTypeId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.NotFound =>
            "No remediation was found for the specified recommendation type id. Verify the recommendation type id.",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record RemediationGetResult(Models.RemediationPackage Remediation);
}
