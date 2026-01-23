// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.ImportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ImportJob;

public sealed class ImportJobCreateCommand(ILogger<ImportJobCreateCommand> logger)
    : BaseManagedLustreCommand<ImportJobCreateOptions>(logger)
{
    private const string CommandTitle = "Create Azure Managed Lustre Import Job";

    private new readonly ILogger<ImportJobCreateCommand> _logger = logger;

    public override string Id => "b1f3c5e7-9d2a-4b8f-6c3e-1a7b9d2f5e8c";

    public override string Name => "create";

    public override string Description =>
        """
        Creates a one-time import job for an Azure Managed Lustre filesystem to import files from the linked blob storage container. The import job performs a one-time sync of data from the configured HSM blob container to the Lustre filesystem. Use this to import specific prefixes or all data from blob storage into the filesystem at a point in time.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        Optional options:
        - job-name: Name for the import job (auto-generated if not provided)
        - conflict-resolution-mode: How to handle conflicting files (Fail, Skip, OverwriteIfDirty, OverwriteAlways, default: Fail)
        - import-prefixes: Blob prefixes to import (default: imports all data from root '/')
        - maximum-errors: Maximum errors allowed before job failure (-1: infinite, 0: fail on first error, default: use service default)
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
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
        command.Options.Add(ManagedLustreOptionDefinitions.FileSystemNameOption.AsRequired());
        command.Options.Add(ManagedLustreOptionDefinitions.OptionalJobNameOption);
        command.Options.Add(ManagedLustreOptionDefinitions.ConflictResolutionModeOption);
        command.Options.Add(ManagedLustreOptionDefinitions.ImportPrefixesOption);
        command.Options.Add(ManagedLustreOptionDefinitions.MaximumErrorsOption);
    }

    protected override ImportJobCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileSystemName = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.FileSystemNameOption.Name);
        options.JobName = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.OptionalJobNameOption.Name);
        options.ConflictResolutionMode = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.ConflictResolutionModeOption.Name);
        options.ImportPrefixes = parseResult.GetValueOrDefault<string[]>(ManagedLustreOptionDefinitions.ImportPrefixesOption.Name);
        options.MaximumErrors = parseResult.GetValueOrDefault<long?>(ManagedLustreOptionDefinitions.MaximumErrorsOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var svc = context.GetService<IManagedLustreService>();

            // Log the prefixes for debugging
            if (options.ImportPrefixes != null && options.ImportPrefixes.Length > 0)
            {
                _logger.LogInformation("Import prefixes received: {Prefixes}", string.Join(", ", options.ImportPrefixes));
            }
            else
            {
                _logger.LogInformation("No import prefixes received, will import all data");
            }

            var job = await svc.CreateImportJobAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileSystemName!,
                options.JobName,
                options.ConflictResolutionMode,
                options.ImportPrefixes,
                options.MaximumErrors,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(job), ManagedLustreJsonContext.Default.ImportJobCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating import job for AMLFS filesystem {FileSystem}. Options: {@Options}",
                options.FileSystemName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ImportJobCreateResult(string JobName);
}
