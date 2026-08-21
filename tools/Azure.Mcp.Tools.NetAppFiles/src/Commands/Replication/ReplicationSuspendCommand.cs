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
    Id = "da863d2c-0d28-4bff-9f23-75c8903ba24f",
    Name = "suspend",
    Description = "Suspend or break Azure NetApp Files replication on the destination volume.",
    Title = "Suspend NetApp Files Replication",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class ReplicationSuspendCommand(ILogger<ReplicationSuspendCommand> logger, INetAppFilesService netAppFilesService) : ReplicationCommandBase<ReplicationSuspendOptions>()
{
    private readonly ILogger<ReplicationSuspendCommand> _logger = logger;
    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RegisterReplicationOptions(command, includeNoWait: true);
        command.Options.Add(NetAppFilesOptionDefinitions.ForceBreakReplication);
    }

    protected override ReplicationSuspendOptions BindOptions(ParseResult parseResult)
    {
        var options = BindReplicationOptions(parseResult, base.BindOptions(parseResult));
        options.ForceBreakReplication = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.ForceBreakReplication.Name);
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
            ValidateUnsupportedActionOptions(options);
            var result = await _netAppFilesService.SuspendReplication(options.Account, options.Pool, options.Volume, options.ResourceGroup, options.Ids, options.Subscription!, options.ForceBreakReplication, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, NetAppFilesJsonContext.Default.ReplicationOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending NetApp Files replication. Volume: {Volume}, Subscription: {Subscription}", options.Volume, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }
}