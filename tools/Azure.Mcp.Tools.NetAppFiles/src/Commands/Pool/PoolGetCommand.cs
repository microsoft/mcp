// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Pool;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Pool;

public sealed class PoolGetCommand(ILogger<PoolGetCommand> logger) : SubscriptionCommand<PoolGetOptions>()
{
    private const string CommandTitle = "Get NetApp Files Capacity Pool Details";

    private readonly ILogger<PoolGetCommand> _logger = logger;

    public override string Id => "a3c7e1d9-5f2b-4a8e-b6c4-d9e2f7a1b3c5";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves detailed information about Azure NetApp Files capacity pools, including pool name, location, resource group, provisioning state, service level, size, QoS type, cool access, and encryption type. If a specific pool name is not provided, the command will return details for all capacity pools in a subscription. Optionally filter by account.
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
    }

    protected override PoolGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.Pool = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Pool.Name);
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

            var pools = await netAppFilesService.GetPoolDetails(
                options.Account,
                options.Pool,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new PoolGetCommandResult(pools?.Results ?? [], pools?.AreResultsTruncated ?? false),
                NetAppFilesJsonContext.Default.PoolGetCommandResult);
        }
        catch (Exception ex)
        {
            if (options.Pool is null)
            {
                _logger.LogError(ex, "Error listing NetApp Files capacity pool details. Subscription: {Subscription}, Options: {@Options}", options.Subscription, options);
            }
            else
            {
                _logger.LogError(ex, "Error getting NetApp Files capacity pool details. Pool: {Pool}, Subscription: {Subscription}, Options: {@Options}",
                    options.Pool, options.Subscription, options);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record PoolGetCommandResult(List<CapacityPoolInfo> Pools, bool AreResultsTruncated);
}
