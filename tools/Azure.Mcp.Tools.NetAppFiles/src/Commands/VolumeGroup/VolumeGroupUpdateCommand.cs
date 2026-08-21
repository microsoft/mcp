// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Microsoft.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.VolumeGroup;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.VolumeGroup;

[CommandMetadata(
    Id = "d8e5f2a1-3b7c-4d9e-a6c4-f0b3e7d1a5c2",
    Name = "update",
    Description =
        """
        Updates an existing Azure NetApp Files volume group in a specified account and returns the updated volume group details including name, location, resource group, provisioning state, application type, application identifier, and group description. Supports updating group description and tags. Requires account name, volume group name, resource group, location, and subscription.
        """,
    Title = "Update NetApp Files Volume Group",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class VolumeGroupUpdateCommand(ILogger<VolumeGroupUpdateCommand> logger, INetAppFilesService netAppFilesService) : SubscriptionCommand<VolumeGroupUpdateOptions>()
{
    private readonly ILogger<VolumeGroupUpdateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(NetAppFilesOptionDefinitions.Account.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.VolumeGroup.AsOptional());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Location);
        command.Options.Add(NetAppFilesOptionDefinitions.GroupDescription.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Tags.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Ids.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.NoWait.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Add.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Set.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Remove.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ForceString.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.GroupMetaData.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Volumes.AsOptional());
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
        options.Ids = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Ids.Name);
        options.NoWait = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.NoWait.Name);
        options.Add = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Add.Name);
        options.Set = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Set.Name);
        options.Remove = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Remove.Name);
        options.ForceString = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.ForceString.Name);
        options.GroupMetaData = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.GroupMetaData.Name);
        options.Volumes = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Volumes.Name);
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

            var volumeGroup = await _netAppFilesService.UpdateVolumeGroup(
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

    private static void ResolveResourceIdArguments(VolumeGroupUpdateOptions options)
    {
        if (options.Ids is { Length: > 0 })
        {
            if (options.Ids.Length > 1)
            {
                throw new ArgumentException("Only a single resource ID is supported for volume group update operations.", nameof(options.Ids));
            }

            var segments = options.Ids[0]
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            options.VolumeGroup = GetSegmentValue(segments, "volumeGroups");
            options.Account = GetSegmentValue(segments, "netAppAccounts");
            options.ResourceGroup = GetSegmentValue(segments, "resourceGroups");
            options.Subscription ??= GetSegmentValue(segments, "subscriptions");
        }

        if (string.IsNullOrWhiteSpace(options.Account) ||
            string.IsNullOrWhiteSpace(options.VolumeGroup) ||
            string.IsNullOrWhiteSpace(options.ResourceGroup))
        {
            throw new ArgumentException("Either --ids or all of --account, --volumeGroup, and --resource-group must be provided for volume group update.");
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

    private static void ValidateUnsupportedUpdateArguments(VolumeGroupUpdateOptions options)
    {
        if (options.NoWait)
        {
            throw new ArgumentException("The --no-wait argument is not supported by this command yet.");
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

        if (!string.IsNullOrWhiteSpace(options.GroupMetaData))
        {
            throw new ArgumentException("The --group-meta-data argument is not supported by this command yet.");
        }

        if (!string.IsNullOrWhiteSpace(options.Volumes))
        {
            throw new ArgumentException("The --volumes argument is not supported by this command yet.");
        }
    }

    internal record VolumeGroupUpdateCommandResult([property: JsonPropertyName("volumeGroup")] VolumeGroupCreateResult VolumeGroup);
}
