// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Policy.Commands;
using Azure.Mcp.Tools.Policy.Commands.Definition;
using Azure.Mcp.Tools.Policy.Models;
using Azure.Mcp.Tools.Policy.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Policy.UnitTests.Definition;

public class PolicyDefinitionGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPolicyService _service;
    private readonly ILogger<PolicyDefinitionGetCommand> _logger;

    public PolicyDefinitionGetCommandTests()
    {
        _service = Substitute.For<IPolicyService>();
        _logger = Substitute.For<ILogger<PolicyDefinitionGetCommand>>();

        var services = new ServiceCollection();
        services.AddSingleton(_service);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        var command = new PolicyDefinitionGetCommand(_logger);

        // Assert
        Assert.NotNull(command);
        Assert.Equal("get", command.Name);
        Assert.Equal("Get Policy Definition", command.Title);
        Assert.Contains("policy definition", command.Description.ToLower());
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.Idempotent);
        Assert.False(command.Metadata.OpenWorld);
        Assert.True(command.Metadata.ReadOnly);
        Assert.False(command.Metadata.LocalRequired);
        Assert.False(command.Metadata.Secret);
    }

    [Theory]
    [InlineData("", "", false, "either --subscription or --management-group")]
    [InlineData("test-definition", "", false, "either --subscription or --management-group")]
    [InlineData("", "test-sub", false, "missing required options")]
    [InlineData("test-definition", "test-sub", true, null)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(
        string definitionName,
        string subscription,
        bool shouldSucceed,
        string? expectedErrorContext)
    {
        // Arrange
        var command = new PolicyDefinitionGetCommand(_logger);
        var args = new List<string>();

        if (!string.IsNullOrEmpty(definitionName))
            args.AddRange(["--definition-name", definitionName]);
        if (!string.IsNullOrEmpty(subscription))
            args.AddRange(["--subscription", subscription]);

        var parseResult = command.GetCommand().Parse([.. args]);
        var context = new CommandContext(_serviceProvider);

        if (shouldSucceed)
        {
            var definition = new PolicyDefinition
            {
                Id = $"/subscriptions/{subscription}/providers/Microsoft.Authorization/policyDefinitions/{definitionName}",
                Name = definitionName,
                DisplayName = "Test Definition"
            };

            _service.GetPolicyDefinitionAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(definition);
        }

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
            if (expectedErrorContext != null)
            {
                Assert.Contains(expectedErrorContext.ToLower(), response.Message.ToLower());
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithManagementGroup_RetrievesDefinitionSuccessfully()
    {
        // Arrange
        var command = new PolicyDefinitionGetCommand(_logger);
        var definition = new PolicyDefinition
        {
            Id = "/providers/Microsoft.Management/managementGroups/test-mg/providers/Microsoft.Authorization/policyDefinitions/test-def",
            Name = "test-def",
            DisplayName = "Test Management Group Definition"
        };

        _service.GetPolicyDefinitionAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(definition);

        var parseResult = command.GetCommand().Parse(["--management-group", "test-mg", "--definition-name", "test-def"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        await _service.Received(1).GetPolicyDefinitionAsync(
            "test-def",
            null,
            "test-mg",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithBothSubscriptionAndManagementGroup_ReturnsBadRequest()
    {
        // Arrange
        var command = new PolicyDefinitionGetCommand(_logger);
        var parseResult = command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--management-group", "test-mg",
            "--definition-name", "test-def"
        ]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("cannot specify both", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WithNeitherSubscriptionNorManagementGroup_ReturnsBadRequest()
    {
        // Arrange
        var command = new PolicyDefinitionGetCommand(_logger);
        var parseResult = command.GetCommand().Parse(["--definition-name", "test-def"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("either --subscription or --management-group must be specified", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_ManagementGroupDefinitionNotFound_ReturnsNotFoundWithCorrectMessage()
    {
        // Arrange
        var command = new PolicyDefinitionGetCommand(_logger);

        _service.GetPolicyDefinitionAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns((PolicyDefinition?)null);

        var parseResult = command.GetCommand().Parse(["--management-group", "test-mg", "--definition-name", "test-def"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("management group", response.Message.ToLower());
        Assert.Contains("test-mg", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var command = new PolicyDefinitionGetCommand(_logger);
        var definition = new PolicyDefinition
        {
            Id = "/subscriptions/test-sub/providers/Microsoft.Authorization/policyDefinitions/test-def",
            Name = "test-def",
            DisplayName = "Test Definition",
            Description = "Test description",
            PolicyType = "Custom",
            Mode = "All"
        };

        _service.GetPolicyDefinitionAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(definition);

        var parseResult = command.GetCommand().Parse(["--subscription", "test-sub", "--definition-name", "test-def"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var commandResult = JsonSerializer.Deserialize(json, PolicyJsonContext.Default.PolicyDefinitionGetCommandResult);
        Assert.NotNull(commandResult);
        Assert.NotNull(commandResult.Definition);
        Assert.Equal("test-def", commandResult.Definition.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var command = new PolicyDefinitionGetCommand(_logger);

        _service.GetPolicyDefinitionAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        var parseResult = command.GetCommand().Parse(["--subscription", "test-sub", "--definition-name", "test-def"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("error", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectParameters()
    {
        // Arrange
        var command = new PolicyDefinitionGetCommand(_logger);
        var definition = new PolicyDefinition
        {
            Id = "/subscriptions/test-sub/providers/Microsoft.Authorization/policyDefinitions/test-def",
            Name = "test-def"
        };

        _service.GetPolicyDefinitionAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(definition);

        var parseResult = command.GetCommand().Parse(["--subscription", "test-sub", "--definition-name", "test-def"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _service.Received(1).GetPolicyDefinitionAsync(
            "test-def",
            "test-sub",
            null,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNullDefinition()
    {
        // Arrange
        var command = new PolicyDefinitionGetCommand(_logger);

        _service.GetPolicyDefinitionAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns((PolicyDefinition?)null);

        var parseResult = command.GetCommand().Parse(["--subscription", "test-sub", "--definition-name", "test-def"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var command = new PolicyDefinitionGetCommand(_logger);
        var parseResult = command.GetCommand().Parse(["--subscription", "test-sub", "--definition-name", "test-def"]);
        var bindMethod = typeof(PolicyDefinitionGetCommand)
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.NotNull(bindMethod);

        // Act
        var options = bindMethod.Invoke(command, [parseResult]);

        // Assert
        Assert.NotNull(options);
    }

}

