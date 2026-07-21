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
    Id = "729a12ee-9c63-4a31-b1b8-4a81ad093564",
    Name = "get",
    Title = "Get Marketplace Product",
    Description = """
        Retrieves detailed information about a specific Azure Marketplace product (offer) for a given subscription,
        including available plans, pricing, and product metadata.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ProductGetCommand(ILogger<ProductGetCommand> logger, IMarketplaceService marketplaceService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ProductGetOptions, ProductGetCommand.ProductGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ProductGetCommand> _logger = logger;
    private readonly IMarketplaceService _marketplaceService = marketplaceService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ProductGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Call service operation with required parameters
            var result = await _marketplaceService.GetProduct(
                options.ProductId,
                options.Subscription!,
                options.IncludeStopSoldPlans,
                options.Language,
                options.LookupOfferInTenantLevel,
                options.PlanId,
                options.SkuId,
                options.IncludeServiceInstructionTemplates,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            // Set results
            context.Response.Results = result != null ?
                ResponseResult.Create(new(result), MarketplaceJsonContext.Default.ProductGetCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            // Log error with all relevant context
            _logger.LogError(ex,
                "Error getting marketplace product. ProductId: {ProductId}, Subscription: {Subscription}, Options: {Options}",
                options.ProductId, options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Implementation-specific error handling
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.NotFound =>
            "Marketplace product not found. Verify the product ID exists and you have access to it.",
        HttpRequestException httpEx =>
            $"Service unavailable or connectivity issues. Details: {httpEx.Message}",
        ArgumentException argEx =>
            $"Invalid parameter provided. Details: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    // Strongly-typed result record
    public sealed record ProductGetCommandResult(ProductDetails Product);
}
