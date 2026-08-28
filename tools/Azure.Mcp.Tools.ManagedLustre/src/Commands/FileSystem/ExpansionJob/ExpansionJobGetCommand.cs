// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.ExpansionJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ExpansionJob;

[CommandMetadata(
    Id = "09eeab98-b2d5-4c8a-8bac-89dec00a3afd",
    Name = "get",
    Title = "Get Azure Managed Lustre Expansion Job",
    Description = """
        Gets the details of expansion jobs for an Azure Managed Lustre filesystem. Use this to retrieve the status, progress, and configuration of expansion operations that resize the filesystem storage capacity. If expansion-job-name is provided, returns details of a specific job; otherwise returns all expansion jobs for the filesystem.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ExpansionJobGetCommand(IManagedLustreService service, ILogger<ExpansionJobGetCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ExpansionJobGetOptions, ExpansionJobGetCommand.ExpansionJobGetResult>(subscriptionResolver)
{
    private readonly IManagedLustreService _service = service;
    private readonly ILogger<ExpansionJobGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ExpansionJobGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(options.ExpansionJobName))
            {
                var result = await _service.GetExpansionJobAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.FilesystemName,
                    options.ExpansionJobName,
                    options.Tenant,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(result, null), ManagedLustreJsonContext.Default.ExpansionJobGetResult);
            }
            else
            {
                var results = await _service.ListExpansionJobsAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.FilesystemName,
                    options.Tenant,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(null, results ?? []), ManagedLustreJsonContext.Default.ExpansionJobGetResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expansion job {JobName} for AMLFS filesystem {FileSystemName}.", options.ExpansionJobName, options.FilesystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ExpansionJobGetResult(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Models.ExpansionJob? Job,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] List<Models.ExpansionJob>? Jobs);
}
