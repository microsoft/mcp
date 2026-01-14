// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.ImportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ImportJob;

public sealed class ImportJobDeleteCommand(ILogger<ImportJobDeleteCommand> logger)
    : BaseManagedLustreCommand<ImportJobDeleteOptions>(logger)
{
    private const string CommandTitle = "Delete Azure Managed Lustre Import Job";

    private new readonly ILogger<ImportJobDeleteCommand> _logger = logger;

    public override string Id => "e4i6f8h0-2g5b-7e9f-1h3d-5g7b9e1g3f5h";

    public override string Name => "delete";

    public override string Description =>
        """
        Deletes an import job for an Azure Managed Lustre filesystem. This removes the job record and history. The job must be completed or cancelled before it can be deleted.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - job-name: Name of the import job to delete
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

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
            var svc = context.GetService<IManagedLustreService>();

            await svc.DeleteImportJobAsync(
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
            _logger.LogError(ex, "Error deleting import job {JobName} for AMLFS filesystem {FileSystem}. Options: {@Options}",
                options.JobName, options.FileSystemName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ImportJobDeleteResult(string JobName);
}
