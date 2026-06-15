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
    Id = "c3d4e5f6-a7b8-9012-cdef-123456789012",
    Name = "delete",
    Title = "Delete Azure Managed Lustre Expansion Job",
    Description = """
        Deletes an expansion job for an Azure Managed Lustre filesystem. This permanently removes the expansion job record from the filesystem. Use this to clean up completed, failed, or cancelled expansion jobs.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - expansion-job-name: The name of the expansion job to delete
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ExpansionJobDeleteCommand(IManagedLustreService service, ILogger<ExpansionJobDeleteCommand> logger)
    : BaseManagedLustreCommand<ExpansionJobDeleteOptions>(logger)
{

    private readonly IManagedLustreService _service = service;
    private new readonly ILogger<ExpansionJobDeleteCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(ManagedLustreOptionDefinitions.FileSystemNameOption);
        command.Options.Add(ManagedLustreOptionDefinitions.ExpansionJobNameOption.AsRequired());
    }

    protected override ExpansionJobDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileSystemName = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.FileSystemNameOption.Name);
        options.JobName = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.ExpansionJobNameOption.Name);
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
            await _service.DeleteExpansionJobAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileSystemName!,
                options.JobName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(options.JobName!, "Deleted"), ManagedLustreJsonContext.Default.ExpansionJobDeleteResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expansion job {JobName} for AMLFS filesystem {FileSystem}.",
                options.JobName, options.FileSystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ExpansionJobDeleteResult(string JobName, string Status);
}
