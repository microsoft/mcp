// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using System.Net;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

public sealed class FileShareDeleteCommand(ILogger<FileShareDeleteCommand> logger)
    : BaseFileSharesCommand<FileShareDeleteOptions>
{
    private const string CommandTitle = "Delete File Share";
    private readonly ILogger<FileShareDeleteCommand> _logger = logger;

    public override string Id => "i4d5e6f7-a8b9-40c1-d2e3-f4a5b6c7d8e9";

    public override string Name => "delete";

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

    public override string Description =>
        "Delete a file share resource.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.FileShareName.AsRequired());
    }

    protected override FileShareDeleteOptions BindOptions(ParseResult parseResult)
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
            await fileSharesService.DeleteFileShare(
                options.Subscription!,
                options.ResourceGroup!,
                options.Name!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new { message = $"File share '{options.Name}' deleted successfully." },
                FileSharesJsonContext.Default.JsonElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred deleting file share {Name}.", options.Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        _ => base.GetStatusCode(ex)
    };
}
