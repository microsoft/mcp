// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

public sealed class FileShareCreateOrUpdateCommand(ILogger<FileShareCreateOrUpdateCommand> logger)
    : BaseFileSharesCommand<FileShareCreateOrUpdateOptions>
{
    private const string CommandTitle = "Create or Update File Share";
    private readonly ILogger<FileShareCreateOrUpdateCommand> _logger = logger;

    public override string Id => "h3c4d5e6-f7a8-49b0-c1d2-e3f4a5b6c7d8";

    public override string Name => "create";

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

    public override string Description =>
        "Create or update a file share resource.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.FileShareName.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.Location.AsRequired());
    }

    protected override FileShareCreateOrUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Name = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.FileShareName.Name);
        options.Location = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.Location.Name);
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
            var fileSharesService = context.GetService<IFileSharesService>();
            var fileShare = await fileSharesService.GetFileShare(
                options.Subscription!,
                options.ResourceGroup!,
                options.Name!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(fileShare, FileSharesJsonContext.Default.FileShareDetail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred creating or updating file share {Name}.", options.Name);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
