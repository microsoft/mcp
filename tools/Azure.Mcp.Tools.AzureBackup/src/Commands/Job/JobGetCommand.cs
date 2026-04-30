// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Job;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Job;

/// <summary>
/// Consolidated job command: when --job is supplied returns a single job's details;
/// otherwise lists all jobs in the vault.
/// </summary>
[CommandMetadata(
    Id = "f1291650-8ff2-413c-8001-e4b33f157a3b",
    Name = "get",
    Title = "Get Backup Job",
    Description = """
        Retrieves backup job information. When --job is specified, returns detailed information
        about a single job including operation type, status, start/end times, error codes, and
        datasource details. When omitted, lists all backup jobs in the vault.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class JobGetCommand(ILogger<JobGetCommand> logger, IAzureBackupService azureBackupService) : BaseAzureBackupCommand<JobGetOptions>()
{
    private readonly ILogger<JobGetCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureBackupOptionDefinitions.Job.AsOptional());
    }

    protected override JobGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Job = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Job.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        context.Activity?.AddTag(AzureBackupTelemetryTags.OperationScope, string.IsNullOrEmpty(options.Job) ? "list" : "single");

        try
        {
            if (!string.IsNullOrEmpty(options.Job))
            {
                var job = await _azureBackupService.GetJobAsync(
                    options.Vault!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Job,
                    options.VaultType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new([job]),
                    AzureBackupJsonContext.Default.JobGetCommandResult);
            }
            else
            {
                var jobs = await _azureBackupService.ListJobsAsync(
                    options.Vault!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.VaultType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(jobs),
                    AzureBackupJsonContext.Default.JobGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job(s). Job: {Job}, Vault: {Vault}", options.Job, options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Job not found. Verify the job ID and vault.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record JobGetCommandResult(List<BackupJobInfo> Jobs);
}
