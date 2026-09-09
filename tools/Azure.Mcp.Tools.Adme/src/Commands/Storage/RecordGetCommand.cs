// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models.Storage;
using Azure.Mcp.Tools.Adme.Options.Storage;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Adme.Commands.Storage;

/// <summary>
/// Gets a single OSDU record from a data partition.
/// </summary>
[CommandMetadata(
    Id = "9e14b28e-95bb-4bcb-9d68-46406ccba813",
    Name = "get",
    Title = "Get ADME Record",
    Description = """
        Get one OSDU record from a data partition, by default its latest version.

        Required: --id, --endpoint, and --data-partition.

        --id must be a fully-qualified record id '{partition}:{group-type}--{EntityType}:{unique-id}',
        for example 'opendes:master-data--Well:W-99'. Pass ids verbatim as returned by
        'azmcp adme storage record list'.

        Optional: --version pins a specific numeric version (discover them with
        'azmcp adme storage record version list'). --attributes projects dotted-path fields such as
        'data.Name' to shrink the record payload.

        For several records in one call use 'azmcp adme storage record fetch'.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class RecordGetCommand(IStorageService storageService)
    : AuthenticatedCommand<RecordGetOptions, StorageRecord>
{
    private readonly IStorageService _storageService = storageService;

    public override void ValidateOptions(RecordGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        AdmeServiceHelper.ValidateTarget(options.Endpoint, options.DataPartition, validationResult);
        AdmeServiceHelper.ValidateRecordId(options.Id, "--id", validationResult);

        if (options.Attributes is not null
            && (options.Attributes.Length == 0 || options.Attributes.Any(string.IsNullOrWhiteSpace)))
        {
            validationResult.Errors.Add("--attributes cannot be empty or contain blank fields when specified.");
        }

        if (options.Version is <= 0)
        {
            validationResult.Errors.Add("--version must be a positive integer.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, RecordGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _storageService.GetRecordAsync(
                options.Endpoint, options.DataPartition, options.Id, options.Version,
                options.Attributes, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, AdmeJsonContext.Default.StorageRecord);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
