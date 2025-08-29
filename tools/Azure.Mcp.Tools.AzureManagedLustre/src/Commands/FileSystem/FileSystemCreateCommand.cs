// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
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

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_nameOption);
        RequireResourceGroup();
        command.AddOption(_locationOption);
        command.AddOption(_skuOption);
        command.AddOption(_sizeOption);
        command.AddOption(_subnetIdOption);
        command.AddOption(_zoneOption);
        command.AddOption(_maintenanceDayOption);
        command.AddOption(_maintenanceTimeOption);
        command.AddOption(_hsmContainerOption);
        command.AddOption(_hsmLogContainerOption);
        command.AddOption(_importPrefixOption);
        command.AddOption(_rootSquashModeOption);
        command.AddOption(_noSquashNidListsOption);
        command.AddOption(_squashUidOption);
        command.AddOption(_squashGidOption);
        command.AddOption(_customEncryptionOption);
        command.AddOption(_keyUrlOption);
        command.AddOption(_sourceVaultOption);
        command.AddOption(_userAssignedIdentityIdOption);
    }

    protected override FileSystemCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueForOption(_nameOption);
        options.Location = parseResult.GetValueForOption(_locationOption);
        options.Sku = parseResult.GetValueForOption(_skuOption);
        options.SizeTiB = parseResult.GetValueForOption(_sizeOption);
        options.SubnetId = parseResult.GetValueForOption(_subnetIdOption);
        options.Zone = parseResult.GetValueForOption(_zoneOption);
        options.HsmContainer = parseResult.GetValueForOption(_hsmContainerOption);
        options.HsmLogContainer = parseResult.GetValueForOption(_hsmLogContainerOption);
        options.ImportPrefix = parseResult.GetValueForOption(_importPrefixOption);
        options.MaintenanceDay = parseResult.GetValueForOption(_maintenanceDayOption);
        options.MaintenanceTime = parseResult.GetValueForOption(_maintenanceTimeOption);
        options.RootSquashMode = parseResult.GetValueForOption(_rootSquashModeOption);
        options.NoSquashNidLists = parseResult.GetValueForOption(_noSquashNidListsOption);
        options.SquashUid = parseResult.GetValueForOption(_squashUidOption);
        options.SquashGid = parseResult.GetValueForOption(_squashGidOption);
        options.EnableCustomEncryption = parseResult.GetValueForOption(_customEncryptionOption);
        options.KeyUrl = parseResult.GetValueForOption(_keyUrlOption);
        options.SourceVaultId = parseResult.GetValueForOption(_sourceVaultOption);
        options.UserAssignedIdentityId = parseResult.GetValueForOption(_userAssignedIdentityIdOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        // Root squash validation: default None; if not None, require the other params
        var mode = options.RootSquashMode;
        if (!string.IsNullOrWhiteSpace(mode) && !mode.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(options.NoSquashNidLists) || options.SquashUid is null || options.SquashGid is null)
            {
                context.Response.Status = 400;
                context.Response.Message = "When root-squash-mode is not 'None', you must provide no-squash-nid-list, squash-uid and squash-gid.";
                return context.Response;
            }
        }
        if (options.EnableCustomEncryption == true)
        {
            if (string.IsNullOrWhiteSpace(options.KeyUrl) || string.IsNullOrWhiteSpace(options.SourceVaultId))
            {
                context.Response.Status = 400;
                context.Response.Message = "Missing Required options: key-url, source-vault when custom-encryption is set";
                return context.Response;
            }
        }

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

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
            _logger.LogError(ex, "Error creating AMLFS. Options: {@Options}", options);
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
