// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.FileShare;

/// <summary>
/// Options for FileShareGetCommand.
/// </summary>
public class FileShareGetOptions : BaseFileSharesOptions
{
    /// <summary>
    /// Gets or sets the name of the file share to retrieve.
    /// </summary>
    public string? FileShareName { get; set; }
}
