// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;

public sealed class FileSystemUpdateCommand(ILogger<FileSystemUpdateCommand> logger)
    : BaseManagedLustreCommand<FileSystemUpdateOptions>(logger)
{
    private const string CommandTitle = "Update Azure Managed Lustre FileSystem";

    private new readonly ILogger<FileSystemUpdateCommand> _logger = logger;

    public override string Name => "update";

    public override string Description =>
        """
        Update maintenance window and/or root squash settings of an existing Azure Managed Lustre (AMLFS) file system. Provide either maintenance day and time or root squash fields (no-squash-nid-list, squash-uid, squash-gid). Root squash fields must be provided if root squash is not None. In case of maintenance window update, both maintenance day and maintenance time should be provided.
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

        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(ManagedLustreOptionDefinitions.NameOption);
        command.Options.Add(ManagedLustreOptionDefinitions.OptionalMaintenanceDayOption);
        command.Options.Add(ManagedLustreOptionDefinitions.OptionalMaintenanceTimeOption);
        command.Options.Add(ManagedLustreOptionDefinitions.NoSquashNidListsOption);
        command.Options.Add(ManagedLustreOptionDefinitions.SquashUidOption);
        command.Options.Add(ManagedLustreOptionDefinitions.SquashGidOption);
        command.Options.Add(ManagedLustreOptionDefinitions.RootSquashModeOption);
        command.Validators.Add(ValidateRootSquashOptions);
        command.Validators.Add(ValidateMaintanenceOptionsUpdate);
        command.Validators.Add(ValidateEncryptionOptions);
        command.Validators.Add(ValidateHSMOptions);
        command.Validators.Add(ValidateHasUpdateOptions);
    }

    protected override FileSystemUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Name = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.NameOption.Name);
        options.MaintenanceDay = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.OptionalMaintenanceDayOption.Name);
        options.MaintenanceTime = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.OptionalMaintenanceTimeOption.Name);
        options.RootSquashMode = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.RootSquashModeOption.Name);
        options.NoSquashNidLists = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.NoSquashNidListsOption.Name);
        options.SquashUid = parseResult.GetValueOrDefault<long?>(ManagedLustreOptionDefinitions.SquashUidOption.Name);
        options.SquashGid = parseResult.GetValueOrDefault<long?>(ManagedLustreOptionDefinitions.SquashGidOption.Name);
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
            var fs = await svc.UpdateFileSystemAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.Name!,
                options.MaintenanceDay,
                options.MaintenanceTime,
                options.RootSquashMode,
                options.NoSquashNidLists,
                options.SquashUid,
                options.SquashGid,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new FileSystemUpdateResult(fs), ManagedLustreJsonContext.Default.FileSystemUpdateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AMLFS.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record FileSystemUpdateResult(Models.LustreFileSystem FileSystem);
}
