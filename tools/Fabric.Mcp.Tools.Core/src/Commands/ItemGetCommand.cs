using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Core.Commands;

/// <summary>Retrieves generic metadata for one Fabric item without reading its data or definition.</summary>
/// <param name="logger">The logger for sanitized operation diagnostics.</param>
/// <param name="fabricCoreService">The service that retrieves and validates item metadata.</param>
[CommandMetadata(
    Id = "65e8b362-6e77-44a0-bae2-9e1f7cde3515",
    Name = "get-item",
    Title = "Get Fabric Item",
    Description = """
        Gets metadata for one existing Microsoft Fabric item, such as a Lakehouse, Notebook, or Report.
        Use this to inspect an item when its workspace ID and item ID are known. Both IDs are required UUIDs.
        Returns the item ID, display name, description when available, type, and workspace ID.
        Does not return item data, definitions, or workload-specific properties, and does not modify the item.
        Use search-catalog to discover items when their IDs are unknown.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class ItemGetCommand(
    ILogger<ItemGetCommand> logger,
    IFabricCoreService fabricCoreService) : AuthenticatedCommand<ItemGetOptions, ItemGetCommandResult>
{
    private readonly ILogger<ItemGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    /// <inheritdoc />
    public override JsonTypeInfo<ItemGetCommandResult> ResultTypeInfo => CoreJsonContext.Default.ItemGetCommandResult;

    /// <inheritdoc />
    public override void ValidateOptions(ItemGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!Guid.TryParse(options.WorkspaceId, out var workspaceId) || workspaceId == Guid.Empty)
        {
            validationResult.Errors.Add("--workspace-id must be a nonempty UUID.");
        }

        if (!Guid.TryParse(options.ItemId, out var itemId) || itemId == Guid.Empty)
        {
            validationResult.Errors.Add("--item-id must be a nonempty UUID.");
        }
    }

    /// <inheritdoc />
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ItemGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _fabricCoreService.GetItemAsync(
                options.WorkspaceId,
                options.ItemId,
                cancellationToken);

            SetResult(context, new(item));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving Fabric item metadata ({ExceptionType}).", ex.GetType().Name);
            HandleException(context, ex);
            context.Response.Results = null;
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        AuthenticationFailedException => HttpStatusCode.Unauthorized,
        OperationCanceledException => HttpStatusCode.RequestTimeout,
        JsonException => HttpStatusCode.BadGateway,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex) => GetStatusCode(ex) switch
    {
        HttpStatusCode.BadRequest => "Provide valid workspace and item UUIDs",
        HttpStatusCode.Unauthorized => "Authentication failed. Sign in with an identity that can read the Fabric item",
        HttpStatusCode.Forbidden => "Access denied. The caller needs read permission for this Fabric item",
        HttpStatusCode.NotFound => "The Fabric item was not found. Check its workspace and item IDs",
        HttpStatusCode.TooManyRequests when ex is FabricItemThrottledException => ex.Message,
        HttpStatusCode.TooManyRequests => "Fabric throttled the request. Wait before retrying",
        HttpStatusCode.RequestTimeout => "The Fabric item request was canceled or timed out",
        HttpStatusCode.BadGateway => "Fabric returned an invalid item metadata response",
        _ => "Unable to retrieve Fabric item metadata. Retry the request or check service availability"
    };
}
