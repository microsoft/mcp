// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options;

public class BaseNetAppFilesOptions : SubscriptionOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.AccountName)]
    public string? Account { get; set; }
}
