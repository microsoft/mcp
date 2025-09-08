// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AzureManagedLustre.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
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
    private readonly Option<string> _adminStatusOption = AzureManagedLustreOptionDefinitions.AdminStatusOption;
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
        command.AddOption(_fileSystemOption);
        command.AddOption(_importPrefixesOption);
        command.AddOption(_conflictResolutionModeOption);
        command.AddOption(_maximumErrorsOption);
        command.AddOption(_adminStatusOption);
        command.AddOption(_nameOption);
    }

    protected override FileSystemImportJobCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.FileSystem = parseResult.GetValueForOption(_fileSystemOption);
        var prefixes = parseResult.GetValueForOption(_importPrefixesOption);
        options.ImportPrefixes = prefixes == null ? new List<string> { "/" } : prefixes.ToList();
        options.ConflictResolutionMode = parseResult.GetValueForOption(_conflictResolutionModeOption) ?? "OverwriteAlways";
        options.MaximumErrors = parseResult.GetValueForOption(_maximumErrorsOption) ?? -1;
        options.AdminStatus = parseResult.GetValueForOption(_adminStatusOption) ?? "Active";
        options.Name = parseResult.GetValueForOption(_nameOption);
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
                options.AdminStatus,
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
