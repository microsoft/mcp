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

public sealed class AutoimportJobGetCommand(ILogger<AutoimportJobGetCommand> logger)
    : BaseManagedLustreCommand<AutoimportJobGetOptions>(logger)
{
    private const string CommandTitle = "Get Azure Managed Lustre Autoimport Job";

    private new readonly ILogger<AutoimportJobGetCommand> _logger = logger;

    public override string Id => "b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e";

    public override string Name => "get";

    public override string Description =>
        """
        Gets the details of an auto import job for an Azure Managed Lustre filesystem. Use this to retrieve the status, configuration, and progress information of an ongoing or completed autoimport operation that syncs data from the linked blob storage container to the Lustre filesystem.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - job-name: The name of the autoimport job
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
        command.Options.Add(ManagedLustreOptionDefinitions.JobNameOption);
    }

    protected override AutoimportJobGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileSystemName ??= parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.FileSystemNameOption.Name);
        options.JobName ??= parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.JobNameOption.Name);
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
            var result = await svc.GetAutoimportJobAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileSystemName!,
                options.JobName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new AutoimportJobGetResult(result), ManagedLustreJsonContext.Default.AutoimportJobGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting autoimport archive job {JobName} for AMLFS filesystem {FileSystemName}. Options: {@Options}", options.JobName, options.FileSystemName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record AutoimportJobGetResult(Models.AutoimportJob Job);
}
