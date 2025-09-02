// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AzureManagedLustre.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Azure.ResourceManager.StorageCache.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;

public sealed class FileSystemUpdateCommand(ILogger<FileSystemUpdateCommand> logger)
    : BaseAzureManagedLustreCommand<FileSystemUpdateOptions>(logger)
{
    private const string CommandTitle = "Update Azure Managed Lustre FileSystem";

    private new readonly ILogger<FileSystemUpdateCommand> _logger = logger;

    private readonly Option<string> _nameOption = AzureManagedLustreOptionDefinitions.NameOption;
    private readonly Option<string> _maintenanceDayOption = AzureManagedLustreOptionDefinitions.MaintenanceDayOption;
    private readonly Option<string> _maintenanceTimeOption = AzureManagedLustreOptionDefinitions.MaintenanceTimeOption;
    private readonly Option<string> _noSquashNidListsOption = AzureManagedLustreOptionDefinitions.NoSquashNidListsOption;
    private readonly Option<long?> _squashUidOption = AzureManagedLustreOptionDefinitions.SquashUidOption;
    private readonly Option<long?> _squashGidOption = AzureManagedLustreOptionDefinitions.SquashGidOption;
    private readonly Option<string> _rootSquashModeOption = AzureManagedLustreOptionDefinitions.RootSquashModeOption;

    // Local optional variants for maintenance options (update does not require them)
    private readonly Option<string> _optionalMaintenanceDayOption = new(AzureManagedLustreOptionDefinitions.MaintenanceDayOption.Aliases.ToArray(), AzureManagedLustreOptionDefinitions.MaintenanceDayOption.Description)
    {
        IsRequired = false
    };
    private readonly Option<string> _optionalMaintenanceTimeOption = new(AzureManagedLustreOptionDefinitions.MaintenanceTimeOption.Aliases.ToArray(), AzureManagedLustreOptionDefinitions.MaintenanceTimeOption.Description)
    {
        IsRequired = false
    };

    public override string Name => "update";

    public override string Description =>
        """
        Update maintenance window and/or root squash settings of an existing Azure Managed Lustre (AMLFS) file system. Provide either maintenance-day/time or root squash fields (no-squash-nid-list, squash-uid, squash-gid). Root squash fields must be provided if root squash is not None must be provided together.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_nameOption);
        RequireResourceGroup();
        // All update fields are optional, we only patch those provided
        command.AddOption(_optionalMaintenanceDayOption);
        command.AddOption(_optionalMaintenanceTimeOption);
        command.AddOption(_noSquashNidListsOption);
        command.AddOption(_squashUidOption);
        command.AddOption(_squashGidOption);
        command.AddOption(_rootSquashModeOption);
    }

    protected override FileSystemUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueForOption(_nameOption);
        options.MaintenanceDay = parseResult.GetValueForOption(_optionalMaintenanceDayOption);
        options.MaintenanceTime = parseResult.GetValueForOption(_optionalMaintenanceTimeOption);
        options.RootSquashMode = parseResult.GetValueForOption(_rootSquashModeOption);
        options.NoSquashNidLists = parseResult.GetValueForOption(_noSquashNidListsOption);
        options.SquashUid = parseResult.GetValueForOption(_squashUidOption);
        options.SquashGid = parseResult.GetValueForOption(_squashGidOption);
        return options;
    }

    public override ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        // Read values from the same option instances used during registration
        var maintenanceDay = commandResult.GetValueForOption(_optionalMaintenanceDayOption);
        var maintenanceTime = commandResult.GetValueForOption(_optionalMaintenanceTimeOption);
        var rootSquashMode = commandResult.GetValueForOption(_rootSquashModeOption);
        var noSquashNidLists = commandResult.GetValueForOption(_noSquashNidListsOption);
        var squashUid = commandResult.GetValueForOption(_squashUidOption);
        var squashGid = commandResult.GetValueForOption(_squashGidOption);

        var maintenanceProvided = !string.IsNullOrWhiteSpace(maintenanceDay) || !string.IsNullOrWhiteSpace(maintenanceTime);
        var rootSquashProvided = !string.IsNullOrWhiteSpace(rootSquashMode);

        // Require at least one update parameter
        if (!maintenanceProvided && !rootSquashProvided)
        {
            if (commandResponse is not null)
            {
                commandResponse.Status = 400;
                commandResponse.Message = "Provide at least one update parameter: maintenance-day/time or root squash fields (root-squash-mode, no-squash-nid-list, squash-uid, squash-gid).";
            }
            return new ValidationResult { IsValid = false, ErrorMessage = commandResponse?.Message };
        }

        // If maintenance is being updated, require both fields
        if (maintenanceProvided && (string.IsNullOrWhiteSpace(maintenanceDay) || string.IsNullOrWhiteSpace(maintenanceTime)))
        {
            if (commandResponse is not null)
            {
                commandResponse.Status = 400;
                commandResponse.Message = "When updating maintenance window, both --maintenance-day and --maintenance-time must be specified.";
            }
            return new ValidationResult { IsValid = false, ErrorMessage = commandResponse?.Message };
        }

        // If root squash mode is provided and not 'none', require UID and GID
        if (!string.IsNullOrWhiteSpace(rootSquashMode) && !rootSquashMode.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            if (!(squashUid.HasValue && squashGid.HasValue && !string.IsNullOrWhiteSpace(noSquashNidLists)))
            {
                if (commandResponse is not null)
                {
                    commandResponse.Status = 400;
                    commandResponse.Message = "When --root-squash-mode is not 'None', --squash-uid, --squash-gid and --no-squash-nid-list must be provided.";
                }
                return new ValidationResult { IsValid = false, ErrorMessage = commandResponse?.Message };
            }
        }

        return base.Validate(commandResult, commandResponse);

    }
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var svc = context.GetService<IAzureManagedLustreService>();
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

            context.Response.Results = ResponseResult.Create(new FileSystemUpdateResult(fs), AzureManagedLustreJsonContext.Default.FileSystemUpdateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AMLFS. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record FileSystemUpdateResult(Models.LustreFileSystem FileSystem);
}
