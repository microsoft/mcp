// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Commands;

[CommandMetadata(
    Id = "3b8bc9c0-b833-4a61-9278-d58e366a70d7",
    Name = "search_catalog",
    Title = "Search Catalog",
    Description = "Searches the Microsoft Fabric OneLake catalog for items matching the specified criteria. Supports cross-workspace search over catalog metadata and returns results filtered to entries the calling principal is authorized to access. Use this when the user wants to discover or find Fabric items (Lakehouses, Reports, Notebooks, and other item types) across workspaces by name, description, or workspace name. Optionally filter by item type.",
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class CatalogSearchCommand(
    ILogger<CatalogSearchCommand> logger,
    IFabricCoreService fabricCoreService) : AuthenticatedCommand<CatalogSearchOptions, CatalogSearchCommandResult>
{
    private readonly ILogger<CatalogSearchCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override void ValidateOptions(CatalogSearchOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.PageSize is < 1 or > 1000)
        {
            validationResult.Errors.Add("Page size must be between 1 and 1000.");
        }
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

            var result = new CatalogSearchCommandResult(searchResults);
            context.Response.Results = ResponseResult.Create(result, CoreJsonContext.Default.CatalogSearchCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching catalog for '{Search}'.", options.Search);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
