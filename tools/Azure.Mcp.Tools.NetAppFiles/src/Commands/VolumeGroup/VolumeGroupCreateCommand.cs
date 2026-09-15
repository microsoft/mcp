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
public sealed class VolumeGroupCreateCommand(ILogger<VolumeGroupCreateCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VolumeGroupCreateOptions, VolumeGroupCreateCommand.VolumeGroupCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<VolumeGroupCreateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, VolumeGroupCreateOptions options, CancellationToken cancellationToken)
    {
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

    public record VolumeGroupCreateCommandResult([property: JsonPropertyName("volumeGroup")] VolumeGroupCreateResult VolumeGroup);
}
