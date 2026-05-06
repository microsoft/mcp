// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Search.Options.Service;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Search.Commands.Service;

public sealed class ServiceListCommand(ILogger<ServiceListCommand> logger, ISearchService searchService) : SubscriptionCommand<ServiceListOptions, ServiceListCommand.ServiceListCommandResult>()
{
    private const string CommandTitle = "List Azure AI Search (formerly known as \"Azure Cognitive Search\") Services";
    private readonly ILogger<ServiceListCommand> _logger = logger;
    private readonly ISearchService _searchService = searchService;

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

    protected override JsonTypeInfo<ServiceListCommandResult> ResultTypeInfo => SearchJsonContext.Default.ServiceListCommandResult;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var services = await _searchService.ListServices(
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            SetResult(context, new(services ?? []));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing search services");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ServiceListCommandResult(List<string> Services);
}
