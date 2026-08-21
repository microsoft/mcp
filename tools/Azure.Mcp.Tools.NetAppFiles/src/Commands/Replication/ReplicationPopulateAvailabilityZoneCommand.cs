// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Options.Replication;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Replication;

[CommandMetadata(
    Id = "bd4d3862-d3fa-47b9-bd39-8ffd31f9cda2",
    Name = "populate-availability-zone",
    Description = "Populate availability zone information for an Azure NetApp Files volume.",
    Title = "Populate NetApp Files Volume Availability Zone",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class ReplicationPopulateAvailabilityZoneCommand(ILogger<ReplicationPopulateAvailabilityZoneCommand> logger, INetAppFilesService netAppFilesService) : ReplicationCommandBase<ReplicationActionOptions>()
{
    private readonly ILogger<ReplicationPopulateAvailabilityZoneCommand> _logger = logger;
    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RegisterReplicationOptions(command, includeNoWait: true);
    }

    protected override ReplicationActionOptions BindOptions(ParseResult parseResult) => BindReplicationOptions(parseResult, base.BindOptions(parseResult));

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
            var result = await _netAppFilesService.PopulateAvailabilityZone(options.Account, options.Pool, options.Volume, options.ResourceGroup, options.Ids, options.Subscription!, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, NetAppFilesJsonContext.Default.NetAppVolumeCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error populating NetApp Files availability zone. Volume: {Volume}, Subscription: {Subscription}", options.Volume, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }
}