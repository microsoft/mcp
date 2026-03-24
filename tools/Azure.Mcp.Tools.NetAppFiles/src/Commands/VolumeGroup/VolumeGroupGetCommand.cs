// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.VolumeGroup;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.VolumeGroup;

public sealed class VolumeGroupGetCommand(ILogger<VolumeGroupGetCommand> logger) : SubscriptionCommand<VolumeGroupGetOptions>()
{
    private const string CommandTitle = "Get NetApp Files Volume Group Details";

    private readonly ILogger<VolumeGroupGetCommand> _logger = logger;

    public override string Id => "a7c3e1d4-9f2b-4a8e-b5c6-d3e7f0a2b4c8";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves detailed information about Azure NetApp Files volume groups, including volume group name, location, resource group, provisioning state, application type, application identifier, and group description. If a specific volume group name is not provided, the command will return details for all volume groups in a subscription. Optionally filter by account.
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
        command.Options.Add(NetAppFilesOptionDefinitions.VolumeGroup.AsOptional());
    }

    protected override VolumeGroupGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.VolumeGroup = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.VolumeGroup.Name);
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

            var volumeGroups = await netAppFilesService.GetVolumeGroupDetails(
                options.Account,
                options.VolumeGroup,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(volumeGroups?.Results ?? [], volumeGroups?.AreResultsTruncated ?? false),
                NetAppFilesJsonContext.Default.VolumeGroupGetCommandResult);
        }
        catch (Exception ex)
        {
            if (options.VolumeGroup is null)
            {
                _logger.LogError(ex, "Error listing NetApp Files volume group details. Subscription: {Subscription}, Options: {@Options}", options.Subscription, options);
            }
            else
            {
                _logger.LogError(ex, "Error getting NetApp Files volume group details. VolumeGroup: {VolumeGroup}, Subscription: {Subscription}, Options: {@Options}",
                    options.VolumeGroup, options.Subscription, options);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record VolumeGroupGetCommandResult(List<VolumeGroupInfo> VolumeGroups, bool AreResultsTruncated);
}
