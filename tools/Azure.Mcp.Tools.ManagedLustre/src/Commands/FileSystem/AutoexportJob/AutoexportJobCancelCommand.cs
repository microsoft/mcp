// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoexportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoexportJob;

public sealed class AutoexportJobCancelCommand(ILogger<AutoexportJobCancelCommand> logger)
    : BaseManagedLustreCommand<AutoexportJobCancelOptions>(logger)
{
    private const string CommandTitle = "Cancel Azure Managed Lustre Autoexport Job";

    private new readonly ILogger<AutoexportJobCancelCommand> _logger = logger;

    public override string Id => "8e2f6d1b-3c9a-4f7e-b2d5-7a8c3e4f5b6d";

    public override string Name => "cancel";

    public override string Description =>
        """
        Cancels a running auto export job for an Azure Managed Lustre filesystem. This stops the ongoing sync operation from the Lustre filesystem to the linked blob storage container. Use this to terminate an autoexport job that is in progress.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - job-name: The name of the autoexport job to cancel
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
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
        command.Options.Add(ManagedLustreOptionDefinitions.FileSystemNameOption);
        command.Options.Add(ManagedLustreOptionDefinitions.JobNameOption);
    }

    protected override AutoexportJobCancelOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileSystemName = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.FileSystemNameOption.Name);
        options.JobName = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.JobNameOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var svc = context.GetService<IManagedLustreService>();
            await svc.CancelAutoexportJobAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileSystemName!,
                options.JobName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new AutoexportJobCancelResult(options.JobName!, "Cancelled"), ManagedLustreJsonContext.Default.AutoexportJobCancelResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling autoexport archive job {JobName} for AMLFS filesystem {FileSystem}. Options: {@Options}",
                options.JobName, options.FileSystemName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AutoexportJobCancelResult(string JobName, string Status);
}
