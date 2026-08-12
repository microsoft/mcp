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
        "artifacts (Azure CLI, PowerShell, Bicep, and ARM template). Optionally filter to a subset of artifact " +
        "types. Use when an agent needs the step-by-step guidance or an executable script to fix a recommendation.",
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

    // The artifact types supported by the Microsoft.Advisor/remediationTypes contract.
    private static readonly HashSet<string> SupportedArtifactTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "cli", "powershell", "bicep", "arm",
    };

    public override void ValidateOptions(RemediationGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrEmpty(options.RecommendationId) ||
            options.RecommendationId.Length != 36 ||
            !Guid.TryParseExact(options.RecommendationId, "D", out _))
        {
            validationResult.Errors.Add("--recommendation-id must be a 36-character GUID in the form xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.");
        }

        if (!TryParseArtifactTypes(options.ArtifactTypes, out _, out var invalid))
        {
            validationResult.Errors.Add(
                $"--artifact-types contains unsupported value(s): {string.Join(", ", invalid)}. " +
                $"Supported values: {string.Join(", ", SupportedArtifactTypes.OrderBy(a => a, StringComparer.Ordinal))}.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RemediationGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            TryParseArtifactTypes(options.ArtifactTypes, out var artifactTypes, out _);

            var remediation = await _remediationService.GetRemediationAsync(
                options.RecommendationId,
                artifactTypes,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new RemediationGetResult(remediation),
                AdvisorJsonContext.Default.RemediationGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting Advisor remediation. RecommendationId: {RecommendationId}, ArtifactTypes: {ArtifactTypes}.",
                options.RecommendationId, options.ArtifactTypes);
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

    // Parses the comma-separated --artifact-types value into a normalized array.
    // Returns false (with the offending values) when any entry is not a supported artifact type.
    private static bool TryParseArtifactTypes(string? value, out string[]? artifactTypes, out List<string> invalid)
    {
        artifactTypes = null;
        invalid = [];

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var parsed = value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

        foreach (var type in parsed)
        {
            if (!SupportedArtifactTypes.Contains(type))
            {
                invalid.Add(type);
            }
        }

        if (invalid.Count > 0)
        {
            return false;
        }

        artifactTypes = parsed.Length > 0 ? parsed : null;
        return true;
    }

    public sealed record RemediationGetResult(Models.RemediationPackage Remediation);
}
