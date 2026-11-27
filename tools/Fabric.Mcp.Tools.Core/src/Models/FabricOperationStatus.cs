// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter<FabricOperationStatus>))]
public enum FabricOperationStatus
{
    Accepted,
    AcceptedWithoutOperationId,
    Pending,
    Succeeded,
    Failed,
    TrackingStopped,
    ResultUnavailable
}
