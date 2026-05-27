// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Options.Container;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
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
public sealed class ContainerSchemaInferCommand(ILogger<ContainerSchemaInferCommand> logger, ICosmosService cosmosService)
    : BaseContainerCommand<ContainerSchemaInferOptions>()
{
    private readonly ILogger<ContainerSchemaInferCommand> _logger = logger;
    private readonly ICosmosService _cosmosService = cosmosService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CosmosOptionDefinitions.SampleSize);
        command.Validators.Add(result =>
        {
            var size = result.GetValueOrDefault<int>(CosmosOptionDefinitions.SampleSize.Name);
            if (size < 1 || size > 20)
            {
                result.AddError("--sample-size must be between 1 and 20.");
            }
        });
    }

    protected override ContainerSchemaInferOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.SampleSize = parseResult.GetValueOrDefault<int>(CosmosOptionDefinitions.SampleSize.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var schema = await _cosmosService.GetApproximateSchema(
                options.Account!,
                options.Database!,
                options.Container!,
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

    internal record ContainerSchemaInferCommandResult(int SampleSize, IReadOnlyList<Models.SchemaProperty> Properties);
}
