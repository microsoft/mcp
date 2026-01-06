// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

public sealed class FileShareCreateCommand(ILogger<FileShareCreateCommand> logger, IFileSharesService service)
    : BaseFileSharesCommand<FileShareCreateOrUpdateOptions>(logger, service)
{
    private const string CommandTitle = "Create File Share";

    public override string Id => "a3e0e0e1-e2e3-e4e5-e6e7-e8e9eaebecea";
    public override string Name => "create";
    public override string Description => "Create a new Azure managed file share resource in a resource group. This creates a high-performance, fully managed file share accessible via NFS protocol.";
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
        command.Options.Add(FileSharesOptionDefinitions.FileShare.Name.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.FileShare.Location.AsRequired());
    }

    protected override FileShareCreateOrUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileShareName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.FileShare.Name.Name);
        options.Location = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.FileShare.Location.Name);
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
            _logger.LogInformation("Creating file share. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}",
                options.Subscription, options.ResourceGroup, options.FileShareName);

            var fileShare = await _fileSharesService.CreateOrUpdateFileShareAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileShareName!,
                options.Location!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var result = new FileShareCreateCommandResult(fileShare);
            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.FileShareCreateCommandResult);

            _logger.LogInformation("File share created successfully. FileShare: {FileShareName}", options.FileShareName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create file share");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record FileShareCreateCommandResult([property: JsonPropertyName("fileShare")] FileShareInfo FileShare);
}
