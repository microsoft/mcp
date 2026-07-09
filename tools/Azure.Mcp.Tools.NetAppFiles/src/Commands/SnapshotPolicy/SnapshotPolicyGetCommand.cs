// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Microsoft.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.SnapshotPolicy;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.SnapshotPolicy;

[CommandMetadata(
    Id = "a3c7e1d9-5f2b-4a8e-b6c4-d9e0f1a2b3c4",
    Name = "get",
    Description =
        """
        Retrieves detailed information about Azure NetApp Files snapshot policies, including policy name, location, resource group, provisioning state, enabled state, and schedule configuration (hourly, daily, weekly, monthly). If a specific snapshot policy name is not provided, the command will return details for all snapshot policies in a subscription. Optionally filter by account.
        """,
    Title = "Get NetApp Files Snapshot Policy Details",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false
)]
public sealed class SnapshotPolicyGetCommand(ILogger<SnapshotPolicyGetCommand> logger, INetAppFilesService netAppFilesService) : SubscriptionCommand<SnapshotPolicyGetOptions>()
{
    private readonly ILogger<SnapshotPolicyGetCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(NetAppFilesOptionDefinitions.Account.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.SnapshotPolicy.AsOptional());
    }

    protected override SnapshotPolicyGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.SnapshotPolicy = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.SnapshotPolicy.Name);
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
            var snapshotPolicies = await _netAppFilesService.GetSnapshotPolicyDetails(
                options.Account,
                options.SnapshotPolicy,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new SnapshotPolicyGetCommandResult(snapshotPolicies?.Results ?? [], snapshotPolicies?.AreResultsTruncated ?? false),
                NetAppFilesJsonContext.Default.SnapshotPolicyGetCommandResult);
        }
        catch (Exception ex)
        {
            if (options.SnapshotPolicy is null)
            {
                _logger.LogError(ex, "Error listing NetApp Files snapshot policy details. Subscription: {Subscription}, Options: {@Options}", options.Subscription, options);
            }
            else
            {
                _logger.LogError(ex, "Error getting NetApp Files snapshot policy details. SnapshotPolicy: {SnapshotPolicy}, Subscription: {Subscription}, Options: {@Options}",
                    options.SnapshotPolicy, options.Subscription, options);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SnapshotPolicyGetCommandResult(List<SnapshotPolicyInfo> SnapshotPolicies, bool AreResultsTruncated);
}
