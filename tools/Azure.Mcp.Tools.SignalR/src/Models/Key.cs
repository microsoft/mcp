// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SignalR.Models;

public class Key
{
    /// <summary> The type of the key (Primary or Secondary). </summary>
    public string? KeyType { get; set; }

    /// <summary> The connection string for this key. </summary>
    public string? ConnectionString { get; set; }

    /// <summary> The primary key value. </summary>
    public string? PrimaryKey { get; set; }

    /// <summary> The secondary key value. </summary>
    public string? SecondaryKey { get; set; }

    /// <summary> The primary connection string. </summary>
    public string? PrimaryConnectionString { get; set; }

    /// <summary> The secondary connection string. </summary>
    public string? SecondaryConnectionString { get; set; }
}
