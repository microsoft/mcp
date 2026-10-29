// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    public async Task<ItemDefinitionUpdateCommandResult> UpdateItemDefinitionAsync(
        string workspaceId,
        string itemId,
        UpdateItemDefinitionRequest request,
        bool? updateMetadata = null,
        FabricOperationWaitOptions? waitOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!Guid.TryParse(workspaceId, out var parsedWorkspaceId) || parsedWorkspaceId == Guid.Empty)
        {
            throw new ArgumentException("Workspace ID must be a nonempty UUID.", nameof(workspaceId));
        }
        if (!Guid.TryParse(itemId, out var parsedItemId) || parsedItemId == Guid.Empty)
        {
            throw new ArgumentException("Item ID must be a nonempty UUID.", nameof(itemId));
        }

        waitOptions ??= new();
        waitOptions.Validate();
        cancellationToken.ThrowIfCancellationRequested();
        _definitionValidator.Validate(request.Definition, cancellationToken);
        var options = FabricItemJsonInput.ParseObject(request.Options?.GetRawText(), "--options");
        cancellationToken.ThrowIfCancellationRequested();

        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{parsedWorkspaceId:D}/items/{parsedItemId:D}/updateDefinition";
        if (updateMetadata is { } requestedUpdateMetadata)
        {
            url += requestedUpdateMetadata ? "?updateMetadata=true" : "?updateMetadata=false";
        }
        var jsonContent = JsonSerializer.Serialize(request with { Options = options }, CoreJsonContext.Default.UpdateItemDefinitionRequest);
        using var response = await SendFabricHttpRequestAsync(HttpMethod.Post, url, jsonContent,
            completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);

        // This endpoint never returns an item or operation result, even after LRO success.
        var contract = new FabricOperationContract<object>(new HashSet<HttpStatusCode> { HttpStatusCode.OK }, ReadResult: null);
        var outcome = await new FabricOperationRunner(_timeProvider).HandleAsync(
            response, contract, waitOptions, GetOperationStateAsync, GetOperationResultAsync, cancellationToken);
        return new(outcome.Operation ?? new(FabricOperationStatus.Succeeded, FabricOperationHttp.GetOperationId(response)));
    }
}
