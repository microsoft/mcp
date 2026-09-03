// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Account;

public class AccountGetOptions : BaseNetAppFilesOptions
{
	[Option(Description = NetAppFilesOptionDefinitions.Ids)]
	public string[]? Ids { get; set; }
}
