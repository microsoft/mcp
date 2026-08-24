// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Docs.Options.PublicApis;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Docs.Commands.BestPractices;

[CommandMetadata(
    Id = "445c49f3-2a5d-478a-82ca-87fde1a7943e",
    Name = "item-definitions",
    Title = "Item Definitions",
    Description = "Retrieves the JSON schema definition for a Microsoft Fabric item type. Use this when the user needs to understand an item's structure or validate an item definition. Returns the schema definition for the specified item type.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class GetItemDefinitionCommand(IFabricPublicApiService service, ILogger<GetItemDefinitionCommand> logger)
    : AuthenticatedCommand<ItemTypeOptions, string>
{
    private readonly ILogger<GetItemDefinitionCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricPublicApiService _service = service ?? throw new ArgumentNullException(nameof(service));

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ItemTypeOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var itemDefinition = _service.GetItemDefinition(options.ItemType);

            context.Response.Results = ResponseResult.Create(itemDefinition, FabricJsonContext.Default.String);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogError(argEx, "Invalid argument for item type {ItemType}", options.ItemType);
            context.Response.Status = HttpStatusCode.NotFound;
            context.Response.Message = $"No item definition found for item type {options.ItemType}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting item definition for item type {ItemType}", options.ItemType);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
