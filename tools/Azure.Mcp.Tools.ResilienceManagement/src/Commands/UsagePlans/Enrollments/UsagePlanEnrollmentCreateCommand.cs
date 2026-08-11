// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
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
    Destructive = true,
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

    public override void ValidateOptions(UsagePlanEnrollmentCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        ValidateUsagePlanResourceName(options.UsagePlan, "usage plan", validationResult);
        ValidateUsagePlanResourceName(options.Enrollment, "enrollment", validationResult);

        if (options.ServiceGroup.Length is < 1 or > 90 || !options.ServiceGroup.All(IsValidServiceGroupNameCharacter))
        {
            validationResult.Errors.Add("The service group name must be 1 to 90 characters and contain only ASCII letters, numbers, hyphens, underscores, periods, or parentheses.");
        }
    }

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
                "Error creating or updating usage plan enrollment. ResourceGroup: {ResourceGroup}, UsagePlan: {UsagePlan}, Enrollment: {Enrollment}, ServiceGroup: {ServiceGroup}, Subscription: {Subscription}.",
                options.ResourceGroup, options.UsagePlan, options.Enrollment, options.ServiceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static void ValidateUsagePlanResourceName(string name, string resourceType, ValidationResult validationResult)
    {
        if (name.Length is < 3 or > 24 || !name.All(IsAsciiLetterNumberOrHyphen))
        {
            validationResult.Errors.Add($"The {resourceType} name must be 3 to 24 characters and contain only ASCII letters, numbers, or hyphens.");
        }
    }

    private static bool IsAsciiLetterNumberOrHyphen(char character) =>
        character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-';

    private static bool IsValidServiceGroupNameCharacter(char character) =>
        IsAsciiLetterNumberOrHyphen(character) || character is '_' or '.' or '(' or ')';

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed creating or updating the usage plan enrollment. Verify you have the required permissions.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Usage plan or service group not found. Verify they exist and you have access.",
        RequestFailedException =>
            "The usage plan enrollment request failed. Verify the request parameters and try again.",
        _ => base.GetErrorMessage(ex)
    };

    public record UsagePlanEnrollmentCreateCommandResult(UsagePlanEnrollmentInfo Enrollment);
}
