// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
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
public sealed class PricingGetCommand(ILogger<PricingGetCommand> logger) : BasePricingCommand<PricingGetOptions>
{
    private const string CommandTitle = "Get Azure Retail Pricing";
    private readonly ILogger<PricingGetCommand> _logger = logger;

    public override string Id => "c5a8f7d2-9e3b-4a1c-8d6f-2b5e9c4a7d3e";

    public override string Name => "get";

    public override string Description =>
    "Get Azure retail pricing information. " +
    "MANDATORY: Do NOT call this tool if the user only specifies a broad service name (e.g., 'Virtual Machines', 'Storage', 'SQL Database') without a specific SKU. " +
    "Instead, FIRST ask the user which specific SKU or tier they want pricing for. " +
    "If the user asks to compare pricing across regions or SKUs without specifying exact SKU names, ask them which specific SKUs they want to compare. " +
    "Do NOT assume or pick default SKUs. " +
    "Only call this tool AFTER the user provides a specific SKU (--sku) or confirms they want all pricing for that service. " +
    "Requires at least one filter: --sku, --service, --region, --service-family, or --filter. " +
    "SAVINGS PLAN: 'SavingsPlan' is NOT a valid --price-type. Use --include-savings-plan flag instead. " +
    "Valid --price-type values: Consumption, Reservation, DevTestConsumption. " +
    "When --include-savings-plan is true, Consumption items include nested 'savingsPlan' array with 1-year/3-year pricing (mainly Linux VMs).";
    
    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false,
        LocalRequired = false
    };

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
            // if (!string.IsNullOrEmpty(service) && string.IsNullOrEmpty(sku))
            // {
            //     result.AddError(
            //         $"When querying by service '{service}', you must also specify --sku to narrow results. " +
            //         "Ask the user which specific SKU they want pricing for. " +
            //         "Examples: --sku Standard_D4s_v5 (for VMs), --sku Standard_LRS (for Storage), --sku GP_Gen5_2 (for SQL).");
            // }
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

            var pricingService = context.GetService<IPricingService>();
            var prices = await pricingService.GetPricesAsync(
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
                new PricingGetCommandResult(prices),
                PricingJsonContext.Default.PricingGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Azure pricing. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record PricingGetCommandResult(List<PriceItem> Prices);
}
