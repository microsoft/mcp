// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Policy;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

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
public sealed class PolicyCreateCommand(ILogger<PolicyCreateCommand> logger, IAzureBackupService azureBackupService) : BaseAzureBackupCommand<PolicyCreateOptions>()
{
    private readonly ILogger<PolicyCreateCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureBackupOptionDefinitions.Policy.AsRequired());
        command.Options.Add(AzureBackupOptionDefinitions.WorkloadType.AsRequired());
        command.Options.Add(AzureBackupOptionDefinitions.DailyRetentionDays);
        // Common schedule flags (new in policy create overhaul; bound here, consumed by builders in a later commit).
        command.Options.Add(AzureBackupOptionDefinitions.TimeZone);
        command.Options.Add(AzureBackupOptionDefinitions.ScheduleFrequency);
        command.Options.Add(AzureBackupOptionDefinitions.ScheduleTimes);
        command.Options.Add(AzureBackupOptionDefinitions.ScheduleDaysOfWeek);
        command.Options.Add(AzureBackupOptionDefinitions.HourlyIntervalHours);
        command.Options.Add(AzureBackupOptionDefinitions.HourlyWindowStartTime);
        command.Options.Add(AzureBackupOptionDefinitions.HourlyWindowDurationHours);
        // Retention flags (new in policy create overhaul; bound here, consumed by builders in a later commit).
        command.Options.Add(AzureBackupOptionDefinitions.WeeklyRetentionWeeks);
        command.Options.Add(AzureBackupOptionDefinitions.WeeklyRetentionDaysOfWeek);
        command.Options.Add(AzureBackupOptionDefinitions.MonthlyRetentionMonths);
        command.Options.Add(AzureBackupOptionDefinitions.MonthlyRetentionWeekOfMonth);
        command.Options.Add(AzureBackupOptionDefinitions.MonthlyRetentionDaysOfWeek);
        command.Options.Add(AzureBackupOptionDefinitions.MonthlyRetentionDaysOfMonth);
        command.Options.Add(AzureBackupOptionDefinitions.YearlyRetentionYears);
        command.Options.Add(AzureBackupOptionDefinitions.YearlyRetentionMonths);
        command.Options.Add(AzureBackupOptionDefinitions.YearlyRetentionWeekOfMonth);
        command.Options.Add(AzureBackupOptionDefinitions.YearlyRetentionDaysOfWeek);
        command.Options.Add(AzureBackupOptionDefinitions.YearlyRetentionDaysOfMonth);
        command.Options.Add(AzureBackupOptionDefinitions.ArchiveTierAfterDays);
        command.Options.Add(AzureBackupOptionDefinitions.ArchiveTierMode);
        // RSV-VM only.
        command.Options.Add(AzureBackupOptionDefinitions.PolicySubType);
        command.Options.Add(AzureBackupOptionDefinitions.InstantRpRetentionDays);
        command.Options.Add(AzureBackupOptionDefinitions.InstantRpResourceGroup);
        command.Options.Add(AzureBackupOptionDefinitions.SnapshotConsistency);
        // RSV-VmWorkload (SQL / SAPHANA / SAPASE).
        command.Options.Add(AzureBackupOptionDefinitions.FullScheduleFrequency);
        command.Options.Add(AzureBackupOptionDefinitions.FullScheduleDaysOfWeek);
        command.Options.Add(AzureBackupOptionDefinitions.DifferentialScheduleDaysOfWeek);
        command.Options.Add(AzureBackupOptionDefinitions.DifferentialRetentionDays);
        command.Options.Add(AzureBackupOptionDefinitions.IncrementalScheduleDaysOfWeek);
        command.Options.Add(AzureBackupOptionDefinitions.IncrementalRetentionDays);
        command.Options.Add(AzureBackupOptionDefinitions.LogFrequencyMinutes);
        command.Options.Add(AzureBackupOptionDefinitions.LogRetentionDays);
        command.Options.Add(AzureBackupOptionDefinitions.IsCompression);
        command.Options.Add(AzureBackupOptionDefinitions.IsSqlCompression);
        // Stage 2 expansion.
        command.Options.Add(AzureBackupOptionDefinitions.SmartTier);
        command.Options.Add(AzureBackupOptionDefinitions.EnableSnapshotBackup);
        command.Options.Add(AzureBackupOptionDefinitions.SnapshotInstantRpRetentionDays);
        command.Options.Add(AzureBackupOptionDefinitions.SnapshotInstantRpResourceGroup);
        command.Options.Add(AzureBackupOptionDefinitions.EnableVaultTierCopy);
        command.Options.Add(AzureBackupOptionDefinitions.VaultTierCopyAfterDays);
        command.Options.Add(AzureBackupOptionDefinitions.BackupMode);
        command.Options.Add(AzureBackupOptionDefinitions.PitrRetentionDays);
        command.Options.Add(AzureBackupOptionDefinitions.PolicyTags);
    }

    protected override PolicyCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Policy = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Policy.Name);
        options.WorkloadType = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.WorkloadType.Name);
        options.DailyRetentionDays = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.DailyRetentionDays.Name);
        options.TimeZone = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.TimeZone.Name);
        options.ScheduleFrequency = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.ScheduleFrequency.Name);
        options.ScheduleTimes = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.ScheduleTimes.Name);
        options.ScheduleDaysOfWeek = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.ScheduleDaysOfWeek.Name);
        options.HourlyIntervalHours = parseResult.GetValueOrDefault<int>(AzureBackupOptionDefinitions.HourlyIntervalHours.Name);
        options.HourlyWindowStartTime = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.HourlyWindowStartTime.Name);
        options.HourlyWindowDurationHours = parseResult.GetValueOrDefault<int>(AzureBackupOptionDefinitions.HourlyWindowDurationHours.Name);
        options.WeeklyRetentionWeeks = parseResult.GetValueOrDefault<int>(AzureBackupOptionDefinitions.WeeklyRetentionWeeks.Name);
        options.WeeklyRetentionDaysOfWeek = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.WeeklyRetentionDaysOfWeek.Name);
        options.MonthlyRetentionMonths = parseResult.GetValueOrDefault<int>(AzureBackupOptionDefinitions.MonthlyRetentionMonths.Name);
        options.MonthlyRetentionWeekOfMonth = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.MonthlyRetentionWeekOfMonth.Name);
        options.MonthlyRetentionDaysOfWeek = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.MonthlyRetentionDaysOfWeek.Name);
        options.MonthlyRetentionDaysOfMonth = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.MonthlyRetentionDaysOfMonth.Name);
        options.YearlyRetentionYears = parseResult.GetValueOrDefault<int>(AzureBackupOptionDefinitions.YearlyRetentionYears.Name);
        options.YearlyRetentionMonths = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.YearlyRetentionMonths.Name);
        options.YearlyRetentionWeekOfMonth = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.YearlyRetentionWeekOfMonth.Name);
        options.YearlyRetentionDaysOfWeek = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.YearlyRetentionDaysOfWeek.Name);
        options.YearlyRetentionDaysOfMonth = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.YearlyRetentionDaysOfMonth.Name);
        options.ArchiveTierAfterDays = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.ArchiveTierAfterDays.Name);
        options.ArchiveTierMode = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.ArchiveTierMode.Name);
        options.PolicySubType = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.PolicySubType.Name);
        options.InstantRpRetentionDays = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.InstantRpRetentionDays.Name);
        options.InstantRpResourceGroup = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.InstantRpResourceGroup.Name);
        options.SnapshotConsistency = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.SnapshotConsistency.Name);
        options.FullScheduleFrequency = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.FullScheduleFrequency.Name);
        options.FullScheduleDaysOfWeek = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.FullScheduleDaysOfWeek.Name);
        options.DifferentialScheduleDaysOfWeek = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.DifferentialScheduleDaysOfWeek.Name);
        options.DifferentialRetentionDays = parseResult.GetValueOrDefault<int>(AzureBackupOptionDefinitions.DifferentialRetentionDays.Name);
        options.IncrementalScheduleDaysOfWeek = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.IncrementalScheduleDaysOfWeek.Name);
        options.IncrementalRetentionDays = parseResult.GetValueOrDefault<int>(AzureBackupOptionDefinitions.IncrementalRetentionDays.Name);
        options.LogFrequencyMinutes = parseResult.GetValueOrDefault<int>(AzureBackupOptionDefinitions.LogFrequencyMinutes.Name);
        options.LogRetentionDays = parseResult.GetValueOrDefault<int>(AzureBackupOptionDefinitions.LogRetentionDays.Name);
        options.IsCompression = parseResult.GetValueOrDefault<bool>(AzureBackupOptionDefinitions.IsCompression.Name);
        options.IsSqlCompression = parseResult.GetValueOrDefault<bool>(AzureBackupOptionDefinitions.IsSqlCompression.Name);
        options.SmartTier = parseResult.GetValueOrDefault<bool>(AzureBackupOptionDefinitions.SmartTier.Name);
        options.EnableSnapshotBackup = parseResult.GetValueOrDefault<bool>(AzureBackupOptionDefinitions.EnableSnapshotBackup.Name);
        options.SnapshotInstantRpRetentionDays = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.SnapshotInstantRpRetentionDays.Name);
        options.SnapshotInstantRpResourceGroup = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.SnapshotInstantRpResourceGroup.Name);
        options.EnableVaultTierCopy = parseResult.GetValueOrDefault<bool>(AzureBackupOptionDefinitions.EnableVaultTierCopy.Name);
        options.VaultTierCopyAfterDays = parseResult.GetValueOrDefault<int>(AzureBackupOptionDefinitions.VaultTierCopyAfterDays.Name);
        options.BackupMode = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.BackupMode.Name);
        options.PitrRetentionDays = parseResult.GetValueOrDefault<int>(AzureBackupOptionDefinitions.PitrRetentionDays.Name);
        options.PolicyTags = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.PolicyTags.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

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
                Policy = options.Policy!,
                WorkloadType = options.WorkloadType!,
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
                options.Vault!,
                options.ResourceGroup!,
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

    internal record PolicyCreateCommandResult(OperationResult Result);
}
