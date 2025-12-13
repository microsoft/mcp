// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.FileShare;

public class FileShareCreateOrUpdateOptions : BaseFileSharesOptions
{
    public string? Name { get; set; }

    public string? Location { get; set; }

    public Dictionary<string, string>? Tags { get; set; }

    public int? QuotaGiB { get; set; }
}
