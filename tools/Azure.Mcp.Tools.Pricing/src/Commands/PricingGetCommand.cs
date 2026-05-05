// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
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
        For Bicep/ARM cost estimation, extract resource type and SKU, query per resource, and sum monthly costs (hourly × 730).
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class PricingGetCommand(ILogger<PricingGetCommand> logger, IPricingService pricingService) : BasePricingCommand<PricingGetOptions>
{
    private readonly ILogger<PricingGetCommand> _logger = logger;
    private readonly IPricingService _pricingService = pricingService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(PricingOptionDefinitions.Sku);
        command.Options.Add(PricingOptionDefinitions.Service);
        command.Options.Add(PricingOptionDefinitions.Region);
        command.Options.Add(PricingOptionDefinitions.ServiceFamily);
        command.Options.Add(PricingOptionDefinitions.PriceType);
        command.Options.Add(PricingOptionDefinitions.IncludeSavingsPlan);
        command.Options.Add(PricingOptionDefinitions.Filter);

        // Add validation: at least one filter must be provided
        command.Validators.Add(result =>
        {
            var sku = result.GetValue(PricingOptionDefinitions.Sku);
            var service = result.GetValue(PricingOptionDefinitions.Service);
            var region = result.GetValue(PricingOptionDefinitions.Region);
            var serviceFamily = result.GetValue(PricingOptionDefinitions.ServiceFamily);
            var priceType = result.GetValue(PricingOptionDefinitions.PriceType);
            var filter = result.GetValue(PricingOptionDefinitions.Filter);

            if (string.IsNullOrEmpty(sku) &&
                string.IsNullOrEmpty(service) &&
                string.IsNullOrEmpty(region) &&
                string.IsNullOrEmpty(serviceFamily) &&
                string.IsNullOrEmpty(priceType) &&
                string.IsNullOrEmpty(filter))
            {
                result.AddError("At least one filter is required. " +
                    "Specify --sku, --service, --region, --service-family, --price-type, or --filter.");
            }

            // Require --sku when --service is provided (broad service queries return too many results)
            if (!string.IsNullOrEmpty(service) && string.IsNullOrEmpty(sku))
            {
                result.AddError(
                    $"When querying by service '{service}', you must also specify --sku to narrow results. " +
                    "Ask the user which specific SKU they want pricing for. " +
                    "Examples: --sku Standard_D4s_v5 (for VMs), --sku Standard_LRS (for Storage), --sku GP_Gen5_2 (for SQL).");
            }
        });
    }

    protected override PricingGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Sku = parseResult.GetValue(PricingOptionDefinitions.Sku);
        options.Service = parseResult.GetValue(PricingOptionDefinitions.Service);
        options.Region = parseResult.GetValue(PricingOptionDefinitions.Region);
        options.ServiceFamily = parseResult.GetValue(PricingOptionDefinitions.ServiceFamily);
        options.PriceType = parseResult.GetValue(PricingOptionDefinitions.PriceType);
        options.IncludeSavingsPlan = parseResult.GetValue(PricingOptionDefinitions.IncludeSavingsPlan);
        options.Filter = parseResult.GetValue(PricingOptionDefinitions.Filter);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

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

    public record PricingGetCommandResult(List<PriceItem> Prices);
}
