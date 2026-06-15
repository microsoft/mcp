// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.ExpansionJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ExpansionJob;

[CommandMetadata(
    Id = "b2c3d4e5-f6a7-8901-bcde-f12345678901",
    Name = "get",
    Title = "Get Azure Managed Lustre Expansion Job",
    Description = """
        Gets the details of expansion jobs for an Azure Managed Lustre filesystem. Use this to retrieve the status, progress, and configuration of expansion operations that resize the filesystem storage capacity. If expansion-job-name is provided, returns details of a specific job; otherwise returns all expansion jobs for the filesystem.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        Optional options:
        - expansion-job-name: The name of a specific expansion job (if omitted, all jobs are returned)
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ExpansionJobGetCommand(IManagedLustreService service, ILogger<ExpansionJobGetCommand> logger)
    : BaseManagedLustreCommand<ExpansionJobGetOptions>(logger)
{

    private readonly IManagedLustreService _service = service;
    private new readonly ILogger<ExpansionJobGetCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(ManagedLustreOptionDefinitions.FileSystemNameOption);
        command.Options.Add(ManagedLustreOptionDefinitions.ExpansionJobNameOption);
    }

    protected override ExpansionJobGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileSystemName ??= parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.FileSystemNameOption.Name);
        options.JobName ??= parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.ExpansionJobNameOption.Name);
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
            if (!string.IsNullOrWhiteSpace(options.JobName))
            {
                var result = await _service.GetExpansionJobAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.FileSystemName!,
                    options.JobName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(result), ManagedLustreJsonContext.Default.ExpansionJobGetResult);
            }
            else
            {
                var results = await _service.ListExpansionJobsAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.FileSystemName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(results ?? []), ManagedLustreJsonContext.Default.ExpansionJobListResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expansion job {JobName} for AMLFS filesystem {FileSystemName}.", options.JobName, options.FileSystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ExpansionJobGetResult(Models.ExpansionJob Job);
    public record ExpansionJobListResult(List<Models.ExpansionJob> Jobs);
}
