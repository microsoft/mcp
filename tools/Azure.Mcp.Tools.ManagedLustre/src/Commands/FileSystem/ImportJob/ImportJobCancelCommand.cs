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

public sealed class ImportJobCancelCommand(ILogger<ImportJobCancelCommand> logger)
    : BaseManagedLustreCommand<ImportJobCancelOptions>(logger)
{
    private const string CommandTitle = "Cancel Azure Managed Lustre Import Job";

    private new readonly ILogger<ImportJobCancelCommand> _logger = logger;

    public override string Id => "d3h5e7g9-1f4a-6d8e-0g2c-4f6a8d0f2e4g";

    public override string Name => "cancel";

    public override string Description =>
        """
        Cancels a running import job for an Azure Managed Lustre filesystem. This stops the import operation and prevents further processing. The job cannot be resumed after cancellation.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - job-name: Name of the import job to cancel
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

    protected override ImportJobCancelOptions BindOptions(ParseResult parseResult)
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

            var cancelledJob = await svc.CancelImportJobAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileSystemName!,
                options.JobName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(options.JobName!, cancelledJob.Properties?.AdminStatus ?? "Unknown"), ManagedLustreJsonContext.Default.ImportJobCancelResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling import job {JobName} for AMLFS filesystem {FileSystem}. Options: {@Options}",
                options.JobName, options.FileSystemName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ImportJobCancelResult(string JobName, string Status);
}
