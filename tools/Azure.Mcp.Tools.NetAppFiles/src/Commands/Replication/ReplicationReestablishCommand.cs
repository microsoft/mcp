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
    Id = "f9c4a7ee-c3bb-4a7f-9a26-ae39f0aa7e45",
    Name = "reestablish",
    Description = "Re-establish Azure NetApp Files replication between previously paired volumes.",
    Title = "Re-Establish NetApp Files Replication",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class ReplicationReestablishCommand(ILogger<ReplicationReestablishCommand> logger, INetAppFilesService netAppFilesService) : ReplicationCommandBase<ReplicationReestablishOptions>()
{
    private readonly ILogger<ReplicationReestablishCommand> _logger = logger;
    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RegisterReplicationOptions(command, includeNoWait: true);
        command.Options.Add(NetAppFilesOptionDefinitions.SourceVolumeId);
    }

    protected override ReplicationReestablishOptions BindOptions(ParseResult parseResult)
    {
        var options = BindReplicationOptions(parseResult, base.BindOptions(parseResult));
        options.SourceVolumeId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.SourceVolumeId.Name);
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
            if (string.IsNullOrWhiteSpace(options.SourceVolumeId))
            {
                throw new ArgumentException("Provide --sourceVolumeId to re-establish a replication.");
            }

            var result = await _netAppFilesService.ReestablishReplication(options.Account, options.Pool, options.Volume, options.ResourceGroup, options.Ids, options.Subscription!, options.SourceVolumeId, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, NetAppFilesJsonContext.Default.ReplicationOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error re-establishing NetApp Files replication. Volume: {Volume}, Subscription: {Subscription}", options.Volume, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }
}