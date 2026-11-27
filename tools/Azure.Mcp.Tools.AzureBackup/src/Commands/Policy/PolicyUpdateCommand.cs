// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Policy;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Policy;

[CommandMetadata(
    Id = "a3f7d2c1-9e84-4b6a-8d3c-5f1e7a2b9c04",
    Name = "update",
    Title = "Update Backup Policy",
    Description = "Updates an existing RSV backup policy, preserving omitted settings. VM policies support Daily/Weekly schedules, Hourly on existing Enhanced policies, long-term retention, instant recovery, snapshot consistency, archive tiering and tags. Standard/Enhanced subtype changes are rejected. SQL, SAP HANA, and file share policies support only schedule-time and daily-retention-days; VM-only flags are rejected. The named policy must already exist in the vault.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class PolicyUpdateCommand(ILogger<PolicyUpdateCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<PolicyUpdateOptions, PolicyUpdateCommand.PolicyUpdateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<PolicyUpdateCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, PolicyUpdateOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);

        var validation = Services.Policy.PolicyUpdateValidator.Validate(options);
        if (!validation.IsValid)
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = string.Join(" ", validation.Issues.Select(i => $"[{i.Flag}] {i.Message}"));
            return context.Response;
        }

        try
        {
            var request = Services.Policy.PolicyUpdateRequest.FromOptions(options);

            var result = await _azureBackupService.UpdatePolicyAsync(
                request,
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.VaultType,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.PolicyUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating policy. Policy: {Policy}, Vault: {Vault}",
                options.Policy, options.Vault);
            HandleAzureBackupException(context, ex, "policy update", GetSuppliedValueOptions(options), AzureBackupErrorGuidance.VmPolicyLimitsHint);
        }

        return context.Response;
    }

    internal static IReadOnlyList<string> GetSuppliedValueOptions(PolicyUpdateOptions o) => AzureBackupErrorGuidance.Supplied(
        ("--schedule-time", o.ScheduleTime is not null),
        ("--daily-retention-days", o.DailyRetentionDays is not null),
        ("--time-zone", o.TimeZone is not null),
        ("--schedule-frequency", o.ScheduleFrequency is not null),
        ("--schedule-times", o.ScheduleTimes is not null),
        ("--schedule-days-of-week", o.ScheduleDaysOfWeek is not null),
        ("--weekly-retention-weeks", o.WeeklyRetentionWeeks != 0),
        ("--weekly-retention-days-of-week", o.WeeklyRetentionDaysOfWeek is not null),
        ("--monthly-retention-months", o.MonthlyRetentionMonths != 0),
        ("--monthly-retention-week-of-month", o.MonthlyRetentionWeekOfMonth is not null),
        ("--monthly-retention-days-of-week", o.MonthlyRetentionDaysOfWeek is not null),
        ("--monthly-retention-days-of-month", o.MonthlyRetentionDaysOfMonth is not null),
        ("--yearly-retention-years", o.YearlyRetentionYears != 0),
        ("--yearly-retention-months", o.YearlyRetentionMonths is not null),
        ("--yearly-retention-week-of-month", o.YearlyRetentionWeekOfMonth is not null),
        ("--yearly-retention-days-of-week", o.YearlyRetentionDaysOfWeek is not null),
        ("--yearly-retention-days-of-month", o.YearlyRetentionDaysOfMonth is not null),
        ("--hourly-interval-hours", o.HourlyIntervalHours.HasValue),
        ("--hourly-window-start-time", o.HourlyWindowStartTime is not null),
        ("--hourly-window-duration-hours", o.HourlyWindowDurationHours.HasValue),
        ("--policy-sub-type", o.PolicySubType is not null),
        ("--instant-rp-retention-days", o.InstantRpRetentionDays is not null),
        ("--instant-rp-resource-group", o.InstantRpResourceGroup is not null),
        ("--snapshot-consistency", o.SnapshotConsistency is not null),
        ("--archive-tier-after-days", o.ArchiveTierAfterDays is not null),
        ("--archive-tier-mode", o.ArchiveTierMode is not null),
        ("--smart-tier", o.SmartTier.HasValue),
        ("--policy-tags", o.PolicyTags is not null));

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Policy or vault not found. Verify the policy name, vault name, and resource group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed updating the policy. Verify Azure RBAC and Resource Guard permissions.",
        RequestFailedException => "Azure rejected the policy update. Verify the schedule, retention limits, and vault security restrictions.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public sealed record PolicyUpdateCommandResult(OperationResult Result);
}
