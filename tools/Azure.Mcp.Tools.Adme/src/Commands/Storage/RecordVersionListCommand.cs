// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models.Storage;
using Azure.Mcp.Tools.Adme.Options.Storage;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Adme.Commands.Storage;

/// <summary>
/// Lists the versions of an OSDU record.
/// </summary>
[CommandMetadata(
    Id = "d73458d9-8996-4836-ae4f-05cb83af7362",
    Name = "list",
    Title = "List ADME Record Versions",
    Description = """
        List the numeric versions of one OSDU record, oldest first.

        Required: --id, --endpoint, and --data-partition. --id is a fully-qualified record id
        '{partition}:{group-type}--{EntityType}:{unique-id}', for example
        'opendes:master-data--Well:W-99'.

        Pass one of the returned versions to 'azmcp adme storage record get --version' to read that
        version of the record.
        """,
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
        AdmeServiceHelper.ValidateTarget(options.Endpoint, options.DataPartition, validationResult);
        AdmeServiceHelper.ValidateRecordId(options.Id, "--id", validationResult);
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
