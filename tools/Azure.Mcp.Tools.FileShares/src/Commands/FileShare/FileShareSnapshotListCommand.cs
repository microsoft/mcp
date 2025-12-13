// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

public sealed class FileShareSnapshotListCommand(ILogger<FileShareSnapshotListCommand> logger)
    : BaseFileSharesCommand<FileShareSnapshotListOptions>
{
    private const string CommandTitle = "List File Share Snapshots";
    private readonly ILogger<FileShareSnapshotListCommand> _logger = logger;

    public override string Id => "k6f7a8b9-c0d1-42e3-f4a5-b6c7d8e9f0a1";

    public override string Name => "list";

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

    public override string Description =>
        "List all snapshots for a file share.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.FileShareName.AsRequired());
    }

    protected override FileShareSnapshotListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileShareName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.FileShareName.Name);
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
            var fileSharesService = context.GetService<IFileSharesService>();
            var snapshots = await fileSharesService.ListFileShareSnapshots(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileShareName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(snapshots ?? []), FileSharesJsonContext.Default.FileShareSnapshotListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing snapshots for file share {FileShareName}.", options.FileShareName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record FileShareSnapshotListCommandResult(List<string> Snapshots);
}
