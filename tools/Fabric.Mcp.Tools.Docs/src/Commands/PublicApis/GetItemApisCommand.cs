// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Docs.Models;
using Fabric.Mcp.Tools.Docs.Options.PublicApis;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Docs.Commands.PublicApis;

[CommandMetadata(
    Id = "97229a98-c1ae-4255-a6e2-07631c2a42c5",
    Name = "item-api-spec",
    Title = "Item API Specification",
    Description = "Retrieves the complete OpenAPI specification for a specific Microsoft Fabric item type. Use this when the user needs detailed API documentation for an item type such as notebook, lakehouse or report. Returns the full API spec in JSON format.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class GetItemApisCommand(IFabricPublicApiService service, ILogger<GetItemApisCommand> logger)
    : AuthenticatedCommand<ItemTypeOptions, FabricPublicApi>
{
    private readonly ILogger<GetItemApisCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricPublicApiService _service = service ?? throw new ArgumentNullException(nameof(service));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ItemTypeOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (options.ItemType.Equals("common", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Status = HttpStatusCode.NotFound;
                context.Response.Message = "No item type 'common' exists. Did you mean 'platform'? A full list of supported item types can be found using the 'list-item-types' tool.";
                return context.Response;
            }

            var apis = await _service.GetPublicApis(options.ItemType, cancellationToken);

            context.Response.Results = ResponseResult.Create(apis, FabricJsonContext.Default.FabricPublicApi);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP error getting Fabric public APIs for item type {ItemType}", options.ItemType);
            if (httpEx.StatusCode == HttpStatusCode.NotFound)
            {
                context.Response.Status = HttpStatusCode.NotFound;
                context.Response.Message = $"No item type '{options.ItemType}' exists. A full list of supported item types can be found using the 'list-item-types' tool.";
            }
            else
            {
                context.Response.Status = httpEx.StatusCode ?? HttpStatusCode.InternalServerError;
                context.Response.Message = httpEx.Message;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Fabric public APIs for item type {ItemType}", options.ItemType);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
