// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.SnapshotPolicy;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.SnapshotPolicy;

public sealed class SnapshotPolicyCreateCommand(ILogger<SnapshotPolicyCreateCommand> logger) : SubscriptionCommand<SnapshotPolicyCreateOptions>()
{
    private const string CommandTitle = "Create NetApp Files Snapshot Policy";

    private readonly ILogger<SnapshotPolicyCreateCommand> _logger = logger;

    public override string Id => "d4f8a2c6-6e3b-4d9f-b7a5-e1c2d3f4a5b6";

    public override string Name => "create";

    public override string Description =>
        """
        Creates an Azure NetApp Files snapshot policy in a specified account and resource group, and returns the created snapshot policy details including name, location, resource group, provisioning state, enabled state, and schedule configuration (hourly, daily, weekly, monthly). Requires account name, snapshot policy name, resource group, location, and subscription. Optionally configure hourly, daily, weekly, and monthly snapshot schedules.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(NetAppFilesOptionDefinitions.Account.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.SnapshotPolicy.AsRequired());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Location);
        command.Options.Add(NetAppFilesOptionDefinitions.HourlyScheduleMinute);
        command.Options.Add(NetAppFilesOptionDefinitions.HourlyScheduleSnapshotsToKeep);
        command.Options.Add(NetAppFilesOptionDefinitions.DailyScheduleHour);
        command.Options.Add(NetAppFilesOptionDefinitions.DailyScheduleMinute);
        command.Options.Add(NetAppFilesOptionDefinitions.DailyScheduleSnapshotsToKeep);
        command.Options.Add(NetAppFilesOptionDefinitions.WeeklyScheduleDay);
        command.Options.Add(NetAppFilesOptionDefinitions.WeeklyScheduleSnapshotsToKeep);
        command.Options.Add(NetAppFilesOptionDefinitions.MonthlyScheduleDaysOfMonth);
        command.Options.Add(NetAppFilesOptionDefinitions.MonthlyScheduleSnapshotsToKeep);
    }

    protected override SnapshotPolicyCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.SnapshotPolicy = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.SnapshotPolicy.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Location = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Location.Name);
        options.HourlyScheduleMinute = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.HourlyScheduleMinute.Name);
        options.HourlyScheduleSnapshotsToKeep = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.HourlyScheduleSnapshotsToKeep.Name);
        options.DailyScheduleHour = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.DailyScheduleHour.Name);
        options.DailyScheduleMinute = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.DailyScheduleMinute.Name);
        options.DailyScheduleSnapshotsToKeep = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.DailyScheduleSnapshotsToKeep.Name);
        options.WeeklyScheduleDay = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.WeeklyScheduleDay.Name);
        options.WeeklyScheduleSnapshotsToKeep = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.WeeklyScheduleSnapshotsToKeep.Name);
        options.MonthlyScheduleDaysOfMonth = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.MonthlyScheduleDaysOfMonth.Name);
        options.MonthlyScheduleSnapshotsToKeep = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.MonthlyScheduleSnapshotsToKeep.Name);
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
            var netAppFilesService = context.GetService<INetAppFilesService>();

            var snapshotPolicy = await netAppFilesService.CreateSnapshotPolicy(
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
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A snapshot policy with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the snapshot policy. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Account or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record SnapshotPolicyCreateCommandResult([property: JsonPropertyName("snapshotPolicy")] SnapshotPolicyCreateResult SnapshotPolicy);
}
