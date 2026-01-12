// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
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

public sealed class SnapshotUpdateCommand(ILogger<SnapshotUpdateCommand> logger, IFileSharesService service)
    : BaseFileSharesCommand<SnapshotUpdateOptions>(logger, service)
{
    private const string CommandTitle = "Update File Share Snapshot";

    public override string Id => "b5c6d7e8-f9a0-4b1c-2d3e-4f5a6b7c8d9e";
    public override string Name => "update";
    public override string Description => "Update properties and metadata of an Azure managed file share snapshot, such as tags or retention policies.";
    public override string Title => CommandTitle;

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
        command.Options.Add(FileSharesOptionDefinitions.Snapshot.Metadata.AsOptional());
    }

    protected override SnapshotUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileShareName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.Snapshot.FileShareName.Name);
        options.SnapshotName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.Snapshot.SnapshotName.Name);
        options.Metadata = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.Snapshot.Metadata.Name);
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
            _logger.LogInformation("Updating snapshot. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}, SnapshotName: {SnapshotName}",
                options.Subscription, options.ResourceGroup, options.FileShareName, options.SnapshotName);

            // Parse metadata if provided
            Dictionary<string, string>? metadata = null;
            if (!string.IsNullOrEmpty(options.Metadata))
            {
                try
                {
                    metadata = JsonSerializer.Deserialize(options.Metadata, FileSharesJsonContext.Default.DictionaryStringString);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse metadata JSON: {Metadata}", options.Metadata);
                }
            }

            var snapshot = await _fileSharesService.PatchSnapshotAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileShareName!,
                options.SnapshotName!,
                metadata,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var result = new SnapshotUpdateCommandResult(snapshot);
            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.SnapshotUpdateCommandResult);

            _logger.LogInformation("Snapshot updated successfully. SnapshotName: {SnapshotName}", options.SnapshotName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update snapshot");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SnapshotUpdateCommandResult([property: JsonPropertyName("snapshot")] FileShareSnapshotInfo Snapshot);
}
