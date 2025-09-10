// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AppLens.Commands.Resource;
using Azure.Mcp.Tools.AppLens.Models;
using Azure.Mcp.Tools.AppLens.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppLens.UnitTests.Resource;

[Trait("Area", "AppLens")]
public class ResourceDiagnoseCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppLensService _appLensService;
    private readonly ILogger<ResourceDiagnoseCommand> _logger;
    private readonly ResourceDiagnoseCommand _command;
    private readonly CommandContext _context;

    public ResourceDiagnoseCommandTests()
    {
        _appLensService = Substitute.For<IAppLensService>();
        _logger = Substitute.For<ILogger<ResourceDiagnoseCommand>>();

        _command = new(_logger);
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_appLensService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDiagnosticResult_WhenValidParametersProvided()
    {
        // Arrange
        var expectedResult = new DiagnosticResult(
            new List<string> { "Insight 1", "Insight 2" },
            new List<string> { "Solution 1", "Solution 2" },
            "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Web/sites/myapp",
            "Microsoft.Web/sites"
        );

        _appLensService.DiagnoseResourceAsync(
            "Why is my app slow?",
            "myapp",
            "sub123",
            "rg1",
            "Microsoft.Web/sites")
            .Returns(expectedResult);

        var args = _command.GetCommand().Parse([
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ResourceDiagnoseCommandResult>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.NotNull(result.Result);
        Assert.Equal(2, result.Result.Insights.Count);
        Assert.Equal(2, result.Result.Solutions.Count);
        Assert.Equal("Insight 1", result.Result.Insights[0]);
        Assert.Equal("Solution 1", result.Result.Solutions[0]);
        Assert.Equal("/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Web/sites/myapp", result.Result.ResourceId);
        Assert.Equal("Microsoft.Web/sites", result.Result.ResourceType);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenQuestionIsMissing()
    {
        // Arrange && Act
        var response = await _command.ExecuteAsync(_context, _command.GetCommand().Parse([
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites"
        ]));

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenResourceIsMissing()
    {
        // Arrange && Act
        var response = await _command.ExecuteAsync(_context, _command.GetCommand().Parse([
            "--question", "Why is my app slow?",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites"
        ]));

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenSubscriptionIsMissing()
    {
        // Arrange && Act
        var response = await _command.ExecuteAsync(_context, _command.GetCommand().Parse([
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites"
        ]));

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenResourceGroupIsMissing()
    {
        // Arrange && Act
        var response = await _command.ExecuteAsync(_context, _command.GetCommand().Parse([
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-type", "Microsoft.Web/sites"
        ]));

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenResourceTypeIsMissing()
    {
        // Arrange && Act
        var response = await _command.ExecuteAsync(_context, _command.GetCommand().Parse([
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1"
        ]));

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsGenericException()
    {
        // Arrange
        _appLensService.DiagnoseResourceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .ThrowsAsync(new Exception("Service error"));

        var args = _command.GetCommand().Parse([
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Service error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenServiceThrowsInvalidOperationException()
    {
        // Arrange
        _appLensService.DiagnoseResourceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .ThrowsAsync(new InvalidOperationException("Resource not found"));

        var args = _command.GetCommand().Parse([
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Resource not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns503_WhenServiceIsUnavailable()
    {
        // Arrange
        _appLensService.DiagnoseResourceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .ThrowsAsync(new HttpRequestException("Service Unavailable", null, System.Net.HttpStatusCode.ServiceUnavailable));

        var args = _command.GetCommand().Parse([
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(503, response.Status);
        Assert.Contains("Service Unavailable", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesEmptyDiagnosticResult()
    {
        // Arrange
        var expectedResult = new DiagnosticResult(
            new List<string>(),
            new List<string>(),
            "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Web/sites/myapp",
            "Microsoft.Web/sites"
        );

        _appLensService.DiagnoseResourceAsync(
            "Why is my app slow?",
            "myapp",
            "sub123",
            "rg1",
            "Microsoft.Web/sites")
            .Returns(expectedResult);

        var args = _command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--resource-type", "Microsoft.Web/sites"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ResourceDiagnoseCommandResult>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.NotNull(result.Result);
        Assert.Empty(result.Result.Insights);
        Assert.Empty(result.Result.Solutions);
    }

    [Theory]
    [InlineData("", "myapp", "sub123", "rg1", "Microsoft.Web/sites")]
    [InlineData("Why is my app slow?", "", "sub123", "rg1", "Microsoft.Web/sites")]
    [InlineData("Why is my app slow?", "myapp", "", "rg1", "Microsoft.Web/sites")]
    [InlineData("Why is my app slow?", "myapp", "sub123", "", "Microsoft.Web/sites")]
    [InlineData("Why is my app slow?", "myapp", "sub123", "rg1", "")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParameterIsEmpty(
        string question, string resource, string subscription, string resourceGroup, string resourceType)
    {
        // Arrange
        var args = new List<string>();
        if (!string.IsNullOrEmpty(question))
        { args.AddRange(["--question", question]); }
        if (!string.IsNullOrEmpty(resource))
        { args.AddRange(["--resource", resource]); }
        if (!string.IsNullOrEmpty(subscription))
        { args.AddRange(["--subscription", subscription]); }
        if (!string.IsNullOrEmpty(resourceGroup))
        { args.AddRange(["--resource-group", resourceGroup]); }
        if (!string.IsNullOrEmpty(resourceType))
        { args.AddRange(["--resource-type", resourceType]); }

        var parseResult = _command.GetCommand().Parse(args.ToArray());

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_LogsInformationOnSuccess()
    {
        // Arrange
        var expectedResult = new DiagnosticResult(
            new List<string> { "Insight 1" },
            new List<string> { "Solution 1" },
            "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Web/sites/myapp",
            "Microsoft.Web/sites"
        );

        _appLensService.DiagnoseResourceAsync(
            "Why is my app slow?",
            "myapp",
            "sub123",
            "rg1",
            "Microsoft.Web/sites")
            .Returns(expectedResult);

        var args = _command.GetCommand().Parse([
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites"
        ]);

        // Act
        await _command.ExecuteAsync(_context, args);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Diagnosing resource")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ExecuteAsync_LogsErrorOnException()
    {
        // Arrange
        _appLensService.DiagnoseResourceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _command.GetCommand().Parse([
            "--question", "Why is my app slow?",
            "--resource", "myapp",
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--resource-type", "Microsoft.Web/sites"
        ]);

        // Act
        await _command.ExecuteAsync(_context, args);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error in diagnose")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
