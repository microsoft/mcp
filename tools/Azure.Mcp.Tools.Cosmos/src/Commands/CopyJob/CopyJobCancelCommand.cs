// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Cosmos.Commands.CopyJob;

public sealed class CopyJobCancelCommand(ILogger<CopyJobCancelCommand> logger) : BaseCopyJobCommand<CopyJobOptions>()
{
    private const string CommandTitle = "Cancel Cosmos DB Copy Job";
    private readonly ILogger<CopyJobCancelCommand> _logger = logger;

    public override string Id => "cj-cancel-a1b2c3d4-e5f6-7890-abcd-000000000004";

    public override string Name => "cancel";

    public override string Description =>
        "Cancel a running or pending Cosmos DB copy job. This action is irreversible — " +
        "the job cannot be resumed after cancellation.";

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

            var result = await service.CancelJobAsync(
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
