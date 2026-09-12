// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models.Storage;
using Azure.Mcp.Tools.Adme.Options.Storage;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Adme.Commands.Storage;

/// <summary>
/// Lists ADME/OSDU record ids for a kind.
/// </summary>
[CommandMetadata(
    Id = "b18a9dd5-252b-4bf0-85e1-4fdf63f996f2",
    Name = "list",
    Title = "List ADME/OSDU Records",
    Description = """
        List multiple ADME/OSDU record IDs for one kind.
        Returns IDs only, in pages.

        Pagination parameters: limit (1-100, default 10) and cursor (continuation token).
        Confirm how many records the user wants before paging through the whole set.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class RecordListCommand(IStorageService storageService)
    : AuthenticatedCommand<RecordListOptions, QueryRecordsResponse>
{
    private const int DefaultLimit = 10;
    private const int MaxLimit = 100;
    private readonly IStorageService _storageService = storageService;

    public override void ValidateOptions(RecordListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        AdmeServiceValidator.ValidateTarget(options.Endpoint, options.DataPartition, validationResult);
        AdmeServiceValidator.ValidateKind(options.Kind, validationResult);

        if (options.Limit is < 1 or > MaxLimit)
        {
            validationResult.Errors.Add($"--limit must be between 1 and {MaxLimit}.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, RecordListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _storageService.QueryRecordsByKindAsync(
                options.Endpoint, options.DataPartition, options.Kind, options.Limit ?? DefaultLimit,
                options.Cursor, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, AdmeJsonContext.Default.QueryRecordsResponse);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
