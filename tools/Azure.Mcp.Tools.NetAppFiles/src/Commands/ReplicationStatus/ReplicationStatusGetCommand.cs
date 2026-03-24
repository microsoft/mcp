// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.ReplicationStatus;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.ReplicationStatus;

public sealed class ReplicationStatusGetCommand(ILogger<ReplicationStatusGetCommand> logger) : SubscriptionCommand<ReplicationStatusGetOptions>()
{
    private const string CommandTitle = "Get NetApp Files Volume Replication Status";

    private readonly ILogger<ReplicationStatusGetCommand> _logger = logger;

    public override string Id => "c9e5f1a3-7b4d-4c8e-a2d6-f0b3e8c1d5a7";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves replication status information for Azure NetApp Files volumes, including endpoint type, replication schedule, remote volume resource ID, remote volume region, and replication ID. Only volumes with cross-region replication configured are returned. If a specific volume name is not provided, the command will return replication status for all replicated volumes in a subscription. Optionally filter by account, capacity pool, and volume.
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
    }

    protected override ReplicationStatusGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.Pool = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Pool.Name);
        options.Volume = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Volume.Name);
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

            var replicationStatuses = await netAppFilesService.GetReplicationStatusDetails(
                options.Account,
                options.Pool,
                options.Volume,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new ReplicationStatusGetCommandResult(replicationStatuses?.Results ?? [], replicationStatuses?.AreResultsTruncated ?? false),
                NetAppFilesJsonContext.Default.ReplicationStatusGetCommandResult);
        }
        catch (Exception ex)
        {
            if (options.Volume is null)
            {
                _logger.LogError(ex, "Error listing NetApp Files replication status details. Subscription: {Subscription}, Options: {@Options}", options.Subscription, options);
            }
            else
            {
                _logger.LogError(ex, "Error getting NetApp Files replication status details. Volume: {Volume}, Subscription: {Subscription}, Options: {@Options}",
                    options.Volume, options.Subscription, options);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ReplicationStatusGetCommandResult(List<ReplicationStatusInfo> ReplicationStatuses, bool AreResultsTruncated);
}
