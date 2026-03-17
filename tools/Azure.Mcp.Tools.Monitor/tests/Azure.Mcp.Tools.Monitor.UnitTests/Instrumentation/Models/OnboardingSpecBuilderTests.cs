using Azure.Mcp.Tools.MonitorInstrumentation.Models;
using Xunit;

namespace Azure.Mcp.Tools.MonitorInstrumentation.UnitTests.Models;

public sealed class OnboardingSpecBuilderTests
{
    [Fact]
    public void Build_WithValidActions_ReturnsSpecWithSequentialOrder()
    {
        // Arrange
        var builder = CreateBuilderWithDecision();

        // Act
        var spec = builder
            .AddManualStepAction("step-1", "First step", "Do first")
            .AddManualStepAction("step-2", "Second step", "Do second", null, "step-1")
            .Build();

        // Assert
        Assert.Equal(2, spec.Actions.Count);
        Assert.Equal(0, spec.Actions[0].Order);
        Assert.Equal(1, spec.Actions[1].Order);
        Assert.Single(spec.Actions[1].DependsOn);
        Assert.Equal("step-1", spec.Actions[1].DependsOn[0]);
    }

    [Fact]
    public void AddAction_WithHigherOrder_UpdatesInternalNextOrder()
    {
        // Arrange
        var builder = CreateBuilderWithDecision();
        var existingAction = ActionDetailsExtensions.CreateAction(
            "existing",
            ActionType.ManualStep,
            "Existing action",
            new ManualStepDetails("Already there"),
            3);

        // Act
        var spec = builder
            .AddAction(existingAction)
            .AddManualStepAction("next", "Next action", "Do next")
            .Build();

        // Assert
        Assert.Equal(2, spec.Actions.Count);
        Assert.Equal(3, spec.Actions[0].Order);
        Assert.Equal(4, spec.Actions[1].Order);
    }

    [Fact]
    public void Build_WithInvalidAction_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = CreateBuilderWithDecision();
        var invalidAction = new OnboardingAction
        {
            Id = "invalid",
            Type = ActionType.ManualStep,
            Description = "Invalid because details are missing required instructions",
            Details = new Dictionary<string, object>(),
            Order = 0
        };

        // Act
        var action = () => builder.AddAction(invalidAction).Build();

        // Assert
        var ex = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("Invalid OnboardingSpec", ex.Message, StringComparison.Ordinal);
    }

    private static OnboardingSpecBuilder CreateBuilderWithDecision()
    {
        return new OnboardingSpecBuilder(new Analysis
        {
            Language = Language.DotNet,
            State = InstrumentationState.Greenfield,
            Projects =
            [
                new ProjectInfo
                {
                    ProjectFile = "sample.csproj",
                    AppType = AppType.AspNetCore,
                    HostingPattern = HostingPattern.MinimalApi
                }
            ]
        })
        .WithDecision(
            OnboardingConstants.Intents.Onboard,
            OnboardingConstants.Approaches.AzureMonitorDistro,
            "Test rationale");
    }
}
