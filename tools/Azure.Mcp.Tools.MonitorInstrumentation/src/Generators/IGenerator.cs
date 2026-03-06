using Azure.Mcp.Tools.MonitorInstrumentation.Models;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Generators;

public interface IGenerator
{
    bool CanHandle(Analysis analysis);
    OnboardingSpec Generate(Analysis analysis);
}
