// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Areas.Server.Commands;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Models.Command;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server;

public class ServiceStartCommandTests
{
    private readonly ServiceStartCommand _command;

    public ServiceStartCommandTests()
    {
        _command = new();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act

        // Assert
        Assert.Equal("start", _command.GetCommand().Name);
        Assert.Equal("Starts Azure MCP Server.", _command.GetCommand().Description!);
    }

    [Theory]
    [InlineData(null, "", "stdio")]
    [InlineData("storage", "storage", "stdio")]
    public void ServiceOption_ParsesCorrectly(string? inputService, string expectedService, string expectedTransport)
    {
        // Arrange
        var parseResult = CreateParseResult(inputService);

        // Act
        var actualServiceArray = parseResult.GetValue(ServiceOptionDefinitions.Namespace);
        var actualService = (actualServiceArray != null && actualServiceArray.Length > 0) ? actualServiceArray[0] : "";
        var actualTransport = parseResult.GetValue(ServiceOptionDefinitions.Transport);

        // Assert
        Assert.Equal(expectedService, actualService ?? "");
        Assert.Equal(expectedTransport, actualTransport);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InsecureDisableElicitationOption_ParsesCorrectly(bool expectedValue)
    {
        // Arrange
        var parseResult = CreateParseResultWithInsecureDisableElicitation(expectedValue);

        // Act
        var actualValue = parseResult.GetValue(ServiceOptionDefinitions.InsecureDisableElicitation);

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void InsecureDisableElicitationOption_DefaultsToFalse()
    {
        // Arrange
        var parseResult = CreateParseResult(null);

        // Act
        var actualValue = parseResult.GetValue(ServiceOptionDefinitions.InsecureDisableElicitation);

        // Assert
        Assert.False(actualValue);
    }

    [Fact]
    public void AllOptionsRegistered_IncludesInsecureDisableElicitation()
    {
        // Arrange & Act
        var command = _command.GetCommand();

        // Assert
        var hasInsecureDisableElicitationOption = command.Options.Any(o =>
            o.Name == ServiceOptionDefinitions.InsecureDisableElicitation.Name);
        Assert.True(hasInsecureDisableElicitationOption, "InsecureDisableElicitation option should be registered");
    }

    [Theory]
    [InlineData("sse")]
    [InlineData("websocket")]
    [InlineData("http")]
    [InlineData("invalid")]
    public async Task ExecuteAsync_InvalidTransport_ReturnsValidationError(string invalidTransport)
    {
        // Arrange
        var parseResult = CreateParseResultWithTransport(invalidTransport);
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains($"Invalid transport '{invalidTransport}'", response.Message);
        Assert.Contains("Valid transports are: stdio.", response.Message);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("unknown")]
    [InlineData("")]
    public async Task ExecuteAsync_InvalidMode_ReturnsValidationError(string invalidMode)
    {
        // Arrange
        var parseResult = CreateParseResultWithMode(invalidMode);
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains($"Invalid mode '{invalidMode}'", response.Message);
        Assert.Contains("Valid modes are: single, namespace, all.", response.Message);
    }

    [Theory]
    [InlineData("single")]
    [InlineData("namespace")]
    [InlineData("all")]
    [InlineData(null)] // null should be valid (uses default)
    public async Task ExecuteAsync_ValidMode_DoesNotReturnValidationError(string? validMode)
    {
        // Arrange
        var parseResult = CreateParseResultWithMode(validMode);
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert - Should not fail validation, though may fail later due to server startup
        if (response.Status == HttpStatusCode.BadRequest && response.Message?.Contains("Invalid mode") == true)
        {
            Assert.Fail($"Mode '{validMode}' should be valid but got validation error: {response.Message}");
        }
    }

    [Fact]
    public void BindOptions_WithAllOptions_ReturnsCorrectlyConfiguredOptions()
    {
        // Arrange
        var parseResult = CreateParseResultWithAllOptions();

        // Act
        var options = GetBoundOptions(parseResult);

        // Assert
        Assert.Equal("stdio", options.Transport);
        Assert.Equal(new[] { "storage", "keyvault" }, options.Namespace);
        Assert.Equal("all", options.Mode);
        Assert.True(options.ReadOnly);
        Assert.True(options.Debug);
        Assert.False(options.EnableInsecureTransports);
        Assert.True(options.InsecureDisableElicitation);
    }

    [Fact]
    public void BindOptions_WithDefaults_ReturnsDefaultValues()
    {
        // Arrange
        var parseResult = CreateParseResultWithMinimalOptions();

        // Act
        var options = GetBoundOptions(parseResult);

        // Assert
        Assert.Equal("stdio", options.Transport); // Default transport
        Assert.Null(options.Namespace);
        Assert.Equal("namespace", options.Mode); // Default mode
        Assert.False(options.ReadOnly); // Default readonly
        Assert.False(options.Debug);
        Assert.False(options.EnableInsecureTransports);
        Assert.False(options.InsecureDisableElicitation);
    }

    [Fact]
    public void Validate_WithValidOptions_ReturnsValidResult()
    {
        // Arrange
        var parseResult = CreateParseResultWithTransport("stdio");
        var commandResult = parseResult.CommandResult;

        // Act
        var result = _command.Validate(commandResult, null);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Validate_WithInvalidTransport_ReturnsInvalidResult()
    {
        // Arrange
        var parseResult = CreateParseResultWithTransport("invalid");
        var commandResult = parseResult.CommandResult;

        // Act
        var result = _command.Validate(commandResult, null);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Invalid transport 'invalid'", result.ErrorMessage);
    }

    [Fact]
    public void Validate_WithInvalidMode_ReturnsInvalidResult()
    {
        // Arrange
        var parseResult = CreateParseResultWithMode("invalid");
        var commandResult = parseResult.CommandResult;

        // Act
        var result = _command.Validate(commandResult, null);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Invalid mode 'invalid'", result.ErrorMessage);
    }

    [Fact]
    public void GetErrorMessage_WithTransportArgumentException_ReturnsCustomMessage()
    {
        // Arrange
        var exception = new ArgumentException("Invalid transport 'sse'. Valid transports are: stdio.");

        // Act
        var message = GetErrorMessage(exception);

        // Assert
        Assert.Contains("Invalid transport option specified", message);
        Assert.Contains("Use --transport stdio", message);
    }

    [Fact]
    public void GetErrorMessage_WithModeArgumentException_ReturnsCustomMessage()
    {
        // Arrange
        var exception = new ArgumentException("Invalid mode 'invalid'. Valid modes are: single, namespace, all.");

        // Act
        var message = GetErrorMessage(exception);

        // Assert
        Assert.Contains("Invalid mode option specified", message);
        Assert.Contains("Use --mode single, namespace, or all", message);
    }

    [Fact]
    public void GetErrorMessage_WithInsecureTransportException_ReturnsCustomMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("Using --enable-insecure-transport requires...");

        // Act
        var message = GetErrorMessage(exception);

        // Assert
        Assert.Contains("Insecure transport configuration error", message);
        Assert.Contains("proper authentication configured", message);
    }

    [Fact]
    public void GetStatusCode_WithArgumentException_Returns400()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");

        // Act
        var statusCode = GetStatusCode(exception);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, statusCode);
    }

