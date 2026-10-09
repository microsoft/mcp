// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.IoTHub.Models;

namespace Azure.Mcp.Tools.IoTHub.Services;

internal sealed record ExistenceResult(
    string Status,
    DateTimeOffset ObservedAt,
    RoutingDiagnosticError? Error);
