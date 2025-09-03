// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
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

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };


    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        UseResourceGroup();
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

            context.Response.Results = fileSystems.Count > 0 ? ResponseResult.Create(
                new FileSystemListResult(fileSystems),
                AzureManagedLustreJsonContext.Default.FileSystemListResult) : null;
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

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record FileSystemListResult(List<Models.LustreFileSystem> FileSystems);
}
