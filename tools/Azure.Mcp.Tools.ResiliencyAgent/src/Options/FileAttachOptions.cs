// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResiliencyAgent.Options;

public sealed class FileAttachOptions
{
    [Option(
        Name = "file-path",
        Description =
            "Fully-qualified path of one user-selected local architecture or infrastructure-as-code file. " +
            "Do not pass a URL, directory, wildcard, inferred path, or a file the user did not select.")]
    public required string FilePath { get; set; }
}
