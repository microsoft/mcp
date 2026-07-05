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
    Id = "f6c29a47-8b51-43d0-9e62-1d4a7c8e9b50",
    Name = "get",
    Title = "Get or List Resilience Usage Plan Enrollments",
    Description = """
        Gets enrollments of a resilience usage plan. Provide an enrollment name to get the full details of that
        enrollment (id, name, the associated service group id, provisioning state, and error details). Omit the
        name to list all enrollments of the usage plan, returning only their id and name.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class UsagePlanEnrollmentGetCommand(ILogger<UsagePlanEnrollmentGetCommand> logger, IResilienceManagementService resilienceManagementService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<UsagePlanEnrollmentGetOptions, UsagePlanEnrollmentGetCommand.UsagePlanEnrollmentGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<UsagePlanEnrollmentGetCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, UsagePlanEnrollmentGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            UsagePlanEnrollmentGetCommandResult result;
            if (string.IsNullOrEmpty(options.Name))
            {
                var enrollments = await _resilienceManagementService.ListUsagePlanEnrollmentsAsync(
                    options.ResourceGroup,
                    options.UsagePlan,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new UsagePlanEnrollmentGetCommandResult(Enrollments: enrollments.ToList());
            }
            else
            {
                var enrollment = await _resilienceManagementService.GetUsagePlanEnrollmentAsync(
                    options.ResourceGroup,
                    options.UsagePlan,
                    options.Name,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new UsagePlanEnrollmentGetCommandResult(Enrollment: enrollment);
            }

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.UsagePlanEnrollmentGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting usage plan enrollment(s). ResourceGroup: {ResourceGroup}, UsagePlan: {UsagePlan}, Name: {Name}, Subscription: {Subscription}.",
                options.ResourceGroup, options.UsagePlan, options.Name, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Usage plan enrollment not found. Verify the enrollment name, usage plan, resource group, subscription, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the usage plan enrollment. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Usage plan enrollment not found. Verify the enrollment and usage plan exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record UsagePlanEnrollmentGetCommandResult(List<ResourceSummary>? Enrollments = null, UsagePlanEnrollmentInfo? Enrollment = null);
}
