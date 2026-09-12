// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventHubs.Models;

public enum NamespaceSkuTier
{
    [JsonStringEnumMemberName("Basic")]
    Basic,

    [JsonStringEnumMemberName("Standard")]
    Standard,

    [JsonStringEnumMemberName("Premium")]
    Premium
}
