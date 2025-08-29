// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Marketplace.Models;
using Azure.Mcp.Tools.Marketplace.Options.Product;
using Azure.Mcp.Tools.Marketplace.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Marketplace.Commands.Product;

public sealed class ProductGetCommand(ILogger<ProductGetCommand> logger) : SubscriptionCommand<ProductGetOptions>
{
    private const string CommandTitle = "Get Marketplace Product";
    private readonly ILogger<ProductGetCommand> _logger = logger;

    // Define options from OptionDefinitions
    private readonly Option<string> _productIdOption = OptionDefinitions.Marketplace.ProductId;
    private readonly Option<bool> _includeStopSoldPlansOption = OptionDefinitions.Marketplace.IncludeStopSoldPlans;
    private readonly Option<string> _languageOption = OptionDefinitions.Marketplace.Language;
    private readonly Option<string> _marketOption = OptionDefinitions.Marketplace.Market;
    private readonly Option<bool> _lookupOfferInTenantLevelOption = OptionDefinitions.Marketplace.LookupOfferInTenantLevel;
    private readonly Option<string> _planIdOption = OptionDefinitions.Marketplace.PlanId;
    private readonly Option<string> _skuIdOption = OptionDefinitions.Marketplace.SkuId;
    private readonly Option<bool> _includeServiceInstructionTemplatesOption = OptionDefinitions.Marketplace.IncludeServiceInstructionTemplates;
    private readonly Option<string> _pricingAudienceOption = OptionDefinitions.Marketplace.PricingAudience;

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves a single private product (offer) for a given subscription from Azure Marketplace.
        Returns detailed information about the specified marketplace product including plans, pricing, and metadata.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_productIdOption);
        command.Options.Add(_includeStopSoldPlansOption);
        command.Options.Add(_languageOption);
        command.Options.Add(_marketOption);
        command.Options.Add(_lookupOfferInTenantLevelOption);
        command.Options.Add(_planIdOption);
        command.Options.Add(_skuIdOption);
        command.Options.Add(_includeServiceInstructionTemplatesOption);
        command.Options.Add(_pricingAudienceOption);
    }

    protected override ProductGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ProductId = parseResult.GetValue(_productIdOption);
        options.IncludeStopSoldPlans = parseResult.GetValue(_includeStopSoldPlansOption);
        options.Language = parseResult.GetValue(_languageOption);
        options.Market = parseResult.GetValue(_marketOption);
        options.LookupOfferInTenantLevel = parseResult.GetValue(_lookupOfferInTenantLevelOption);
        options.PlanId = parseResult.GetValue(_planIdOption);
        options.SkuId = parseResult.GetValue(_skuIdOption);
        options.IncludeServiceInstructionTemplates = parseResult.GetValue(_includeServiceInstructionTemplatesOption);
        options.PricingAudience = parseResult.GetValue(_pricingAudienceOption);
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

            // Get the marketplace service from DI
            var marketplaceService = context.GetService<IMarketplaceService>();

            // Call service operation with required parameters
            var result = await marketplaceService.GetProduct(
                options.ProductId!,
                options.Subscription!,
                options.IncludeStopSoldPlans,
                options.Language,
                options.Market,
                options.LookupOfferInTenantLevel,
                options.PlanId,
                options.SkuId,
                options.IncludeServiceInstructionTemplates,
                options.PricingAudience,
                options.Tenant,
                options.RetryPolicy);

            // Set results
            context.Response.Results = result != null ?
                ResponseResult.Create(
                    new ProductGetCommandResult(result),
                    MarketplaceJsonContext.Default.ProductGetCommandResult) :
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

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        HttpRequestException httpEx => (int)httpEx.StatusCode.GetValueOrDefault(HttpStatusCode.InternalServerError),
        ArgumentException => 400,
        _ => base.GetStatusCode(ex)
    };

    // Strongly-typed result record
    internal record ProductGetCommandResult(ProductDetails Product);
}
