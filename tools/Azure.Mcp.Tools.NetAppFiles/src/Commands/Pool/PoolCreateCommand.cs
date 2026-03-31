// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Pool;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Pool;

public sealed class PoolCreateCommand(ILogger<PoolCreateCommand> logger) : SubscriptionCommand<PoolCreateOptions>()
{
    private const string CommandTitle = "Create NetApp Files Capacity Pool";

    private readonly ILogger<PoolCreateCommand> _logger = logger;

    public override string Id => "c4f8a2e6-7d3b-4c9e-a1f5-e8b6d3c7a2f4";

    public override string Name => "create";

    public override string Description =>
        """
        Creates an Azure NetApp Files capacity pool in a specified account and returns the created pool details including name, location, resource group, provisioning state, service level, size, QoS type, cool access, and encryption type. Requires account name, pool name, resource group, location, size, and service level.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(NetAppFilesOptionDefinitions.Account.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Pool.AsRequired());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Location);
        command.Options.Add(NetAppFilesOptionDefinitions.Size);
        command.Options.Add(NetAppFilesOptionDefinitions.ServiceLevel);
        command.Options.Add(NetAppFilesOptionDefinitions.QosType);
        command.Options.Add(NetAppFilesOptionDefinitions.CoolAccess);
        command.Options.Add(NetAppFilesOptionDefinitions.EncryptionType);
    }

    protected override PoolCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.Pool = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Pool.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Location = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Location.Name);
        options.Size = parseResult.GetValueOrDefault<long>(NetAppFilesOptionDefinitions.Size.Name);
        options.ServiceLevel = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ServiceLevel.Name);
        options.QosType = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.QosType.Name);
        options.CoolAccess = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.CoolAccess.Name);
        options.EncryptionType = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.EncryptionType.Name);
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

            var pool = await netAppFilesService.CreatePool(
                options.Account!,
                options.Pool!,
                options.ResourceGroup!,
                options.Location!,
                options.Size!.Value,
                options.Subscription!,
                options.ServiceLevel,
                options.QosType,
                options.CoolAccess,
                options.EncryptionType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(pool),
                NetAppFilesJsonContext.Default.PoolCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating NetApp Files capacity pool. Pool: {Pool}, Account: {Account}, Options: {@Options}",
                options.Pool, options.Account, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A capacity pool with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the capacity pool. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Account or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record PoolCreateCommandResult([property: JsonPropertyName("pool")] CapacityPoolCreateResult Pool);
}
