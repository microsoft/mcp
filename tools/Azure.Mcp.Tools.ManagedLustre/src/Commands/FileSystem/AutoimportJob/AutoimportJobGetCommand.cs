// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoimportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoimportJob;

[CommandMetadata(
    Id = "b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e",
    Name = "get",
    Title = "Get Azure Managed Lustre Autoimport Job",
    Description = """
        Gets the details of auto import jobs for an Azure Managed Lustre filesystem. Use this to retrieve the status, configuration, and progress information of autoimport operations that sync data from the linked blob storage container to the Lustre filesystem. If job-name is provided, returns details of a specific job; otherwise returns all jobs for the filesystem.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        Optional options:
        - job-name: The name of a specific autoimport job (if omitted, all jobs are returned)
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AutoimportJobGetCommand(IManagedLustreService service, ILogger<AutoimportJobGetCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AutoimportJobGetOptions, AutoimportJobGetCommand.AutoimportJobGetResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<AutoimportJobGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AutoimportJobGetOptions options, CancellationToken cancellationToken)
    {
        try
        {

            if (!string.IsNullOrWhiteSpace(options.JobName))
            {
                // Get specific job
                var result = await _service.GetAutoimportJobAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.FilesystemName,
                    options.JobName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(result, null), ManagedLustreJsonContext.Default.AutoimportJobGetResult);
            }
            else
            {
                // List all jobs
                var results = await _service.ListAutoimportJobsAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.FilesystemName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(null, results ?? []), ManagedLustreJsonContext.Default.AutoimportJobGetResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting autoimport job {JobName} for AMLFS filesystem {FileSystemName}.", options.JobName, options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record AutoimportJobGetResult(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Models.AutoimportJob? Job,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] List<Models.AutoimportJob>? Jobs);
}
