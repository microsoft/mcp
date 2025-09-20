// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.ResourceHealth.Options.AvailabilityStatus;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ResourceHealth.Commands.AvailabilityStatus;

/// <summary>
/// Gets the current availability status of the specified Azure resource for health diagnostics.
/// </summary>
public sealed class AvailabilityStatusGetCommand(ILogger<AvailabilityStatusGetCommand> logger)
    : BaseResourceHealthCommand<AvailabilityStatusGetOptions>()
{
    private const string CommandTitle = "Get Resource Availability Status";
    private readonly ILogger<AvailabilityStatusGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        $"""
        Get the current availability status of an Azure resource to diagnose health issues.
        Provides detailed information about resource availability state, potential issues, and timestamps.
        Equivalent to Azure Resource Health availability status API.
        """;

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
        command.Options.Add(ResourceHealthOptionDefinitions.ResourceId);
    }

    protected override AvailabilityStatusGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceId = parseResult.GetValueOrDefault(ResourceHealthOptionDefinitions.ResourceId);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var resourceHealthService = context.GetService<IResourceHealthService>() ??
                throw new InvalidOperationException("Resource Health service is not available.");

            var status = await resourceHealthService.GetAvailabilityStatusAsync(
                options.ResourceId!,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(status), ResourceHealthJsonContext.Default.AvailabilityStatusGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get availability status for resource {ResourceId}", options.ResourceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AvailabilityStatusGetCommandResult(Models.AvailabilityStatus Status);
}
