// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.UsagePlans;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans;

[CommandMetadata(
    Id = "c4f1a8d2-6e93-4b07-95a8-1f2c7d0b3e64",
    Name = "create",
    Title = "Create or Update Resilience Usage Plan",
    Description = """
        Creates or updates a resilience usage plan in the specified resource group with the given plan type,
        and returns the usage plan information including id, name, resource type, location, tags, plan type,
        and provisioning state. If the usage plan already exists, its properties are updated.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class UsagePlanCreateCommand(ILogger<UsagePlanCreateCommand> logger, IResilienceManagementService resilienceManagementService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<UsagePlanCreateOptions, UsagePlanCreateCommand.UsagePlanCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<UsagePlanCreateCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(UsagePlanCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.UsagePlan.Length is < 3 or > 24 || !options.UsagePlan.All(IsValidUsagePlanNameCharacter))
        {
            validationResult.Errors.Add("The usage plan name must be 3 to 24 characters and contain only ASCII letters, numbers, or hyphens.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, UsagePlanCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var usagePlan = await _resilienceManagementService.CreateUsagePlanAsync(
                options.ResourceGroup,
                options.UsagePlan,
                options.PlanType,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new UsagePlanCreateCommandResult(usagePlan),
                ResilienceManagementJsonContext.Default.UsagePlanCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating or updating usage plan. ResourceGroup: {ResourceGroup}, UsagePlan: {UsagePlan}, PlanType: {PlanType}, Subscription: {Subscription}.",
                options.ResourceGroup, options.UsagePlan, options.PlanType, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static bool IsValidUsagePlanNameCharacter(char character) =>
        character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-';

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The usage plan could not be created or updated because it conflicts with the current resource state.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed creating or updating the usage plan. Verify you have the required permissions.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource group not found. Verify the resource group exists and you have access.",
        RequestFailedException =>
            "The usage plan request failed. Verify the request parameters and try again.",
        _ => base.GetErrorMessage(ex)
    };

    public record UsagePlanCreateCommandResult(UsagePlanInfo UsagePlan);
}
