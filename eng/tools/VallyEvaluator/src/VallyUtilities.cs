using McpToolEvaluator.Core.Models;
using VallyEvaluator.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VallyEvaluator;

internal class VallyUtilities
{
    public static async Task WritePromptsAsync(List<TestPrompt> prompts,
        string outputFile,
        string environment = "development",
        bool force = false)
    {
        var serializer = new StaticSerializerBuilder(new VallyYamlStaticContext())
            .EnsureRoundtrip()
            .WithIndentedSequences()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .Build();

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

        var serialized = serializer.Serialize(evaluation);

        if (File.Exists(outputFile))
        {
            if (!force)
            {
                throw new InvalidOperationException($"Output file {outputFile} already exists.");
            } else
            {
                File.Delete(outputFile);
            }
        }

        await File.WriteAllTextAsync(outputFile, serialized);
    }

    private static StimulusGraderConfig GetToolCallGrader(string toolName, string toolCommand)
    {
        var graderConfig = new GraderConfigEntry()
        {
            Required = [new GraderConfigEntryPair
            {
                Name = toolName,
                Command = toolCommand
            }]
        };

        return new StimulusGraderConfig
        {
            Type = "tool-calls",
            Config = graderConfig
        };
    }
}
