// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Blob.Batch;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage.Commands.Blob.Batch;

public sealed class BatchSetTierCommand(ILogger<BatchSetTierCommand> logger) : BaseContainerCommand<BatchSetTierOptions>()
{
    private const string CommandTitle = "Set Access Tier for Multiple Blobs";
    private readonly ILogger<BatchSetTierCommand> _logger = logger;

    private readonly Option<string> _tierOption = StorageOptionDefinitions.Tier;
    private readonly Option<string[]> _blobsOption = StorageOptionDefinitions.Blobs;

    public override string Name => "set-tier";

    public override string Description =>
        $"""
        Set access tier for multiple blobs in a single batch operation. This tool efficiently changes the
        storage tier for multiple blobs simultaneously in a single request. Different tiers offer different
        trade-offs between storage costs, access costs, and retrieval latency.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_tierOption);
        command.Options.Add(_blobsOption);
    }

    protected override BatchSetTierOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Tier = parseResult.GetValueOrDefault(_tierOption);
        options.BlobNames = parseResult.GetValueOrDefault(_blobsOption);
        return options;
    }

    [McpServerTool(Destructive = false, ReadOnly = false, Title = CommandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var storageService = context.GetService<IStorageService>();
            var result = await storageService.SetBlobTierBatch(
                options.Account!,
                options.Container!,
                options.Tier!,
                options.BlobNames!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new BatchSetTierCommandResult(result.SuccessfulBlobs, result.FailedBlobs),
                StorageJsonContext.Default.BatchSetTierCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error setting blob tier batch. Account: {Account}, Container: {Container}, Tier: {Tier}, BlobCount: {BlobCount}.",
                options.Account, options.Container, options.Tier, options.BlobNames?.Length ?? 0);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record BatchSetTierCommandResult(List<string> SuccessfulBlobs, List<string> FailedBlobs);
}
