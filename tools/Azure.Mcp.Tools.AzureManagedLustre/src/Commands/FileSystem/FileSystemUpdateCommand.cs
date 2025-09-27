// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
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

    public override string Name => "update";

    public override string Description =>
        """
        Update maintenance window and/or root squash settings of an existing Azure Managed Lustre (AMLFS) file system. Provide either maintenance-day/time or root squash fields (no-squash-nid-list, squash-uid, squash-gid). Root squash fields must be provided if root squash is not None must be provided together.
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

    public override ValidationResult Validate(System.CommandLine.Parsing.CommandResult commandResult, CommandResponse? response)
    {
        var validationSteps = new List<Func<System.CommandLine.Parsing.CommandResult, CommandResponse?, ValidationResult>>
        {
            base.Validate,
            ValidateRootSquashOptions,
            ValidateMaintanenceOptionsUpdate,
            ValidateEncryptionOptions,
            ValidateHSMOptions
        };
        var result = new ValidationResult { IsValid = true };
        foreach (var step in validationSteps)
        {
            result = step(commandResult, response);
            if (!result.IsValid)
            {
                break;
            }
        }
        return result;
    }

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(AzureManagedLustreOptionDefinitions.NameOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.OptionalMaintenanceDayOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.OptionalMaintenanceTimeOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.NoSquashNidListsOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.SquashUidOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.SquashGidOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.RootSquashModeOption);
    }

    protected override FileSystemUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Name = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.NameOption.Name);
        options.MaintenanceDay = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.OptionalMaintenanceDayOption.Name);
        options.MaintenanceTime = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.OptionalMaintenanceTimeOption.Name);
        options.RootSquashMode = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.RootSquashModeOption.Name);
        options.NoSquashNidLists = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.NoSquashNidListsOption.Name);
        options.SquashUid = parseResult.GetValueOrDefault<long?>(AzureManagedLustreOptionDefinitions.SquashUidOption.Name);
        options.SquashGid = parseResult.GetValueOrDefault<long?>(AzureManagedLustreOptionDefinitions.SquashGidOption.Name);
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

            if (string.IsNullOrWhiteSpace(options.MaintenanceDay) &&
                string.IsNullOrWhiteSpace(options.MaintenanceTime) &&
                string.IsNullOrWhiteSpace(options.RootSquashMode))
            {
                context.Response.Status = HttpStatusCode.BadRequest;
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

    internal record FileSystemUpdateResult(Models.LustreFileSystem FileSystem);
}
