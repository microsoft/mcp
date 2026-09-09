// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using McpToolEvaluator.Core.Models;
using VallyEvaluator.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VallyEvaluator;

internal class VallyUtilities
{
    private const string EnvironmentPlaceholder = "${ENVIRONMENT}";

    internal static readonly ISerializer Serializer =
        new StaticSerializerBuilder(new VallyYamlStaticContext())
            .EnsureRoundtrip()
            .WithIndentedSequences()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .Build();

    internal static readonly IDeserializer Deserializer =
        new StaticDeserializerBuilder(new VallyYamlStaticContext())
            .WithCaseInsensitivePropertyMatching()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .Build();

    public static async Task WritePromptsAsync(List<TestPrompt> prompts,
        string outputFile,
        string environment = EnvironmentPlaceholder,
        bool force = false)
    {
        var stimuli = new List<Stimulus>();
        for (int i = 0; i < prompts.Count; i++)
        {
            var p = prompts[i];
            var graders = new List<StimulusGraderConfig>()
            {
                GetToolCallGrader(p.Namespace, p.Tool)
            };
            var stimulus = new Stimulus
            {
                Name = $"{p.Namespace} evaluation {i}",
                Prompt = p.Prompt,
                Environment = environment,
                Graders = graders
            };

            stimuli.Add(stimulus);
        }

        var section = prompts[0].Section;
        var evaluation = new Evaluation
        {
            Name = $"{section} evaluations",
            Description = "Evaluation of prompts in the section " + section,
            Stimuli = stimuli
        };

        var serialized = Serializer.Serialize(evaluation);

        if (File.Exists(outputFile))
        {
            if (!force)
            {
                throw new InvalidOperationException($"Output file {outputFile} already exists.");
            }
            else
            {
                File.Delete(outputFile);
            }
        }

        await File.WriteAllTextAsync(outputFile, serialized);
    }

    internal static StimulusGraderConfig GetToolCallGrader(string toolName, string toolCommand)
    {
        var commandArgsEntry = new GraderConfigEntryPair
        {
            Name = toolName,
        };
        commandArgsEntry.Args.Add("command", toolCommand);

        var graderConfig = new GraderConfigEntry()
        {
            Required = [commandArgsEntry]
        };

        return new StimulusGraderConfig
        {
            Type = "tool-calls",
            Config = graderConfig
        };
    }
}
