// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.FileShare;

/// <summary>
/// Options for FileShareDeleteCommand.
/// </summary>
public class FileShareDeleteOptions : BaseFileSharesOptions
{
    /// <summary>
    /// Gets or sets the name of the file share to delete.
    /// </summary>
    public string? FileShareName { get; set; }
}
