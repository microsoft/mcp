// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Account;

public class AccountGetOptions : BaseNetAppFilesOptions
{
	[JsonPropertyName(NetAppFilesOptionDefinitions.IdsName)]
	public string[]? Ids { get; set; }
}
