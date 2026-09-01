// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
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
public sealed class VolumeCreateCommand(ILogger<VolumeCreateCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VolumeCreateOptions, VolumeCreateCommand.VolumeCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<VolumeCreateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, VolumeCreateOptions options, CancellationToken cancellationToken)
    {
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

    public record VolumeCreateCommandResult([property: JsonPropertyName("volume")] NetAppVolumeCreateResult Volume);
}
