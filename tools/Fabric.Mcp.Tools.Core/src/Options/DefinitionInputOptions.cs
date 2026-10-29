// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words TMDL TMSL PBIR

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

/// <summary>Optional definition sources. Each operation decides whether a source is required.</summary>
public sealed class DefinitionInputOptions()
{
    [Option(Description = "Inline definition JSON as a string: {\"parts\":[{\"path\":\"logical/path\",\"payload\":\"base64\",\"payloadType\":\"InlineBase64\"}],\"format\":\"optional format\"}. Supported in local and HTTP modes. Mutually exclusive with every definition-directory/files/format option. Supply only the definition envelope, not a file path or an API request body.")]
    public string? Definition { get; set; }

    [Option(Description = "Existing absolute local item directory containing saved definition files. Local execution only; unavailable over HTTP. Requires definition-files; never scans the folder automatically. Mutually exclusive with inline definition.")]
    public string? DefinitionDirectory { get; set; }

    [Option(Description = "Explicit file selection as a JSON array string: [{\"partPath\":\"artifact.content.ipynb\",\"sourcePath\":\"sales.ipynb\"}]. Paths are relative to definition-directory; sourcePath defaults to partPath. Logical part paths use '/'. No wildcards or automatic enumeration. Local execution only.")]
    public string? DefinitionFiles { get; set; }

    [Option(Description = "Optional Fabric definition format for saved-file input only, for example ipynb, FabricGitSource, TMDL, TMSL, PBIR, or PBIR-Legacy. Requires definition-directory and definition-files. For inline input, put format inside definition JSON. Format and content are forwarded unchanged after generic validation; Fabric validates format support and item-specific layouts.")]
    public string? DefinitionFormat { get; set; }

    /// <summary>Any supplied source option, including incomplete input that still requires validation.</summary>
    public bool HasInput => Definition is not null || DefinitionDirectory is not null ||
        DefinitionFiles is not null || DefinitionFormat is not null;
}
