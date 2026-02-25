// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Telemetry;
using NSubstitute;
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
    public void DangerouslyDisableElicitationOption_ParsesCorrectly(bool expectedValue)
    {
        // Arrange
        var parseResult = CreateParseResultWithDangerouslyDisableElicitation(expectedValue);

        // Act
        var actualValue = parseResult.GetValue(ServiceOptionDefinitions.DangerouslyDisableElicitation);

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void DangerouslyDisableElicitationOption_DefaultsToFalse()
    {
        // Arrange
        var parseResult = CreateParseResult(null);

        // Act
        var actualValue = parseResult.GetValue(ServiceOptionDefinitions.DangerouslyDisableElicitation);

        // Assert
        Assert.False(actualValue);
    }

    [Fact]
    public void AllOptionsRegistered_IncludesDangerouslyDisableElicitation()
    {
        // Arrange & Act
        var command = _command.GetCommand();

        // Assert
        var hasDangerouslyDisableElicitationOption = command.Options.Any(o =>
            o.Name == ServiceOptionDefinitions.DangerouslyDisableElicitation.Name);
        Assert.True(hasDangerouslyDisableElicitationOption, "DangerouslyDisableElicitation option should be registered");
    }

    [Fact]
    public void AllOptionsRegistered_IncludesTool()
    {
        // Arrange & Act
        var command = _command.GetCommand();

        // Assert
        var hasToolOption = command.Options.Any(o =>
            o.Name == ServiceOptionDefinitions.Tool.Name);
        Assert.True(hasToolOption, "Tool option should be registered");
    }

    [Theory]
    [InlineData("azmcp_storage_account_get")]
    [InlineData("azmcp_keyvault_secret_get")]
    [InlineData(null)]
    public void ToolOption_ParsesCorrectly(string? expectedTool)
    {
        // Arrange
        var parseResult = CreateParseResultWithTool(expectedTool != null ? [expectedTool] : null);

        // Act
        var actualTools = parseResult.GetValue(ServiceOptionDefinitions.Tool);

        // Assert
        if (expectedTool == null)
        {
            Assert.True(actualTools == null || actualTools.Length == 0);
        }
        else
        {
            Assert.NotNull(actualTools);
            Assert.Single(actualTools);
            Assert.Equal(expectedTool, actualTools[0]);
        }
    }

    [Fact]
    public void ToolOption_ParsesMultipleToolsCorrectly()
    {
        // Arrange
        var expectedTools = new[] { "azmcp_storage_account_get", "azmcp_keyvault_secret_get" };
        var parseResult = CreateParseResultWithTool(expectedTools);

        // Act
        var actualTools = parseResult.GetValue(ServiceOptionDefinitions.Tool);

        // Assert
        Assert.NotNull(actualTools);
        Assert.Equal(expectedTools.Length, actualTools.Length);
        Assert.Equal(expectedTools, actualTools);
    }

    [Theory]
    [InlineData("sse")]
    [InlineData("websocket")]
    [InlineData("invalid")]
    public async Task ExecuteAsync_InvalidTransport_ReturnsValidationError(string invalidTransport)
    {
        // Arrange
        var parseResult = CreateParseResultWithTransport(invalidTransport);
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains($"Invalid transport '{invalidTransport}'", response.Message);
        Assert.Contains("Valid transports are: stdio, http.", response.Message);
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
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains($"Invalid mode '{invalidMode}'", response.Message);
        Assert.Contains("Valid modes are: single, namespace, all, consolidated.", response.Message);
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
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

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
        Assert.Equal(TransportTypes.StdIo, options.Transport);
        Assert.Equal(new[] { "storage", "keyvault" }, options.Namespace);
        Assert.Equal("all", options.Mode);
        Assert.True(options.ReadOnly);
        Assert.True(options.Debug);
        Assert.False(options.DangerouslyDisableHttpIncomingAuth);
        Assert.True(options.DangerouslyDisableElicitation);
    }

    [Fact]
    public void BindOptions_WithTool_ReturnsCorrectlyConfiguredOptions()
    {
        // Arrange
        var expectedTool = "azmcp_group_list";
        var parseResult = CreateParseResultWithTool([expectedTool]);

        // Act
        var options = GetBoundOptions(parseResult);

        // Assert
        Assert.NotNull(options.Tool);
        Assert.Single(options.Tool);
        Assert.Equal(expectedTool, options.Tool[0]);
        Assert.Equal(TransportTypes.StdIo, options.Transport);
        Assert.Equal("all", options.Mode);
    }

    [Fact]
    public void BindOptions_WithMultipleToolsAndExplicitMode_OverridesToAllMode()
    {
        // Arrange - Explicitly set mode to single but also provide multiple tools
        var tools = new[] { "azmcp_group_list", "azmcp_subscription_list" };
        var parseResult = CreateParseResultWithToolsAndMode(tools, "single");

        // Act
        var options = GetBoundOptions(parseResult);

        // Assert
        Assert.NotNull(options.Tool);
        Assert.Equal(2, options.Tool.Length);
        Assert.Equal(tools, options.Tool);
        Assert.Equal("all", options.Mode);
    }

    [Fact]
    public void BindOptions_WithDefaults_ReturnsDefaultValues()
    {
        // Arrange
        var parseResult = CreateParseResultWithMinimalOptions();

        // Act
        var options = GetBoundOptions(parseResult);

        // Assert
        Assert.Equal(TransportTypes.StdIo, options.Transport); // Default transport
        Assert.Null(options.Namespace);
        Assert.Equal("namespace", options.Mode); // Default mode
        Assert.False(options.ReadOnly); // Default readonly
        Assert.False(options.Debug);
        Assert.False(options.DangerouslyDisableHttpIncomingAuth);
        Assert.False(options.DangerouslyDisableElicitation);
        Assert.Null(options.SupportLoggingFolder);
    }

    [Theory]
    [InlineData("/tmp/logs")]
    [InlineData("C:\\logs")]
    [InlineData(null)]
    public void DangerouslyWriteSupportLogsToDirOption_ParsesCorrectly(string? expectedFolder)
    {
        // Arrange
        var parseResult = CreateParseResultWithSupportLogging(expectedFolder);

        // Act
        var actualValue = parseResult.GetValue(ServiceOptionDefinitions.DangerouslyWriteSupportLogsToDir);

        // Assert
        Assert.Equal(expectedFolder, actualValue);
    }

    [Fact]
    public void BindOptions_WithSupportLoggingFolder_ReturnsCorrectlyConfiguredOptions()
    {
        // Arrange
        var logFolder = "/tmp/mcp-support-logs";
        var parseResult = CreateParseResultWithSupportLogging(logFolder);

        // Act
        var options = GetBoundOptions(parseResult);

        // Assert
        Assert.Equal(logFolder, options.SupportLoggingFolder);
    }

    [Fact]
    public void BindOptions_WithoutSupportLoggingFolder_ReturnsCorrectlyConfiguredOptions()
    {
        // Arrange
        var parseResult = CreateParseResultWithSupportLogging(null);

        // Act
        var options = GetBoundOptions(parseResult);

        // Assert
        Assert.Null(options.SupportLoggingFolder);
    }

    [Fact]
    public void AllOptionsRegistered_IncludesSupportLoggingToFolder()
    {
        // Arrange & Act
        var command = _command.GetCommand();

        // Assert
        var hasSupportLoggingFolderOption = command.Options.Any(o =>
            o.Name == ServiceOptionDefinitions.DangerouslyWriteSupportLogsToDir.Name);
        Assert.True(hasSupportLoggingFolderOption, "DangerouslyWriteSupportLogsToDir option should be registered");
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
        Assert.Empty(result.Errors);
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
        Assert.Contains("Invalid transport 'invalid'", string.Join('\n', result.Errors));
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
        Assert.Contains("Invalid mode 'invalid'", string.Join('\n', result.Errors));
    }

    [Fact]
    public void Validate_WithNamespaceAndTool_ReturnsInvalidResult()
    {
        // Arrange
        var parseResult = CreateParseResultWithNamespaceAndTool();
        var commandResult = parseResult.CommandResult;

        // Act
        var result = _command.Validate(commandResult, null);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("--namespace and --tool options cannot be used together", string.Join('\n', result.Errors));
    }

    [Fact]
    public void Validate_WithSupportLoggingFolderWhitespace_ReturnsInvalidResult()
    {
        // Arrange
        var parseResult = CreateParseResultWithSupportLogging("   ");
        var commandResult = parseResult.CommandResult;

        // Act
        var result = _command.Validate(commandResult, null);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("The --dangerously-write-support-logs-to-dir option requires a valid folder path", string.Join('\n', result.Errors));
    }

    [Fact]
    public void Validate_WithValidSupportLoggingFolder_ReturnsValidResult()
    {
        // Arrange
        var parseResult = CreateParseResultWithSupportLogging("/tmp/mcp-support-logs");
        var commandResult = parseResult.CommandResult;

        // Act
        var result = _command.Validate(commandResult, null);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithoutSupportLoggingFolder_ReturnsValidResult()
    {
        // Arrange
        var parseResult = CreateParseResultWithSupportLogging(null);
        var commandResult = parseResult.CommandResult;

        // Act
        var result = _command.Validate(commandResult, null);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_WithSupportLoggingFolderWhitespace_ReturnsValidationError()
    {
        // Arrange
        var parseResult = CreateParseResultWithSupportLogging("   ");
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("The --dangerously-write-support-logs-to-dir option requires a valid folder path", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithNamespaceAndTool_ReturnsValidationError()
    {
        // Arrange
        var parseResult = CreateParseResultWithNamespaceAndTool();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--namespace and --tool options cannot be used together", response.Message);
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
    public void GetErrorMessage_WithDangerouslyDisableHttpIncomingAuthException_ReturnsCustomMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("Using --dangerously-disable-http-incoming-auth requires...");

        // Act
        var message = GetErrorMessage(exception);

        // Assert
        Assert.Contains("Configuration error to disable incoming HTTP authentication", message);
        Assert.Contains("proper authentication is configured", message);
    }

    [Fact]
    public void GetErrorMessage_WithNamespaceAndToolException_ReturnsCustomMessage()
    {
        // Arrange
        var exception = new ArgumentException("--namespace and --tool options cannot be used together");

        // Act
        var message = GetErrorMessage(exception);

        // Assert
        Assert.Contains("Configuration error", message);
        Assert.Contains("mutually exclusive", message);
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
            await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);
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
            await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);
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


    [Fact]
    public void InitializedHandler_SetsStartupInformation()
    {
        // Arrange
        var serviceStartOptions = new ServiceStartOptions
        {
            Transport = TransportTypes.StdIo,
            Mode = "test-mode",
            Tool = ["test-tool1", "test-tool2"],
            ReadOnly = false,
            Debug = true,
            Namespace = ["storage", "keyvault"],
            DangerouslyDisableElicitation = false,
            DangerouslyDisableHttpIncomingAuth = true,
        };
        var activity = new Activity("test-activity");
        var mockTelemetry = Substitute.For<ITelemetryService>();
        mockTelemetry.StartActivity(Arg.Any<string>()).Returns(activity);


        // Act
        ServiceStartCommand.LogStartTelemetry(mockTelemetry, serviceStartOptions);

        // Assert
        mockTelemetry.Received(1).StartActivity(ActivityName.ServerStarted);

        var dangerouslyDisableHttpIncomingAuth = GetAndAssertTagKeyValue(activity, TagName.DangerouslyDisableHttpIncomingAuth);
        Assert.Equal(serviceStartOptions.DangerouslyDisableHttpIncomingAuth, dangerouslyDisableHttpIncomingAuth);

        var dangerouslyDisableElicitation = GetAndAssertTagKeyValue(activity, TagName.DangerouslyDisableElicitation);
        Assert.Equal(serviceStartOptions.DangerouslyDisableElicitation, dangerouslyDisableElicitation);

        var transport = GetAndAssertTagKeyValue(activity, TagName.Transport);
        Assert.Equal(serviceStartOptions.Transport, transport);

        var mode = GetAndAssertTagKeyValue(activity, TagName.ServerMode);
        Assert.Equal(serviceStartOptions.Mode, mode);

        var tool = GetAndAssertTagKeyValue(activity, TagName.Tool);
        Assert.Equal(string.Join(",", serviceStartOptions.Tool), tool);

        var readOnly = GetAndAssertTagKeyValue(activity, TagName.IsReadOnly);
        Assert.Equal(serviceStartOptions.ReadOnly, readOnly);

        var debug = GetAndAssertTagKeyValue(activity, TagName.IsDebug);
        Assert.Equal(serviceStartOptions.Debug, debug);

        var namespaces = GetAndAssertTagKeyValue(activity, TagName.Namespace);
        Assert.Equal(string.Join(",", serviceStartOptions.Namespace), namespaces);
    }

    [Fact]
    public void InitializedHandler_SetsCorrectInformationWhenNull()
    {
        // Arrange
        // Tool, Mode, and Namespace are null
        var serviceStartOptions = new ServiceStartOptions
        {
            Transport = TransportTypes.StdIo,
            Mode = null,
            ReadOnly = true,
            Debug = false,
            DangerouslyDisableElicitation = true,
            DangerouslyDisableHttpIncomingAuth = false,
        };
        var activity = new Activity("test-activity");
        var mockTelemetry = Substitute.For<ITelemetryService>();
        mockTelemetry.StartActivity(Arg.Any<string>()).Returns(activity);


        // Act
        ServiceStartCommand.LogStartTelemetry(mockTelemetry, serviceStartOptions);



        // Assert
        mockTelemetry.Received(1).StartActivity(ActivityName.ServerStarted);

        var dangerouslyDisableHttpIncomingAuth = GetAndAssertTagKeyValue(activity, TagName.DangerouslyDisableHttpIncomingAuth);
        Assert.Equal(serviceStartOptions.DangerouslyDisableHttpIncomingAuth, dangerouslyDisableHttpIncomingAuth);

        var dangerouslyDisableElicitation = GetAndAssertTagKeyValue(activity, TagName.DangerouslyDisableElicitation);
        Assert.Equal(serviceStartOptions.DangerouslyDisableElicitation, dangerouslyDisableElicitation);

        var transport = GetAndAssertTagKeyValue(activity, TagName.Transport);
        Assert.Equal(serviceStartOptions.Transport, transport);

        Assert.DoesNotContain(TagName.ServerMode, activity.TagObjects.Select(x => x.Key));

        Assert.DoesNotContain(TagName.Tool, activity.TagObjects.Select(x => x.Key));

        var readOnly = GetAndAssertTagKeyValue(activity, TagName.IsReadOnly);
        Assert.Equal(serviceStartOptions.ReadOnly, readOnly);

        var debug = GetAndAssertTagKeyValue(activity, TagName.IsDebug);
        Assert.Equal(serviceStartOptions.Debug, debug);

        Assert.DoesNotContain(TagName.Namespace, activity.TagObjects.Select(x => x.Key));
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

    private ParseResult CreateParseResultWithDangerouslyDisableElicitation(bool dangerouslyDisableElicitation)
    {
        var args = new List<string>
        {
            "--transport",
            "stdio"
        };

        if (dangerouslyDisableElicitation)
        {
            args.Add("--dangerously-disable-elicitation");
        }

        return _command.GetCommand().Parse([.. args]);
    }

    private ParseResult CreateParseResultWithTransport(string transport)
    {
        var args = new List<string>
        {
            "--transport",
            transport,
            "--mode",
            "all",
            "--read-only"
        };

        return _command.GetCommand().Parse([.. args]);
    }

    private ParseResult CreateParseResultWithoutTransport()
    {
        var args = new List<string>
        {
            "--mode",
            "all",
            "--read-only"
        };

        return _command.GetCommand().Parse([.. args]);
    }

    private ParseResult CreateParseResultWithMode(string? mode)
    {
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

        return _command.GetCommand().Parse([.. args]);
    }

    private ParseResult CreateParseResultWithAllOptions()
    {
        var args = new List<string>
        {
            "--transport", "stdio",
            "--namespace", "storage",
            "--namespace", "keyvault",
            "--mode", "all",
            "--read-only",
            "--debug",
            "--dangerously-disable-elicitation"
        };

        return _command.GetCommand().Parse([.. args]);
    }

    private ParseResult CreateParseResultWithTool(string[]? tools)
    {
        var args = new List<string>
        {
            "--transport", "stdio"
        };

        if (tools is not null)
        {
            foreach (var tool in tools)
            {
                args.Add("--tool");
                args.Add(tool);
            }
        }

        return _command.GetCommand().Parse([.. args]);
    }

    private ParseResult CreateParseResultWithMinimalOptions()
    {
        return _command.GetCommand().Parse([]);
    }

    private ParseResult CreateParseResultWithSupportLogging(string? folderPath)
    {
        var args = new List<string>
        {
            "--transport", "stdio"
        };

        if (folderPath is not null)
        {
            args.Add("--dangerously-write-support-logs-to-dir");
            args.Add(folderPath);
        }

        return _command.GetCommand().Parse([.. args]);
    }

    private ParseResult CreateParseResultWithToolsAndMode(string[] tools, string mode)
    {
        var args = new List<string>
        {
            "--transport", "stdio",
            "--mode", mode
        };

        foreach (var tool in tools)
        {
            args.Add("--tool");
            args.Add(tool);
        }

        return _command.GetCommand().Parse([.. args]);
    }

    private ParseResult CreateParseResultWithNamespaceAndTool()
    {
        var args = new List<string>
        {
            "--transport", "stdio",
            "--namespace", "storage",
            "--tool", "azmcp_storage_account_get"
        };

        return _command.GetCommand().Parse([.. args]);
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

    private static object GetAndAssertTagKeyValue(Activity activity, string tagName)
    {
        var matching = activity.TagObjects.SingleOrDefault(x => string.Equals(x.Key, tagName, StringComparison.OrdinalIgnoreCase));

        Assert.False(matching.Equals(default(KeyValuePair<string, object?>)), $"Tag '{tagName}' was not found in activity tags.");
        Assert.NotNull(matching.Value);

        return matching.Value;
    }

    #region CORS Policy Tests

    [Fact]
    public void ConfigureCors_DevelopmentWithAuthDisabled_RestrictsToLocalhost()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serverOptions = new ServiceStartOptions
        {
            DangerouslyDisableHttpIncomingAuth = true
        };

        // Set development environment
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        try
        {
            // Arrange environment
            var environment = Substitute.For<IWebHostEnvironment>();
            environment.EnvironmentName.Returns("Development");

            // Act
            var method = typeof(ServiceStartCommand).GetMethod("ConfigureCors",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method!.Invoke(null, [services, environment, serverOptions]);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var corsService = serviceProvider.GetService<Microsoft.AspNetCore.Cors.Infrastructure.ICorsService>();
            Assert.NotNull(corsService);

            // Verify policy was registered
            var corsOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>>();
            Assert.NotNull(corsOptions.Value);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    [Theory]
    [InlineData("http://localhost:3000", true)]
    [InlineData("http://localhost:5173", true)]
    [InlineData("http://127.0.0.1:8080", true)]
    [InlineData("http://[::1]:9000", true)]
    [InlineData("https://localhost:443", true)]
    [InlineData("http://example.com", false)]
    [InlineData("https://evil.com", false)]
    [InlineData("http://192.168.1.100", false)]
    public void ConfigureCors_DevelopmentWithAuthDisabled_ValidatesOrigins(string origin, bool shouldBeAllowed)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serverOptions = new ServiceStartOptions
        {
            DangerouslyDisableHttpIncomingAuth = true
        };

        // Set development environment
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        try
        {
            // Arrange environment
            var environment = Substitute.For<IWebHostEnvironment>();
            environment.EnvironmentName.Returns("Development");

            // Act
            var method = typeof(ServiceStartCommand).GetMethod("ConfigureCors",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method!.Invoke(null, [services, environment, serverOptions]);

            var serviceProvider = services.BuildServiceProvider();
            var corsOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>>();
            var policy = corsOptions.Value.GetPolicy("McpCorsPolicy");

            Assert.NotNull(policy);

            // Verify origin validation
            if (shouldBeAllowed)
            {
                Assert.True(policy.IsOriginAllowed(origin), $"Origin '{origin}' should be allowed in development mode with auth disabled");
                Assert.True(policy.SupportsCredentials, "AllowCredentials should be true in development mode");
            }
            else
            {
                Assert.False(policy.IsOriginAllowed(origin), $"Origin '{origin}' should NOT be allowed in development mode with auth disabled");
            }
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    [Fact]
    public void ConfigureCors_DevelopmentWithAuthEnabled_AllowsAllOrigins()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serverOptions = new ServiceStartOptions
        {
            DangerouslyDisableHttpIncomingAuth = false
        };

        // Set development environment
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        try
        {
            // Arrange environment
            var environment = Substitute.For<IWebHostEnvironment>();
            environment.EnvironmentName.Returns("Development");

            // Act
            var method = typeof(ServiceStartCommand).GetMethod("ConfigureCors",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method!.Invoke(null, [services, environment, serverOptions]);

            var serviceProvider = services.BuildServiceProvider();
            var corsOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>>();
            var policy = corsOptions.Value.GetPolicy("McpCorsPolicy");

            Assert.NotNull(policy);

            // Verify all origins are allowed
            Assert.True(policy.AllowAnyOrigin, "AllowAnyOrigin should be true when authentication is enabled");
            Assert.False(policy.SupportsCredentials, "SupportsCredentials should be false when AllowAnyOrigin is true");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    [Fact]
    public void ConfigureCors_ProductionWithAuthDisabled_AllowsAllOrigins()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serverOptions = new ServiceStartOptions
        {
            DangerouslyDisableHttpIncomingAuth = true
        };

        // Set production environment
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

        try
        {
            // Arrange environment
            var environment = Substitute.For<IWebHostEnvironment>();
            environment.EnvironmentName.Returns("Production");

            // Act
            var method = typeof(ServiceStartCommand).GetMethod("ConfigureCors",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method!.Invoke(null, [services, environment, serverOptions]);

            var serviceProvider = services.BuildServiceProvider();
            var corsOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>>();
            var policy = corsOptions.Value.GetPolicy("McpCorsPolicy");

            Assert.NotNull(policy);

            // Verify all origins are allowed
            Assert.True(policy.AllowAnyOrigin, "AllowAnyOrigin should be true in production");
            Assert.False(policy.SupportsCredentials, "SupportsCredentials should be false when AllowAnyOrigin is true");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    [Fact]
    public void ConfigureCors_ProductionWithAuthEnabled_AllowsAllOrigins()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serverOptions = new ServiceStartOptions
        {
            DangerouslyDisableHttpIncomingAuth = false
        };

        // Set production environment
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

        try
        {
            // Arrange environment
            var environment = Substitute.For<IWebHostEnvironment>();
            environment.EnvironmentName.Returns("Production");

            // Act
            var method = typeof(ServiceStartCommand).GetMethod("ConfigureCors",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method!.Invoke(null, [services, environment, serverOptions]);

            var serviceProvider = services.BuildServiceProvider();
            var corsOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>>();
            var policy = corsOptions.Value.GetPolicy("McpCorsPolicy");

            Assert.NotNull(policy);

            // Verify all origins are allowed
            Assert.True(policy.AllowAnyOrigin, "AllowAnyOrigin should be true in production with auth enabled");
            Assert.False(policy.SupportsCredentials, "SupportsCredentials should be false when AllowAnyOrigin is true");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    [Fact]
    public void ConfigureCors_NoEnvironmentSet_DefaultsToAllowAllOrigins()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serverOptions = new ServiceStartOptions
        {
            DangerouslyDisableHttpIncomingAuth = true
        };

        // Ensure environment variable is not set
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);

        try
        {
            // Arrange environment (not Development, simulating Staging or other non-dev environment)
            var environment = Substitute.For<IWebHostEnvironment>();
            environment.EnvironmentName.Returns("Staging");

            // Act
            var method = typeof(ServiceStartCommand).GetMethod("ConfigureCors",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method!.Invoke(null, [services, environment, serverOptions]);

            var serviceProvider = services.BuildServiceProvider();
            var corsOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>>();
            var policy = corsOptions.Value.GetPolicy("McpCorsPolicy");

            Assert.NotNull(policy);

            // Verify all origins are allowed when environment is not Development
            Assert.True(policy.AllowAnyOrigin, "AllowAnyOrigin should be true when ASPNETCORE_ENVIRONMENT is not set to Development");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    [Fact]
    public void ConfigureCors_DevelopmentWithAuthDisabled_AllowsAnyMethodAndHeader()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serverOptions = new ServiceStartOptions
        {
            DangerouslyDisableHttpIncomingAuth = true
        };

        // Set development environment
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        try
        {
            // Arrange environment
            var environment = Substitute.For<IWebHostEnvironment>();
            environment.EnvironmentName.Returns("Development");

            // Act
            var method = typeof(ServiceStartCommand).GetMethod("ConfigureCors",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method!.Invoke(null, [services, environment, serverOptions]);

            var serviceProvider = services.BuildServiceProvider();
            var corsOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>>();
            var policy = corsOptions.Value.GetPolicy("McpCorsPolicy");

            Assert.NotNull(policy);

            // Verify methods and headers
            Assert.True(policy.AllowAnyMethod, "AllowAnyMethod should be true");
            Assert.True(policy.AllowAnyHeader, "AllowAnyHeader should be true");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    #endregion
}
