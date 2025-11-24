// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoimportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoimportJob;

public sealed class AutoimportJobDeleteCommand(ILogger<AutoimportJobDeleteCommand> logger)
    : BaseManagedLustreCommand<AutoimportJobDeleteOptions>(logger)
{
    private const string CommandTitle = "Delete Azure Managed Lustre Autoimport Job";

    private new readonly ILogger<AutoimportJobDeleteCommand> _logger = logger;

    public override string Id => "0h4i2j3k-5e1c-6h9g-d4f7-9c0e5g7h8d9i";

    public override string Name => "delete";

    public override string Description =>
        """
        Deletes an auto import job for an Azure Managed Lustre filesystem. This permanently removes the job record from the filesystem. Use this to clean up completed, failed, or cancelled autoimport jobs.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - job-name: The name of the autoimport job to delete
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

    protected override AutoimportJobDeleteOptions BindOptions(ParseResult parseResult)
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
            await svc.DeleteAutoimportJobAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileSystemName!,
                options.JobName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new AutoimportJobDeleteResult(options.JobName!, "Deleted"), ManagedLustreJsonContext.Default.AutoimportJobDeleteResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting autoimport archive job {JobName} for AMLFS filesystem {FileSystem}. Options: {@Options}",
                options.JobName, options.FileSystemName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AutoimportJobDeleteResult(string JobName, string Status);
}