    [Fact]
    public void GetStatusCode_WithInvalidOperationException_Returns422()
    {
        // Arrange
        var exception = new InvalidOperationException("Invalid operation");

        // Act
        var statusCode = GetStatusCode(exception);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, statusCode);
    }

    [Fact]
    public void GetStatusCode_WithGenericException_Returns500()
    {
        // Arrange
        var exception = new Exception("Generic error");

        // Act
        var statusCode = GetStatusCode(exception);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, statusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidTransport_DoesNotThrow()
    {
        // Arrange
        var parseResult = CreateParseResultWithTransport("stdio");
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var context = new CommandContext(serviceProvider);

        // Act & Assert - Check that ArgumentException is not thrown for valid transport
        try
        {
            await _command.ExecuteAsync(context, parseResult);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("transport"))
        {
            Assert.Fail($"ArgumentException should not be thrown for valid transport: {ex.Message}");
        }
        catch
        {
            // Other exceptions are expected since the server can't actually start in a unit test
            // We only care that ArgumentException about transport is not thrown
        }
    }

    [Fact]
    public async Task ExecuteAsync_OmittedTransport_UsesDefaultAndDoesNotThrow()
    {
        // Arrange
        var parseResult = CreateParseResultWithoutTransport();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var context = new CommandContext(serviceProvider);

        // Act & Assert - Check that ArgumentException is not thrown when transport is omitted
        try
        {
            await _command.ExecuteAsync(context, parseResult);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("transport"))
        {
            Assert.Fail($"ArgumentException should not be thrown when transport is omitted (should use default): {ex.Message}");
        }
        catch
        {
            // Other exceptions are expected since the server can't actually start in a unit test
            // We only care that ArgumentException about transport is not thrown
        }
    }

    private static ParseResult CreateParseResult(string? serviceValue)
    {
        var root = new RootCommand
        {
            ServiceOptionDefinitions.Namespace,
            ServiceOptionDefinitions.Transport
        };
        var args = new List<string>();
        if (!string.IsNullOrEmpty(serviceValue))
        {
            args.Add("--namespace");
            args.Add(serviceValue);
        }
        // Add required transport default for test
        args.Add("--transport");
        args.Add("stdio");

        return root.Parse([.. args]);
    }

    private static ParseResult CreateParseResultWithInsecureDisableElicitation(bool insecureDisableElicitation)
    {
        var root = new RootCommand
        {
            ServiceOptionDefinitions.Namespace,
            ServiceOptionDefinitions.Transport,
            ServiceOptionDefinitions.InsecureDisableElicitation
        };
        var args = new List<string>
        {
            "--transport",
            "stdio"
        };

        if (insecureDisableElicitation)
        {
            args.Add("--insecure-disable-elicitation");
        }

        return root.Parse([.. args]);
    }

    private static ParseResult CreateParseResultWithTransport(string transport)
    {
        var root = new RootCommand
        {
            ServiceOptionDefinitions.Namespace,
            ServiceOptionDefinitions.Transport,
            ServiceOptionDefinitions.Mode,
            ServiceOptionDefinitions.ReadOnly,
            ServiceOptionDefinitions.Debug,
            ServiceOptionDefinitions.EnableInsecureTransports,
            ServiceOptionDefinitions.InsecureDisableElicitation
        };
        var args = new List<string>
        {
            "--transport",
            transport,
            "--mode",
            "all",
            "--read-only"
        };

        return root.Parse([.. args]);
    }

    private static ParseResult CreateParseResultWithoutTransport()
    {
        var root = new RootCommand
        {
            ServiceOptionDefinitions.Namespace,
            ServiceOptionDefinitions.Transport,
            ServiceOptionDefinitions.Mode,
            ServiceOptionDefinitions.ReadOnly,
            ServiceOptionDefinitions.Debug,
            ServiceOptionDefinitions.EnableInsecureTransports,
            ServiceOptionDefinitions.InsecureDisableElicitation
        };
        var args = new List<string>
        {
            "--mode",
            "all",
            "--read-only"
        };

        return root.Parse([.. args]);
    }

    private static ParseResult CreateParseResultWithMode(string? mode)
    {
        var root = new RootCommand
        {
            ServiceOptionDefinitions.Namespace,
            ServiceOptionDefinitions.Transport,
            ServiceOptionDefinitions.Mode,
            ServiceOptionDefinitions.ReadOnly,
            ServiceOptionDefinitions.Debug,
            ServiceOptionDefinitions.EnableInsecureTransports,
            ServiceOptionDefinitions.InsecureDisableElicitation
        };
        var args = new List<string>
        {
            "--transport",
            "stdio"
        };

        if (mode is not null)
        {
            args.Add("--mode");
            args.Add(mode);
        }

        return root.Parse([.. args]);
    }

    private static ParseResult CreateParseResultWithAllOptions()
    {
        var root = new RootCommand
        {
            ServiceOptionDefinitions.Namespace,
            ServiceOptionDefinitions.Transport,
            ServiceOptionDefinitions.Mode,
            ServiceOptionDefinitions.ReadOnly,
            ServiceOptionDefinitions.Debug,
            ServiceOptionDefinitions.EnableInsecureTransports,
            ServiceOptionDefinitions.InsecureDisableElicitation
        };
        var args = new List<string>
        {
            "--transport", "stdio",
            "--namespace", "storage",
            "--namespace", "keyvault",
            "--mode", "all",
            "--read-only",
            "--debug",
            "--insecure-disable-elicitation"
        };

        return root.Parse([.. args]);
    }

    private static ParseResult CreateParseResultWithMinimalOptions()
    {
        var root = new RootCommand
        {
            ServiceOptionDefinitions.Namespace,
            ServiceOptionDefinitions.Transport,
            ServiceOptionDefinitions.Mode,
            ServiceOptionDefinitions.ReadOnly,
            ServiceOptionDefinitions.Debug,
            ServiceOptionDefinitions.EnableInsecureTransports,
            ServiceOptionDefinitions.InsecureDisableElicitation
        };
        var args = new List<string>();

        return root.Parse([.. args]);
    }

    private ServiceStartOptions GetBoundOptions(ParseResult parseResult)
    {
        // Use reflection to access the protected BindOptions method
        var method = typeof(ServiceStartCommand).GetMethod("BindOptions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (ServiceStartOptions)method!.Invoke(_command, [parseResult])!;
    }

    private string GetErrorMessage(Exception exception)
    {
        // Use reflection to access the protected GetErrorMessage method
        var method = typeof(ServiceStartCommand).GetMethod("GetErrorMessage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (string)method!.Invoke(_command, [exception])!;
    }

    private HttpStatusCode GetStatusCode(Exception exception)
    {
        // Use reflection to access the protected GetStatusCode method
        var method = typeof(ServiceStartCommand).GetMethod("GetStatusCode",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (HttpStatusCode)method!.Invoke(_command, [exception])!;
    }
}
