// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Cosmos.Commands.CopyJob;

public sealed class CopyJobGetCommand(ILogger<CopyJobGetCommand> logger) : BaseCopyJobCommand<CopyJobOptions>()
{
    private const string CommandTitle = "Get Cosmos DB Copy Job Status";
    private readonly ILogger<CopyJobGetCommand> _logger = logger;

    public override string Id => "cj-get-a1b2c3d4-e5f6-7890-abcd-000000000002";

    public override string Name => "get";

    public override string Description =>
        "Get the status and details of a Cosmos DB copy job. Returns job status, progress, " +
        "per-task completion percentages, and error information if the job faulted.";

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

            var result = await service.GetJobAsync(
                options.Subscription!,
                options.Account!,
                options.JobName!,
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
