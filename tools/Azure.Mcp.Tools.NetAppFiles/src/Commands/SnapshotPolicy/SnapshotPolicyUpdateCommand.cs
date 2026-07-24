// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Mcp.Core.Commands.Subscription;
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
public sealed class SnapshotPolicyUpdateCommand(ILogger<SnapshotPolicyUpdateCommand> logger, INetAppFilesService netAppFilesService) : SubscriptionCommand<SnapshotPolicyUpdateOptions>()
{
    private readonly ILogger<SnapshotPolicyUpdateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(NetAppFilesOptionDefinitions.Account.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.SnapshotPolicy.AsOptional());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Ids.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Location.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.HourlyScheduleMinute);
        command.Options.Add(NetAppFilesOptionDefinitions.HourlyScheduleSnapshotsToKeep);
        command.Options.Add(NetAppFilesOptionDefinitions.DailyScheduleHour);
        command.Options.Add(NetAppFilesOptionDefinitions.DailyScheduleMinute);
        command.Options.Add(NetAppFilesOptionDefinitions.DailyScheduleSnapshotsToKeep);
        command.Options.Add(NetAppFilesOptionDefinitions.WeeklyScheduleDay);
        command.Options.Add(NetAppFilesOptionDefinitions.WeeklyScheduleHour);
        command.Options.Add(NetAppFilesOptionDefinitions.WeeklyScheduleMinute);
        command.Options.Add(NetAppFilesOptionDefinitions.WeeklyScheduleSnapshotsToKeep);
        command.Options.Add(NetAppFilesOptionDefinitions.MonthlyScheduleDaysOfMonth);
        command.Options.Add(NetAppFilesOptionDefinitions.MonthlyScheduleHour);
        command.Options.Add(NetAppFilesOptionDefinitions.MonthlyScheduleMinute);
        command.Options.Add(NetAppFilesOptionDefinitions.MonthlyScheduleSnapshotsToKeep);
        command.Options.Add(NetAppFilesOptionDefinitions.Enabled.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Tags.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.NoWait.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.AcquirePolicyToken.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ChangeReference.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Add.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Set.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Remove.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ForceString.AsOptional());
    }

    protected override SnapshotPolicyUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.SnapshotPolicy = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.SnapshotPolicy.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Ids = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Ids.Name);
        options.Location = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Location.Name);
        options.HourlyScheduleMinute = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.HourlyScheduleMinute.Name);
        options.HourlyScheduleSnapshotsToKeep = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.HourlyScheduleSnapshotsToKeep.Name);
        options.DailyScheduleHour = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.DailyScheduleHour.Name);
        options.DailyScheduleMinute = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.DailyScheduleMinute.Name);
        options.DailyScheduleSnapshotsToKeep = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.DailyScheduleSnapshotsToKeep.Name);
        options.WeeklyScheduleDay = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.WeeklyScheduleDay.Name);
        options.WeeklyScheduleHour = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.WeeklyScheduleHour.Name);
        options.WeeklyScheduleMinute = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.WeeklyScheduleMinute.Name);
        options.WeeklyScheduleSnapshotsToKeep = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.WeeklyScheduleSnapshotsToKeep.Name);
        options.MonthlyScheduleDaysOfMonth = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.MonthlyScheduleDaysOfMonth.Name);
        options.MonthlyScheduleHour = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.MonthlyScheduleHour.Name);
        options.MonthlyScheduleMinute = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.MonthlyScheduleMinute.Name);
        options.MonthlyScheduleSnapshotsToKeep = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.MonthlyScheduleSnapshotsToKeep.Name);
        options.Enabled = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.Enabled.Name);
        options.Tags = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Tags.Name);
        options.NoWait = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.NoWait.Name);
        options.AcquirePolicyToken = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.AcquirePolicyToken.Name);
        options.ChangeReference = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ChangeReference.Name);
        options.Add = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Add.Name);
        options.Set = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Set.Name);
        options.Remove = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Remove.Name);
        options.ForceString = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.ForceString.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

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

    internal record SnapshotPolicyUpdateCommandResult([property: JsonPropertyName("snapshotPolicy")] SnapshotPolicyCreateResult SnapshotPolicy);
}
