// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Snapshot;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Snapshot;

public sealed class SnapshotGetCommand(ILogger<SnapshotGetCommand> logger) : SubscriptionCommand<SnapshotGetOptions>()
{
    private const string CommandTitle = "Get NetApp Files Snapshot Details";

    private readonly ILogger<SnapshotGetCommand> _logger = logger;

    public override string Id => "a3c7e1d9-5f2b-4a8c-b6d0-e9f4a2c8d1b5";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves detailed information about Azure NetApp Files snapshots, including snapshot name, location, resource group, provisioning state, and creation time. If a specific snapshot name is not provided, the command will return details for all snapshots in a subscription. Optionally filter by account, capacity pool, and volume.
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
        command.Options.Add(NetAppFilesOptionDefinitions.Account.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Pool.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Volume.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Snapshot.AsOptional());
    }

    protected override SnapshotGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.Pool = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Pool.Name);
        options.Volume = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Volume.Name);
        options.Snapshot = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Snapshot.Name);
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
            var netAppFilesService = context.GetService<INetAppFilesService>();

            var snapshots = await netAppFilesService.GetSnapshotDetails(
                options.Account,
                options.Pool,
                options.Volume,
                options.Snapshot,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(snapshots?.Results ?? [], snapshots?.AreResultsTruncated ?? false),
                NetAppFilesJsonContext.Default.SnapshotGetCommandResult);
        }
        catch (Exception ex)
        {
            if (options.Snapshot is null)
            {
                _logger.LogError(ex, "Error listing NetApp Files snapshot details. Subscription: {Subscription}, Options: {@Options}", options.Subscription, options);
            }
            else
            {
                _logger.LogError(ex, "Error getting NetApp Files snapshot details. Snapshot: {Snapshot}, Subscription: {Subscription}, Options: {@Options}",
                    options.Snapshot, options.Subscription, options);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SnapshotGetCommandResult(List<SnapshotInfo> Snapshots, bool AreResultsTruncated);
}
