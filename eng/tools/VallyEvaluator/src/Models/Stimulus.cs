// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace VallyEvaluator.Models;

using YamlDotNet.Serialization;

public class Stimulus
{
    public required string Name { get; set; }

    public required string Prompt { get; set; }

    public Dictionary<string, string>? Tags { get; set; }

    [YamlMember(Alias = "agent_environment", ApplyNamingConventions = false)]
    public string? Environment { get; set; }

    public List<StimulusGraderConfig>? Graders { get; set; }
}
