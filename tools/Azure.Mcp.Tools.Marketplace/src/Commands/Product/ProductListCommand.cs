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

public sealed class ProductListCommand(ILogger<ProductListCommand> logger) : SubscriptionCommand<ProductListOptions>
{
    private const string CommandTitle = "List Marketplace Products";
    private readonly ILogger<ProductListCommand> _logger = logger;

    // Define options from OptionDefinitions
    private readonly Option<string> _languageOption = OptionDefinitions.Marketplace.Language;
    private readonly Option<string> _searchOption = OptionDefinitions.Marketplace.Search;
    private readonly Option<string> _filterOption = OptionDefinitions.Marketplace.Filter;
    private readonly Option<string> _orderByOption = OptionDefinitions.Marketplace.OrderBy;
    private readonly Option<string> _selectOption = OptionDefinitions.Marketplace.Select;
    private readonly Option<string> _nextCursorOption = OptionDefinitions.Marketplace.NextCursor;
    private readonly Option<string> _expandOption = OptionDefinitions.Marketplace.Expand;

    public override string Name => "list";

    public override string Description =>
        """
        Retrieves products (offers) that a subscription has access to in the Azure Marketplace.
        Returns a list of marketplace products including display names, publishers, pricing information, and metadata.

        Required options:
        - subscription: Azure subscription ID or name

        Additional options:
        - search: Search for products using a short general term (up to 25 characters)
        - language: Specify the language for returned information (default: en)

        Advanced query options (OData):
        - filter: Filter results using OData syntax (e.g., "displayName eq 'Azure'")
        - orderby: Sort results by a single field using OData syntax (e.g., "displayName asc")
        - select: Select specific fields using OData syntax (e.g., "displayName,publisherDisplayName")
        - expand: Include related data in the response using OData syntax (e.g., "plans" to include plan details)

        Pagination:
        - next-cursor: Token used for pagination to request the next batch of products in a multi-part response
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        // Add marketplace-specific options
        var options = command.Options;
        options.Add(_languageOption);
        options.Add(_searchOption);
        options.Add(_filterOption);
        options.Add(_orderByOption);
        options.Add(_selectOption);
        options.Add(_nextCursorOption);
        options.Add(_expandOption);
    }

    protected override ProductListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Language = parseResult.GetValue(_languageOption);
        options.Search = parseResult.GetValue(_searchOption);
        options.Filter = parseResult.GetValue(_filterOption);
        options.OrderBy = parseResult.GetValue(_orderByOption);
        options.Select = parseResult.GetValue(_selectOption);
        options.NextCursor = parseResult.GetValue(_nextCursorOption);
        options.Expand = parseResult.GetValue(_expandOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            // Required validation step
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }


            // Get the marketplace service from DI
            var marketplaceService = context.GetService<IMarketplaceService>();

            // Call service operation with required parameters
            var results = await marketplaceService.ListProducts(
                options.Subscription!,
                options.Language,
                options.Search,
                options.Filter,
                options.OrderBy,
                options.Select,
                options.NextCursor,
                options.Expand,
                options.Tenant,
                options.RetryPolicy);

            // Set results
            context.Response.Results = results.Items?.Count > 0 ?
                ResponseResult.Create(
                    new ProductListCommandResult(results.Items, results.NextCursor),
                    MarketplaceJsonContext.Default.ProductListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            // Log error with all relevant context
            _logger.LogError(ex,
                "Error listing marketplace products. Subscription: {Subscription}, Search: {Search}, Options: {@Options}",
                options.Subscription, options.Search, options);
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

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        HttpRequestException httpEx => (int)httpEx.StatusCode.GetValueOrDefault(HttpStatusCode.InternalServerError),
        ArgumentException => 400,
        _ => base.GetStatusCode(ex)
    };

    // Strongly-typed result record
    internal record ProductListCommandResult(List<ProductSummary> Products, string? NextCursor);
}
