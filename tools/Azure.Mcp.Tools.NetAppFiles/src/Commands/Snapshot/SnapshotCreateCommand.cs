// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
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
    Id = "b4d8e2f0-6c3a-4b9d-a7e1-d5f9c3a8b2e6",
    Name = "create",
    Description =
        """
        Creates an Azure NetApp Files snapshot for a specified volume in a capacity pool under a NetApp account, and returns the created snapshot details including name, location, resource group, provisioning state, and creation time. Requires account name, pool name, volume name, snapshot name, resource group, location, and subscription.
        """,
    Title = "Create NetApp Files Snapshot",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class SnapshotCreateCommand(ILogger<SnapshotCreateCommand> logger, INetAppFilesService netAppFilesService) : SubscriptionCommand<SnapshotCreateOptions>()
{
    private readonly ILogger<SnapshotCreateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(NetAppFilesOptionDefinitions.Account.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Pool.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Volume.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Snapshot.AsRequired());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Location);
        command.Options.Add(NetAppFilesOptionDefinitions.NoWait.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.AcquirePolicyToken.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ChangeReference.AsOptional());
    }

    protected override SnapshotCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.Pool = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Pool.Name);
        options.Volume = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Volume.Name);
        options.Snapshot = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Snapshot.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Location = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Location.Name);
        options.NoWait = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.NoWait.Name);
        options.AcquirePolicyToken = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.AcquirePolicyToken.Name);
        options.ChangeReference = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ChangeReference.Name);
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
            ValidateUnsupportedCreateArguments(options);

            var snapshot = await _netAppFilesService.CreateSnapshot(
                options.Account!,
                options.Pool!,
                options.Volume!,
                options.Snapshot!,
                options.ResourceGroup!,
                options.Location!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(snapshot),
                NetAppFilesJsonContext.Default.SnapshotCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating NetApp Files snapshot. Account: {Account}, Pool: {Pool}, Volume: {Volume}, Snapshot: {Snapshot}, Subscription: {Subscription}, Options: {@Options}",
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
            $"Authorization failed creating the snapshot. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Account, pool, volume, or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    private static void ValidateUnsupportedCreateArguments(SnapshotCreateOptions options)
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
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record SnapshotCreateCommandResult([property: JsonPropertyName("snapshot")] SnapshotCreateResult Snapshot);
}
