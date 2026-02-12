// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.MySql.Options;

/// <summary>
/// Options for the consolidated MySQL list command that supports hierarchical listing.
/// </summary>
public class MySqlListOptions : BaseMySqlOptions
{
    public string? Server { get; set; }
    public string? Database { get; set; }
}
