// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;

public sealed class FileSystemListCommand(ILogger<FileSystemListCommand> logger)
    : BaseAzureManagedLustreCommand<FileSystemListOptions>(logger)
{
    private const string CommandTitle = "List Azure Managed Lustre File Systems";

    public override string Name => "list";

    public override string Description =>
        """
        Lists Azure Managed Lustre (AMLFS) file systems in a subscription or optional resource group including provisioning state, MGS address, tier, capacity (TiB), blob integration container, and maintenance window details. Use to inventory Azure Managed Lustre filesystems and to check their properties.
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
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override FileSystemListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var svc = context.GetService<IAzureManagedLustreService>();
            var fileSystems = await svc.ListFileSystemsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(fileSystems ?? []), AzureManagedLustreJsonContext.Default.FileSystemListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing AMLFS file systems. ResourceGroup: {ResourceGroup} Options: {@Options}",
                options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record FileSystemListResult(List<Models.LustreFileSystem> FileSystems);
}
