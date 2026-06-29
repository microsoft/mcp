// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace McpToolEvaluator.Core.Models;

public sealed class TestPrompt(string section, string tool, string prompt, string @namespace = "")
{
    public string Section { get; set; } = section;
    public string Tool { get; set; } = tool;
    public string Prompt { get; set; } = prompt;
    public string Namespace { get; set; } = @namespace;
}
