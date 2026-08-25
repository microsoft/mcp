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
        "Returns remediation metadata, safety flags (destructive, reversible, grounded, confidence), " +
        "human-readable methods with parameters, ordered steps, and verification, and ready-to-run inline " +
        "artifacts (Azure CLI, PowerShell, Bicep, and ARM template). " +
        "Use when an agent needs the step-by-step guidance or an executable script to fix a recommendation.",
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

        if (string.IsNullOrEmpty(options.RecommendationId) ||
            options.RecommendationId.Length != 36 ||
            !Guid.TryParseExact(options.RecommendationId, "D", out _))
        {
            validationResult.Errors.Add("--recommendation-id must be a 36-character GUID in the form xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RemediationGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var remediation = await _remediationService.GetRemediationAsync(
                options.RecommendationId,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new RemediationGetResult(remediation),
                AdvisorJsonContext.Default.RemediationGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting Advisor remediation. RecommendationId: {RecommendationId}.",
                options.RecommendationId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.NotFound =>
            "No remediation was found for the specified recommendation type. Verify the recommendation id.",
        HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.Unauthorized =>
            $"Authentication failed accessing the Advisor remediation API. Please run 'az login' and try again. Details: {httpEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record RemediationGetResult(Models.RemediationPackage Remediation);
}
