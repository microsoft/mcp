// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Functions.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Functions.Options;

public sealed class ProjectGetOptions
{
    [Option(Description = FunctionsOptionDescriptions.Language)]
    public required SupportedLanguages Language { get; set; }
}
