// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Azure.Mcp.Tools.CloudArchitect.Commands.Design;
using Azure.Mcp.Tools.CloudArchitect.Options;

namespace Azure.Mcp.Tools.CloudArchitect.UnitTests.Design;

public class DesignCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DesignCommand> _logger;
    private readonly DesignCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DesignCommandTests()
    {
        _logger = Substitute.For<ILogger<DesignCommand>>();

        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("design", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);

        // Check that the description contains the expected content from the actual implementation
        Assert.Contains("Recommends architecture design for cloud services/apps/solutions", command.Description);
        Assert.Contains("file storage, banking, video streaming, e-commerce, SaaS", command.Description);
        Assert.Contains("Ask about user role, business goals, etc (1-2 questions at a time)", command.Description);
        Assert.Contains("Track confidence returned by service and update requirements", command.Description);
        Assert.Contains("confidence >= 0.7", command.Description);
        Assert.Contains("Present architecture with table format, visual organization, ASCII diagrams", command.Description);
        Assert.Contains("Follow Azure Well-Architected Framework principles", command.Description);
        Assert.Contains("infrastructure, platform, application, data, security, operations", command.Description);
        Assert.Contains("State tracks components, requirements by category, and confidence factors", command.Description);
        Assert.Contains("Be conservative with suggestions", command.Description);
    }

    [Fact]
    public void Command_HasCorrectOptions()
    {
        var command = _command.GetCommand();

        // Check that the command has the expected options
        var optionNames = command.Options.Select(o => o.Name).ToList();

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
        // Arrange
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Empty(response.Message);

        // Verify that results contain the architecture design response structure
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        response.Results.Write(writer);
        writer.Flush();

        var serializedResult = Encoding.UTF8.GetString(stream.ToArray());


        var responseObject = JsonSerializer.Deserialize(serializedResult, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);

        Assert.NotNull(responseObject);
        Assert.NotEmpty(responseObject.DesignArchitecture);
        Assert.NotNull(responseObject.ResponseObject);

        // Verify it contains some expected architecture-related content
        Assert.Contains("architecture", responseObject.DesignArchitecture.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WithAllOptionsSet()
    {
        // Arrange
        var args = new[]
        {
            "--question", "What is your application type?",
            "--question-number", "1",
            "--total-questions", "5",
            "--answer", "Web application",
            "--next-question-needed", "true",
            "--confidence-score", "0.8",
        };

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Empty(response.Message);

        // Verify the command executed successfully regardless of the input options
        string serializedResult = SerializeResponseResult(response.Results);
        var responseObject = JsonSerializer.Deserialize(serializedResult, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);
        Assert.NotNull(responseObject);
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
        // Arrange
        var args = new[] { "--question", questionWithQuotes };
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Empty(response.Message);

        // Verify that the command executed successfully with the quoted input
        string serializedResult = SerializeResponseResult(response.Results);
        var responseObject = JsonSerializer.Deserialize(serializedResult, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);
        Assert.NotNull(responseObject);
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
        var parseResult = _commandDefinition.Parse(args);

        // Ensure there were no parse/validation errors
        Assert.True(!parseResult.Errors.Any(), string.Join("; ", parseResult.Errors.Select(e => e.Message)));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Empty(response.Message);

        // Verify all options were parsed correctly using the canonical option definitions
        var questionValue = parseResult.GetValue(CloudArchitectOptionDefinitions.Question);
        var answerValue = parseResult.GetValue(CloudArchitectOptionDefinitions.Answer);

        Assert.Equal(complexQuestion, questionValue);
        Assert.Equal(complexAnswer, answerValue);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        // Arrange & Act
        var metadata = _command.Metadata;

        // Assert
        Assert.False(metadata.Destructive);
        Assert.True(metadata.ReadOnly);
    }

    [Fact]
    public void Properties_AreConfiguredCorrectly()
    {
        // Arrange & Act
        var name = _command.Name;
        var title = _command.Title;
        var description = _command.Description;

        // Assert
        Assert.Equal("design", name);
        Assert.Equal("Design Azure cloud architectures through guided questions", title);
        Assert.NotEmpty(description);
        Assert.Contains("Recommends architecture design for cloud services/apps/solutions", description);
    }

    [Fact]
    public async Task ExecuteAsync_LoadsEmbeddedResourceText()
    {
        // Arrange
        var args = new[] { "--question", "Test question" };
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        string serializedResult = SerializeResponseResult(response.Results!);
        var responseObject = JsonSerializer.Deserialize(serializedResult, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);

        Assert.NotNull(responseObject);
        Assert.NotEmpty(responseObject.DesignArchitecture);
        // The embedded resource should contain Azure architecture guidance
        Assert.Contains("Azure", responseObject.DesignArchitecture);
    }

    [Fact]
    public async Task ExecuteAsync_WithStateOption()
    {
        // Arrange - Create a simple JSON state object
        var stateJson = "{\"architectureComponents\":[],\"architectureTiers\":{\"infrastructure\":[],\"platform\":[],\"application\":[],\"data\":[],\"security\":[],\"operations\":[]},\"requirements\":{\"explicit\":[],\"implicit\":[],\"assumed\":[]},\"confidenceFactors\":{\"explicitRequirementsCoverage\":0.5,\"implicitRequirementsCertainty\":0.7,\"assumptionRisk\":0.3}}";
        var args = new[] { "--state", stateJson };
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Empty(response.Message);

        // Verify the command executed successfully with state option
        string serializedResult = SerializeResponseResult(response.Results);
        var responseObject = JsonSerializer.Deserialize(serializedResult, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);
        Assert.NotNull(responseObject);
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

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Empty(response.Message);

        // Verify all options were parsed correctly
        var command = _command.GetCommand();
        var questionValue = parseResult.GetValue((Option<string>)command.Options.First(o => o.Name == "--question"));
        var questionNumberValue = parseResult.GetValue((Option<int>)command.Options.First(o => o.Name == "--question-number"));
        var totalQuestionsValue = parseResult.GetValue((Option<int>)command.Options.First(o => o.Name == "--total-questions"));
        var answerValue = parseResult.GetValue((Option<string>)command.Options.First(o => o.Name == "--answer"));
        var nextQuestionNeededValue = parseResult.GetValue((Option<bool>)command.Options.First(o => o.Name == "--next-question-needed"));
        var confidenceScoreValue = parseResult.GetValue((Option<double>)command.Options.First(o => o.Name == "--confidence-score"));

        Assert.Equal("What type of application are you building?", questionValue);
        Assert.Equal(3, questionNumberValue);
        Assert.Equal(8, totalQuestionsValue);
        Assert.Equal("A financial trading platform", answerValue);
        Assert.False(nextQuestionNeededValue);
        Assert.Equal(0.9, confidenceScoreValue);

        // Verify the response structure
        string serializedResult = SerializeResponseResult(response.Results);
        var responseObject = JsonSerializer.Deserialize(serializedResult, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);
        Assert.NotNull(responseObject);
        Assert.NotEmpty(responseObject.DesignArchitecture);
        Assert.NotNull(responseObject.ResponseObject);
        Assert.Equal(questionValue, responseObject.ResponseObject.DisplayText);
        Assert.Equal(questionNumberValue, responseObject.ResponseObject.QuestionNumber);
        Assert.Equal(totalQuestionsValue, responseObject.ResponseObject.TotalQuestions);
        Assert.Equal(nextQuestionNeededValue, responseObject.ResponseObject.NextQuestionNeeded);
    }

    private static string SerializeResponseResult(ResponseResult responseResult)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        responseResult.Write(writer);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    #region Validation Tests

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(2.0)]
    [InlineData(-1.0)]
    public void Parse_InvalidConfidenceScore_ReturnsError(double invalidScore)
    {
        // Arrange
        var args = new[] { "--confidence-score", invalidScore.ToString() };

        // Act
        var parseResult = _commandDefinition.Parse(args);

        // Assert
        Assert.NotEmpty(parseResult.Errors);
        Assert.Contains("Confidence score must be between 0.0 and 1.0", parseResult.Errors.Select(e => e.Message));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(0.1)]
    [InlineData(0.9)]
    public void Parse_ValidConfidenceScore_NoErrors(double validScore)
    {
        // Arrange
        var args = new[] { "--confidence-score", validScore.ToString() };

        // Act
        var parseResult = _commandDefinition.Parse(args);

        // Assert
        Assert.Empty(parseResult.Errors);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-100)]
    public void Parse_NegativeQuestionNumber_ReturnsError(int invalidQuestionNumber)
    {
        // Arrange
        var args = new[] { "--question-number", invalidQuestionNumber.ToString() };

        // Act
        var parseResult = _commandDefinition.Parse(args);

        // Assert
        Assert.NotEmpty(parseResult.Errors);
        Assert.Contains("Question number cannot be negative", parseResult.Errors.Select(e => e.Message));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void Parse_ValidQuestionNumber_NoErrors(int validQuestionNumber)
    {
        // Arrange
        var args = new[] { "--question-number", validQuestionNumber.ToString() };

        // Act
        var parseResult = _commandDefinition.Parse(args);

        // Assert
        Assert.Empty(parseResult.Errors);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-100)]
    public void Parse_NegativeTotalQuestions_ReturnsError(int invalidTotalQuestions)
    {
        // Arrange
        var args = new[] { "--total-questions", invalidTotalQuestions.ToString() };

        // Act
        var parseResult = _commandDefinition.Parse(args);

        // Assert
        Assert.NotEmpty(parseResult.Errors);
        Assert.Contains("Total questions cannot be negative", parseResult.Errors.Select(e => e.Message));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void Parse_ValidTotalQuestions_NoErrors(int validTotalQuestions)
    {
        // Arrange
        var args = new[] { "--total-questions", validTotalQuestions.ToString() };

        // Act
        var parseResult = _commandDefinition.Parse(args);

        // Assert
        Assert.Empty(parseResult.Errors);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(5, 5)]
    [InlineData(3, 10)]
    [InlineData(0, 5)] // Zero is valid for question number
    public void Parse_QuestionNumberWithinTotalQuestions_NoErrors(int questionNumber, int totalQuestions)
    {
        // Arrange
        var args = new[]
        {
            "--question-number", questionNumber.ToString(),
            "--total-questions", totalQuestions.ToString()
        };

        // Act
        var parseResult = _commandDefinition.Parse(args);

        // Assert
        Assert.Empty(parseResult.Errors);
    }

    [Fact]
    public void Parse_MultipleValidationErrors_ReturnsFirstError()
    {
        // Arrange - Set both invalid confidence score and negative question number
        var args = new[]
        {
            "--confidence-score", "1.5",
            "--question-number", "-1"
        };

        // Act
        var parseResult = _commandDefinition.Parse(args);

        // Assert
        Assert.NotEmpty(parseResult.Errors);
        // Should return the first validation error encountered
        Assert.Contains("Confidence score must be between 0.0 and 1.0", parseResult.Errors.Select(e => e.Message));
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

        var args = new[]
        {
            "--state", stateJson,
            "--question", "What is your primary business goal?",
            "--confidence-score", "0.5"
        };

        // Act
        var parseResult = _commandDefinition.Parse(args);
        var result = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Empty(parseResult.Errors);
        Assert.Equal(HttpStatusCode.OK, _context.Response.Status);

        // Verify that the state was parsed correctly by checking the response
        string serializedResult = SerializeResponseResult(_context.Response.Results!);
        var responseObject = JsonSerializer.Deserialize(serializedResult, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);
        Assert.NotNull(responseObject);
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
    //     var response = await _command.ExecuteAsync(_context, parseResult);

    //     // Assert - The command should handle the error gracefully and return an error response
    //     Assert.NotEqual(HttpStatusCode.OK, response.Status);
    //     Assert.NotEmpty(response.Message);
    // }

    [Fact]
    public async Task ExecuteAsync_WithEmptyState_CreatesDefaultState()
    {
        // Arrange
        var args = new[] { "--state", "" };
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var result = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, _context.Response.Status);

        string serializedResult = SerializeResponseResult(_context.Response.Results!);
        var responseObject = JsonSerializer.Deserialize(serializedResult, CloudArchitectJsonContext.Default.CloudArchitectDesignResponse);
        Assert.NotNull(responseObject);
        Assert.NotNull(responseObject.ResponseObject.State);
        Assert.Empty(responseObject.ResponseObject.State.ArchitectureComponents);
        Assert.NotNull(responseObject.ResponseObject.State.Requirements);
        Assert.Empty(responseObject.ResponseObject.State.Requirements.Explicit);
    }

    [Fact]
    public void BindOptions_WithInvalidStateJson_ThrowsException()
    {
        // Arrange
        var invalidStateJson = "{ invalid json }";
        var args = new[] { "--state", invalidStateJson };
        var parseResult = _commandDefinition.Parse(args);


        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() =>
        {
            // Access the protected BindOptions method via reflection to test state deserialization
            var command = _command.GetCommand();
            var stateOption = command.Options.First(o => o.Name == "--state");
            var stateValue = parseResult.GetValue((Option<string>)stateOption);

            // Manually call the state deserialization that happens in BindOptions
            var deserializeMethod = typeof(DesignCommand).GetMethod("DeserializeState",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.NotNull(deserializeMethod);
            deserializeMethod.Invoke(null, [stateValue]);
        });

        // Verify the inner exception is the InvalidOperationException we expect
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("Failed to deserialize state JSON", exception.InnerException!.Message);
    }

    #endregion
}
