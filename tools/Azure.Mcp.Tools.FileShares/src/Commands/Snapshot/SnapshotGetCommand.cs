// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Options.Snapshot;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FileShares.Commands.Snapshot;

public sealed class SnapshotGetCommand(ILogger<SnapshotGetCommand> logger, IFileSharesService service)
    : BaseFileSharesCommand<SnapshotGetOptions>(logger, service)
{
    private const string CommandTitle = "Get File Share Snapshot";

    public override string Id => "a3b4c5d6-e7f8-4a9b-0c1d-2e3f4a5b6c7d";
    public override string Name => "get";
    public override string Description => "Get details of a specific file share snapshot or list all snapshots. If --snapshot-name is provided, returns a specific snapshot; otherwise, lists all snapshots for the file share.";
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
        command.Options.Add(FileSharesOptionDefinitions.Snapshot.FileShareName.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.Snapshot.SnapshotName.AsOptional());
    }

    protected override SnapshotGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileShareName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.Snapshot.FileShareName.Name);
        options.SnapshotName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.Snapshot.SnapshotName.Name);
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
            // If snapshot name is provided, get specific snapshot
            if (!string.IsNullOrEmpty(options.SnapshotName))
            {
                _logger.LogInformation("Getting snapshot. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}, SnapshotName: {SnapshotName}",
                    options.Subscription, options.ResourceGroup, options.FileShareName, options.SnapshotName);

                var snapshot = await _fileSharesService.GetSnapshotAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.FileShareName!,
                    options.SnapshotName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var singleResult = new SnapshotGetCommandResult([snapshot]);
                context.Response.Results = ResponseResult.Create(singleResult, FileSharesJsonContext.Default.SnapshotGetCommandResult);

                _logger.LogInformation("Successfully retrieved snapshot. SnapshotName: {SnapshotName}", options.SnapshotName);
            }
            else
            {
                // List all snapshots
                _logger.LogInformation("Listing snapshots. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}",
                    options.Subscription, options.ResourceGroup, options.FileShareName);

                var snapshots = await _fileSharesService.ListSnapshotsAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.FileShareName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var result = new SnapshotGetCommandResult(snapshots ?? []);
                context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.SnapshotGetCommandResult);

                _logger.LogInformation("Successfully listed {Count} snapshots for file share {FileShareName}", snapshots?.Count ?? 0, options.FileShareName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get snapshot(s)");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SnapshotGetCommandResult([property: JsonPropertyName("snapshots")] List<FileShareSnapshotInfo> Snapshots);
}
