// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoexportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoexportJob;

[CommandMetadata(
    Id = "9a3b7e2f-4d6c-8a1e-b5f3-2c7d8e9a1b4f",
    Name = "get",
    Title = "Get Azure Managed Lustre Autoexport Job",
    Description = """
        Gets the details of auto export jobs for an Azure Managed Lustre filesystem. Use this to retrieve the status, configuration, and progress information of autoexport operations that sync data from the Lustre filesystem to the linked blob storage container. If job-name is provided, returns details of a specific job; otherwise returns all jobs for the filesystem.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        Optional options:
        - job-name: The name of a specific autoexport job (if omitted, all jobs are returned)
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AutoexportJobGetCommand(IManagedLustreService service, ILogger<AutoexportJobGetCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AutoexportJobGetOptions, AutoexportJobGetCommand.AutoexportJobGetResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<AutoexportJobGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AutoexportJobGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(options.JobName))
            {
                // Get specific job
                var result = await _service.GetAutoexportJobAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.FilesystemName,
                    options.JobName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(result, null), ManagedLustreJsonContext.Default.AutoexportJobGetResult);
            }
            else
            {
                // List all jobs
                var results = await _service.ListAutoexportJobsAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.FilesystemName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(null, results ?? []), ManagedLustreJsonContext.Default.AutoexportJobGetResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting autoexport job {JobName} for AMLFS filesystem {FileSystemName}.", options.JobName, options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record AutoexportJobGetResult(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Models.AutoexportJob? Job,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] List<Models.AutoexportJob>? Jobs);
}
