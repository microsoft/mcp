// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Search.Options.Service;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Search.Commands.Service;

public sealed class ServiceListCommand(ILogger<ServiceListCommand> logger) : SubscriptionCommand<ServiceListOptions>()
{
    private const string CommandTitle = "List Azure AI Search (formerly known as \"Azure Cognitive Search\") Services";
    private readonly ILogger<ServiceListCommand> _logger = logger;

    public override string Id => "b0684f8c-20de-4bc0-bbc3-982575c8441f";

    public override string Name => "list";

    public override string Description =>
        """
        List/show Azure AI Search services in a subscription, returning details about each service.
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var searchService = context.GetService<ISearchService>();

            var services = await searchService.ListServices(
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(services ?? []), SearchJsonContext.Default.ServiceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing search services");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal sealed record ServiceListCommandResult(List<string> Services);
}
