// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>Optional Create Item label settings. Omitted values retain Fabric's defaults.</summary>
public sealed record FabricSensitivityLabelSettings(Guid? LabelId = null, string? SensitivityLabelApplyStrategy = null);
