// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;

[CommandMetadata(
    Id = "c9b73e54-126a-4f38-8d7b-5e92ac6410fd",
    Name = "delete",
    Title = "Delete Resilience Drill",
    Description = """
        Deletes a resilience drill from an Azure service group. This permanently removes the drill definition
        and cannot be undone. Use this only when the drill is no longer needed; do not use it to stop a running
        drill execution.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillDeleteCommand(ILogger<DrillDeleteCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillDeleteOptions, DrillDeleteCommand.DrillDeleteCommandResult>
{
    private readonly ILogger<DrillDeleteCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillDeleteOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.ServiceGroup.Length is < 1 or > 90 || !options.ServiceGroup.All(IsValidServiceGroupNameCharacter))
        {
            validationResult.Errors.Add("The service group name must be 1 to 90 characters and contain only ASCII letters, numbers, hyphens, underscores, periods, or parentheses.");
        }

        if (string.IsNullOrWhiteSpace(options.Drill) || options.Drill.Contains('/'))
        {
            validationResult.Errors.Add("The drill name must be a single non-empty path segment.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await _resilienceManagementService.DeleteDrillAsync(
                options.ServiceGroup,
                options.Drill,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DrillDeleteCommandResult($"Resilience drill '{options.Drill}' was successfully deleted from service group '{options.ServiceGroup}'.", true),
                ResilienceManagementJsonContext.Default.DrillDeleteCommandResult);
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            context.Response.Results = ResponseResult.Create(
                new DrillDeleteCommandResult($"Resilience drill '{options.Drill}' is already absent from service group '{options.ServiceGroup}'.", true),
                ResilienceManagementJsonContext.Default.DrillDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting drill. ServiceGroup: {ServiceGroup}, Drill: {Drill}.",
                options.ServiceGroup, options.Drill);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static bool IsValidServiceGroupNameCharacter(char character) =>
        character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_' or '.' or '(' or ')';

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed deleting the drill. Verify you have the required permissions.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The drill cannot be deleted while it is running or locked. Complete or stop the active drill execution and try again.",
        RequestFailedException =>
            "The drill delete request failed. Verify the request parameters and try again.",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DrillDeleteCommandResult(string Message, bool Success);
}
