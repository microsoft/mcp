// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using Azure.Identity;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;

public abstract class BaseGoalAssignmentCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>
    : AuthenticatedCommand<TOptions, TResult>
    where TOptions : class, IGoalAssignmentOptions
{
    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        RecoveryPlanValidation.ValidateServiceGroup(options.ServiceGroup, validationResult);
        if (string.IsNullOrWhiteSpace(options.GoalAssignment) ||
            options.GoalAssignment.Length is < 3 or > 24 ||
            options.GoalAssignment.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-'))
        {
            validationResult.Errors.Add("The goal assignment name must be 3-24 ASCII letters, digits, or hyphens.");
        }
    }

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        context.Response.Results = null;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException { ErrorCode: "RHUBUserErrorServiceGroupMembersLimitExceeded" } =>
            "The service group exceeds the 500-member discovery limit (RHUBUserErrorServiceGroupMembersLimitExceeded). Use a smaller dedicated service group; do not remove unrelated memberships",
        AuthenticationFailedException => "Authentication failed. Sign in to the requested tenant and try again",
        RequestFailedException { Status: 403 } => "Authorization failed. Verify permissions on the service group and goal assignment",
        RequestFailedException { Status: 404 } or KeyNotFoundException => "The goal assignment or service group was not found",
        RequestFailedException { Status: 409 } => "The request conflicts with the current goal assignment state. Inspect the assignment before retrying",
        TimeoutException or OperationCanceledException => "The request timed out or was canceled. Completion is unknown; inspect the assignment before retrying",
        ArgumentException => "Invalid goal assignment request. Verify the parameters and GA resource payload",
        RequestFailedException => "The goal assignment request failed. Verify the parameters and service state",
        _ => "The goal assignment operation failed. Inspect the service logs for details"
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        OperationCanceledException => HttpStatusCode.GatewayTimeout,
        RequestFailedException { Status: < 400 } => HttpStatusCode.InternalServerError,
        _ => base.GetStatusCode(ex)
    };
}
