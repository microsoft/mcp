// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Replication;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Replication;

[CommandMetadata(
    Id = "31893531-e5d5-47f1-b59b-6f7c64320b69",
    Name = "list",
    Description = "List all Azure NetApp Files replications for a specified volume.",
    Title = "List NetApp Files Replications",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false
)]
public sealed class ReplicationListCommand(ILogger<ReplicationListCommand> logger, INetAppFilesService netAppFilesService) : ReplicationCommandBase<ReplicationListOptions>()
{
    private readonly ILogger<ReplicationListCommand> _logger = logger;
    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RegisterReplicationOptions(command, includeNoWait: false);
        command.Options.Add(NetAppFilesOptionDefinitions.Exclude);
    }

    protected override ReplicationListOptions BindOptions(ParseResult parseResult)
    {
        var options = BindReplicationOptions(parseResult, base.BindOptions(parseResult));
        options.Exclude = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Exclude.Name);
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
            ValidateVolumeTarget(options);
            ValidateUnsupportedCommonOptions(options);
            var result = await _netAppFilesService.ListReplications(options.Account, options.Pool, options.Volume, options.ResourceGroup, options.Ids, options.Subscription!, options.Exclude, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, NetAppFilesJsonContext.Default.ReplicationListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing NetApp Files replications. Volume: {Volume}, Subscription: {Subscription}", options.Volume, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }
}