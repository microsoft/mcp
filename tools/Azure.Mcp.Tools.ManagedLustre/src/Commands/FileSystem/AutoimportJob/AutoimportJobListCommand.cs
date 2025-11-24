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

public sealed class AutoimportJobListCommand(ILogger<AutoimportJobListCommand> logger)
    : BaseManagedLustreCommand<AutoimportJobListOptions>(logger)
{
    private const string CommandTitle = "List Azure Managed Lustre Autoimport Jobs";

    private new readonly ILogger<AutoimportJobListCommand> _logger = logger;

    public override string Id => "8c5f0e4d-6e3b-7f9c-b4g2-0d9e8f7b3c5g";

    public override string Name => "list";

    public override string Description =>
        """
        Lists all auto import jobs for an Azure Managed Lustre filesystem. Use this to view all autoimport operations that sync data from the linked blob storage container to the Lustre filesystem, including their status and configuration details.
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

    protected override AutoimportJobListOptions BindOptions(ParseResult parseResult)
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
            var results = await svc.ListAutoimportJobsAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileSystemName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new AutoimportJobListResult(results ?? []), ManagedLustreJsonContext.Default.AutoimportJobListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing autoimport archive jobs for AMLFS filesystem {FileSystemName}. Options: {@Options}", options.FileSystemName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record AutoimportJobListResult(List<Models.AutoimportJob> Jobs);
}
