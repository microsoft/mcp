// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.Informational;

public class FileShareGetUsageDataOptions : BaseFileSharesOptions
{
    public string? Location { get; set; }

    public string? TimeRange { get; set; }
}
