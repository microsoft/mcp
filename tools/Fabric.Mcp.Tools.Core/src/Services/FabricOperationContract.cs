// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;

namespace Fabric.Mcp.Tools.Core.Services;

// A null reader explicitly declares a documented empty result, not unknown result availability.
internal sealed record FabricOperationContract<T>(
    IReadOnlySet<HttpStatusCode> CompletedStatusCodes,
    Func<JsonElement, T>? ReadResult) where T : class;
