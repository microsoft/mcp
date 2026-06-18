// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace McpToolEvaluator.Core.Models;

public sealed record TestPrompt(
    string Section,
    string Tool,
    string Prompt,
    string Namespace = ""
);
