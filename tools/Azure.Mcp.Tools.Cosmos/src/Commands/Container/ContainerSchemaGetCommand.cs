// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Options.Container;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Cosmos.Commands.Container;

[CommandMetadata(
    Id = "f1c6a0e2-3d40-4b3f-9a37-2dc1f6cf4a12",
    Name = "get",
    Title = "Infer Cosmos DB Container Schema",
    Description = "Infer an approximate schema for a Cosmos DB container by sampling documents and reporting the top-level properties along with their observed JSON types and how often each appeared.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ContainerSchemaGetCommand(ILogger<ContainerSchemaGetCommand> logger, ICosmosService cosmosService)
    : BaseContainerCommand<ContainerSchemaGetOptions>()
{
    private readonly ILogger<ContainerSchemaGetCommand> _logger = logger;
    private readonly ICosmosService _cosmosService = cosmosService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CosmosOptionDefinitions.SampleSize);
        command.Validators.Add(result =>
        {
            var size = result.GetValueOrDefault<int>(CosmosOptionDefinitions.SampleSize.Name);
            if (size < 1 || size > 100)
            {
                result.AddError("--sample-size must be between 1 and 100.");
            }
        });
    }

    protected override ContainerSchemaGetOptions BindOptions(ParseResult parseResult)
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
                new ContainerSchemaGetCommandResult(schema.SampleSize, schema.Properties),
                CosmosJsonContext.Default.ContainerSchemaGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Account: {Account}, Database: {Database}, Container: {Container}",
                Name, options.Account, options.Database, options.Container);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        CosmosException cosmosEx => cosmosEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        CosmosException cosmosEx => cosmosEx.StatusCode,
        _ => base.GetStatusCode(ex)
    };

    internal record ContainerSchemaGetCommandResult(int SampleSize, IReadOnlyList<Models.SchemaProperty> Properties);
}
