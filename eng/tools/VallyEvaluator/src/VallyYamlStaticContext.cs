using VallyEvaluator.Models;
using YamlDotNet.Serialization;

namespace VallyEvaluator;

[YamlStaticContext]
[YamlSerializable(typeof(Evaluation))]
[YamlSerializable(typeof(Stimulus))]
[YamlSerializable(typeof(StimulusGraderConfig))]
[YamlSerializable(typeof(GraderConfigEntry))]
[YamlSerializable(typeof(GraderConfigEntryPair))]
public partial class VallyYamlStaticContext : StaticContext
{
}
