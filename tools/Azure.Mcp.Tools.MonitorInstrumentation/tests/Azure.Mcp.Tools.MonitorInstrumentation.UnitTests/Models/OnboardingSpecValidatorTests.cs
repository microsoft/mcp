using Azure.Mcp.Tools.MonitorInstrumentation.Models;
using Xunit;

namespace Azure.Mcp.Tools.MonitorInstrumentation.UnitTests.Models;

public sealed class OnboardingSpecValidatorTests
{
    [Fact]
    public void Validate_WithValidSpec_ReturnsValidResult()
    {
        // Arrange
        var spec = CreateValidSpec();
        var validator = new OnboardingSpecValidator();

        // Act
        var result = validator.Validate(spec);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithMissingDecisionFields_ReturnsErrors()
    {
        // Arrange
        var spec = CreateValidSpec() with
        {
            Decision = new Decision
            {
                Intent = string.Empty,
                TargetApproach = string.Empty,
                Rationale = string.Empty
            }
        };
        var validator = new OnboardingSpecValidator();

        // Act
        var result = validator.Validate(spec);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Decision.Intent is required", result.Errors);
        Assert.Contains("Decision.TargetApproach is required", result.Errors);
        Assert.Contains("Decision.Rationale is required", result.Errors);
    }

    [Fact]
    public void Validate_WithDuplicateActionId_ThrowsArgumentException()
    {
        // Arrange
        var spec = CreateValidSpec() with
        {
            Actions =
            [
                CreateManualStepAction("a1", 0),
                CreateManualStepAction("a1", 1)
            ]
        };
        var validator = new OnboardingSpecValidator();

        // Assert
        Assert.Throws<ArgumentException>(() => validator.Validate(spec));
    }

    [Fact]
    public void Validate_WithMissingDependency_ReturnsError()
    {
        // Arrange
        var spec = CreateValidSpec() with
        {
            Actions =
            [
                CreateManualStepAction("a1", 0, "missing")
            ]
        };
        var validator = new OnboardingSpecValidator();

        // Act
        var result = validator.Validate(spec);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("depends on non-existent action 'missing'", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_WithCircularDependency_ReturnsError()
    {
        // Arrange
        var spec = CreateValidSpec() with
        {
            Actions =
            [
                CreateManualStepAction("a1", 0, "a2"),
                CreateManualStepAction("a2", 1, "a1")
            ]
        };
        var validator = new OnboardingSpecValidator();

        // Act
        var result = validator.Validate(spec);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("Circular dependency detected", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_WithOrderGap_AddsWarning()
    {
        // Arrange
        var spec = CreateValidSpec() with
        {
            Actions =
            [
                CreateManualStepAction("a1", 0),
                CreateManualStepAction("a2", 2)
            ]
        };
        var validator = new OnboardingSpecValidator();

        // Act
        var result = validator.Validate(spec);

        // Assert
        Assert.True(result.IsValid);
        Assert.Contains(result.Warnings, warning => warning.Contains("Gap in action ordering", StringComparison.Ordinal));
    }

    private static OnboardingSpec CreateValidSpec()
    {
        return new OnboardingSpec
        {
            Version = "0.1",
            Analysis = new Analysis
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
            },
            Decision = new Decision
            {
                Intent = OnboardingConstants.Intents.Onboard,
                TargetApproach = OnboardingConstants.Approaches.AzureMonitorDistro,
                Rationale = "Valid spec for testing"
            },
            Actions =
            [
                CreateManualStepAction("a1", 0)
            ]
        };
    }

    private static OnboardingAction CreateManualStepAction(string id, int order, params string[] dependsOn)
    {
        return ActionDetailsExtensions.CreateAction(
            id,
            ActionType.ManualStep,
            "Do the step",
            new ManualStepDetails("Run the required step"),
            order,
            dependsOn);
    }
}
