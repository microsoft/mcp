// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Telemetry;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server;

public class ServiceStartCommandTests
{
    private readonly ServerStartCommand _command = new();
    private static readonly Lock s_currentDirectoryLock = new();

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
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
        var args = new List<string>() { "--transport", "stdio" };
        if (!string.IsNullOrEmpty(inputService))
        {
            args.Add("--namespace");
            args.Add(inputService);
        }

        // Act
        var options = BindOptions(args);

        // Assert
        Assert.Equal(expectedService, (options.Namespace != null && options.Namespace.Length > 0) ? options.Namespace[0] : "");
        Assert.Equal(expectedTransport, options.Transport);
    }

    [Theory]
    [MemberData(nameof(BoolOptionTestData))]
    public void BoolOption_ParsesCorrectly(string optionName, bool expectedValue, bool implicitBool)
    {
        // Arrange
        var args = new List<string>
        {
            "--transport",
            "stdio"
        };

        if (expectedValue)
        {
            args.Add(optionName);
            if (implicitBool)
            {
                args.Add("true");
            }
        }
        else if (implicitBool)
        {
            args.Add(optionName);
            args.Add("false");
        }

        // Act
        var parseResult = _command.GetCommand().Parse(args);
        var actualValue = parseResult.GetValue<bool>(optionName);

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    public static TheoryData<string, bool, bool> BoolOptionTestData()
    {
        var options = new[] {
            "--read-only",
            "--debug",
            "--dangerously-disable-http-incoming-auth",
            "--dangerously-disable-elicitation",
            "--dangerously-disable-retry-limits",
            "--disable-caching"
        };
        var theoryData = new TheoryData<string, bool, bool>();
        foreach (var option in options)
        {
            theoryData.Add(option, true, true); // explicitly set to true
            theoryData.Add(option, true, false); // implicitly set to true
            theoryData.Add(option, false, true); // explicitly set to false
            theoryData.Add(option, false, false); // implicitly set to false by omitting the option
        }
        return theoryData;
    }

    [Fact]
    public void AllOptionsRegistered_IncludesDangerouslyDisableElicitation()
    {
        // Arrange & Act
        var command = _command.GetCommand();

        // Assert
        var hasDangerouslyDisableElicitationOption = command.Options.Any(o => o.Name == "--dngerously-disable-elicitation");
        Assert.True(hasDangerouslyDisableElicitationOption, "DangerouslyDisableElicitation option should be registered");
    }

    [Fact]
    public void AllOptionsRegistered_IncludesTool()
    {
        // Arrange & Act
        var command = _command.GetCommand();

        // Assert
        var hasToolOption = command.Options.Any(o => o.Name == "--tool");
        Assert.True(hasToolOption, "Tool option should be registered");
    }

    [Theory]
    [InlineData("azmcp_storage_account_get")]
    [InlineData("azmcp_keyvault_secret_get")]
    [InlineData("azmcp_storage_account_get", "azmcp_keyvault_secret_get")]
    [InlineData(null)]
    public void ToolOption_ParsesCorrectly(params string[]? expectedTool)
    {
        // Arrange & Act
        var options = BindOptionsWithTool(expectedTool ?? null);

        // Assert
        if (expectedTool == null)
        {
            Assert.True(options.Tool == null || options.Tool.Length == 0);
        }
        else
        {
            Assert.NotNull(options.Tool);
            Assert.Equal(expectedTool.Length, options.Tool.Length);
            foreach (var tool in expectedTool)
            {
                Assert.Contains(tool, options.Tool);
            }
        }
    }

    [Theory]
    [InlineData("sse")]
    [InlineData("websocket")]
    [InlineData("invalid")]
    public async Task ExecuteAsync_InvalidTransport_ReturnsValidationError(string invalidTransport)
    {
        // Arrange & Act
        var response = await ExecuteAsync(BindOptionsWithTransport(invalidTransport));

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
        // Arrange & Act
        var response = await ExecuteAsync(BindOptionsWithMode(invalidMode));

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
        // Arrange & Act
        var response = await ExecuteAsync(BindOptionsWithMode(validMode));

        // Assert - Should not fail validation, though may fail later due to server startup
        if (response.Status == HttpStatusCode.BadRequest && response.Message?.Contains("Invalid mode") == true)
        {
            Assert.Fail($"Mode '{validMode}' should be valid but got validation error: {response.Message}");
        }
    }

    [Fact]
    public void BindOptions_WithAllOptions_ReturnsCorrectlyConfiguredOptions()
    {
        // Arrange & Act
        var options = BindOptions(
            "--transport", "stdio",
            "--namespace", "storage",
            "--namespace", "keyvault",
            "--mode", "all",
            "--read-only",
            "--debug",
            "--dangerously-disable-elicitation",
            "--disable-caching");

        // Assert
        Assert.Equal(TransportTypes.StdIo, options.Transport);
        Assert.Equal(new[] { "storage", "keyvault" }, options.Namespace);
        Assert.Equal("all", options.Mode);
        Assert.True(options.ReadOnly);
        Assert.True(options.Debug);
        Assert.False(options.DangerouslyDisableHttpIncomingAuth);
        Assert.True(options.DangerouslyDisableElicitation);
        Assert.True(options.DisableCaching);
    }

    [Fact]
    public void BindOptions_WithTool_ReturnsCorrectlyConfiguredOptions()
    {
        // Arrange & Act
        var expectedTool = "azmcp_group_list";
        var options = BindOptionsWithTool([expectedTool]);

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
        // Arrange & Act - Explicitly set mode to single but also provide multiple tools
        var tools = new[] { "azmcp_group_list", "azmcp_subscription_list" };
        var options = BindOptions("--transport", "stdio", "--mode", "single", "--tool", tools[0], "--tool", tools[1]);

        // Assert
        Assert.NotNull(options.Tool);
        Assert.Equal(2, options.Tool.Length);
        Assert.Equal(tools, options.Tool);
        Assert.Equal("all", options.Mode);
    }

    [Fact]
    public void BindOptions_WithDefaults_ReturnsDefaultValues()
    {
        // Arrange & Act
        var options = BindOptions();

        // Assert
        Assert.Equal(TransportTypes.StdIo, options.Transport); // Default transport
        Assert.Null(options.Namespace);
        Assert.Equal("namespace", options.Mode); // Default mode
        Assert.False(options.ReadOnly); // Default readonly
        Assert.False(options.Debug);
        Assert.False(options.DangerouslyDisableHttpIncomingAuth);
        Assert.False(options.DangerouslyDisableElicitation);
        Assert.Null(options.DangerouslyWriteSupportLogsToDir);
        Assert.False(options.DisableCaching);
    }

    [Theory]
    [InlineData("/tmp/logs")]
    [InlineData("C:\\logs")]
    [InlineData(null)]
    public void DangerouslyWriteSupportLogsToDirOption_ParsesCorrectly(string? expectedFolder)
    {
        // Arrange & Act
        var options = BindOptionsWithSupportLogging(expectedFolder);

        // Assert
        Assert.Equal(expectedFolder, options.DangerouslyWriteSupportLogsToDir);
    }

    [Fact]
    public void BindOptions_WithSupportLoggingFolder_ReturnsCorrectlyConfiguredOptions()
    {
        // Arrange & Act
        var logFolder = "/tmp/mcp-support-logs";
        var options = BindOptionsWithSupportLogging(logFolder);

        // Assert
        Assert.Equal(logFolder, options.DangerouslyWriteSupportLogsToDir);
    }

    [Fact]
    public void BindOptions_WithoutSupportLoggingFolder_ReturnsCorrectlyConfiguredOptions()
    {
        // Act
        var options = BindOptionsWithSupportLogging(null);

        // Assert
        Assert.Null(options.DangerouslyWriteSupportLogsToDir);
    }

    [Fact]
    public void AllOptionsRegistered_IncludesSupportLoggingToFolder()
    {
        // Arrange & Act
        var command = _command.GetCommand();

        // Assert
        var hasSupportLoggingFolderOption = command.Options.Any(o => o.Name == "--dangerously-write-support-logs-to-dir");
        Assert.True(hasSupportLoggingFolderOption, "DangerouslyWriteSupportLogsToDir option should be registered");
    }

    [Fact]
    public void Validate_WithValidOptions_ReturnsValidResult()
    {
        // Arrange & Act
        var result = ValidateOptions(BindOptionsWithTransport("stdio"));

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithInvalidTransport_ReturnsInvalidResult()
    {
        // Arrange & Act
        var result = ValidateOptions(BindOptionsWithTransport("invalid"));

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Invalid transport 'invalid'", string.Join('\n', result.Errors));
    }

    [Fact]
    public void Validate_WithInvalidMode_ReturnsInvalidResult()
    {
        // Arrange & Act
        var result = ValidateOptions(BindOptionsWithMode("invalid"));

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Invalid mode 'invalid'", string.Join('\n', result.Errors));
    }

    [Fact]
    public void Validate_WithNamespaceAndTool_ReturnsInvalidResult()
    {
        // Arrange & Act
        var result = ValidateOptions(BindOptionsWithNamespaceAndTool());

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("--namespace and --tool options cannot be used together", string.Join('\n', result.Errors));
    }

    [Fact]
    public void Validate_WithSupportLoggingFolderWhitespace_ReturnsInvalidResult()
    {
        // Arrange & Act
        var result = ValidateOptions(BindOptionsWithSupportLogging("   "));

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("The --dangerously-write-support-logs-to-dir option requires a valid folder path", string.Join('\n', result.Errors));
    }

    [Fact]
    public void Validate_WithValidSupportLoggingFolder_ReturnsValidResult()
    {
        // Arrange & Act
        var result = ValidateOptions(BindOptionsWithSupportLogging("/tmp/mcp-support-logs"));

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithoutSupportLoggingFolder_ReturnsValidResult()
    {
        // Arrange & Act
        var result = ValidateOptions(BindOptionsWithSupportLogging(null));

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_WithSupportLoggingFolderWhitespace_ReturnsValidationError()
    {
        // Arrange & Act
        var response = await ExecuteAsync(BindOptionsWithSupportLogging("   "));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("The --dangerously-write-support-logs-to-dir option requires a valid folder path", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithNamespaceAndTool_ReturnsValidationError()
    {
        // Arrange & Act
        var response = await ExecuteAsync(BindOptionsWithNamespaceAndTool());

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

    [Theory]
    [InlineData(typeof(ArgumentException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(InvalidOperationException), HttpStatusCode.UnprocessableEntity)]
    [InlineData(typeof(Exception), HttpStatusCode.InternalServerError)]
    public void GetStatusCode_ReturnsExpectedStatusCode(Type exceptionType, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test exception message")!;

        // Act
        var statusCode = GetStatusCode(exception);

        // Assert
        Assert.Equal(expectedStatusCode, statusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidTransport_DoesNotThrow()
    {
        // Arrange, Act, & Assert - Check that ArgumentException is not thrown for valid transport
        try
        {
            await ExecuteAsync(BindOptionsWithTransport("stdio"));
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
        // Arrange, Act, & Assert - Check that ArgumentException is not thrown when transport is omitted
        try
        {
            await ExecuteAsync(BindOptions("--mode", "all", "--read-only"));
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
        var serviceStartOptions = new ServerStartOptions
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
        ServerStartCommand.LogStartTelemetry(mockTelemetry, serviceStartOptions);

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
        var serviceStartOptions = new ServerStartOptions
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
        ServerStartCommand.LogStartTelemetry(mockTelemetry, serviceStartOptions);

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

    [Fact]
    public void CreateStdioHost_UsesApplicationBaseAsContentRoot()
    {
        // Arrange
        var options = new ServerStartOptions
        {
            Transport = TransportTypes.StdIo,
            Mode = "namespace"
        };
        var originalCurrentDirectory = Environment.CurrentDirectory;
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"mcp-content-root-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);

        lock (s_currentDirectoryLock)
        {
            try
            {
                Environment.CurrentDirectory = temporaryDirectory;

                // Act
                using var host = ServerStartCommand.CreateStdioHost(options);
                var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();

                // Assert
                Assert.Equal(Path.GetFullPath(AppContext.BaseDirectory), Path.GetFullPath(hostEnvironment.ContentRootPath));
            }
            finally
            {
                Environment.CurrentDirectory = originalCurrentDirectory;
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public void HttpContentRootOptions_UseApplicationBaseAsContentRoot()
    {
        // Act
        var options = Assert.IsType<WebApplicationOptions>(ServerStartCommand.s_httpWebApplicationOptions);

        // Assert
        Assert.Equal(Path.GetFullPath(AppContext.BaseDirectory), Path.GetFullPath(options.ContentRootPath!));
    }

    private ServerStartOptions BindOptionsWithTransport(string transport) =>
        BindOptions("--transport", transport, "--mode", "all", "--read-only");

    private ServerStartOptions BindOptionsWithMode(string? mode)
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

        return BindOptions(args);
    }

    private ServerStartOptions BindOptionsWithTool(string[]? tools)
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

        return BindOptions(args);
    }

    private ServerStartOptions BindOptionsWithSupportLogging(string? folderPath)
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

        return BindOptions(args);
    }

    private ServerStartOptions BindOptionsWithNamespaceAndTool() =>
        BindOptions("--transport", "stdio", "--namespace", "storage", "--tool", "azmcp_storage_account_get");

    private ServerStartOptions BindOptions(params string[] args) => _command.BindOptions(_command.GetCommand().Parse(args));

    private ServerStartOptions BindOptions(List<string> args) => _command.BindOptions(_command.GetCommand().Parse(args));

    private ValidationResult ValidateOptions(ServerStartOptions options)
    {
        var validationResult = new ValidationResult();
        _command.ValidateOptions(options, validationResult);
        return validationResult;
    }

    private async Task<CommandResponse> ExecuteAsync(ServerStartOptions options)
    {
        var context = new CommandContext(new ServiceCollection().BuildServiceProvider());
        return await _command.ExecuteAsync(context, options, TestContext.Current.CancellationToken);
    }

    private string GetErrorMessage(Exception exception)
    {
        // Use reflection to access the protected GetErrorMessage method
        var method = typeof(ServerStartCommand).GetMethod("GetErrorMessage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (string)method!.Invoke(_command, [exception])!;
    }

    private HttpStatusCode GetStatusCode(Exception exception)
    {
        // Use reflection to access the protected GetStatusCode method
        var method = typeof(ServerStartCommand).GetMethod("GetStatusCode",
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
        var serverOptions = new ServerStartOptions
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
            ServerStartCommand.ConfigureCors(services, environment, serverOptions);

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
        var serverOptions = new ServerStartOptions
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
            ServerStartCommand.ConfigureCors(services, environment, serverOptions);

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
        var serverOptions = new ServerStartOptions
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
            ServerStartCommand.ConfigureCors(services, environment, serverOptions);

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
        var serverOptions = new ServerStartOptions
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
            ServerStartCommand.ConfigureCors(services, environment, serverOptions);

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
        var serverOptions = new ServerStartOptions
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
            ServerStartCommand.ConfigureCors(services, environment, serverOptions);

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
        var serverOptions = new ServerStartOptions
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
            ServerStartCommand.ConfigureCors(services, environment, serverOptions);

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
        var serverOptions = new ServerStartOptions
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
            ServerStartCommand.ConfigureCors(services, environment, serverOptions);

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
