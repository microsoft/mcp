// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.ImportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ImportJob;

[CommandMetadata(
    Id = "c2g4d6f8-0e3a-5c7d-9f1b-3e5a7c9f1d3f",
    Name = "get",
    Title = "Get Azure Managed Lustre Import Job",
    Description = """
        Gets import job details or lists all import jobs for an Azure Managed Lustre filesystem. If job-name is provided, returns details for that specific job. If job-name is omitted, returns a list of all import jobs for the filesystem.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        Optional options:
        - job-name: Name of specific import job to get (omit to list all jobs)
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ImportJobGetCommand(IManagedLustreService service, ILogger<ImportJobGetCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ImportJobGetOptions, ImportJobGetCommand.ImportJobGetResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<ImportJobGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ImportJobGetOptions options, CancellationToken cancellationToken)
    {
        try
        {

            if (!string.IsNullOrWhiteSpace(options.JobName))
            {
                // Get specific job
                var result = await _service.GetImportJobAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.FilesystemName,
                    options.JobName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(result, null), ManagedLustreJsonContext.Default.ImportJobGetResult);
            }
            else
            {
                // List all jobs
                var results = await _service.ListImportJobsAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.FilesystemName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(null, results), ManagedLustreJsonContext.Default.ImportJobGetResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting import job(s) for AMLFS filesystem {FileSystem}.",
                options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ImportJobGetResult(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Models.ImportJob? Job,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] List<Models.ImportJob>? Jobs);
}
