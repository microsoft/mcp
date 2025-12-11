// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.FileShare;

public class FileShareDeleteOptions : BaseFileSharesOptions
{
    public string? Name { get; set; }

    public bool Force { get; set; }
}
