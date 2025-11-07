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

public sealed class AutoexportJobCreateCommand(ILogger<AutoexportJobCreateCommand> logger)
    : BaseManagedLustreCommand<AutoexportJobCreateOptions>(logger)
{
    private const string CommandTitle = "Create Azure Managed Lustre Autoexport Job";

    private new readonly ILogger<AutoexportJobCreateCommand> _logger = logger;

    public override string Id => "9f3e7c2a-4b8d-4e5f-a1c6-8d9e2f3b4a5c";

    public override string Name => "create";

    public override string Description =>
        """
        Creates an auto export job for an Azure Managed Lustre filesystem to continuously export modified files to the linked blob storage container. The auto export job syncs changes from the Lustre filesystem to the configured HSM blob container. Use this to keep blob storage updated with changes in the filesystem.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
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
    }

    protected override AutoexportJobCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileSystemName = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.FileSystemNameOption.Name);
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
            var job = await svc.CreateAutoexportJobAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileSystemName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(job), ManagedLustreJsonContext.Default.AutoexportJobCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating autoexport archive job for AMLFS filesystem {FileSystem}. Options: {@Options}",
                options.FileSystemName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AutoexportJobCreateResult(string JobName);
}
