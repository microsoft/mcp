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

public sealed class AutoexportJobListCommand(ILogger<AutoexportJobListCommand> logger)
    : BaseManagedLustreCommand<AutoexportJobListOptions>(logger)
{
    private const string CommandTitle = "List Azure Managed Lustre Autoexport Jobs";

    private new readonly ILogger<AutoexportJobListCommand> _logger = logger;

    public override string Id => "7b4e9f3c-5d2a-6e8b-a3f1-9c8d7e6a2b4f";

    public override string Name => "list";

    public override string Description =>
        """
        Lists all auto export jobs for an Azure Managed Lustre filesystem. Use this to view all autoexport operations that sync data from the Lustre filesystem to the linked blob storage container, including their status and configuration details.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(ManagedLustreOptionDefinitions.FileSystemNameOption);
    }

    protected override AutoexportJobListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileSystemName ??= parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.FileSystemNameOption.Name);
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
            var results = await svc.ListAutoexportJobsAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileSystemName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new AutoexportJobListResult(results ?? []), ManagedLustreJsonContext.Default.AutoexportJobListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing autoexport archive jobs for AMLFS filesystem {FileSystemName}. Options: {@Options}", options.FileSystemName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record AutoexportJobListResult(List<Models.AutoexportJob> Jobs);
}
