// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

/// <summary>
/// Command to list file shares in a subscription or resource group.
/// </summary>
public sealed class FileShareListCommand(ILogger<FileShareListCommand> logger) : SubscriptionCommand<SubscriptionOptions>()
{
    private readonly ILogger<FileShareListCommand> _logger = logger;

    public override string Id => "d1e0e0e1-e2e3-e4e5-e6e7-e8e9eaebeced";
    public override string Name => "list";
    public override string Description => "List file shares in a subscription or resource group.";
    public override string Title => "List File Shares";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var service = context.GetService<IFileSharesService>() ?? throw new InvalidOperationException("FileShares service is not available.");
            var results = await service.ListFileSharesAsync(
                options.Subscription!,
                tenant: options.Tenant,
                retryPolicy: options.RetryPolicy,
                cancellationToken: cancellationToken);

            var result = new FileShareListCommandResult(results ?? []);
            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.FileShareListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list file shares");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record FileShareListCommandResult(IEnumerable<FileShareInfo> FileShares);
}

