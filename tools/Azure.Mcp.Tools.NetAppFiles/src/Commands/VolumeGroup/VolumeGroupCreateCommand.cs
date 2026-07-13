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
    Id = "c9f4d3a7-1e6b-4c8d-b2a5-e7f1d8c6a3b9",
    Name = "create",
    Description =
        """
        Creates an Azure NetApp Files volume group in a specified account and returns the created volume group details including name, location, resource group, provisioning state, application type, application identifier, and group description. Requires account name, volume group name, resource group, location, application type, and application identifier.
        """,
    Title = "Create NetApp Files Volume Group",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class VolumeGroupCreateCommand(ILogger<VolumeGroupCreateCommand> logger, INetAppFilesService netAppFilesService) : SubscriptionCommand<VolumeGroupCreateOptions>()
{
    private readonly ILogger<VolumeGroupCreateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(NetAppFilesOptionDefinitions.Account);
        command.Options.Add(NetAppFilesOptionDefinitions.VolumeGroup);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Location);
        command.Options.Add(NetAppFilesOptionDefinitions.ApplicationType);
        command.Options.Add(NetAppFilesOptionDefinitions.ApplicationIdentifier);
        command.Options.Add(NetAppFilesOptionDefinitions.GroupDescription);
        command.Options.Add(NetAppFilesOptionDefinitions.Tags);
        command.Options.Add(NetAppFilesOptionDefinitions.NoWait);
        command.Options.Add(NetAppFilesOptionDefinitions.Pool.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Subnet);
        command.Options.Add(NetAppFilesOptionDefinitions.Vnet);
        command.Options.Add(NetAppFilesOptionDefinitions.Zones);
        command.Options.Add(NetAppFilesOptionDefinitions.EncryptionKeySource);
        command.Options.Add(NetAppFilesOptionDefinitions.KeyVaultPrivateEndpointResourceId);
        command.Options.Add(NetAppFilesOptionDefinitions.GpRules);
        command.Options.Add(NetAppFilesOptionDefinitions.ProximityPlacementGroup);
        command.Options.Add(NetAppFilesOptionDefinitions.BackupNfsv3);
        command.Options.Add(NetAppFilesOptionDefinitions.DataBackupReplSkd);
        command.Options.Add(NetAppFilesOptionDefinitions.DataBackupSize);
        command.Options.Add(NetAppFilesOptionDefinitions.DataBackupSrcId);
        command.Options.Add(NetAppFilesOptionDefinitions.DataBackupThroughput);
        command.Options.Add(NetAppFilesOptionDefinitions.DataReplSkd);
        command.Options.Add(NetAppFilesOptionDefinitions.DataSize);
        command.Options.Add(NetAppFilesOptionDefinitions.DataSrcId);
        command.Options.Add(NetAppFilesOptionDefinitions.DataThroughput);
        command.Options.Add(NetAppFilesOptionDefinitions.LogBackupSize);
        command.Options.Add(NetAppFilesOptionDefinitions.LogBackupSrcId);
        command.Options.Add(NetAppFilesOptionDefinitions.LogBackupThroughput);
        command.Options.Add(NetAppFilesOptionDefinitions.LogBackupReplSkd);
        command.Options.Add(NetAppFilesOptionDefinitions.LogSize);
        command.Options.Add(NetAppFilesOptionDefinitions.LogThroughput);
        command.Options.Add(NetAppFilesOptionDefinitions.BinarySize);
        command.Options.Add(NetAppFilesOptionDefinitions.BinaryThroughput);
        command.Options.Add(NetAppFilesOptionDefinitions.LogMirrorSize);
        command.Options.Add(NetAppFilesOptionDefinitions.LogMirrorThroughput);
        command.Options.Add(NetAppFilesOptionDefinitions.Volumes);
        command.Options.Add(NetAppFilesOptionDefinitions.SharedReplSkd);
        command.Options.Add(NetAppFilesOptionDefinitions.SharedSize);
        command.Options.Add(NetAppFilesOptionDefinitions.SharedSrcId);
        command.Options.Add(NetAppFilesOptionDefinitions.SharedThroughput);
        command.Options.Add(NetAppFilesOptionDefinitions.DatabaseSize);
        command.Options.Add(NetAppFilesOptionDefinitions.DatabaseThroughput);
        command.Options.Add(NetAppFilesOptionDefinitions.NumberOfVolumes);
        command.Options.Add(NetAppFilesOptionDefinitions.Memory);
        command.Options.Add(NetAppFilesOptionDefinitions.NumberOfHosts);
        command.Options.Add(NetAppFilesOptionDefinitions.AddSnapshotCapacity);
        command.Options.Add(NetAppFilesOptionDefinitions.Prefix);
        command.Options.Add(NetAppFilesOptionDefinitions.SmbAccess);
        command.Options.Add(NetAppFilesOptionDefinitions.SmbBrowsable);
        command.Options.Add(NetAppFilesOptionDefinitions.StartHostId);
        command.Options.Add(NetAppFilesOptionDefinitions.SystemRole);
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
        options.Tags = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Tags.Name);
        options.NoWait = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.NoWait.Name);
        options.Pool = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Pool.Name);
        options.Subnet = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Subnet.Name);
        options.Vnet = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Vnet.Name);
        options.Zones = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Zones.Name);
        options.EncryptionKeySource = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.EncryptionKeySource.Name);
        options.KeyVaultPrivateEndpointResourceId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.KeyVaultPrivateEndpointResourceId.Name);
        options.GpRules = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.GpRules.Name);
        options.ProximityPlacementGroup = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ProximityPlacementGroup.Name);
        options.BackupNfsv3 = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.BackupNfsv3.Name);
        options.DataBackupReplSkd = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.DataBackupReplSkd.Name);
        options.DataBackupSize = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.DataBackupSize.Name);
        options.DataBackupSrcId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.DataBackupSrcId.Name);
        options.DataBackupThroughput = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.DataBackupThroughput.Name);
        options.DataReplSkd = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.DataReplSkd.Name);
        options.DataSize = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.DataSize.Name);
        options.DataSrcId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.DataSrcId.Name);
        options.DataThroughput = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.DataThroughput.Name);
        options.LogBackupSize = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.LogBackupSize.Name);
        options.LogBackupSrcId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.LogBackupSrcId.Name);
        options.LogBackupThroughput = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.LogBackupThroughput.Name);
        options.LogBackupReplSkd = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.LogBackupReplSkd.Name);
        options.LogSize = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.LogSize.Name);
        options.LogThroughput = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.LogThroughput.Name);
        options.BinarySize = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.BinarySize.Name);
        options.BinaryThroughput = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.BinaryThroughput.Name);
        options.LogMirrorSize = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.LogMirrorSize.Name);
        options.LogMirrorThroughput = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.LogMirrorThroughput.Name);
        options.Volumes = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Volumes.Name);
        options.SharedReplSkd = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.SharedReplSkd.Name);
        options.SharedSize = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.SharedSize.Name);
        options.SharedSrcId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.SharedSrcId.Name);
        options.SharedThroughput = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.SharedThroughput.Name);
        options.DatabaseSize = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.DatabaseSize.Name);
        options.DatabaseThroughput = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.DatabaseThroughput.Name);
        options.NumberOfVolumes = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.NumberOfVolumes.Name);
        options.Memory = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.Memory.Name);
        options.NumberOfHosts = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.NumberOfHosts.Name);
        options.AddSnapshotCapacity = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.AddSnapshotCapacity.Name);
        options.Prefix = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Prefix.Name);
        options.SmbAccess = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.SmbAccess.Name);
        options.SmbBrowsable = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.SmbBrowsable.Name);
        options.StartHostId = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.StartHostId.Name);
        options.SystemRole = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.SystemRole.Name);
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
            if (!string.IsNullOrWhiteSpace(options.Tags))
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

            var volumeGroup = await _netAppFilesService.CreateVolumeGroup(
                options.Account!,
                options.VolumeGroup!,
                options.ResourceGroup!,
                options.Location!,
                options.ApplicationType!,
                options.ApplicationIdentifier!,
                options.Subscription!,
                options.GroupDescription,
                tags,
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
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A volume group with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the volume group. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Account or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    private static void ValidateUnsupportedCreateArguments(VolumeGroupCreateOptions options)
    {
        if (options.NoWait)
        {
            throw new ArgumentException("The --no-wait argument is not supported by this command yet.");
        }

        if (options.BackupNfsv3 ||
            !string.IsNullOrWhiteSpace(options.Pool) ||
            !string.IsNullOrWhiteSpace(options.Subnet) ||
            !string.IsNullOrWhiteSpace(options.Vnet) ||
            options.Zones is { Length: > 0 } ||
            !string.IsNullOrWhiteSpace(options.EncryptionKeySource) ||
            !string.IsNullOrWhiteSpace(options.KeyVaultPrivateEndpointResourceId) ||
            !string.IsNullOrWhiteSpace(options.DataBackupReplSkd) ||
            options.DataBackupSize.HasValue ||
            !string.IsNullOrWhiteSpace(options.DataBackupSrcId) ||
            options.DataBackupThroughput.HasValue ||
            !string.IsNullOrWhiteSpace(options.DataReplSkd) ||
            options.DataSize.HasValue ||
            !string.IsNullOrWhiteSpace(options.DataSrcId) ||
            options.DataThroughput.HasValue ||
            !string.IsNullOrWhiteSpace(options.GpRules) ||
            options.LogBackupSize.HasValue ||
            !string.IsNullOrWhiteSpace(options.LogBackupSrcId) ||
            options.LogBackupThroughput.HasValue ||
            !string.IsNullOrWhiteSpace(options.LogBackupReplSkd) ||
            options.LogSize.HasValue ||
            options.LogThroughput.HasValue ||
            options.BinarySize.HasValue ||
            options.BinaryThroughput.HasValue ||
            options.LogMirrorSize.HasValue ||
            options.LogMirrorThroughput.HasValue ||
            !string.IsNullOrWhiteSpace(options.Volumes) ||
            !string.IsNullOrWhiteSpace(options.SharedReplSkd) ||
            options.SharedSize.HasValue ||
            !string.IsNullOrWhiteSpace(options.SharedSrcId) ||
            options.SharedThroughput.HasValue ||
            options.DatabaseSize.HasValue ||
            options.DatabaseThroughput.HasValue ||
            options.NumberOfVolumes.HasValue ||
            options.Memory.HasValue ||
            options.NumberOfHosts.HasValue ||
            options.AddSnapshotCapacity.HasValue ||
            !string.IsNullOrWhiteSpace(options.ProximityPlacementGroup) ||
            !string.IsNullOrWhiteSpace(options.Prefix) ||
            !string.IsNullOrWhiteSpace(options.SmbAccess) ||
            !string.IsNullOrWhiteSpace(options.SmbBrowsable) ||
            options.StartHostId.HasValue ||
            !string.IsNullOrWhiteSpace(options.SystemRole))
        {
            throw new ArgumentException("One or more advanced volume group create arguments are not supported by this command yet.");
        }
    }

    internal record VolumeGroupCreateCommandResult([property: JsonPropertyName("volumeGroup")] VolumeGroupCreateResult VolumeGroup);
}
