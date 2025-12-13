// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

public sealed class FileShareListCommand(ILogger<FileShareListCommand> logger)
    : BaseFileSharesCommand<FileShareListOptions>
{
    private const string CommandTitle = "List File Shares";
    private readonly ILogger<FileShareListCommand> _logger = logger;

    public override string Id => "f1a2b3c4-d5e6-47f8-a9b0-c1d2e3f4a5b6";

    public override string Name => "list";

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
        "List all file shares in a subscription or resource group.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(FileSharesOptionDefinitions.Filter);
    }

    protected override FileShareListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Filter = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.Filter.Name);
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
            var fileShares = await fileSharesService.ListFileShares(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(fileShares ?? []), FileSharesJsonContext.Default.FileShareListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing file shares.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record FileShareListCommandResult(List<string> FileShares);
}
