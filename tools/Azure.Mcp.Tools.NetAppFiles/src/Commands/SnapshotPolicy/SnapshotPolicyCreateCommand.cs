// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.SnapshotPolicy;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.SnapshotPolicy;

[CommandMetadata(
    Id = "61fd7277-a71f-437d-8476-b627a2715b78",
    Name = "create",
    Title = "Create Azure NetApp Files Snapshot Policy",
    Description = "Creates an Azure NetApp Files snapshot policy with hourly, daily, weekly, or monthly retention schedules. Requires an account, snapshot policy name, location, at least one complete schedule, resource group, and subscription. Returns the created policy details.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class SnapshotPolicyCreateCommand(
    ILogger<SnapshotPolicyCreateCommand> logger,
    INetAppFilesSnapshotPolicyService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SnapshotPolicyCreateOptions, SnapshotPolicyCreateCommand.SnapshotPolicyCreateResult>(subscriptionResolver)
{
    private const int MaximumSnapshotsToKeep = 255;
    private static readonly HashSet<string> ValidWeekdays = new(StringComparer.OrdinalIgnoreCase)
    {
        "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
    };

    private readonly ILogger<SnapshotPolicyCreateCommand> _logger = logger;
    private readonly INetAppFilesSnapshotPolicyService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        SnapshotPolicyCreateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var tags = options.Tags is null
                ? null
                : JsonSerializer.Deserialize(options.Tags, NetAppFilesJsonContext.Default.DictionaryStringString);
            var request = new SnapshotPolicyCreateRequest(
                options.Account,
                options.SnapshotPolicy,
                options.Location,
                options.ResourceGroup,
                options.Enabled,
                options.HourlyMinute,
                options.HourlySnapshotsToKeep,
                options.DailyHour,
                options.DailyMinute,
                options.DailySnapshotsToKeep,
                options.WeeklyDay,
                options.WeeklyHour,
                options.WeeklyMinute,
                options.WeeklySnapshotsToKeep,
                options.MonthlyDaysOfMonth,
                options.MonthlyHour,
                options.MonthlyMinute,
                options.MonthlySnapshotsToKeep,
                tags);

            var policy = await _service.CreateSnapshotPolicyAsync(
                request,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new SnapshotPolicyCreateResult(policy),
                NetAppFilesJsonContext.Default.SnapshotPolicyCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating Azure NetApp Files snapshot policy. Account: {Account}, SnapshotPolicy: {SnapshotPolicy}, ResourceGroup: {ResourceGroup}, Location: {Location}, Subscription: {Subscription}",
                options.Account,
                options.SnapshotPolicy,
                options.ResourceGroup,
                options.Location,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(SnapshotPolicyCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!AccountNameValidator.IsValid(options.Account))
        {
            validationResult.Errors.Add(AccountNameValidator.ErrorMessage);
        }

        if (!NetAppFilesChildResourceNameValidator.IsValid(options.SnapshotPolicy))
        {
            validationResult.Errors.Add("--snapshot-policy must be 1-64 characters, start and end with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.");
        }

        if (string.IsNullOrWhiteSpace(options.Location))
        {
            validationResult.Errors.Add("--location cannot be empty or whitespace.");
        }

        ValidateSchedules(options, validationResult);
        ValidateTags(options.Tags, validationResult);
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "The resource group was not found. Verify it exists and you have access.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Conflict =>
            "The Azure NetApp Files snapshot policy could not be created because of a resource conflict. Verify the policy name and account state.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed creating the Azure NetApp Files snapshot policy. Verify that you have the required RBAC permissions.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "The resource group or Azure NetApp Files account was not found.",
        _ => base.GetErrorMessage(ex)
    };

    private static void ValidateSchedules(SnapshotPolicyCreateOptions options, ValidationResult validationResult)
    {
        var hasHourly = options.HourlyMinute.HasValue || options.HourlySnapshotsToKeep.HasValue;
        var hasDaily = options.DailyHour.HasValue || options.DailyMinute.HasValue || options.DailySnapshotsToKeep.HasValue;
        var hasWeekly = options.WeeklyDay is not null || options.WeeklyHour.HasValue || options.WeeklyMinute.HasValue || options.WeeklySnapshotsToKeep.HasValue;
        var hasMonthly = options.MonthlyDaysOfMonth is not null || options.MonthlyHour.HasValue || options.MonthlyMinute.HasValue || options.MonthlySnapshotsToKeep.HasValue;

        if (!hasHourly && !hasDaily && !hasWeekly && !hasMonthly)
        {
            validationResult.Errors.Add("At least one hourly, daily, weekly, or monthly schedule must be provided.");
        }

        if (hasHourly && (!options.HourlyMinute.HasValue || !options.HourlySnapshotsToKeep.HasValue))
        {
            validationResult.Errors.Add("The hourly schedule requires --hourly-minute and --hourly-snapshots-to-keep.");
        }

        if (hasDaily && (!options.DailyHour.HasValue || !options.DailyMinute.HasValue || !options.DailySnapshotsToKeep.HasValue))
        {
            validationResult.Errors.Add("The daily schedule requires --daily-hour, --daily-minute, and --daily-snapshots-to-keep.");
        }

        if (hasWeekly && (string.IsNullOrWhiteSpace(options.WeeklyDay) || !options.WeeklyHour.HasValue || !options.WeeklyMinute.HasValue || !options.WeeklySnapshotsToKeep.HasValue))
        {
            validationResult.Errors.Add("The weekly schedule requires --weekly-day, --weekly-hour, --weekly-minute, and --weekly-snapshots-to-keep.");
        }

        if (hasMonthly && (string.IsNullOrWhiteSpace(options.MonthlyDaysOfMonth) || !options.MonthlyHour.HasValue || !options.MonthlyMinute.HasValue || !options.MonthlySnapshotsToKeep.HasValue))
        {
            validationResult.Errors.Add("The monthly schedule requires --monthly-days-of-month, --monthly-hour, --monthly-minute, and --monthly-snapshots-to-keep.");
        }

        ValidateRange(options.HourlyMinute, 0, 59, "--hourly-minute", validationResult);
        ValidateRange(options.DailyHour, 0, 23, "--daily-hour", validationResult);
        ValidateRange(options.DailyMinute, 0, 59, "--daily-minute", validationResult);
        ValidateRange(options.WeeklyHour, 0, 23, "--weekly-hour", validationResult);
        ValidateRange(options.WeeklyMinute, 0, 59, "--weekly-minute", validationResult);
        ValidateRange(options.MonthlyHour, 0, 23, "--monthly-hour", validationResult);
        ValidateRange(options.MonthlyMinute, 0, 59, "--monthly-minute", validationResult);
        ValidateRange(options.HourlySnapshotsToKeep, 1, MaximumSnapshotsToKeep, "--hourly-snapshots-to-keep", validationResult);
        ValidateRange(options.DailySnapshotsToKeep, 1, MaximumSnapshotsToKeep, "--daily-snapshots-to-keep", validationResult);
        ValidateRange(options.WeeklySnapshotsToKeep, 1, MaximumSnapshotsToKeep, "--weekly-snapshots-to-keep", validationResult);
        ValidateRange(options.MonthlySnapshotsToKeep, 1, MaximumSnapshotsToKeep, "--monthly-snapshots-to-keep", validationResult);

        if (options.WeeklyDay is not null && !IsValidWeekdayList(options.WeeklyDay))
        {
            validationResult.Errors.Add("--weekly-day must be a comma-separated list of English weekday names.");
        }

        if (options.MonthlyDaysOfMonth is not null && !IsValidMonthDayList(options.MonthlyDaysOfMonth))
        {
            validationResult.Errors.Add("--monthly-days-of-month must be a comma-separated list of values from 1 to 31.");
        }
    }

    private static void ValidateTags(string? tags, ValidationResult validationResult)
    {
        if (tags is null)
        {
            return;
        }

        try
        {
            if (JsonSerializer.Deserialize(tags, NetAppFilesJsonContext.Default.DictionaryStringString) is null)
            {
                validationResult.Errors.Add("--tags must be a JSON key-value object.");
            }
        }
        catch (JsonException)
        {
            validationResult.Errors.Add("--tags must be a JSON key-value object with string values.");
        }
    }

    private static void ValidateRange(int? value, int minimum, int maximum, string option, ValidationResult validationResult)
    {
        if (value is int suppliedValue && (suppliedValue < minimum || suppliedValue > maximum))
        {
            validationResult.Errors.Add($"{option} must be between {minimum} and {maximum}.");
        }
    }

    private static bool IsValidWeekdayList(string value) => value
        .Split(',', StringSplitOptions.TrimEntries)
        is { Length: > 0 } weekdays && weekdays.All(day => day.Length > 0 && ValidWeekdays.Contains(day));

    private static bool IsValidMonthDayList(string value) => value
        .Split(',', StringSplitOptions.TrimEntries)
        is { Length: > 0 } days && days.All(day => day.Length > 0 && int.TryParse(day, out var parsedDay) && parsedDay is >= 1 and <= 31);

    public record SnapshotPolicyCreateResult(NetAppFilesSnapshotPolicy SnapshotPolicy);
}
