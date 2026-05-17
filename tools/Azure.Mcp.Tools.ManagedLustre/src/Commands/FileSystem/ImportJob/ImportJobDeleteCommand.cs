// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.ImportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ImportJob;

[CommandMetadata(
    Id = "e4i6f8h0-2g5b-7e9f-1h3d-5g7b9e1g3f5h",
    Name = "delete",
    Title = "Delete Azure Managed Lustre Import Job",
    Description = """
        Deletes an import job for an Azure Managed Lustre filesystem. This removes the job record and history. The job must be completed or cancelled before it can be deleted.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - job-name: Name of the import job to delete
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ImportJobDeleteCommand(IManagedLustreService service, ILogger<ImportJobDeleteCommand> logger)
    : BaseManagedLustreCommand<ImportJobDeleteOptions>(logger)
{

    private readonly IManagedLustreService _service = service;
    private new readonly ILogger<ImportJobDeleteCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(ManagedLustreOptionDefinitions.FileSystemNameOption.AsRequired());
        command.Options.Add(ManagedLustreOptionDefinitions.JobNameOption.AsRequired());
    }

    protected override ImportJobDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileSystemName = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.FileSystemNameOption.Name);
        options.JobName = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.JobNameOption.Name);
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

            await _service.DeleteImportJobAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileSystemName!,
                options.JobName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(options.JobName!), ManagedLustreJsonContext.Default.ImportJobDeleteResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting import job {JobName} for AMLFS filesystem {FileSystem}.",
                options.JobName, options.FileSystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ImportJobDeleteResult(string JobName);
}
