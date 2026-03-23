// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Policy;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Policy;

public sealed class PolicyCreateCommand(ILogger<PolicyCreateCommand> logger) : BaseAzureBackupCommand<PolicyCreateOptions>()
{
    private const string CommandTitle = "Create Backup Policy";
    private readonly ILogger<PolicyCreateCommand> _logger = logger;

    public override string Id => "b1a2c3d4-e5f6-7890-abcd-ef12345678a2";
    public override string Name => "create";
    public override string Description => "Creates a backup policy for a specified workload type with schedule and retention rules.";
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new() { Destructive = true, Idempotent = false, OpenWorld = false, ReadOnly = false, LocalRequired = false, Secret = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureBackupOptionDefinitions.Policy);
        command.Options.Add(AzureBackupOptionDefinitions.WorkloadType.AsRequired());
        command.Options.Add(AzureBackupOptionDefinitions.ScheduleFrequency);
        command.Options.Add(AzureBackupOptionDefinitions.ScheduleTime);
        command.Options.Add(AzureBackupOptionDefinitions.DailyRetentionDays);
        command.Options.Add(AzureBackupOptionDefinitions.WeeklyRetentionWeeks);
        command.Options.Add(AzureBackupOptionDefinitions.MonthlyRetentionMonths);
        command.Options.Add(AzureBackupOptionDefinitions.YearlyRetentionYears);
    }

    protected override PolicyCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Policy = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Policy.Name);
        options.WorkloadType = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.WorkloadType.Name);
        options.ScheduleFrequency = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.ScheduleFrequency.Name);
        options.ScheduleTime = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.ScheduleTime.Name);
        options.DailyRetentionDays = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.DailyRetentionDays.Name);
        options.WeeklyRetentionWeeks = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.WeeklyRetentionWeeks.Name);
        options.MonthlyRetentionMonths = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.MonthlyRetentionMonths.Name);
        options.YearlyRetentionYears = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.YearlyRetentionYears.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;
        var options = BindOptions(parseResult);
        try
        {
            var service = context.GetService<IAzureBackupService>();
            var result = await service.CreatePolicyAsync(options.Vault!, options.ResourceGroup!, options.Subscription!, options.Policy!, options.WorkloadType!, options.VaultType, options.ScheduleFrequency, options.ScheduleTime, options.DailyRetentionDays, options.WeeklyRetentionWeeks, options.MonthlyRetentionMonths, options.YearlyRetentionYears, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(new PolicyCreateCommandResult(result), AzureBackupJsonContext.Default.PolicyCreateCommandResult);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error creating policy"); HandleException(context, ex); }
        return context.Response;
    }

    internal record PolicyCreateCommandResult([property: JsonPropertyName("result")] OperationResult Result);
}
