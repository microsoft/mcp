// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>
/// One JSON response from an allowlisted Fabric or Power BI REST endpoint.
/// Links are relative paths on the same host; links to other hosts are dropped.
/// </summary>
public sealed record FabricApiResponse(
    string Api,
    int Status,
    bool HasBody,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] JsonElement Value,
    string? NextPath = null,
    string? Location = null,
    double? RetryAfterSeconds = null,
    string? RequestId = null);
