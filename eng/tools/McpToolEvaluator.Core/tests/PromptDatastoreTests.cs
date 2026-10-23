// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using McpToolEvaluator.Core.Models;
using Xunit;

namespace McpToolEvaluator.Core.Tests;

public sealed class PromptDatastoreTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();

    public void Dispose() => File.Delete(_tempFile);

    [Fact]
    public void GetPromptsByNamespace_InteractivePrompts()
    {
        var expectedPrompts = new List<TestPrompt>
        {
            new("advisor", "advisor_recommendation_list", "list recommendations", "advisor", PromptInteraction.None),
            new("advisor", "advisor_recommendation_apply", "apply recommendations to this template", "advisor", PromptInteraction.ContextRequired),
            new("advisor", "advisor_recommendation_apply", "apply recommendations", "advisor", PromptInteraction.ClarificationRequired),
            new("advisor", "advisor_recommendation_list", "investigate recommendation routing", "advisor", PromptInteraction.InvestigationRequired),
        };

        File.WriteAllText(_tempFile, """
            ## advisor

            | Tool Name | Prompt | Interaction |
            |-----------|--------|-------------|
            | advisor_recommendation_list | list recommendations | none |
            | advisor_recommendation_apply | apply recommendations to this template | context-required |
            | advisor_recommendation_apply | apply recommendations | clarification-required |
            | advisor_recommendation_list | investigate recommendation routing | investigation-required |
            """);

        var datastore = new PromptDatastore(_tempFile);
        var matching = datastore.GetPromptsByNamespace("advisor");

        Assert.NotNull(matching);
        Assert.Equal(4, matching.Count);
        foreach (var expected in expectedPrompts)
        {
            var match = matching.FirstOrDefault(p => AreEqual(p, expected));

            Assert.NotNull(match);
        }
    }

    private static bool AreEqual(TestPrompt a, TestPrompt b)
    {
        return a.Section == b.Section &&
               a.Tool == b.Tool &&
               a.Prompt == b.Prompt &&
               a.Namespace == b.Namespace &&
               a.Interaction == b.Interaction;
    }
}
