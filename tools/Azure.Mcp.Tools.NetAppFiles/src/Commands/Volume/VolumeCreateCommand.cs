// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Microsoft.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Volume;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Services.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Volume;

[CommandMetadata(
    Id = "d7e2f4a8-3b1c-4d5e-a9f6-c2e8b7d4a1f3",
    Name = "create",
    Description =
        """
        Creates an Azure NetApp Files volume in a specified capacity pool and returns the created volume details including name, location, resource group, provisioning state, service level, quota, creation token, subnet, and protocol types. Requires account name, pool name, volume name, resource group, location, creation token, usage threshold, and subnet ID.
        """,
    Title = "Create NetApp Files Volume",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class VolumeCreateCommand(ILogger<VolumeCreateCommand> logger, INetAppFilesService netAppFilesService) : SubscriptionCommand<VolumeCreateOptions>()
{
    private readonly ILogger<VolumeCreateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

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
        command.Options.Add(NetAppFilesOptionDefinitions.SubnetId.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Subnet.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Vnet.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ServiceLevel);
        command.Options.Add(NetAppFilesOptionDefinitions.ProtocolTypes);
        command.Options.Add(NetAppFilesOptionDefinitions.AcceptGrowCapacityPoolForShortTermCloneSplit.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.AllowedClients.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.AvsDataStore.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.BackupId.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.BackupPolicyId.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.BackupVaultId.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.CoolAccessRetrievalPolicy.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.CoolAccessTieringPolicy.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.CapacityPoolResourceId.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ChownMode.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Cifs.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.CoolAccessVolume.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.CoolnessPeriod.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.DeleteBaseSnapshot.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.DesiredArpState.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.EnableSubvolumes.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.EncryptionKeySource.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ExportPolicyRules.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ExternalHostName.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ExternalServerName.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ExternalVolumeName.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.HasRootAccess.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.IsLargeVolume.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.KerberosEnabled.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Kerberos5R.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Kerberos5Rw.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Kerberos5IR.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Kerberos5IRw.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Kerberos5PR.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Kerberos5PRw.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.KeyVaultPrivateEndpointResourceId.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.LdapEnabled.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.NetworkFeatures.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.PlacementRules.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.PolicyEnforced.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ProximityPlacementGroup.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.RelocationRequested.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.RemoteVolumeResourceId.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.RemoteVolumeRegion.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ReplicationSchedule.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.RuleIndex.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.SecurityStyle.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.SmbAccessEnumeration.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.SmbContinuouslyAvailable.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.SmbEncryption.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.SmbNonBrowsable.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.SnapshotDirectoryVisible.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.SnapshotId.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.SnapshotPolicyId.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Tags.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ThroughputMibps.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.UnixPermissions.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.UnixReadOnly.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.UnixReadWrite.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.VolumeSpecName.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.VolumeType.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Zones.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.NoWait.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.AcquirePolicyToken.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ChangeReference.AsOptional());
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
        options.Subnet = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Subnet.Name);
        options.Vnet = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Vnet.Name);
        options.ServiceLevel = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ServiceLevel.Name);
        options.ProtocolTypes = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.ProtocolTypes.Name);
        options.AcceptGrowCapacityPoolForShortTermCloneSplit = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.AcceptGrowCapacityPoolForShortTermCloneSplit.Name);
        options.AllowedClients = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.AllowedClients.Name);
        options.AvsDataStore = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.AvsDataStore.Name);
        options.BackupId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.BackupId.Name);
        options.BackupPolicyId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.BackupPolicyId.Name);
        options.BackupVaultId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.BackupVaultId.Name);
        options.CoolAccessRetrievalPolicy = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.CoolAccessRetrievalPolicy.Name);
        options.CoolAccessTieringPolicy = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.CoolAccessTieringPolicy.Name);
        options.CapacityPoolResourceId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.CapacityPoolResourceId.Name);
        options.ChownMode = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ChownMode.Name);
        options.Cifs = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.Cifs.Name);
        options.CoolAccessVolume = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.CoolAccessVolume.Name);
        options.CoolnessPeriod = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.CoolnessPeriod.Name);
        options.DeleteBaseSnapshot = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.DeleteBaseSnapshot.Name);
        options.DesiredArpState = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.DesiredArpState.Name);
        options.EnableSubvolumes = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.EnableSubvolumes.Name);
        options.EncryptionKeySource = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.EncryptionKeySource.Name);
        options.ExportPolicyRules = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ExportPolicyRules.Name);
        options.ExternalHostName = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ExternalHostName.Name);
        options.ExternalServerName = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ExternalServerName.Name);
        options.ExternalVolumeName = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ExternalVolumeName.Name);
        options.HasRootAccess = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.HasRootAccess.Name);
        options.IsLargeVolume = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.IsLargeVolume.Name);
        options.KerberosEnabled = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.KerberosEnabled.Name);
        options.Kerberos5R = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.Kerberos5R.Name);
        options.Kerberos5Rw = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.Kerberos5Rw.Name);
        options.Kerberos5IR = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.Kerberos5IR.Name);
        options.Kerberos5IRw = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.Kerberos5IRw.Name);
        options.Kerberos5PR = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.Kerberos5PR.Name);
        options.Kerberos5PRw = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.Kerberos5PRw.Name);
        options.KeyVaultPrivateEndpointResourceId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.KeyVaultPrivateEndpointResourceId.Name);
        options.LdapEnabled = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.LdapEnabled.Name);
        options.NetworkFeatures = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.NetworkFeatures.Name);
        options.PlacementRules = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.PlacementRules.Name);
        options.PolicyEnforced = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.PolicyEnforced.Name);
        options.ProximityPlacementGroup = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ProximityPlacementGroup.Name);
        options.RelocationRequested = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.RelocationRequested.Name);
        options.RemoteVolumeResourceId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.RemoteVolumeResourceId.Name);
        options.RemoteVolumeRegion = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.RemoteVolumeRegion.Name);
        options.ReplicationSchedule = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ReplicationSchedule.Name);
        options.RuleIndex = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.RuleIndex.Name);
        options.SecurityStyle = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.SecurityStyle.Name);
        options.SmbAccessEnumeration = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.SmbAccessEnumeration.Name);
        options.SmbContinuouslyAvailable = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.SmbContinuouslyAvailable.Name);
        options.SmbEncryption = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.SmbEncryption.Name);
        options.SmbNonBrowsable = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.SmbNonBrowsable.Name);
        options.SnapshotDirectoryVisible = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.SnapshotDirectoryVisible.Name);
        options.SnapshotId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.SnapshotId.Name);
        options.SnapshotPolicyId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.SnapshotPolicyId.Name);
        options.Tags = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Tags.Name);
        options.ThroughputMibps = parseResult.GetValueOrDefault<int?>(NetAppFilesOptionDefinitions.ThroughputMibps.Name);
        options.UnixPermissions = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.UnixPermissions.Name);
        options.UnixReadOnly = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.UnixReadOnly.Name);
        options.UnixReadWrite = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.UnixReadWrite.Name);
        options.VolumeSpecName = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.VolumeSpecName.Name);
        options.VolumeType = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.VolumeType.Name);
        options.Zones = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Zones.Name);
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
            ValidateUnsupportedArguments(options);

            Dictionary<string, string>? tags = null;
            if (!string.IsNullOrEmpty(options.Tags))
            {
                try
                {
                    tags = JsonSerializer.Deserialize(options.Tags, NetAppFilesJsonContext.Default.DictionaryStringString);
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException($"Invalid tags JSON format: {ex.Message}", nameof(options.Tags), ex);
                }
            }

            var createParameters = new NetAppVolumeCreateParameters
            {
                CreationToken = options.CreationToken!,
                UsageThreshold = options.UsageThreshold!.Value,
                SubnetId = options.SubnetId,
                Subnet = options.Subnet,
                Vnet = options.Vnet,
                ServiceLevel = options.ServiceLevel,
                ProtocolTypes = options.ProtocolTypes?.ToList(),
                AcceptGrowCapacityPoolForShortTermCloneSplit = options.AcceptGrowCapacityPoolForShortTermCloneSplit,
                AllowedClients = options.AllowedClients,
                AvsDataStore = options.AvsDataStore,
                BackupId = options.BackupId,
                BackupPolicyId = options.BackupPolicyId,
                BackupVaultId = options.BackupVaultId,
                CoolAccessRetrievalPolicy = options.CoolAccessRetrievalPolicy,
                CoolAccessTieringPolicy = options.CoolAccessTieringPolicy,
                CapacityPoolResourceId = options.CapacityPoolResourceId,
                ChownMode = options.ChownMode,
                Cifs = options.Cifs,
                CoolAccess = options.CoolAccessVolume,
                CoolnessPeriod = options.CoolnessPeriod,
                DeleteBaseSnapshot = options.DeleteBaseSnapshot,
                DesiredArpState = options.DesiredArpState,
                EnableSubvolumes = options.EnableSubvolumes,
                EncryptionKeySource = options.EncryptionKeySource,
                ExportPolicyRules = ParseJsonElementOption(options.ExportPolicyRules, nameof(options.ExportPolicyRules)),
                ExternalHostName = options.ExternalHostName,
                ExternalServerName = options.ExternalServerName,
                ExternalVolumeName = options.ExternalVolumeName,
                HasRootAccess = options.HasRootAccess,
                IsLargeVolume = options.IsLargeVolume,
                KerberosEnabled = options.KerberosEnabled,
                Kerberos5R = options.Kerberos5R,
                Kerberos5Rw = options.Kerberos5Rw,
                Kerberos5IR = options.Kerberos5IR,
                Kerberos5IRw = options.Kerberos5IRw,
                Kerberos5PR = options.Kerberos5PR,
                Kerberos5PRw = options.Kerberos5PRw,
                KeyVaultPrivateEndpointResourceId = options.KeyVaultPrivateEndpointResourceId,
                LdapEnabled = options.LdapEnabled,
                NetworkFeatures = options.NetworkFeatures,
                PlacementRules = ParseJsonElementOption(options.PlacementRules, nameof(options.PlacementRules)),
                PolicyEnforced = options.PolicyEnforced,
                ProximityPlacementGroup = options.ProximityPlacementGroup,
                RelocationRequested = options.RelocationRequested,
                RemoteVolumeResourceId = options.RemoteVolumeResourceId,
                RemoteVolumeRegion = options.RemoteVolumeRegion,
                ReplicationSchedule = options.ReplicationSchedule,
                RuleIndex = options.RuleIndex,
                SecurityStyle = options.SecurityStyle,
                SmbAccessEnumeration = options.SmbAccessEnumeration,
                SmbContinuouslyAvailable = options.SmbContinuouslyAvailable,
                SmbEncryption = options.SmbEncryption,
                SmbNonBrowsable = options.SmbNonBrowsable,
                SnapshotDirectoryVisible = options.SnapshotDirectoryVisible,
                SnapshotId = options.SnapshotId,
                SnapshotPolicyId = options.SnapshotPolicyId,
                Tags = tags,
                ThroughputMibps = options.ThroughputMibps,
                UnixPermissions = options.UnixPermissions,
                UnixReadOnly = options.UnixReadOnly,
                UnixReadWrite = options.UnixReadWrite,
                VolumeSpecName = options.VolumeSpecName,
                VolumeType = options.VolumeType,
                Zones = options.Zones?.ToList()
            };

            var volume = await _netAppFilesService.CreateVolume(
                options.Account!,
                options.Pool!,
                options.Volume!,
                options.ResourceGroup!,
                options.Location!,
                options.Subscription!,
                createParameters,
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
                "Error creating NetApp Files volume. Volume: {Volume}, Account: {Account}, Pool: {Pool}",
                options.Volume, options.Account, options.Pool);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A volume with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the volume. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Account, pool, or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    private static void ValidateUnsupportedArguments(VolumeCreateOptions options)
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

    private static JsonElement? ParseJsonElementOption(string? value, string optionName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize(value, NetAppFilesJsonContext.Default.JsonElement);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format for {optionName}: {ex.Message}", optionName, ex);
        }
    }

    internal record VolumeCreateCommandResult([property: JsonPropertyName("volume")] NetAppVolumeCreateResult Volume);
}
