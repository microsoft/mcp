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
    private readonly Option<string> _maintenanceDayOption = AzureManagedLustreOptionDefinitions.OptionalMaintenanceDayOption;
    private readonly Option<string> _maintenanceTimeOption = AzureManagedLustreOptionDefinitions.OptionalMaintenanceTimeOption;
    private readonly Option<string> _noSquashNidListsOption = AzureManagedLustreOptionDefinitions.NoSquashNidListsOption;
    private readonly Option<long?> _squashUidOption = AzureManagedLustreOptionDefinitions.SquashUidOption;
    private readonly Option<long?> _squashGidOption = AzureManagedLustreOptionDefinitions.SquashGidOption;
    private readonly Option<string> _rootSquashModeOption = AzureManagedLustreOptionDefinitions.RootSquashModeOption;

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
        command.AddOption(_maintenanceDayOption);
        command.AddOption(_maintenanceTimeOption);
        command.AddOption(_noSquashNidListsOption);
        command.AddOption(_squashUidOption);
        command.AddOption(_squashGidOption);
        command.AddOption(_rootSquashModeOption);
    }

    protected override FileSystemUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueForOption(_nameOption);
        options.MaintenanceDay = parseResult.GetValueForOption(_maintenanceDayOption);
        options.MaintenanceTime = parseResult.GetValueForOption(_maintenanceTimeOption);
        options.RootSquashMode = parseResult.GetValueForOption(_rootSquashModeOption);
        options.NoSquashNidLists = parseResult.GetValueForOption(_noSquashNidListsOption);
        options.SquashUid = parseResult.GetValueForOption(_squashUidOption);
        options.SquashGid = parseResult.GetValueForOption(_squashGidOption);
        return options;
    }
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid ||
                !base.ValidateRootSquashOptions(parseResult.CommandResult, context.Response).IsValid ||
                !base.ValidateMaintenanceOptions(parseResult.CommandResult, context.Response, true).IsValid)
            {
                return context.Response;
            }

            if (string.IsNullOrWhiteSpace(options.MaintenanceDay) &&
                string.IsNullOrWhiteSpace(options.MaintenanceTime) &&
                string.IsNullOrWhiteSpace(options.RootSquashMode))
            {
                context.Response.Status = 400;
                context.Response.Message = "At least one of maintenance-day/time or root-squash fields must be provided.";
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
