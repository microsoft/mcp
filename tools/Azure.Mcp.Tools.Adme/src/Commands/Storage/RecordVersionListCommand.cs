// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models.Storage;
using Azure.Mcp.Tools.Adme.Options.Storage;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Adme.Commands.Storage;

/// <summary>
/// Lists the versions of an ADME/OSDU record.
/// </summary>
[CommandMetadata(
    Id = "d73458d9-8996-4836-ae4f-05cb83af7362",
    Name = "list",
    Title = "List ADME/OSDU Record Versions",
    Description = """
        List only the versions of one ADME/OSDU record by known record ID.
        Returns record versions ordered oldest first.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class RecordVersionListCommand(IStorageService storageService)
    : AuthenticatedCommand<RecordVersionListOptions, RecordVersionsResponse>
{
    private readonly IStorageService _storageService = storageService;

    public override void ValidateOptions(RecordVersionListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        AdmeServiceValidator.ValidateTarget(options.Endpoint, options.DataPartition, validationResult);
        AdmeServiceValidator.ValidateRecordId(options.Id, "--id", validationResult);
    }

    /// <summary>
    /// Executes the record version listing request.
    /// </summary>
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, RecordVersionListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _storageService.ListRecordVersionsAsync(
                options.Endpoint,
                options.DataPartition,
                options.Id,
                options.Tenant,
                cancellationToken);
            context.Response.Results = ResponseResult.Create(result, AdmeJsonContext.Default.RecordVersionsResponse);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
