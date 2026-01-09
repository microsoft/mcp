// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Options.Informational;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FileShares.Commands.Informational;

public sealed class FileShareGetLimitsCommand(ILogger<FileShareGetLimitsCommand> logger, IFileSharesService service)
    : SubscriptionCommand<FileShareGetLimitsOptions>()
{
    private readonly ILogger<FileShareGetLimitsCommand> _logger = logger;
    private readonly IFileSharesService _service = service;

    public override string Id => "a9e1f0b2-c3d4-4e5f-a6b7-c8d9e0f1a2b3";
    public override string Name => "limits";
    public override string Description => "Get file share limits for a subscription and location";
    public override string Title => "Get File Share Limits";
    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false,
        LocalRequired = false
    };

    /// <inheritdoc />
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FileSharesOptionDefinitions.Location.AsRequired());
    }

    /// <inheritdoc />
    protected override FileShareGetLimitsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Location = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.Location.Name);
        return options;
    }

    /// <inheritdoc />
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        var options = BindOptions(parseResult);

        try
        {
            _logger.LogInformation("Getting file share limits for subscription {Subscription} in location {Location}",
                options.Subscription, options.Location);

            var result = await _service.GetLimitsAsync(
                options.Subscription!,
                options.Location!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.FileShareLimitsResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file share limits. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }
}

