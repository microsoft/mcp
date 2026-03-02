// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace CopilotCliTester.Models;

internal sealed record TestPrompt(
    string Section,
    string Tool,
    string Prompt,
    string Namespace = ""
);