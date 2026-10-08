// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Core.Commands;

[CommandMetadata(
    Id = "3b8bc9c0-b833-4a61-9278-d58e366a70d7",
    Name = "search-catalog",
    Title = "Search Catalog",
    Description = """
        Searches one page of Microsoft Fabric OneLake catalog metadata across workspaces, limited to entries
        the calling principal can access. Find Fabric items by name, description, or workspace name, optionally
        filtering by item type. Returns typed entries with available workspace hierarchy and a continuation token.
        For the next page, send the returned continuation-token without search or filter: the token already
        carries the original search, filter, and page size. An explicitly supplied page-size must be 1 through 1000.
        Does not fetch all pages, read item data, or retry automatically. Delegated callers need Catalog.Read.All.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class CatalogSearchCommand(ILogger<CatalogSearchCommand> logger, IFabricCoreService fabricCoreService)
    : FabricCoreCommand<CatalogSearchOptions, CatalogSearchCommandResult>
{
    private readonly ILogger<CatalogSearchCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override void ValidateOptions(CatalogSearchOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        CatalogSearchInputValidator.Validate(options.Search, options.Filter, options.PageSize, options.ContinuationToken, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CatalogSearchOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var request = new CatalogSearchRequest
            {
                Search = options.Search,
                Filter = options.Filter,
                PageSize = options.PageSize,
                ContinuationToken = options.ContinuationToken
            };

            var searchResults = await _fabricCoreService.SearchCatalogAsync(request, cancellationToken);

            _logger.LogInformation("Catalog search for '{Search}' returned {Count} entries.",
                options.Search, searchResults.Value?.Count ?? 0);

            context.Response.Results = ResponseResult.Create(new(searchResults), CoreJsonContext.Default.CatalogSearchCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching catalog for '{Search}'.", options.Search);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        if (ex is CommandValidationException validationException)
        {
            context.Response.Message = GetValidationErrorMessage(validationException,
                "Invalid catalog search request. Check option names and values; page-size must be an integer from 1 through 1000.");
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        CredentialUnavailableException =>
            "Fabric credentials are unavailable. Authenticate the configured Fabric identity before searching the catalog",
        HttpRequestException { StatusCode: null } =>
            "Service unavailable or network connectivity issues prevented the Fabric catalog search. Check connectivity and retry later",
        OperationCanceledException or TimeoutException =>
            "The Fabric catalog search timed out or was canceled. Retry the search later",
        _ => GetStatusCode(ex) switch
        {
            HttpStatusCode.BadRequest =>
                "Fabric rejected the catalog search. Check the search criteria, filter, page size, and continuation token",
            HttpStatusCode.Unauthorized =>
                "Authentication failed while searching the Fabric catalog. Check the configured Fabric identity and its catalog access",
            HttpStatusCode.Forbidden =>
                "Fabric denied the catalog search. Check permissions and whether catalog search is supported and enabled for the tenant and capacity",
            HttpStatusCode.NotFound =>
                "The Fabric catalog search resource was not found, or the caller does not have access",
            HttpStatusCode.Conflict =>
                "The catalog search conflicts with the current Fabric state. Check the catalog state before retrying",
            HttpStatusCode.TooManyRequests =>
                "Fabric throttled the catalog search or reached a capacity limit. Retry the request later",
            _ when ex is HttpRequestException { StatusCode: { } statusCode } =>
                $"Fabric catalog search failed with HTTP {(int)statusCode}. Check service availability before retrying",
            _ =>
                "The Fabric catalog search could not be completed. Check service availability before retrying"
        }
    };
}
