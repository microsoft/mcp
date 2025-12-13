// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

public sealed class FileShareSnapshotGetCommand(ILogger<FileShareSnapshotGetCommand> logger)
    : BaseFileSharesCommand<FileShareSnapshotGetOptions>
{
    private const string CommandTitle = "Get File Share Snapshot";
    private readonly ILogger<FileShareSnapshotGetCommand> _logger = logger;

    public override string Id => "l7a8b9c0-d1e2-43f4-a5b6-c7d8e9f0a1b2";

    public override string Name => "get";

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
        "Get a specific file share snapshot.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.FileShareName.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.FileShareName.AsRequired());
    }

    protected override FileShareSnapshotGetOptions BindOptions(ParseResult parseResult)
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
            var snapshot = await fileSharesService.GetFileShareSnapshot(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileShareName!,
                options.SnapshotName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(snapshot, FileSharesJsonContext.Default.FileShareSnapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred getting snapshot {SnapshotName}.", options.SnapshotName);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
