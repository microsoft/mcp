// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Volume;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Volume;

public sealed class VolumeGetCommand(ILogger<VolumeGetCommand> logger) : SubscriptionCommand<VolumeGetOptions>()
{
    private const string CommandTitle = "Get NetApp Files Volume Details";

    private readonly ILogger<VolumeGetCommand> _logger = logger;

    public override string Id => "b8d4f2c5-0e3a-4b9f-c6d7-e4f8a1b3c5d6";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves detailed information about Azure NetApp Files volumes, including volume name, location, resource group, provisioning state, service level, quota (usage threshold), creation token, subnet, protocol types, and network features. If a specific volume name is not provided, the command will return details for all volumes in a subscription. Optionally filter by account and capacity pool.
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

    protected override VolumeGetOptions BindOptions(ParseResult parseResult)
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

            var volumes = await netAppFilesService.GetVolumeDetails(
                options.Account,
                options.Pool,
                options.Volume,
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

    internal record VolumeGetCommandResult(List<NetAppVolumeInfo> Volumes, bool AreResultsTruncated);
}
