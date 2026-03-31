// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.VolumeGroup;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.VolumeGroup;

public sealed class VolumeGroupCreateCommand(ILogger<VolumeGroupCreateCommand> logger) : SubscriptionCommand<VolumeGroupCreateOptions>()
{
    private const string CommandTitle = "Create NetApp Files Volume Group";

    private readonly ILogger<VolumeGroupCreateCommand> _logger = logger;

    public override string Id => "c9f4d3a7-1e6b-4c8d-b2a5-e7f1d8c6a3b9";

    public override string Name => "create";

    public override string Description =>
        """
        Creates an Azure NetApp Files volume group in a specified account and returns the created volume group details including name, location, resource group, provisioning state, application type, application identifier, and group description. Requires account name, volume group name, resource group, location, application type, and application identifier.
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
        command.Options.Add(NetAppFilesOptionDefinitions.VolumeGroup.AsRequired());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Location);
        command.Options.Add(NetAppFilesOptionDefinitions.ApplicationType);
        command.Options.Add(NetAppFilesOptionDefinitions.ApplicationIdentifier);
        command.Options.Add(NetAppFilesOptionDefinitions.GroupDescription);
    }

    protected override VolumeGroupCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.VolumeGroup = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.VolumeGroup.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Location = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Location.Name);
        options.ApplicationType = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ApplicationType.Name);
        options.ApplicationIdentifier = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ApplicationIdentifier.Name);
        options.GroupDescription = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.GroupDescription.Name);
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

            var volumeGroup = await netAppFilesService.CreateVolumeGroup(
                options.Account!,
                options.VolumeGroup!,
                options.ResourceGroup!,
                options.Location!,
                options.ApplicationType!,
                options.ApplicationIdentifier!,
                options.Subscription!,
                options.GroupDescription,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(volumeGroup),
                NetAppFilesJsonContext.Default.VolumeGroupCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating NetApp Files volume group. VolumeGroup: {VolumeGroup}, Account: {Account}, Options: {@Options}",
                options.VolumeGroup, options.Account, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A volume group with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the volume group. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Account or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VolumeGroupCreateCommandResult([property: JsonPropertyName("volumeGroup")] VolumeGroupCreateResult VolumeGroup);
}
