// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.FileShares.Commands;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

public sealed class FileShareUpdateCommand(ILogger<FileShareUpdateCommand> logger, IFileSharesService service)
    : BaseFileSharesCommand<FileShareCreateOrUpdateOptions>(logger, service)
{
    private const string CommandTitle = "Update File Share";

    public override string Id => "b4f1f1f2-f3f4-f5f6-f7f8-f9f0fafbfcfd";
    public override string Name => "update";
    public override string Description => "Update an existing Azure managed file share resource. Allows updating mutable properties like provisioned storage, IOPS, throughput, and network access settings.";
    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
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
        command.Options.Add(FileSharesOptionDefinitions.FileShare.Location.AsOptional());
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
            _logger.LogInformation("Updating file share. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}",
                options.Subscription, options.ResourceGroup, options.FileShareName);

            var fileShare = await _fileSharesService.CreateOrUpdateFileShareAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileShareName!,
                options.Location!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var result = new FileShareUpdateCommandResult(fileShare);
            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.FileShareUpdateCommandResult);

            _logger.LogInformation("File share updated successfully. FileShare: {FileShareName}", options.FileShareName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update file share");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record FileShareUpdateCommandResult([property: JsonPropertyName("fileShare")] FileShareInfo FileShare);
}
