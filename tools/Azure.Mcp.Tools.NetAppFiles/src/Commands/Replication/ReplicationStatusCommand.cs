// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Options.Replication;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Replication;

[CommandMetadata(
    Id = "5f43637f-75bd-40a7-999d-553f666a5fc8",
    Name = "status",
    Description = "Get the runtime status of Azure NetApp Files replication for a specified volume.",
    Title = "Get NetApp Files Replication Runtime Status",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false
)]
public sealed class ReplicationStatusCommand(ILogger<ReplicationStatusCommand> logger, INetAppFilesService netAppFilesService) : ReplicationCommandBase<ReplicationStatusOptions>()
{
    private readonly ILogger<ReplicationStatusCommand> _logger = logger;
    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RegisterReplicationOptions(command, includeNoWait: false);
    }

    protected override ReplicationStatusOptions BindOptions(ParseResult parseResult) => BindReplicationOptions(parseResult, base.BindOptions(parseResult));

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
            var result = await _netAppFilesService.GetReplicationStatus(options.Account, options.Pool, options.Volume, options.ResourceGroup, options.Ids, options.Subscription!, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, NetAppFilesJsonContext.Default.VolumeReplicationStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting NetApp Files replication runtime status. Volume: {Volume}, Subscription: {Subscription}", options.Volume, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }
}