// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Volume;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Volume;

[CommandMetadata(
    Id = "b8d4f2c5-0e3a-4b9f-c6d7-e4f8a1b3c5d6",
    Name = "get",
    Description =
        """
        Retrieves detailed information about Azure NetApp Files volumes, including volume name, location, resource group, provisioning state, service level, quota (usage threshold), creation token, subnet, protocol types, and network features. If a specific volume name is not provided, the command will return details for all volumes in a subscription. Optionally filter by account, capacity pool, resource group, or resource IDs.
        """,
    Title = "Get NetApp Files Volume Details",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false
)]
public sealed class VolumeGetCommand(ILogger<VolumeGetCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VolumeGetOptions, VolumeGetCommand.VolumeGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<VolumeGetCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, VolumeGetOptions options, CancellationToken cancellationToken)
    {
        Console.WriteLine(options);

        try
        {
            var volumes = await _netAppFilesService.GetVolumeDetails(
                options.Account,
                options.Pool,
                options.Volume,
                options.ResourceGroup,
                options.Ids,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(volumes?.Results ?? [], volumes?.AreResultsTruncated ?? false),
                NetAppFilesJsonContext.Default.VolumeGetCommandResult);
        }
        catch (Exception ex)
        {
            if (options.Volume is null)
            {
                _logger.LogError(ex, "Error listing NetApp Files volume details. Subscription: {Subscription}, Options: {@Options}", options.Subscription, options);
            }
            else
            {
                _logger.LogError(ex, "Error getting NetApp Files volume details. Volume: {Volume}, Subscription: {Subscription}, Options: {@Options}",
                    options.Volume, options.Subscription, options);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record VolumeGetCommandResult(List<NetAppVolumeInfo> Volumes, bool AreResultsTruncated);
}
