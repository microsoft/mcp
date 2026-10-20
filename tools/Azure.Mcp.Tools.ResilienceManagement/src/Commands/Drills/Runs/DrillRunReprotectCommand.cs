// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Runs;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager.ResilienceManagement;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs;

[CommandMetadata(
    Id = "a331e69f-4669-40de-97ad-10a1cebe4b54",
    Name = "reprotect",
    Title = "Reprotect a Resilience Drill Run",
    Description = "Initiates reprotection for failed-over resources in a resilience drill run. Accepts an optional caller-owned operation ID in canonical GUID format; reuse it for the same logical request, or omit it to generate a new GUID.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillRunReprotectCommand(ILogger<DrillRunReprotectCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillRunReprotectOptions, DrillRunReprotectCommand.DrillRunReprotectCommandResult>
{
    private static readonly ResourceType ServiceGroupResourceType = new("Microsoft.Management/serviceGroups");
    private readonly ILogger<DrillRunReprotectCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillRunReprotectOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        ResilienceManagementOperationIdValidation.Validate(options.OperationId, validationResult);
        ValidatePathSegment(options.ServiceGroup, "service group", validationResult);
        ValidatePathSegment(options.Drill, "drill", validationResult);
        ValidatePathSegment(options.DrillRun, "drill run", validationResult);
        if (options.SelectedResourceIds?.Any(resourceId =>
                !IsDrillResourceId(resourceId, options.ServiceGroup, options.Drill)) == true)
        {
            validationResult.Errors.Add("Each selected resource ID must be a full drill-resource ID under the requested service group and drill with a GUID resource name.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillRunReprotectOptions options, CancellationToken cancellationToken)
    {
        try
        {
            string operationId = await _resilienceManagementService.ReprotectDrillRunAsync(
                options.ServiceGroup,
                options.Drill,
                options.DrillRun,
                options.SelectedResourceIds,
                options.Tenant,
                ResilienceManagementOperationIdValidation.Normalize(options.OperationId),
                cancellationToken);

            var result = new DrillRunReprotectCommandResult(operationId, options.DrillRun, Accepted: true);
            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.DrillRunReprotectCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error initiating drill run reprotection. ServiceGroup: {ServiceGroup}, Drill: {Drill}, DrillRun: {DrillRun}.",
                options.ServiceGroup, options.Drill, options.DrillRun);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Reprotection cannot start while the drill run is in its current state. Complete the active operation or verify reprotection is available, then try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed initiating drill run reprotection. Verify you have permission to run drill actions in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill run not found. Verify the drill run, drill, and service group exist and you have access.",
        RequestFailedException =>
            "The drill run reprotect request failed. Verify the drill run, drill, and service group, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    private static void ValidatePathSegment(string value, string optionName, ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('/') || value.Contains('\\'))
        {
            validationResult.Errors.Add($"The {optionName} name must be a single non-empty path segment.");
        }
    }

    private static bool IsDrillResourceId(string resourceId, string serviceGroup, string drill)
    {
        try
        {
            var parsed = new ResourceIdentifier(resourceId);
            ResourceIdentifier? drillId = parsed.Parent;
            ResourceIdentifier? serviceGroupId = drillId?.Parent;
            return parsed.ResourceType == DrillTargetResource.ResourceType &&
                drillId?.ResourceType == ResilienceManagementDrillResource.ResourceType &&
                serviceGroupId?.ResourceType == ServiceGroupResourceType &&
                string.Equals(serviceGroupId.Name, serviceGroup, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(drillId.Name, drill, StringComparison.OrdinalIgnoreCase) &&
                Guid.TryParse(parsed.Name, out _);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            return false;
        }
    }

    public sealed record DrillRunReprotectCommandResult(string OperationId, string DrillRun, bool Accepted);
}
