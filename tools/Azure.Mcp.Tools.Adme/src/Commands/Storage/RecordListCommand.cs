// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models.Storage;
using Azure.Mcp.Tools.Adme.Options.Storage;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Adme.Commands.Storage;

/// <summary>
/// Lists OSDU record ids for a kind.
/// </summary>
[CommandMetadata(
    Id = "b18a9dd5-252b-4bf0-85e1-4fdf63f996f2",
    Name = "list",
    Title = "List ADME Records",
    Description = """
        List the ids of OSDU records of one kind in a data partition. Returns ids only - use
        'azmcp adme storage record get' or 'azmcp adme storage record fetch' to read the records.

        Required: --kind, --endpoint, and --data-partition.

        --kind must be a fully-qualified kind '{authority}:{source}:{entityType}:{version}', for example
        'osdu:wks:master-data--Well:1.0.0'; use 'azmcp adme schema list' to discover valid kinds.

        Paging: --limit (1-100, default 10) and --cursor, which takes the cursor from a previous
        response. A null cursor means there are no more pages. A kind can hold thousands of records,
        so confirm how many the user wants before paging through the whole set.
        """,
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
        AdmeServiceHelper.ValidateTarget(options.Endpoint, options.DataPartition, validationResult);
        AdmeServiceHelper.ValidateKind(options.Kind, validationResult);

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
