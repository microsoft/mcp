// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Cosmos.Commands.CopyJob;

public sealed class CopyJobCreateCommand(ILogger<CopyJobCreateCommand> logger) : BaseCopyJobCommand<CopyJobCreateOptions>()
{
    private const string CommandTitle = "Create Cosmos DB Copy Job";
    private readonly ILogger<CopyJobCreateCommand> _logger = logger;

    public override string Id => "cj-create-a1b2c3d4-e5f6-7890-abcd-000000000001";

    public override string Name => "create";

    public override string Description =>
        "Create a new Cosmos DB container copy job. Use --job-properties to specify the job type, source, " +
        "destination, and tasks in JSON format. Supported job types: NoSqlRUToNoSqlRU, CassandraRUToCassandraRU, " +
        "MongoRUToMongoRU, MongoRUToMongoVCore, CassandraRUToAzureBlobStorage, AzureBlobStorageToCassandraRU. " +
        "Example --job-properties: {\"jobType\":\"NoSqlRUToNoSqlRU\",\"tasks\":[{\"source\":{\"databaseName\":\"db1\"," +
        "\"containerName\":\"src\"},\"destination\":{\"databaseName\":\"db1\",\"containerName\":\"dest\"}}]}";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CosmosOptionDefinitions.JobProperties);
        command.Options.Add(CosmosOptionDefinitions.Mode);
        command.Options.Add(CosmosOptionDefinitions.WorkerCount);
    }

    protected override CopyJobCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.JobProperties = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.JobProperties.Name);
        options.Mode = parseResult.GetValueOrDefault<string?>(CosmosOptionDefinitions.Mode.Name);
        options.WorkerCount = parseResult.GetValueOrDefault<int?>(CosmosOptionDefinitions.WorkerCount.Name);
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
            var service = context.GetService<ICopyJobService>() ?? throw new InvalidOperationException("Copy job service is not available.");

            var result = await service.CreateJobAsync(
                options.Subscription!,
                options.Account!,
                options.JobName!,
                options.JobProperties!,
                options.Mode,
                options.WorkerCount,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new CopyJobResult(result),
                CosmosJsonContext.Default.CopyJobResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {@Options}", Name, options);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
