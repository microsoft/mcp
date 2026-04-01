// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Job;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Job;

/// <summary>
/// Consolidated job command: when --job is supplied returns a single job's details;
/// otherwise lists all jobs in the vault.
/// </summary>
public sealed class JobGetCommand(ILogger<JobGetCommand> logger) : BaseAzureBackupCommand<JobGetOptions>()
{
    private const string CommandTitle = "Get Backup Job";
    private readonly ILogger<JobGetCommand> _logger = logger;

    public override string Id => "f1291650-8ff2-413c-8001-e4b33f157a3b";
    public override string Name => "get";
    public override string Description =>
        """
        Retrieves backup job information. When --job is specified, returns detailed information
        about a single job including operation type, status, start/end times, error codes, and
        datasource details. When omitted, lists all backup jobs in the vault.
        """;
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

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

        try
        {
            var service = context.GetService<IAzureBackupService>();

            if (!string.IsNullOrEmpty(options.Job))
            {
                var job = await service.GetJobAsync(
                    options.Vault!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Job,
                    options.VaultType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new JobGetCommandResult([job]),
                    AzureBackupJsonContext.Default.JobGetCommandResult);
            }
            else
            {
                var jobs = await service.ListJobsAsync(
                    options.Vault!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.VaultType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new JobGetCommandResult(jobs),
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

    internal record JobGetCommandResult([property: JsonPropertyName("jobs")] List<BackupJobInfo> Jobs);
}
