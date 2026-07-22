// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.FileShare;

/// <summary>
/// Options for FileShareCheckNameAvailabilityCommand.
/// </summary>
public class FileShareCheckNameAvailabilityOptions : BaseFileSharesOptions
{
    /// <summary>
    /// Gets or sets the name of the file share to check availability for.
    /// </summary>
    public string? FileShareName { get; set; }

    /// <summary>
    /// Gets or sets the location to check name availability in.
    /// </summary>
    public string? Location { get; set; }
}
