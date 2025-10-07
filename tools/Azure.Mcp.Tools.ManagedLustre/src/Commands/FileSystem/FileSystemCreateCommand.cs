// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;

public sealed class FileSystemCreateCommand(ILogger<FileSystemCreateCommand> logger)
    : BaseManagedLustreCommand<FileSystemCreateOptions>(logger)
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
        command.Options.Add(ManagedLustreOptionDefinitions.NameOption);
        command.Options.Add(ManagedLustreOptionDefinitions.LocationOption);
        command.Options.Add(ManagedLustreOptionDefinitions.SkuOption);
        command.Options.Add(ManagedLustreOptionDefinitions.SizeOption);
        command.Options.Add(ManagedLustreOptionDefinitions.SubnetIdOption);
        command.Options.Add(ManagedLustreOptionDefinitions.ZoneOption);
        command.Options.Add(ManagedLustreOptionDefinitions.MaintenanceDayOption);
        command.Options.Add(ManagedLustreOptionDefinitions.MaintenanceTimeOption);
        command.Options.Add(ManagedLustreOptionDefinitions.HsmContainerOption);
        command.Options.Add(ManagedLustreOptionDefinitions.HsmLogContainerOption);
        command.Options.Add(ManagedLustreOptionDefinitions.ImportPrefixOption);
        command.Options.Add(ManagedLustreOptionDefinitions.RootSquashModeOption);
        command.Options.Add(ManagedLustreOptionDefinitions.NoSquashNidListsOption);
        command.Options.Add(ManagedLustreOptionDefinitions.SquashUidOption);
        command.Options.Add(ManagedLustreOptionDefinitions.SquashGidOption);
        command.Options.Add(ManagedLustreOptionDefinitions.CustomEncryptionOption);
        command.Options.Add(ManagedLustreOptionDefinitions.KeyUrlOption);
        command.Options.Add(ManagedLustreOptionDefinitions.SourceVaultOption);
        command.Options.Add(ManagedLustreOptionDefinitions.UserAssignedIdentityIdOption);
        command.Validators.Add(ValidateRootSquashOptions);
        command.Validators.Add(ValidateMaintanenceOptionsCreate);
        command.Validators.Add(ValidateEncryptionOptions);
        command.Validators.Add(ValidateHSMOptions);
    }

    protected override FileSystemCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Name = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.NameOption.Name);
        options.Location = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.LocationOption.Name);
        options.Sku = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.SkuOption.Name);
        options.SizeTiB = parseResult.GetValueOrDefault<int>(ManagedLustreOptionDefinitions.SizeOption.Name);
        options.SubnetId = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.SubnetIdOption.Name);
        options.Zone = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.ZoneOption.Name);
        options.HsmContainer = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.HsmContainerOption.Name);
        options.HsmLogContainer = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.HsmLogContainerOption.Name);
        options.ImportPrefix = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.ImportPrefixOption.Name);
        options.MaintenanceDay = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.MaintenanceDayOption.Name);
        options.MaintenanceTime = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.MaintenanceTimeOption.Name);
        options.RootSquashMode = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.RootSquashModeOption.Name);
        options.NoSquashNidLists = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.NoSquashNidListsOption.Name);
        options.SquashUid = parseResult.GetValueOrDefault<long?>(ManagedLustreOptionDefinitions.SquashUidOption.Name);
        options.SquashGid = parseResult.GetValueOrDefault<long?>(ManagedLustreOptionDefinitions.SquashGidOption.Name);
        options.EnableCustomEncryption = parseResult.GetValueOrDefault<bool>(ManagedLustreOptionDefinitions.CustomEncryptionOption.Name);
        options.KeyUrl = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.KeyUrlOption.Name);
        options.SourceVaultId = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.SourceVaultOption.Name);
        options.UserAssignedIdentityId = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.UserAssignedIdentityIdOption.Name);
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

            var svc = context.GetService<IManagedLustreService>();
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

            context.Response.Results = ResponseResult.Create(new(fs), ManagedLustreJsonContext.Default.FileSystemCreateResult);
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
