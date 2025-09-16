// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AzureManagedLustre.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Azure.Mcp.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;

public sealed class FileSystemImportJobCreateCommand(ILogger<FileSystemImportJobCreateCommand> logger)
    : BaseAzureManagedLustreCommand<FileSystemImportJobCreateOptions>(logger)
{
    private const string CommandTitle = "Create AMLFS Import Job";

    private readonly Option<string> _fileSystemOption = AzureManagedLustreOptionDefinitions.FileSystemOption;
    private readonly Option<string[]> _importPrefixesOption = AzureManagedLustreOptionDefinitions.ImportPrefixesOption;
    private readonly Option<string> _conflictResolutionModeOption = AzureManagedLustreOptionDefinitions.ConflictResolutionModeOption;
    private readonly Option<int?> _maximumErrorsOption = AzureManagedLustreOptionDefinitions.MaximumErrorsOption;
    private readonly Option<string> _nameOption = AzureManagedLustreOptionDefinitions.NameOption;

    public override string Name => "create";

    public override string Description =>
        """
        Creates a manual import job for an Azure Managed Lustre (AMLFS) file system. The import job scans the linked HSM/Blob container and imports specified path prefixes (or all when omitted) honoring the chosen conflict resolution mode. Use to hydrate the AMLFS namespace or refresh content.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RequireResourceGroup();
        command.Options.Add(_fileSystemOption);
        command.Options.Add(_importPrefixesOption);
        command.Options.Add(_conflictResolutionModeOption);
        command.Options.Add(_maximumErrorsOption);
        command.Options.Add(_nameOption);
    }

    protected override FileSystemImportJobCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.FileSystem = parseResult.GetValueOrDefault(_fileSystemOption);
        var prefixes = parseResult.GetValueOrDefault(_importPrefixesOption);
        if (prefixes == null || prefixes.Length == 0)
        {
            options.ImportPrefixes = new List<string> { "/" };
        }
        else
        {
            options.ImportPrefixes = prefixes.ToList();
        }
        options.ConflictResolutionMode = parseResult.GetValueOrDefault(_conflictResolutionModeOption) ?? "OverwriteAlways";
        options.MaximumErrors = parseResult.GetValueOrDefault(_maximumErrorsOption) ?? -1;
        options.AdminStatus = "Active"; // Hard-coded since service no longer accepts parameter
        options.Name = parseResult.GetValueOrDefault(_nameOption);
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

            var svc = context.GetService<IAzureManagedLustreService>();
            var result = await svc.CreateImportJobAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileSystem!,
                options.Name,
                options.ImportPrefixes,
                options.ConflictResolutionMode ?? "OverwriteAlways",
                options.MaximumErrors ?? -1,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new FileSystemImportJobCreateResult(result),
                AzureManagedLustreJsonContext.Default.FileSystemImportJobCreateResult);
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

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record FileSystemImportJobCreateResult(Models.ImportJobInfo ImportJob);
}
