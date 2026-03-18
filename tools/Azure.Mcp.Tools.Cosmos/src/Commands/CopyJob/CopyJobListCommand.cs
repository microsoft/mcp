// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Cosmos.Commands.CopyJob;

/// <summary>
/// Lists all copy jobs on a Cosmos DB account. Only needs --account (no --job-name).
/// </summary>
public sealed class CopyJobListCommand(ILogger<CopyJobListCommand> logger) : SubscriptionCommand<BaseCosmosOptions>()
{
    private const string CommandTitle = "List Cosmos DB Copy Jobs";
    private readonly ILogger<CopyJobListCommand> _logger = logger;

    public override string Id => "cj-list-a1b2c3d4-e5f6-7890-abcd-000000000003";

    public override string Name => "list";

    public override string Description =>
        "List all copy jobs on a Cosmos DB account. Returns job names, statuses, types, " +
        "and progress for each job.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CosmosOptionDefinitions.Account);
    }

    protected override BaseCosmosOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.Account.Name);
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

            var result = await service.ListJobsAsync(
                options.Subscription!,
                options.Account!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new CopyJobListResult(result),
                CosmosJsonContext.Default.CopyJobListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {@Options}", Name, options);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
