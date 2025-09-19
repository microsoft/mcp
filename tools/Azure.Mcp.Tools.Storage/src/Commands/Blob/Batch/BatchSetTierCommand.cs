// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
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

    public override string Name => "set-tier";

    public override string Description =>
        $"""
        Sets access tier for multiple blobs in a single batch operation, returning the names of blobs that had their access
        tier set and blobs that failed to have their access tier set.
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
        command.Options.Add(StorageOptionDefinitions.Tier);
        command.Options.Add(StorageOptionDefinitions.Blobs);
    }

    protected override BatchSetTierOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Tier = parseResult.GetValueOrDefault<string>(StorageOptionDefinitions.Tier.Name);
        options.BlobNames = parseResult.GetValueOrDefault<string[]>(StorageOptionDefinitions.Blobs.Name);
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

            context.Response.Results = ResponseResult.Create(new(result.SuccessfulBlobs, result.FailedBlobs), StorageJsonContext.Default.BatchSetTierCommandResult);
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

    internal record BatchSetTierCommandResult(
        [property: JsonPropertyName("successfulBlobs")] List<string> SuccessfulBlobs,
        [property: JsonPropertyName("failedBlobs")] List<string> FailedBlobs);
}
