// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models.Storage;
using Azure.Mcp.Tools.Adme.Options.Storage;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Adme.Commands.Storage;

/// <summary>
/// Fetches multiple ADME/OSDU records by id in a single call.
/// </summary>
[CommandMetadata(
    Id = "513e8747-ce60-43fd-99bf-19ff211f3679",
    Name = "fetch",
    Title = "Fetch ADME/OSDU Records",
    Description = """
        Fetch multiple ADME/OSDU records in one batch using known record IDs.
        Returns full record content, selected attributes, or conversion status and errors.

        Optional parameters: attributes (projection, dotted-path fields) and frame-of-reference conversion
        (SI measurements, WGS84 coordinates, UTC dates), limit (1-20, or 1-100 when using attributes),
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class RecordFetchCommand(IStorageService storageService)
    : AuthenticatedCommand<RecordFetchOptions, FetchRecordsResponse>
{
    private const int MaxBatchIds = 20;
    private const int MaxProjectionIds = 100;
    private readonly IStorageService _storageService = storageService;

    public override void ValidateOptions(RecordFetchOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        AdmeServiceValidator.ValidateTarget(options.Endpoint, options.DataPartition, validationResult);

        if (options.Attributes is not null
            && (options.Attributes.Length == 0 || options.Attributes.Any(string.IsNullOrWhiteSpace)))
        {
            validationResult.Errors.Add("--attributes cannot be empty or contain blank fields when specified.");
        }

        var projecting = options.Attributes is { Length: > 0 };
        if (options.Ids is not { Length: > 0 })
        {
            validationResult.Errors.Add("--ids must contain at least one record id.");
        }
        else
        {
            foreach (var id in options.Ids)
            {
                AdmeServiceValidator.ValidateRecordId(id, "--ids", validationResult);
            }

            if (options.Ids.Length > (projecting ? MaxProjectionIds : MaxBatchIds))
            {
                validationResult.Errors.Add(projecting
                    ? $"--ids must contain at most {MaxProjectionIds} ids when --attributes is used."
                    : $"--ids must contain at most {MaxBatchIds} ids.");
            }
        }

        if (projecting && options.FrameOfReference)
        {
            validationResult.Errors.Add("--frame-of-reference cannot be combined with --attributes.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, RecordFetchOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _storageService.FetchRecordsAsync(
                options.Endpoint, options.DataPartition, options.Ids, options.Attributes,
                options.FrameOfReference, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, AdmeJsonContext.Default.FetchRecordsResponse);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
