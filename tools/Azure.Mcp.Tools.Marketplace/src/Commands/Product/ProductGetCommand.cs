// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
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

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves detailed information about a specific Azure Marketplace product (offer) for a given subscription,
         including available plans, pricing, and product metadata.
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
        command.Options.Add(OptionDefinitions.Marketplace.ProductId);
        command.Options.Add(OptionDefinitions.Marketplace.IncludeStopSoldPlans);
        command.Options.Add(OptionDefinitions.Marketplace.Language);
        command.Options.Add(OptionDefinitions.Marketplace.Market);
        command.Options.Add(OptionDefinitions.Marketplace.LookupOfferInTenantLevel);
        command.Options.Add(OptionDefinitions.Marketplace.PlanId);
        command.Options.Add(OptionDefinitions.Marketplace.SkuId);
        command.Options.Add(OptionDefinitions.Marketplace.IncludeServiceInstructionTemplates);
        command.Options.Add(OptionDefinitions.Marketplace.PricingAudience);
    }

    protected override ProductGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ProductId = parseResult.GetValueOrDefault<string>(OptionDefinitions.Marketplace.ProductId.Name);
        options.IncludeStopSoldPlans = parseResult.GetValueOrDefault<bool>(OptionDefinitions.Marketplace.IncludeStopSoldPlans.Name);
        options.Language = parseResult.GetValueOrDefault<string>(OptionDefinitions.Marketplace.Language.Name);
        options.Market = parseResult.GetValueOrDefault<string>(OptionDefinitions.Marketplace.Market.Name);
        options.LookupOfferInTenantLevel = parseResult.GetValueOrDefault<bool>(OptionDefinitions.Marketplace.LookupOfferInTenantLevel.Name);
        options.PlanId = parseResult.GetValueOrDefault<string>(OptionDefinitions.Marketplace.PlanId.Name);
        options.SkuId = parseResult.GetValueOrDefault<string>(OptionDefinitions.Marketplace.SkuId.Name);
        options.IncludeServiceInstructionTemplates = parseResult.GetValueOrDefault<bool>(OptionDefinitions.Marketplace.IncludeServiceInstructionTemplates.Name);
        options.PricingAudience = parseResult.GetValueOrDefault<string>(OptionDefinitions.Marketplace.PricingAudience.Name);
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

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        HttpRequestException httpEx => httpEx.StatusCode.GetValueOrDefault(HttpStatusCode.InternalServerError),
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    // Strongly-typed result record
    internal record ProductGetCommandResult(ProductDetails Product);
}
