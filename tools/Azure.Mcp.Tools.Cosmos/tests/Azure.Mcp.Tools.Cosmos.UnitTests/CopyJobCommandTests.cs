// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Cosmos.Commands.CopyJob;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.UnitTests;

public class CopyJobGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICopyJobService _copyJobService;
    private readonly ILogger<CopyJobGetCommand> _logger;
    private readonly CopyJobGetCommand _command;
    private readonly Command _commandDefinition;
    private readonly CommandContext _context;

    public CopyJobGetCommandTests()
    {
        _copyJobService = Substitute.For<ICopyJobService>();
        _logger = Substitute.For<ILogger<CopyJobGetCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_copyJobService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public void Name_IsCorrect()
    {
        Assert.Equal("get", _command.Name);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        Assert.False(_command.Metadata.Destructive);
        Assert.True(_command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsJob_WhenAllParametersProvided()
    {
        // Arrange
        using var expectedDoc = JsonDocument.Parse("{\"id\":\"job1\",\"properties\":{\"status\":\"Completed\"}}");
        var expectedJob = expectedDoc.RootElement.Clone();

        _copyJobService.GetJobAsync(
            Arg.Is("sub123"),
            Arg.Is("myaccount"),
            Arg.Is("myjob"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedJob);

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--account", "myaccount",
            "--job-name", "myjob"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrows()
    {
        // Arrange
        _copyJobService.GetJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--account", "myaccount",
            "--job-name", "myjob"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Service error", response.Message);
    }

    [Theory]
    [InlineData("--account", "myaccount", "--job-name", "myjob")] // Missing subscription
    [InlineData("--subscription", "sub123", "--job-name", "myjob")] // Missing account
    [InlineData("--subscription", "sub123", "--account", "myaccount")] // Missing job-name
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}

public class CopyJobCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICopyJobService _copyJobService;
    private readonly ILogger<CopyJobCreateCommand> _logger;
    private readonly CopyJobCreateCommand _command;
    private readonly Command _commandDefinition;
    private readonly CommandContext _context;

    private const string SampleJobProperties =
        "{\"jobType\":\"NoSqlRUToNoSqlRU\",\"tasks\":[{\"source\":{\"databaseName\":\"db1\",\"containerName\":\"src\"},\"destination\":{\"databaseName\":\"db1\",\"containerName\":\"dest\"}}]}";

    public CopyJobCreateCommandTests()
    {
        _copyJobService = Substitute.For<ICopyJobService>();
        _logger = Substitute.For<ILogger<CopyJobCreateCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_copyJobService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public void Name_IsCorrect()
    {
        Assert.Equal("create", _command.Name);
    }

    [Fact]
    public void Metadata_IsDestructive()
    {
        Assert.True(_command.Metadata.Destructive);
        Assert.False(_command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesJob_WithRequiredParameters()
    {
        // Arrange
        using var expectedDoc = JsonDocument.Parse("{\"id\":\"job1\",\"properties\":{\"status\":\"Created\"}}");
        var expectedJob = expectedDoc.RootElement.Clone();

        _copyJobService.CreateJobAsync(
            Arg.Is("sub123"),
            Arg.Is("myaccount"),
            Arg.Is("myjob"),
            Arg.Is(SampleJobProperties),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedJob);

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--account", "myaccount",
            "--job-name", "myjob",
            "--job-properties", SampleJobProperties
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesJob_WithOptionalParameters()
    {
        // Arrange
        using var expectedDoc = JsonDocument.Parse("{\"id\":\"job1\"}");
        var expectedJob = expectedDoc.RootElement.Clone();

        _copyJobService.CreateJobAsync(
            Arg.Is("sub123"),
            Arg.Is("myaccount"),
            Arg.Is("myjob"),
            Arg.Is(SampleJobProperties),
            Arg.Is("Online"),
            Arg.Is(4),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedJob);

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--account", "myaccount",
            "--job-name", "myjob",
            "--job-properties", SampleJobProperties,
            "--mode", "Online",
            "--worker-count", "4"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrows()
    {
        // Arrange
        _copyJobService.CreateJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Create failed"));

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--account", "myaccount",
            "--job-name", "myjob",
            "--job-properties", SampleJobProperties
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Create failed", response.Message);
    }

    [Theory]
    [InlineData("--account", "myaccount", "--job-name", "myjob", "--job-properties", "{}")] // Missing subscription
    [InlineData("--subscription", "sub123", "--job-name", "myjob", "--job-properties", "{}")] // Missing account
    [InlineData("--subscription", "sub123", "--account", "myaccount", "--job-properties", "{}")] // Missing job-name
    [InlineData("--subscription", "sub123", "--account", "myaccount", "--job-name", "myjob")] // Missing job-properties
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}

public class CopyJobListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICopyJobService _copyJobService;
    private readonly ILogger<CopyJobListCommand> _logger;
    private readonly CopyJobListCommand _command;
    private readonly Command _commandDefinition;
    private readonly CommandContext _context;

    public CopyJobListCommandTests()
    {
        _copyJobService = Substitute.For<ICopyJobService>();
        _logger = Substitute.For<ILogger<CopyJobListCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_copyJobService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public void Name_IsCorrect()
    {
        Assert.Equal("list", _command.Name);
    }

    [Fact]
    public void Metadata_IsReadOnly()
    {
        Assert.False(_command.Metadata.Destructive);
        Assert.True(_command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsJobs_WhenJobsExist()
    {
        // Arrange
        using var doc1 = JsonDocument.Parse("{\"name\":\"job1\",\"properties\":{\"status\":\"Completed\"}}");
        using var doc2 = JsonDocument.Parse("{\"name\":\"job2\",\"properties\":{\"status\":\"Running\"}}");
        var expectedJobs = new List<JsonElement>
        {
            doc1.RootElement.Clone(),
            doc2.RootElement.Clone()
        };

        _copyJobService.ListJobsAsync(
            Arg.Is("sub123"),
            Arg.Is("myaccount"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedJobs);

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--account", "myaccount"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoJobsExist()
    {
        // Arrange
        _copyJobService.ListJobsAsync(
            Arg.Is("sub123"),
            Arg.Is("myaccount"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<JsonElement>());

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--account", "myaccount"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrows()
    {
        _copyJobService.ListJobsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("List failed"));

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--account", "myaccount"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("List failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenSubscriptionMissing()
    {
        var args = _commandDefinition.Parse(["--account", "myaccount"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}

public class CopyJobCancelCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICopyJobService _copyJobService;
    private readonly ILogger<CopyJobCancelCommand> _logger;
    private readonly CopyJobCancelCommand _command;
    private readonly Command _commandDefinition;
    private readonly CommandContext _context;

    public CopyJobCancelCommandTests()
    {
        _copyJobService = Substitute.For<ICopyJobService>();
        _logger = Substitute.For<ILogger<CopyJobCancelCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_copyJobService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public void Name_IsCorrect()
    {
        Assert.Equal("cancel", _command.Name);
    }

    [Fact]
    public void Metadata_IsDestructive()
    {
        Assert.True(_command.Metadata.Destructive);
        Assert.False(_command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_CancelsJob_WhenAllParametersProvided()
    {
        // Arrange
        using var expectedDoc = JsonDocument.Parse("{\"status\":\"cancel accepted\",\"jobName\":\"myjob\"}");
        var expectedResult = expectedDoc.RootElement.Clone();

        _copyJobService.CancelJobAsync(
            Arg.Is("sub123"),
            Arg.Is("myaccount"),
            Arg.Is("myjob"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--account", "myaccount",
            "--job-name", "myjob"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrows()
    {
        _copyJobService.CancelJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Cancel failed"));

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--account", "myaccount",
            "--job-name", "myjob"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Cancel failed", response.Message);
    }

    [Theory]
    [InlineData("--account", "myaccount", "--job-name", "myjob")] // Missing subscription
    [InlineData("--subscription", "sub123", "--job-name", "myjob")] // Missing account
    [InlineData("--subscription", "sub123", "--account", "myaccount")] // Missing job-name
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}
