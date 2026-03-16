// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.DocumentDb.Options;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.DocumentDb.Commands.Others;

public sealed class GetStatsCommand(ILogger<GetStatsCommand> logger)
    : BaseDocumentDbCommand<GetStatsOptions>()
{
    private readonly ILogger<GetStatsCommand> _logger = logger;

    public override string Id => "73d37b66-e26e-4cd0-b401-cff1f9f09d8e";

    public override string Name => "get_stats";

    public override string Description => "Get statistics for an Azure DocumentDB collection, database, or index by resource type.";

    public override string Title => "Get Statistics";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DocumentDbOptionDefinitions.ResourceType);
        command.Options.Add(DocumentDbOptionDefinitions.DbName);
        command.Options.Add(DocumentDbOptionDefinitions.CollectionName.AsOptional());
    }

    protected override GetStatsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceType = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.ResourceType.Name);
        options.DbName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.DbName.Name);
        options.CollectionName = parseResult.GetValueOrDefault<string>(DocumentDbOptionDefinitions.CollectionName.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        GetStatsOptions? commandOptions = null;

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var options = commandOptions = BindOptions(parseResult);

            if ((options.ResourceType is "collection" or "index") && string.IsNullOrWhiteSpace(options.CollectionName))
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = $"--collection-name is required when --resource-type is '{options.ResourceType}'.";
                return context.Response;
            }

            var service = context.GetService<IDocumentDbService>();
            var result = options.ResourceType switch
            {
                "collection" => await service.GetCollectionStatsAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, cancellationToken),
                "database" => await service.GetDatabaseStatsAsync(options.ConnectionString!, options.DbName!, cancellationToken),
                "index" => await service.GetIndexStatsAsync(options.ConnectionString!, options.DbName!, options.CollectionName!, cancellationToken),
                _ => throw new InvalidOperationException($"Unsupported resource type '{options.ResourceType}'.")
            };

            DocumentDbResponseHelper.ProcessResponse(context, result);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to get {ResourceType} statistics for database: {DbName}, collection: {CollectionName}",
                commandOptions?.ResourceType,
                commandOptions?.DbName,
                commandOptions?.CollectionName);
            HandleException(context, ex);
            return context.Response;
        }
    }
}
