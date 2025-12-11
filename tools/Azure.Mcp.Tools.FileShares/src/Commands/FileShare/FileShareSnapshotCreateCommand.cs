// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

public sealed class FileShareSnapshotCreateCommand(ILogger<FileShareSnapshotCreateCommand> logger)
    : BaseFileSharesCommand<FileShareSnapshotCreateOptions>
{
    private const string CommandTitle = "Create File Share Snapshot";
    private readonly ILogger<FileShareSnapshotCreateCommand> _logger = logger;

    public override string Id => "m8b9c0d1-e2f3-44a5-b6c7-d8e9f0a1b2c3";

    public override string Name => "create";

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

    public override string Description =>
        "Create a snapshot of a file share.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.FileShareName.AsRequired());
    }

    protected override FileShareSnapshotCreateOptions BindOptions(ParseResult parseResult)
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
            var snapshot = await fileSharesService.CreateFileShareSnapshot(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileShareName!,
                options.SnapshotName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(snapshot, FileSharesJsonContext.Default.FileShareSnapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred creating snapshot for file share {FileShareName}.", options.FileShareName);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
