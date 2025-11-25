// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoimportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoimportJob;

public sealed class AutoimportJobCreateCommand(ILogger<AutoimportJobCreateCommand> logger)
    : BaseManagedLustreCommand<AutoimportJobCreateOptions>(logger)
{
    private const string CommandTitle = "Create Azure Managed Lustre Autoimport Job";

    private new readonly ILogger<AutoimportJobCreateCommand> _logger = logger;

    public override string Id => "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d";

    public override string Name => "create";

    public override string Description =>
        """
        Creates an auto import job for an Azure Managed Lustre filesystem to continuously import new or modified files from the linked blob storage container. The auto import job syncs changes from the configured HSM blob container to the Lustre filesystem. Use this to keep the filesystem updated with changes in blob storage.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        Optional parameters:
        - job-name: Custom name for the job (default: autoimport-{timestamp})
        - conflict-resolution-mode: How to handle conflicts (Fail/Skip/OverwriteIfDirty/OverwriteAlways, default: Skip)
        - autoimport-prefixes: Array of blob paths/prefixes to auto import (default: '/', max: 100)
        - admin-status: Administrative status (Enable/Disable, default: Enable)
        - enable-deletions: Enable deletions during auto import (default: false)
        - maximum-errors: Max errors before failure (-1: infinite, 0: immediate exit, default: none)
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
        command.Options.Add(ManagedLustreOptionDefinitions.FileSystemNameOption);
        command.Options.Add(ManagedLustreOptionDefinitions.OptionalJobNameOption);
        command.Options.Add(ManagedLustreOptionDefinitions.ConflictResolutionModeOption);
        command.Options.Add(ManagedLustreOptionDefinitions.AutoimportPrefixesOption);
        command.Options.Add(ManagedLustreOptionDefinitions.AdminStatusOption);
        command.Options.Add(ManagedLustreOptionDefinitions.EnableDeletionsOption);
        command.Options.Add(ManagedLustreOptionDefinitions.MaximumErrorsOption);
    }

    protected override AutoimportJobCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileSystemName = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.FileSystemNameOption.Name);
        options.JobName = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.OptionalJobNameOption.Name);
        options.ConflictResolutionMode = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.ConflictResolutionModeOption.Name);
        options.AutoimportPrefixes = parseResult.GetValueOrDefault<string[]>(ManagedLustreOptionDefinitions.AutoimportPrefixesOption.Name);
        options.AdminStatus = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.AdminStatusOption.Name);
        options.EnableDeletions = parseResult.GetValueOrDefault<bool?>(ManagedLustreOptionDefinitions.EnableDeletionsOption.Name);
        options.MaximumErrors = parseResult.GetValueOrDefault<long?>(ManagedLustreOptionDefinitions.MaximumErrorsOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var svc = context.GetService<IManagedLustreService>();
            
            // Log the prefixes for debugging
            if (options.AutoimportPrefixes != null && options.AutoimportPrefixes.Length > 0)
            {
                _logger.LogInformation("Autoimport prefixes received: {Prefixes}", string.Join(", ", options.AutoimportPrefixes));
            }
            else
            {
                _logger.LogInformation("No autoimport prefixes received, will use default");
            }
            
            var job = await svc.CreateAutoimportJobAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileSystemName!,
                options.JobName,
                options.ConflictResolutionMode,
                options.AutoimportPrefixes,
                options.AdminStatus,
                options.EnableDeletions,
                options.MaximumErrors,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(job), ManagedLustreJsonContext.Default.AutoimportJobCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating autoimport archive job for AMLFS filesystem {FileSystem}. Options: {@Options}",
                options.FileSystemName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AutoimportJobCreateResult(string JobName);
}
