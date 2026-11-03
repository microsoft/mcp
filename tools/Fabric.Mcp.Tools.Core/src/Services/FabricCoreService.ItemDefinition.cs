// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    // Definition bodies include Base64 plus JSON/path overhead. This wire limit is
    // separate from decoded file budgets and must not widen other operation contracts.
    internal const int MaxDefinitionJsonBytes = 64 * 1024 * 1024;
    private const string InvalidDefinitionMessage =
        "Fabric returned an invalid item definition. Expected a definition envelope with nonempty parts, " +
        "portable unique paths, InlineBase64 payloads, and content within the configured byte/count limits.";
    private static readonly CoreJsonContext DefinitionResponseContext = new(new(CoreJsonContext.Default.Options)
    {
        AllowDuplicateProperties = false,
        RespectRequiredConstructorParameters = true
    });

    public async Task<ItemDefinitionGetCommandResult> GetItemDefinitionAsync(
        string? workspaceId = null,
        string? itemId = null,
        string? format = null,
        string? operationId = null,
        FabricOperationWaitOptions? waitOptions = null,
        CancellationToken cancellationToken = default)
    {
        waitOptions ??= new();
        var ids = ItemDefinitionGetInput.Validate(workspaceId, itemId, format, operationId, waitOptions);
        cancellationToken.ThrowIfCancellationRequested();

        if (ids.OperationId is { } completedOperation)
        {
            var result = await GetOperationResultAsync(completedOperation, MaxDefinitionJsonBytes, cancellationToken);
            return new(ReadItemDefinition(result.Value, cancellationToken),
                new(FabricOperationStatus.Succeeded, completedOperation));
        }

        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{ids.WorkspaceId:D}/items/{ids.ItemId:D}/getDefinition";
        if (format is not null)
        {
            url += $"?format={Uri.EscapeDataString(format)}";
        }
        using var response = await SendFabricHttpRequestAsync(HttpMethod.Post, url,
            completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);
        var contract = new FabricOperationContract<FabricItemDefinition>(
            new HashSet<HttpStatusCode> { HttpStatusCode.OK },
            value => ReadItemDefinition(value, cancellationToken),
            MaxDefinitionJsonBytes);
        try
        {
            var outcome = await new FabricOperationRunner(_timeProvider).HandleAsync(
                response, contract, waitOptions, GetOperationStateAsync,
                (id, token) => GetOperationResultAsync(id, MaxDefinitionJsonBytes, token), cancellationToken);
            return new(outcome.Result, outcome.Operation is { } operation ? DefinitionReceipt(operation) : null);
        }
        catch (FabricOperationCanceledException canceled)
        {
            throw new FabricOperationCanceledException(DefinitionReceipt(canceled.Operation), cancellationToken);
        }
    }

    private FabricItemDefinition ReadItemDefinition(JsonElement value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            if (value.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException();
            }
            var response = value.Deserialize(DefinitionResponseContext.FabricItemDefinitionResponse);
            if (response?.Definition is not { } definition)
            {
                throw new JsonException();
            }
            _definitionValidator.Validate(definition, cancellationToken);
            return definition;
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException or InvalidDataException)
        {
            throw new FabricOperationException("InvalidItemDefinition", InvalidDefinitionMessage);
        }
    }

    private static FabricOperationReceipt DefinitionReceipt(FabricOperationReceipt receipt)
    {
        if (receipt.Issue is not { } issue)
        {
            return receipt;
        }
        var message = issue.Code switch
        {
            "MissingOperationId" => "Fabric accepted definition retrieval but returned no usable operation ID. Completion is unconfirmed.",
            "OperationFailed" => "Fabric reports that definition retrieval failed. Inspect the safe error code and request ID.",
            "WaitBudgetExpired" => receipt.Status == FabricOperationStatus.ResultUnavailable
                ? "The operation succeeded, but its definition result was not retrieved within the local wait budget."
                : "The local wait budget expired. Definition retrieval may still be running; the Fabric operation was not canceled.",
            "CallerCanceled" => "The caller canceled the local wait, not the Fabric operation.",
            "ReadTimedOut" => "The operation read timed out; the Fabric operation was not canceled.",
            "InvalidItemDefinition" => InvalidDefinitionMessage,
            "InvalidJson" => "Fabric returned malformed operation JSON. No content was fabricated or truncated.",
            "ResultTooLarge" => receipt.Status == FabricOperationStatus.TrackingStopped
                ? "Operation state JSON exceeds the 1 MiB (1,048,576 byte) limit. No content was truncated."
                : "Definition JSON exceeds the 64 MiB (67,108,864 byte) limit. No content was truncated.",
            "UnsupportedResultMediaType" => "Fabric operation reads require application/json. Binary and other media types are not downloaded.",
            "MissingResult" => "Fabric reported completion but did not return the required definition JSON.",
            "InvalidOperationId" => "Fabric returned inconsistent operation identification. Use only the original operation ID.",
            "InvalidOperationState" => "Fabric returned an invalid operation state. Completion is unconfirmed.",
            _ => "Definition retrieval or tracking could not be completed. Check Fabric availability, operation state, and read/write item permissions."
        };
        return receipt with
        {
            Issue = issue with
            {
                Message = message + " No definition was returned or saved." + (receipt.OperationId is null ? "" :
                    " Resume with get-operation-state; when Succeeded, use get-item-definition with --operation-id instead of workspace/item IDs to read or export the result without another POST.")
            }
        };
    }
}
