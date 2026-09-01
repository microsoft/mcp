// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.SnapshotPolicy;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.SnapshotPolicy;

[CommandMetadata(
    Id = "e5a9b3d7-7f4c-4e8a-c6b2-f3d1e4a5c7b8",
    Name = "update",
    Description =
        """
        Updates an existing Azure NetApp Files snapshot policy in a specified account and resource group, and returns the updated snapshot policy details including name, location, resource group, provisioning state, enabled state, and schedule configuration (hourly, daily, weekly, monthly). Supports updating hourly, daily, weekly, and monthly snapshot schedules. Requires account name, snapshot policy name, resource group, location, and subscription.
        """,
    Title = "Update NetApp Files Snapshot Policy",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class SnapshotPolicyUpdateCommand(ILogger<SnapshotPolicyUpdateCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SnapshotPolicyUpdateOptions, SnapshotPolicyUpdateCommand.SnapshotPolicyUpdateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<SnapshotPolicyUpdateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SnapshotPolicyUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            ResolveResourceIdArguments(options);
            ValidateUnsupportedUpdateArguments(options);

            Dictionary<string, string>? tags = null;
            if (!string.IsNullOrEmpty(options.Tags))
            {
                try
                {
                    tags = System.Text.Json.JsonSerializer.Deserialize(options.Tags, NetAppFilesJsonContext.Default.DictionaryStringString);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    throw new ArgumentException($"Invalid tags JSON format: {ex.Message}", nameof(options.Tags));
                }
            }

            var snapshotPolicy = await _netAppFilesService.UpdateSnapshotPolicy(
                options.Account!,
                options.SnapshotPolicy!,
                options.ResourceGroup!,
                options.Location,
                options.Subscription!,
                options.HourlyScheduleMinute,
                options.HourlyScheduleSnapshotsToKeep,
                options.DailyScheduleHour,
                options.DailyScheduleMinute,
                options.DailyScheduleSnapshotsToKeep,
                options.WeeklyScheduleDay,
                options.WeeklyScheduleSnapshotsToKeep,
                options.MonthlyScheduleDaysOfMonth,
                options.MonthlyScheduleSnapshotsToKeep,
                options.Enabled,
                options.WeeklyScheduleHour,
                options.WeeklyScheduleMinute,
                options.MonthlyScheduleHour,
                options.MonthlyScheduleMinute,
                tags,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new SnapshotPolicyUpdateCommandResult(snapshotPolicy),
                NetAppFilesJsonContext.Default.SnapshotPolicyUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating NetApp Files snapshot policy. Account: {Account}, SnapshotPolicy: {SnapshotPolicy}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Account, options.SnapshotPolicy, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A snapshot policy with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed updating the snapshot policy. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Snapshot policy, account, or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    private static void ResolveResourceIdArguments(SnapshotPolicyUpdateOptions options)
    {
        if (options.Ids is { Length: > 0 })
        {
            if (options.Ids.Length > 1)
            {
                throw new ArgumentException("Only a single resource ID is supported for snapshot policy update operations.", nameof(options.Ids));
            }

            var resourceIdentifier = new ResourceIdentifier(options.Ids[0]);
            options.SnapshotPolicy = resourceIdentifier.Name;
            options.ResourceGroup = resourceIdentifier.ResourceGroupName;
            options.Subscription = resourceIdentifier.SubscriptionId;

            var accountSegment = resourceIdentifier.Parent?.Parent?.Name ?? resourceIdentifier.Parent?.Name;
            if (!string.IsNullOrWhiteSpace(accountSegment))
            {
                options.Account = accountSegment;
            }
        }

        if (string.IsNullOrWhiteSpace(options.Account) || string.IsNullOrWhiteSpace(options.SnapshotPolicy) || string.IsNullOrWhiteSpace(options.ResourceGroup))
        {
            throw new ArgumentException("Either --ids or all of --account, --snapshotPolicy, and --resource-group must be provided for snapshot policy update.");
        }
    }

    private static void ValidateUnsupportedUpdateArguments(SnapshotPolicyUpdateOptions options)
    {
        if (options.NoWait)
        {
            throw new ArgumentException("The --no-wait argument is not supported by this command yet.");
        }

        if (options.AcquirePolicyToken)
        {
            throw new ArgumentException("The --acquirePolicyToken argument is not supported by this command yet.");
        }

        if (!string.IsNullOrWhiteSpace(options.ChangeReference))
        {
            throw new ArgumentException("The --changeReference argument is not supported by this command yet.");
        }

        if (options.Add is { Length: > 0 })
        {
            throw new ArgumentException("The --add argument is not supported by this command yet.");
        }

        if (options.Set is { Length: > 0 })
        {
            throw new ArgumentException("The --set argument is not supported by this command yet.");
        }

        if (options.Remove is { Length: > 0 })
        {
            throw new ArgumentException("The --remove argument is not supported by this command yet.");
        }

        if (options.ForceString)
        {
            throw new ArgumentException("The --force-string argument is not supported by this command yet.");
        }
    }

    public record SnapshotPolicyUpdateCommandResult([property: JsonPropertyName("snapshotPolicy")] SnapshotPolicyCreateResult SnapshotPolicy);
}
