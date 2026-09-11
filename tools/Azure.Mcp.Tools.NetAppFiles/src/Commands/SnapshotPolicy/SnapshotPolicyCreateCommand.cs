// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    Id = "d4f8a2c6-6e3b-4d9f-b7a5-e1c2d3f4a5b6",
    Name = "create",
    Description =
        """
        Creates an Azure NetApp Files snapshot policy in a specified account and resource group, and returns the created snapshot policy details including name, location, resource group, provisioning state, enabled state, and schedule configuration (hourly, daily, weekly, monthly). Requires account name, snapshot policy name, resource group, location, and subscription. Optionally configure hourly, daily, weekly, and monthly snapshot schedules.
        """,
    Title = "Create NetApp Files Snapshot Policy",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class SnapshotPolicyCreateCommand(ILogger<SnapshotPolicyCreateCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SnapshotPolicyCreateOptions, SnapshotPolicyCreateCommand.SnapshotPolicyCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<SnapshotPolicyCreateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SnapshotPolicyCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            ValidateUnsupportedCreateArguments(options);

            Dictionary<string, string>? tags = null;
            if (!string.IsNullOrEmpty(options.Tags))
            {
                try
                {
                    tags = JsonSerializer.Deserialize(options.Tags, NetAppFilesJsonContext.Default.DictionaryStringString);
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException($"Invalid tags JSON format: {ex.Message}", nameof(options.Tags));
                }
            }

            var snapshotPolicy = await _netAppFilesService.CreateSnapshotPolicy(
                options.Account!,
                options.SnapshotPolicy!,
                options.ResourceGroup!,
                options.Location!,
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
                new(snapshotPolicy),
                NetAppFilesJsonContext.Default.SnapshotPolicyCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating NetApp Files snapshot policy. Account: {Account}, SnapshotPolicy: {SnapshotPolicy}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
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
            $"Authorization failed creating the snapshot policy. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Account or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    private static void ValidateUnsupportedCreateArguments(SnapshotPolicyCreateOptions options)
    {
        if (options.AcquirePolicyToken)
        {
            throw new ArgumentException("The --acquirePolicyToken argument is not supported by this command yet.");
        }

        if (!string.IsNullOrWhiteSpace(options.ChangeReference))
        {
            throw new ArgumentException("The --changeReference argument is not supported by this command yet.");
        }
    }

    public record SnapshotPolicyCreateCommandResult([property: JsonPropertyName("snapshotPolicy")] SnapshotPolicyCreateResult SnapshotPolicy);
}
