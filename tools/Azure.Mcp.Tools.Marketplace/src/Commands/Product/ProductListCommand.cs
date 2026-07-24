// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Marketplace.Models;
using Azure.Mcp.Tools.Marketplace.Options.Product;
using Azure.Mcp.Tools.Marketplace.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Marketplace.Commands.Product;

[CommandMetadata(
    Id = "0485e8f9-61bf-4baf-b914-7fa5530a6f78",
    Name = "list",
    Title = "List Marketplace Products",
    Description = "Retrieves and lists all marketplace products (offers) available to a subscription in the Azure Marketplace. Use this tool to search, select, browse, or filter marketplace offers by product name, publisher, pricing, or metadata. Returns information for each product, including display name, publisher details, category, pricing data, and available plans.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ProductListCommand(ILogger<ProductListCommand> logger, IMarketplaceService marketplaceService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ProductListOptions, ProductListCommand.ProductListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ProductListCommand> _logger = logger;
    private readonly IMarketplaceService _marketplaceService = marketplaceService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ProductListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Call service operation with required parameters
            var results = await _marketplaceService.ListProducts(
                options.Subscription!,
                options.Language,
                options.Search,
                options.Filter,
                options.OrderBy,
                options.Select,
                options.NextCursor,
                options.Expand,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            // Set results
            context.Response.Results = ResponseResult.Create(new(results.Items ?? [], results.NextCursor), MarketplaceJsonContext.Default.ProductListCommandResult);
        }
        catch (Exception ex)
        {
            // Log error with all relevant context
            _logger.LogError(ex,
                "Error listing marketplace products. Subscription: {Subscription}, Search: {Search}.",
                options.Subscription, options.Search);
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Implementation-specific error handling
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        HttpRequestException { StatusCode: HttpStatusCode.NotFound } =>
            "No marketplace products found for the specified subscription. Verify the subscription exists and you have access to it.",
        HttpRequestException { StatusCode: HttpStatusCode.Forbidden } =>
            "Access denied to marketplace products. Ensure you have appropriate permissions for the subscription.",
        HttpRequestException httpEx =>
            $"Service unavailable or connectivity issues. Details: {httpEx.Message}",
        ArgumentException argEx =>
            $"Invalid parameter provided. Details: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    // Strongly-typed result record
    public sealed record ProductListCommandResult(List<ProductSummary> Products, string? NextCursor);
}
