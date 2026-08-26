// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Runs;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs;

[CommandMetadata(
    Id = "8fe94d6f-feb1-468c-bdbd-c93384e4639a",
    Name = "failover",
    Title = "Fail Over a Resilience Drill Run",
    Description = "Initiates failover for a run of a resilience drill from specified physical Azure zones in an Azure service group.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillRunFailoverCommand(ILogger<DrillRunFailoverCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillRunFailoverOptions, DrillRunFailoverCommand.DrillRunFailoverCommandResult>
{
    private readonly ILogger<DrillRunFailoverCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillRunFailoverOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ValidatePathSegment(options.ServiceGroup, "service group", validationResult);
        ValidatePathSegment(options.Drill, "drill", validationResult);
        ValidatePathSegment(options.DrillRun, "drill run", validationResult);

        if (options.SourceLocations.Length == 0)
        {
            validationResult.Errors.Add("At least one source location is required.");
        }
        else if (options.SourceLocations.Any(location => !IsValidPhysicalZone(location)))
        {
            validationResult.Errors.Add("Each source location must use the physical Azure zone format '<region>-az<zone-number>', such as 'eastus-az1'.");
        }

        if (options.SelectedResourceIds?.Any(resourceId => !IsValidResourceIdentifier(resourceId)) == true)
        {
            validationResult.Errors.Add("Each selected resource ID must be an absolute Azure resource ID.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillRunFailoverOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await _resilienceManagementService.FailoverDrillRunAsync(
                options.ServiceGroup,
                options.Drill,
                options.DrillRun,
                options.SourceLocations,
                options.SelectedResourceIds,
                options.AutoFailover,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var result = new DrillRunFailoverCommandResult(options.DrillRun, Accepted: true);
            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.DrillRunFailoverCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error initiating drill run failover. ServiceGroup: {ServiceGroup}, Drill: {Drill}, DrillRun: {DrillRun}.",
                options.ServiceGroup, options.Drill, options.DrillRun);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Failover cannot start while the drill run is in its current state. Complete the active operation or verify the drill run state, then try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed initiating drill run failover. Verify you have permission to run drill actions in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill run not found. Verify the drill run, drill, and service group exist and you have access.",
        RequestFailedException =>
            "The drill run failover request failed. Verify the drill run, source locations, selected resources, and service group, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    private static void ValidatePathSegment(string value, string optionName, ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('/') || value.Contains('\\'))
        {
            validationResult.Errors.Add($"The {optionName} name must be a single non-empty path segment.");
        }
    }

    private static bool IsValidPhysicalZone(string location)
    {
        const string separator = "-az";
        int separatorIndex = location.LastIndexOf(separator, StringComparison.OrdinalIgnoreCase);
        if (separatorIndex <= 0 || separatorIndex + separator.Length >= location.Length)
        {
            return false;
        }

        ReadOnlySpan<char> region = location.AsSpan(0, separatorIndex);
        ReadOnlySpan<char> zone = location.AsSpan(separatorIndex + separator.Length);
        return IsAsciiAlphanumeric(region) &&
            int.TryParse(zone, out int zoneNumber) &&
            zoneNumber > 0;
    }

    private static bool IsAsciiAlphanumeric(ReadOnlySpan<char> value)
    {
        foreach (char character in value)
        {
            if (!char.IsAsciiLetterOrDigit(character))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidResourceIdentifier(string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId) || !resourceId.StartsWith('/'))
        {
            return false;
        }

        try
        {
            _ = new ResourceIdentifier(resourceId);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public sealed record DrillRunFailoverCommandResult(string DrillRun, bool Accepted);
}
