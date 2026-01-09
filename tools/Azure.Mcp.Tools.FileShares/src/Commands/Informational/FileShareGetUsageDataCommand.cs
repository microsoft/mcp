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

public sealed class FileShareGetUsageDataCommand(ILogger<FileShareGetUsageDataCommand> logger, IFileSharesService service)
    : SubscriptionCommand<FileShareGetUsageDataOptions>()
{
    private readonly ILogger<FileShareGetUsageDataCommand> _logger = logger;
    private readonly IFileSharesService _service = service;

    public override string Id => "93d14ba8-5e75-4190-93dd-f47e932b849b";
    public override string Name => "usage";
    public override string Description => "Get file share usage data for a subscription and location";
    public override string Title => "Get File Share Usage Data";
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
    protected override FileShareGetUsageDataOptions BindOptions(ParseResult parseResult)
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
            _logger.LogInformation("Getting file share usage data for subscription {Subscription} in location {Location}",
                options.Subscription, options.Location);

            var result = await _service.GetUsageDataAsync(
                options.Subscription!,
                options.Location!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.FileShareUsageDataResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file share usage data. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }
}

