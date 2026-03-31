// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
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

public sealed class VolumeGroupUpdateCommand(ILogger<VolumeGroupUpdateCommand> logger) : SubscriptionCommand<VolumeGroupUpdateOptions>()
{
    private const string CommandTitle = "Update NetApp Files Volume Group";

    private readonly ILogger<VolumeGroupUpdateCommand> _logger = logger;

    public override string Id => "d8e5f2a1-3b7c-4d9e-a6c4-f0b3e7d1a5c2";

    public override string Name => "update";

    public override string Description =>
        """
        Updates an existing Azure NetApp Files volume group in a specified account and returns the updated volume group details including name, location, resource group, provisioning state, application type, application identifier, and group description. Supports updating group description and tags. Requires account name, volume group name, resource group, location, and subscription.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
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
        command.Options.Add(NetAppFilesOptionDefinitions.GroupDescription.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Tags.AsOptional());
    }

    protected override VolumeGroupUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.VolumeGroup = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.VolumeGroup.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Location = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Location.Name);
        options.GroupDescription = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.GroupDescription.Name);
        options.Tags = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Tags.Name);
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

            var volumeGroup = await netAppFilesService.UpdateVolumeGroup(
                options.Account!,
                options.VolumeGroup!,
                options.ResourceGroup!,
                options.Location!,
                options.Subscription!,
                options.GroupDescription,
                tags,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VolumeGroupUpdateCommandResult(volumeGroup),
                NetAppFilesJsonContext.Default.VolumeGroupUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating NetApp Files volume group. VolumeGroup: {VolumeGroup}, Account: {Account}, Options: {@Options}",
                options.VolumeGroup, options.Account, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed updating the volume group. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Volume group, account, or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record VolumeGroupUpdateCommandResult([property: JsonPropertyName("volumeGroup")] VolumeGroupCreateResult VolumeGroup);
}
