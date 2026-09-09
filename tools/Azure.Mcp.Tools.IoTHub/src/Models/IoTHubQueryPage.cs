// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.IoTHub.Models;

public record IoTHubQueryPage(
    List<JsonElement> Items,
    string? ContinuationToken);
