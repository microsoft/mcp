// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResilienceManagement.Options.UsagePlans;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans;

[CommandMetadata(
    Id = "7d5ccdb2-6595-49cf-89d7-8a55dc1dc823",
    Name = "delete",
    Title = "Delete Resilience Usage Plan",
    Description = "Deletes a named resilience usage plan from an Azure resource group or removes the usage plan of a service group. Deletes the entire parent plan and reports whether it existed; do not use this to remove only an enrollment or association. Dependent enrollments block plan deletion and are not deleted automatically; list the exact enrollments and obtain explicit confirmation before deleting each separately.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class UsagePlanDeleteCommand(
    ILogger<UsagePlanDeleteCommand> logger,
    IResilienceManagementService resilienceManagementService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<UsagePlanDeleteOptions, UsagePlanDeleteCommand.UsagePlanDeleteCommandResult>(subscriptionResolver)
{
    private readonly ILogger<UsagePlanDeleteCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(UsagePlanDeleteOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        UsagePlanResourceNameValidator.Validate(options.UsagePlan, "usage plan", validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        UsagePlanDeleteOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            bool deleted = await _resilienceManagementService.DeleteUsagePlanAsync(
                options.ResourceGroup,
                options.UsagePlan,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new UsagePlanDeleteCommandResult(deleted, options.UsagePlan),
                ResilienceManagementJsonContext.Default.UsagePlanDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error deleting usage plan. ResourceGroup: {ResourceGroup}, UsagePlan: {UsagePlan}, Subscription: {Subscription}.",
                options.ResourceGroup,
                options.UsagePlan,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        TimeoutException => "The usage plan delete request timed out. Check whether the usage plan still exists before trying again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The usage plan cannot be deleted while dependent enrollments exist. List the enrollments and obtain explicit confirmation before deleting them separately, then try again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed deleting the usage plan. Verify you have permission to delete usage plans in the resource group.",
        RequestFailedException =>
            "The usage plan delete request failed. Verify the usage plan, resource group, subscription, and request parameters, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex is TimeoutException
        ? HttpStatusCode.GatewayTimeout
        : base.GetStatusCode(ex);

    public sealed record UsagePlanDeleteCommandResult(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] bool Deleted,
        string UsagePlan);
}
