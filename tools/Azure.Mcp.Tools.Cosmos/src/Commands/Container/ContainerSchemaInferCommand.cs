// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Cosmos.Options.Container;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Cosmos.Commands.Container;

[CommandMetadata(
    Id = "f1c6a0e2-3d40-4b3f-9a37-2dc1f6cf4a12",
    Name = "infer",
    Title = "Infer Cosmos DB Container Schema",
    Description = "Infer an approximate schema for a Cosmos DB container by sampling documents and reporting the top-level properties along with their inferred types and the number of sampled documents in which each appeared. Nested objects and arrays are reported as `object` / `array` without recursion — to discover nested structure (e.g., the dot-path to a vector property), fetch an individual document via `cosmos database container item get` and inspect it.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ContainerSchemaInferCommand(ILogger<ContainerSchemaInferCommand> logger, ICosmosService cosmosService, ISubscriptionResolver subscriptionResolver)
    : BaseCosmosCommand<ContainerSchemaInferOptions, ContainerSchemaInferCommand.ContainerSchemaInferCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ContainerSchemaInferCommand> _logger = logger;
    private readonly ICosmosService _cosmosService = cosmosService;

    public override void ValidateOptions(ContainerSchemaInferOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.SampleSize != null && (options.SampleSize < 1 || options.SampleSize > 20))
        {
            validationResult.Errors.Add("--sample-size must be between 1 and 20.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ContainerSchemaInferOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var schema = await _cosmosService.GetApproximateSchema(
                options.Account,
                options.Database,
                options.Container,
                options.SampleSize ?? 10,
                options.Subscription!,
                options.AuthMethod ?? AuthMethod.Credential,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new ContainerSchemaInferCommandResult(schema.SampleSize, schema.Properties),
                CosmosJsonContext.Default.ContainerSchemaInferCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Account: {Account}, Database: {Database}, Container: {Container}",
                Name, options.Account, options.Database, options.Container);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ContainerSchemaInferCommandResult(int SampleSize, IReadOnlyList<Models.SchemaProperty> Properties);
}
