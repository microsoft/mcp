// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Cosmos.Commands.CopyJob;

internal record CopyJobResult(JsonElement Job);

internal record CopyJobListResult(List<JsonElement> Jobs);
