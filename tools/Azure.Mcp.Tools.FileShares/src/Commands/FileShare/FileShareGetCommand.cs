// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.FileShares.Commands;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

public sealed class FileShareGetCommand(ILogger<FileShareGetCommand> logger, IFileSharesService service)
    : BaseFileSharesCommand<FileShareGetOptions>(logger, service)
{
    private const string CommandTitle = "Get File Share";


    public override string Id => "c5d6e7f8-a9b0-4c1d-2e3f-4a5b6c7d8e9f";
    public override string Name => "get";
    public override string Description => "Get details of a specific file share or list all file shares. If --name is provided, returns a specific file share; otherwise, lists all file shares in the subscription or resource group.";
    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(FileSharesOptionDefinitions.FileShare.Name.AsOptional());
    }

    protected override FileShareGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileShareName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.FileShare.Name.Name);
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
            // If file share name is provided, get specific file share
            if (!string.IsNullOrEmpty(options.FileShareName))
            {
                _logger.LogInformation("Getting file share. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}",
                    options.Subscription, options.ResourceGroup, options.FileShareName);

                var fileShare = await _fileSharesService.GetFileShareAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.FileShareName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var singleResult = new FileShareGetCommandResult([fileShare]);
                context.Response.Results = ResponseResult.Create(singleResult, FileSharesJsonContext.Default.FileShareGetCommandResult);

                _logger.LogInformation("Successfully retrieved file share. FileShareName: {FileShareName}", options.FileShareName);
            }
            else
            {
                // List all file shares
                _logger.LogInformation("Listing file shares. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}",
                    options.Subscription, options.ResourceGroup ?? "(all)");

                var fileShares = await _fileSharesService.ListFileSharesAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var result = new FileShareGetCommandResult(fileShares ?? []);
                context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.FileShareGetCommandResult);

                _logger.LogInformation("Successfully listed {Count} file shares", fileShares?.Count ?? 0);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file share(s)");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record FileShareGetCommandResult([property: JsonPropertyName("fileShares")] List<FileShareInfo> FileShares);
}
