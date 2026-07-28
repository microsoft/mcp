// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.UsagePlans.Enrollments;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans.Enrollments;

[CommandMetadata(
    Id = "e0a7c3b9-4d12-4f86-9c50-8b2f1a6d7e43",
    Name = "create",
    Title = "Create or Update Resilience Usage Plan Enrollment",
    Description = """
        Creates or updates an enrollment under a resilience usage plan, associating it with the specified
        service group, and returns the enrollment information including id, name, the associated service group
        id, provisioning state, and error details.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class UsagePlanEnrollmentCreateCommand(ILogger<UsagePlanEnrollmentCreateCommand> logger, IResilienceManagementService resilienceManagementService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<UsagePlanEnrollmentCreateOptions, UsagePlanEnrollmentCreateCommand.UsagePlanEnrollmentCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<UsagePlanEnrollmentCreateCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, UsagePlanEnrollmentCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var enrollment = await _resilienceManagementService.CreateUsagePlanEnrollmentAsync(
                options.ResourceGroup,
                options.UsagePlan,
                options.Enrollment,
                options.ServiceGroup,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new UsagePlanEnrollmentCreateCommandResult(enrollment),
                ResilienceManagementJsonContext.Default.UsagePlanEnrollmentCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating usage plan enrollment. ResourceGroup: {ResourceGroup}, UsagePlan: {UsagePlan}, Enrollment: {Enrollment}, ServiceGroup: {ServiceGroup}, Subscription: {Subscription}.",
                options.ResourceGroup, options.UsagePlan, options.Enrollment, options.ServiceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the usage plan enrollment. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Usage plan or service group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record UsagePlanEnrollmentCreateCommandResult(UsagePlanEnrollmentInfo Enrollment);
}
