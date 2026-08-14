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
    Id = "bc5e600b-c414-4bce-8b7d-a6021cfd3d23",
    Name = "create",
    Title = "Create Backup Policy",
    Description = "Creates a backup policy for a specified workload type with schedule and retention rules.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class PolicyCreateCommand(ILogger<PolicyCreateCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<PolicyCreateOptions, PolicyCreateCommand.PolicyCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<PolicyCreateCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, PolicyCreateOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultAndWorkloadTags(context.Activity, options.VaultType, options.WorkloadType);

        var validation = Services.Policy.PolicyCreateValidator.Validate(options);
        if (!validation.IsValid)
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = string.Join(" ", validation.Issues.Select(i => $"[{i.Flag}] {i.Message}"));
            return context.Response;
        }

        try
        {
            var request = new Services.Policy.PolicyCreateRequest
            {
                Policy = options.Policy,
                WorkloadType = options.WorkloadType,
                DailyRetentionDays = options.DailyRetentionDays,
                TimeZone = options.TimeZone,
                ScheduleFrequency = options.ScheduleFrequency,
                ScheduleTimes = options.ScheduleTimes,
                ScheduleDaysOfWeek = options.ScheduleDaysOfWeek,
                HourlyIntervalHours = options.HourlyIntervalHours,
                HourlyWindowStartTime = options.HourlyWindowStartTime,
                HourlyWindowDurationHours = options.HourlyWindowDurationHours,
                WeeklyRetentionWeeks = options.WeeklyRetentionWeeks,
                WeeklyRetentionDaysOfWeek = options.WeeklyRetentionDaysOfWeek,
                MonthlyRetentionMonths = options.MonthlyRetentionMonths,
                MonthlyRetentionWeekOfMonth = options.MonthlyRetentionWeekOfMonth,
                MonthlyRetentionDaysOfWeek = options.MonthlyRetentionDaysOfWeek,
                MonthlyRetentionDaysOfMonth = options.MonthlyRetentionDaysOfMonth,
                YearlyRetentionYears = options.YearlyRetentionYears,
                YearlyRetentionMonths = options.YearlyRetentionMonths,
                YearlyRetentionWeekOfMonth = options.YearlyRetentionWeekOfMonth,
                YearlyRetentionDaysOfWeek = options.YearlyRetentionDaysOfWeek,
                YearlyRetentionDaysOfMonth = options.YearlyRetentionDaysOfMonth,
                ArchiveTierAfterDays = options.ArchiveTierAfterDays,
                ArchiveTierMode = options.ArchiveTierMode,
                PolicySubType = options.PolicySubType,
                InstantRpRetentionDays = options.InstantRpRetentionDays,
                InstantRpResourceGroup = options.InstantRpResourceGroup,
                SnapshotConsistency = options.SnapshotConsistency,
                FullScheduleFrequency = options.FullScheduleFrequency,
                FullScheduleDaysOfWeek = options.FullScheduleDaysOfWeek,
                DifferentialScheduleDaysOfWeek = options.DifferentialScheduleDaysOfWeek,
                DifferentialRetentionDays = options.DifferentialRetentionDays,
                IncrementalScheduleDaysOfWeek = options.IncrementalScheduleDaysOfWeek,
                IncrementalRetentionDays = options.IncrementalRetentionDays,
                LogFrequencyMinutes = options.LogFrequencyMinutes,
                LogRetentionDays = options.LogRetentionDays,
                IsCompression = options.IsCompression,
                IsSqlCompression = options.IsSqlCompression,
                SmartTier = options.SmartTier,
                EnableSnapshotBackup = options.EnableSnapshotBackup,
                SnapshotInstantRpRetentionDays = options.SnapshotInstantRpRetentionDays,
                SnapshotInstantRpResourceGroup = options.SnapshotInstantRpResourceGroup,
                EnableVaultTierCopy = options.EnableVaultTierCopy,
                VaultTierCopyAfterDays = options.VaultTierCopyAfterDays,
                BackupMode = options.BackupMode,
                PitrRetentionDays = options.PitrRetentionDays,
                PolicyTags = options.PolicyTags,
            };

            var result = await _azureBackupService.CreatePolicyAsync(
                request,
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.VaultType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.PolicyCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating policy. Policy: {Policy}, Vault: {Vault}, WorkloadType: {WorkloadType}",
                options.Policy, options.Vault, options.WorkloadType);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault not found. Verify the vault name and resource group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A policy with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the policy. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record PolicyCreateCommandResult(OperationResult Result);
}
