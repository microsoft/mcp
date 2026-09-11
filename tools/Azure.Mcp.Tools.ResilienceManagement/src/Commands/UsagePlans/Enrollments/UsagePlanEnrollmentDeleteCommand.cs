// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans;
using Azure.Mcp.Tools.ResilienceManagement.Options.UsagePlans.Enrollments;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans.Enrollments;

[CommandMetadata(
    Id = "88aefef8-13ca-4e02-907f-ad6fbf23a28e",
    Name = "delete",
    Title = "Delete Resilience Usage Plan Enrollment",
    Description = "Deletes a named enrollment from a resilience usage plan to remove a service group association while keeping the parent plan. Deletes only the enrollment and reports whether it existed. Use this to unenroll a service group, not to delete the entire usage plan resource.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class UsagePlanEnrollmentDeleteCommand(
    ILogger<UsagePlanEnrollmentDeleteCommand> logger,
    IResilienceManagementService resilienceManagementService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<UsagePlanEnrollmentDeleteOptions, UsagePlanEnrollmentDeleteCommand.UsagePlanEnrollmentDeleteCommandResult>(subscriptionResolver)
{
    private readonly ILogger<UsagePlanEnrollmentDeleteCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(UsagePlanEnrollmentDeleteOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        UsagePlanResourceNameValidator.Validate(options.UsagePlan, "usage plan", validationResult);
        UsagePlanResourceNameValidator.Validate(options.Enrollment, "enrollment", validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        UsagePlanEnrollmentDeleteOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            bool deleted = await _resilienceManagementService.DeleteUsagePlanEnrollmentAsync(
                options.ResourceGroup,
                options.UsagePlan,
                options.Enrollment,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new UsagePlanEnrollmentDeleteCommandResult(deleted, options.UsagePlan, options.Enrollment),
                ResilienceManagementJsonContext.Default.UsagePlanEnrollmentDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error deleting usage plan enrollment. ResourceGroup: {ResourceGroup}, UsagePlan: {UsagePlan}, Enrollment: {Enrollment}, Subscription: {Subscription}.",
                options.ResourceGroup,
                options.UsagePlan,
                options.Enrollment,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        TimeoutException => "The usage plan enrollment delete request timed out. Check whether the enrollment still exists before trying again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed deleting the usage plan enrollment. Verify you have permission to delete enrollments in the resource group.",
        RequestFailedException =>
            "The usage plan enrollment delete request failed. Verify the usage plan, enrollment, resource group, subscription, and request parameters, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex is TimeoutException
        ? HttpStatusCode.GatewayTimeout
        : base.GetStatusCode(ex);

    public sealed record UsagePlanEnrollmentDeleteCommandResult(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] bool Deleted,
        string UsagePlan,
        string Enrollment);
}
