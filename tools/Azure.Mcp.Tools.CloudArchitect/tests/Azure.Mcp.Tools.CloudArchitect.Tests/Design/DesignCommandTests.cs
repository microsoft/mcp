// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.CloudArchitect.Commands.Design;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.CloudArchitect.Tests.Design;

public class DesignCommandTests : CommandUnitTestsBase<DesignCommand, object>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("design", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public void Command_HasCorrectOptions()
    {
        // Check that the command has the expected options
        var optionNames = CommandDefinition.Options.Select(o => o.Name).ToList();

        Assert.Contains("--question", optionNames);
        Assert.Contains("--question-number", optionNames);
        Assert.Contains("--total-questions", optionNames);
        Assert.Contains("--answer", optionNames);
        Assert.Contains("--next-question-needed", optionNames);
        Assert.Contains("--confidence-score", optionNames);
        Assert.Contains("--state", optionNames);
    }

    // TODO: jongio - See why --architecture-tier are in the tests, but not in the DesignCommand.
    [Theory]
    [InlineData("")]
    [InlineData("--question \"What is your application type?\"")]
    [InlineData("--question-number 1")]
    [InlineData("--total-questions 5")]
    [InlineData("--answer \"Web application\"")]
    [InlineData("--next-question-needed true")]
    [InlineData("--confidence-score 0.8")]
    [InlineData("--question \"App type?\" --question-number 1 --total-questions 5")]
    public async Task ExecuteAsync_ReturnsArchitectureDesignText(string args)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        var responseObject = ValidateAndDeserializeResponse(response, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);

        Assert.NotEmpty(responseObject.DesignArchitecture);
        Assert.NotNull(responseObject.ResponseObject);

        // Verify it contains some expected architecture-related content
        Assert.Contains("architecture", responseObject.DesignArchitecture.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WithAllOptionsSet()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--question", "What is your application type?",
            "--question-number", "1",
            "--total-questions", "5",
            "--answer", "Web application",
            "--next-question-needed", "true",
            "--confidence-score", "0.8");

        // Assert
        var responseObject = ValidateAndDeserializeResponse(response, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);

        Assert.NotEmpty(responseObject.DesignArchitecture);
        Assert.NotNull(responseObject.ResponseObject);
        Assert.Equal("What is your application type?", responseObject.ResponseObject.DisplayText);
    }

    [Theory]
    [InlineData("What's your app type?", "What's your app type?")]
    [InlineData("How \"big\" is your app?", "How \"big\" is your app?")]
    [InlineData("Is it a \"web app\" or \"mobile app\"?", "Is it a \"web app\" or \"mobile app\"?")]
    [InlineData("What's the app's \"main purpose\"?", "What's the app's \"main purpose\"?")]
    [InlineData("Use 'single quotes' here", "Use 'single quotes' here")]
    [InlineData("Mixed \"quotes\" and 'apostrophes'", "Mixed \"quotes\" and 'apostrophes'")]
    public async Task ExecuteAsync_HandlesQuotesAndEscapingProperly(string questionWithQuotes, string expectedQuestion)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync("--question", questionWithQuotes);

        // Assert
        var responseObject = ValidateAndDeserializeResponse(response, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);

        Assert.NotEmpty(responseObject.DesignArchitecture);
        Assert.NotNull(responseObject.ResponseObject);

        // Verify the question was parsed correctly by checking the DisplayText in response
        Assert.Equal(expectedQuestion, responseObject.ResponseObject.DisplayText);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesComplexEscapingScenarios()
    {
        // Arrange - Test multiple options with various escaping scenarios
        var complexQuestion = "What is your \"primary\" application 'type' and how \"big\" will it be?";
        var complexAnswer = "It's a \"web application\" with 'high' scalability requirements";

        var args = new[]
        {
            "--question", complexQuestion,
            "--answer", complexAnswer,
            "--question-number", "2",
            "--total-questions", "10"
        };

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Empty(response.Message);

        // Verify all options were parsed correctly using the canonical option definitions
        var options = Command.BindOptions(CommandDefinition.Parse(args));

        Assert.Equal(complexQuestion, options.Question);
        Assert.Equal(complexAnswer, options.Answer);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Fact]
    public void Properties_AreConfiguredCorrectly()
    {
        Assert.Equal("design", Command.Name);
        Assert.Equal("Design Azure cloud architectures through guided questions", Command.Title);
        Assert.NotEmpty(Command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_LoadsEmbeddedResourceText()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync("--question", "Test question");

        // Assert
        var responseObject = ValidateAndDeserializeResponse(response, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);

        Assert.NotEmpty(responseObject.DesignArchitecture);
        // The embedded resource should contain Azure architecture guidance
        Assert.Contains("Azure", responseObject.DesignArchitecture);
    }

    [Fact]
    public async Task ExecuteAsync_WithStateOption()
    {
        // Arrange - Create a simple JSON state object
        var stateJson = "{\"architectureComponents\":[],\"architectureTiers\":{\"infrastructure\":[],\"platform\":[],\"application\":[],\"data\":[],\"security\":[],\"operations\":[]},\"requirements\":{\"explicit\":[],\"implicit\":[],\"assumed\":[]},\"confidenceFactors\":{\"explicitRequirementsCoverage\":0.5,\"implicitRequirementsCertainty\":0.7,\"assumptionRisk\":0.3}}";

        // Act
        var response = await ExecuteCommandAsync("--state", stateJson);

        // Assert
        // Verify the command executed successfully with state option
        var responseObject = ValidateAndDeserializeResponse(response, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);

        Assert.NotEmpty(responseObject.DesignArchitecture);
        Assert.NotNull(responseObject.ResponseObject);
        Assert.NotNull(responseObject.ResponseObject.State);
    }

    [Fact]
    public async Task ExecuteAsync_WithCompleteOptionSet()
    {
        // Arrange - Test all options together including the new ones
        var args = new[]
        {
            "--question", "What type of application are you building?",
            "--question-number", "3",
            "--total-questions", "8",
            "--answer", "A financial trading platform",
            "--next-question-needed", "false",
            "--confidence-score", "0.9",
        };

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        // Verify all options were parsed correctly
        var options = Command.BindOptions(CommandDefinition.Parse(args));

        Assert.Equal("What type of application are you building?", options.Question);
        Assert.Equal(3, options.QuestionNumber);
        Assert.Equal(8, options.TotalQuestions);
        Assert.Equal("A financial trading platform", options.Answer);
        Assert.False(options.NextQuestionNeeded);
        Assert.Equal(0.9, options.ConfidenceScore);

        // Verify the response structure
        var responseObject = ValidateAndDeserializeResponse(response, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);

        Assert.NotEmpty(responseObject.DesignArchitecture);
        Assert.NotNull(responseObject.ResponseObject);
        Assert.Equal(options.Question, responseObject.ResponseObject.DisplayText);
        Assert.Equal(options.QuestionNumber, responseObject.ResponseObject.QuestionNumber);
        Assert.Equal(options.TotalQuestions, responseObject.ResponseObject.TotalQuestions);
        Assert.Equal(options.NextQuestionNeeded, responseObject.ResponseObject.NextQuestionNeeded);
    }

    #region Validation Tests

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(2.0)]
    [InlineData(-1.0)]
    public void Parse_InvalidConfidenceScore_ReturnsError(double invalidScore)
    {
        // Arrange & Act
        var validationResult = Validate("--confidence-score", invalidScore.ToString());

        // Assert
        Assert.NotEmpty(validationResult.Errors);
        Assert.Contains("Confidence score must be between 0.0 and 1.0", validationResult.Errors);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(0.1)]
    [InlineData(0.9)]
    public void Parse_ValidConfidenceScore_NoErrors(double validScore)
    {
        // Arrange & Act
        var validationResult = Validate("--confidence-score", validScore.ToString());

        // Assert
        Assert.Empty(validationResult.Errors);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-100)]
    public void Parse_NegativeQuestionNumber_ReturnsError(int invalidQuestionNumber)
    {
        // Arrange & Act
        var validationResult = Validate("--question-number", invalidQuestionNumber.ToString());

        // Assert
        Assert.NotEmpty(validationResult.Errors);
        Assert.Contains("Question number cannot be negative", validationResult.Errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void Parse_ValidQuestionNumber_NoErrors(int validQuestionNumber)
    {
        // Arrange & Act
        var validationResult = Validate("--question-number", validQuestionNumber.ToString());

        // Assert
        Assert.Empty(validationResult.Errors);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-100)]
    public void Parse_NegativeTotalQuestions_ReturnsError(int invalidTotalQuestions)
    {
        // Arrange & Act
        var validationResult = Validate("--total-questions", invalidTotalQuestions.ToString());

        // Assert
        Assert.NotEmpty(validationResult.Errors);
        Assert.Contains("Total questions cannot be negative", validationResult.Errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void Parse_ValidTotalQuestions_NoErrors(int validTotalQuestions)
    {
        // Arrange & Act
        var validationResult = Validate("--total-questions", validTotalQuestions.ToString());

        // Assert
        Assert.Empty(validationResult.Errors);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(5, 5)]
    [InlineData(3, 10)]
    [InlineData(0, 5)] // Zero is valid for question number
    public void Parse_QuestionNumberWithinTotalQuestions_NoErrors(int questionNumber, int totalQuestions)
    {
        // Arrange & Act
        var validationResult = Validate("--question-number", questionNumber.ToString(), "--total-questions", totalQuestions.ToString());

        // Assert
        Assert.Empty(validationResult.Errors);
    }

    [Fact]
    public void Parse_MultipleValidationErrors_ReturnsFirstError()
    {
        // Arrange & Act - Set both invalid confidence score and negative question number
        var validationResult = Validate("--confidence-score", "1.5", "--question-number", "-1");

        // Assert
        Assert.NotEmpty(validationResult.Errors);
        // Should return the first validation error encountered
        Assert.Contains("Confidence score must be between 0.0 and 1.0", validationResult.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_WithComplexStateJson_ParsesSuccessfully()
    {
        // Arrange - Use the exact JSON from the original error
        var stateJson = """
                        {
                            "architectureComponents": [],
                            "architectureTiers": {
                                "infrastructure": [],
                                "platform": [],
                                "application": [],
                                "data": [],
                                "security": [],
                                "operations": []
                            },
                            "requirements": {
                                "explicit": [
                                    {
                                        "category": "functionality",
                                        "description": "Video upload capability for users",
                                        "source": "User statement",
                                        "importance": "high",
                                        "confidence": 1
                                    },
                                    {
                                        "category": "functionality",
                                        "description": "Video viewing/playback capability for users",
                                        "source": "User statement",
                                        "importance": "high",
                                        "confidence": 1
                                    }
                                ],
                                "implicit": [
                                    {
                                        "category": "performance",
                                        "description": "Large-scale video processing and streaming required",
                                        "source": "Inferred from 'large video playback company'",
                                        "importance": "high",
                                        "confidence": 0.9
                                    }
                                ],
                                "assumed": [
                                    {
                                        "category": "scale",
                                        "description": "Potentially thousands of concurrent users",
                                        "source": "Assumed from 'large' company description",
                                        "importance": "medium",
                                        "confidence": 0.6
                                    }
                                ]
                            },
                            "confidenceFactors": {
                                "explicitRequirementsCoverage": 0.4,
                                "implicitRequirementsCertainty": 0.8,
                                "assumptionRisk": 0.4
                            }
                        }
                        """;

        // Act
        var response = await ExecuteCommandAsync(
            "--state", stateJson,
            "--question", "What is your primary business goal?",
            "--confidence-score", "0.5");

        // Assert
        var responseObject = ValidateAndDeserializeResponse(response, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);

        Assert.NotNull(responseObject.ResponseObject.State);
        Assert.Empty(responseObject.ResponseObject.State.ArchitectureComponents);
        Assert.NotNull(responseObject.ResponseObject.State.Requirements);
        Assert.Equal(2, responseObject.ResponseObject.State.Requirements.Explicit.Count);
        Assert.Single(responseObject.ResponseObject.State.Requirements.Implicit);
        Assert.Single(responseObject.ResponseObject.State.Requirements.Assumed);
    }

    // TODO: jongio - Talk with author.  It looks like the code intentionally throws
    // [Fact]
    // public async Task ExecuteAsync_WithInvalidStateJson_HandlesGracefully()
    // {
    //     // Arrange
    //     var invalidStateJson = "{ invalid json }";
    //     var args = new[] { "--state", invalidStateJson };
    //     var parseResult = _commandDefinition.Parse(args);

    //     // Act
    //     var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

    //     // Assert - The command should handle the error gracefully and return an error response
    //     Assert.NotEqual(HttpStatusCode.OK, response.Status);
    //     Assert.NotEmpty(response.Message);
    // }

    [Fact]
    public async Task ExecuteAsync_WithEmptyState_CreatesDefaultState()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync("--state", "");

        // Assert
        var responseObject = ValidateAndDeserializeResponse(response, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);

        Assert.NotNull(responseObject.ResponseObject.State);
        Assert.Empty(responseObject.ResponseObject.State.ArchitectureComponents);
        Assert.NotNull(responseObject.ResponseObject.State.Requirements);
        Assert.Empty(responseObject.ResponseObject.State.Requirements.Explicit);
    }

    [Fact]
    public void BindOptions_WithInvalidStateJson_ThrowsException()
    {
        // Arrange, Act, & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            // Access the protected BindOptions method via reflection to test state deserialization
            var options = Command.BindOptions(CommandDefinition.Parse(["--state", "{ invalid json }"]));

            // Manually call the state deserialization that happens in BindOptions
            var deserializeMethod = DesignCommand.DeserializeState(options.State);
        });

        // Verify the inner exception is the InvalidOperationException we expect
        Assert.Contains("Failed to deserialize state JSON", exception.Message);
    }

    #endregion

    private ValidationResult Validate(params string[] args)
    {
        var options = Command.BindOptions(CommandDefinition.Parse(args));
        var validationResult = new ValidationResult();
        Command.ValidateOptions(options, validationResult);

        return validationResult;
    }
}
