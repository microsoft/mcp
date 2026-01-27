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

public sealed class SnapshotCreateCommand(ILogger<SnapshotCreateCommand> logger, IFileSharesService service)
    : BaseFileSharesCommand<SnapshotCreateOptions>(logger, service)
{
    private const string CommandTitle = "Create File Share Snapshot";

    public override string Id => "f1a2b3c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c";
    public override string Name => "create";
    public override string Description => "Create a snapshot of an Azure managed file share. Snapshots are read-only point-in-time copies used for backup and recovery.";
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

    protected override SnapshotCreateOptions BindOptions(ParseResult parseResult)
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
            _logger.LogInformation("Creating snapshot. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}, SnapshotName: {SnapshotName}",
                options.Subscription, options.ResourceGroup, options.FileShareName, options.SnapshotName);

            // Parse metadata if provided
            Dictionary<string, string>? metadata = null;
            if (!string.IsNullOrEmpty(options.Metadata))
            {
                try
                {
                    metadata = JsonSerializer.Deserialize(options.Metadata, FileSharesJsonContext.Default.DictionaryStringString);
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException($"Invalid metadata JSON format: {ex.Message}", nameof(options.Metadata));
                }
            }

            var snapshot = await _fileSharesService.CreateSnapshotAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileShareName!,
                options.SnapshotName!,
                metadata,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var result = new SnapshotCreateCommandResult(snapshot);
            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.SnapshotCreateCommandResult);

            _logger.LogInformation("Snapshot created successfully. SnapshotName: {SnapshotName}", options.SnapshotName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create snapshot");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SnapshotCreateCommandResult([property: JsonPropertyName("snapshot")] FileShareSnapshotInfo Snapshot);
}
