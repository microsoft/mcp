// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using McpToolEvaluator.Core.Models;
using Xunit;

namespace VallyEvaluator.Tests;

public sealed class VallyUtilitiesTests
{
    [Fact]
    public void GetToolCallGrader_CreatesRequiredCommandEntry()
    {
        var grader = VallyUtilities.GetToolCallGrader("storage", "storage_account_list");

        Assert.Equal("tool-calls", grader.Type);
        var requiredEntry = Assert.Single(grader.Config.Required);
        Assert.Equal("storage", requiredEntry.Name);
        Assert.Equal("storage_account_list", requiredEntry.Args["command"]);
    }

    [Fact]
    public async Task WritePromptsAsync_WritesEvaluationYaml()
    {
        var outputFile = GetTemporaryFilePath();
        var prompts = new List<TestPrompt>
        {
            new("Storage", "storage_account_list", "List my storage accounts", "storage"),
            new("Storage", "storage_account_get", "Show my storage account", "storage")
        };

        try
        {
            await VallyUtilities.WritePromptsAsync(prompts, outputFile);

            var yaml = await File.ReadAllTextAsync(outputFile, TestContext.Current.CancellationToken);
            var evaluation = VallyUtilities.Deserializer.Deserialize<Models.Evaluation>(yaml);

            Assert.Equal("Storage evaluations", evaluation.Name);
            Assert.Equal("Evaluation of prompts in the section Storage", evaluation.Description);
            Assert.Equal("capability", evaluation.Type);
            Assert.Collection(
                evaluation.Stimuli,
                stimulus => AssertStimulus(stimulus, "storage evaluation 0", "List my storage accounts", "storage_account_list"),
                stimulus => AssertStimulus(stimulus, "storage evaluation 1", "Show my storage account", "storage_account_get"));
        }
        finally
        {
            File.Delete(outputFile);
        }
    }

    [Fact]
    public async Task WritePromptsAsync_WhenFileExistsAndForceIsTrue_OverwritesFile()
    {
        var outputFile = GetTemporaryFilePath();
        await File.WriteAllTextAsync(outputFile, "existing content", TestContext.Current.CancellationToken);
        var prompts = new List<TestPrompt>
        {
            new("Storage", "storage_account_list", "List my storage accounts", "storage")
        };

        try
        {
            await VallyUtilities.WritePromptsAsync(prompts, outputFile, force: true);

            var yaml = await File.ReadAllTextAsync(outputFile, TestContext.Current.CancellationToken);
            var evaluation = VallyUtilities.Deserializer.Deserialize<Models.Evaluation>(yaml);

            Assert.Equal("Storage evaluations", evaluation.Name);
            Assert.Single(evaluation.Stimuli);
            Assert.Equal("List my storage accounts", evaluation.Stimuli[0].Prompt);
        }
        finally
        {
            File.Delete(outputFile);
        }
    }

    [Fact]
    public async Task WritePromptsAsync_UsesProvidedEnvironment()
    {
        var outputFile = GetTemporaryFilePath();
        const string customEnvironment = "${CUSTOM_ENV}";
        var prompts = new List<TestPrompt>
        {
            new("Storage", "storage_account_list", "List my storage accounts", "storage")
        };

        try
        {
            await VallyUtilities.WritePromptsAsync(prompts, outputFile, environment: customEnvironment);

            var yaml = await File.ReadAllTextAsync(outputFile, TestContext.Current.CancellationToken);
            var evaluation = VallyUtilities.Deserializer.Deserialize<Models.Evaluation>(yaml);

            var stimulus = Assert.Single(evaluation.Stimuli);
            Assert.Equal(customEnvironment, stimulus.Environment);
        }
        finally
        {
            File.Delete(outputFile);
        }
    }

    [Fact]
    public async Task WritePromptsAsync_WhenPromptsAreEmpty_Throws()
    {
        var outputFile = GetTemporaryFilePath();

        try
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => VallyUtilities.WritePromptsAsync([], outputFile));
        }
        finally
        {
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }
    }

    [Fact]
    public async Task WritePromptsAsync_WhenFileExistsAndForceIsFalse_DoesNotOverwriteFile()
    {
        var outputFile = GetTemporaryFilePath();
        const string originalContent = "existing content";
        await File.WriteAllTextAsync(outputFile, originalContent, TestContext.Current.CancellationToken);
        var prompts = new List<TestPrompt>
        {
            new("Storage", "storage_account_list", "List my storage accounts", "storage")
        };

        try
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => VallyUtilities.WritePromptsAsync(prompts, outputFile));

            Assert.Contains(outputFile, exception.Message);
            Assert.Equal(
                originalContent,
                await File.ReadAllTextAsync(outputFile, TestContext.Current.CancellationToken));
        }
        finally
        {
            File.Delete(outputFile);
        }
    }

    private static string GetTemporaryFilePath() =>
        Path.Combine(Path.GetTempPath(), $"VallyEvaluatorTests-{Guid.NewGuid():N}.yaml");

    private static void AssertStimulus(
        Models.Stimulus stimulus,
        string expectedName,
        string expectedPrompt,
        string expectedCommand)
    {
        Assert.Equal(expectedName, stimulus.Name);
        Assert.Equal(expectedPrompt, stimulus.Prompt);
        Assert.Equal("${ENVIRONMENT}", stimulus.Environment);

        var grader = Assert.Single(stimulus.Graders!);
        var requiredEntry = Assert.Single(grader.Config.Required);
        Assert.Equal("storage", requiredEntry.Name);
        Assert.Equal(expectedCommand, requiredEntry.Args["command"]);
    }
}
