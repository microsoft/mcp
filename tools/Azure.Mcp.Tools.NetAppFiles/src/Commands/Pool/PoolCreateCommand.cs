// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Microsoft.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Pool;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Pool;

public sealed class PoolCreateCommand(ILogger<PoolCreateCommand> logger, INetAppFilesService netAppFilesService) : SubscriptionCommand<PoolCreateOptions>()
{
    private const string CommandTitle = "Create NetApp Files Capacity Pool";

    private readonly ILogger<PoolCreateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override string Id => "c4f8a2e6-7d3b-4c9e-a1f5-e8b6d3c7a2f4";

    public override string Name => "create";

    public override string Description =>
        """
        Creates an Azure NetApp Files capacity pool in a specified account and returns the created pool details including name, location, resource group, provisioning state, service level, size, QoS type, cool access, and encryption type. Supports size or sizeInBytes, customThroughputMibps, and tags, with optional policy metadata parameters.
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
        command.Options.Add(NetAppFilesOptionDefinitions.Size.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.SizeInBytes.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ServiceLevel);
        command.Options.Add(NetAppFilesOptionDefinitions.CustomThroughputMibps.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.QosType);
        command.Options.Add(NetAppFilesOptionDefinitions.CoolAccess);
        command.Options.Add(NetAppFilesOptionDefinitions.EncryptionType);
        command.Options.Add(NetAppFilesOptionDefinitions.Tags.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.NoWait.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.AcquirePolicyToken.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ChangeReference.AsOptional());
    }

    protected override PoolCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.Pool = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Pool.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Location = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Location.Name);
        var size = parseResult.GetValueOrDefault<long>(NetAppFilesOptionDefinitions.Size.Name);
        options.Size = size != 0 ? size : null;
        options.SizeInBytes = parseResult.GetValueOrDefault<long?>(NetAppFilesOptionDefinitions.SizeInBytes.Name);
        options.ServiceLevel = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ServiceLevel.Name);
        options.CustomThroughputMibps = parseResult.GetValueOrDefault<long?>(NetAppFilesOptionDefinitions.CustomThroughputMibps.Name);
        options.QosType = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.QosType.Name);
        options.CoolAccess = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.CoolAccess.Name);
        options.EncryptionType = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.EncryptionType.Name);
        options.Tags = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Tags.Name);
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

            Dictionary<string, string>? tags = null;
            if (!string.IsNullOrEmpty(options.Tags))
            {
                try
                {
                    tags = JsonSerializer.Deserialize(options.Tags, NetAppFilesJsonContext.Default.DictionaryStringString);
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException($"Invalid tags JSON format: {ex.Message}", nameof(options.Tags));
                }
            }

            var resolvedSize = ResolvePoolSize(options.Size, options.SizeInBytes);

            var pool = await _netAppFilesService.CreatePool(
                options.Account!,
                options.Pool!,
                options.ResourceGroup!,
                options.Location!,
                resolvedSize,
                options.Subscription!,
                options.ServiceLevel,
                options.CustomThroughputMibps,
                options.QosType,
                options.CoolAccess,
                options.EncryptionType,
                tags,
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
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A capacity pool with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the capacity pool. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Account or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    private static long ResolvePoolSize(long? size, long? sizeInBytes)
    {
        var hasSize = size.HasValue && size.Value > 0;
        var hasSizeInBytes = sizeInBytes.HasValue && sizeInBytes.Value > 0;

        if (hasSize && hasSizeInBytes)
        {
            throw new ArgumentException("Use either --size or --sizeInBytes, not both.");
        }

        if (!hasSize && !hasSizeInBytes)
        {
            throw new ArgumentException("Either --size or --sizeInBytes must be provided.");
        }

        return hasSize ? size!.Value : sizeInBytes!.Value;
    }

    private static void ValidateUnsupportedCreateArguments(PoolCreateOptions options)
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

    internal record PoolCreateCommandResult([property: JsonPropertyName("pool")] CapacityPoolCreateResult Pool);
}
