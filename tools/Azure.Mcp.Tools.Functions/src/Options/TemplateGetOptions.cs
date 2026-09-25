// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Functions.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Functions.Options;

public sealed class TemplateGetOptions
{
    [Option(Description = FunctionsOptionDescriptions.Language)]
    public required SupportedLanguages Language { get; set; }

    [Option(Description = "Name of the function template to retrieve (e.g., HttpTrigger, BlobTrigger). Omit to list all available templates for the specified language.")]
    public string? Template { get; set; }

    [Option(Description = "Optional runtime version for Java or TypeScript/JavaScript. When provided, template placeholders like {{javaVersion}} or {{nodeVersion}} are replaced automatically. See 'functions language list' for supported versions.")]
    public string? RuntimeVersion { get; set; }

    [Option(Description = "Output format. 'New' (default) returns all files in a single 'files' list for creating complete projects. 'Add' separates files into 'functionFiles' and 'projectFiles' with merge instructions for adding to existing projects.")]
    public TemplateOutput? Output { get; set; }
}
