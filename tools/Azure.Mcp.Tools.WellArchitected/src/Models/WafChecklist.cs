// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitected.Models;

public record WafChecklist(
    string Pillar,
    List<WafChecklistItem> Items);
