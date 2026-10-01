// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
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
    Description = "Fix, resolve, or remediate a specific Azure Advisor recommendation type id. " +
        "Use this whenever you are asked how to fix or how to resolve an Advisor recommendation. Returns its " +
        "remediation package: step-by-step remediation steps plus ready-to-run artifacts, scripts, and " +
        "deployment templates in ARM template, Bicep, Azure CLI, PowerShell, and terraform formats to " +
        "remediate the issue. Also indicates remediation characteristics such as whether it is destructive, " +
        "reversible, or grounded, along with its confidence, effort, and output type. Use whenever you need " +
        "to fix, resolve, remediate, or verify a recommendation, or want " +
        "the ARM, Bicep, CLI, PowerShell, or terraform artifacts and scripts to remediate it.",
    OperationPlane = ToolOperationPlane.Control,
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
        // The service throws HttpRequestException only when ARM returned an error status, carrying
        // ARM's exact error payload as the message. Surface it as-is instead of the base class's
        // generic "service unavailable or network connectivity issues" text, which is misleading
        // for normal HTTP error responses such as 404 RemediationNotFound.
        HttpRequestException httpEx when IsRemediationUnavailableForTenant(httpEx.Message) =>
            "Azure Advisor returned RemediationNotAvailableForTenant for this recommendation type. " +
            "Review the recommendation in Azure Advisor for its available guidance. " +
            "If you expected a remediation package to be available, contact Azure Support and provide this error " +
            "and the request or correlation ID, if available.",
        HttpRequestException httpEx => httpEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    private static bool IsRemediationUnavailableForTenant(string errorMessage)
    {
        try
        {
            using var errorDocument = JsonDocument.Parse(errorMessage);
            return errorDocument.RootElement.ValueKind == JsonValueKind.Object &&
                errorDocument.RootElement.TryGetProperty("error", out var error) &&
                error.ValueKind == JsonValueKind.Object &&
                error.TryGetProperty("code", out var code) &&
                code.ValueKind == JsonValueKind.String &&
                code.GetString() == "RemediationNotAvailableForTenant";
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public sealed record RemediationGetResult(Models.RemediationPackage Remediation);
}
