namespace VallyEvaluator.Models;

public class Stimulus
{
    public required string Name { get; set; }

    public required string Prompt { get; set; }

    public Dictionary<string, string>? Tags { get; set; }

    public string? Environment { get; set; }

    public List<StimulusGraderConfig>? Graders { get; set; }
}
