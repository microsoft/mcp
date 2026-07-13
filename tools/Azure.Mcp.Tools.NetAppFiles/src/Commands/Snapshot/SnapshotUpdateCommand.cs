// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Mcp.Core.Commands.Subscription;
using Microsoft.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Snapshot;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Snapshot;

[CommandMetadata(
    Id = "c6e9f3a1-7d4b-4c8e-b2a5-f1d8e6b4c9a3",
    Name = "update",
    Description =
        """
        Updates an existing Azure NetApp Files snapshot for a specified volume in a capacity pool under a NetApp account, and returns the updated snapshot details including name, location, resource group, provisioning state, and creation time. Requires account name, pool name, volume name, snapshot name, resource group, location, and subscription.
        """,
    Title = "Update NetApp Files Snapshot",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class SnapshotUpdateCommand(ILogger<SnapshotUpdateCommand> logger, INetAppFilesService netAppFilesService) : SubscriptionCommand<SnapshotUpdateOptions>()
{
    private readonly ILogger<SnapshotUpdateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(NetAppFilesOptionDefinitions.Account.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Pool.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Volume.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Snapshot.AsOptional());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Ids.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Location.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.NoWait.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.AcquirePolicyToken.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ChangeReference.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Add.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Set.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Remove.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ForceString.AsOptional());
    }

    protected override SnapshotUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.Pool = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Pool.Name);
        options.Volume = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Volume.Name);
        options.Snapshot = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Snapshot.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Ids = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Ids.Name);
        options.Location = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Location.Name);
        options.NoWait = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.NoWait.Name);
        options.AcquirePolicyToken = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.AcquirePolicyToken.Name);
        options.ChangeReference = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ChangeReference.Name);
        options.Add = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Add.Name);
        options.Set = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Set.Name);
        options.Remove = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Remove.Name);
        options.ForceString = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.ForceString.Name);
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
            ResolveResourceIdArguments(options);
            ValidateUnsupportedUpdateArguments(options);

            var snapshot = await _netAppFilesService.UpdateSnapshot(
                options.Account!,
                options.Pool!,
                options.Volume!,
                options.Snapshot!,
                options.ResourceGroup!,
                options.Location,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new SnapshotUpdateCommandResult(snapshot),
                NetAppFilesJsonContext.Default.SnapshotUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating NetApp Files snapshot. Account: {Account}, Pool: {Pool}, Volume: {Volume}, Snapshot: {Snapshot}, Subscription: {Subscription}, Options: {@Options}",
                options.Account, options.Pool, options.Volume, options.Snapshot, options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A snapshot with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed updating the snapshot. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Snapshot, account, pool, volume, or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    private static void ResolveResourceIdArguments(SnapshotUpdateOptions options)
    {
        if (options.Ids is { Length: > 0 })
        {
            if (options.Ids.Length > 1)
            {
                throw new ArgumentException("Only a single resource ID is supported for snapshot update operations.", nameof(options.Ids));
            }

            var resourceIdentifier = new ResourceIdentifier(options.Ids[0]);
            var segments = options.Ids[0]
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            options.Snapshot = GetSegmentValue(segments, "snapshots") ?? resourceIdentifier.Name;
            options.Volume = GetSegmentValue(segments, "volumes");
            options.Pool = GetSegmentValue(segments, "capacityPools");
            options.Account = GetSegmentValue(segments, "netAppAccounts");
            options.ResourceGroup = resourceIdentifier.ResourceGroupName;
            options.Subscription = resourceIdentifier.SubscriptionId;
        }

        if (string.IsNullOrWhiteSpace(options.Account) ||
            string.IsNullOrWhiteSpace(options.Pool) ||
            string.IsNullOrWhiteSpace(options.Volume) ||
            string.IsNullOrWhiteSpace(options.Snapshot) ||
            string.IsNullOrWhiteSpace(options.ResourceGroup))
        {
            throw new ArgumentException("Either --ids or all of --account, --pool, --volume, --snapshot, and --resource-group must be provided for snapshot update.");
        }
    }

    private static string? GetSegmentValue(string[] segments, string segmentName)
    {
        for (var index = 0; index < segments.Length - 1; index++)
        {
            if (string.Equals(segments[index], segmentName, StringComparison.OrdinalIgnoreCase))
            {
                return segments[index + 1];
            }
        }

        return null;
    }

    private static void ValidateUnsupportedUpdateArguments(SnapshotUpdateOptions options)
    {
        if (options.NoWait)
        {
            throw new ArgumentException("The --no-wait argument is not supported by this command yet.");
        }

        if (options.AcquirePolicyToken)
        {
            throw new ArgumentException("The --acquirePolicyToken argument is not supported by this command yet.");
        }

        if (!string.IsNullOrWhiteSpace(options.ChangeReference))
        {
            throw new ArgumentException("The --changeReference argument is not supported by this command yet.");
        }

        if (options.Add is { Length: > 0 })
        {
            throw new ArgumentException("The --add argument is not supported by this command yet.");
        }

        if (options.Set is { Length: > 0 })
        {
            throw new ArgumentException("The --set argument is not supported by this command yet.");
        }

        if (options.Remove is { Length: > 0 })
        {
            throw new ArgumentException("The --remove argument is not supported by this command yet.");
        }

        if (options.ForceString)
        {
            throw new ArgumentException("The --force-string argument is not supported by this command yet.");
        }
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record SnapshotUpdateCommandResult([property: JsonPropertyName("snapshot")] SnapshotCreateResult Snapshot);
}
