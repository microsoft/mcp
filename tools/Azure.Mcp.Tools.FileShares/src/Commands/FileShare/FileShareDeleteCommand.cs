// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

/// <summary>
/// Deletes a file share.
/// </summary>
public sealed class FileShareDeleteCommand(ILogger<FileShareDeleteCommand> logger, IFileSharesService fileSharesService)
    : BaseFileSharesCommand<FileShareDeleteOptions>(logger, fileSharesService)
{
    public override string Id => "azmcp-fileshares-fileshare-delete";
    public override string Name => "delete";
    public override string Description => "Delete a file share";
    public override string Title => "Delete FileShare";
    public override ToolMetadata Metadata => new();

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.FileShare.Name.AsRequired());
    }

    protected override FileShareDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileShareName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.FileShare.Name.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        var options = BindOptions(parseResult);

        try
        {
            _logger.LogInformation(
                "Deleting file share {FileShareName} in resource group {ResourceGroup}, subscription {Subscription}",
                options.FileShareName,
                options.ResourceGroup,
                options.Subscription);

            await _fileSharesService.DeleteFileShareAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileShareName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new FileShareDeleteCommandResult(true, options.FileShareName!),
                FileSharesJsonContext.Default.FileShareDeleteCommandResult);

            _logger.LogInformation(
                "Successfully deleted file share {FileShareName}",
                options.FileShareName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file share. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record FileShareDeleteCommandResult(bool Deleted, string FileShareName);
}
