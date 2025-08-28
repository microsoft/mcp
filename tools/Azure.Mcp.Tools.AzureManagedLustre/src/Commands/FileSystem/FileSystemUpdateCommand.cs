// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.AzureManagedLustre.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.Logging;

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

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_nameOption);
        RequireResourceGroup();
        // All update fields are optional, we only patch those provided
        command.Options.Add(_maintenanceDayOption);
        command.Options.Add(_maintenanceTimeOption);
        command.Options.Add(_noSquashNidListsOption);
        command.Options.Add(_squashUidOption);
        command.Options.Add(_squashGidOption);
        command.Options.Add(_rootSquashModeOption);
    }

    protected override FileSystemUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueOrDefault(_nameOption);
        options.MaintenanceDay = parseResult.GetValueOrDefault(_maintenanceDayOption);
        options.MaintenanceTime = parseResult.GetValueOrDefault(_maintenanceTimeOption);
        options.RootSquashMode = parseResult.GetValueOrDefault(_rootSquashModeOption);
        options.NoSquashNidLists = parseResult.GetValueOrDefault(_noSquashNidListsOption);
        options.SquashUid = parseResult.GetValueOrDefault(_squashUidOption);
        options.SquashGid = parseResult.GetValueOrDefault(_squashGidOption);
        return options;
    }
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid ||
                !base.ValidateRootSquashOptions(parseResult.CommandResult, context.Response).IsValid ||
                !base.ValidateMaintenanceOptions(parseResult.CommandResult, context.Response, true).IsValid)
            {
                return context.Response;
            }
            var options = BindOptions(parseResult);

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
            _logger.LogError(ex, "Error updating AMLFS.");
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
