// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Options.Snapshot;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FileShares.Commands.Snapshot;

/// <summary>
/// Deletes a file share snapshot.
/// </summary>
public sealed class SnapshotDeleteCommand(ILogger<SnapshotDeleteCommand> logger, IFileSharesService fileSharesService)
    : BaseFileSharesCommand<SnapshotDeleteOptions>(logger, fileSharesService)
{
    public override string Id => "c7d8e9f0-a1b2-4c3d-4e5f-6a7b8c9d0e1f";
    public override string Name => "delete";
    public override string Description => "Delete a file share snapshot permanently. This operation cannot be undone.";
    public override string Title => "Delete File Share Snapshot";

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
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
        command.Options.Add(FileSharesOptionDefinitions.Snapshot.FileShareName.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.Snapshot.SnapshotName.AsRequired());
    }

    protected override SnapshotDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileShareName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.Snapshot.FileShareName.Name);
        options.SnapshotName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.Snapshot.SnapshotName.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        var options = BindOptions(parseResult);

        try
        {
            _logger.LogInformation(
                "Deleting snapshot {SnapshotName} for file share {FileShareName} in resource group {ResourceGroup}, subscription {Subscription}",
                options.SnapshotName,
                options.FileShareName,
                options.ResourceGroup,
                options.Subscription);

            await _fileSharesService.DeleteSnapshotAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileShareName!,
                options.SnapshotName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new SnapshotDeleteCommandResult(true, options.SnapshotName!),
                FileSharesJsonContext.Default.SnapshotDeleteCommandResult);

            _logger.LogInformation(
                "Successfully deleted snapshot {SnapshotName}",
                options.SnapshotName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting snapshot. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SnapshotDeleteCommandResult(bool Deleted, string SnapshotName);
}
