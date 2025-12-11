// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

public sealed class FileShareGetCommand(ILogger<FileShareGetCommand> logger)
    : BaseFileSharesCommand<FileShareGetOptions>
{
    private const string CommandTitle = "Get File Share";
    private readonly ILogger<FileShareGetCommand> _logger = logger;

    public override string Id => "g2b3c4d5-e6f7-48a9-b0c1-d2e3f4a5b6c7";

    public override string Name => "get";

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

    public override string Description =>
        "Get a specific file share by name.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.FileShareName.AsRequired());
    }

    protected override FileShareGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Name = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.FileShareName.Name);
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
            _logger.LogError(ex, "An exception occurred getting file share {Name}.", options.Name);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
