// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Volume;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Volume;

public sealed class VolumeCreateCommand(ILogger<VolumeCreateCommand> logger) : SubscriptionCommand<VolumeCreateOptions>()
{
    private const string CommandTitle = "Create NetApp Files Volume";

    private readonly ILogger<VolumeCreateCommand> _logger = logger;

    public override string Id => "d7e2f4a8-3b1c-4d5e-a9f6-c2e8b7d4a1f3";

    public override string Name => "create";

    public override string Description =>
        """
        Creates an Azure NetApp Files volume in a specified capacity pool and returns the created volume details including name, location, resource group, provisioning state, service level, quota, creation token, subnet, and protocol types. Requires account name, pool name, volume name, resource group, location, creation token, usage threshold, and subnet ID.
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
        command.Options.Add(NetAppFilesOptionDefinitions.Volume.AsRequired());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Location);
        command.Options.Add(NetAppFilesOptionDefinitions.CreationToken);
        command.Options.Add(NetAppFilesOptionDefinitions.UsageThreshold);
        command.Options.Add(NetAppFilesOptionDefinitions.SubnetId);
        command.Options.Add(NetAppFilesOptionDefinitions.ServiceLevel);
        command.Options.Add(NetAppFilesOptionDefinitions.ProtocolTypes);
    }

    protected override VolumeCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.Pool = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Pool.Name);
        options.Volume = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Volume.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Location = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Location.Name);
        options.CreationToken = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.CreationToken.Name);
        options.UsageThreshold = parseResult.GetValueOrDefault<long>(NetAppFilesOptionDefinitions.UsageThreshold.Name);
        options.SubnetId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.SubnetId.Name);
        options.ServiceLevel = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ServiceLevel.Name);
        options.ProtocolTypes = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.ProtocolTypes.Name);
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

            var volume = await netAppFilesService.CreateVolume(
                options.Account!,
                options.Pool!,
                options.Volume!,
                options.ResourceGroup!,
                options.Location!,
                options.CreationToken!,
                options.UsageThreshold!.Value,
                options.SubnetId!,
                options.Subscription!,
                options.ServiceLevel,
                options.ProtocolTypes?.ToList(),
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(volume),
                NetAppFilesJsonContext.Default.VolumeCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating NetApp Files volume. Volume: {Volume}, Account: {Account}, Pool: {Pool}, Options: {@Options}",
                options.Volume, options.Account, options.Pool, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A volume with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the volume. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Account, pool, or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VolumeCreateCommandResult([property: JsonPropertyName("volume")] NetAppVolumeCreateResult Volume);
}
