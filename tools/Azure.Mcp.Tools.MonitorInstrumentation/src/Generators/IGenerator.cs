using Azure.Mcp.Tools.MonitorInstrumentation.Models;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Generators;

/// <summary>
/// Interface for generating onboarding specifications based on workspace analysis
/// </summary>
public interface IGenerator
{
    /// <summary>
    /// Determines if this generator can handle the given analysis result
    /// </summary>
    bool CanHandle(Analysis analysis);

    /// <summary>
    /// Generates an onboarding specification for the analyzed workspace
    /// </summary>
    OnboardingSpec Generate(Analysis analysis);
}
