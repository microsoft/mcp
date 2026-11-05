// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class ItemCreateRequestInput
{
    internal const int MaxLabelSettingsBytes = 4096;

    internal static CreateItemRequest CreateRequest(ItemCreateOptions options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var hasDefinitionInput = options.DefinitionInput?.HasInput is true;
        if (options.CreationPayload is not null && hasDefinitionInput)
        {
            throw new FabricItemInputException(
                "--creation-payload cannot be combined with any inline or saved-file definition option.");
        }
        if (options.Options is not null && !hasDefinitionInput)
        {
            throw new FabricItemInputException("--options requires an inline or saved-file definition source.");
        }

        Guid? folderId = null;
        if (options.FolderId is not null)
        {
            if (!Guid.TryParse(options.FolderId, out var parsedFolderId) || parsedFolderId == Guid.Empty)
            {
                throw new FabricItemInputException("--folder-id must be a nonempty UUID for an existing Fabric folder.");
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
            CreationPayload = FabricItemJsonInput.ParseObject(options.CreationPayload, "--creation-payload"),
            Options = FabricItemJsonInput.ParseObject(options.Options, "--options"),
            SensitivityLabelSettings = ParseLabelSettings(options.SensitivityLabelSettings)
        };
        cancellationToken.ThrowIfCancellationRequested();
        return request;
    }

    private static FabricSensitivityLabelSettings? ParseLabelSettings(string? json)
    {
        var value = FabricItemJsonInput.ParseObject(json, "--sensitivity-label-settings", MaxLabelSettingsBytes);
        if (value is null)
        {
            return null;
        }

        FabricSensitivityLabelSettings settings;
        try
        {
            settings = value.Value.Deserialize(FabricItemCreateInputJsonContext.Default.FabricSensitivityLabelSettings) ??
                throw new FabricItemInputException("--sensitivity-label-settings must be a JSON object.");
        }
        catch (JsonException)
        {
            throw new FabricItemInputException(
                "--sensitivity-label-settings requires labelId (a nonempty UUID) and accepts optional sensitivityLabelApplyStrategy (a string).");
        }
        if (settings.LabelId == Guid.Empty)
        {
            throw new FabricItemInputException("--sensitivity-label-settings requires a nonempty labelId UUID.");
        }
        if (value.Value.TryGetProperty("sensitivityLabelApplyStrategy", out _) &&
            settings.SensitivityLabelApplyStrategy is not ("ApplyOrFail" or "Ignore"))
        {
            throw new FabricItemInputException(
                "A supplied sensitivityLabelApplyStrategy must be ApplyOrFail or Ignore. Omit it to retain Fabric's ApplyOrFail default.");
        }

        // Do not fill defaults or replace a requested policy with Ignore on failure.
        return settings;
    }
}
