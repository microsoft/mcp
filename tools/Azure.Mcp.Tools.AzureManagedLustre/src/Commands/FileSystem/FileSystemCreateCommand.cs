// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
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
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(AzureManagedLustreOptionDefinitions.NameOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.LocationOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.SkuOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.SizeOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.SubnetIdOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.ZoneOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.MaintenanceDayOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.MaintenanceTimeOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.HsmContainerOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.HsmLogContainerOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.ImportPrefixOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.RootSquashModeOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.NoSquashNidListsOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.SquashUidOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.SquashGidOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.CustomEncryptionOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.KeyUrlOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.SourceVaultOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.UserAssignedIdentityIdOption);
        command.Validators.Add(ValidateRootSquashOptions);
        command.Validators.Add(ValidateMaintanenceOptionsCreate);
        command.Validators.Add(ValidateEncryptionOptions);
        command.Validators.Add(ValidateHSMOptions);
    }

    protected override FileSystemCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Name = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.NameOption.Name);
        options.Location = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.LocationOption.Name);
        options.Sku = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.SkuOption.Name);
        options.SizeTiB = parseResult.GetValueOrDefault<int>(AzureManagedLustreOptionDefinitions.SizeOption.Name);
        options.SubnetId = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.SubnetIdOption.Name);
        options.Zone = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.ZoneOption.Name);
        options.HsmContainer = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.HsmContainerOption.Name);
        options.HsmLogContainer = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.HsmLogContainerOption.Name);
        options.ImportPrefix = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.ImportPrefixOption.Name);
        options.MaintenanceDay = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.MaintenanceDayOption.Name);
        options.MaintenanceTime = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.MaintenanceTimeOption.Name);
        options.RootSquashMode = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.RootSquashModeOption.Name);
        options.NoSquashNidLists = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.NoSquashNidListsOption.Name);
        options.SquashUid = parseResult.GetValueOrDefault<long?>(AzureManagedLustreOptionDefinitions.SquashUidOption.Name);
        options.SquashGid = parseResult.GetValueOrDefault<long?>(AzureManagedLustreOptionDefinitions.SquashGidOption.Name);
        options.EnableCustomEncryption = parseResult.GetValueOrDefault<bool>(AzureManagedLustreOptionDefinitions.CustomEncryptionOption.Name);
        options.KeyUrl = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.KeyUrlOption.Name);
        options.SourceVaultId = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.SourceVaultOption.Name);
        options.UserAssignedIdentityId = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.UserAssignedIdentityIdOption.Name);
        return options;
    }


    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
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

            context.Response.Results = ResponseResult.Create(new(fs), AzureManagedLustreJsonContext.Default.FileSystemCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AMLFS.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record FileSystemCreateResult(Models.LustreFileSystem FileSystem);
}
