// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class ItemCreateRequestInput
{
    internal const int MaxObjectBytes = 1024 * 1024;
    internal const int MaxLabelSettingsBytes = 4096;
    internal const int MaxObjectDepth = 32;

    internal static CreateItemRequest CreateRequest(ItemCreateOptions options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var hasDefinitionInput = options.DefinitionInput?.HasInput is true;
        if (options.CreationPayload is not null && hasDefinitionInput)
        {
            throw new ItemCreateInputException(
                "--creation-payload cannot be combined with any inline or saved-file definition option.");
        }
        if (options.Options is not null && !hasDefinitionInput)
        {
            throw new ItemCreateInputException("--options requires an inline or saved-file definition source.");
        }

        Guid? folderId = null;
        if (options.FolderId is not null)
        {
            if (!Guid.TryParse(options.FolderId, out var parsedFolderId) || parsedFolderId == Guid.Empty)
            {
                throw new ItemCreateInputException("--folder-id must be a nonempty UUID for an existing Fabric folder.");
            }
            folderId = parsedFolderId;
        }

        // Validate every new field before the caller can read definition files or submit creation.
        // Workload properties remain opaque JSON; their per-item semantics belong to Fabric.
        var request = new CreateItemRequest
        {
            DisplayName = options.DisplayName,
            Type = options.ItemType,
            Description = options.Description,
            FolderId = folderId,
            CreationPayload = ParseObject(options.CreationPayload, "--creation-payload", MaxObjectBytes),
            Options = ParseObject(options.Options, "--options", MaxObjectBytes),
            SensitivityLabelSettings = ParseLabelSettings(options.SensitivityLabelSettings)
        };
        cancellationToken.ThrowIfCancellationRequested();
        return request;
    }

    private static JsonElement? ParseObject(string? json, string option, int maxBytes)
    {
        if (json is null)
        {
            return null;
        }
        if (json.Length > maxBytes || Encoding.UTF8.GetByteCount(json) > maxBytes)
        {
            throw new ItemCreateInputException(FormattableString.Invariant(
                $"{option} must not exceed {maxBytes} UTF-8 bytes."));
        }
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                MaxDepth = MaxObjectDepth,
                AllowDuplicateProperties = false
            });
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new ItemCreateInputException($"{option} must be a JSON object, not null, an array, or a scalar.");
            }
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new ItemCreateInputException(FormattableString.Invariant(
                $"{option} must contain valid JSON with no duplicate properties and at most {MaxObjectDepth} nesting levels."));
        }
    }

    private static FabricSensitivityLabelSettings? ParseLabelSettings(string? json)
    {
        var value = ParseObject(json, "--sensitivity-label-settings", MaxLabelSettingsBytes);
        if (value is null)
        {
            return null;
        }

        FabricSensitivityLabelSettings settings;
        try
        {
            settings = value.Value.Deserialize(FabricItemCreateInputJsonContext.Default.FabricSensitivityLabelSettings) ??
                throw new ItemCreateInputException("--sensitivity-label-settings must be a JSON object.");
        }
        catch (JsonException)
        {
            throw new ItemCreateInputException(
                "--sensitivity-label-settings accepts only labelId (a UUID) and sensitivityLabelApplyStrategy (a string).");
        }
        if (value.Value.TryGetProperty("labelId", out _) && (settings.LabelId is null || settings.LabelId == Guid.Empty))
        {
            throw new ItemCreateInputException("A supplied sensitivity labelId must be a nonempty UUID; omit the property instead of using null.");
        }
        if (value.Value.TryGetProperty("sensitivityLabelApplyStrategy", out _) &&
            settings.SensitivityLabelApplyStrategy is not ("ApplyOrFail" or "Ignore"))
        {
            throw new ItemCreateInputException(
                "A supplied sensitivityLabelApplyStrategy must be ApplyOrFail or Ignore. Omit it to retain Fabric's ApplyOrFail default.");
        }

        // Do not fill defaults or replace a requested policy with Ignore on failure.
        return settings;
    }
}
