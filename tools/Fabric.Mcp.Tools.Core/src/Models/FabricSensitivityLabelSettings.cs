// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>Label settings require a label ID; an omitted strategy retains Fabric's default.</summary>
public sealed record FabricSensitivityLabelSettings(Guid LabelId, string? SensitivityLabelApplyStrategy = null);
