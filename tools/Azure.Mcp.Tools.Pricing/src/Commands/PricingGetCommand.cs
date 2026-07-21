// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Pricing.Models;
using Azure.Mcp.Tools.Pricing.Options;
using Azure.Mcp.Tools.Pricing.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Pricing.Commands;

/// <summary>
/// Gets Azure retail pricing information based on specified filters.
/// </summary>
[CommandMetadata(
    Id = "c5a8f7d2-9e3b-4a1c-8d6f-2b5e9c4a7d3e",
    Name = "get",
    Title = "Get Azure Retail Pricing",
    Description = """
        Get Azure retail pricing information. Do NOT call this tool if the user provides only a broad service name (e.g., "Virtual Machines", "Storage", "SQL Database") without a specific SKU—ask for the exact SKU or tier first. 
        For comparisons across regions or SKUs, require explicit ARM SKU names. Do not assume defaults. Call this tool only after the user specifies a SKU (--sku) or confirms they want all pricing for a service. Requires at least one filter: --sku, --service, --region, --service-family, or --filter. 
        SavingsPlan is not a valid --price-type; use --include-savings-plan instead. Valid --price-type: Consumption, Reservation, DevTestConsumption. When --include-savings-plan is true, Consumption results include a nested savingsPlan array (1-year/3-year pricing, mainly Linux VMs). 
        For Bicep/ARM cost estimation, extract resource type and SKU, query per resource, and sum monthly costs (hourly x 730).
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class PricingGetCommand(ILogger<PricingGetCommand> logger, IPricingService pricingService)
    : AuthenticatedCommand<PricingGetOptions, PricingGetCommand.PricingGetCommandResult>
{
    private readonly ILogger<PricingGetCommand> _logger = logger;
    private readonly IPricingService _pricingService = pricingService;

    public override void ValidateOptions(PricingGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrEmpty(options.Sku) &&
            string.IsNullOrEmpty(options.Service) &&
            string.IsNullOrEmpty(options.Region) &&
            string.IsNullOrEmpty(options.ServiceFamily) &&
            string.IsNullOrEmpty(options.PriceType) &&
            string.IsNullOrEmpty(options.Filter))
        {
            validationResult.Errors.Add("At least one filter is required. " +
                "Specify --sku, --service, --region, --service-family, --price-type, or --filter.");
        }

        // Require --sku when --service is provided (broad service queries return too many results)
        if (!string.IsNullOrEmpty(options.Service) && string.IsNullOrEmpty(options.Sku))
        {
            validationResult.Errors.Add(
                $"When querying by service '{options.Service}', you must also specify --sku to narrow results. " +
                "Ask the user which specific SKU they want pricing for. " +
                "Examples: --sku Standard_D4s_v5 (for VMs), --sku Standard_LRS (for Storage), --sku GP_Gen5_2 (for SQL).");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        PricingGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug(
                "Getting Azure pricing. SKU: {Sku}, Service: {Service}, Region: {Region}, " +
                "ServiceFamily: {ServiceFamily}, PriceType: {PriceType}, Currency: {Currency}",
                options.Sku,
                options.Service,
                options.Region,
                options.ServiceFamily,
                options.PriceType,
                options.Currency ?? "USD");

            var prices = await _pricingService.GetPricesAsync(
                sku: options.Sku,
                service: options.Service,
                region: options.Region,
                serviceFamily: options.ServiceFamily,
                priceType: options.PriceType,
                currency: options.Currency,
                includeSavingsPlan: options.IncludeSavingsPlan,
                filter: options.Filter,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(prices),
                PricingJsonContext.Default.PricingGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Azure pricing. Service: {Service}, Region: {Region}.", options.Service, options.Region);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record PricingGetCommandResult(List<PriceItem> Prices);
}
