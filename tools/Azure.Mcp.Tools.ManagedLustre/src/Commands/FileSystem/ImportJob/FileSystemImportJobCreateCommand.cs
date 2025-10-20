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

public sealed class FileSystemImportJobCreateCommand(ILogger<FileSystemImportJobCreateCommand> logger)
    : BaseManagedLustreCommand<FileSystemImportJobCreateOptions>(logger)
{
    private const string CommandTitle = "Create AMLFS Import Job";

    public override string Name => "create";

    public override string Description =>
        """
        Creates a manual import job for an Azure Managed Lustre (AMLFS) file system. The import job scans the linked HSM/Blob container and imports specified path prefixes (or all when omitted) honoring the chosen conflict resolution mode. Use to hydrate the AMLFS namespace or refresh content.
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
        // Required common option
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        // Service-specific options
        command.Options.Add(ManagedLustreOptionDefinitions.FileSystemOption);
        command.Options.Add(ManagedLustreOptionDefinitions.ImportPrefixesOption);
        command.Options.Add(ManagedLustreOptionDefinitions.ConflictResolutionModeOption);
        command.Options.Add(ManagedLustreOptionDefinitions.MaximumErrorsOption);
        command.Options.Add(ManagedLustreOptionDefinitions.JobNameOption);

        // Validation for conflict resolution mode (Skip|Fail) – consistent with validator style in SubnetSizeAskCommand
        command.Validators.Add(cmdResult =>
        {
            if (cmdResult.TryGetValue(ManagedLustreOptionDefinitions.ConflictResolutionModeOption, out var mode)
                && !string.IsNullOrWhiteSpace(mode)
                && !string.Equals(mode, "Skip", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(mode, "Fail", StringComparison.OrdinalIgnoreCase))
            {
                cmdResult.AddError("Invalid conflict resolution mode. Allowed values: Skip, Fail.");
            }
        });
    }

    protected override FileSystemImportJobCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.FileSystem = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.FileSystemOption.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        var prefixes = parseResult.GetValueOrDefault<string[]>(ManagedLustreOptionDefinitions.ImportPrefixesOption.Name);
        if (prefixes == null || prefixes.Length == 0)
        {
            options.ImportPrefixes = new List<string> { "/" };
        }
        else
        {
            options.ImportPrefixes = prefixes.ToList();
        }
        var conflictMode = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.ConflictResolutionModeOption.Name);
        conflictMode = string.IsNullOrWhiteSpace(conflictMode)
            ? "Skip"
            : char.ToUpperInvariant(conflictMode[0]) + conflictMode.Substring(1).ToLowerInvariant();
        options.ConflictResolutionMode = conflictMode;
        options.MaximumErrors = parseResult.GetValueOrDefault<int?>(ManagedLustreOptionDefinitions.MaximumErrorsOption.Name) ?? -1;
        options.AdminStatus = "Active"; // Hard-coded since service no longer accepts parameter
        options.Name = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.JobNameOption.Name);
        return options;
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

            var svc = context.GetService<IManagedLustreService>();
            var result = await svc.CreateImportJobAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileSystem!,
                options.Name,
                options.ImportPrefixes,
                options.ConflictResolutionMode!,
                options.MaximumErrors,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new FileSystemImportJobCreateResult(result),
                ManagedLustreJsonContext.Default.FileSystemImportJobCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating AMLFS import job. FileSystem: {FileSystem} ResourceGroup: {ResourceGroup} Options: {@Options}",
                options.FileSystem, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record FileSystemImportJobCreateResult(Models.ImportJobInfo ImportJob);
}
