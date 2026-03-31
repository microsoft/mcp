// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Account;

public class AccountUpdateOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.TagsName)]
    public string? Tags { get; set; }
}
