namespace VallyEvaluator.Models;

public class Evaluation
{
    public required string Name { get; set; }

    public required string Description { get; set; }

    public string Type { get; set; } = "capability";

    public List<Stimulus> Stimuli { get; set; } = [];
}
