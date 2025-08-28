// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.AzureManagedLustre.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;

public sealed class FileSystemCreateCommand(ILogger<FileSystemCreateCommand> logger)
    : BaseAzureManagedLustreCommand<FileSystemCreateOptions>(logger)
{
    private const string CommandTitle = "Create Azure Managed Lustre FileSystem";

    private new readonly ILogger<FileSystemCreateCommand> _logger = logger;

    private readonly Option<string> _nameOption = AzureManagedLustreOptionDefinitions.NameOption;
    private readonly Option<string> _locationOption = AzureManagedLustreOptionDefinitions.LocationOption;
    private readonly Option<string> _skuOption = AzureManagedLustreOptionDefinitions.SkuOption;
    private readonly Option<int> _sizeOption = AzureManagedLustreOptionDefinitions.SizeOption;
    private readonly Option<string> _subnetIdOption = AzureManagedLustreOptionDefinitions.SubnetIdOption;
    private readonly Option<string> _zoneOption = AzureManagedLustreOptionDefinitions.ZoneOption;

    private readonly Option<string> _hsmContainerOption = AzureManagedLustreOptionDefinitions.HsmContainerOption;
    private readonly Option<string> _hsmLogContainerOption = AzureManagedLustreOptionDefinitions.HsmLogContainerOption;
    private readonly Option<string> _importPrefixOption = AzureManagedLustreOptionDefinitions.ImportPrefixOption;

    private readonly Option<string> _maintenanceDayOption = AzureManagedLustreOptionDefinitions.MaintenanceDayOption;

    private readonly Option<string> _maintenanceTimeOption = AzureManagedLustreOptionDefinitions.MaintenanceTimeOption;

    private readonly Option<string> _rootSquashModeOption = AzureManagedLustreOptionDefinitions.RootSquashModeOption;
    private readonly Option<string> _noSquashNidListsOption = AzureManagedLustreOptionDefinitions.NoSquashNidListsOption;
    private readonly Option<long?> _squashUidOption = AzureManagedLustreOptionDefinitions.SquashUidOption;
    private readonly Option<long?> _squashGidOption = AzureManagedLustreOptionDefinitions.SquashGidOption;

    private readonly Option<bool> _customEncryptionOption = AzureManagedLustreOptionDefinitions.CustomEncryptionOption;
    private readonly Option<string> _keyUrlOption = AzureManagedLustreOptionDefinitions.KeyUrlOption;
    private readonly Option<string> _sourceVaultOption = AzureManagedLustreOptionDefinitions.SourceVaultOption;
    private readonly Option<string> _userAssignedIdentityIdOption = AzureManagedLustreOptionDefinitions.UserAssignedIdentityIdOption;

    public override string Name => "create";

    public override string Description =>
        """
        Create an Azure Managed Lustre (AMLFS) file system using the specified network, capacity, maintenance window and availability zone.
        Optionally provides possibility to define Blob Integration, customer managed key encryption and root squash configuration.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RequireResourceGroup();
        command.Options.Add(_nameOption);
        command.Options.Add(_locationOption);
        command.Options.Add(_skuOption);
        command.Options.Add(_sizeOption);
        command.Options.Add(_subnetIdOption);
        command.Options.Add(_zoneOption);
        command.Options.Add(_maintenanceDayOption);
        command.Options.Add(_maintenanceTimeOption);
        command.Options.Add(_hsmContainerOption);
        command.Options.Add(_hsmLogContainerOption);
        command.Options.Add(_importPrefixOption);
        command.Options.Add(_rootSquashModeOption);
        command.Options.Add(_noSquashNidListsOption);
        command.Options.Add(_squashUidOption);
        command.Options.Add(_squashGidOption);
        command.Options.Add(_customEncryptionOption);
        command.Options.Add(_keyUrlOption);
        command.Options.Add(_sourceVaultOption);
        command.Options.Add(_userAssignedIdentityIdOption);
    }

    protected override FileSystemCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueOrDefault(_nameOption);
        options.Location = parseResult.GetValueOrDefault(_locationOption);
        options.Sku = parseResult.GetValueOrDefault(_skuOption);
        options.SizeTiB = parseResult.GetValueOrDefault(_sizeOption);
        options.SubnetId = parseResult.GetValueOrDefault(_subnetIdOption);
        options.Zone = parseResult.GetValueOrDefault(_zoneOption);
        options.HsmContainer = parseResult.GetValueOrDefault(_hsmContainerOption);
        options.HsmLogContainer = parseResult.GetValueOrDefault(_hsmLogContainerOption);
        options.ImportPrefix = parseResult.GetValueOrDefault(_importPrefixOption);
        options.MaintenanceDay = parseResult.GetValueOrDefault(_maintenanceDayOption);
        options.MaintenanceTime = parseResult.GetValueOrDefault(_maintenanceTimeOption);
        options.RootSquashMode = parseResult.GetValueOrDefault(_rootSquashModeOption);
        options.NoSquashNidLists = parseResult.GetValueOrDefault(_noSquashNidListsOption);
        options.SquashUid = parseResult.GetValueOrDefault(_squashUidOption);
        options.SquashGid = parseResult.GetValueOrDefault(_squashGidOption);
        options.EnableCustomEncryption = parseResult.GetValueOrDefault(_customEncryptionOption);
        options.KeyUrl = parseResult.GetValueOrDefault(_keyUrlOption);
        options.SourceVaultId = parseResult.GetValueOrDefault(_sourceVaultOption);
        options.UserAssignedIdentityId = parseResult.GetValueOrDefault(_userAssignedIdentityIdOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid ||
                !base.ValidateRootSquashOptions(parseResult.CommandResult, context.Response).IsValid ||
                !base.ValidateMaintenanceOptions(parseResult.CommandResult, context.Response).IsValid ||
                !base.ValidateEncryptionOptions(parseResult.CommandResult, context.Response).IsValid ||
                !base.ValidateHSMOptions(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var options = BindOptions(parseResult);

            var svc = context.GetService<IAzureManagedLustreService>();
            var fs = await svc.CreateFileSystemAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.Name!,
                options.Location!,
                options.Sku!,
                options.SizeTiB!.Value,
                options.SubnetId!,
                options.Zone!,
                options.MaintenanceDay!,
                options.MaintenanceTime!,
                options.HsmContainer,
                options.HsmLogContainer,
                options.ImportPrefix,
                options.RootSquashMode,
                options.NoSquashNidLists,
                options.SquashUid,
                options.SquashGid,
                options.EnableCustomEncryption ?? false,
                options.KeyUrl,
                options.SourceVaultId,
                options.UserAssignedIdentityId,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new FileSystemCreateResult(fs), AzureManagedLustreJsonContext.Default.FileSystemCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AMLFS.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record FileSystemCreateResult(Models.LustreFileSystem FileSystem);
}
