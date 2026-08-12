// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace McpToolEvaluator.Core.Tests;

public sealed class PromptDatastoreTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();

    public void Dispose() => File.Delete(_tempFile);

    [Fact]
    public void GetPromptsByNamespace_InteractivePrompts_ExcludesInteractivePrompts()
    {
        File.WriteAllText(_tempFile, """
            ## advisor

            | Tool Name | Prompt | Interaction |
            |-----------|--------|-------------|
            | advisor_recommendation_list | list recommendations | none |
            | advisor_recommendation_apply | apply recommendations to this template | context-required |
            | advisor_recommendation_apply | apply recommendations | clarification-required |
            """);

        var datastore = new PromptDatastore(_tempFile);

        var prompt = Assert.Single(datastore.GetPromptsByNamespace("advisor"));
        Assert.Equal("list recommendations", prompt.Prompt);
    }
}
